using System.Security.Cryptography;

namespace AQMS.Web.Middleware;

//Middleware klasse - prüft bei eingehenden paketen ob im header ein korrekter api key vorhanden ist
//ohne key => reject; (raspi kann nicht cookie technisch prüfen wie ein browser)
//Pakete die durch die prüfung kommen werden an controller übergeben;
//durch die middleware steuert man also selbst wie die pakete abgefertigt werden;

//keine static class, hält den zustand; 
//eine Instanz der klasse wird gehalten;
//keine internal class, public components sind auch so erkennbar
public class ApiKeyMiddleware
{
    //_ konvention für private felder / variablen; 
    //requestDelegate beschreibt eine funktion in einer variable die aufgerufen werden kann mit _next(params);
    //RequestDelegate nimmt einen HttpContext und gibt einen Task zurück; 
    //reicht das Paket an die nächste Station in der Pipeline weiter;
    private readonly RequestDelegate _next;

    //const - steht zu beginn fest und wird nicht mehr geändert;
    //wird gebraucht um beim request.header den key auszulesen;
    private const string ApiKeyHeader = "X-API-Key";

    // konstruktor wird immer ohne rückgabewert geschrieben;
    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    //pro request zusätzlich IConfiguration config als Parameter holen
    //dadurch geschieht die dependency injection des containers automatisch bei aufruf
    //nutzt automatisch den vom Framework pro Request geöffneten Scope
    //anders als beim Seeder, der manuell einen Scope erzeugen musste
    //so kann die klasse auf den DI-Container zugreifen bei methodenaufrufen;
    public async Task InvokeAsync(HttpContext context, IConfiguration config)
    {
        //prüfen ob der pfad mit api beginnt mit der StartsWithSegment Methode;
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            //wenn nicht mit api beginnend, weiter reichen in nächsten schritt durch aufruf von next
            await _next(context);
            //funktion beenden, da weitere prüfung nicht nötig ist nach weiterreichen an _next
            return;
        }

        //hier wird der gesendete api key in eine variable gespeichert
        //kann null sein, ein string oder mehrere strings mit komma seperiert wenn mehrere header dabei sind
        string? requestApiKey = context.Request.Headers[ApiKeyHeader];

        //kann null sein bei config fehler;
        //key auslesen aus config für vergleich; 
        string? expectedApiKey = config["ApiKey"];

        //überprüfung der variablen ob zugriff oder nicht;
        if(string.IsNullOrWhiteSpace(requestApiKey) || string.IsNullOrWhiteSpace(expectedApiKey))
        {
            //Unauthorized Meldung in response header für raspi worker und funktion beenden;
            context.Response.StatusCode = 401;
            //kein _next(context) aufrufen, in pipeline soll nicht weiter gereicht werden;
            return; 
        }

        //prüfung der gleichheit der variablen
        //bewusst nicht mit != operator; dieser misst bis zum ersten unterschiedlichen zeichen
        //so kann mit einer timing attack der key zeichen für zeichen rekostruiert werden
        //weil anfragen bei mehreren treffern länger dauern als bei sofortigem abbruch beim ersten zeichen
        //mit CryptographicOperations.FixedTimeEquals() kann im gleichen zeitraum geprüft werden, so gibts keien unterschiede in der response time die muster erkennen lässt
        var requestBytes = System.Text.Encoding.UTF8.GetBytes(requestApiKey);
        var expectedBytes = System.Text.Encoding.UTF8.GetBytes(expectedApiKey);

        if (!CryptographicOperations.FixedTimeEquals(requestBytes, expectedBytes)) 
        {
            //nicht gleich, reject mit unauthorized; funktion hier beenden;
            context.Response.StatusCode = 401;
            return;
        }

        //wenn bis hier der code durchläuft ist alles gut und _next kann weiter gehen;
        await _next(context);

    }
}
