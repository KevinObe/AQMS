namespace AQMS.Worker;

//Ds18b20 Temperatursensor wird in Linux als Datei ausgelesen
//Kernel legt die Daten als virtuelle Dateien ab (sys/bus/w1/devices/...)
//Diese Klasse liest diese Dateien aus und findet die Temperatur in jeder 
//2 Methoden, Temperatur auslesen, Text auseinandernehmen für die reinen Werte
//klasse public damit test sie sehen kann, liegt außerhalb des worker projekt folders
public class Ds18b20Reader(ILogger<Ds18b20Reader> logger)
{
    //Ordner am raspi wo die dateien liegen
    private const string BasePath = "/sys/bus/w1/devices";

    // Liest die aktuelle Temperatur in °C, oder null wenn kein Sensor oder fehler
    //auslesen vom DS18B20 über sysfs 1-Wire 
    //null bei fehler um monitoring daten nicht zu verschleiern
    //besser keine als falsche daten
    public async Task<double?> ReadTemperatureAsync(CancellationToken stoppingToken)
    {
        try
        {
            // 28-*-Ordner finden, sensor wird dynamisch ausgelesen - tausch möglich ohne codeänderung
            // erstes gefundenes element -> nur 1 sensor
            var directory = Directory.GetDirectories(BasePath, "28-*");
            if (directory.Length == 0)
            {
                logger.LogWarning("Kein DS18B20-Sensor (28-*) unter {pfad} gefunden.", BasePath);
                return null;
            }
            
            // w1_slave im gefundenen Ordner lesen.
            //path combine methode statt string verkettung, kümmert sich um / etc automatisch
            string filePath = Path.Combine(directory[0], "w1_slave");
            //asynchrones auslesen, blockieren des lese oder mess vorgangs vermeiden mit parallelen tasks
            string content = await File.ReadAllTextAsync(filePath, stoppingToken);

            //übergabe des gelesenenen content der datei and die parsetemperature methode zur verarbeitung
            double? temperature = ParseTemperature(content);
            //bei null wird geloggt bzw gewarnt - null zurückgegeben
            if (temperature is null)
                logger.LogWarning("DS18B20-Read ungültig (CRC NO oder Parse-Fehler): {pfad}", filePath);

            return temperature;
        }
        catch (Exception ex)
        {
            // Fehler loggen + return null, damit der Loop weiterläuft (kein Host-Stopp).
            logger.LogError(ex, "Fehler beim Lesen des DS18B20.");
            return null;
        }
    }

    // parsetemperature -> verarbeitet ausgelesene info
    // rohe daten am pi nach auslesen:
    // 8f 01 55 05 7f a5 a5 66 1a : crc=1a YES
    //8f 01 55 05 7f a5 a5 66 1a t = 24937
    // CRC -> Cyclic Redundancy Check = (zyklische Redundanzprüfung). Eine Prüfsumme, mit der man erkennt, ob Daten bei der Übertragung fehlerfrei angekommen sind.
    public static double? ParseTemperature(string w1Content)
    {
        // In einen array aus zeilen splitten, leere skippen mit removeEmptyEntries und whitespaces raus mit trimEntries;
        // fehlerfall: zu kurze daten -> fehlerhaft, werden auf null gesetzt
        var stringParts = w1Content.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (stringParts.Length < 2)
            return null;

        // Hier muss die Zeile mit yes enden, wenn nicht ungültiges auslesen -> null return statt fehlerhaftem wert;
        if (!stringParts[0].EndsWith("YES"))
            return null;

        // In Zeile 2 nach "t=" suchen.
        //dieser wert ist die aktuelle temperatur die ausgelesen wird;
        int index = stringParts[1].IndexOf("t=");
        if (index == -1)
            return null;

        // Wert hinter "t=" parsen (Milligrad Celsius als int).
        //MG darstellung für normal lesbaren temperatur wert;
        string rawValue = stringParts[1].Substring(index + 2);
        if (!int.TryParse(rawValue, out int milliGrad))
            return null;

        // /1000 -> Celsius als double zurückgeben aus der methode.
        return milliGrad / 1000.0;
    }
}
