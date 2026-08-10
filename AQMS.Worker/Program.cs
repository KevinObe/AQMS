using AQMS.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

//vorkonfigurierten client registrieren, der im betrieb immer die selbe basis URL und die gleichen header mitsendet;
builder.Services.AddHttpClient("aqms-api", client =>
{
    var apiKey = builder.Configuration["ApiKey"];

    client.BaseAddress = new Uri(builder.Configuration["AqmsApi:BaseUrl"]!);
    client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
});

//Addsingleton = eine Instanz, die die ganze Laufzeit über bestehen bleibt -> d.h. bei jeder DI wird die selbe Instanz verwendet / injiziert;
//reader ist zustandslos, hält keine daten zwischen aufrufen zur verarbeitung; 
//worker selbst ist eine art singleton -> builder.Services.AddHostedService<Worker>(); = wird einmal aufgerufen beim start und bekommt einmal DIs;
//addscoped = Instanz pro scope; 
//addtransient = bei jedem zugriff eine neue instanz
builder.Services.AddSingleton<Ds18b20Reader>();

var host = builder.Build();
host.Run();
