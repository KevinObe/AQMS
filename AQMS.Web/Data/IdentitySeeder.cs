using Microsoft.AspNetCore.Identity;

namespace AQMS.Web.Data;

public static class IdentitySeeder
{
    public const string AdminRole = "Admin";

    public const string UserRole = "User";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleHandler = services.GetRequiredService<RoleManager<IdentityRole>>();

        var userHandler = services.GetRequiredService<UserManager<IdentityUser>>();

        var config = services.GetRequiredService<IConfiguration>();

        //array erzeugen aus den vorhandenen rollen
        foreach (var role in new[] { AdminRole, UserRole })
        {
            //wenn die rolle noch nicht existiert, anlegen
            if (!await roleHandler.RoleExistsAsync(role))
            {
                await roleHandler.CreateAsync(new IdentityRole(role));
            }
        }

        //Abfragen der Zugangsdaten für den Admin aus der config 
        //keine secrets im code
        //der admin ist pflicht; ohne kann sich niemand anmelden, weil es keine registrierung gibt
        //prüfen ob admin daten in config vorhanden sind, sonst app start beenden;
        var adminEmail = config["AdminBenutzer:Email"];
        var adminPwd = config["AdminBenutzer:Passwort"];

        if (String.IsNullOrWhiteSpace(adminEmail) || String.IsNullOrWhiteSpace(adminPwd))
        {
            throw new InvalidOperationException(
                "Konfiguration 'AdminBenutzer:Email' oder 'AdminBenutzer:Passwort' fehlt.");
        }

        if (!await EnsureUserAsync(userHandler, adminEmail, adminPwd, AdminRole))
        {
            throw new InvalidOperationException("Admin Konto konnte nicht angelegt werden.");
        }

        //der standardbenutzer ist optional
        //im moment nur für rollen und rechte tests wichtig
        await EnsureUserAsync(
            userHandler,
            config["StandardBenutzer:Email"],
            config["StandardBenutzer:Passwort"],
            UserRole);
    }


    //neue konten anlegen, falls fehlend
    //läuft bei jedem app-start -> idempotent;
    private static async Task<bool> EnsureUserAsync(UserManager<IdentityUser> userHandler,string? email,string? password,string role)
    {

        //falls eine der beiden variablen leer oder null ist, abfangen; 
        //config[string] kann nullable oder beliebig sein, würde kompilieren aber fehlerhaft
        if (String.IsNullOrWhiteSpace(email) || String.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        //user suchen und bei bedarf anlegen;
        var user = await userHandler.FindByEmailAsync(email);

        if (user is null)
        {
            user = new IdentityUser
            {
                UserName = email,
                Email = email,
                //account aktivieren für login
                EmailConfirmed = true
            };

            //user noch nicht in db, wird hier neu eingetragen
            var result = await userHandler.CreateAsync(user, password);

            //ohne diese prüfung würde bei einem von der passwortrichtlinie abgelehnten passwort gleich darauf eine rolle an ein konto vergeben, das gar nicht in der db steht
            if (!result.Succeeded)
            {
                return false;
            }
        }

        //prüfen ob die rolle korrekt gesetzt ist und ggfs zuweisen;
        if (!await userHandler.IsInRoleAsync(user, role))
        {
            await userHandler.AddToRoleAsync(user, role);
        }

        return true;

    }

}
