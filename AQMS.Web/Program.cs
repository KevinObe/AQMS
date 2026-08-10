using AQMS.Web.Data;
using AQMS.Web.Middleware;
using AQMS.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<AqmsDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AqmsDbContext>();
builder.Services.AddControllersWithViews();

//einfügen des CommandService mittels scoped-lebensdauer
//scoped bedeutet, dass pro http request ein scope erstellt wird und für die request dauer am leben gehalten wird.
builder.Services.AddScoped<CommandService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

//einbinden der middleware klasse in die app
//da es sich um einen sicherheits-check handelt früher einbinden als routing oder authorization
app.UseMiddleware<ApiKeyMiddleware>();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();

//Default route ändern, damit nicht eigeloggte user auf / landen und zum login geleitet werden mittels [Authorize]
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

// Öffentliche Registrierung deaktiviert: Aufrufe der Register-Seite werden
// auf die Login-Seite umgeleitet. Neue Benutzer werden ausschließlich
// administrativ angelegt (vorerst direkt in der DB, später per Identity-Seeder).
app.MapGet("/Identity/Account/Register", () => Results.Redirect("/Identity/Account/Login"));
app.MapPost("/Identity/Account/Register", () => Results.Redirect("/Identity/Account/Login"));

//test der api routen middleware - temporär
//muss header ohne key und falschem key auf 401 leiten und bei echtem key aus user secrets 200 pong antworten;
//powershell: curl.exe -i -H "X-API-Key: UserSecretsApiKey" https://localhost:PORT/api/ping
//app.MapGet("/api/ping", () => "pong");

//bei anwendungsstart werden die rollen und userdaten angelegt und der admin zugang sichergestellt
//bevor die app vollständig gestartet wurde und läuft
//läuft jedes mal beim start um den admin user sicherzustellen
//scope ist nötig weil RoleManager und UserManager in IdentitySeeder.cs scoped Services sind, die man nicht direkt vom App Root holen kann; 
//bei einem http request würde bereits ein scope bestehen, ist beim seeder start nicht der fall
using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();
