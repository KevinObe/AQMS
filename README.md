# AQMS-2026 — Aquarium Monitoring & Control System

Diplomarbeit · Kevin Oberluggauer · FAAI WIFI Tirol · 2025/2026

AQMS ist ein verteiltes System zur Überwachung und Steuerung eines Aquariums.
Ein Raspberry Pi misst die Wassertemperatur über einen DS18B20-Sensor, meldet
die Messwerte an eine ASP.NET-Core-Webanwendung und schaltet auf deren Anweisung
Shelly-Smart-Plugs (Heizung, Beleuchtung, Filter).

---

## Architektur

```
┌──────────────────┐   HTTPS + X-API-Key   ┌────────────────────┐
│  AQMS.Worker     │ ────────────────────► │  AQMS.Web          │
│  (Raspberry Pi)  │                       │  (VPS, Debian)     │
│                  │ ◄──────────────────── │                    │
│  · DS18B20 lesen │   Pending Commands    │  · REST-API        │
│  · Shelly HTTP   │                       │  · Dashboard (MVC) │
└──────────────────┘                       │  · Identity/Auth   │
         │                                 └─────────┬──────────┘
         │ HTTP (LAN)                                │ EF Core
         ▼                                           ▼
   ┌───────────┐                              ┌─────────────┐
   │  Shellys  │                              │ SQL Server  │
   └───────────┘                              └─────────────┘
```

Der Worker ist **Pull-basiert**: Er pollt die API nach offenen Kommandos, statt
eingehende Verbindungen anzunehmen. Dadurch braucht der Pi im Heimnetz keinen
offenen Port und keine Portweiterleitung.

Kommandos werden **at-least-once** gemeldet — die Ergebnismeldung ist
serverseitig idempotent, doppelte Meldungen erzeugen keinen zweiten
`StateChange`.

## Projekte

| Projekt | Zweck |
|---|---|
| `AQMS.Web` | ASP.NET Core MVC — REST-API, Dashboard, Identity, EF-Core-Persistenz |
| `AQMS.Worker` | .NET Worker Service auf dem Raspberry Pi — Sensorik und Shelly-Steuerung |
| `AQMS.Tests` | xUnit-Tests für Command-Idempotenz und Sensor-Parsing |

## Technologie-Stack

- .NET 10 (`net10.0`)
- ASP.NET Core MVC + ASP.NET Core Identity
- Entity Framework Core 10 mit SQL Server
- xUnit
- Bootstrap 5, jQuery Validation
- Deployment: Debian VPS (systemd) + Raspberry Pi OS

## Konfiguration

Es sind **keine Zugangsdaten im Repository** — alle Secrets werden über
User Secrets (Entwicklung) bzw. Umgebungsvariablen (Produktion) gesetzt.

**AQMS.Web:**

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Database=aqms;..."
dotnet user-secrets set "ApiKey"                    "<frei gewählter Schlüssel>"
dotnet user-secrets set "AdminBenutzer:Email"       "admin@example.com"
dotnet user-secrets set "AdminBenutzer:Passwort"    "<passwort>"
dotnet user-secrets set "StandardBenutzer:Email"    "user@example.com"
dotnet user-secrets set "StandardBenutzer:Passwort" "<passwort>"
```

`AdminBenutzer` ist verpflichtend — fehlt der Eintrag, bricht der
`IdentitySeeder` beim Start mit einer Exception ab. `StandardBenutzer` ist
optional.

**AQMS.Worker:**

```bash
dotnet user-secrets set "ApiKey" "<derselbe Schlüssel wie in AQMS.Web>"
```

`AqmsApi:BaseUrl` steht in [`AQMS.Worker/appsettings.json`](AQMS.Worker/appsettings.json)
und zeigt per Default auf `http://localhost:5258`.

## Starten

```bash
dotnet restore
dotnet ef database update --project AQMS.Web    # Schema anlegen
dotnet run --project AQMS.Web
dotnet run --project AQMS.Worker                # separates Terminal
dotnet test
```

Der Worker liest den DS18B20 über das 1-Wire-Sysfs-Interface
(`/sys/bus/w1/devices/`) und läuft in dieser Form nur auf dem Raspberry Pi.
Die Parsing-Logik ist bewusst von der Dateisystem-I/O getrennt und daher ohne
Hardware testbar.

## Dokumentation

[`docs/AQMS_Masterdoku.md`](docs/AQMS_Masterdoku.md) ist das chronologische
Projekt-Tagebuch: Planung, Architektur- und Designentscheidungen inklusive
verworfener Alternativen, Hardware-Aufbau, Netzwerkkonfiguration, Persistenz-
schicht und VPS-Deployment — jeweils mit Kontext, Problem und Lösung.

## Hinweise zu dieser Fassung

Dies ist ein **bereinigter Veröffentlichungs-Snapshot** für die Abgabe. Gegenüber
dem privaten Arbeitsrepository gilt:

- Server-IPs, Domain, SSH-Benutzername und E-Mail-Adressen in der Dokumentation
  sind durch reservierte Doku-Werte ersetzt (`203.0.113.x` nach RFC 5737,
  `aqms.example.com` nach RFC 2606). Sie sind absichtlich nicht funktionsfähig.
- Die IP-Adressen `10.0.0.222`–`10.0.0.227` sind private LAN-Adressen (RFC 1918)
  und von außen nicht erreichbar. Sie stehen als Seed-Daten in den
  EF-Core-Migrationen und wurden deshalb unverändert gelassen.
- Zwischenstände, Duplikate, Projektmanagement-Binärdateien sowie fremde
  Muster- und Herstellerdokumente sind nicht enthalten.
- Die Diplomarbeit selbst ist nicht Teil dieses Repositorys.
