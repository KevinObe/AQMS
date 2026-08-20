using AQMS.Worker.Dtos;
using System.Net;                // HttpStatusCode - für den 409-Vergleich beim Melden
using System.Net.Http.Json;      // ReadFromJsonAsync + PostAsJsonAsync (im Shared Framework, kein Extra-Paket)

namespace AQMS.Worker
{
    // BackgroundService = langlebiger Hintergrunddienst, der mit dem Host startet/stoppt.
    // Primary Constructor: DI injiziert Logger, HttpClient-Factory und Konfiguration direkt
    // als Konstruktor-Parameter (entspricht "this.logger = logger" ohne Boilerplate).
    // AddSingleton Ds18b20Reader sensorReader -> einmaliges Injizieren der Reader Klasse;
    public class Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration, Ds18b20Reader sensorReader) : BackgroundService
    {
        // ExecuteAsync läuft einmal beim Start; die while-Schleife hält den Dienst am Leben.
        // stoppingToken wird beim Herunterfahren signalisiert -> sauberer Ausstieg aus dem Loop.
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // DeviceIdentifier wird beim Messen gebraucht, fürs Polling nicht;
            var deviceIdentifier = configuration["Worker:DeviceIdentifier"];

            // Poll-Intervall aus der Konfig. Einmal vor der Schleife gelesen (ändert sich nicht zur Laufzeit).
            var intervalSeconds = configuration.GetValue<int>("Worker:PollIntervalSeconds");

            //Mess-Intervall, damit die Temperatur nicht alle 10 sekunden ausgelesen wird;
            //Default zum entwickeln bzw zum testen 60 sekunden; 
            var measurementIntervalSeconds = configuration.GetValue<int>("Worker:MeasurementIntervalSeconds");

            //bei null wieder auf 60 zurück setzen;
            if(measurementIntervalSeconds <= 0)
            {
                measurementIntervalSeconds = 60;
            }

            //Zeitpunkt der letzten Messung festhalten mit DateTime.MinValue - entspricht noch nie, abstand zu groß deshalb sofortiges auslösen;
            DateTime lastMeasurement = DateTime.MinValue;

            // Interval-Guard: GetValue<int> liefert bei fehlendem/vertipptem Key still 0.
            // Task.Delay(0) = keine Pause -> Tight-Loop, der die API flutet. Darum Default + Warnung.
            if (intervalSeconds <= 0)
            {
                const int standardIntervall = 5;
                logger.LogWarning(
                    "Worker:PollIntervalSeconds ist {konfiguriert} (ungültig), nutze Standard {standard} Sekunden.",
                    intervalSeconds, standardIntervall);
                intervalSeconds = standardIntervall;
            }

            // Schwelle für aufeinanderfolgende Sensor-Fehler (Default 10).
            // Verhindert dass aufeinanderfolgende Fehler nicht unerkannt bleiben;
            // fehlerhafte messungen werden übersprungen, bei ständigen messfehlern würde es einfach mit null durchlaufen ohne aufzufallen;
            var maxSensorErrors = configuration.GetValue<int>("Worker:MaxContinuousSensorErrors");
            if (maxSensorErrors <= 0)
                maxSensorErrors = 10;

            // Zähler außerhalb der Schleife, sonst würde er bei jedem Durchlauf zurückgesetzt.
            int continuousSensorErrors = 0;

            // Haupt-Poll-Schleife: läuft bis zum Shutdown-Signal.
            while (!stoppingToken.IsCancellationRequested)
            {
                // Named Client "aqms-api": in Program.cs vorkonfiguriert mit BaseAddress = AQMS-API
                // und X-API-Key-Header. nur für die API gedacht - nicht für Shellys (s. unten).
                var client = httpClientFactory.CreateClient("aqms-api");

                // Timeout von 100 s (Default) auf 10 s: sonst blockiert ein hängender VPS den Poll
                // bis zu 100 s und wirft dann eine TaskCanceledException (unten abgefangen).
                client.Timeout = TimeSpan.FromSeconds(10);

                // Relative URL -> wird gegen die BaseAddress des Clients aufgelöst.
                string url = $"api/commands/pending";

                // äußerer Schutz: umschließt Poll und Parse und Dispatch.
                // Hält die Schleife bei jedem unerwarteten Fehler am Leben (s. catch unten).
                try
                {
                    // GET an die API (konzeptuell wie fetch() in JS).
                    var response = await client.GetAsync(url, stoppingToken);

                    // Strukturiertes Logging: {status} wird durch response.StatusCode ersetzt.
                    logger.LogInformation("Status: {status}", response.StatusCode);

                    // Nur bei 2xx parsen. 400/401 bleiben als Status-Log sichtbar (Debug-Signal).
                    if (response.IsSuccessStatusCode)
                    {
                        // Liest den Response-Stream direkt in die DTO-Liste (Pendant zu response.json()).
                        // Property-Namen der DTO matchen das JSON -> case-insensitive Bindung.
                        var commands = await response.Content.ReadFromJsonAsync<List<PendingCommandDto>>(stoppingToken);

                        // Defensiver Guard: ReadFromJsonAsync kann null liefern (leerer/null Body).
                        // && wertet links zuerst -> .Count wird nie auf null aufgerufen.
                        // Leere Liste ([], Normalfall ohne offene Befehle) -> nichts zu tun.
                        if (commands is not null && commands.Count > 0)
                        {
                            // Shelly-Client einmal pro Batch: separater Default-Client, bewusst nicht "aqms-api"
                            // (dessen BaseAddress + X-API-Key gehören zur API; gegen einen Shelly würde die
                            // absolute URL kollidieren und der API-Key leaken).
                            // Eigener kurzer Timeout: der Default (100 s) würde bei einem hängenden Shelly
                            // den Loop ewig blockieren. 3 s begrenzen den Worst Case pro Befehl.
                            var shellyClient = httpClientFactory.CreateClient();
                            shellyClient.Timeout = TimeSpan.FromSeconds(3);

                            // Pro empfangenem Befehl: ausführen (Shelly schalten) + Ergebnis melden.
                            foreach (var command in commands)
                            {
                                logger.LogInformation(
                                    "Befehl empfangen: {commandId} {action} (erstellt {createdAt})",
                                    command.CommandId, command.Action, command.CreatedAt);

                                // IP-Guard: IPAddress ist string? (die Domäne erlaubt Geräte ohne IP).
                                // Der Service filtert null zwar raus, aber die DTO garantiert es nicht.
                                // Als Failed melden (verlässt Pending) und überspringen, statt eine kaputte URL zu bauen.
                                if (string.IsNullOrWhiteSpace(command.IPAddress))
                                {
                                    logger.LogWarning("Befehl {commandId}: Keine IP Adresse, wird übersprungen.", command.CommandId);
                                    await ReportResultAsync(command.CommandId, false, "Keine IP Adresse gefunden");
                                    continue;
                                }

                                // Action ("On"/"Off", aus dem DeviceState-Enum) -> Shelly-Verb mappen.
                                // Ergebnis in eine neue Variable - command.Action nicht überschreiben.
                                string shellyTurn;
                                switch (command.Action)
                                {
                                    case "On":
                                        shellyTurn = "on";
                                        break;   // break verlässt den switch -> weiter unter dem switch
                                    case "Off":
                                        shellyTurn = "off";
                                        break;
                                    default:
                                        // Unbekannte Action nicht blind senden -> als Failed melden + überspringen.
                                        logger.LogWarning("Befehl {commandId} mit unbekannter Action {action} nicht ausgeführt.",
                                            command.CommandId, command.Action);
                                        await ReportResultAsync(command.CommandId, false, $"Unbekannte Action: {command.Action}");
                                        continue;   // continue überspringt den Rest dieses Schleifendurchlaufs
                                }

                                // Klassische Shelly-HTTP-API (§7.4) - kompatibel über alle 5 Geräte.
                                string shellyUrl = $"http://{command.IPAddress}/relay/0?turn={shellyTurn}";

                                // Ergebnis-Variablen vor dem try deklarieren: sie werden in jedem Zweig
                                // (Erfolg / nicht erreichbar / Timeout) gesetzt und danach gemeldet.
                                bool success;
                                string? report;

                                // innerer Schutz pro Befehl: ein einzelner Shelly-Fehler darf die
                                // übrigen Befehle dieser Batch nicht ausfallen lassen -> fangen + weiter.
                                try
                                {
                                    var shellyResponse = await shellyClient.GetAsync(shellyUrl, stoppingToken);
                                    // 2xx = geschaltet. Bei Nicht-2xx die Meldung mit dem Statuscode füllen.
                                    success = shellyResponse.IsSuccessStatusCode;
                                    report = success ? null : $"Shelly-Status {(int)shellyResponse.StatusCode}";

                                    if (success)
                                        logger.LogInformation("Befehl {commandId} ausgeführt: {device} -> {turn}",
                                            command.CommandId, command.DeviceIdentifier, shellyTurn);
                                    else
                                        // Shelly hat geantwortet, aber nicht mit 2xx (z. B. falscher Relay-Index).
                                        logger.LogWarning("Befehl {commandId}: Shelly antwortete mit {status}",
                                            command.CommandId, shellyResponse.StatusCode);
                                }
                                catch (HttpRequestException ex)
                                {
                                    // Shelly gar nicht erreichbar (aus, falsche IP, kein LAN). Loggen, kein throw.
                                    success = false;
                                    report = "Shelly nicht erreichbar";
                                    logger.LogError(ex, "Shelly {device} nicht erreichbar - Befehl {commandId}",
                                        command.DeviceIdentifier, command.CommandId);
                                }
                                catch (TaskCanceledException) when (!stoppingToken.IsCancellationRequested)
                                {
                                    // Cancellation, die nicht vom Shutdown-Token kommt -> es war der 3-s-Timeout
                                    // des Shelly-Clients (Gerät erreichbar, aber hängt). Kommt sie vom Shutdown,
                                    // ist der Filter false -> Exception läuft durch -> Host stoppt sauber.
                                    success = false;
                                    report = "Shelly Timeout 3s";
                                    logger.LogWarning("Befehl {commandId}: Shelly {device} Timeout (3 s)",
                                        command.CommandId, command.DeviceIdentifier);
                                }

                                // Ergebnis in jedem Ausgang an die API melden (Erfolg und Fehler):
                                // setzt den Befehl im Backend auf Executed/Failed -> raus aus Pending, feuert nicht mehr.
                                await ReportResultAsync(command.CommandId, success, report);
                            }
                        }
                    }
                }
                // spezifischer Catch zuerst: Poll-Timeout (VPS antwortet >10 s nicht) kommt als
                // TaskCanceledException. Der when-Filter grenzt den echten Shutdown aus (dann läuft
                // die Exception durch -> sauberer Host-Stopp).
                catch (TaskCanceledException) when (!stoppingToken.IsCancellationRequested)
                {
                    logger.LogWarning("Poll-Timeout gegen die API - nächster Poll folgt.");
                }
                // breiter Catch danach: Poll-/Parse-/unerwartete Fehler -> Loop überlebt jeden Durchlauf.
                // Wichtig: TaskCanceledException erbt von OperationCanceledException, wird hier also
                // vom Filter (is not OperationCanceledException) nicht gefangen - deshalb der spezifische oben.
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Fehler im Poll-Durchlauf ({url}) - nächster Poll folgt.", url);
                }

                // Delay nicht im try, sonst zu enger dauerloop bei fehlern
                await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);

                // Mess-Kadenz: nur posten, wenn seit der letzten Messung genug Zeit vergangen ist.
                // bewusst zeitvergleich
                // bei 2. interval mit while schleife wäre die fehlerquelle wesentlich höher als bei simplen zeitvergleich
                // simpel -> ist seit lezter messung genug zeit vergangen??
                if (DateTime.UtcNow - lastMeasurement >= TimeSpan.FromSeconds(measurementIntervalSeconds))
                {
                    await ReportMeasurementAsync();

                    //updaten von lastmeasurement variable damit die kadenz an den letzten messversuch gekoppelt ist
                    //nur aktualisieren bei tatsächlichem aufruf, messfehler sind hier egal, kadenz bleibt zeitabhängig;
                    lastMeasurement = DateTime.UtcNow;
                }
            }

            // Lokale Funktion (in C# "gehoisted", wie eine function-Deklaration in JS: am Ende definiert,
            // aber oben in der Schleife schon aufrufbar).
            // Meldet ein Befehlsergebnis an POST /api/commands/result.
            // Eigener Client + Timeout + Timeout-Catch, damit ein hängender VPS beim Melden den Host nicht stoppt.
            async Task ReportResultAsync(long commandId, bool success, string? report)
            {
                // Request-Body: matcht die Web-DTO (CommandId/Success/ResultMessage), sonst bindet das JSON nicht.
                var resultDto = new CommandResultDto
                {
                    CommandId = commandId,
                    Success = success,
                    ResultMessage = report,
                };

                // Über den aqms-api-Client (BaseAddress API + X-API-Key), nicht den Shelly-Client.
                // Eigener 10-s-Timeout - selbe Absicherung wie beim Poll-Client.
                var reportClient = httpClientFactory.CreateClient("aqms-api");
                reportClient.Timeout = TimeSpan.FromSeconds(10);

                try
                {
                    // PostAsJsonAsync = Gegenstück zu ReadFromJsonAsync: serialisiert die DTO in den JSON-Body.
                    var resultResponse = await reportClient.PostAsJsonAsync("api/commands/result", resultDto, stoppingToken);

                    if (resultResponse.IsSuccessStatusCode)
                        // 200: Backend hat den Befehl auf Executed/Failed gesetzt.
                        logger.LogInformation("Befehl {commandId} gemeldet (Success={success})", commandId, success);
                    else if (resultResponse.StatusCode == HttpStatusCode.Conflict)
                        // 409: war schon verarbeitet - harmlos (Beweis, dass der Befehl sauber durch ist).
                        logger.LogInformation("Befehl {commandId} war bereits verarbeitet (409).", commandId);
                    else
                        // z. B. 404 (Befehl verschwunden) - unerwartet, aber kein Grund zum Absturz.
                        logger.LogWarning("Befehl {commandId}: Result-Meldung abgelehnt ({status}).",
                            commandId, resultResponse.StatusCode);
                }
                catch (HttpRequestException ex)
                {
                    // VPS weg -> Meldung verloren. kein throw: Befehl bleibt Pending, wird beim
                    // nächsten Poll erneut ausgeführt+gemeldet (at-least-once).
                    logger.LogError(ex, "Befehl {commandId}: Result-Meldung fehlgeschlagen - Retry beim nächsten Poll.", commandId);
                }
                catch (TaskCanceledException) when (!stoppingToken.IsCancellationRequested)
                {
                    // VPS hängt bis zum 10-s-Timeout (nicht der Shutdown). Auch hier kein throw -> Retry nächster Poll.
                    logger.LogWarning("Befehl {commandId}: Result-Meldung Timeout - Retry beim nächsten Poll.", commandId);
                }
            }

            // Liest die Temperatur und meldet sie an POST /api/measurements.
            // Anders als ReportResultAsync: bei Fehler kein Retry - ein verlorener Messwert ist bewusst als ok hingenommen
            // die nächste Kadenz liest einen frischen. Ein alter Wert nachgeliefert wäre schädlich (veraltet).
            async Task ReportMeasurementAsync()
            {
                // Temperatur lesen. null = kein Sensor / CRC NO / Parse-Fehler -> nichts posten.
                double? temperature = await sensorReader.ReadTemperatureAsync(stoppingToken);
                if (temperature is null)
                {
                    // Fehl-Read (CRC NO / kein Sensor). Der einzelne Read wurde in ReadTemperaturAsync
                    // schon als Warning geloggt. Hier zählen wir die Serie und eskalieren auf Error,
                    // damit ein dauerausfall auffällt, statt im Grundrauschen der Warnings unterzugehen.
                    continuousSensorErrors++;
                    if (continuousSensorErrors >= maxSensorErrors)
                    {
                        //Folgefehler bei Messungen loggen
                        //spätere Meldelogik um Nutzer auf Sensorfehler hinzuweisen; 
                        logger.LogError("Ds18B20 seit {count} Messungen in Folge ausgefallen.", continuousSensorErrors);
                    }
                    return;
                }

                // Erfolgreiches Messen der Temp -> Folgefehlercounter zurücksetzen; 
                continuousSensorErrors = 0;

                // DTO bauen. MeasurementTypeName muss genau "Temperature" (Seed-Name) sein, sonst 400.
                // deviceIdentifier = Config-Wert "raspberry-pi" (oben in ExecuteAsync gelesen).
                var dto = new CreateMeasurementDto
                {
                    DeviceIdentifier = deviceIdentifier!,
                    MeasurementTypeName = "Temperature",
                    Value = temperature.Value,
                    Timestamp = DateTime.UtcNow,
                };

                // POST über den aqms-api-Client (eigene Instanz + Timeout, wie bei Result-Reporting).
                var client = httpClientFactory.CreateClient("aqms-api");
                client.Timeout = TimeSpan.FromSeconds(10);

                try
                {
                    var response = await client.PostAsJsonAsync("api/measurements", dto, stoppingToken);

                    if (response.IsSuccessStatusCode)
                        logger.LogInformation("Messung gesendet: {temp} °C", temperature.Value);
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                        // 400 = unbekanntes Gerät/Typ -> Config-Fehler (falscher Identifier/Typname).
                        logger.LogWarning("Messung abgelehnt (400) - Gerät oder Typ unbekannt?");
                    else
                        logger.LogWarning("Messung abgelehnt ({status}).", response.StatusCode);
                }
                catch (HttpRequestException ex)
                {
                    // VPS weg -> Messwert verloren. kein throw, kein Retry
                    logger.LogError(ex, "Messung fehlgeschlagen - übersprungen.");
                }
                catch (TaskCanceledException) when (!stoppingToken.IsCancellationRequested)
                {
                    // Timeout (VPS hängt), nicht der Shutdown. Auch hier: übersprungen, kein Retry.
                    logger.LogWarning("Messung-Timeout - übersprungen.");
                }
            }
        }
    }
}