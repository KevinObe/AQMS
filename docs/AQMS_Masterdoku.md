# AQMS-2026 — Master-Dokumentation

**Aquarium Monitoring & Control System**
Kevin Oberluggauer · FAAI WIFI Tirol · 2025/2026

> **Hinweis zur veröffentlichten Fassung.** Dies ist die bereinigte
> Abgabe-Version. Zwei Punkte weichen vom Arbeitsdokument ab:
>
> 1. **Anonymisierung.** Server-IPs, Domain, SSH-Benutzername und
>    E-Mail-Adressen sind durch reservierte Doku-Werte ersetzt
>    (`203.0.113.x` nach RFC 5737, `aqms.example.com` nach RFC 2606,
>    Benutzer `deployuser`). Sie sind absichtlich nicht funktionsfähig.
>    Die Adressen `10.0.0.222`–`10.0.0.227` sind private LAN-Adressen
>    (RFC 1918) und blieben unverändert.
> 2. **Entfernte Verweise.** Im Text genannte Begleitdateien —
>    `AQMS_Projektstand_<datum>.md` sowie die Ordner `docs/DB_Setup`,
>    `docs/db_schema`, `docs/VS_Setup`, `docs/VPS_Setup`, `docs/Hardware`
>    und `docs/Projektmanagement` — sind nicht Teil dieser Veröffentlichung.
>    Ihr Inhalt ist in dieses Dokument konsolidiert; die Verweise bleiben
>    aus Gründen der Nachvollziehbarkeit im Text stehen.

---

> Diese Datei ist das **chronologische Projekt-Tagebuch** von AQMS-2026.
> Sie dokumentiert lückenlos den gesamten Verlauf von der ersten Projektidee
> bis zum aktuellen Stand: Projektplanung, Architektur, Hardware-Aufbau,
> Netzwerkkonfiguration, Visual-Studio-Setup, Persistenzschicht inkl. aller
> Designentscheidungen, VPS-Deployment — jeweils mit **Kontext, Begründung,
> Problemen und Lösungen**, einschließlich verworfener Alternativen und
> Praxisprobleme. Als Tagebuch enthält sie bewusst auch historische Stände,
> die inzwischen überholt sind (jeweils als solche markiert).
>
> **Für den aktuellen, bereinigten Ist-Stand des Projekts** (was *jetzt*
> gebaut und konfiguriert ist, ohne historische Drift) siehe die separate
> Datei `AQMS_Projektstand_<datum>.md`. Diese Master-Doku bleibt das
> Tagebuch; der Projektstand ist die Single Source of Truth für „wie ist es
> jetzt".
>
> Die Doku dient drei Zwecken: erstens als Wiedereinstiegsdokument bei Pausen,
> zweitens als Quellmaterial für die Diplomarbeit (insbesondere der
> Verlaufs- und Lessons-Learned-Teil), drittens als technische Referenz für
> die noch ausstehende Implementierung.
>
> Alle vorherigen Einzeldokumente in `docs/DB_Setup`, `docs/db_schema`,
> `docs/VS_Setup`, `docs/VPS_Setup`, `docs/Hardware` und `docs/Projektmanagement`
> wurden in diese Master-Doku konsolidiert. Die Originaldateien bleiben als
> Versionsstände erhalten, sind aber als VERALTET markiert.

**Stand:** 2026-06-07 · KW 23 · Phase 2 begonnen: Worker-Gerüst (Config + named HttpClient `aqms-api`) gebaut und verifiziert

---

## Inhaltsverzeichnis

**Teil A — Projekt & Konzeption**
1. [Projektüberblick und Zielsetzung](#1-projektüberblick-und-zielsetzung)
2. [Projektphasen, Meilensteine und Zeitplan](#2-projektphasen-meilensteine-und-zeitplan)
3. [Architektur und Datenfluss](#3-architektur-und-datenfluss)
4. [Technologie-Stack mit Begründungen](#4-technologie-stack-mit-begründungen)

**Teil B — Hardware und Netzwerk**
5. [Hardware-Komponenten und Sensor-Inbetriebnahme](#5-hardware-komponenten-und-sensor-inbetriebnahme)
   - 5.1 Komponenten-Übersicht
   - 5.2 Hardware-Bestand für Phase 1 (Beschaffung)
   - 5.3 Pi-OS-Installation und Erstkonfiguration
   - 5.4 DS18B20: Verkabelung am 40-Pin-GPIO-Header (J8)
   - 5.5 Das Pull-Up-Problem: Diagnose und Lösung
   - 5.6 1-Wire am Pi aktivieren
   - 5.7 Temperatur aus dem Linux-Dateisystem lesen
   - 5.8 Gesamtstand der Hardware-Phase
   - 5.9 Sicherheitsbetrachtung Hardware
6. [Raspberry Pi: WLAN-Setup](#6-raspberry-pi-wlan-setup)
7. [Shelly Smart Plugs: Netzwerk und API](#7-shelly-smart-plugs-netzwerk-und-api)
   - 7.1 Erst-Inbetriebnahme der Shellys (Shelly-App)
   - 7.2 DHCP-Reservierung am Router (A1 Glasfaser)
   - 7.3 Endgültiger IP-Plan
   - 7.4 Shelly-API: zwei Modelle
   - 7.5 Manuelle Tests vom Pi aus + Test-Loop
   - 7.6 Kommunikationsmodell
   - 7.7 Diplomarbeit-Argumentation
   - 7.8 Was der Worker später macht

**Teil C — Entwicklungsumgebung und Solution**
8. [Entwicklungstools](#8-entwicklungstools)
9. [Solution-Struktur: Erstaufbau und Restrukturierung](#9-solution-struktur-erstaufbau-und-restrukturierung)
10. [NuGet-Pakete](#10-nuget-pakete)
11. [Konfigurationsdateien](#11-konfigurationsdateien)
12. [Git/GitHub-Setup](#12-gitgithub-setup)

**Teil D — Persistenzschicht (Datenbank)**
13. [Anforderungen an die Persistenzschicht](#13-anforderungen-an-die-persistenzschicht)
14. [Schema-Iteration: vom ersten Entwurf zum aktuellen Stand](#14-schema-iteration-vom-ersten-entwurf-zum-aktuellen-stand)
15. [Normalformen und 3NF/BCNF](#15-normalformen-und-3nfbcnf)
16. [Architekturentscheidungen im Datenmodell](#16-architekturentscheidungen-im-datenmodell)
17. [Entity-Klassen im Detail](#17-entity-klassen-im-detail)
18. [AqmsDbContext](#18-aqmsdbcontext)
19. [OnModelCreating und Fluent API](#19-onmodelcreating-und-fluent-api)
20. [Tiefenkonzepte zum DbContext](#20-tiefenkonzepte-zum-dbcontext)
21. [Indizes, OnDelete und DB-Defaults](#21-indizes-ondelete-und-db-defaults)
22. [Migrations-Workflow und Seed-Daten](#22-migrations-workflow-und-seed-daten)

**Teil E — VPS und Deployment (Test-Stand)**

*Aktueller Server (Debian 13):*
- 23A. [VPS-Migration: Anlass und Entscheidung](#23a-vps-migration-anlass-und-entscheidung)
- 23B. [DNS-Umstellung auf den neuen Server](#23b-dns-umstellung-auf-den-neuen-server)
- 23C. [Server-Grundeinrichtung](#23c-server-grundeinrichtung)
- 23D. [.NET 10 Runtime installieren (Debian 13)](#23d-net-10-runtime-installieren-debian-13)
- 23E. [Docker installieren (Debian 13)](#23e-docker-installieren-debian-13)
- 23F. [SQL Server in Docker](#23f-sql-server-in-docker)
- 23G. [Deployment der AQMS.Web](#23g-deployment-der-aqmsweb)
- 23H. [systemd-Service](#23h-systemd-service)
- 23I. [Nginx Reverse Proxy](#23i-nginx-reverse-proxy)
- 23J. [HTTPS mit Let's Encrypt](#23j-https-mit-lets-encrypt)
- 23K. [Erster Identity-User und Deaktivierung der Registrierung](#23k-erster-identity-user-und-deaktivierung-der-registrierung)
- 23L. [API-Key-Middleware](#23l-api-key-middleware)
- 23M. [MeasurementsController](#23m-measurementscontroller)
- 23N. [CommandsController und Service-Layer](#23n-commandscontroller-und-service-layer-commandservice)
- 23O. [Deployment des Worker-Service auf dem Raspberry Pi](#23o-deployment-des-worker-service-auf-dem-raspberry-pi)
- 23P. [Befehls-Erstellung, Autorisierung und Dashboard](#23p-befehls-erstellung-autorisierung-und-dashboard)
- 23Q. [Unit-Tests mit xUnit](#23q-unit-tests-mit-xunit)
- 23R. [Messintervall und 24-h-Dauerlauf](#23r-messintervall-und-24-h-dauerlauf)
- 23S. [Phase 5: Abschluss der Integrations- und Sicherheitstests](#23s-phase-5-abschluss-der-integrations-und-sicherheitstests)

*Historisch (alter VPS, Debian 12):*
23. [VPS-Vorbereitung und Runtime-Installation](#23-vps-vorbereitung-und-runtime-installation)
24. [Deploy-Verzeichnis und manueller Test](#24-deploy-verzeichnis-und-manueller-test)
25. [systemd-Service und 217/USER-Problem](#25-systemd-service-und-217user-problem)
26. [Nginx Reverse Proxy](#26-nginx-reverse-proxy)
27. [HTTPS mit Let's Encrypt](#27-https-mit-lets-encrypt)
28. [Praxisproblem: Upload-Rechte für deployuser](#28-praxisproblem-upload-rechte-für-deployuser)

**Teil F — Diplomarbeit-Material**
29. [Argumentationen für die Verteidigung](#29-argumentationen-für-die-verteidigung)
30. [Reflexion: Verworfene Alternativen](#30-reflexion-verworfene-alternativen)
31. [Glossar](#31-glossar)

**Teil G — Status und Roadmap**
32. [Aktueller technischer Stand](#32-aktueller-technischer-stand)
33. [Nächste Schritte](#33-nächste-schritte)
34. [Doku-Versionierung und veraltete Dateien](#34-doku-versionierung-und-veraltete-dateien)

---

# Teil A — Projekt & Konzeption

## 1. Projektüberblick und Zielsetzung

### 1.1 Was ist AQMS?

AQMS-2026 ist ein **Aquarium Monitoring & Control System**, das im Rahmen
der Diplomarbeit an der FAAI (Fachausbildung für Angewandte Informatik) am
WIFI Tirol entsteht. Das System überwacht und steuert ein Süßwasseraquarium
und besteht aus drei verteilten Komponenten:

- **Edge Device** (Raspberry Pi 3B+ im Heimnetz): Temperaturmessung über
  DS18B20-Sensor, Steuerung der Shelly Smart Plugs.
- **Backend** (Linux-VPS, öffentlich erreichbar als
  `aqms.aqms.example.com`): ASP.NET-Core-MVC-Anwendung mit SQL-Server-DB,
  REST-API und Web-Dashboard.
- **Aktoren** (5x Shelly Smart Plug): schalten 230V-Verbraucher
  (Filter, Licht, CO2-Anlage, Skimmer, Heizstab).

### 1.2 Projektziel

Das System soll:

- **Sensorwerte** kontinuierlich erfassen: Temperatur (DS18B20),
  Leistung (Watt von Shelly Plus 1PM).
- **Geräte schalten** über das Web-Dashboard.
- **Schaltzustände dokumentieren**: vollständiger Audit-Trail wer wann
  welches Gerät ein/aus geschaltet hat.
- **Mehrbenutzerfähig** sein: Login mit Rollen (Admin, User), Audit-Trail.
- **Fehlertolerant** funktionieren: auch bei kurzzeitigen
  Verbindungsabbrüchen weiterlaufen.

### 1.3 Eckdaten

| Eckpunkt | Wert |
|---|---|
| Bearbeitungszeitraum | 31.03.2026 – 31.08.2026 |
| Gesamtbudget | ca. 200 € |
| Projektleitung / Diplomand | Kevin Oberluggauer |
| Bildungseinrichtung | FAAI WIFI Tirol |
| Domain Backend | aqms.aqms.example.com |
| VPS IP (aktuell, Debian 13) | 203.0.113.10 |
| VPS IP (alt, Debian 12, stillgelegt) | 203.0.113.11 |
| Pi IP (Heimnetz) | 10.0.0.222 |

### 1.4 Warum dieses Projekt?

Die fachliche Motivation: ein Aquarium ist ein lebendes System mit täglich
wiederkehrenden Steuerungsaufgaben (Licht ein/aus, CO2 nach Lichtphase,
Filterzeiten). Manuelle Steuerung ist fehleranfällig, kommerzielle
Lösungen sind teuer und proprietär. Das Projekt baut eine offene,
nachvollziehbare Eigenentwicklung mit klassischem Webstack — zugleich als
exemplarische, praxisnahe Anwendung für die Diplomarbeit.

Die technische Motivation: das Projekt deckt **realistische Themen** ab —
verteilte Architektur über NAT hinweg, persistente Datenhaltung mit
historischen Time-Series, Authentifizierung, Deployment auf einem
Linux-VPS, Code-First-Datenbankdesign in 3NF/BCNF.

---

## 2. Projektphasen, Meilensteine und Zeitplan

### 2.1 Phasenübersicht

Das Projekt ist in sechs Phasen gegliedert. Die Phasen folgen dem
PSP-Strukturplan ([docs/Projektmanagement/AQMS_PSP.md](Projektmanagement/AQMS_PSP.md))
und enden jeweils mit einem definierten Meilenstein.

| Nr. | Phase | Zeitraum | Meilenstein | Datum | Abnahme |
|---|---|---|---|---|---|
| 1 | Vorbereitung | KW 14–17 | MS1 — Architektur + Hardware freigegeben | 14.04.2026 | WIFI-Trainer |
| 2 | Edge Device (Worker) | KW 18–20 | MS2 — Pi sendet Daten + steuert Shellys | 19.05.2026 | Selbstabnahme |
| 3 | Backend (ASP.NET) | KW 20–25 | MS3 — Backend deployed + API + Login aktiv | 09.06.2026 | WIFI-Trainer |
| 4 | Frontend (Razor) | KW 25–28 | MS4 — End-to-End Login → Toggle → Status | 30.06.2026 | WIFI-Trainer |
| 5 | Testing | KW 28–31 | MS5 — Alle Tests bestanden, System stabil | 21.07.2026 | WIFI-Trainer |
| 6 | Doku & Abgabe | KW 31–36 | MS6 — Diplomarbeit eingereicht | 31.08.2026 | Prüfungskommission |

### 2.2 Aktueller Phasen-Stand

> Der laufend gepflegte Phasen- und Komponentenstand steht in **§32**
> (innerhalb dieses Tagebuchs) sowie in der separaten, bereinigten Datei
> **`AQMS_Projektstand_<datum>.md`** (Single Source of Truth für den
> Ist-Stand). Dieser Abschnitt wird **nicht mehr separat gepflegt**, um
> konkurrierende Status-Angaben zu vermeiden.
>
> Kurzfassung zum Datum dieser Doku-Version: Phase 1 abgeschlossen; Phase 2
> (Worker-Software) offen bei stehender Hardware; Phase 3 (Backend)
> teilweise — Persistenz, IdentitySeeder, API-Key-Middleware und
> MeasurementsController stehen und sind VPS-verifiziert; Phasen 4–6
> ausstehend.

### 2.3 GZ 1 — Vorbereitung (abgeschlossen)

**GZ 1.1 — Projektdefinition & Architektur**

| ID | Feinziel | Status |
|---|---|---|
| 1.1.1 | Projektauftrag finalisieren & freigeben | ✓ |
| 1.1.2 | Architektur definieren | ✓ |
| 1.1.3 | DB-Schema entwerfen | ✓ (mehrere Iterationen, siehe §14) |
| 1.1.4 | Sicherheitskonzept | ✓ (Identity, HTTPS, API-Key geplant) |
| 1.1.5 | Solution-Struktur anlegen | ✓ (mit Umbenennung, siehe §9) |

**GZ 1.2 — Hardware-Beschaffung & Aufbau**

| ID | Feinziel | Status |
|---|---|---|
| 1.2.1 | Komponenten bestellen | ✓ |
| 1.2.2 | Raspberry Pi einrichten | ✓ (Debian Trixie, NetworkManager) |
| 1.2.3 | DS18B20 verdrahten + 1-Wire Bustest | ✓ |
| 1.2.4 | Shelly Smart Plug konfigurieren + API testen | ✓ (5 Geräte mit DHCP-Reservierungen) |

### 2.4 Kritische Erfolgsfaktoren

- Stabile HTTPS-Verbindung Pi → VPS
- Polling-Mechanismus zuverlässig (Befehl darf nicht verloren gehen)
- EF Core Migrations fehlerfrei auf VPS anwendbar
- Shelly im gleichen WLAN wie Pi erreichbar
- **Kein 230V-Direktbetrieb** — ausschließlich über Shelly Smart Plugs

### 2.5 Budget

| Posten | Kosten |
|---|---|
| Raspberry Pi 3B+ | 50 € |
| DS18B20 Sensor + Kleinteile | 15 € |
| Shelly Smart Plug(s) | 60 € |
| VPS (5 Monate × 10 €) | 50 € |
| Sonstiges | 25 € |
| **Gesamt** | **ca. 200 €** |

---

## 3. Architektur und Datenfluss

### 3.1 Übersicht

```
                    Browser (User)
                         │
                         │  HTTPS (Login, Dashboard, Toggle)
                         ▼
        ┌────────────────────────────────────────┐
        │  VPS (Linux, aqms.aqms.example.com)       │
        │  ┌──────────────────────────────────┐  │
        │  │  Nginx (Port 443, TLS)           │  │
        │  │     ↓ Reverse Proxy              │  │
        │  │  Kestrel (Port 5000)             │  │
        │  │     ↓                            │  │
        │  │  ASP.NET Core MVC (AQMS.Web)     │  │
        │  │     ↓ EF Core                    │  │
        │  │  SQL Server Datenbank            │  │
        │  └──────────────────────────────────┘  │
        └────────────────────────────────────────┘
                         ▲
                         │  HTTPS (REST + Polling)
                         │  POST /api/measurements
                         │  GET  /api/commands/pending
                         │  POST /api/commands/result
                         │
        ┌────────────────────────────────────────┐
        │  Heimnetz (10.0.0.0/24, hinter NAT)    │
        │                                        │
        │  ┌──────────────────────────────────┐  │
        │  │  Raspberry Pi 3B+ (10.0.0.222)   │  │
        │  │  C# Worker Service (AQMS.Worker) │  │
        │  │   ↓ DS18B20 (1-Wire /sys/bus/w1) │  │
        │  │   ↓ HTTP-Client                  │  │
        │  └──────────────────────────────────┘  │
        │       │                                │
        │       │  HTTP (lokales LAN)            │
        │       │  GET /relay/0?turn=on|off      │
        │       ▼                                │
        │  ┌──────────────────────────────────┐  │
        │  │  Shelly Smart Plugs              │  │
        │  │  10.0.0.223–227 (5 Geräte)       │  │
        │  └──────────────────────────────────┘  │
        │       │                                │
        │       ▼                                │
        │  Verbraucher: Filter, Licht, CO2,      │
        │  Heizstab, Skimmer (230V)              │
        └────────────────────────────────────────┘
```

### 3.2 Wichtigste Architekturentscheidung: Polling-Pattern

Der Pi sitzt im privaten Heimnetz hinter NAT (Router) und ist vom VPS
**nicht direkt erreichbar**. Daraus ergibt sich zwingend die Kommunikationsrichtung:

```
VPS → Pi:  Verbindung aufbauen → NICHT MÖGLICH (private IP)
Pi → VPS:  Verbindung aufbauen → PROBLEMLOS (ausgehende Verbindung)
```

Konsequenz: **der Pi initiiert immer die Verbindung**. Für die
Datenübertragung ist das einfach (POST). Für die Steuerung würde man
eigentlich Push wollen — der VPS soll dem Pi sagen, "Filter einschalten".
Da das nicht geht, wird **Polling** verwendet: der Pi fragt alle 10 Sekunden
beim VPS nach neuen Befehlen. Das ist nicht ein Kompromiss, sondern die
**technisch richtige Lösung** unter der gegebenen Netzwerktopologie.

Verworfene Alternativen:

- **WebSocket / SignalR**: VPS müsste Pi erreichen → unmöglich.
- **VPN (Tailscale, WireGuard)**: zusätzliche Infrastruktur, ausserhalb
  des Projektumfangs.
- **Reverse Tunnel** (autossh): brüchig, schwer zu betreiben.

Nachteil von Polling: Maximale Reaktionszeit = Polling-Intervall = 10
Sekunden. Für ein Aquarium völlig akzeptabel.

### 3.3 API-Endpunkte (geplant)

| Methode | Endpoint | Aufrufer | Zweck |
|---|---|---|---|
| `POST` | `/api/measurements` | Pi → VPS | Messwerte hochladen |
| `GET` | `/api/commands/pending` | Pi → VPS | offene Schaltbefehle pollen |
| `POST` | `/api/commands/result` | Pi → VPS | Befehlsergebnis melden |
| `POST` | `/device/toggle/{id}` | Browser → VPS | Toggle-Befehl anlegen |
| `GET` | `/dashboard` | Browser → VPS | Live-Daten + Verlauf |

Schutz der Pi-API durch **API-Key Middleware** (Header-Validierung), Schutz
der Web-Routen durch **ASP.NET Identity** mit Rollen.

---

## 4. Technologie-Stack mit Begründungen

### 4.1 Übersicht

| Schicht | Komponente | Technologie | Nativ / Extern |
|---|---|---|---|
| Edge Device | Raspberry Pi 3B+ | C# Worker Service (.NET 10) | Nativ .NET |
| Edge Device | Temperatursensor | DS18B20 / 1-Wire / sysfs | Linux Kernel |
| Edge Device | Aktorsteuerung | Shelly HTTP-API | Extern (Hardware) |
| Backend | Webserver | Kestrel | Nativ ASP.NET Core |
| Backend | Web-Framework | ASP.NET Core MVC | Nativ .NET |
| Backend | HTML-Templates | Razor Views | Nativ ASP.NET Core |
| Backend | ORM / DB | EF Core 10 + SQL Server | Nativ .NET / Microsoft |
| Backend | Authentifizierung | ASP.NET Identity | Nativ ASP.NET Core |
| Backend | Reverse Proxy | Nginx | Extern (Linux-Tool) |
| Backend | TLS-Zertifikat | Let's Encrypt / Certbot | Extern (kostenlos) |
| Frontend | CSS Framework | Bootstrap 5 | Extern (CDN) |
| Frontend | Diagramme | Chart.js | Extern (CDN) |
| Kommunikation | Pi → VPS | HTTPS / REST / Polling | Konzept |
| Kommunikation | Pi → Shelly | Lokale HTTP-API | Konzept |
| Sicherheit | API-Schutz | API-Key Middleware | Selbst implementiert |

### 4.2 Detaillierte Begründungen

**Kestrel** ist der eingebaute HTTP-Webserver von ASP.NET Core. Auf Windows
gibt es IIS, auf Linux nicht — Kestrel übernimmt diese Rolle. Plattformübergreifend,
direkt in .NET integriert, kein separates Tool. Im Deployment läuft er
intern auf Port 5000, davor sitzt Nginx mit TLS auf 443.

**Razor Views mit MVC** wurde gegenüber folgenden Alternativen gewählt:

| Alternative | Warum nicht |
|---|---|
| Blazor Server | Zu komplex, SignalR-Overhead |
| Blazor WASM | Großer Initial-Download, langsamer Start |
| Reine API + React/Vue | Zwei Projekte, mehr Aufwand |
| Razor Pages | Weniger strukturiert für MVC-Trennung |

Razor mit MVC ist der direkteste Weg: Controller → ViewModel → View → HTML.

**ASP.NET Identity** ist vollständig nativ in ASP.NET Core enthalten —
Login, Logout, Passwort-Hashing, Session-Cookies, Rollen, alles vorgefertigt.
Eine Zeile in Program.cs reicht. Kein externer Auth-Dienst (Auth0, Firebase,
Keycloak) nötig.

**Polling statt SignalR/WebSockets** ist erzwungen durch die NAT-Topologie
(siehe §3.2). Polling ist die einfachste, robusteste und am besten
dokumentierbare Variante.

**Chart.js** ist die einzige echte externe Frontend-Abhängigkeit. Per CDN
eingebunden, kein npm/Build-Prozess. Kostenlos, gut dokumentiert, deutlich
einfacher als D3.js, kostenlos im Gegensatz zu Highcharts.

**Nginx als Reverse Proxy** vor Kestrel:

- TLS-Terminierung (Let's Encrypt einfacher zu verwalten)
- Sicherheit: Kestrel ist nicht für direkte Internet-Exposition designed
- Standard: offizielle Microsoft-Empfehlung auf Linux
- erlaubt mehrere Domains/Subdomains auf einem Server

**SQL Server LocalDB** für Entwicklung, **SQL Server** für Produktion. Microsofts
nativer DB-Provider, beste EF-Core-Integration. Code-First mit Migrations
für versionierte Schema-Änderungen.

**Code-First mit EF Core**: Datenbankschema wird aus C#-Klassen generiert,
nicht per Hand in SQL definiert. Vorteile: Schema im Source-Control,
reproduzierbar auf jedem System, Compiler garantiert Konsistenz zwischen
Anwendungsmodell und Datenmodell.

---

# Teil B — Hardware und Netzwerk

## 5. Hardware-Komponenten und Sensor-Inbetriebnahme

### 5.1 Komponenten-Übersicht

| Komponente | Modell | Adresse | Funktion |
|---|---|---|---|
| Edge Device | Raspberry Pi 3B+ | 10.0.0.222 | Sensor-Auslesung + Aktor-Steuerung |
| Temperatur-Sensor | DS18B20 (1-Wire, wasserdicht, 3-adrig) | — | Wassertemperatur |
| Pull-Up-Widerstand | 4,7 kΩ | — | erforderlich für 1-Wire-Bus |
| Verbindungskabel | Jumper / Dupont | — | GPIO-Anschluss am 40-Pin-Header |
| Smart Plug 1 | Shelly | 10.0.0.227 | Filter |
| Smart Plug 2 | Shelly | 10.0.0.226 | Licht |
| Smart Plug 3 | Shelly | 10.0.0.225 | CO2-Anlage |
| Smart Plug 4 | Shelly | 10.0.0.224 | Heizstab |
| Smart Plug 5 | Shelly | 10.0.0.223 | Skimmer |

Optional als Reserve / spätere Erweiterung verfügbar:
**Raspberry Pi Pico Starter Kit** (aktuell nicht im Projekt aktiv).

Alle Geräte hängen im selben Subnetz `10.0.0.0/24`. IP-Vergabe per
**DHCP mit MAC-Reservierung** im Router. Damit sind die IPs stabil über
Reboots, ohne dass am Gerät selbst etwas konfiguriert werden muss.

### 5.2 Hardware-Bestand für Phase 1 (Beschaffung)

Die folgenden Komponenten wurden für die Hardware-Phase beschafft:

| Komponente | Anzahl | Verwendung |
|---|---|---|
| Raspberry Pi 3 Model B+ | 1 | Edge Device |
| microSD-Karte (mind. 8 GB) | 1 | OS-Speicher |
| Netzteil (5V, 2.5A, micro-USB) | 1 | Stromversorgung Pi |
| DS18B20 Temperatursensor (wasserdicht, 3-adrig) | 1 | Wassertemperatur |
| Jumper-Kabel (Dupont) | mehrere | Verbindung Sensor ↔ GPIO |
| 4,7 kΩ Widerstand | 1 | Pull-Up auf 1-Wire-Datenleitung |
| Shelly Smart Plugs | 5 | Aktoren für 230V-Verbraucher |

**Wichtig:** Der DS18B20 in der wasserdichten 3-adrigen Variante hat ein
gekapseltes Edelstahl-Gehäuse mit etwa 1 m Kabel. Drei Adern (rot, schwarz,
gelb/weiß) — Farbzuordnung kann je nach Lieferant variieren, **immer mit
dem Datenblatt prüfen**.

### 5.3 Pi-OS-Installation und Erstkonfiguration

#### 5.3.1 OS-Image auf microSD schreiben

Verwendet wurde der **Raspberry Pi Imager** (offizielles Tool von der
Raspberry Pi Foundation, kostenlos).

Auswahl:

| Einstellung | Wert |
|---|---|
| Device | Raspberry Pi 3 |
| OS | Raspberry Pi OS Lite (64-bit) — basiert auf Debian Trixie |
| Storage | microSD-Karte |

"Lite" wurde bewusst gewählt — keine Desktop-Umgebung, headless-Betrieb,
weniger Ressourcen, weniger Angriffsfläche.

#### 5.3.2 Erste Anmeldung (lokal)

Erste Konfiguration erfolgte **lokal über Tastatur und Monitor** am Pi
(nicht headless), bevor das WLAN-Setup funktioniert hat:

| Schritt | Aktion |
|---|---|
| Benutzer anlegen | User `kev` mit Passwort |
| Netzwerk testen | initial per Ethernet (Kabel) — funktionierte |
| SSH aktivieren | `sudo raspi-config` → Interface Options → SSH → Enable |
| WLAN-Hardware prüfen | vorhanden, funktionsfähig |
| Bluetooth | vorhanden, optional, nicht aktiv genutzt |

Nach SSH-Aktivierung läuft die weitere Konfiguration headless über
`ssh kev@10.0.0.222`. WLAN-Setup siehe §6.

#### 5.3.3 Sicherheitshinweis Standardpasswort

Das Standard-Pi-Passwort wurde sofort auf ein eigenes geändert. Für
Produktion zusätzlich empfohlen:

- SSH-Schlüsselauthentifizierung statt Passwort
- `PasswordAuthentication no` in `/etc/ssh/sshd_config`
- Firewall (`ufw` oder `nftables`)

Das eigentliche Pi-Passwort gehört **nicht in diese Doku** und auch nicht
in das Git-Repo — es bleibt nur lokal.

### 5.4 DS18B20: Verkabelung am 40-Pin-GPIO-Header (J8)

Der DS18B20 hat **drei Leitungen**:

| Sensor-Leitung | Funktion |
|---|---|
| VCC / VDD | 3,3 V Versorgungsspannung |
| GND | Masse |
| DATA / DQ | 1-Wire Datenleitung |

Anschluss am Pi-GPIO-Header (J8):

```
Pi 40-Pin-Header (Auszug)
                                   _________________
              3,3 V    Pin 1  ----|  o   o  |--- Pin 2   5 V
              GPIO 2   Pin 3  ----|  o   o  |--- Pin 4   5 V
              GPIO 3   Pin 5  ----|  o   o  |--- Pin 6   GND
              GPIO 4   Pin 7  ----|  o   o  |--- Pin 8   GPIO 14 (TXD)
                                   - - - - - - - - -
```

**Konkrete Verdrahtung:**

| DS18B20 | Pi-Pin | Funktion |
|---|---|---|
| VCC | Pin 1 | 3,3 V |
| GND | Pin 6 (oder 9, 14, 20, 25, 30, 34, 39) | GND |
| DATA | Pin 7 | GPIO 4 (BCM4) — 1-Wire |

**Pull-Up-Widerstand 4,7 kΩ** wird **zwischen DATA (Pin 7) und VCC (Pin 1)**
geklemmt — direkt am Header oder auf einem Steckbrett.

```
            +3,3V (Pin 1) ─────┬──────── VCC (Sensor)
                               │
                              ┌┴┐
                              │ │  4,7 kΩ
                              │ │  Pull-Up
                              └┬┘
                               │
            GPIO 4 (Pin 7) ────┴──────── DATA (Sensor)

            GND (Pin 6) ──────────────── GND (Sensor)
```

### 5.5 Das Pull-Up-Problem: Diagnose und Lösung

> Diese Sektion dokumentiert ein konkretes Praxisproblem aus der
> Hardware-Inbetriebnahme — wertvoll für die Diplomarbeit, weil es zeigt,
> dass Hardware-Debugging strukturiert geht.

**Symptom:** nach dem ersten Anschluss des DS18B20 (ohne externen
Widerstand, in der Annahme der Sensor habe internen Pull-Up) erschien
der Sensor **nicht zuverlässig** im Linux-Dateisystem. Im Verzeichnis
`/sys/bus/w1/devices/` waren zeitweise nur generische Geräte sichtbar:

```
w1_bus_master1
00-xxxxxxxx           ← generisch, kein echter Sensor
```

Aber **nicht** das erwartete:

```
28-xxxxxxxx           ← Familie 28 = DS18B20
```

**Ursache:** der **4,7 kΩ Pull-Up-Widerstand fehlte**. Die DATA-Leitung
"floatete" — sie hatte keinen definierten Ruhepegel. Die 1-Wire-Kommunikation
ist auf einen sauberen HIGH im Ruhezustand angewiesen, von dem der Sensor
aktiv auf LOW zieht, wenn Daten gesendet werden. Ohne Pull-Up:

- zufällige Pegel auf der Leitung
- Sensor wird mal erkannt, mal nicht
- generische `00-*`-Geräte erscheinen, weil der Bus elektrisch instabil ist

**Fehlannahme im Vorfeld:** Händler-Beschreibungen suggerierten oft, dass
der Sensor einen "internen Pull-Up" habe. Das stimmt für **manche
fertigen Module** (z.B. das LK-Temp2 von Joy-IT), aber **nicht für den
rohen DS18B20** mit drei losen Adern. Bei diesem ist der externe Pull-Up
zwingend.

**Lösung:** 4,7 kΩ Widerstand zwischen DATA (Pin 7) und 3,3 V (Pin 1)
direkt am GPIO-Header eingesetzt. Sofort danach erschien:

```
$ ls /sys/bus/w1/devices/
28-xxxxxxxx        w1_bus_master1
```

Sensor stabil erkannt.

**Lessons Learned (für die Diplomarbeit):**

- Sensorik auf Hardware-Ebene erfordert korrekte elektrische Grundlagen
- Händler-Dokumentation ist nicht immer eindeutig (interner vs. externer Pull-Up)
- Linux abstrahiert Hardware über sysfs (`/sys/bus/w1/devices/`)
- Ein einziges fehlendes 0,02 €-Bauteil kann ein komplettes System blockieren
- Systematisches Debugging: erst Symptom, dann Ursache, dann Lösung — nicht raten

### 5.6 1-Wire am Pi aktivieren

1-Wire ist standardmäßig **deaktiviert**. Aktivierung über
`/boot/config.txt`:

```bash
sudo nano /boot/config.txt
```

Am Ende der Datei einfügen:

```
dtoverlay=w1-gpio,gpiopin=4
```

Speichern (Ctrl+O, Enter, Ctrl+X), dann **Reboot**:

```bash
sudo reboot
```

**Erklärung des Overlays:**

- `dtoverlay=w1-gpio` aktiviert das 1-Wire-Kernelmodul
- `gpiopin=4` legt fest, dass die DATA-Leitung an GPIO 4 (= Pin 7 am Header)
  angeschlossen ist
- Ohne expliziten `gpiopin`-Parameter wäre Default ebenfalls GPIO 4

Nach dem Neustart ist der 1-Wire-Bus aktiv. Verifikation:

```bash
ls /sys/bus/w1/devices/
# erwartete Ausgabe:
# 28-xxxxxxxx  w1_bus_master1
```

Die `28-`-Familie steht für den DS18B20. Andere 1-Wire-Geräte würden
andere Familien-Codes zeigen (z.B. `26-` für DS2438).

### 5.7 Temperatur aus dem Linux-Dateisystem lesen

Temperatur liegt als virtuelle Datei vor — kein spezielles API, einfach
`cat`:

```bash
cat /sys/bus/w1/devices/28-xxxxxxxx/w1_slave
```

Beispiel-Ausgabe:

```
b2 01 4b 46 7f ff 0e 10 d8 : crc=d8 YES
b2 01 4b 46 7f ff 0e 10 d8 t=27125
```

Format:

- Zeile 1: Roh-Bytes vom Sensor + CRC-Status (`YES` = OK, `NO` = ungültig)
- Zeile 2: Roh-Bytes + `t=` mit Temperatur in **Milligrad Celsius**

Im Beispiel: `t=27125` → 27,125 °C.

Alternative bei neueren Kerneln:

```bash
cat /sys/bus/w1/devices/28-xxxxxxxx/temperature
# Ausgabe: 27125
```

#### Auslesungs-Logik (vereinfacht in Pseudocode)

```
1. Datei lesen
2. Zeile 1 prüfen: endet mit "YES"? → CRC ok
                    endet mit "NO"   → erneut lesen
3. Zeile 2: nach "t=" suchen
4. Wert hinter "t=" parsen → int
5. ÷ 1000 → Celsius als float
```

#### Joy-IT-Referenz-Implementierung (Python)

Aus dem LK-Temp2-Datenblatt von Joy-IT — als **Code-Referenz**, nicht als
final verwendete Implementierung. Der spätere AQMS-Worker läuft in C#
und implementiert die gleiche Logik:

```python
import glob, time

base_dir = '/sys/bus/w1/devices/'

# Sensor-Verzeichnis suchen (28-* Familie)
device_folder = glob.glob(base_dir + '28*')[0]
device_file = device_folder + '/w1_slave'

def read_temp_raw():
    with open(device_file, 'r') as f:
        return f.readlines()

def read_temp():
    lines = read_temp_raw()
    # warten bis CRC ok
    while lines[0].strip()[-3:] != 'YES':
        time.sleep(0.2)
        lines = read_temp_raw()
    # t=-Wert extrahieren
    equals_pos = lines[1].find('t=')
    if equals_pos != -1:
        temp_string = lines[1][equals_pos + 2:]
        return float(temp_string) / 1000.0

print(f"Temperatur: {read_temp()} °C")
```

#### Geplante C#-Umsetzung im AQMS.Worker (Phase 2)

Skelett für die spätere Implementierung:

```csharp
public class DS18B20Reader
{
    private readonly string _devicePath;

    public DS18B20Reader(string deviceId)
    {
        _devicePath = $"/sys/bus/w1/devices/{deviceId}/w1_slave";
    }

    public async Task<double?> ReadAsync()
    {
        // 1. File lesen (kann blockieren bis Sensor-Daten da)
        var lines = await File.ReadAllLinesAsync(_devicePath);

        // 2. CRC-Check
        if (lines.Length < 2 || !lines[0].EndsWith("YES"))
            return null;

        // 3. t=-Wert parsen
        var idx = lines[1].IndexOf("t=", StringComparison.Ordinal);
        if (idx == -1) return null;

        if (!int.TryParse(lines[1].AsSpan(idx + 2), out var milliCelsius))
            return null;

        return milliCelsius / 1000.0;
    }
}
```

Wird im Worker-Service per `IHostedService` periodisch aufgerufen,
Wert über `HttpClient` an `POST /api/measurements` gesendet.

### 5.8 Gesamtstand der Hardware-Phase (abgeschlossen)

Nach dieser Phase steht:

- ✓ Raspberry Pi mit OS, SSH, WLAN, fester IP `10.0.0.222`
- ✓ DS18B20 verkabelt, mit Pull-Up
- ✓ 1-Wire aktiviert
- ✓ Temperaturwerte stabil im Dateisystem auslesbar
- ✓ Linux-Basisdienste funktionsfähig
- ✓ Alle 5 Shellys im Netz, per HTTP-API steuerbar (siehe §7)

Damit ist die Hardware-Phase **abgeschlossen** und das System ist bereit
für die Software-Logik (Worker-Service, Datenpersistenz, Backend-Anbindung).

### 5.9 Sicherheitsbetrachtung Hardware

**Kein 230V-Direktbetrieb.** Sämtliche schaltbaren Verbraucher hängen an
den Shelly Smart Plugs, die geprüfte Geräte mit eigenen Sicherheitsmechanismen
sind. Pi und Sensor laufen mit 3,3 V — vollständig getrennt von der
Netzspannung. Der DS18B20 ist in seiner wasserdichten Edelstahl-Variante
zugelassen für den Einsatz in Wasser, hat eine Schutzklasse gegen
Eindringen von Flüssigkeiten und ist galvanisch vom 230V-Netz getrennt.

---

## 6. Raspberry Pi: WLAN-Setup

### 6.1 Ausgangslage

- Raspberry Pi 3B+ mit **Debian Trixie** (Raspberry Pi OS Lite 64-bit)
- User: `kev` (für SSH und Login)
- Kein Netzwerkkabel verfügbar → WLAN-Verbindung nötig
- Ziel: stabile, automatische WLAN-Verbindung mit fester IP

### 6.2 Networking-Backend: NetworkManager

Debian Trixie verwendet **NetworkManager** für die Netzwerksteuerung,
nicht mehr die alten Tools `dhcpcd` / `ifupdown` / direktes
`wpa_supplicant`-Binding. Konfiguration über `nmcli`.

```bash
# NetworkManager aktivieren und starten
sudo systemctl enable NetworkManager
sudo systemctl start NetworkManager
systemctl status NetworkManager     # → active (running)
```

### 6.3 WLAN-Profil erstellen

```bash
# Verfügbare Netze scannen
nmcli device wifi list

# Profil anlegen (HomeWLAN ist die produktive SSID)
nmcli connection delete HomeWLAN 2>/dev/null
nmcli connection add type wifi ifname wlan0 con-name HomeWLAN ssid "HomeWLAN"

# WPA2-PSK setzen
nmcli connection modify HomeWLAN wifi-sec.key-mgmt wpa-psk
nmcli connection modify HomeWLAN wifi-sec.psk "DEIN_PASSWORT"

# Aktivieren
nmcli connection up HomeWLAN

# Auto-Reconnect bei Boot
nmcli connection modify HomeWLAN connection.autoconnect yes
```

### 6.4 Stabile IP über DHCP-Reservierung

Statt einer statischen IP am Pi wurde **DHCP-Reservierung am Router**
verwendet — die MAC-Adresse des Pi ist im Router fest auf `10.0.0.222`
gemappt.

Vorteile:

- Keine statische IP-Konfiguration am Gerät → kein IP-Konflikt-Risiko
- Zentrales Netzwerk-Management
- Reproduzierbares Verhalten über Reboots

### 6.5 Verifikation

```bash
ip a                               # zeigt 10.0.0.222/24
ping -c 3 1.1.1.1                  # Gateway erreichbar
ping -c 3 google.com               # DNS funktioniert
nmcli device                       # zeigt wlan0 connected HomeWLAN
```

Damit ist der Pi headless betreibbar — Boot mit automatischer
WLAN-Verbindung, fester IP, SSH-Zugriff über `ssh kev@10.0.0.222`.

---

## 7. Shelly Smart Plugs: Netzwerk und API

### 7.1 Erst-Inbetriebnahme der Shellys

Die fünf Shelly Smart Plugs wurden über die offizielle **Shelly-App**
(Smartphone-App, iOS/Android, kostenlos im jeweiligen Store) ins WLAN
eingebunden und konfiguriert. Schrittweise pro Gerät:

1. **Shelly mit Strom versorgen** (in eine Steckdose stecken). Beim ersten
   Start öffnet das Gerät einen eigenen WLAN-Hotspot (SSID typischerweise
   `ShellyPlugS-XXXXXX` oder `shellyplus1pm-XXXXXX`).
2. **Shelly-App öffnen** und neuen Account anlegen (oder ohne Cloud-Account
   im Lokalmodus arbeiten — das System nutzt nur die lokale HTTP-API,
   Cloud ist nicht erforderlich).
3. **"Gerät hinzufügen"** in der App → das Smartphone verbindet sich kurz
   mit dem Shelly-Hotspot, übergibt die WLAN-Credentials des HomeWLAN
   und der Shelly bucht sich selbst ins HomeWLAN ein.
4. **Gerätename vergeben** in der App: pro Gerät ein eindeutiger Name —
   `AQMS_Filter`, `AQMS_Light`, `AQMS_CO2`, `AQMS_Heater`, `AQMS_Skimmer`.
   Dieser Name wird als Hostname über DHCP an den Router gemeldet und
   erscheint dort in der Geräte-Liste.
5. **Funktionstest in der App**: per Toggle-Schalter in der App das
   Schalten des Shellys verifizieren — Klick-Geräusch des Relais hörbar,
   angeschlossener Verbraucher schaltet ein/aus.

Dieser Prozess wurde für alle 5 Shellys durchgeführt. Die Hostnames
liefern die Grundlage für die spätere Identifikation im Router (siehe §7.2).

### 7.2 DHCP-Reservierung am Router

**Router:** A1 Glasfaser-Router (genaues Modell projektintern nicht
weiter spezifiziert — der konkrete Web-Interface-Workflow ist im
Anhang der Diplomarbeit per Screenshots dokumentiert).

**Workflow (allgemein bei den meisten Routern, sinngemäß auch beim A1-Router):**

1. **Router-Web-Interface** im Browser öffnen (typischerweise
   `http://192.168.1.1`, `http://10.0.0.138` oder ähnlich — Adresse je
   nach Router-Konfiguration).
2. Login mit Admin-Zugangsdaten.
3. In den **Heimnetz-/LAN-Einstellungen** → **DHCP-Geräteliste** die fünf
   neu eingebuchten Shellys identifizieren (sichtbar an ihren Hostnames
   `AQMS_Filter` etc., die in der Shelly-App vergeben wurden).
4. Pro Shelly: **DHCP-Reservierung anlegen** — der Router merkt sich die
   MAC-Adresse und ordnet ihr immer dieselbe IP zu.
5. **Ziel-IPs vergeben** gemäß folgender Tabelle.

> **Hinweis:** Die konkreten Schritte im A1-Router-Interface sind in
> separaten Screenshots dokumentiert. Diese gehören als Bildanhang in die
> Diplomarbeit (Kapitel "Hardware-Setup" / "Netzwerkkonfiguration").
> Stand 09.05.2026 liegen die Screenshots noch außerhalb des
> `docs/`-Repositorys — sie sollten in `docs/Hardware/Router-Screenshots/`
> einsortiert und in der Diplomarbeit referenziert werden.

### 7.3 Endgültiger IP-Plan

Nach der DHCP-Reservierung ergibt sich folgende stabile Zuordnung:

| Funktion | Hostname (App-vergeben) | IP (DHCP-reserviert) | MAC |
|---|---|---|---|
| Pi (Sensor) | raspberrypi | 10.0.0.222 | (im Router) |
| Skimmer | AQMS_Skimmer | 10.0.0.223 | (im Router) |
| Heater | AQMS_Heater | 10.0.0.224 | (im Router) |
| CO2 | AQMS_CO2 | 10.0.0.225 | (im Router) |
| Light | AQMS_Light | 10.0.0.226 | (im Router) |
| Filter | AQMS_Filter | 10.0.0.227 | (im Router) |

**Vorteile dieser Lösung gegenüber statischer IP am Gerät selbst:**

- Geräte selbst bleiben unverändert (DHCP-Modus) — können bei Bedarf in
  ein anderes Netz umziehen ohne Re-Konfiguration
- IP-Zuordnung zentral im Router — eine einzige Stelle der Wahrheit
- Keine IP-Konflikte (Router weiß welche IPs er vergibt)
- Reproduzierbar: bei Router-Reset reichen die DHCP-Reservierungs-Einstellungen

### 7.4 Shelly-API: zwei Modelle

Shelly bietet je nach Gerätegeneration zwei API-Stile parallel an:

**Klassische HTTP-API** (Gen1, weiterhin bei Gen2+ kompatibel):

```
GET /relay/0?turn=on
GET /relay/0?turn=off
GET /status
GET /settings
GET /shelly
```

**JSON-RPC** (Gen2+ / Gen3 — der offizielle moderne Standard):

```
GET /rpc/Switch.Set?id=0&on=true
GET /rpc/Switch.Set?id=0&on=false
GET /rpc/Switch.Toggle?id=0
GET /rpc/Switch.GetStatus?id=0
GET /rpc/Switch.GetConfig?id=0
GET /rpc/Shelly.GetStatus
GET /rpc/Shelly.GetDeviceInfo
```

### 7.5 Manuelle Tests vom Pi aus (Phase 1, abgeschlossen)

Nach DHCP-Reservierung und Hostname-Vergabe wurden alle 5 Shellys vom Pi
aus per `curl` getestet — als Funktionsnachweis dass die Kommunikationskette
**Pi → HomeWLAN → Router → Shelly** funktioniert.

#### Einzeltest pro Shelly

```bash
# Vom Pi aus:
ssh kev@10.0.0.222

# Filter einschalten (klassisch)
curl http://10.0.0.227/relay/0?turn=on

# Status abfragen
curl http://10.0.0.227/status

# Modern (RPC)
curl "http://10.0.0.227/rpc/Switch.Set?id=0&on=true"
curl "http://10.0.0.227/rpc/Switch.GetStatus?id=0"
```

#### Test-Loop für alle 5 Shellys

Bash-Schleife zum Durchschalten aller Geräte (sinnvoll als
Hardwaretest-Skript, läuft direkt am Pi):

```bash
#!/bin/bash
# aqms-shelly-test.sh
# Alle 5 Shellys nacheinander an, kurz warten, aus

declare -A SHELLYS=(
    [filter]=10.0.0.227
    [light]=10.0.0.226
    [co2]=10.0.0.225
    [heater]=10.0.0.224
    [skimmer]=10.0.0.223
)

for name in "${!SHELLYS[@]}"; do
    ip="${SHELLYS[$name]}"
    echo ">>> $name ($ip): EIN"
    curl -s "http://$ip/relay/0?turn=on" > /dev/null
    sleep 2

    echo ">>> $name ($ip): Status"
    curl -s "http://$ip/status" | grep -o '"ison":[a-z]*'

    echo ">>> $name ($ip): AUS"
    curl -s "http://$ip/relay/0?turn=off" > /dev/null
    sleep 1
done

echo "Alle Shellys getestet."
```

Alle 5 Shellys reagierten erwartungsgemäß — hörbares Klick-Geräusch des
Relais, Status-Antwort `"ison":true`/`"ison":false` korrekt. Damit ist der
Funktionsnachweis für **Phase 1 (Hardware)** erbracht.

### 7.6 Kommunikationsmodell

- **Zustandslos** (kein persistentes Connection-State)
- **Pull-Modell**: nur der Pi initiiert Verbindungen zu den Shellys
- **HTTP** (nicht HTTPS) im LAN — bewusst gewählt, da:
  - Heimnetz vertrauenswürdig
  - keine Exponierung ins Internet
  - keine sensiblen Daten in der Übertragung
  - Komplexität minimiert (kein TLS-Setup auf jedem Shelly)

### 7.7 Diplomarbeit-Argumentation

Beide API-Stile in der Arbeit erwähnen:

> *Der klassische HTTP-Aufruf (`/relay/0?turn=on`) wurde für den schnellen
> Hardwaretest genutzt. Das moderne JSON-RPC-Modell (`Switch.Set`) stellt
> die offizielle API-Struktur aktueller Shelly-OS-Geräte dar (Shelly 2024)
> und wird im Worker-Service als bevorzugte Variante implementiert.*

Quellen: [shelly-api-docs.shelly.cloud](https://shelly-api-docs.shelly.cloud).

### 7.8 Was der Worker später macht

Das C#-Worker-Pattern (Phase 2, noch nicht implementiert):

```
1. Polling-Loop, alle 10s:
   GET aqms.aqms.example.com/api/commands/pending

2. Pro pendingem Befehl:
   GET 10.0.0.{ip}/relay/0?turn=on|off
   → Antwort verarbeiten

3. Ergebnis melden:
   POST aqms.aqms.example.com/api/commands/result
```

---

# Teil C — Entwicklungsumgebung und Solution

## 8. Entwicklungstools

### 8.1 Auf dem Windows-Entwicklungsrechner

| Tool | Zweck |
|---|---|
| Visual Studio 2022 Community | IDE mit ASP.NET-Templates, EF-Tooling, Debugger |
| .NET 10 SDK | Kommt mit VS, oder separat von dotnet.microsoft.com |
| SQL Server Developer Edition | Lokale DB für Entwicklung |
| SQL Server Management Studio (SSMS) | DB-Inspektion |
| Git + GitHub CLI (`gh`) | Versionskontrolle |
| FileZilla | Datei-Upload zum VPS |

Der eigentliche Workflow läuft **in Visual Studio**, nicht über die .NET CLI
als primäres Tool. Die Package Manager Console innerhalb von VS wird für
Migrations und NuGet-Installs verwendet.

### 8.2 Entscheidung: Visual Studio statt VS Code

Visual Studio bietet eingebaute Tools für:

- ASP.NET-Projektvorlagen (MVC, Worker, xUnit)
- EF-Core-Migrations (Add-Migration, Update-Database in der PMC)
- Razor-View-IntelliSense
- Integrierter Debugger für Web + Worker
- SQL Server Object Explorer

VS Code wäre möglich, würde aber zusätzliche Konfiguration und Extensions
erfordern.

### 8.3 Auf dem Raspberry Pi

Bereits eingerichtet (siehe §6):

- Debian Trixie
- SSH-Zugriff über `ssh kev@10.0.0.222`
- DS18B20 angeschlossen und auslesbar

Auf dem Pi wird später nur die **Runtime** des Worker-Service liegen — das
Build erfolgt auf dem Entwicklungsrechner mit `dotnet publish`.

---

## 9. Solution-Struktur: Erstaufbau und Restrukturierung

### 9.1 Erste Solution-Struktur (Stand März 2026)

Beim ersten Aufbau wurde folgende Struktur gewählt — bewusst entlang der
Systemarchitektur in drei Projekte:

| Projekt | Vorlage | Zweck | Läuft auf |
|---|---|---|---|
| `RaspiWorkerService` | Worker Service | Sensor + Shelly-Steuerung | Raspberry Pi |
| `AQMS_Web` | ASP.NET Core Web App (MVC) | Backend + Frontend | Linux VPS |
| `AQMS_Test` | xUnit Test Project | Unit Tests | Entwicklungsrechner |

Die Solution selbst wurde als **Blank Solution** angelegt, die drei
Projekte wurden danach hinzugefügt.

**Wichtige Optionen beim Erstellen des Web-Projekts:**

| Option | Wert | Begründung |
|---|---|---|
| MVC | ja | Architektur sieht sowohl Backend-Logik als auch Web-UI vor |
| HTTPS | ja | realistische Auth-Cookie-Tests in Dev |
| Authentication Type | None (initial) | Identity wurde später bewusst manuell integriert |
| EF Auto-Scaffold | nein | bewusst manuell, damit nachvollziehbar |

Im Test-Projekt wurde eine Projekt-Referenz auf das Worker-Projekt
hinzugefügt, damit dort später Klassen unit-testbar sind.

### 9.2 Erste Ordnerstruktur im Web-Projekt

```
AQMS_Web
├── Controllers
├── Data
├── Models
├── Repositories       ← (vorgesehen, später leer geblieben)
├── Services           ← (vorgesehen, später leer geblieben)
├── Views
├── wwwroot
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```

### 9.3 Restrukturierung (Mai 2026): zur .NET-Standard-Notation

Während der DB-Schema-Iteration wurde die Solution **umgebaut** — primär
um zur Standard-.NET-Punkt-Notation zu wechseln und um das MVC-Template
mit Identity-UI als Basis zu nutzen.

**Änderungen:**

| Vorher | Nachher | Grund |
|---|---|---|
| `AQMS_Web` | `AQMS.Web` | .NET-Standard: Punkt für Namespace-Hierarchie |
| `AQMS_Test` | `AQMS.Tests` | Plural + Punkt-Notation |
| `RaspiWorkerService` | `AQMS.Worker` | konsistente Benennung mit AQMS-Präfix |
| Solution-Datei `.sln` | `.slnx` (XML-Format) | modernes Solution-Format |
| Web-Projekt: ohne Identity-UI | mit Identity-UI-Template | bringt Login/Register-Razor-Pages mit |

Die alten Projektordner (`AQMS_Web`, `AQMS_Test`, `RaspiWorkerService`)
wurden gelöscht. Im Git-Status erscheinen sie als "deleted" — die neuen
Projekte als "untracked" bis zum Commit.

### 9.4 Aktuelle Solution (Stand Mai 2026)

```
AQMS-2026/
├── AQMS.slnx                          ← Solution-Datei (XML-Format)
├── AQMS.Web/                          ← ASP.NET Core MVC + Identity
│   ├── Areas/Identity/                ← Razor Pages für Login (Identity-UI)
│   ├── Controllers/HomeController.cs
│   ├── Data/
│   │   ├── AqmsDbContext.cs           ← der zentrale DbContext
│   │   └── Migrations/                ← EF-Core-Migrations
│   ├── Models/
│   │   ├── Device.cs
│   │   ├── DeviceType.cs
│   │   ├── DeviceCommand.cs
│   │   ├── Measurement.cs
│   │   ├── MeasurementType.cs
│   │   ├── StateChange.cs
│   │   └── ErrorViewModel.cs
│   ├── Views/
│   ├── wwwroot/
│   ├── appsettings.json
│   ├── Program.cs
│   └── AQMS.Web.csproj
├── AQMS.Worker/                       ← Skelett, noch keine Logik
│   ├── Program.cs
│   ├── Worker.cs                      ← Default-Skelett mit Logging
│   └── AQMS.Worker.csproj
├── AQMS.Tests/                        ← Skelett
└── docs/                              ← diese Dokumentation
```

### 9.5 Inhalt der `AQMS.slnx`

```xml
<Solution>
  <Project Path="AQMS.Tests/AQMS.Tests.csproj" />
  <Project Path="AQMS.Web/AQMS.Web.csproj" />
  <Project Path="AQMS.Worker/AQMS.Worker.csproj" />
</Solution>
```

Das `.slnx`-Format ist die moderne XML-basierte Variante der klassischen
`.sln`-Datei. Funktional gleichwertig, aber besser merge-bar in Git.

---

## 10. NuGet-Pakete

### 10.1 AQMS.Web — Pakete

Aus [AQMS.Web/AQMS.Web.csproj](../AQMS.Web/AQMS.Web.csproj):

```xml
<PackageReference Include="Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore" Version="10.0.6" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.6" />
<PackageReference Include="Microsoft.AspNetCore.Identity.UI" Version="10.0.6" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.6" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.6" PrivateAssets="all" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.6" />
```

**Zweck der Pakete:**

| Paket | Wofür |
|---|---|
| `Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore` | Migrations-Endpoint für Dev (`UseMigrationsEndPoint`) |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | Identity-Speicher in EF Core |
| `Microsoft.AspNetCore.Identity.UI` | fertige Razor Pages für Login/Register |
| `Microsoft.EntityFrameworkCore.SqlServer` | EF-Provider für SQL Server |
| `Microsoft.EntityFrameworkCore.Design` | Design-Time-Tooling (Add-Migration) |
| `Microsoft.EntityFrameworkCore.Tools` | PowerShell-Cmdlets in der PMC |

`PrivateAssets="all"` bei Design bedeutet: das Paket wird beim Build des
Web-Projekts genutzt, aber nicht als Runtime-Abhängigkeit weiterpropagiert.

### 10.2 AQMS.Worker — Pakete

```xml
<PackageReference Include="Microsoft.Extensions.Hosting" Version="10.0.6" />
<PackageReference Include="Microsoft.Extensions.Http" Version="10.0.6" />
```

`Microsoft.Extensions.Hosting` ist das Default-Worker-Service-Template.
`Microsoft.Extensions.Http` wurde am 2026-06-07 ergänzt, als der `HttpClient`
für den Polling-Worker registriert wurde (§11.6). Es liefert die Extension-
Methode `AddHttpClient(...)` und den `IHttpClientFactory`.

**Warum nicht `new HttpClient()`?** Der Worker läuft dauerhaft und pollt im
Intervall. Würde pro Request ein `new HttpClient()` erzeugt, blieben dessen
Sockets nach dem Dispose im `TIME_WAIT`-Zustand hängen — bei einem
Langläufer führt das nach Stunden zur Socket-Erschöpfung. `AddHttpClient`
registriert stattdessen einen über `IHttpClientFactory` verwalteten Client,
der die `HttpMessageHandler` poolt und deren Lebensdauer steuert. Als
*named client* (`"aqms-api"`) bündelt er BaseAddress und den
`X-API-Key`-Header an einer Stelle.

Im Verlauf von Phase 2 kommen voraussichtlich noch dazu:

- `System.IO.Ports` (eventuell, falls 1-Wire-Fallback nötig)
- `Polly` (für Retry-Logik bei HTTP-Calls)

### 10.3 AQMS.Tests — Pakete

Standard xUnit-Template, später Erweiterung um:

- `Microsoft.EntityFrameworkCore.InMemory` (für DbContext-Tests)
- `Moq` (Mocking)

### 10.4 Installations-Befehle (zur Reproduktion)

```powershell
# In der Package Manager Console mit Default-Project = AQMS.Web
Install-Package Microsoft.EntityFrameworkCore.SqlServer
Install-Package Microsoft.EntityFrameworkCore.Tools
Install-Package Microsoft.EntityFrameworkCore.Design
Install-Package Microsoft.AspNetCore.Identity.EntityFrameworkCore
Install-Package Microsoft.AspNetCore.Identity.UI
```

Für das Worker-Projekt (2026-06-07):

```powershell
dotnet add AQMS.Worker package Microsoft.Extensions.Http
```

### 10.5 Praxisproblem: `AddHttpClient` wird nicht als Methode erkannt

**Symptom.** Beim Registrieren des `HttpClient` in
[`AQMS.Worker/Program.cs`](../AQMS.Worker/Program.cs) meldet der Compiler,
dass `AddHttpClient` keine bekannte Methode von `IServiceCollection` ist —
obwohl derselbe Aufruf im Web-Projekt anstandslos funktioniert.

**Ursache.** `AddHttpClient` ist eine Extension-Methode aus dem NuGet-Paket
`Microsoft.Extensions.Http`. Das Web-Projekt referenziert das ASP.NET-Core-
Framework (`Microsoft.AspNetCore.App`), das dieses Paket bereits bündelt —
daher ist die Methode dort ohne Zutun verfügbar. Das Worker-Projekt nutzt
hingegen nur das Worker-SDK (`Microsoft.NET.Sdk.Worker`) und referenziert
dieses Framework **nicht**; das Paket fehlt also.

**Lösung.** Paket explizit nachziehen:

```powershell
dotnet add AQMS.Worker package Microsoft.Extensions.Http
```

Ein zusätzliches `using` ist nicht nötig: die Worker-Templates haben
`ImplicitUsings` aktiviert, womit `Microsoft.Extensions.DependencyInjection`
(der Namespace der Extension-Methode) bereits global eingebunden ist.

**Lessons Learned.** Das Web-SDK und das Worker-SDK bringen **unterschiedliche**
Pakete von Haus aus mit. Was im Web ohne PackageReference kompiliert, kann im
Worker ein fehlendes Paket sein. Bei „Methode nicht gefunden"-Fehlern im
Worker zuerst prüfen, ob das zugehörige `Microsoft.Extensions.*`-Paket
referenziert ist.

---

## 11. Konfigurationsdateien

### 11.1 `AQMS.Web/appsettings.json` — im Repo (ohne Secrets)

Die Datei wird **mit leerer Connection-String** ins Repo eingecheckt:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**Warum leer?** Der Connection-String enthält je nach Umgebung
sensible Daten (Server-Adresse, evtl. Passwort, DB-Name). Diese gehören
nicht in den Source-Control. Stattdessen werden sie pro Umgebung lokal
gesetzt (siehe §11.2).

### 11.2 Lokale Connection-String — User Secrets (Dev)

Auf dem Entwicklungsrechner wird die Connection-String per
**.NET User Secrets** gesetzt, nicht in `appsettings.Development.json`:

```powershell
cd AQMS.Web
dotnet user-secrets set "ConnectionStrings:DefaultConnection" `
  "Server=(localdb)\mssqllocaldb;Database=AqmsDb;Trusted_Connection=True;MultipleActiveResultSets=true"
```

User Secrets liegen unter
`%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json` im
**Userprofil**, **außerhalb des Repos**. Die zugehörige `UserSecretsId`
steht im `AQMS.Web.csproj` und verlinkt das Projekt mit dem Secret-Store.

**Hinweise:**

- `(localdb)\\mssqllocaldb` ist die Standard-LocalDB-Instanz auf
  Windows-Entwicklungsrechnern.
- `MultipleActiveResultSets=true` (MARS) ist eine SQL-Server-Spezialität,
  die mehrere offene Reader auf der gleichen Connection erlaubt — bei
  EF Core mit Lazy-Loading manchmal nötig.

### 11.3 Production: Umgebungsvariable auf dem VPS

Auf dem VPS wird die Connection-String per **Umgebungsvariable** in der
systemd-Unit gesetzt (siehe §25.5):

```ini
Environment=ConnectionStrings__DefaultConnection=Server=...;Database=AQMS;User Id=...;Password=...
```

Die doppelten Unterstriche `__` ersetzen den Punkt der JSON-Hierarchie —
.NET liest das automatisch in `IConfiguration.GetConnectionString("DefaultConnection")`.

### 11.4 `appsettings.Development.json` und `appsettings.Production.json`

Diese Dateien sind durch `.gitignore` **vom Repo ausgeschlossen** (siehe
`.gitignore`-Zeilen für `appsettings.Development.json`,
`appsettings.Production.json`, `appsettings.Local.json`,
`appsettings.*.local.json`). Sie könnten lokal existieren und
environment-spezifische Konfiguration enthalten — werden für AQMS aber
**nicht als Träger von Connection-Strings oder Passwörtern verwendet**.
Begründung und Workflow siehe §11.7 (OneDrive-Sync-Problematik).

### 11.5 `AQMS.Web/Program.cs`

```csharp
using AQMS.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<AqmsDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<AqmsDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages().WithStaticAssets();

app.Run();
```

**Was hier passiert (Zeile für Zeile):**

| Zeile | Bedeutung |
|---|---|
| `var builder = WebApplication.CreateBuilder(args)` | Standard-ASP.NET-Core-Init |
| `GetConnectionString("DefaultConnection")` | aus appsettings.json |
| `AddDbContext<AqmsDbContext>(...)` | DbContext für DI registrieren |
| `AddDatabaseDeveloperPageExceptionFilter()` | bessere Fehlermeldungen bei Migrations-Problemen in Dev |
| `AddDefaultIdentity<IdentityUser>(...)` | Identity konfigurieren mit Bestätigungs-E-Mail-Anforderung |
| `AddEntityFrameworkStores<AqmsDbContext>` | Identity nutzt unseren DbContext |
| `AddControllersWithViews()` | MVC aktivieren |
| `UseMigrationsEndPoint()` | Dev-Endpoint zum Anwenden von Migrations |
| `UseExceptionHandler/UseHsts` | Production-Sicherheit |
| `MapRazorPages()` | für Identity-UI (Login/Register sind Razor Pages) |

**Was noch fehlt** (geplant für Phase 3):

- API-Key Middleware
- API-Controller `/api/measurements`, `/api/commands/...`
- Repository-Layer
- Custom Error-Handling Middleware

### 11.6 `AQMS.Worker/Program.cs` (HttpClient registriert, 2026-06-07)

```csharp
using AQMS.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

// vorkonfigurierten Client registrieren, der im Betrieb immer dieselbe
// Basis-URL und denselben API-Key-Header mitsendet
builder.Services.AddHttpClient("aqms-api", client =>
{
    var apiKey = builder.Configuration["ApiKey"];

    client.BaseAddress = new Uri(builder.Configuration["AqmsApi:BaseUrl"]!);
    client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
});

var host = builder.Build();
host.Run();
```

Der named `HttpClient` `"aqms-api"` ist die Verbindung des Workers zur
Backend-API. `BaseAddress` und der `X-API-Key`-Header (erwartet von der
API-Key-Middleware, §23L) werden einmalig bei der Registrierung gesetzt.
Begründung der HttpClient-Factory statt `new HttpClient()` siehe §10.2.

**Worker-Konfigurationsschlüssel.** Die nicht-geheimen Werte stehen in der
`AQMS.Worker/appsettings.json`, das Secret in den User Secrets des
Worker-Projekts:

```json
{
  "AqmsApi": {
    "BaseUrl": "https://localhost:7144"
  },
  "Worker": {
    "DeviceIdentifier": "raspberry-pi",
    "PollIntervalSeconds": 10
  }
}
```

```powershell
dotnet user-secrets init --project AQMS.Worker
dotnet user-secrets set "ApiKey" "<DEIN_KEY>" --project AQMS.Worker
```

| Schlüssel | Ablage | Inhalt |
|---|---|---|
| `AqmsApi:BaseUrl` | appsettings.json (pro Umgebung) | API-Basis-URL; im Dev lokale Web, auf dem Pi die Produktions-URL |
| `Worker:DeviceIdentifier` | appsettings.json | Natural Key des Pi (`raspberry-pi`), Query-Parameter beim Polling |
| `Worker:PollIntervalSeconds` | appsettings.json | Polling-Intervall in Sekunden |
| `ApiKey` | User Secrets (Dev) / systemd-Override (Pi) | gemeinsames Secret mit der API; **flach**, nicht verschachtelt |

**Warum `ApiKey` flach (nicht `AqmsApi:ApiKey`).** Zwei Gründe:

1. *Konsistenz.* Die API-Key-Middleware liest den Schlüssel als
   `config["ApiKey"]` — flach (§23L). §11.7 / §7 (Projektstand) halten fest,
   dass Worker und API **denselben Wert** teilen. Gleicher Name an beiden
   Enden macht diese Beziehung im Config sichtbar.
2. *systemd-Stolperfalle.* `ApiKey` ist der einzige Worker-Wert, der auf dem
   Pi später per systemd-Override gesetzt wird. Ein verschachtelter Schlüssel
   müsste dort als `AqmsApi__ApiKey` mit **doppeltem Unterstrich** geschrieben
   werden — genau die Falle, die in §23L.8 schon einmal den stillen 401
   verursacht hat. Flach bleibt es `Environment="ApiKey=<wert>"`. Die übrigen
   Worker-Werte gehen nie durch einen Override (sie liegen pro Gerät in der
   appsettings.json), daher dürfen sie gruppiert bleiben.

**Polling-Loop in `Worker.cs` (2026-06-28).** `Worker.cs` enthält nun den
produktiven Polling-Loop. In `ExecuteAsync` werden `DeviceIdentifier` und
`PollIntervalSeconds` aus der Konfiguration gelesen; danach pollt eine
`while (!stoppingToken.IsCancellationRequested)`-Schleife über den named
`HttpClient` `"aqms-api"` den Endpunkt `GET /api/commands/pending`. Zwei
bewusste Resilienz-Entscheidungen, für ein 24/7 laufendes Edge-Device
unverzichtbar:

1. **`try/catch` nur um den Request, Schleife läuft weiter.** Ein transienter
   Netzwerkfehler (VPS-Neustart, DNS-Hänger, WLAN-Aussetzer am Pi) wirft
   `HttpRequestException`. Unbehandelt würde das den **gesamten Host stoppen** —
   seit .NET 6 ist `BackgroundServiceExceptionBehavior.StopHost` der Default für
   unbehandelte Exceptions in `ExecuteAsync`. Auf dem Pi hieße das: der erste
   Aussetzer beendet den Dienst, mit systemd `Restart=always` entstünde eine
   Crash-Restart-Schleife. Gefangen wird **gezielt nur `HttpRequestException`** —
   die `TaskCanceledException`, die `GetAsync`/`Task.Delay` beim sauberen
   Herunterfahren werfen, muss durchlaufen (normaler Stopp-Pfad, kein Fehler);
   ein breites `catch (Exception)` würde bei jedem Shutdown eine Fehlerzeile
   loggen. Das `Task.Delay` steht **außerhalb** des try-Blocks, damit die
   Wartezeit auch im Fehlerfall greift und bei Dauerausfall kein Tight-Loop die
   API flutet.
2. **Interval-Guard.** `configuration.GetValue<int>("Worker:PollIntervalSeconds")`
   liefert bei fehlendem/vertipptem Schlüssel still `0`; `Task.Delay(0)` wäre ein
   Tight-Loop. Ein `if (intervalSeconds <= 0)` fällt auf einen Standardwert
   zurück und meldet das einmalig per `LogWarning`.

**Verifiziert am 2026-06-28 (lokal + VPS).** *Resilienz lokal:* bei gestoppter
`AQMS.Web` loggt der Worker den Fehler und **pollt weiter**, der Host bleibt am
Leben; nach Neustart liefert der nächste Poll wieder `Status: OK`. *VPS:*
`GET /api/commands/pending` über `https://aqms.aqms.example.com` mit VPS-API-Key
liefert `200`, der Worker loggt wiederholt `Status: OK` — volle Kette Worker →
Nginx → Kestrel → ApiKeyMiddleware → CommandsController → CommandService → SQL
Server produktiv bewiesen (§32.3 Update 2026-06-28).

**Resilienz-Punkte (Stand 2026-07-02):** (a) **Gelöst (§11.9):** Timeout-vs-Shutdown — der
`aqms-api`-Client hat einen eigenen 10-s-Timeout, und Poll wie Result-Meldung fangen die
Timeout-`TaskCanceledException` gezielt per `when (!stoppingToken.IsCancellationRequested)` ab
(Shutdown läuft durch). Noch offen: (b) echte Backoff-Retry (Polly, §33.1) ergänzt das
manuelle Fangen später, ersetzt es nicht; (c) der fehlgeschlagene Request wird dreifach
geloggt (zwei `IHttpClientFactory`-Info-Logs + eigenes `LogError`); Abhilfe
`System.Net.Http.HttpClient` auf `Warning`.

Der Empfangs- und Parse-Pfad wurde später erweitert (Deserialisierung, geräte-
übergreifender Poll mit `DeviceIdentifier` + `IPAddress`) — siehe §23N.8. Die
DS18B20-Auslesung und die Shelly-Steuerung folgen als nächste Worker-Einheiten
in Phase 2.

### 11.7 Mehrgerät-Realität und bewusste Wahl gegen `appsettings.Development.json`

In der Praxis wird das Projekt auf **drei Umgebungen** entwickelt und
betrieben — mit jeweils unterschiedlicher Datenbank:

| Umgebung | Datenbank | Connection-String-Quelle |
|---|---|---|
| Firmen-Gerät (Entwicklung) | SQL Server 2022 in Docker (lokal) | User Secrets |
| Privat-Gerät (Entwicklung) | SQL Server LocalDB (VS-Default) | User Secrets |
| VPS (Produktion) | SQL Server 2022 in Docker (§23F) | systemd-Override (§23H.3) |

Hintergrund Firmen-Gerät: SQL Server (einschließlich LocalDB) ist dort
durch IT-Richtlinien blockiert, daher wird die Datenbank in einem
lokalen Docker-Container betrieben. Auf dem Privat-Gerät ist
LocalDB als Visual-Studio-Default verfügbar und genügt.

**Bewusste Entscheidung gegen `appsettings.Development.json` als
Secret-Träger.** Obwohl `appsettings.Development.json` in §11.4 als
durch `.gitignore` geschützt beschrieben ist und dort *technisch*
Connection-Strings stehen könnten, wird sie für AQMS **nicht** als
Secret-Träger genutzt. Der Grund: Der Projektordner wird über OneDrive
zwischen Firmen-Gerät und Privat-Gerät synchronisiert. Eine
`appsettings.Development.json` mit Connection-String würde mitsynchronisiert
— mit zwei Konsequenzen:

1. Geräte-spezifische Connection-Strings würden sich gegenseitig
   überschreiben (Firmen-String würde auf Privat-Gerät landen und
   umgekehrt) — der Grund, warum man überhaupt pro Gerät unterschiedliche
   Strings braucht, wäre annulliert.
2. Das Secret läge — wenn auch git-ignoriert — als Klartextdatei im
   gesyncten Ordner, also auch in der OneDrive-Cloud. Das widerspricht dem
   Grundprinzip von §11 („Secrets liegen nicht im Projektordner").

Die User-Secrets-Variante hingegen legt Werte unter
`%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json` ab —
**außerhalb** des OneDrive-Ordners, **außerhalb** der Solution.
Jedes Gerät hat damit seinen eigenen, unabhängigen Secret-Speicher, der
beim Sync ignoriert wird.

**Lessons Learned aus dem Cleanup vom 2026-05-22:** In einer frühen
Projektphase war auf dem Firmen-Gerät der lokale Docker-Connection-String
versehentlich in der committeten `appsettings.json` gelandet (statt in
User Secrets). Die Datei wurde aber niemals mit Secret gepusht — der
GitHub-Stand blieb stets der saubere `""`-String. Die Aufräumaktion:
`ConnectionStrings`-Block vollständig aus `appsettings.json` entfernen,
auf beiden Entwicklungsgeräten alle drei User Secrets setzen
(`ConnectionStrings:DefaultConnection`, `AdminBenutzer:Email`,
`AdminBenutzer:Passwort`), `appsettings.Development.json` löschen. Daraus
die Workflow-Regel:

> **Vor jedem Commit kurz `git diff --cached` prüfen.** Taucht ein
> Connection-String oder Passwort darin auf, sofort
> `git restore --staged <datei>` und die Quelle säubern. Längerfristig
> ein pre-commit-Hook, der `Password=`, `Server=` und ähnliche Muster in
> `*.json`-Dateien blockiert.

**Workflow „App auf neuem Gerät einrichten" (verbindlich):**

```powershell
# 1. Solution aus Git oder OneDrive holen.
# 2. User Secrets dieser Maschine setzen — Beispiel Privat-Gerät:
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=aqms;Trusted_Connection=True;MultipleActiveResultSets=true" --project AQMS.Web
dotnet user-secrets set "AdminBenutzer:Email" "<email>" --project AQMS.Web
dotnet user-secrets set "AdminBenutzer:Passwort" "<starkes-passwort>" --project AQMS.Web
dotnet user-secrets set "ApiKey" "<zufaelliger-key-32+ zeichen>" --project AQMS.Web

# 3. Datenbank-Schema und HasData-Seeds anlegen (in der PMC):
Update-Database

# 4. App starten — erst hier läuft der IdentitySeeder (§23K.3), legt
#    Rollen und Admin-User an. Migration und Laufzeit-Seeding sind
#    getrennte Schritte:
#    - Update-Database führt Migrations + HasData aus
#    - Der IdentitySeeder läuft erst im Program.cs beim App-Start
```

Hinweis zum `ApiKey`: pro Umgebung **ein anderer Wert**
(Firmen-Gerät, Privat-Gerät, VPS+Pi). Sicherheits-Isolation — leakt ein
Schlüssel, ist nicht gleich alles betroffen. Erzeugung lokal z.B. mit
`[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Maximum 256 }))`.
Details zur Middleware, die den Schlüssel prüft: §23L.

Die Trennung in Schritt 3 und 4 ist nicht optional und nicht
zusammenführbar: Passwort-Hashing geht nicht über `HasData`, daher ist
der Identity-Seed zwingend laufzeitgebunden (siehe Begründung in §23K.3).

---

### 11.8 Dispatch: Ausführung der Befehle (Shelly-Steuerung, 2026-07-02)

Nach dem Empfang (§23N.8) folgt die Ausführung: im `foreach` über die empfangenen
Befehle wird der Ziel-Shelly per HTTP geschaltet. Ablauf pro Befehl: IP-Guard
(`string.IsNullOrWhiteSpace(command.IPAddress)` → überspringen) → `switch` auf
`command.Action` (`"On"` → `"on"`, `"Off"` → `"off"`, sonst LogWarning + `continue`)
→ Shelly-URL `http://{IPAddress}/relay/0?turn={verb}` → HTTP-GET → Erfolg/Fehler loggen.

**Vier bewusste Entscheidungen:**

1. **Klassische Shelly-API statt RPC.** `GET /relay/0?turn=on|off` (§7.4) — kompatibel
   über Gen1 **und** Gen2+, eine URL-Form für die (evtl. gemischte) Flotte, in §7.5
   bereits per curl gegen alle 5 Geräte bewiesen.
2. **Separater Default-HttpClient, NICHT `"aqms-api"`.** Der named Client trägt
   BaseAddress = API + `X-API-Key`. Gegen einen Shelly wäre beides falsch: die absolute
   URL kollidierte mit der BaseAddress, und der API-Key würde an ein Fremdgerät geleakt.
   `CreateClient()` (unkonfiguriert) mit absoluter URL trennt das sauber.
3. **Zweischichtige Fehlerbehandlung.** Der äußere Schleifen-Catch wurde von
   `HttpRequestException` auf `catch (Exception ex) when (ex is not OperationCanceledException)`
   verbreitert (fängt jetzt auch `JsonException` aus dem Parsen; Shutdown-Cancellation
   läuft durch). Zusätzlich ein **innerer** `try/catch` pro Befehl: ein einzelner nicht
   erreichbarer Shelly darf die restlichen Befehle der Batch nicht ausfallen lassen
   (`HttpRequestException` → loggen + weiter, kein `throw`).
4. **Eigener Shelly-Timeout (3 s) + Timeout-vs-Shutdown-Unterscheidung.** Der
   HttpClient-Default von 100 s würde bei einem hängenden Shelly den ganzen Loop
   blockieren. `shellyClient.Timeout = TimeSpan.FromSeconds(3)`. Ein Timeout wirft
   `TaskCanceledException` (eine `OperationCanceledException`) ohne gesetztes
   `stoppingToken` — gefangen mit `catch (TaskCanceledException) when (!stoppingToken.IsCancellationRequested)`:
   kommt die Cancellation vom Shutdown, ist der Filter false und sie läuft durch.

**Verifikation (2026-07-02, echte Hardware).** End-to-end sowohl vom Dev-Rechner als
auch **vom Pi aus**: Poll gegen den VPS (`Status: OK`) → `Befehl empfangen: 2 On` für
`shelly-filter` mit IP → `GET http://10.0.0.227/relay/0?turn=on` → `200` →
`Befehl 2 ausgeführt: shelly-filter -> on`. Der reale Shelly schaltete. Die gesamte
Steuerungskette (VPS → Poll → Parse → Dispatch → LAN-Shelly) ist damit produktiv bewiesen.

**Anschluss:** Das Result-Reporting, das den Befehlszyklus schließt (der Befehl bleibt sonst
`Pending` und feuert bei jedem Poll erneut), folgte in §11.9 (verifiziert 2026-07-02). Bewusst
noch offen: Named `"shelly"`-Client in `Program.cs` (statt inline-Timeout) als zentralisierte
Variante — verschoben.

### 11.9 Result-Reporting: Befehlszyklus schließen (2026-07-02)

Nach dem Dispatch (§11.8) meldet der Worker das Ergebnis jedes Befehls an
`POST /api/commands/result` zurück — sonst bliebe der Befehl `Pending` und würde bei jedem
Poll erneut ausgeführt (der im Pi-Log sichtbare Dauer-Feuer-Defekt). Das Backend
(`ProcessCommandResult`, §23N) setzt den Befehl bei `Success=true` auf `Executed`, sonst auf
`Failed`.

**Umsetzung.** Das Melden wurde in eine lokale Funktion `ReportResultAsync(commandId, success,
report)` am Ende von `ExecuteAsync` ausgelagert (in C# „gehoisted", aus der Schleife
aufrufbar). Sie baut die worker-lokale `CommandResultDto` (`CommandId`/`Success`/
`ResultMessage`, Namen identisch zur Web-DTO) und sendet sie per `PostAsJsonAsync` über den
**`aqms-api`-Client** (nicht den Shelly-Client — die Meldung geht an die eigene API mit
BaseAddress + `X-API-Key`). Aufgerufen an drei Stellen: nach dem Dispatch (Erfolg *und*
Fehler) sowie in den zwei Skip-Fällen (kein IP / unbekannte Action), die jetzt als
`Success=false` → `Failed` gemeldet werden, statt still `Pending` zu bleiben.

**Zwei Resilienz-Punkte dabei geschlossen** (vorher offen, §11.6):

1. **Eigener Timeout für den `aqms-api`-Client.** Poll- und Result-Client bekommen
   `Timeout = 10 s` (Default wäre 100 s) — ein hängender VPS blockiert den Loop nicht mehr
   minutenlang.
2. **Timeout-vs-Shutdown-Unterscheidung auf den API-Pfaden.** Poll *und* Result fangen die
   Timeout-`TaskCanceledException` gezielt per
   `catch (TaskCanceledException) when (!stoppingToken.IsCancellationRequested)` ab (loggen +
   weiter). Der echte Shutdown (`stoppingToken` gesetzt) matcht den Filter nicht → läuft durch
   → sauberer Host-Stopp. Reihenfolge: der spezifische Timeout-Catch steht **vor** dem breiten
   `catch (Exception ex) when (ex is not OperationCanceledException)`, weil letzterer die
   `TaskCanceledException` (erbt von `OperationCanceledException`) sonst nicht fängt.

**Semantik: at-least-once.** Schlägt der Result-POST selbst fehl (VPS weg oder Timeout), wird
**nicht** geworfen — der Befehl bleibt `Pending` und wird beim nächsten Poll erneut
ausgeführt + gemeldet. Ein `409 Conflict` (schon verarbeitet) wird als `LogInformation`
behandelt, nicht als Fehler — er beweist, dass der Befehl sauber durch ist. Der Shelly-Call
(on/off) ist idempotent, das erneute Ausführen also unkritisch.

**Verifikation (2026-07-02).** Lokal end-to-end: Befehl 3 (shelly-filter, mit IP) →
`ausgeführt` → `gemeldet (Success=True)` → DB-Status `Executed`; Befehle 4/5 (ohne IP) →
`übersprungen` → `gemeldet (Success=False)` → `Failed`; der Folge-Poll zeigt `Status: OK`
**ohne** „Befehl empfangen" (Zyklus geschlossen, kein Dauer-Feuern mehr). Anschließend auf den
Pi re-deployt und dort gegen den VPS verifiziert (`Hosting environment: Production`, Poll
gegen `aqms.aqms.example.com`).

### 11.10 Sensor-Pfad: DS18B20 → /api/measurements (2026-07-07)

Die zweite Hälfte des Worker-Zwecks (Monitoring): Wassertemperatur vom DS18B20 lesen und an
die API posten. Damit ist der Pi nicht mehr nur Aktor (Steuerung), sondern auch Sensor.

**`Ds18b20Reader` (eigene Klasse).** Liest über sysfs (§5.4–§5.7). `ReadTemperatureAsync`
sucht den Sensor per `Directory.GetDirectories("/sys/bus/w1/devices", "28-*")` (Glob statt
hartcodierter ROM-ID → überlebt Sensortausch), liest `w1_slave` mit `File.ReadAllTextAsync`
(blockiert ~750 ms wegen Sensor-Konversion, daher async) und übergibt an `ParseTemperature`.
Die Parse-Logik ist bewusst **`static`** und nimmt nur den String — dadurch **ohne Hardware
am Dev-Rechner unit-testbar** (der xUnit-Kandidat, §33.1). Sie prüft die CRC-Zeile (Zeile 1
muss mit `YES` enden — sonst `null`, „lieber kein Wert als ein falscher"), extrahiert den
`t=`-Wert und teilt durch `1000.0` (Milligrad → °C). Registriert als `AddSingleton`
(zustandslos, wird vom Singleton-Worker konsumiert).

**`ReportMeasurementAsync` (lokale Funktion, neben `ReportResultAsync`).** Liest die
Temperatur; bei `null` (kein Sensor / CRC NO) wird **nichts** gepostet. Sonst baut sie die
worker-lokale `CreateMeasurementDto` (`DeviceIdentifier` = `"raspberry-pi"` — hier wird der
bis dahin ungenutzte `Worker:DeviceIdentifier` gebraucht; `MeasurementTypeName` = exakt
`"Temperature"`, der Seed-Name, sonst 400; `Value`; `Timestamp` = `DateTime.UtcNow`) und
postet per `PostAsJsonAsync` an `/api/measurements` über den `aqms-api`-Client (eigener
10-s-Timeout), mit drei `catch` wie beim Result-Reporting.

**Bewusste Abweichung: kein Retry bei Mess-Fehler.** Anders als das Result-Reporting
(at-least-once, §11.9) wird ein fehlgeschlagener Mess-POST **nicht** wiederholt. Begründung:
Ein verlorener Messwert ist unkritisch — die nächste Kadenz liest einen frischen. Einen alten
Temperaturwert nachzuliefern wäre sogar schädlich (veraltete Daten in einer Zeitreihe).
Loggen, weitermachen.

**Sensor-Health-Eskalation (Option A).** Ein Zähler `continousSensorErrors` (im
`ExecuteAsync`-Scope, überlebt die Mess-Zyklen) zählt aufeinanderfolgende Fehl-Reads; ab
`Worker:MaxContinousSensorErrors` (Default 10) wird von `LogWarning` auf `LogError`
eskaliert, Reset bei jedem gültigen Read. Grund: Ein *einzelner* verworfener Read ist
Grundrauschen; ein *Dauerausfall* (Sensor abgezogen, Kabelbruch) würde sonst unbemerkt als
Stille durchlaufen — für ein Monitoring-System der gefährlichste Zustand („meldet nichts"
ununterscheidbar von „alles ok"). Eskalationszeit = Schwelle × Mess-Intervall (Default
10 × 20 s ≈ 3,3 min). Die **vollständige** Lösung (Ausfall als Zustand in DB/Dashboard
sichtbar) ist bewusst als Ausblick eingeordnet (Option B, Phase 4) — Problem erkannt,
schlanke Lösung gebaut, vollständige später.

**Kadenz.** `Worker:MeasurementIntervalSeconds` (Default 60, im Betrieb 20) entkoppelt die
Messung vom 5-s-Command-Poll (sonst DB-Flut). Umsetzung: ein **Zeitvergleich** im bestehenden
Loop (`DateTime.UtcNow - lastMeasurement >= …`) statt eines zweiten Timers/Threads — weniger
bewegliche Teile. `lastMeasurement = DateTime.MinValue` initial → erste Messung sofort beim
Start; danach nur bei tatsächlichem Aufruf aktualisiert.

**Architektur-Trade-off (Ein-Loop, bewusst).** Sensor-Read (~750 ms) und Mess-POST laufen im
selben Loop wie der Command-Poll. Zwei bewusst in Kauf genommene Konsequenzen: (1) Während
einer Messung pollt der Worker kurz keine Befehle — ein Schaltbefehl kann sich um bis zu
~10 s verzögern (unkritisch für Aquarium-Aktoren). (2) Die effektive Mess-Kadenz ist an das
Poll-Raster gequantelt — sie trifft nur Vielfache des Poll-Intervalls (bei 20 s = 4 × 5 s
geht das glatt auf). Ein zweiter Loop/Thread würde beides entkoppeln, brächte aber
Synchronisations-Komplexität; für einen Ein-Zweck-Edge-Dienst ist der eine Loop die
einfachere, robustere Wahl.

**Verifikation (2026-07-07, am Pi gegen VPS).** `POST /api/measurements` → **201 Created**,
Log `Messung gesendet: 25.125 °C`; die `Measurements`-Tabelle füllt sich mit plausiblen
Werten im Mess-Intervall. Damit ist **Phase 2 funktional abgeschlossen**: der Pi macht
Steuerung **und** Monitoring.

**Bekannte offene Punkte (Politur, nicht funktional blockierend):** `double`-Speicherung
zeigt Fließkomma-Artefakte (`25.062000…1`) — `decimal` wäre exakt, für Aquarium-Temperaturen
irrelevant; dreifaches Logging und der Config-Key-Tippfehler `MaxContinousSensorErrors`
(→ `Continuous`) bleiben offen (§33.1). Polly-Retry wurde bewusst zurückgestellt (§30.16).

> **Erledigt seit 2026-07-12:** `ParseTemperature` ist mit xUnit getestet (§23Q.2); die
> BaseUrl-Entscheidung ist getroffen — die Overlay-Datei bleibt (§23O.8).

## 12. Git/GitHub-Setup

### 12.1 Repository

Das Projekt wird in einem **GitHub-Repository** versioniert. Initialer
Setup-Workflow:

```powershell
git init
dotnet new gitignore        # Standard-.NET-.gitignore
git add .
git commit -m "feat: initial solution structure"

# Verbinden mit Remote (Repository muss existieren)
git remote add origin https://github.com/KevinObe/AQMS-2026.git
git push -u origin main
```

### 12.2 Was versioniert wird

| Versioniert | Ignoriert (`.gitignore`) |
|---|---|
| `.slnx` | `bin/` |
| `*.csproj` | `obj/` |
| `Program.cs`, `*.cs` | `.vs/` |
| `Models/`, `Data/`, `Migrations/` | `*.user`, `*.suo` |
| `appsettings.json` (ohne Secrets!) | `appsettings.Development.json` (lokal) — diskutierbar |
| `Views/`, `wwwroot/lib/...` | User Secrets |
| `docs/*.md` | DB-Files (`.mdf`, `.ldf`) |

### 12.3 Bisherige Commits (Stand 09.05.2026)

```
31483eb  Add MeasurementTypes model, migration, and project documentation
2b34666  Fixed - old test code removed from project
2b4051c  Added Solution Structure and Basic VS setup.
e7d8169  Initial Commit
```

Aktuell sind die Solution-Restrukturierung (AQMS_Web → AQMS.Web etc.) und
die neue `AddDomainEntities`-Migration **noch nicht committet**. Nach
diesem Doku-Update folgt ein größerer Commit, der diesen Refactor und
die finale Persistenzschicht zusammenfasst.

---

# Teil D — Persistenzschicht (Datenbank)

## 13. Anforderungen an die Persistenzschicht

### 13.1 Fachliche Anforderungen

| Anforderung | DB-Konsequenz |
|---|---|
| Kontinuierliche Sensorwerte erfassen | Time-Series-Tabelle `Measurements` mit Indizes |
| Mehrere Sensortypen flexibel handhaben | Lookup-Tabelle `MeasurementTypes` |
| Geräte unterschiedlicher Klassen | Lookup-Tabelle `DeviceTypes` |
| Schaltbefehle persistieren (Pi pollt) | `DeviceCommands` mit Status-Workflow |
| Schaltzustände als Audit-Trail | eigene Tabelle `StateChanges` |
| Mehrbenutzerfähigkeit, Login | `IdentityDbContext` mit AspNet*-Tabellen |

### 13.2 Technische Anforderungen

| Anforderung | Lösung |
|---|---|
| Time-Series (viele Inserts) | `long` als PK, Indizes auf Timestamp |
| Schnelles Pi-Polling alle 10 s | Filtered Index `WHERE [Status] = 'Pending'` |
| Datensicherheit gegen versehentliches Löschen | `OnDelete.Restrict` durchgängig |
| DSGVO-Konformität bei User-Löschung | `OnDelete.SetNull` bei User-FKs |
| Reproduzierbares Setup für Demo | Seed-Daten via `HasData` |
| Schema-Versionierung im Repo | EF Core Migrations |
| Produktion: SQL Server | EF Provider `Microsoft.EntityFrameworkCore.SqlServer` |
| Entwicklung ohne Server-Install | LocalDB (`(localdb)\\mssqllocaldb`) |

---

## 14. Schema-Iteration: vom ersten Entwurf zum aktuellen Stand

Diese Sektion ist wichtig für das Verständnis: das Schema ist **nicht in
einem Wurf** entstanden, sondern in mehreren bewussten Iterationen, jede
mit eigener Begründung. Die Diplomarbeit kann diese Evolution als
Lernprozess dokumentieren.

### 14.1 Iteration 1 (KW 13): das initiale Modell

Erste Tabellen, sehr einfach:

```
Devices (Id, Name, ShellyIp, IsOnline, LastSeen)
Measurements (Id, Timestamp, TemperatureC, DeviceId)
DeviceCommands (Id, DeviceId, Command, CreatedAt, ExecutedAt, Status)
```

**Probleme dieses Modells:**

- `TemperatureC` als Spaltenname **bettet die Einheit in den Spaltennamen**.
  Kommt ein zweiter Sensor (Power, pH), bräuchte man eine neue Spalte oder
  eine zweite Tabelle. **Verstoß gegen 3NF**.
- `IsOnline` als boolean **ist ableitbar aus `LastSeen`** — wenn die letzte
  Kontaktzeit weniger als 30 Sekunden zurückliegt, ist das Gerät online.
  Das Speichern dieser Information ist redundant. **Verstoß gegen 3NF**.
- Kein `DeviceType` → keine Möglichkeit Geräte sinnvoll zu kategorisieren.
- `Command` und `Status` waren freie Strings — keine
  Wertebereichsintegrität.

### 14.2 Iteration 2 (KW 13): erste Migration mit den drei Tabellen

Migration `20260324203752_initialMigration` wurde generiert und auf LocalDB
angewendet. Schema ist physisch da, aber konzeptuell noch nicht 3NF-konform.

### 14.3 Iteration 3 (KW 13): MeasurementTypes-Auslagerung (3NF-Schritt)

Die Spalte `TemperatureC` wurde durch `Value` ersetzt + neue Lookup-Tabelle
`MeasurementTypes`. Migration `20260327091358_added-measurementtypes`.

```
MeasurementTypes (Id, Name, Unit)              ← neu
Measurements (Id, Timestamp, Value,            ← TemperatureC raus
              DeviceId, MeasurementTypeId)     ← FK auf neuen Lookup
```

Neue API-Form:

```json
POST /api/measurements
{
  "deviceId": 1,
  "measurementTypeId": 1,
  "value": 24.3,
  "timestamp": "2026-04-22T10:00:00Z"
}
```

statt vorher `{ "deviceId": "raspi-aquarium", "temperatureC": 24.3, ... }`.

### 14.4 Iteration 4 (KW 14, geplant): Volldokumentation und CHECK-Constraints

Während der ausführlichen DB-Doku ([docs/db_schema/AQMS_Datenbankdokumentation.md](db_schema/AQMS_Datenbankdokumentation.md))
wurde geplant:

- `MeasurementType` mit `MinValue`, `MaxValue` für Plausibilitätsgrenzen
- 5 fixe DeviceTypes nach Funktion (Filter, Licht, CO2-Anlage, Heizstab,
  Surface Skimmer)
- CHECK-Constraints für `Command IN ('on','off')` und `Status IN ('pending', 'executed', 'failed')`
- `DeviceTypes`-Lookup statt freier String

Dieser Plan wurde **teilweise umgesetzt**, aber bei der Konkretisierung
der finalen Implementation (Iteration 5) noch einmal verfeinert.

### 14.5 Iteration 5 (KW 17–18): die finale Modellierung

Das aktuell implementierte Schema (siehe §17). Die wichtigsten
Verfeinerungen gegenüber Iteration 4:

| Änderung | Begründung |
|---|---|
| DeviceTypes nach **Hardware-Klasse** (Sensor, SmartPlug) statt Funktion (Filter, Licht...) | Funktion gehört in `Device.Name`, nicht in den Typ. Pi ist ein "Sensor", die 5 Shellys sind "SmartPlug". |
| `ShellyIp` → `IPAddress` | Der Pi ist auch ein Device — `ShellyIp` wäre semantisch falsch. Generischer Name erlaubt zukünftige Geräte ohne Schemaänderung. |
| `Command` → `Action` als typed Enum (DeviceState) | Property `Command` in Klasse `DeviceCommand` wäre Wortwiederholung. Enum ersetzt Strings + CHECK-Constraint. |
| `MeasurementType.MinValue/MaxValue` weggelassen | Bewusst minimal — Plausibilitätsgrenzen können später ergänzt werden, gehören aber eher in die Validierungsschicht. |
| Neue Tabelle `StateChanges` | Trennung "stetige Werte" (Measurements) von "diskreten Schaltereignissen" (StateChanges). Erst überlegt als MeasurementType "State", verworfen. |
| `RequestedByUserId` (FK auf AspNetUsers) bei DeviceCommand | Audit-Trail wer hat geschaltet. Optional (System-Befehle können null sein). |
| `ChangedByUserId` (FK) bei StateChange | analog für Schaltereignisse |
| `Pi` als Device modellieren | Pi liefert Messungen und hat IP/LastSeen wie ein Shelly — gehört in dieselbe Tabelle |
| PK-Typ: `int` für Lookup, `long` für Time-Series | Time-Series-Tabellen sammeln über Jahre viele Zeilen; `long` ist Standard |
| Enums als String via `HasConversion<string>` | Lesbarkeit in DB-Tools, JSON-APIs, Filtered Index möglich |

**Finale Migration:** `20260509115558_AddDomainEntities` (auf LocalDB
angewendet, alle Seeds drin).

### 14.6 Was alle alten Migrationen ablösen (Refactor)

Beim Wechsel zur neuen Solution-Struktur (`AQMS_Web` → `AQMS.Web`) wurden
die alten Migrations gelöscht. Die neue Initial-Migration (für die
Identity-Tabellen) heißt `00000000000000_CreateIdentitySchema` (kommt aus
dem Identity-UI-Template), und die Domain-Tabellen kommen via
`20260509115558_AddDomainEntities`. Damit gibt es nur **zwei Migrationen**
in der neuen Struktur — sauber und reproduzierbar.

---

## 15. Normalformen und 3NF/BCNF

### 15.1 Theoretische Grundlagen

Relationale Datenbanken werden in Normalformen klassifiziert. Höhere
Normalform = weniger Redundanz, weniger Anomalien. AQMS erfüllt 1NF, 2NF,
3NF und darüber hinaus Boyce-Codd (BCNF).

### 15.2 1NF — Atomarität

Eine Tabelle ist in 1NF, wenn jedes Attribut atomar ist (keine
Listen/Tupel) und jede Zeile eindeutig identifizierbar ist.

**Verstoß-Beispiel:**

| Id | Geräte |
|----|--------|
| 1  | Filter, Licht, Heizstab |

Die Zelle `Geräte` enthält mehrere Werte → 1NF verletzt.

**Im AQMS:** alle Tabellen haben atomare Werte und einen Primärschlüssel.
1NF erfüllt.

### 15.3 2NF — vollständige Schlüsselabhängigkeit

Erst relevant bei zusammengesetzten Primärschlüsseln. Im AQMS hat jede
Tabelle einen einfachen `Id`-PK → 2NF strukturbedingt erfüllt.

### 15.4 3NF — keine transitive Abhängigkeit

Kein Nicht-Schlüssel-Attribut darf von einem anderen Nicht-Schlüssel-Attribut
abhängen.

**Verstoß-Beispiel 1: `TemperatureC` als Spaltenname**

In Iteration 1 hieß die Spalte `Measurements.TemperatureC`. Die Information
"ist eine Temperatur in °C" war im **Spaltennamen** kodiert. Sobald ein
zweiter Sensortyp dazukommt, müsste das Schema geändert werden. Das ist
eine transitive Abhängigkeit zwischen Typ und Spaltenname.

**Lösung:** Spalte heißt jetzt `Value`, Typ ist über
`MeasurementTypeId` als FK referenziert. Neuer Sensor → neuer Eintrag in
`MeasurementTypes`, kein Schemawechsel.

**Verstoß-Beispiel 2: `IsOnline` als persistierter Boolean**

`IsOnline` ergibt sich direkt aus `LastSeen` (`< 30s zurück = online`).
Das Speichern wäre Redundanz. Lösung: nicht in DB, sondern als
`[NotMapped]`-Property im C#-Model:

```csharp
[NotMapped]
public bool IsOnline => LastSeen.HasValue && LastSeen > DateTime.UtcNow.AddSeconds(-30);
```

Im Razor View und C#-Code transparent verwendbar (`device.IsOnline`),
aber nicht in der DB.

**Verstoß-Beispiel 3: `DeviceType` als freier String**

In Iteration 1 hätte `Devices` ein freies String-Feld `DeviceType`. Problem:
`"Heater"` und `"heater"` sind DB-seitig unterschiedliche Werte. Keine
referenzielle Integrität. Lösung: `DeviceTypes`-Lookup mit FK
`DeviceTypeId`.

### 15.5 BCNF — verschärfte 3NF

Boyce-Codd: jede funktionale Abhängigkeit geht nur vom Primärschlüssel
aus. Im AQMS: alle Tabellen haben einfache Primärschlüssel, keine
nicht-trivialen funktionalen Abhängigkeiten zwischen
Nicht-Schlüssel-Attributen → BCNF erfüllt.

### 15.6 Ein Sonderfall: `Device.CurrentState` als bewusste Denormalisierung

`Device.CurrentState` ist **theoretisch ableitbar** aus `StateChanges`
(`MAX(Timestamp).State`). Trotzdem persistent. Begründung:

- Live-Dashboard liest oft den aktuellen Zustand
- Aggregation über StateChanges wäre teuer
- **CQRS-Idee**: Lese-Pfad eigener Datenstrom für Performance

In der Diplomarbeit als bewusste Denormalisierung dokumentieren — das ist
kein 3NF-Verstoß "by accident", sondern eine begründete Architekturentscheidung.

---

## 16. Architekturentscheidungen im Datenmodell

### 16.1 PK-Typ: `int` für Lookups, `long` für Time-Series

| Tabelle | PK | Begründung |
|---|---|---|
| DeviceTypes, MeasurementTypes, Devices | `int` | wenige fixe Einträge, kompakt, lesbar |
| Measurements, DeviceCommands, StateChanges | `long` | Time-Series: viele Inserts über Jahre |

`int` reicht zwar theoretisch für Jahrzehnte (~2,1 Mrd Werte), aber `long`
ist Standard für hochfrequente Tabellen und kostet pro Zeile nur 4 Bytes
mehr.

### 16.2 Pi als Device modellieren

Der Pi ist **nicht** außerhalb der DB, sondern selbst ein Device mit
DeviceType "Sensor". Vorteile:

- Saubere Symmetrie: alle Geräte in einer Tabelle
- Pi liefert Messungen → `Measurement.DeviceId` zeigt auf den Pi-Eintrag
- `LastSeen`/`IsOnline` funktionieren für Pi und Shellys gleich

### 16.3 Schaltzustände: drei Konzepte sauber getrennt

| Konzept | Spalte/Tabelle | Bedeutung |
|---|---|---|
| System-Verwaltung | `Device.IsEnabled` | "Ist das Gerät überhaupt im System aktiv?" |
| Aktueller Zustand (Cache) | `Device.CurrentState` | "Ist gerade an oder aus?" — schneller Read |
| Historie | `StateChanges`-Tabelle | "Wann wurde geschaltet?" — vollständige Events |

`CurrentState` ist bewusste Denormalisierung (siehe §15.6).

### 16.4 Surrogate Key + Natural Key bei Devices

```csharp
public int Id { get; set; }                           // Surrogate
public string DeviceIdentifier { get; set; }          // Natural (UNIQUE)
```

| | Surrogate (`Id`) | Natural (`DeviceIdentifier`) |
|---|---|---|
| Vergabe | DB beim INSERT | wir (im Seed/Code) |
| Bedeutung | keine technische | fachlich relevant |
| Speicher | klein (4B int) | größer (string) |
| Stabil bei DB-Reset | nein (neue Ids möglich) | ja |
| Lesbar in URLs/Logs | nein | ja |
| Performance in JOINs | sehr schnell | langsamer |

**Wo welcher verwendet wird:**

- `Id` als PK + FK in Beziehungen (Performance)
- `DeviceIdentifier` in Worker-Konfig (`appsettings.json`), API-URLs
  (`/api/devices/shelly-filter/toggle`), Logs (Diagnose)

Natural Keys sind kebab-case: `raspberry-pi`, `shelly-filter`,
`shelly-light`, `shelly-co2`, `shelly-heater`, `shelly-skimmer`.
Konvention aus DevOps (Kubernetes, Docker, URL-Slugs).

**3NF-Konformität:** beide Spalten sind direkt vom PK abhängig, keine
transitive Abhängigkeit.

### 16.5 Stetige Werte vs. diskrete Events trennen

**Erste Idee:** alles in `Measurements` mit MeasurementType "State"
(Wert 0.0/1.0).

**Verworfen:**

- bool als float speichern ist semantisch unsauber
- Schaltereignisse sind Domain-Events, keine Messungen
- Trennung erlaubt klarere Queries und Reportings

**Resultat:** 2 MeasurementTypes (Temperature, Power) statt 3, plus eigene
`StateChanges`-Tabelle.

### 16.6 Enum-Speicherung als String

Drei Enums im Datenmodell:

| Enum | Verwendung |
|---|---|
| `DeviceState` (Off, On) | `Device.CurrentState`, `StateChange.State`, `DeviceCommand.Action` |
| `CommandStatus` (Pending, Executed, Failed) | `DeviceCommand.Status` |

Speicherung als `nvarchar` mittels `HasConversion<string>()` statt Default
`int`. Begründung:

- Lesbarkeit in DB-Tools (`Status = 'Pending'` statt `Status = 0`)
- Klare JSON-APIs (`"status": "Pending"`)
- Filtered Index funktioniert (`WHERE [Status] = 'Pending'`)
- Selbsterklärend in Backups und Logs

### 16.7 Generische Spalten statt hardware-spezifischer

`IPAddress` statt `ShellyIP` — die Spalte trägt eine Netzwerkadresse,
unabhängig vom Gerätetyp. Ein Tasmota-Plug oder ein zweiter Pi würde
ohne Schemaänderung passen.

### 16.8 OnDelete durchgehend Restrict bei Pflichtbeziehungen

EF Core verwendet als Default `Cascade`. Für AQMS:

- bei Pflichtbeziehungen: `Restrict` (Daten bleiben erhalten)
- bei optionaler User-Beziehung: `SetNull` (DSGVO-konform)

Datensicherheit > Bequemlichkeit. Versehentliches Löschen eines Devices
darf nie monatelange Sensordaten zerstören.

### 16.9 DB-seitige Defaults für Zeitstempel

`Measurement.Timestamp`, `DeviceCommand.CreatedAt`, `StateChange.Timestamp`
haben `HasDefaultValueSql("GETUTCDATE()")`. Damit ist der gespeicherte
Zeitpunkt der **echte Insert-Moment**, unabhängig davon, wie lange ein
Entity-Objekt vorher im C#-Speicher existierte.

Speicherung in **UTC**, Umrechnung in lokale Zeit erst im Frontend.

---

## 17. Entity-Klassen im Detail

Die folgenden sechs fachlichen Entitäten bilden das Datenmodell. Code-Stand
aus [AQMS.Web/Models/](../AQMS.Web/Models/).

### 17.1 DeviceType — Lookup für Gerätetypen

**Aufgabe:** verhindert Tippfehler bei Gerätetyp-Zuordnungen ("Sensor"
statt "Sesnor"), ermöglicht referenzielle Integrität.

```csharp
namespace AQMS.Web.Models;

public class DeviceType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Device> Devices { get; set; } = new List<Device>();
}
```

**Konfiguration:**

| Property | Konfiguration |
|---|---|
| `Name` | `nvarchar(50)`, NOT NULL |

**Seed-Daten:**

| Id | Name |
|---|---|
| 1 | Sensor (für den Pi) |
| 2 | SmartPlug (für die 5 Shellys) |

### 17.2 MeasurementType — Lookup für Messwerttypen

**Aufgabe:** definiert welche Arten von Messwerten erfasst werden, mit
physikalischer Einheit. Erweiterbar ohne Schemaänderung.

```csharp
namespace AQMS.Web.Models;

public class MeasurementType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;

    public ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();
}
```

**Konfiguration:**

| Property | Konfiguration |
|---|---|
| `Name` | `nvarchar(50)`, NOT NULL |
| `Unit` | `nvarchar(10)`, NOT NULL |

**Seed-Daten:**

| Id | Name | Unit |
|---|---|---|
| 1 | Temperature | °C |
| 2 | Power | W |

State entfällt hier bewusst — Schaltereignisse sind in `StateChanges` ausgelagert.

### 17.3 Device — Geräte

**Aufgabe:** zentrale Entity für alle physischen Geräte: Pi (1) und
Shellys (5).

```csharp
using System.ComponentModel.DataAnnotations.Schema;

namespace AQMS.Web.Models;

public class Device
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DeviceIdentifier { get; set; } = string.Empty;
    public string? IPAddress { get; set; }
    public DeviceState? CurrentState { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTime? LastSeen { get; set; }

    public int DeviceTypeId { get; set; }
    public DeviceType DeviceType { get; set; } = null!;

    public ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();
    public ICollection<DeviceCommand> Commands { get; set; } = new List<DeviceCommand>();
    public ICollection<StateChange> StateChanges { get; set; } = new List<StateChange>();

    [NotMapped]
    public bool IsOnline => LastSeen.HasValue && LastSeen > DateTime.UtcNow.AddSeconds(-30);
}

public enum DeviceState
{
    Off,
    On
}
```

**Property-Übersicht:**

| Property | Typ | Pflicht | Rolle |
|---|---|---|---|
| Id | int | PK | Surrogate Key |
| Name | string | ja | Anzeigename |
| DeviceIdentifier | string | ja, UNIQUE | Natural Key (kebab-case) |
| IPAddress | string? | nein | Netzwerkadresse |
| CurrentState | DeviceState? | nein | Cache des Schaltzustands |
| IsEnabled | bool | ja, Default true | System-Aktivierung |
| LastSeen | DateTime? | nein | letztes Lebenszeichen |
| DeviceTypeId | int | ja | FK → DeviceTypes |
| DeviceType | Navigation | — | Parent-Navigation |
| Measurements | ICollection | — | 1:n Children |
| Commands | ICollection | — | 1:n Children |
| StateChanges | ICollection | — | 1:n Children |
| IsOnline | bool | NotMapped | berechnet aus LastSeen |

**Konfiguration:**

| Property | Fluent API |
|---|---|
| `DeviceIdentifier` | IsRequired, MaxLength(100), HasIndex IsUnique |
| `Name` | IsRequired, MaxLength(50) |
| `IPAddress` | MaxLength(45) — IPv6-max |
| `CurrentState` | HasConversion\<string>(), MaxLength(20) |
| Beziehung zu DeviceType | OnDelete.Restrict |

**Seed-Daten (alle 6 Devices):**

| Id | DeviceIdentifier | Name | DeviceTypeId | IPAddress | IsEnabled |
|---|---|---|---|---|---|
| 1 | raspberry-pi | AQMS - RaspberryPi | 1 (Sensor) | 10.0.0.222 | true |
| 2 | shelly-filter | Filter | 2 (SmartPlug) | 10.0.0.227 | true |
| 3 | shelly-light | Light | 2 | 10.0.0.226 | true |
| 4 | shelly-co2 | CO2 | 2 | 10.0.0.225 | true |
| 5 | shelly-heater | Heater | 2 | 10.0.0.224 | true |
| 6 | shelly-skimmer | Skimmer | 2 | 10.0.0.223 | true |

`CurrentState` und `LastSeen` initial null — werden vom Worker gefüllt.

### 17.4 Measurement — Messwerte

**Aufgabe:** speichert kontinuierliche Sensorwerte: Temperatur vom DS18B20
(Pi), Leistung von Shelly-Plugs.

```csharp
namespace AQMS.Web.Models;

public class Measurement
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }

    public int MeasurementTypeId { get; set; }
    public int DeviceId { get; set; }

    public Device Device { get; set; } = null!;
    public MeasurementType MeasurementType { get; set; } = null!;
}
```

**Konfiguration:**

| Punkt | Wert |
|---|---|
| `Timestamp` Default | `HasDefaultValueSql("GETUTCDATE()")` |
| Beziehung zu Device | OnDelete.Restrict |
| Beziehung zu MeasurementType | OnDelete.Restrict |
| Index 1 | Composite auf `(DeviceId, Timestamp)` |
| Index 2 | Single auf `Timestamp` |

**Keine Seed-Daten** — Messdaten kommen ausschließlich vom Worker zur Laufzeit.

### 17.5 DeviceCommand — Schaltbefehle

**Aufgabe:** speichert vom User (oder System) initiierte Schaltbefehle.
Wird vom Pi gepollt und nach Ausführung als "Executed" oder "Failed"
markiert.

```csharp
namespace AQMS.Web.Models;

public class DeviceCommand
{
    public long Id { get; set; }
    public DeviceState Action { get; set; }
    public CommandStatus Status { get; set; } = CommandStatus.Pending;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExecutedAt { get; set; }
    public string? ResultMessage { get; set; }

    public int DeviceId { get; set; }
    public string? RequestedByUserId { get; set; }

    public Device Device { get; set; } = null!;
}

public enum CommandStatus
{
    Pending,
    Executed,
    Failed
}
```

**Konfiguration:**

| Punkt | Wert |
|---|---|
| `Action` | HasConversion\<string>(), MaxLength(10) |
| `Status` | HasConversion\<string>(), MaxLength(20), HasDefaultValue(Pending) |
| `ResultMessage` | MaxLength(200) |
| `RequestedByUserId` | MaxLength(450) — Identity-Default |
| `CreatedAt` | DB-Default `GETUTCDATE()` |
| Beziehung zu Device | OnDelete.Restrict |
| Beziehung zu IdentityUser | OnDelete.SetNull, kein Navigation-Property |
| Index | Filtered: `Status` mit Filter `[Status] = 'Pending'` |

**Property-Naming `Action` statt `Command`:** vermeidet Wortwiederholung
in Klasse `DeviceCommand`. `Type` wäre generisch und kollidiert mit
`System.Type`. `Action` (imperativer Sinn: "schalte ein/aus") gewählt.

**Beziehung zu IdentityUser ohne Navigation-Property:** das
Domain-Model wird nicht direkt an Identity gekoppelt. User kann separat
per `db.Users.Find(cmd.RequestedByUserId)` geladen werden.

### 17.6 StateChange — Schaltereignisse

**Aufgabe:** vollständige Historie aller Schaltvorgänge. Wird beim
erfolgreichen Schaltbefehl vom Worker oder bei externem State-Change durch
Polling angelegt.

```csharp
namespace AQMS.Web.Models;

public class StateChange
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; }
    public DeviceState State { get; set; }

    public int DeviceId { get; set; }
    public Device Device { get; set; } = null!;

    public string? ChangedByUserId { get; set; }
}
```

**Konfiguration:**

| Punkt | Wert |
|---|---|
| `State` | HasConversion\<string>(), MaxLength(10) |
| `ChangedByUserId` | MaxLength(450) |
| `Timestamp` | DB-Default `GETUTCDATE()` |
| Beziehung zu Device | OnDelete.Restrict |
| Beziehung zu IdentityUser | OnDelete.SetNull, ohne Navigation |
| Index | Composite auf `(DeviceId, Timestamp)` |

**Konsistenz:** Property `State` nutzt denselben `DeviceState`-Enum wie
`Device.CurrentState` und `DeviceCommand.Action`. Schalt-Logik durchgängig
typsicher — nur Off oder On.

---

## 18. AqmsDbContext

### 18.1 Klassendefinition

Aus [AQMS.Web/Data/AqmsDbContext.cs](../AQMS.Web/Data/AqmsDbContext.cs):

```csharp
using AQMS.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AQMS.Web.Data;

public class AqmsDbContext(DbContextOptions<AqmsDbContext> options)
    : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<DeviceType>      DeviceTypes      { get; set; } = null!;
    public DbSet<MeasurementType> MeasurementTypes { get; set; } = null!;
    public DbSet<Device>          Devices          { get; set; } = null!;
    public DbSet<Measurement>     Measurements     { get; set; } = null!;
    public DbSet<DeviceCommand>   DeviceCommands   { get; set; } = null!;
    public DbSet<StateChange>     StateChanges     { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // ... 250 Zeilen Fluent-API-Konfiguration (siehe §19) ...
    }
}
```

### 18.2 Vererbungshierarchie

```
DbContext                              ← EF Core Basis
   │   (SaveChanges, DbSets, OnModelCreating)
   ▼
IdentityDbContext<IdentityUser>        ← ASP.NET Identity
   │   (7 AspNet*-Tabellen)
   ▼
AqmsDbContext                          ← unsere Klasse
       (6 fachliche Tabellen)
```

Durch Vererbung kommen automatisch in die Migration:

- AspNetUsers
- AspNetRoles
- AspNetUserRoles
- AspNetUserClaims
- AspNetRoleClaims
- AspNetUserLogins
- AspNetUserTokens

### 18.3 Generic-Parameter `<IdentityUser>`

`IdentityDbContext<TUser>` ist generisch mit dem User-Typ. AQMS verwendet
die Standard-Klasse `IdentityUser` mit ~14 Properties (Email, PasswordHash,
UserName, EmailConfirmed, ...). Ausreichend für AQMS — eine custom Klasse
`ApplicationUser : IdentityUser` mit z.B. DisplayName wäre möglich, aber
nicht nötig.

**Wichtig:** der Generic-Parameter muss in `Program.cs` und im DbContext
identisch sein:

```csharp
// Program.cs
builder.Services.AddDefaultIdentity<IdentityUser>(...)

// AqmsDbContext.cs
public class AqmsDbContext(...) : IdentityDbContext<IdentityUser>(options)
```

Sonst Runtime-Crash beim Start.

### 18.4 Primary Constructor (C# 12)

```csharp
public class AqmsDbContext(DbContextOptions<AqmsDbContext> options)
    : IdentityDbContext<IdentityUser>(options)
{
}
```

Drei Konzepte in einer Zeile: Klassendeklaration, Konstruktor-Parameter,
Vererbung mit Forwarding. Klassische Schreibweise zum Vergleich:

```csharp
public class AqmsDbContext : IdentityDbContext<IdentityUser>
{
    public AqmsDbContext(DbContextOptions<AqmsDbContext> options)
        : base(options) { }
}
```

Funktional identisch. Modern bevorzugt: Primary Constructor wegen weniger
Boilerplate.

### 18.5 DbSet-Properties

Jede `DbSet<T>`-Property = eine Tabelle. EF Core leitet aus dem
Property-Namen den Tabellennamen ab (Plural-Konvention).

| Konvention | Beispiel | Wirkung |
|---|---|---|
| Generic-Typ = Entity | `DbSet<Device>` | EF Core mappt `Device` auf Tabelle |
| Property-Name = Tabellenname | `Devices` | Tabelle heißt `Devices` |
| `public` | sichtbar | Repositories können zugreifen |
| `{ get; set; }` | Property mit Setter | EF Core füllt beim Konstruktor |
| `= null!` | Null-Forgiving | NRT-Warning unterdrücken |

**Wichtig:** Identity-Tabellen (AspNetUsers etc.) sind **nicht** als DbSet
sichtbar. Sie werden durch Vererbung erzeugt. Zugriff über
`UserManager<IdentityUser>` aus der Identity-API.

### 18.6 `null!` bei DbSets

Bei aktivierten Nullable Reference Types (Standard in .NET 8+) gibt der
Compiler eine Warning:

> *"Non-nullable property 'Devices' must contain a non-null value when
> exiting constructor."*

EF Core füllt die DbSets aber erst **nach** dem Konstruktor-Aufruf der
Basis-Klasse. Mit `= null!` versprichst du dem Compiler, dass die Property
zur Laufzeit nicht null ist.

Es ist ein **Versprechen, kein Kontrollmechanismus**. Greift man vor
EF-Core-Init auf das DbSet zu, knallt es trotzdem.

---

## 19. OnModelCreating und Fluent API

### 19.1 Was ist OnModelCreating?

Eine Methode, die EF Core **automatisch aufruft**, bevor es das Schema baut.
Sie ist der zentrale Konfigurationspunkt: hier wird festgelegt, wie genau
die Tabellen aussehen, welche Beziehungen, Indizes, Constraints, Defaults.

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);     // PFLICHT für Identity-Tabellen!

    // hier kommt die gesamte Konfiguration
}
```

### 19.2 Bedeutung der drei Schlüsselwörter

| Schlüsselwort | Wofür |
|---|---|
| `protected` | sichtbar nur für diese Klasse + erbende Klassen |
| `override` | überschreibt Methode aus Basis-Klasse `DbContext` |
| `void` | gibt nichts zurück, modifiziert nur den `builder` |

### 19.3 Warum `base.OnModelCreating(builder)` als erste Zeile?

Die Basis-Klasse `IdentityDbContext` konfiguriert in **ihrer**
Implementation die 7 Identity-Tabellen. Wenn die Zeile fehlt:

- Identity-Tabellen erscheinen nicht in der Migration
- Login wirft Runtime-Fehler `"Invalid object name 'AspNetUsers'"`
- Stunden Debugging vorprogrammiert

**Immer als allererste Zeile.** Erst Eltern-Konfiguration, dann eigene.

### 19.4 Fluent API: was und warum

Fluent API = Methodenketten zur Konfiguration. Beispiel:

```csharp
entity.Property(d => d.Name)
      .IsRequired()
      .HasMaxLength(50);
```

Liest sich: *"Beim Property `Name` — ist erforderlich — hat MaxLength 50."*

### 19.5 Fluent API vs. Data Annotations

| Konfiguration | Annotation | Fluent API |
|---|---|---|
| MaxLength | `[MaxLength(50)]` | `.HasMaxLength(50)` |
| Required | `[Required]` | `.IsRequired()` |
| Single Index | `[Index]` (.NET 8+) | `.HasIndex(...)` |
| Composite Index | ✗ | ✓ |
| Filtered Index | ✗ | ✓ |
| OnDelete-Verhalten | ✗ | ✓ |
| Enum als String | ✗ | ✓ `HasConversion<string>()` |
| DB-Default-Werte | ✗ | ✓ `HasDefaultValueSql(...)` |
| Seed-Daten | ✗ | ✓ `HasData(...)` |

Im AQMS durchgängig **Fluent API**, weil mehrere Features nur damit
verfügbar sind und alle Konfigurationen an einer zentralen Stelle stehen.

### 19.6 Block-Struktur pro Entity

```csharp
builder.Entity<Device>(entity =>
{
    // 1. Property-Konfigurationen (HasMaxLength, IsRequired, HasConversion)
    entity.Property(d => d.Name).IsRequired().HasMaxLength(50);
    entity.Property(d => d.DeviceIdentifier).IsRequired().HasMaxLength(100);
    entity.Property(d => d.IPAddress).HasMaxLength(45);
    entity.Property(d => d.CurrentState).HasConversion<string>().HasMaxLength(20);

    // 2. Indizes (HasIndex)
    entity.HasIndex(d => d.DeviceIdentifier).IsUnique();

    // 3. Beziehungen (HasOne/WithMany/HasForeignKey/OnDelete)
    entity.HasOne(d => d.DeviceType)
          .WithMany(dt => dt.Devices)
          .HasForeignKey(d => d.DeviceTypeId)
          .OnDelete(DeleteBehavior.Restrict);

    // 4. Seed-Daten (HasData)
    entity.HasData(/* ... */);
});
```

### 19.7 Vollständige Konfiguration im Überblick

**MeasurementType-Block:** Pflichtfelder, MaxLengths, 2 Seed-Einträge

**DeviceType-Block:** Pflichtfeld Name, MaxLength, 2 Seed-Einträge

**Device-Block:**

- CurrentState als String mit MaxLength(20)
- DeviceIdentifier required, MaxLength(100), UNIQUE
- Name required, MaxLength(50)
- IPAddress MaxLength(45)
- Beziehung zu DeviceType: Restrict
- 6 Seed-Einträge

**DeviceCommand-Block:**

- ResultMessage MaxLength(200)
- RequestedByUserId MaxLength(450)
- Action als String MaxLength(10)
- Status als String MaxLength(20), Default "Pending"
- CreatedAt DB-Default GETUTCDATE()
- Beziehung zu IdentityUser: SetNull (ohne Navigation)
- Beziehung zu Device: Restrict
- Filtered Index auf Status='Pending'

**Measurement-Block:**

- Beziehung zu Device: Restrict
- Beziehung zu MeasurementType: Restrict
- Composite Index (DeviceId, Timestamp)
- Single Index auf Timestamp
- Timestamp DB-Default GETUTCDATE()

**StateChange-Block:**

- ChangedByUserId MaxLength(450)
- State als String MaxLength(10)
- Beziehung zu Device: Restrict
- Beziehung zu IdentityUser: SetNull
- Composite Index (DeviceId, Timestamp)
- Timestamp DB-Default GETUTCDATE()

---

## 20. Tiefenkonzepte zum DbContext

### 20.1 Nullable Reference Types und null!

Seit C# 8 / .NET 6 sind NRT Standard:

- `string` = nicht null. Compiler überwacht das.
- `string?` = darf null sein. Beim Lesen prüfen.

**Drei Wege Compiler glücklich zu machen:**

| Pattern | Beispiel | Wann |
|---|---|---|
| Default-Wert | `public string Name { get; set; } = string.Empty;` | Pflicht-Strings |
| Konstruktor füllt | im Konstruktor zuweisen | wenn Wert gleich gesetzt wird |
| `null!` | `public DeviceType DeviceType { get; set; } = null!;` | Navigation-Properties die EF Core lädt |

**Pattern-Übersicht:**

| Situation | Pattern |
|---|---|
| Pflicht-String | `string Foo { get; set; } = string.Empty;` |
| Optionaler String | `string? Foo { get; set; }` |
| Pflicht-Navigation | `Foo Bar { get; set; } = null!;` |
| Optionale Navigation | `Foo? Bar { get; set; }` |
| Collection-Navigation | `ICollection<Foo> Foos { get; set; } = new List<Foo>();` |

### 20.2 ICollection und Navigation-Properties

**Sammlungs-Hierarchie:**

```
IEnumerable<T>      → durchlaufen (foreach)
   ↓
ICollection<T>      → + Add/Remove/Count/Contains
   ↓
IList<T>            → + Index-Zugriff [0]
   ↓
List<T>             → + Sort, Find, ...
```

EF Core verlangt **mindestens** `ICollection<T>` (für Add/Remove).
Konvention: das **kleinste passende Interface** verwenden, EF Core
flexibler bei der Implementierungswahl.

**Pattern: Interface außen, List innen:**

```csharp
public ICollection<Device> Devices { get; set; } = new List<Device>();
```

Property-Typ ist Interface (Vertrag), Initialisierung ist konkrete Klasse.

### 20.3 Enums in EF Core

**Default-Verhalten:** EF Core speichert Enums als `int`. Werte 0, 1, 2 in der DB.

**Variante mit String-Speicherung:**

```csharp
entity.Property(c => c.Status)
      .HasConversion<string>()
      .HasMaxLength(20);
```

DB-Spalte `nvarchar(20)`. Werte "Pending", "Executed", "Failed".

**Vorteile (siehe §16.6):** Lesbarkeit, JSON-API-Klarheit, Filtered Index.

### 20.4 Konventionen vs. Annotation vs. Fluent API

| Ansatz | Beispiel | Wann |
|---|---|---|
| Konvention | `public int Id` → wird PK | Standardfall |
| Annotation | `[MaxLength(50)]` am Property | einfache Konfigurationen |
| Fluent API | `.HasMaxLength(50)` in OnModelCreating | komplexe Konfigurationen |

Im AQMS durchgängig **Fluent API** (siehe §19.5 für die Begründung).

### 20.5 Was die DbContext-Klasse NICHT macht

Die Klasse beschreibt nur die **Struktur** der Datenbank. Folgendes ist
explizit NICHT in der Datei:

- **Geschäftslogik** → in Service-Klassen
- **Datenzugriffsmethoden** wie `GetActiveDevices()` → in Repository-Klassen
- **Validierung** → in Models oder DTOs
- **Logging** → per DI/Configuration
- **Caching** → eigene Schicht
- **Transaktionssteuerung** → vom DbContext implizit oder von Services explizit
- **Authentifizierungslogik** → Identity macht das

Single-Responsibility-Prinzip: reine Schema-Beschreibung.

---

## 21. Indizes, OnDelete und DB-Defaults

### 21.1 Composite Index

```csharp
entity.HasIndex(m => new { m.DeviceId, m.Timestamp });
```

Index über zwei Spalten in dieser Reihenfolge. Optimiert Queries wie:

```sql
SELECT * FROM Measurements
WHERE DeviceId = 1
  AND Timestamp BETWEEN '2026-04-01' AND '2026-05-01'
ORDER BY Timestamp DESC;
```

**Reihenfolge ist kritisch:** die Spalte mit häufigerem Filter (eindeutiger Wert)
gehört nach vorne. Bei AQMS ist das `DeviceId` — wir suchen meist "alle Werte
EINES Geräts" in einem Zeitraum, nicht "alle Werte ALLER Geräte".

### 21.2 Filtered Index für Pi-Polling

```csharp
entity.HasIndex(c => c.Status)
      .HasFilter("[Status] = 'Pending'");
```

Indiziert nur die Zeilen mit `Status = 'Pending'`. Anwendungsfall: Pi
pollt alle 10 s:

```sql
SELECT * FROM DeviceCommands WHERE Status = 'Pending';
```

Da der Großteil aller Befehle `Executed` oder `Failed` ist, wäre ein
normaler Index ineffizient. Filtered Index ist klein und sehr schnell.

**Voraussetzung:** Status muss als String gespeichert sein (siehe §16.6).

### 21.3 UNIQUE-Index

```csharp
entity.HasIndex(d => d.DeviceIdentifier).IsUnique();
```

Erzwingt Eindeutigkeit auf DB-Ebene. Verhindert Duplikate selbst bei
direktem SQL-Insert.

### 21.4 Automatische Indizes auf FKs

EF Core legt automatisch Indizes auf alle FK-Spalten an. Beschleunigt Joins
und FK-Constraint-Prüfungen.

### 21.5 Alle Indizes im AQMS

| Index | Tabelle | Spalten | Typ |
|---|---|---|---|
| `IX_Devices_DeviceIdentifier` | Devices | DeviceIdentifier | UNIQUE |
| `IX_Devices_DeviceTypeId` | Devices | DeviceTypeId | auto FK |
| `IX_Measurements_DeviceId_Timestamp` | Measurements | DeviceId, Timestamp | Composite |
| `IX_Measurements_Timestamp` | Measurements | Timestamp | Single |
| `IX_Measurements_MeasurementTypeId` | Measurements | MeasurementTypeId | auto FK |
| `IX_DeviceCommands_Status` | DeviceCommands | Status | filtered |
| `IX_DeviceCommands_DeviceId` | DeviceCommands | DeviceId | auto FK |
| `IX_DeviceCommands_RequestedByUserId` | DeviceCommands | RequestedByUserId | auto FK |
| `IX_StateChanges_DeviceId_Timestamp` | StateChanges | DeviceId, Timestamp | Composite |
| `IX_StateChanges_ChangedByUserId` | StateChanges | ChangedByUserId | auto FK |

### 21.6 OnDelete-Verhalten — drei Varianten

| Wert | Verhalten beim Parent-Delete |
|---|---|
| `Cascade` | Children werden mitgelöscht (EF Core Default!) |
| `Restrict` | Parent kann nicht gelöscht werden, solange Children existieren |
| `SetNull` | Children-FK wird auf NULL (nur bei nullable FKs) |

### 21.7 OnDelete-Konfiguration im AQMS

| Beziehung | Verhalten | Begründung |
|---|---|---|
| Device → DeviceType | Restrict | Lookup-Schutz |
| Measurement → Device | Restrict | Datensicherheit |
| Measurement → MeasurementType | Restrict | Lookup-Schutz |
| DeviceCommand → Device | Restrict | Befehlshistorie schützen |
| DeviceCommand → IdentityUser | SetNull | DSGVO bei User-Löschung |
| StateChange → Device | Restrict | Eventhistorie schützen |
| StateChange → IdentityUser | SetNull | DSGVO |

### 21.8 Pattern für die Beziehungs-Konfiguration

```csharp
entity.HasOne(d => d.DeviceType)
      .WithMany(dt => dt.Devices)
      .HasForeignKey(d => d.DeviceTypeId)
      .OnDelete(DeleteBehavior.Restrict);
```

Liest sich: *"Ein Device hat einen DeviceType, ein DeviceType hat viele
Devices, FK heißt DeviceTypeId, beim Löschen blockieren."*

**Fixe Reihenfolge:**

1. `HasOne(...)` — eine Navigation auf der aktuellen Seite
2. `WithMany(...)` — die andere Seite
3. `HasForeignKey(...)` — welche Property der FK ist
4. `OnDelete(...)` — Verhalten

**Spezialfall: Beziehung zu IdentityUser:**

```csharp
entity.HasOne<IdentityUser>()
      .WithMany()
      .HasForeignKey(c => c.RequestedByUserId)
      .OnDelete(DeleteBehavior.SetNull);
```

`HasOne<IdentityUser>()` mit Generic statt Lambda, weil keine
Navigation-Property auf der DeviceCommand-Klasse existiert. `WithMany()`
ohne Argument, weil auf der IdentityUser-Seite keine Liste der Commands
existiert.

### 21.9 DB-seitige Default-Werte

```csharp
entity.Property(m => m.Timestamp)
      .HasDefaultValueSql("GETUTCDATE()");
```

Bei jedem INSERT ohne expliziten Wert ruft SQL Server `GETUTCDATE()` auf.

**Drei Strategien für Zeitstempel:**

| Variante | Wo läuft das | Wann ist der Zeitpunkt |
|---|---|---|
| Property-Init `= DateTime.UtcNow` | C# beim Erstellen | Wann das Objekt instanziiert wurde |
| Im Code explizit setzen | C# beim Insert | Wann SaveChanges aufgerufen |
| DB-Default `GETUTCDATE()` | SQL Server beim INSERT | Wann die DB den INSERT verarbeitet |

DB-Default ist am robustesten: garantiert der echte Persist-Zeitpunkt.

**UTC vs. Lokalzeit:** UTC. VPS und Pi unterschiedliche Zeitzonen,
Sommerzeit-Umstellung kann Stempel doppelt machen, Sortierungen nur in
UTC eindeutig. Umrechnung erst im Frontend mit `.ToLocalTime()`.

**DB-Spezifität:** `GETUTCDATE()` ist SQL-Server-spezifisch. Bei Postgres
wäre es `NOW() AT TIME ZONE 'utc'`, bei MySQL `UTC_TIMESTAMP()`. Da AQMS
ausschließlich SQL Server nutzt, unkritisch.

---

## 22. Migrations-Workflow und Seed-Daten

### 22.1 Befehle in der Package Manager Console

| Befehl | Wirkung |
|---|---|
| `Add-Migration <Name>` | generiert Migration aus aktueller Konfiguration |
| `Update-Database` | wendet alle ausstehenden Migrations an |
| `Update-Database <Name>` | wechselt zu einer bestimmten Migration |
| `Remove-Migration` | entfernt zuletzt erzeugte (wenn nicht angewendet) |
| `Drop-Database` | DB komplett löschen (nur Dev) |
| `Script-Migration` | generiert SQL-Skript |

### 22.2 Workflow

1. Entity-Klasse oder DbContext-Konfiguration ändern
2. `Add-Migration <SinnvollerName>`
3. Generierte `.cs`-Datei prüfen (Migrations/-Ordner)
4. Bei Bedarf `Remove-Migration` und Code anpassen
5. `Update-Database` — Migration auf DB anwenden
6. Tabellen verifizieren (SQL Server Object Explorer)

### 22.3 Naming-Konvention

PascalCase, beschreibend:

```
✓ AddDomainEntities
✓ AddCurrentStateToDevice
✓ RemoveObsoleteIndex
✗ added-stuff
✗ migration1
✗ fix
```

C# warnt bei Klassennamen, die nur Kleinbuchstaben enthalten (CS8981).

### 22.4 Aktuelle Migrations im AQMS

```
AQMS.Web/Data/Migrations/
├── 00000000000000_CreateIdentitySchema.cs       ← aus Identity-UI-Template
├── 00000000000000_CreateIdentitySchema.Designer.cs
├── 20260509115558_AddDomainEntities.cs           ← unsere Domain-Tabellen + Seeds
├── 20260509115558_AddDomainEntities.Designer.cs
└── ApplicationDbContextModelSnapshot.cs
```

`AddDomainEntities` ist die finale Initial-Migration für die
Persistenzschicht und enthält:

- Erstellung aller 6 Domain-Tabellen
- Alle Indizes (Composite, Filtered, UNIQUE)
- Alle FK-Constraints mit OnDelete-Verhalten
- Alle Seed-Inserts (2 DeviceTypes, 2 MeasurementTypes, 6 Devices)

### 22.5 Seed-Daten via HasData

`HasData(...)` definiert Initialdaten. Versioniert über Migrations,
automatisch beim `Update-Database`.

```csharp
entity.HasData(
    new DeviceType { Id = 1, Name = "Sensor" },
    new DeviceType { Id = 2, Name = "SmartPlug" }
);
```

**Wichtige Regeln:**

| Regel | Konsequenz |
|---|---|
| Id explizit setzen | Migration crasht sonst |
| Keine Navigation-Properties | EF Core warnt |
| FK-Properties explizit setzen | Beziehungen werden korrekt aufgebaut |
| Property-Defaults aus Klasse greifen NICHT | z.B. `IsEnabled = true` muss im Seed explizit |
| Neue Seed-Werte → neue Migration | bestehende werden in nächster Migration aktualisiert |

### 22.6 AQMS-Seeds-Übersicht

| Tabelle | Anzahl | Inhalt |
|---|---|---|
| DeviceTypes | 2 | Sensor, SmartPlug |
| MeasurementTypes | 2 | Temperature (°C), Power (W) |
| Devices | 6 | Pi + 5 Shellys mit IPs und kebab-case Identifier |

Identity-User und -Rollen werden **nicht** über `HasData` gesetzt —
Passwort-Hashing geht nicht in HasData. Geplant: separate
DbSeeder-Klasse beim App-Start mit `UserManager`.

### 22.7 Praxisproblem: Schema-Skript auf SQL Server (Linux) einspielen

Beim erstmaligen Einspielen des per `Script-Migration -Idempotent`
erzeugten Skripts in die SQL-Server-Instanz auf dem VPS (Docker-Container,
eingespielt über `sqlcmd`) traten nacheinander zwei Fehler auf. Beide sind
nicht offensichtlich, weshalb sie hier für künftige Deployments
dokumentiert sind.

**Problem 1 — BOM am Dateianfang**

*Symptom:* `Incorrect syntax near '﻿'` in Zeile 1, gefolgt von
`Invalid object name '__EFMigrationsHistory'` für alle nachfolgenden
Prüfungen.

*Ursache:* `Script-Migration` schreibt die `.sql`-Datei als UTF-8 **mit
BOM** (Byte Order Mark, `EF BB BF`). `sqlcmd` interpretiert das BOM nicht
und stolpert über die erste Zeile. Da diese den `CREATE TABLE` für
`__EFMigrationsHistory` enthält, scheitern alle folgenden idempotenten
`IF NOT EXISTS`-Prüfungen, die genau diese Tabelle abfragen.

*Lösung:* BOM vor dem Einspielen aus der Datei entfernen:

```bash
sed -i '1s/^\xEF\xBB\xBF//' aqms-schema.sql
```

**Problem 2 — QUOTED_IDENTIFIER bei Filtered Index**

*Symptom:* `CREATE INDEX failed because the following SET options have
incorrect settings: 'QUOTED_IDENTIFIER'`, gefolgt von
`Foreign key '...' references invalid table 'AspNetUsers'`.

*Ursache:* Das Schema enthält einen **Filtered Index** auf
`DeviceCommands.Status` (`HasFilter("[Status] = 'Pending'")`, siehe §21).
SQL Server verlangt für Filtered Indexes zwingend
`SET QUOTED_IDENTIFIER ON`. `sqlcmd` setzt diese Option per Default nicht.
Bricht der `CREATE INDEX` ab, stoppt das Skript an dieser Stelle —
`AspNetUsers` wird nicht angelegt, und die FK von `DeviceCommands` darauf
schlägt als Folgefehler fehl.

*Lösung:* `sqlcmd` mit Schalter `-I` (Quoted Identifiers aktivieren)
aufrufen.

**Verbindlicher Einspiel-Befehl für künftige Deployments:**

```bash
# 1. BOM entfernen
sed -i '1s/^\xEF\xBB\xBF//' aqms-schema.sql

# 2. Skript mit -I in die DB streamen
docker exec -i aqms-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P '<PASSWORD>' -C -I -d aqms \
  < aqms-schema.sql
```

Die `Warning!`-Meldungen zur 900-Byte-Schlüssellänge der
Identity-Tabellen (`PK_AspNetUserRoles`, `PK_AspNetUserTokens`) sind
**harmlos** — sie betreffen nur theoretische Maximallängen der
`nvarchar(450)`-Schlüssel und treten im Betrieb nicht ein.

*Lessons Learned:* Ein per `Script-Migration` erzeugtes idempotentes
Skript ist nicht ohne Weiteres `sqlcmd`-kompatibel. Für reproduzierbare
Deployments muss der Einspiel-Vorgang BOM-Entfernung und den `-I`-Schalter
fest vorsehen. Ein einzelner fehlgeschlagener `CREATE INDEX` reißt wegen
der sequentiellen Skript-Abarbeitung nachgelagerte Objekte (Tabellen, FKs)
mit — Fehlermeldungen daher immer von oben nach unten lesen, der erste
echte Fehler ist die Wurzel.

> **Ergänzung — `-I` gilt für jeden Schreibzugriff.** Der `-I`-Schalter
> ist nicht skript-spezifisch. Sobald die `aqms`-Datenbank einen Filtered
> Index enthält (hier: auf `DeviceCommands.Status`, siehe §21), verlangt
> SQL Server `SET QUOTED_IDENTIFIER ON` für **jede** schreibende Operation
> in der gesamten Datenbank — auch für ein simples `UPDATE` auf einer
> völlig anderen Tabelle wie `AspNetUsers`. Die Option wird datenbankweit
> geprüft, nicht pro Tabelle. Konsequenz: **Jeder** manuelle
> `sqlcmd`-Aufruf mit `INSERT`/`UPDATE`/`DELETE` gegen die `aqms`-DB muss
> `-I` setzen, nicht nur das Einspielen des Schema-Skripts. Reine
> Lese-Abfragen (`SELECT`) sind nicht betroffen.

---

# Teil E — VPS und Deployment (Test-Stand)

> **Hinweis zur Struktur von Teil E.** Die Sektionen §23A–§23F dokumentieren
> die **aktuelle** Server-Umgebung (neuer VPS, Debian 13). Sie entstanden
> durch die VPS-Migration im Mai 2026. Die nachfolgenden Sektionen
> §23–§28 beschreiben die **frühere** Einrichtung auf dem ersten VPS
> (Debian 12) und gelten als **historischer Stand** — sie bleiben erhalten,
> weil die dort dokumentierten Praxisprobleme (systemd 217/USER §25,
> Upload-Rechte §28) als Lernmaterial für die Diplomarbeit relevant sind
> und in §23A–§23F teils referenziert werden. Für den aktuellen
> Server-Stand gelten §23A–§23F.

## 23A. VPS-Migration: Anlass und Entscheidung

### 23A.1 Anlass der Migration

Der ursprüngliche VPS (Debian 12, Einrichtung dokumentiert in §23–§28)
verfügte über **1,9 GB RAM**. Für den Test-Stand mit einer leeren
ASP.NET-Core-Anwendung reichte das aus. Mit dem Fortschritt der
Persistenzschicht (§13–§22) wurde jedoch eine **echte
SQL-Server-Datenbank** auf dem Server benötigt — bis dahin lag die
Datenbank nur lokal auf der Windows-Entwicklungsmaschine (LocalDB).

LocalDB ist eine reine Windows-Entwickler-Engine und auf einem
Linux-Server nicht verfügbar. Für den Produktivbetrieb war daher eine
echte Datenbank-Instanz auf dem VPS erforderlich. Microsoft liefert
**SQL Server für Linux**, das den im gesamten Projekt verwendeten
Provider (`UseSqlServer`, Code-First mit EF Core) unverändert
weiterverwenden lässt.

**Das Problem:** SQL Server für Linux verlangt **mindestens 2 GB RAM
allein für die Datenbank-Engine** und startet ohne diese gar nicht.
Eine Messung auf dem alten VPS ergab:

```
              gesamt   benutzt   frei    verfügbar
Speicher:      1,9Gi    709Mi   193Mi      1,2Gi
Swap:             0B       0B      0B
```

Mit 1,9 GB Gesamt-RAM und ohne Swap konnte der SQL-Server-Container
nicht betrieben werden. Selbst mit Swap wäre nach Abzug des Engine-
Bedarfs kein Spielraum für die `AQMS.Web` und die auf dem Server bereits
laufenden Dienste geblieben.

### 23A.2 Abgewogene Optionen

Drei Lösungswege wurden gegeneinander abgewogen:

| Option | Beschreibung | Bewertung |
|---|---|---|
| A — Swap einrichten | 4 GB Swap-File auf dem alten VPS, bei SQL Server bleiben | Engine könnte starten, aber Swap ist Festplatte statt RAM → spürbare Verlangsamung unter Last; kein Puffer für Web-App und Bestandsdienste |
| B — Wechsel zu PostgreSQL | Schlanke Engine (~200–300 MB), läuft ohne Swap | Provider-Wechsel im Code (`Npgsql`), Migrations müssten neu generiert werden (provider-spezifisches SQL wie `GETUTCDATE()`) |
| C — Neuer Server | VPS mit mehr RAM | Geringe Mehrkosten, kein Code-Eingriff, gesamter Tech-Stack bleibt unangetastet |

**Gewählt: Option C.** Begründung: Ein RAM-Upgrade des bestehenden VPS
war beim Anbieter nicht möglich, ein neuer Server ohnehin nötig. Damit
entfiel der Nachteil von C weitgehend. Gegenüber A bietet C echten RAM
statt Swap und damit verlässliche Performance für den 24h-Dauerlauf in
Phase 5. Gegenüber B bleibt der etablierte Tech-Stack (SQL Server,
EF-Core-Migrations) unverändert — kein Provider-Wechsel, kein
Neugenerieren der Migrations, keine Diskrepanz zwischen Entwicklungs- und
Produktivdatenbank. Die im Projekt früh getroffene
SQL-Server-Festlegung wird so konsistent durchgehalten.

> Diese Migration ist ein dokumentiertes Beispiel dafür, wie eine reale
> Hardware-Beschränkung eine frühe Technologie-Annahme korrigiert. Die
> verworfenen Optionen A und B sind in §30 als verworfene Alternativen
> festgehalten.

### 23A.3 Server-Vergleich alt/neu

| Eckdatum | Alter VPS (historisch) | Neuer VPS (aktuell) |
|---|---|---|
| Betriebssystem | Debian 12 (bookworm) | Debian 13 (trixie) |
| Architektur | x86-64 | x86-64 |
| RAM | 1,9 GB | 8 GB |
| Swap | 0 B | 0 B (nicht nötig) |
| IP-Adresse | 203.0.113.11 | 203.0.113.10 |
| Domain | aqms.aqms.example.com | aqms.aqms.example.com (neuer A-Record) |
| Datenbank | keine (nur Test-App) | SQL Server 2022 in Docker |

Die **Architektur x86-64** wurde bewusst beibehalten: Microsoft liefert
SQL Server für Linux ausschließlich als x86-64-Image aus, ein offizielles
ARM64-Image existiert nicht. Ein ARM-Server hätte SQL Server nur über
Emulation betreiben können — für einen Test-/Produktivbetrieb nicht
vertretbar. Bei der RAM-Größe fiel die Wahl auf **8 GB** statt der knapp
ausreichenden 4 GB: SQL Server allein beansprucht ~2–2,5 GB, hinzu kommen
`AQMS.Web`, die Bestandsdienste, Nginx und OS-Overhead. 8 GB bieten
echten Puffer, sodass die Datenbank-Engine Cache nutzen kann und der
geplante 24h-Lasttest (Phase 5) nicht durch Speicherdruck verfälscht
wird.

## 23B. DNS-Umstellung auf den neuen Server

Die Subdomain `aqms.aqms.example.com` musste auf die neue Server-IP
`203.0.113.10` zeigen. Die DNS-Verwaltung von `aqms.example.com` enthielt
zwei bestehende A-Records:

- `*` (Wildcard) — fängt alle Subdomains ab, zeigte auf den alten Server.
- `@` (Root-Domain) — `aqms.example.com` selbst.

**Vorgehen:** Statt den Wildcard-Record umzubiegen, wurde ein
**spezifischer A-Record** `aqms` neu angelegt:

| Feld | Wert |
|---|---|
| Name / Host | `aqms` |
| Typ | `A` |
| Ziel | `203.0.113.10` |

*Begründung:* In DNS schlägt ein exakter Record den Wildcard. Ein
expliziter `aqms`-Record lenkt nur diese eine Subdomain auf den neuen
Server; alle anderen Subdomains laufen über den unveränderten Wildcard
weiter auf dem alten Server. So fiel während der Migration kein anderer
Dienst aus, und der alte Server blieb über seine IP voll erreichbar.

## 23C. Server-Grundeinrichtung

### 23C.1 Eckdaten-Prüfung

Nach dem Erst-Login als `root` wurden Architektur, OS und RAM verifiziert:

```bash
uname -m && cat /etc/os-release | head -2 && free -h
```

Ergebnis: `x86_64`, `Debian GNU/Linux 13 (trixie)`, `7,8 GiB` RAM.
Bestätigt die bestellten Eckdaten.

### 23C.2 System aktualisieren

```bash
apt update && apt upgrade -y
```

*Begründung:* Frischer Server zuerst auf aktuellen Paketstand bringen,
bevor weitere Software installiert wird — vermeidet Konflikte mit
veralteten Abhängigkeiten.

### 23C.3 Nicht-root-Benutzer anlegen

```bash
adduser deployuser
usermod -aG sudo deployuser
```

*Begründung:* Dauerhaftes Arbeiten als `root` ist riskant — jeder
Tippfehler läuft mit voller Systemberechtigung. Der dedizierte User
`deployuser` mit `sudo`-Rechten administriert bei Bedarf, ohne permanent
root zu sein. Der Benutzername ist bewusst identisch zum alten VPS
gewählt, damit Pfade und Konventionen konsistent bleiben. Der Login
erfolgt für den Test-Stand per Passwort; eine Härtung des SSH-Zugangs
(Key-Authentifizierung, root-Login deaktivieren) ist als späterer Schritt
für die Sicherheitsbetrachtung vorgesehen.

## 23D. .NET 10 Runtime installieren (Debian 13)

Hier weicht das Vorgehen vom alten VPS ab: Microsoft stellt die
Paketquelle **pro Debian-Version** bereit. Der Pfad enthält `debian/13`
statt `debian/12`.

```bash
wget https://packages.microsoft.com/config/debian/13/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

sudo apt-get update && sudo apt-get install -y aspnetcore-runtime-10.0
```

*Begründung:* Auf dem Server wird nur die **Runtime** installiert, nicht
das volle SDK — entwickelt und gebaut wird auf der Windows-Maschine.
Konkret die **ASP.NET Core Runtime** (nicht die schmalere
`dotnet-runtime-10.0`), da `AQMS.Web` eine ASP.NET-Core-MVC-Anwendung ist.
Das hält die Angriffsfläche klein (kein Compiler auf dem Produktivserver).

Verifikation mit `dotnet --info` bestätigte:
`Microsoft.AspNetCore.App 10.0.8` und `Microsoft.NETCore.App 10.0.8`,
keine SDKs — wie beabsichtigt.

## 23E. Docker installieren (Debian 13)

Docker wird für den Betrieb des SQL-Server-Containers benötigt. Installiert
wurde Docker CE aus dem **offiziellen Docker-Repository** (aktueller
gepflegt als Debians `docker.io`-Paket, für Container-Betrieb empfohlen).

```bash
sudo apt update
sudo apt install -y ca-certificates curl
sudo install -m 0755 -d /etc/apt/keyrings
sudo curl -fsSL https://download.docker.com/linux/debian/gpg -o /etc/apt/keyrings/docker.asc
sudo chmod a+r /etc/apt/keyrings/docker.asc

sudo tee /etc/apt/sources.list.d/docker.sources <<EOF
Types: deb
URIs: https://download.docker.com/linux/debian
Suites: $(. /etc/os-release && echo "$VERSION_CODENAME")
Components: stable
Architectures: $(dpkg --print-architecture)
Signed-By: /etc/apt/keyrings/docker.asc
EOF

sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

sudo usermod -aG docker deployuser
```

*Begründung:* Der GPG-Schlüssel verifiziert die Paket-Echtheit.
`$(. /etc/os-release && echo "$VERSION_CODENAME")` setzt den Codename
`trixie` automatisch ein — kein hartkodierter Wert, der bei einem
OS-Wechsel falsch wäre. Die Aufnahme von `deployuser` in die `docker`-Gruppe
erlaubt `docker`-Befehle ohne `sudo` (greift nach erneutem Login).
Sicherheitshinweis: Die `docker`-Gruppe verleiht faktisch root-äquivalente
Rechte — auf einem Single-Admin-Server vertretbar, aber eine bewusste
Abwägung.

Verifikation: `docker run hello-world` lief nach erneutem Login ohne
`sudo` erfolgreich durch.

## 23F. SQL Server in Docker

### 23F.1 Container starten

```bash
docker run -d \
  --name aqms-sqlserver \
  --restart unless-stopped \
  -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=<PASSWORD>" \
  -e "MSSQL_PID=Express" \
  -p 127.0.0.1:1433:1433 \
  -v aqms-sqldata:/var/opt/mssql \
  mcr.microsoft.com/mssql/server:2022-latest
```

*Begründung der zentralen Optionen:*

- `-p 127.0.0.1:1433:1433` — der Datenbank-Port wird **nur an localhost**
  gebunden, nicht an das öffentliche Interface. Die Datenbank ist
  ausschließlich für die `AQMS.Web` auf demselben Host erreichbar, niemals
  aus dem Internet. Dies ist die zentrale Sicherheitsentscheidung des
  Datenbank-Setups.
- `--restart unless-stopped` — der Container startet nach einem
  Server-Reboot automatisch, analog zum systemd-Service der Web-App.
- `MSSQL_PID=Express` — die kostenlose Express-Edition ist lizenzfrei und
  für die Datenmengen eines Aquarium-Monitorings (wenige Messwerte pro
  Minute) deutlich ausreichend.
- `-v aqms-sqldata:/var/opt/mssql` — ein Named Volume entkoppelt die
  Datenbank-Dateien vom Container; sie überleben einen Neuaufbau des
  Containers.

Das SA-Passwort wird nicht in Code oder Doku notiert (Platzhalter
`<PASSWORD>`). SQL Server verlangt ein starkes Passwort (8+ Zeichen,
Groß-/Kleinbuchstaben, Ziffer, Sonderzeichen).

### 23F.2 Datenbank anlegen und Schema einspielen

Nach erfolgreichem Container-Start (`docker logs` meldete
`SQL Server is now ready for client connections`) wurde die leere
Datenbank `aqms` angelegt und das per `Script-Migration -Idempotent`
erzeugte Schema-Skript eingespielt. Beim Einspielen über `sqlcmd` traten
zwei nicht offensichtliche Fehler auf (BOM am Dateianfang,
`QUOTED_IDENTIFIER` bei Filtered Index) — beide sind mit Symptom, Ursache,
Lösung und dem verbindlichen Einspiel-Befehl in **§22.7** dokumentiert.

Der erfolgreich erprobte Ablauf:

```bash
# DB anlegen (ggf. sauber zurücksetzen)
docker exec -i aqms-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P '<PASSWORD>' -C \
  -Q "DROP DATABASE IF EXISTS aqms; CREATE DATABASE aqms;"

# BOM entfernen, dann Skript mit -I einspielen (siehe §22.7)
sed -i '1s/^\xEF\xBB\xBF//' aqms-schema.sql
docker exec -i aqms-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P '<PASSWORD>' -C -I -d aqms \
  < aqms-schema.sql
```

### 23F.3 Verifikation des Schemas

Die Datenbank wurde nach dem Einspielen geprüft:

```bash
docker exec -it aqms-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P '<PASSWORD>' -C -d aqms \
  -Q "SELECT COUNT(*) AS Devices FROM Devices; SELECT MigrationId FROM __EFMigrationsHistory; SELECT name FROM sys.tables ORDER BY name;"
```

Ergebnis bestätigt:

- **6 Devices** (die Seed-Daten: Pi + 5 Shellys, siehe §22.6)
- **2 Migrations** registriert: `CreateIdentitySchema` und
  `AddDomainEntities`
- **14 Tabellen** insgesamt: 6 Domain-Tabellen (`Devices`, `DeviceTypes`,
  `DeviceCommands`, `Measurements`, `MeasurementTypes`, `StateChanges`),
  7 Identity-Tabellen (`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`,
  `AspNetUserClaims`, `AspNetUserLogins`, `AspNetUserTokens`,
  `AspNetRoleClaims`) sowie `__EFMigrationsHistory`.

Damit ist die Persistenzschicht auf dem neuen Server vollständig und
verifiziert betriebsbereit.

## 23G. Deployment der AQMS.Web

### 23G.1 Veröffentlichung auf der Entwicklungsmaschine

Die Anwendung wird auf der Windows-Entwicklungsmaschine veröffentlicht
und als fertiges Paket auf den Server übertragen (Framework-Dependent
Deployment — die .NET-10-Runtime ist auf dem Server bereits installiert,
siehe §23D).

```powershell
dotnet publish AQMS.Web -c Release -o C:\temp\publish-aqms
```

*Begründung:* `-c Release` baut optimiert ohne Debug-Symbole. Der
Output-Ordner liegt **außerhalb** des OneDrive-Sync-Verzeichnisses —
andernfalls würde OneDrive während des Builds hunderte Build-Dateien
synchronisieren, was den Vorgang verlangsamt und Dateisperren verursachen
kann. Kein `--runtime`-Schalter, da die Runtime serverseitig vorhanden ist
(schlankeres Paket).

### 23G.2 Upload auf den Server

```bash
# auf dem Server: Zielverzeichnis anlegen
sudo mkdir -p /var/www/aqms
sudo chown deployuser:deployuser /var/www/aqms
```

```powershell
# auf der Entwicklungsmaschine: Upload
scp -r C:\temp\publish-aqms\* deployuser@203.0.113.10:/var/www/aqms/
```

*Begründung:* `/var/www/aqms` als Deploy-Verzeichnis (konsistent mit dem
historischen §24). Der `chown` auf `deployuser` erlaubt den `scp`-Upload
ohne root-Rechte.

### 23G.3 Manueller Start-Test

Vor der Einrichtung als Dienst wurde geprüft, dass die Anwendung lädt:

```bash
cd /var/www/aqms && dotnet AQMS.Web.dll
```

Kestrel meldete `Now listening on: http://localhost:5000`, Hosting
Environment `Production`. Die Warnung zum nicht konfigurierten
XML-Encryptor (Data Protection) ist in diesem Zustand erwartbar und wird
durch den Dauerbetrieb unter systemd unkritisch.

## 23H. systemd-Service

### 23H.1 Dedizierter Service-User

```bash
sudo useradd -r -s /usr/sbin/nologin aqms
sudo chown -R aqms:aqms /var/www/aqms
```

*Begründung:* `-r` legt einen System-Account ohne Home-Verzeichnis an,
`-s /usr/sbin/nologin` verhindert interaktiven Login. Der Dienst läuft
unter einem eigenen, rechtearmen User — nicht unter `deployuser`, nicht
unter root. Wird die Web-Anwendung kompromittiert, ist der Schaden auf
diesen rechtelosen Account begrenzt. Dasselbe Prinzip wie auf dem alten
VPS (§25).

> **Praxishinweis:** Nach `chown` auf `aqms` kann `deployuser` nicht mehr
> per `scp` ins Deploy-Verzeichnis schreiben — dies ist das aus §28
> bekannte Upload-Rechte-Problem. Beim nächsten Deployment ist es bewusst
> zu behandeln (z.B. `deployuser` in die `aqms`-Gruppe aufnehmen und
> Gruppen-Schreibrechte setzen).

### 23H.2 Unit-Datei

`/etc/systemd/system/aqms-web.service`:

```ini
[Unit]
Description=AQMS Web Application
After=network.target

[Service]
WorkingDirectory=/var/www/aqms
ExecStart=/usr/bin/dotnet /var/www/aqms/AQMS.Web.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=aqms-web
User=aqms
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

*Begründung:* `Restart=always` startet die Anwendung nach einem Absturz
neu — die in §1.2 geforderte Fehlertoleranz. `User=aqms` bindet den Dienst
an den rechtearmen User. Der Connection String wird bewusst **nicht** in
diese Datei geschrieben, sondern in eine separate Override-Datei (§23H.3).

### 23H.3 Connection String als geschützte Override-Variable

```bash
sudo systemctl edit aqms-web
```

Inhalt der Override (`/etc/systemd/system/aqms-web.service.d/override.conf`):

```ini
[Service]
Environment="ConnectionStrings__DefaultConnection=Server=127.0.0.1,1433;Database=aqms;User Id=sa;Password=<PASSWORD>;TrustServerCertificate=True"
```

*Begründung:* Der doppelte Unterstrich `__` ist die ASP.NET-Core-
Konvention für verschachtelte Konfigurationsschlüssel —
`ConnectionStrings__DefaultConnection` entspricht exakt
`GetConnectionString("DefaultConnection")` in `Program.cs`.
`TrustServerCertificate=True` ist nötig, da der SQL-Server-Container ein
selbstsigniertes Zertifikat verwendet. Die Override-Datei ist nur für root
lesbar — das Passwort steht damit nicht im Git-Repository und nicht in der
Haupt-Unit. Dies setzt die in §11 festgelegte Secrets-Strategie
(Umgebungsvariable in Produktion) konkret um.

### 23H.4 Dienst aktivieren

```bash
sudo systemctl daemon-reload
sudo systemctl enable --now aqms-web
```

`enable --now` startet den Dienst sofort und richtet den Autostart beim
Boot ein. Verifikation: `systemctl status aqms-web` zeigte
`active (running)`.

### 23H.5 Verifikation der Datenbank-Verbindung

Der Start allein beweist die DB-Anbindung noch nicht — der DbContext
verbindet sich erst bei der ersten datenbankgebundenen Anfrage. Geprüft
wurde daher die Identity-Login-Seite, die zwingend auf die
Identity-Tabellen zugreift:

```bash
curl -i http://localhost:5000/Identity/Account/Login
```

Ergebnis: HTTP `200` mit dem HTML des Login-Formulars. Damit ist die
gesamte Kette **Anwendung → Connection String → SQL-Server-Container →
Datenbank `aqms`** verifiziert.

### 23H.6 Praxisproblem: SQL-Login fehlgeschlagen (Fehler 18456)

*Symptom:* Beim ersten realen Datenbank-Schreibzugriff (Registrierung
eines Identity-Users über das Web-Formular) zeigte die Anwendung die
Production-Fehlerseite. Das Server-Log
(`journalctl -u aqms-web`) wies die Ursache aus:

```
Microsoft.Data.SqlClient.SqlException: Login failed for user 'sa'.
Error Number:18456,State:1,Class:14
```

*Ursache:* SQL-Server-Fehler 18456 bedeutet eine fehlgeschlagene
Authentifizierung — nicht ein Netzwerkproblem. Die Anwendung erreichte den
Container, aber das im Connection String der systemd-Override (§23H.3)
hinterlegte Passwort stimmte nicht mit dem `MSSQL_SA_PASSWORD` des
Containers überein.

*Lösung:* Das korrekte SA-Passwort ist jenes, mit dem das Einspielen des
Schema-Skripts via `sqlcmd` (§23F.2) erfolgreich war. Die Override-Datei
wurde mit `systemctl edit aqms-web` auf genau dieses Passwort korrigiert,
danach `daemon-reload` und `restart aqms-web`. Die anschließende
Registrierung lief fehlerfrei durch, der User wurde in `AspNetUsers`
persistiert.

*Lessons Learned:* Das SA-Passwort existiert an zwei Stellen — im
`docker run`-Kommando (§23F.1) und in der systemd-Override (§23H.3). Beide
müssen zeichengenau übereinstimmen. Besondere Vorsicht bei Sonderzeichen
(`$`, `!`, `` ` ``, `\`): Die Bash-Shell beim `docker run` interpretiert
diese, eine systemd-`Environment`-Zeile in doppelten Anführungszeichen
hingegen nicht — eine Quelle für unbemerkte Abweichungen. Der Fehler
tritt erst beim ersten DB-Zugriff auf, nicht beim Start des Dienstes, da
EF Core die Verbindung verzögert (lazy) öffnet.

### 23H.7 Folge-Deployments: Zwei-Schritt-Upload wegen Verzeichnisrechten

Nach §23H.1 gehört `/var/www/aqms` dem Service-User `aqms`. Der
SSH-Benutzer `deployuser` kann daher **nicht** mehr direkt per `scp` in das
Deploy-Verzeichnis schreiben — dies ist das vom alten VPS bekannte
Upload-Rechte-Problem (§28). Für Folge-Deployments (z.B. nach einer
Code-Änderung) wird daher ein Zwei-Schritt-Verfahren verwendet:

```powershell
# Entwicklungsmaschine: Upload in ein Zwischenverzeichnis im Home von deployuser
scp -r C:\temp\publish-aqms\* deployuser@203.0.113.10:/home/deployuser/deploy-tmp/
```

```bash
# Server: Dienst stoppen, Dateien ans Ziel kopieren, Rechte zurückgeben
sudo systemctl stop aqms-web
sudo cp -r /home/deployuser/deploy-tmp/* /var/www/aqms/
sudo chown -R aqms:aqms /var/www/aqms
sudo systemctl start aqms-web
```

*Begründung:* `deployuser` lädt in sein eigenes Home-Verzeichnis hoch (dort
besteht Schreibrecht), anschließend kopiert `sudo cp` die Dateien ins
Deploy-Verzeichnis und `chown` gibt sie wieder dem Service-User. Der
Dienst muss vor dem Kopieren gestoppt werden, da eine laufende .NET-
Anwendung ihre DLLs sperrt und ein Überschreiben im Betrieb scheitern
würde.

*Mögliche Vereinfachung für künftige Deployments:* `deployuser` in die
Gruppe `aqms` aufnehmen und `/var/www/aqms` mit Gruppen-Schreibrecht
versehen — dann genügt wieder ein direkter `scp`. Für den aktuellen Stand
wurde das Zwei-Schritt-Verfahren beibehalten, da es ohne zusätzliche
Rechtevergabe auskommt.

## 23I. Nginx Reverse Proxy

Der neue Server war frisch aufgesetzt — kein vorinstalliertes Nginx, keine
Bestandsdienste, Port 80 und 443 frei. Daher eine saubere Erstinstallation
(im Unterschied zum alten VPS, wo Nginx neben bestehende Server-Blöcke
eingefügt werden musste, §26).

```bash
sudo apt update && sudo apt install -y nginx
```

Server-Block `/etc/nginx/sites-available/aqms`:

```nginx
server {
    listen 80;
    server_name aqms.aqms.example.com;

    location / {
        proxy_pass         http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host $host;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

```bash
sudo ln -s /etc/nginx/sites-available/aqms /etc/nginx/sites-enabled/
sudo rm /etc/nginx/sites-enabled/default
sudo nginx -t && sudo systemctl reload nginx
```

*Begründung:* `proxy_pass` leitet alle Anfragen an die Kestrel-Anwendung
auf `localhost:5000` weiter. Die `X-Forwarded-*`-Header informieren die
ASP.NET-Anwendung über den tatsächlichen Host und das Protokoll des
ursprünglichen Aufrufs — andernfalls erzeugt sie fehlerhafte Redirect-URLs.
Das Entfernen des `default`-Server-Blocks schaltet die Nginx-Willkommens-
seite ab. `nginx -t` prüft die Syntax vor dem Reload.

Verifikation: lokaler `curl` mit `Host`-Header und externer Aufruf von
`http://aqms.aqms.example.com` lieferten beide HTTP `200` mit der AQMS-App.

## 23J. HTTPS mit Let's Encrypt

```bash
sudo apt install -y certbot python3-certbot-nginx
sudo certbot --nginx -d aqms.aqms.example.com
```

*Begründung:* Das `python3-certbot-nginx`-Plugin trägt den
`listen 443 ssl`-Block samt Zertifikatspfaden automatisch in die
Nginx-Konfiguration ein. Certbot verifiziert den Domain-Besitz per
HTTP-Challenge — möglich, weil der DNS-Eintrag (§23B) auf den Server zeigt
und Port 80 erreichbar war. Bei der interaktiven Abfrage wurde die
automatische HTTP-zu-HTTPS-Umleitung aktiviert.

Let's-Encrypt-Zertifikate sind 90 Tage gültig; Certbot installiert einen
systemd-Timer, der rechtzeitig automatisch erneuert
(`systemctl list-timers | grep certbot`).

**Verifikation:** `https://aqms.aqms.example.com` ist im Browser mit
gültigem Zertifikat erreichbar, HTTP wird auf HTTPS umgeleitet. Damit ist
der Backend-Stack auf dem neuen VPS vollständig:
Debian 13 → Docker / SQL Server → AQMS.Web (systemd) → Nginx → HTTPS.

## 23K. Erster Identity-User und Deaktivierung der Registrierung

### 23K.1 Ersten Benutzer anlegen und bestätigen

Über das Registrierungs-Formular der Anwendung wurde der erste
Identity-User angelegt. Da `Program.cs` mit
`RequireConfirmedAccount = true` konfiguriert ist und kein E-Mail-Versand
eingerichtet wurde, hatte der User zunächst `EmailConfirmed = 0` und
konnte sich nicht anmelden. Die Bestätigung erfolgte direkt in der
Datenbank:

```bash
docker exec -it aqms-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P '<PASSWORD>' -C -I -d aqms \
  -Q "UPDATE AspNetUsers SET EmailConfirmed = 1;"
```

Der `-I`-Schalter ist auch hier erforderlich (siehe Ergänzung in §22.7 —
`-I` gilt für jeden schreibenden `sqlcmd`-Zugriff auf die `aqms`-DB).
Nach der Bestätigung war der Login über
`https://aqms.aqms.example.com/Identity/Account/Login` erfolgreich — die
Authentifizierungskette ist damit End-to-End verifiziert.

### 23K.2 Öffentliche Registrierung deaktivieren

**Entscheidung:** Die öffentliche Selbst-Registrierung wird vollständig
deaktiviert.

**Begründung:** Das Backend ist über `https://aqms.aqms.example.com`
öffentlich erreichbar. Eine offene Registrierung würde es beliebigen
Dritten erlauben, Benutzerkonten im System anzulegen — für ein
Aquarium-Steuerungssystem mit Schaltbefehlen an reale 230V-Verbraucher
ein nicht vertretbares Risiko. Der Nutzerkreis ist klein und bekannt
(Betreiber, ggf. Prüfer); neue Benutzer werden administrativ angelegt
(vorerst direkt in der DB, später per Identity-Seeder, siehe §23K.3).

**Umsetzung:** Die Registrierungs-Seite stammt aus dem kompilierten
Paket `Microsoft.AspNetCore.Identity.UI` und liegt nicht als Datei im
Projekt vor — sie kann daher nicht einfach entfernt werden. Stattdessen
werden die Register-Routen in `Program.cs` nach `app.MapRazorPages()`
durch eigene Endpunkte überschrieben, die auf die Login-Seite umleiten:

```csharp
// Öffentliche Registrierung deaktiviert: Aufrufe der Register-Seite werden
// auf die Login-Seite umgeleitet. Neue Benutzer werden ausschließlich
// administrativ angelegt (vorerst direkt in der DB, später per Identity-Seeder).
app.MapGet("/Identity/Account/Register", () => Results.Redirect("/Identity/Account/Login"));
app.MapPost("/Identity/Account/Register", () => Results.Redirect("/Identity/Account/Login"));
```

*Begründung der Technik:* Ein explizit per `MapGet`/`MapPost`
registrierter Endpunkt ist spezifischer als die generische
Razor-Pages-Registrierung und gewinnt das Routing-Match. Sowohl der
direkte Seitenaufruf (`GET`) als auch das Absenden des Formulars (`POST`)
werden so abgefangen, ohne das Identity-UI-Template zu scaffolden oder zu
verändern — ein minimal-invasiver Eingriff. Verifiziert: Ein Aufruf von
`/Identity/Account/Register` öffnet die Login-Seite.

### 23K.3 IdentitySeeder als dauerhafte Lösung

Die in §23K.1 beschriebene manuelle Bestätigung per
`UPDATE AspNetUsers SET EmailConfirmed = 1` war ein Provisorium für den
ersten Login. Eine dauerhafte Lösung muss zwei Anforderungen erfüllen,
die im Manuell-Ansatz fehlen: die Rollen `Admin` und `User` müssen im
System existieren (für späteres `[Authorize(Roles = ...)]` an
Controllern, §33.2, Phase 3), und der Admin-Benutzer muss inklusive Rollen-
zuweisung **reproduzierbar** und **idempotent** beim Anwendungsstart
hergestellt werden — auf der lokalen Entwicklungsmaschine ebenso wie auf
dem VPS, ohne manuelle Eingriffe.

Realisiert als statische Klasse `IdentitySeeder` im Namespace
`AQMS.Web.Data` mit einer einzigen Methode `SeedAsync`, die beim App-
Start aufgerufen wird.

**Warum nicht über `HasData` in den Migrations?** Das Passwort-Hashing
eines Identity-Users erfolgt durch den `UserManager` zur Laufzeit (BCrypt-
artiger Hash, Salt, konfigurierbare Iterationen). `HasData` schreibt nur
statische Spaltenwerte und kann den Hash-Vorgang nicht aufrufen — ein
über `HasData` gesetzter Passwort-Wert wäre entweder ungehasht oder ein
hartkodierter Hash-String, beides unbrauchbar. Identity-Seeding gehört
daher zwingend in die App-Startup-Phase, nicht in die Migration.

**Warum überhaupt eine Seeder-Klasse statt Inline-Code in `Program.cs`?**
Trennung von Verantwortlichkeiten und Testbarkeit. `Program.cs` ruft die
Methode nur auf; die Logik (Rollen anlegen, User suchen/anlegen, Rollen
zuweisen) ist isoliert und ließe sich künftig auch separat testen oder
um weitere Seed-Aufgaben erweitern (z.B. Geräte-Defaults), ohne
`Program.cs` aufzublähen.

**Aufbau der Methode** (`AQMS.Web/Data/IdentitySeeder.cs`):

```csharp
public static async Task SeedAsync(IServiceProvider services)
{
    var roleHandler = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userHandler = services.GetRequiredService<UserManager<IdentityUser>>();
    var config      = services.GetRequiredService<IConfiguration>();

    // 1. Rollen idempotent anlegen
    foreach (var role in new[] { AdminRole, UserRole })
    {
        if (!await roleHandler.RoleExistsAsync(role))
            await roleHandler.CreateAsync(new IdentityRole(role));
    }

    // 2. Admin-Zugangsdaten aus Konfiguration; ohne Daten kein Admin-Seed
    var adminEmail = config["AdminBenutzer:Email"];
    var adminPwd   = config["AdminBenutzer:Passwort"];
    if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPwd))
        return;

    // 3. Admin-Benutzer finden oder neu anlegen (mit EmailConfirmed = true)
    var adminUser = await userHandler.FindByEmailAsync(adminEmail);
    if (adminUser is null)
    {
        adminUser = new IdentityUser
        {
            UserName       = adminEmail,
            Email          = adminEmail,
            EmailConfirmed = true
        };
        await userHandler.CreateAsync(adminUser, adminPwd);
    }

    // 4. Admin-Rolle zuweisen, falls noch nicht gesetzt
    if (!await userHandler.IsInRoleAsync(adminUser, AdminRole))
        await userHandler.AddToRoleAsync(adminUser, AdminRole);
}
```

Die Rollennamen sind als `public const string` in der Klasse abgelegt
(`AdminRole`, `UserRole`) — dieselben Konstanten werden später bei
`[Authorize(Roles = IdentitySeeder.AdminRole)]` in Controllern
referenziert. Vermeidet „magic strings" und wird vom Compiler
namensgeprüft.

**Idempotenz als Designprinzip:** Jeder schreibende Schritt prüft vorher
(`RoleExistsAsync`, `FindByEmailAsync`, `IsInRoleAsync`), ob die Aktion
nötig ist. Der Seeder läuft bei *jedem* App-Start mit; mehrfaches
Ausführen ist garantiert unschädlich. Drei Lebenszustände werden
abgedeckt:

| Lebenszustand | Verhalten |
|---|---|
| Erststart, leere DB | Rollen anlegen, User anlegen, Rolle zuweisen |
| User existiert, Rolle fehlt (Realfall VPS) | User finden, Rolle ergänzen |
| Alles bereits vorhanden (Folgestarts) | Alle Prüfungen `true`, keine Schreibvorgänge |

**Integration in `Program.cs`** — zwei Änderungen:

```csharp
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()                  // NEU: Rollen-Support aktivieren
    .AddEntityFrameworkStores<AqmsDbContext>();
```

```csharp
// Beim Anwendungsstart: Rollen und Admin-User idempotent sicherstellen.
// Eigener DI-Scope nötig, da UserManager/RoleManager als scoped Services
// registriert sind und nicht direkt vom App-Root aufgelöst werden können.
using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();
```

`.AddRoles<IdentityRole>()` registriert den `RoleManager<IdentityRole>`
im DI-Container — ohne diese Zeile würde `GetRequiredService` im Seeder
zur Laufzeit fehlschlagen. Die Position **vor** `AddEntityFrameworkStores`
ist gewollt: Letzteres konfiguriert die Speicherung für alles vorher
Registrierte.

Der `using`-Block erzeugt einen künstlichen, kurzlebigen DI-Scope. ASP.NET
Core verweigert das direkte Auflösen scoped Services am App-Root, weil
diese sonst die gesamte App-Laufzeit am Root hingen und Memory-Leaks
verursachten. Mit `app.Services.CreateScope()` öffnet sich ein
abgegrenzter Bereich, der nach dem `using` automatisch via `Dispose()`
entsorgt wird.

**Konfigurations-Setup** — die Admin-Zugangsdaten kommen
umgebungsabhängig:

- **Lokal (Entwicklung):** per User Secrets im Projekt `AQMS.Web` —
  `dotnet user-secrets set "AdminBenutzer:Email" "..."` und
  `dotnet user-secrets set "AdminBenutzer:Passwort" "..."`. Außerhalb des
  Repos abgelegt, konsistent mit der bestehenden Connection-String-
  Verwaltung (§11).
- **Produktion (VPS):** als `Environment=`-Zeilen in der systemd-Override
  von `aqms-web` (§23H.3 erweitert):

  ```ini
  Environment="AdminBenutzer__Email=admin@aqms.example.com"
  Environment="AdminBenutzer__Passwort=<PASSWORD>"
  ```

  Schlüsselübersetzung: Im C#-Code `AdminBenutzer:Email` mit Doppelpunkt,
  in Umgebungsvariablen `AdminBenutzer__Email` mit doppeltem
  Unterstrich (Umgebungsvariablen erlauben keinen Doppelpunkt).
  ASP.NET Core mappt beides auf denselben Konfigurationspfad.

**Verifikation:**

- Lokal (LocalDB, leere DB): Nach `F5`-Start des `AQMS.Web` zeigt der
  SQL Server Object Explorer in `AspNetRoles` die zwei Einträge `Admin`
  und `User`, in `AspNetUsers` den Admin mit `EmailConfirmed = 1`, und in
  `AspNetUserRoles` die Zuweisung. Login mit dem konfigurierten Passwort
  erfolgreich.
- VPS (existierender User aus §23K.1, ohne Rolle): Nach Deployment
  + systemd-Override-Erweiterung + Service-Restart bestätigt die
  Datenbank-Abfrage zwei Rollen und `admin@aqms.example.com` mit Rolle
  `Admin`. Damit ist das Provisorium aus §23K.1 obsolet — die Bestätigung
  und Rollenzuweisung erfolgen jetzt reproduzierbar beim App-Start.

### 23K.4 Zweites Konto in der Rolle `User` und Admin als Pflichtkonto

**Anlass.** Der Seeder legte seit §23K.3 beide Rollen an, vergeben wurde aber
nur `Admin`. Die Rolle `User` existierte im System, ohne jemals einem Konto
zugeordnet zu sein. Aufgefallen ist das bei der Vorbereitung des
Rollentrennungs-Tests (§23S.4): ohne ein Konto in dieser Rolle ist der Test
nicht durchführbar. Damit war auch Hauptziel 6 des Projektauftrags
(Zugriffsschutz mit den Rollen `Admin` und `User`) bis dahin nur zur Hälfte
belegt.

**Umsetzung.** Der Anlegevorgang wandert aus `SeedAsync` in eine private
Hilfsmethode `EnsureUserAsync(userHandler, email, password, role)`, die zweimal
aufgerufen wird. Die Zugangsdaten des zweiten Kontos kommen aus der neuen
Konfigurationssektion `StandardBenutzer` und folgen damit demselben Muster wie
`AdminBenutzer` — User Secrets lokal, systemd-Override in Produktion (§11.7).

```csharp
await EnsureUserAsync(userHandler, adminEmail, adminPwd, AdminRole);   // Pflicht
await EnsureUserAsync(userHandler,
    config["StandardBenutzer:Email"],
    config["StandardBenutzer:Passwort"], UserRole);                    // optional
```

**Warum nicht einmalig direkt in der Datenbank?** Ein per SQL angelegtes Konto
existiert nur auf dem einen System, auf dem der Befehl abgesetzt wurde. Genau
dieses Provisorium war nach §23K.1 schon einmal der Ausgangspunkt und wurde in
§23K.3 aus demselben Grund abgelöst. Ein zweites Mal denselben Weg zu gehen
hätte die Reproduzierbarkeit wieder aufgegeben, die der Seeder herstellt.

**Warum ist der Admin Pflicht und der Standardbenutzer optional?** Ohne
Administrator ist die Anwendung unbenutzbar, weil die öffentliche Registrierung
nach §23K.2 deaktiviert ist und sich folglich niemand anmelden kann. Die
Anwendung würde trotzdem starten und auf Anfragen antworten — der Fehler fiele
erst beim ersten Anmeldeversuch auf. Fehlt die Sektion `AdminBenutzer`, wird
deshalb eine `InvalidOperationException` geworfen und der Start bricht ab.
Dasselbe gilt, wenn das Konto konfiguriert ist, sich aber nicht anlegen lässt
(etwa weil das Passwort die Identity-Richtlinie verletzt): `EnsureUserAsync`
liefert dafür `bool` zurück, der Aufrufer entscheidet je Konto, was ein
Fehlschlag bedeutet.

Die Unterscheidung verläuft damit **nicht** zwischen den beiden Konten, sondern
zwischen *nicht konfiguriert* und *konfiguriert und trotzdem nicht anlegbar*.
Der zweite Fall ist immer ein Konfigurationsfehler.

**Korrektur am Bestand.** Das Ergebnis von `CreateAsync` wurde bisher verworfen.
Lehnte die Passwortrichtlinie das Passwort ab, stand das Konto nicht in der
Datenbank, und der folgende `AddToRoleAsync`-Aufruf zielte auf ein nicht
existierendes Konto. Die Prüfung auf `result.Succeeded` schließt diese Lücke.

**Kopplung an `Program.cs`.** Das Flag `EmailConfirmed = true` ist zwingend, weil
Identity mit `RequireConfirmedAccount = true` konfiguriert ist und kein
E-Mail-Versand eingerichtet wurde (§23K.1). Ohne das Flag scheitert der Login —
beim Prüfkonto hätte der Sicherheitstest dann eine fehlgeschlagene Anmeldung
statt einer greifenden Rollenprüfung gemessen.

**Bewusster Preis.** Auf dem VPS läuft der Dienst mit `Restart=always`. Eine
fehlende Admin-Konfiguration führt dort zu einer Neustartschleife statt zu einem
stillen Fehlstart. Das ist lauter, aber sichtbar: `systemctl status aqms-web`
zeigt den fehlgeschlagenen Start samt Meldung, während die stille Variante nichts
anzeigt. Der Zusammenhang zu §23L.8, wo systemd eine unvollständige
`Environment=`-Zeile kommentarlos angenommen hat, ist unmittelbar — dieselbe
Klasse von Fehler, diesmal aber laut.

**Verifikation.** Beim Testlauf gegen die leere Datenbank (§23S.3) existierte zum
Zeitpunkt der Migrationsbefehle die Tabelle `AspNetRoles` noch nicht. Wäre der
Seeder bei einem `dotnet ef`-Aufruf mitgelaufen, hätte `RoleExistsAsync` mit
einem SQL-Fehler abgebrochen. Das geschah nicht — die EF-Werkzeuge brechen den
Host vor dem Seeder-Aufruf ab, die neue Ausnahme kann Migrationsbefehle also
nicht beeinträchtigen. Nach dem anschließenden App-Start lieferte die Abfrage
über `AspNetUsers`/`AspNetUserRoles` zwei Zeilen (`Admin`, `User`), beide Logins
im Browser erfolgreich.

**Veralteter Kommentar entfernt.** Die Anmerkung in `Program.cs` zur
deaktivierten Registrierung sprach davon, neue Benutzer würden „vorerst direkt in
der DB, später per Identity-Seeder" angelegt. Der zweite Teil ist damit
eingelöst.

---

## 23L. API-Key-Middleware

### 23L.1 Anlass und Zielbild

Der **IdentitySeeder** (§23K.3) hat das Cookie-basierte Identity-Login auf
ein produktives Fundament gestellt — funktioniert aber nur für Clients
mit Cookie-Verwaltung, also Browser. Der **Worker auf dem Pi** (Phase 2)
ist ein reiner HTTP-Client ohne Cookie-Speicher; er kann sich nicht über
ein Login-Formular authentifizieren. Für die `/api/...`-Endpunkte braucht
es daher eine **zweite, parallele Authentifizierungsmethode**:
ein gemeinsam vereinbarter API-Schlüssel im HTTP-Header.

Die `ApiKeyMiddleware` schaltet sich in die ASP.NET-Core-Pipeline ein,
prüft bei jeder Anfrage auf `/api/...` den Header `X-API-Key`, vergleicht
ihn konstantzeitig gegen den in der Konfiguration hinterlegten Wert und
gibt im Fehlerfall `401 Unauthorized` zurück, **ohne** die Anfrage an
nachgelagerte Stationen weiterzuleiten. Anfragen außerhalb von `/api/...`
(also HTML-Seiten, Login, statische Dateien) sieht die Middleware zwar,
reicht sie aber unverändert durch — die Identity-Cookie-Auth bleibt
für Browser unverändert nutzbar.

### 23L.2 Architekturentscheidungen

**Ein API-Schlüssel, nicht mehrere.** Eine zukünftige Verwaltung mehrerer
Schlüssel (DB-Tabelle `ApiKeys`, einzeln widerrufbar, verschiedene Worker)
wäre möglich, aber für AQMS mit einem einzigen Worker auf dem Pi
überdimensioniert. *YAGNI* (You aren't gonna need it) — sollte später ein
zweiter Worker dazukommen, ist die Erweiterung um eine Tabelle ein klar
abgegrenztes Refactoring.

**Header `X-API-Key`, nicht Query-String.** Query-String-Parameter landen
in Webserver-Logs und Browser-History und können dort eingesehen werden.
HTTP-Header sind diskreter. `X-API-Key` ist Pseudo-Standard (das `X-`-
Präfix gilt offiziell als veraltet, ist aber praktisch verbreitet) — der
Header-Name ist im Code als private `const string` gehalten, nicht als
„magic string" im Vergleich verstreut.

**Konstantzeitiger String-Vergleich (`CryptographicOperations.FixedTimeEquals`).**
Ein gewöhnlicher `!=`-Vergleich auf Strings bricht beim ersten unterschied-
lichen Zeichen ab; ein Angreifer könnte aus der gemessenen Antwortzeit
zeichenweise auf den korrekten Schlüssel zurückschließen
(*Timing-Attack*). `FixedTimeEquals` benötigt für jeden Vergleich dieselbe
Zeit, unabhängig vom Inhalt. Für AQMS im LAN ist das Risiko praktisch
gering, aber der korrekte Vergleich kostet eine einzige Zeile mehr und
ist Diplomarbeit-relevantes Detail im Bereich „Security-Awareness".

**Pipeline-Position möglichst früh (direkt nach `UseHttpsRedirection`).**
Die Middleware ist eine eigene Authentifizierungsmethode, parallel zur
Cookie-basierten Identity. Sie hat nichts mit dem `User`-Objekt zu tun,
das Identity setzt. Je früher in der Pipeline, desto weniger nutzlose
Arbeit vor einer Abweisung. Der Pfad-Filter `if (!StartsWithSegments("/api"))`
*innerhalb* der Middleware schützt Browser-Routen — Position vorn und
Pfad-Filter innen ergänzen sich.

### 23L.3 Klassen-Aufbau

`AQMS.Web/Middleware/ApiKeyMiddleware.cs`:

```csharp
public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeader = "X-API-Key";

    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IConfiguration config)
    {
        // 1. Pfad-Filter: nur /api/...-Anfragen prüfen
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await _next(context);
            return;
        }

        // 2. Header und Konfigurationswert lesen
        string? requestApiKey  = context.Request.Headers[ApiKeyHeader];
        string? expectedApiKey = config["ApiKey"];

        // 3. Null-/Leer-Check: fehlender Header ODER fehlende Konfiguration
        if (string.IsNullOrWhiteSpace(requestApiKey)
            || string.IsNullOrWhiteSpace(expectedApiKey))
        {
            context.Response.StatusCode = 401;
            return;
        }

        // 4. Konstantzeitiger Vergleich gegen Timing-Attacks
        var requestBytes  = System.Text.Encoding.UTF8.GetBytes(requestApiKey);
        var expectedBytes = System.Text.Encoding.UTF8.GetBytes(expectedApiKey);
        if (!CryptographicOperations.FixedTimeEquals(requestBytes, expectedBytes))
        {
            context.Response.StatusCode = 401;
            return;
        }

        // 5. Alle Prüfungen bestanden: an die nächste Pipeline-Station
        await _next(context);
    }
}
```

**Klasse, nicht statisch:** Eine Middleware hält Zustand — das Feld
`_next` enthält die Referenz auf die nächste Pipeline-Station, gespeichert
beim App-Start. Eine `static class` hätte keine Instanz und damit keinen
Zustand pro Objekt.

**`InvokeAsync` mit Method Injection:** ASP.NET Core sucht per Konvention
nach einer Methode genau dieses Namens mit `HttpContext` als erstem
Parameter. Weitere Parameter wie `IConfiguration` werden bei jedem Request
vom DI-Container automatisch eingefügt — kein expliziter
`GetRequiredService<>`-Aufruf nötig. Das nutzt den Pro-Request-Scope, den
ASP.NET ohnehin öffnet; anders als beim IdentitySeeder (§23K.3), der
mangels Request einen Scope manuell erzeugen musste.

### 23L.4 Integration in `Program.cs`

```csharp
app.UseHttpsRedirection();

// API-Key-Schutz möglichst früh: ungültige API-Anfragen werden abgewiesen,
// bevor Routing, Authentication und Authorization angestoßen werden.
app.UseMiddleware<ApiKeyMiddleware>();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
```

> **Nebenbefund während der Integration:** Beim Einbau fiel auf, dass
> `app.UseAuthentication();` in der Pipeline bisher fehlte —
> `UseAuthorization` allein reicht nicht; ohne `UseAuthentication` ist das
> `User`-Objekt bei nachgelagerten Anfragen leer, und spätere
> `[Authorize]`-Attribute (§33.2, Phase 3) würden nicht greifen. Dass das
> Identity-Login bisher dennoch funktionierte, lag an der Identity-UI, die
> ihre eigenen Endpunkte intern bedient — das hätte sich aber spätestens
> beim ersten geschützten Controller gerächt. Die Zeile ist mit aufgenommen.

### 23L.5 Konfiguration des Schlüssels

Der API-Key ist ein Secret und folgt der §11-Strategie:

- **Lokal (Entwicklung):** als User Secret pro Gerät —
  `dotnet user-secrets set "ApiKey" "..." --project AQMS.Web`.
  Pro Umgebung ein eigener Wert (Firmen-Gerät, Privat-Gerät),
  generiert z.B. mit
  `[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Maximum 256 }))`.
- **Produktion (VPS):** als zusätzliche `Environment=`-Zeile in der
  systemd-Override:
  ```ini
  Environment="ApiKey=<wert>"
  ```
  (Keine `__`-Schreibweise wie bei `ConnectionStrings__DefaultConnection`
  oder `AdminBenutzer__Email`, weil `ApiKey` ein flacher Top-Level-
  Schlüssel ohne Sektion ist.)
- **Pi-Worker** (Phase 2): bekommt denselben Wert wie der VPS, sonst
  scheitert die Authentifizierung. Wird dort ebenfalls als
  Umgebungsvariable / Konfigurationsdatei abgelegt — kein hartkodierter
  Wert im Worker-Code.

### 23L.6 Verifikation (lokal)

Für isolierte Tests der Middleware wurde ein minimal-API-Endpunkt
`/api/ping` in `Program.cs` eingefügt — gibt bei erfolgreicher Auth
`"pong"` mit Status 200 zurück. Smoke-Test-Endpunkt, der bei künftigen
Deployments auf den VPS ein schneller Funktionsbeweis ist.

Drei Test-Szenarien wurden auf dem Firmen-Gerät ausgeführt:

| Test | Header | Erwartung | Ergebnis |
|---|---|---|---|
| 1 | kein `X-API-Key` | 401 | ✓ 401 |
| 2 | `X-API-Key: falsch` | 401 | ✓ 401 |
| 3 | `X-API-Key: <korrekt>` | 200 + "pong" | ✓ 200 |

Zusätzlich verifiziert:
- Startseite `/` lädt unberührt (Pfad-Filter funktioniert),
- Identity-Login wie zuvor — `UseAuthentication` greift, keine Regression.

Test-Aufruf in PowerShell (mit `Invoke-WebRequest`, da PowerShells
`curl`-Alias den Linux-Argumenten nicht entspricht):

```powershell
Invoke-WebRequest -Uri "https://localhost:<port>/api/ping" `
    -Headers @{ "X-API-Key" = "<korrekter-key>" } -SkipCertificateCheck
```

### 23L.7 Status pro Umgebung

| Umgebung | `ApiKey`-Wert | Middleware aktiv | Verifikation |
|---|---|---|---|
| Firmen-Gerät | gesetzt | ✓ | drei Test-Szenarien bestanden (2026-05-29) |
| Privat-Gerät | gesetzt (anderer Wert) | ✓ | Middleware-Verifikation nachzuholen — bisher nur Login geprüft |
| VPS (Produktion) | gesetzt (eigener Wert, separat von Entwicklung) | ✓ | drei Test-Szenarien bestanden (2026-06-01) — siehe §23M.5 |

Mit dem VPS-Wert ist der `ApiKey` in **allen drei Umgebungen** gesetzt
und folgt durchgängig dem §11.7-Prinzip „pro Umgebung anderer Wert".
Der VPS-Wert wird später identisch beim Pi-Worker (Phase 2) hinterlegt,
sodass Worker und API über denselben Schlüssel kommunizieren.

### 23L.8 Praxisproblem: stiller systemd-Tippfehler beim ApiKey-Override

**Symptom.** Beim ersten Test des `MeasurementsController` gegen die
Production-API (§23M.5, VPS-Verifikation) wurden alle drei Test-Szenarien
mit `401 Unauthorized` beantwortet — auch der POST und der GET mit
korrekt gesetztem `X-API-Key`-Header. Der Negativ-Test (kein Header → 401)
funktionierte erwartungsgemäß, d.h. die Middleware **lief grundsätzlich**.
Der Service-Status war `active (running)`, keine Stack-Traces in
`journalctl`, der Health-Check `GET https://aqms.aqms.example.com/` (HTML-
Startseite) antwortete mit `200`.

**Ursache.** Die systemd-Override-Datei
`/etc/systemd/system/aqms-web.service.d/override.conf` enthielt zwar
einen Eintrag für den API-Schlüssel, aber syntaktisch unvollständig — der
Konfigurationsschlüssel-Name `ApiKey=` war im Wert-String vergessen worden.
Statt der korrekten Form

```ini
Environment="ApiKey=<wert>"
```

stand dort nur

```ini
Environment="<wert>"
```

Konsequenz: systemd akzeptiert die Zeile syntaktisch (es ist eine gültige
`Environment=`-Direktive), setzt aber **keine** Umgebungsvariable namens
`ApiKey`. Die App liest beim Start `config["ApiKey"]` → `null`. In der
`ApiKeyMiddleware` greift die Null-Prüfung (§23L.3), und jede `/api/...`-
Anfrage wird mit `401` abgewiesen — unabhängig davon, ob der Client einen
gültigen Header gesendet hat. Das Verhalten ist **identisch** zum Fall
„kein Header gesetzt", weshalb der Negativ-Test irreführend wie geplant
durchging und damit zunächst Vertrauen in das Setup gab.

**Lösung.** Override-Datei korrekt editiert:

```bash
sudo systemctl edit aqms-web
# Zeile auf vollständige Form bringen:
#   Environment="ApiKey=<wert>"
sudo systemctl daemon-reload
sudo systemctl restart aqms-web
```

Nach dem Restart wurden alle drei Test-Szenarien wie erwartet beantwortet
(401 ohne Header, 201 für POST, 200 für GET).

**Diagnose-Werkzeug.** Der schnellste Weg zum Nachweis, welche
Umgebungsvariablen ein laufender systemd-Service tatsächlich sieht:

```bash
sudo systemctl show aqms-web --property=Environment
```

Gibt eine Zeile `Environment=ApiKey=... ConnectionStrings__DefaultConnection=... AdminBenutzer__Email=... ...`
zurück — alle aktiven Variablen, leerzeichengetrennt. Fehlt `ApiKey=` in
der Liste, ist die Override fehlerhaft. Diese Diagnose-Zeile **vor** dem
ersten Test ausführen erspart das hier dokumentierte Verwirrungs-Manöver.

**Lessons Learned.**

1. *systemd-`Environment=`-Direktiven schweigen bei semantischer
   Schlamperei.* Solange die Anführungszeichen geschlossen sind und ein
   Zeichen-String drinsteht, ist die Zeile aus systemd-Sicht „in Ordnung".
   Dass die Variable inhaltslos ist (Wert ohne `NAME=` davor), wird **nicht**
   gemeldet — weder beim Edit, noch beim `daemon-reload`, noch beim
   `restart`. Das ist ein Klassiker.
2. *Negativ-Tests können trügerisch sein.* Der „kein Header → 401"-Test
   sah aus wie „Middleware funktioniert", war aber in Wahrheit „App liest
   nie einen erwarteten Schlüssel". Bei jedem neuen Production-Setup
   gehört der **Positiv**-Test als Erstes — wenn er fehlschlägt, weiß man
   sofort, dass etwas mit der Konfiguration nicht stimmt.
3. *Übersetzung Schlüsselpfad → Environment-Variable.* ASP.NET-Core-
   Konfiguration nutzt für verschachtelte Pfade den doppelten Unterstrich
   (`ConnectionStrings:DefaultConnection` → `ConnectionStrings__DefaultConnection`).
   Bei flachen Top-Level-Schlüsseln wie `ApiKey` ist es einfach der
   Schlüsselname selbst. Dieser Unterschied ist im §23L.5-Block beschrieben
   und gilt unverändert.

---

## 23M. MeasurementsController

### 23M.1 Anlass und Zielbild

Mit der API-Key-Middleware (§23L) ist die Schutzschicht für `/api/...`-
Routen vorhanden, hat aber bisher keinen Adressaten — es existierte kein
echter API-Endpunkt. Der `MeasurementsController` ist der erste
produktive API-Controller in AQMS. Er trägt den **zentralen Schreibweg**
des Systems (Worker → API → DB) und den **Leseweg** für die spätere
Dashboard-UI (UI → API → DB).

Schreibweg konkret: Der Worker auf dem Pi liest Sensoren bzw. fragt
Shellys ab und schickt jeden Messwert per `POST /api/measurements` an
das Backend. Backend übersetzt die fachlichen Identifier (kebab-case-
DeviceIdentifier, Typ-Name) in DB-Fremdschlüssel und persistiert.

Leseweg konkret: `GET /api/measurements` mit Query-Parametern liefert die
letzten N Messwerte eines Geräts vom angegebenen Typ — zeitlich absteigend
sortiert. Form: schmaler Response-DTO für JSON, ohne Navigation-Properties.

### 23M.2 Architekturentscheidungen

**Kein Voll-CRUD — bewusste Reduktion auf POST und GET.**
Lehrbuch-REST-Beispiele führen für jede Entität alle vier CRUD-
Operationen vor. Bei Messwerten ergibt das fachlich keinen Sinn:
Messwerte sind unveränderliche historische Fakten. Update („wir
korrigieren nachträglich die Temperatur") entwertet die Messreihe,
Delete bricht historische Auswertungen. Daher **append-only-Modell** —
nur POST (anlegen) und GET (lesen). Für `DeviceCommand` (§33.2, Phase 3,
CommandsController) wird das anders aussehen, weil dort ein Update für
Status-Übergänge (`Pending` → `Executed` / `Failed`) zwingend ist.

**`[ApiController]` + `[Route]` als Klassen-Attribute.**
`[ApiController]` aktiviert automatische Modell-Validierung
(ungültiger Request-Body → automatisch 400 Bad Request, ohne dass die
Action das selbst prüfen muss) und sorgt dafür, dass komplexe Parameter-
Typen automatisch dem Body und einfache Typen automatisch dem
Query-String zugeordnet werden. `[Route("api/measurements")]` legt den
Basispfad fest; die Action-Methoden hängen ihre HTTP-Verben (`[HttpPost]`,
`[HttpGet]`) an die Klasse an, statt eigene Pfade zu definieren.

**Konstruktor-Injektion für den DbContext.**
Dritter DI-Stil im Projekt nach Service Locator (§23K.3, IdentitySeeder)
und Method Injection (§23L.3, ApiKeyMiddleware). Der `AqmsDbContext`
wird per Konstruktor-Parameter angefordert und in einem `private readonly`-
Feld abgelegt. Der DI-Container erkennt dies und liefert pro Request einen
frischen DbContext (scoped lifetime). Konstruktor-Injektion ist der
Default-Stil für Controller — explizite Verkabelung über die Parameter-
Liste, gut testbar.

**Eingehende und ausgehende DTOs statt direkter Entity-Verwendung.**
Es wäre technisch möglich, die `Measurement`-Entity als POST-Eingabe und
als GET-Ausgabe direkt zu verwenden. Aus drei Gründen wird das bewusst
nicht getan:

1. *Eingabe-Kontrolle:* Eine Entity transportiert Felder wie `Id`, die
   von der DB vergeben werden und vom Client nicht gesendet werden
   dürfen. Ein DTO verhindert das per Definition.
2. *JSON-Zyklus-Problem:* Entity-Klassen tragen Navigation-Properties
   (`Measurement.Device.Measurements.Device...`). Beim JSON-Serialisieren
   führt das zu Endlos-Verweisen und einer `JsonException` mit
   *"object cycle detected"* — siehe §23M.6 (Praxisproblem).
3. *Kopplung API↔DB:* Eine Änderung am DB-Modell würde die API-Schnittstelle
   sofort mitziehen. DTOs entkoppeln die externe Vertrags-Form vom internen
   Speicher-Modell.

Daher zwei eigene Klassen: `CreateMeasurementDto` für die POST-Eingabe
(vier Felder), `MeasurementResponseDto` für die GET-Ausgabe (drei Felder).

**Globale Exception-Behandlung statt try/catch in Actions.**
ASP.NET Core hat in `Program.cs` bereits `UseExceptionHandler("/Home/Error")`
(Production) bzw. die Developer-Exception-Page (Development) als
zentrale Fehler-Anlaufstelle. Ein try/catch in den Action-Methoden um
`SaveChangesAsync` würde diese Schicht duplizieren, ohne Mehrwert (es sei
denn, es wird gezielt geloggt — kommt mit dem Service-Layer in §33.2 Phase 3).
Daher: Exceptions fliegen hoch, globaler Handler übernimmt.

**400 Bad Request für unbekannte fachliche Identifier.**
Schickt der Client einen `DeviceIdentifier` oder `MeasurementTypeName`,
den die DB nicht kennt, antwortet der Controller mit
`BadRequest("Unknown Device-Identifier: ...")`. **Nicht** mit 404: 400
signalisiert „die Anfrage ist syntaktisch ok, aber inhaltlich
unbrauchbar"; 404 wäre für „die Resource selbst gibt's nicht"
(z.B. `/api/measurements/99999` — eine spezifische Messung). Die
Fehlermeldung enthält den falschen Wert zur Diagnose im Worker-Log.

### 23M.3 Klassen-Aufbau

**`AQMS.Web/Dtos/CreateMeasurementDto.cs`** — POST-Eingabe:

```csharp
public class CreateMeasurementDto
{
    public string DeviceIdentifier    { get; set; } = string.Empty;
    public string MeasurementTypeName { get; set; } = string.Empty;
    public double   Value     { get; set; }
    public DateTime Timestamp { get; set; }
}
```

**`AQMS.Web/Dtos/MeasurementResponseDto.cs`** — GET-Antwortform pro
Messwert, schmal gehalten:

```csharp
public class MeasurementResponseDto
{
    public DateTime Timestamp { get; set; }
    public double   Value     { get; set; }
    public string   Unit      { get; set; } = string.Empty;
}
```

**`AQMS.Web/Controllers/MeasurementsController.cs`** — der Controller:

```csharp
[ApiController]
[Route("api/measurements")]
public class MeasurementsController : ControllerBase
{
    private readonly AqmsDbContext _db;

    public MeasurementsController(AqmsDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateMeasurementDto dto)
    {
        var device = await _db.Devices
            .FirstOrDefaultAsync(d => d.DeviceIdentifier == dto.DeviceIdentifier);
        if (device is null)
            return BadRequest($"Unknown Device-Identifier: {dto.DeviceIdentifier}");

        var measurementType = await _db.MeasurementTypes
            .FirstOrDefaultAsync(m => m.Name == dto.MeasurementTypeName);
        if (measurementType is null)
            return BadRequest($"Unknown Measurement-Type: {dto.MeasurementTypeName}");

        var measurement = new Measurement
        {
            DeviceId          = device.Id,
            MeasurementTypeId = measurementType.Id,
            Value             = dto.Value,
            Timestamp         = dto.Timestamp
        };

        _db.Measurements.Add(measurement);
        await _db.SaveChangesAsync();

        return StatusCode(201);
    }

    [HttpGet]
    public async Task<ActionResult> GetByDevice(
        string deviceIdentifier, string typeName, int limit = 100)
    {
        var device = await _db.Devices
            .FirstOrDefaultAsync(d => d.DeviceIdentifier == deviceIdentifier);
        if (device is null)
            return BadRequest($"Unknown Device-Identifier: {deviceIdentifier}");

        var measurementType = await _db.MeasurementTypes
            .FirstOrDefaultAsync(t => t.Name == typeName);
        if (measurementType is null)
            return BadRequest($"Unknown Measurement-Type: {typeName}");

        var measurements = await _db.Measurements
            .Where(m => m.DeviceId == device.Id
                     && m.MeasurementTypeId == measurementType.Id)
            .OrderByDescending(m => m.Timestamp)
            .Take(limit)
            .Select(m => new MeasurementResponseDto
            {
                Timestamp = m.Timestamp,
                Value     = m.Value,
                Unit      = m.MeasurementType.Unit
            })
            .ToListAsync();

        return Ok(measurements);
    }
}
```

Strukturmerkmale:

- **`ControllerBase` statt `Controller`:** ControllerBase ist die schlanke
  Basis ohne View-Unterstützung — passend für reine JSON-APIs. `Controller`
  (mit View-Renderung) ist für die MVC-Seite (HomeController etc.).
- **`ActionResult` als Rückgabetyp:** Kapselt jede mögliche HTTP-Antwort.
  `BadRequest(...)`, `Ok(...)`, `StatusCode(201)` sind Helfermethoden, die
  fertig konfigurierte `ActionResult`-Objekte erzeugen.
- **`[FromBody]` explizit angegeben:** Bei `[ApiController]` würde ASP.NET
  Core den DTO-Parameter auch ohne Attribut korrekt aus dem Body lesen.
  Die explizite Angabe ist Diplomarbeit-konform — sichtbar, woher der
  Parameter kommt.
- **LINQ-Kette mit Projektion:** Der `.Select(...)`-Bauklotz übersetzt
  EF Core in ein effizientes SQL `SELECT m.Timestamp, m.Value, mt.Unit`
  mit JOIN auf `MeasurementTypes` — nur die drei nötigen Spalten werden
  geholt, keine ganze Entity-Hierarchie. Performance-relevant bei
  wachsenden Tabellen.

### 23M.4 Integration in `Program.cs`

Keine eigenen Anpassungen nötig — der Controller wird automatisch über
`AddControllersWithViews()` (bereits beim Solution-Aufbau gesetzt) erkannt
und über `MapControllerRoute(...)` in die Pipeline aufgenommen. Attribute-
Routing greift unabhängig vom MVC-Routing-Pattern, sodass `/api/measurements`
trotz `{controller=Home}/{action=Index}/{id?}` als Default sauber gefunden
wird.

### 23M.5 Verifikation

#### Lokal (Firmen-Gerät)

POST-Test:

```powershell
$body = @{
    deviceIdentifier    = "raspberry-pi"
    measurementTypeName = "Temperature"
    value               = 22.5
    timestamp           = "2026-05-31T11:00:00Z"
} | ConvertTo-Json

Invoke-WebRequest -Uri "https://localhost:<port>/api/measurements" `
    -Method POST `
    -Headers @{ "X-API-Key" = "<lokaler-key>" } `
    -ContentType "application/json" `
    -Body $body `
    -SkipCertificateCheck -UseBasicParsing
```

→ Erwartung: Status `201 Created`, leerer Body. **Bestätigt** (2026-05-31).

GET-Test:

```powershell
Invoke-WebRequest -Uri "https://localhost:<port>/api/measurements?deviceIdentifier=raspberry-pi&typeName=Temperature&limit=10" `
    -Method GET `
    -Headers @{ "X-API-Key" = "<lokaler-key>" } `
    -SkipCertificateCheck -UseBasicParsing
```

→ Erwartung: Status `200 OK`, `Content-Type: application/json`, im Body
ein JSON-Array mit `timestamp`/`value`/`unit`-Objekten in camelCase
(automatische PascalCase→camelCase-Übersetzung durch System.Text.Json).
**Bestätigt** mit den eben angelegten Werten zurückgelesen.

#### Production (VPS, 2026-06-01)

Nach Deployment auf den VPS (`dotnet publish` lokal → SFTP nach
`/home/deployuser/deploy-tmp/` → Zwei-Schritt-Deploy nach `/var/www/aqms/`
mit `chown aqms:aqms`, gefolgt von `systemctl restart aqms-web`) und
Setzen des `ApiKey`-Werts in der systemd-Override (siehe §23L.5,
Praxisproblem §23L.8 bezüglich Tippfehler), wurden dieselben drei
Test-Szenarien gegen die Production-URL ausgeführt:

| Test | Setup | Erwartung | Ergebnis |
|---|---|---|---|
| 1 | `POST /api/measurements`, **kein** `X-API-Key`-Header | 401 | ✓ 401 |
| 2 | `POST /api/measurements` mit korrektem Header, gültigem Body | 201 | ✓ 201 |
| 3 | `GET /api/measurements?deviceIdentifier=raspberry-pi&typeName=Temperature&limit=10` mit korrektem Header | 200 + JSON-Array | ✓ 200 + Messwert von Test 2 zurückgelesen |

Aufruf für Test 2 und 3 (mit Production-URL und VPS-Key):

```powershell
$base = "https://aqms.aqms.example.com"
$key  = "<VPS-Key>"

$body = @{
    deviceIdentifier    = "raspberry-pi"
    measurementTypeName = "Temperature"
    value               = 22.5
    timestamp           = "2026-06-01T13:19:00Z"
} | ConvertTo-Json

Invoke-WebRequest -Uri "$base/api/measurements" -Method POST `
    -Headers @{ "X-API-Key" = $key } `
    -ContentType "application/json" -Body $body -UseBasicParsing

Invoke-WebRequest -Uri "$base/api/measurements?deviceIdentifier=raspberry-pi&typeName=Temperature&limit=10" `
    -Method GET -Headers @{ "X-API-Key" = $key } -UseBasicParsing
```

**Was damit bewiesen ist.** Die Response-Header zeigten `Server: nginx`
(statt `Server: Kestrel` wie lokal). Das beweist, dass die Anfragen den
**vollständigen Production-Stack** durchlaufen haben: DNS-Auflösung
`aqms.aqms.example.com` → Nginx (HTTPS-Terminierung, Reverse-Proxy) → Kestrel
(ASP.NET-Core-Hostprozess) → `ApiKeyMiddleware` → `MeasurementsController`
→ EF Core → SQL Server in Docker → Response. Sechs Komponenten in Reihe,
alle korrekt verkabelt und produktiv erreichbar. Mit der lokalen
Verifikation zusammen ist der Backend-Stack **doppelt belegt** —
funktioniert in Entwicklung *und* Production.

### 23M.6 Praxisproblem: JSON-Zyklus beim ersten GET

**Symptom.** Beim ersten Aufruf von `GET /api/measurements` brach die
Pipeline in der `ApiKeyMiddleware` an der Zeile `await _next(context);`
mit einer `System.Text.Json.JsonException` ab:
*"A possible object cycle was detected... Path:
$.Device.Measurements.Device.Measurements.Device.Measurements..."*

**Ursache.** Die GET-Action gab zunächst direkt `List<Measurement>`
zurück. Beim JSON-Serialisieren versuchte System.Text.Json den ganzen
Objekt-Graphen abzuwickeln: `Measurement.Device` → `Device.Measurements`
→ wieder `Measurement.Device` → usw. Nach 32 Verschachtelungs-Ebenen
greift der Endlos-Schleifen-Schutz und wirft. Die Navigation-Properties
waren befüllt, weil die im selben Controller-Lauf bereits geladenen
`device`- und `measurementType`-Objekte den Change Tracker über die
Beziehungen informierten.

Die Ausnahme „bricht in der Middleware aus", obwohl die Ursache in der
Action liegt, weil im ASP.NET-Pipeline-Modell das `_next(context)`
weiter unten in der Pipeline läuft — Action und JSON-Schreiben sind
*Teil* des `_next`-Aufrufs aus Sicht der Middleware.

**Lösung.** Einführung der separaten Antwort-Klasse
`MeasurementResponseDto` (drei Felder: `Timestamp`, `Value`, `Unit`) plus
`.Select(...)`-Projektion in der LINQ-Kette. Damit wird:

- nicht mehr die ganze Entity zurückgegeben,
- kein `Device`-Objekt mitgesendet,
- kein Zyklus erzeugt.

EF Core übersetzt die `.Select`-Projektion in eine effizientere
SQL-Abfrage (nur drei Spalten mit JOIN auf `MeasurementTypes`).

**Lessons Learned.**

1. Entity-Klassen niemals direkt als API-Antwort verwenden, sobald
   Navigation-Properties existieren. Das Argument *„DTO wegen Trennung
   API/DB"* ist nicht akademisch — es ist eine harte technische Notwendigkeit
   bei JSON-Serialisierung.
2. Exception-Trace lesen mit Verständnis der Pipeline: Wenn eine Exception
   „in der Middleware" auftritt, kann die Ursache in jeder Komponente
   *nach* dieser Middleware liegen. Die Middleware ist nur der Aufrufer
   des `_next`-Delegates.
3. Diplomarbeit-Argumentation: Diese Erkenntnis wurde **selbst durchlebt**,
   nicht theoretisch übernommen. Genau das macht das DTO-Konzept hier
   verteidigungsfähig.

### 23M.7 Bekannte offene Punkte

- **Timestamp ohne Zeitzonen-Marker im JSON-Output.** Die DB-Spalte
  `Timestamp` ist `datetime` (nicht `datetimeoffset`), die C#-Property
  `DateTime` (nicht `DateTimeOffset`). Resultat: ein per POST
  geschickter UTC-Wert (`"2026-05-31T11:00:00Z"`) wird in der DB ohne
  TZ-Info gespeichert und im GET als `"2026-05-31T11:00:00"` (ohne `Z`)
  zurückgegeben. Konvention §11 fordert „UTC durchgängig" — technisch
  hält der Code das ein (es *ist* UTC), aber die API-Antwort verliert
  die Information. Fix-Möglichkeiten: DB-Spalte auf `datetimeoffset`,
  oder im DTO `DateTime.SpecifyKind(... DateTimeKind.Utc)` setzen.
  Verschoben in eine spätere Polishing-Phase, da nicht funktional
  blockierend.
- **Keine fachliche Wertvalidierung.** `dto.Value` wird ohne Plausibilitäts-
  prüfung gespeichert (negative Temperatur am Strom-Sensor wird akzeptiert).
  Gehört in den Service-Layer (§33.2, Phase 3).
- **Keine Authentifizierung auf User-Ebene.** API-Key-Middleware reicht
  für Maschine-zu-Maschine; sobald die GET-Endpoints auch vom Browser
  konsumiert werden, kommt `[Authorize(Roles = ...)]` dazu (§33.2, Phase 3).

---

## 23N. CommandsController und Service-Layer (CommandService)

### 23N.1 Anlass und Zielbild

Der Pi-Worker braucht zwei Endpunkte: einen, um seine offenen Schaltbefehle
abzuholen (`GET /api/commands/pending`), und einen, um das Ergebnis einer
ausgeführten Schaltung zurückzumelden (`POST /api/commands/result`). Damit
fällt zweierlei zum ersten Mal an: die **erste Service-Schicht** des Projekts
und die **erste Update-Logik** — ein Statusübergang `Pending → Executed/Failed`,
im Gegensatz zum append-only-MeasurementsController (§23M).

### 23N.2 Architekturentscheidungen

- **Service-Layer eingeführt, ohne Interface.** Die Geschäftslogik liegt in
  `CommandService` (`AQMS.Web/Services/`), nicht im Controller. Ein
  `ICommandService`-Interface mit genau einem Implementierer wurde bewusst
  *nicht* angelegt — dieselbe leere Abstraktion wie beim verworfenen
  Repository-Pattern (§30.12), siehe §30.13. Der Service ist bereits die
  Abstraktion über dem DbContext und ohne Interface testbar (EF-Core-InMemory
  bzw. SQLite gegen die echte LINQ-Logik statt Mock).
- **Service bleibt HTTP-frei.** Er gibt ein Domänen-Enum `CommandResult`
  (`Success`/`CommandNotFound`/`AlreadyProcessed`) bzw. eine `List<…>?`
  (null = Gerät unbekannt) zurück — *keinen* `ActionResult`. Die Übersetzung
  in HTTP-Status macht allein der Controller. Begründung: Geschäftslogik ohne
  ASP.NET-Hochfahren testbar, Transport (HTTP/gRPC/Queue) austauschbar, der
  Service in anderen Kontexten (Razor-Seite, Background-Job) wiederverwendbar.
- **Result-Pattern statt Exceptions.** Erwartbare Fälle (unbekanntes Gerät,
  fehlender/erledigter Befehl) werden über Rückgabewerte signalisiert, nicht
  über geworfene Ausnahmen — bei Normalbetrieb-Fällen das sauberere Modell.
- **Multi-Entity-Transaktion in einem `SaveChanges`.** `ProcessCommandResult`
  aktualisiert bei Erfolg in *einer* impliziten Transaktion den Befehlsstatus,
  schreibt einen `StateChange` (Audit-Trail) und aktualisiert
  `Device.CurrentState`/`LastSeen`. Atomarität ohne explizites
  `BeginTransaction` — entweder alles oder nichts. So können Status-Update und
  Audit-Trail nicht auseinanderlaufen.
- **Idempotenz-Riegel.** Ist der Befehl nicht mehr `Pending`, gibt der Service
  `AlreadyProcessed` zurück, ohne erneut zu schreiben. Schützt gegen einen
  doppelten `StateChange`, falls der Worker `/result` nach einem
  Netzwerk-Hänger erneut sendet, obwohl der erste Aufruf durchkam.
- **`LastSeen` nur im Erfolgszweig.** Eine fehlgeschlagene Schaltung heißt, die
  Shelly war nicht erreichbar — dann wird sie auch nicht als „gesehen"
  markiert. Bei Misserfolg bleibt `CurrentState` ebenfalls unverändert.
- **Status-Mapping (Service-Ergebnis → HTTP, im Controller):** `null` → **400**
  (konsistent mit §23M.2 für unbekannte fachliche Identifier); Liste, auch
  leer → **200**; `Success` → **200**; `CommandNotFound` → **404** (per
  `commandId` adressierte Ressource existiert nicht — bewusste Abgrenzung zum
  400 beim Query-Identifier); `AlreadyProcessed` → **409 Conflict**
  (wohlgeformte Anfrage, die mit dem aktuellen Zustand der Ressource
  kollidiert).
- **Scoped-Registrierung.** `AddScoped<CommandService>()` — gleiche Lebensdauer
  wie der DbContext (eine Instanz pro Request). Eine Singleton-Registrierung
  würde einen DbContext für immer festhalten (Captive Dependency); der
  DbContext ist nicht thread-safe und trackt Request-spezifischen Zustand.

### 23N.3 Klassen-Aufbau

**DTOs (`AQMS.Web/Dtos/`):**

```csharp
public class CommandResultDto      // Eingabe POST /result
{
    public long CommandId { get; set; }
    public bool Success { get; set; }
    public string? ResultMessage { get; set; }
}

public class CommandPendingDto     // Ausgabe GET /pending
{
    public long CommandId { get; set; }
    public string Action { get; set; } = string.Empty;  // string statt Enum: System.Text.Json
    public DateTime CreatedAt { get; set; }              // serialisiert Enums sonst als Zahl
}
```

**Service (`AQMS.Web/Services/CommandService.cs`):**

```csharp
public enum CommandResult { Success, CommandNotFound, AlreadyProcessed }

public class CommandService
{
    private readonly AqmsDbContext _db;
    public CommandService(AqmsDbContext db) { _db = db; }

    // null = Gerät unbekannt; leere Liste = keine offenen Befehle
    public async Task<List<CommandPendingDto>?> GetPendingCommands(string deviceIdentifier)
    {
        var device = await _db.Devices.FirstOrDefaultAsync(d => d.DeviceIdentifier == deviceIdentifier);
        if (device is null) return null;

        // erst laden (SQL), dann mappen (C#): enum.ToString() ist nicht zuverlässig SQL-übersetzbar
        var pendingCommands = await _db.DeviceCommands
            .Where(c => c.Status == CommandStatus.Pending && c.DeviceId == device.Id)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return pendingCommands.Select(c => new CommandPendingDto
        {
            CommandId = c.Id,
            Action = c.Action.ToString(),
            CreatedAt = c.CreatedAt,
        }).ToList();
    }

    public async Task<CommandResult> ProcessCommandResult(CommandResultDto dto)
    {
        var timestampNow = DateTime.UtcNow;

        var command = await _db.DeviceCommands
            .Include(c => c.Device)
            .FirstOrDefaultAsync(c => c.Id == dto.CommandId);

        if (command is null) return CommandResult.CommandNotFound;
        if (command.Status != CommandStatus.Pending) return CommandResult.AlreadyProcessed;

        command.ExecutedAt = timestampNow;
        command.ResultMessage = dto.ResultMessage;

        if (dto.Success)
        {
            command.Status = CommandStatus.Executed;
            _db.StateChanges.Add(new StateChange
            {
                DeviceId = command.DeviceId,
                Timestamp = timestampNow,
                State = command.Action,
                ChangedByUserId = command.RequestedByUserId
            });
            command.Device.CurrentState = command.Action;
            command.Device.LastSeen = timestampNow;
        }
        else
        {
            command.Status = CommandStatus.Failed;
        }

        await _db.SaveChangesAsync();
        return CommandResult.Success;
    }
}
```

**Controller (`AQMS.Web/Controllers/CommandsController.cs`):**

```csharp
[ApiController]
[Route("api/commands")]
public class CommandsController : ControllerBase
{
    private readonly CommandService _commandService;
    public CommandsController(CommandService commandService) { _commandService = commandService; }

    [HttpGet("pending")]
    public async Task<ActionResult> GetPending(string deviceIdentifier)
    {
        var commandServiceResult = await _commandService.GetPendingCommands(deviceIdentifier);
        if (commandServiceResult is null) return BadRequest($"Unknown Device-Identifier: {deviceIdentifier}");
        return Ok(commandServiceResult);
    }

    [HttpPost("result")]
    public async Task<ActionResult> SubmitResult([FromBody] CommandResultDto dto)
    {
        var commandServiceResultEnum = await _commandService.ProcessCommandResult(dto);
        switch (commandServiceResultEnum)
        {
            case CommandResult.Success:           return Ok();
            case CommandResult.CommandNotFound:   return NotFound("Command not found.");
            case CommandResult.AlreadyProcessed:  return Conflict("Command already processed.");
            default:                              return StatusCode(500);
        }
    }
}
```

### 23N.4 Integration in `Program.cs`

Eine Zeile bei den Service-Registrierungen (nach `AddControllersWithViews()`)
plus `using AQMS.Web.Services;` oben:

```csharp
builder.Services.AddScoped<CommandService>();
```

Ein `MapControllers()` ist **nicht** nötig — das bestehende
`MapControllerRoute("default", …)` mappt die attributbasierten API-Controller
mit (gleiche Endpoint-Datenquelle), wie schon beim MeasurementsController
(§23M.4).

### 23N.5 Verifikation

Beide Endpunkte wurden mit denselben vier Szenarien geprüft — **lokal
(`localhost`) und auf dem VPS (`https://aqms.aqms.example.com`), beide am
2026-06-04**:

| Szenario | Erwartet | Lokal | VPS |
|---|---|---|---|
| `GET /pending`, gültiges Gerät, keine offenen Befehle | 200 `[]` | ✓ | ✓ |
| `GET /pending`, unbekanntes Gerät | 400 | ✓ | ✓ |
| `POST /result`, unbekannte `CommandId` | 404 | ✓ | ✓ |
| `POST /result` desselben Befehls zweimal | 1× 200, 2× 409 | ✓ | ✓ |

Happy-Path zusätzlich: nach erfolgreichem `POST /result` ist ein neuer
`StateChanges`-Eintrag vorhanden und `Device.CurrentState` auf `On`
aktualisiert.

**VPS-Deployment** nach dem etablierten Verfahren (§23G.1 Publish, §23H.7
Zwei-Schritt-Deploy): `dotnet publish -c Release` → Upload nach
`/home/deployuser/deploy-tmp/` → `sudo systemctl stop aqms-web` →
`sudo cp -r /home/deployuser/deploy-tmp/* /var/www/aqms/` →
`sudo chown -R aqms:aqms /var/www/aqms` → `sudo systemctl start aqms-web`.
Der `ApiKey` war bereits aus der API-Key-/Measurements-Verifikation (§23L.7)
in der systemd-Override gesetzt — kein neuer Schlüssel nötig.

**Reproduzierbare Tests (PowerShell).** Wichtig: in **Windows PowerShell 5.1**
existiert der Parameter `-SkipHttpErrorCheck` *nicht* (wirft sofort einen
Fehler) — daher werden die Fehlerfälle per `try/catch` über den Statuscode
abgefangen. Setup:

```powershell
$base    = "https://aqms.aqms.example.com"
$headers = @{ "X-API-Key" = "<VPS_APIKEY>" }
```

```powershell
# Erfolgsfall (200 mit Liste/leerem Array)
Invoke-RestMethod "$base/api/commands/pending?deviceIdentifier=shelly-heater" -Headers $headers

# Fehlerfälle: Statuscode über try/catch (PS 5.1)
try { Invoke-WebRequest "$base/api/commands/pending?deviceIdentifier=quatsch" -Headers $headers | Out-Null }
catch { [int]$_.Exception.Response.StatusCode }   # -> 400

$body = @{ commandId = 999999; success = $true } | ConvertTo-Json
try { Invoke-WebRequest "$base/api/commands/result" -Method Post -Headers $headers -ContentType "application/json" -Body $body | Out-Null }
catch { [int]$_.Exception.Response.StatusCode }   # -> 404

# Happy-Path: zweite identische Meldung -> 409
$ok = @{ commandId = <ID>; success = $true; resultMessage = "VPS Test" } | ConvertTo-Json
Invoke-WebRequest "$base/api/commands/result" -Method Post -Headers $headers -ContentType "application/json" -Body $ok | Select-Object StatusCode  # 200
try { Invoke-WebRequest "$base/api/commands/result" -Method Post -Headers $headers -ContentType "application/json" -Body $ok | Out-Null }
catch { [int]$_.Exception.Response.StatusCode }   # -> 409
```

**`Pending`-Befehl für den Happy-Path** (VPS-DB im Docker-Container; `-C`
Server-Zert vertrauen, `-I` für jeden Schreibzugriff wegen Filtered Index,
§22.7):

```bash
docker exec -i aqms-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P '<SA_PASSWORT>' -C -I -d aqms \
  -Q "INSERT INTO DeviceCommands (Action, Status, CreatedAt, DeviceId) VALUES ('On','Pending',GETUTCDATE(), <DeviceId>);"
```

### 23N.6 Lessons Learned beim Bau

- **`enum.ToString()` ist nicht zuverlässig SQL-übersetzbar.** Eine Projektion
  `Select(c => new Dto { Action = c.Action.ToString() })` *innerhalb* der
  `IQueryable` landet in der SQL-Übersetzung und wirft je nach EF-Version eine
  „could not be translated"-Exception. Lösung: erst `ToListAsync()`
  (materialisieren), dann client-seitig mappen. Die Grenze ist `ToListAsync()`
  — davor SQL, danach C#.
- **Eine neue Entity ohne `Add()` wird still nicht persistiert.** Bei
  *getrackten* Entities reicht Mutieren (Change Tracking erzeugt das UPDATE),
  aber ein *neu* erstelltes `StateChange`-Objekt muss EF Core per
  `_db.StateChanges.Add(...)` als „Added" gemeldet bekommen — sonst speichert
  `SaveChanges` Status- und Device-Update, der Audit-Eintrag fehlt aber
  lautlos. Bei einem Audit-Trail die gefährlichste Fehlerklasse.
- **`await` bindet um die ganze Methodenkette.** `await query.ToListAsync().Select(...)`
  ruft `.Select` auf dem `Task` auf (Compilerfehler), nicht auf der Liste. Erst
  materialisieren (`await … ToListAsync()` in eine Variable), dann mappen.
- **`-SkipHttpErrorCheck` gibt es in Windows PowerShell 5.1 nicht.** Beim
  VPS-Test warf der Parameter sofort einen Fehler („Parameter kann nicht
  gefunden werden") — er wurde erst mit PowerShell 6/7 eingeführt. In 5.1
  werfen `Invoke-WebRequest`/`Invoke-RestMethod` bei einem Nicht-2xx-Status
  eine Exception; den Statuscode liest man über `try/catch` aus
  (`[int]$_.Exception.Response.StatusCode`). Erfolgsfälle (2xx) laufen ohne
  Catch durch. Die funktionierenden Testbefehle stehen in §23N.5.

### 23N.7 Bekannte offene Punkte

- **`[Authorize]`** ist an `/api/commands/...` bewusst nicht gesetzt —
  Maschine-zu-Maschine läuft über den API-Key (§23L), nicht über Identity. Die
  rollenbasierte Autorisierung betrifft später die *Browser*-Routen.
- ~~**Befehls-Erstellung fehlt.**~~ **Erledigt am 2026-07-12** (§23P.3): `CreateCommandAsync`
  im `CommandService`, aufgerufen vom Dashboard-Toggle über eine **MVC**-Route
  (`POST /Dashboard/Toggle`), bewusst **nicht** über `/api` — siehe die Begründung in §23P.2
  (der API-Key dürfte nicht in die Browser-Seite gelangen).
- **Fachliche Validierung** (z. B. Plausibilität) könnte jetzt im Service-Layer
  ergänzt werden.

### 23N.8 Befehls-Empfang v2: geräte-übergreifender Poll (2026-06-28)

**Anlass.** Der v1-Vertrag (§23N.1–.5) scopte auf *ein* Gerät:
`GetPendingCommands(deviceIdentifier)` lieferte die Pending-Befehle, deren
`DeviceId` genau diesem Gerät gehört. Beim Bau des Worker-Polls zeigte sich, dass
das nicht zur Topologie passt:

- Der Worker pollt als `raspberry-pi` (korrekt für den *Mess*-Pfad — der Pi postet
  Temperatur als sich selbst). Schaltbefehle hängen aber an den **Shellys**, die je
  ein eigenes `Device` mit eigenem `DeviceIdentifier` und eigener `IPAddress` sind.
  Ein Poll als `raspberry-pi` liefert für Schaltbefehle daher dauerhaft `[]`.
- Selbst ein empfangener Befehl wäre nicht ausführbar gewesen: die v1-DTO trug nur
  `CommandId`/`Action`/`CreatedAt` — **kein Zielgerät, keine IP**.

**Entscheidung (1a + 2a).** Ein Poll holt alle offenen Aktor-Befehle, je mit
Zielgerät + IP angereichert:

- **1a — Scoping über den Gerätetyp.** `GetPendingCommands()` (parameterlos)
  liefert alle `Pending`-Befehle für Geräte vom Typ **SmartPlug**
  (`DeviceType.Name == "SmartPlug"`, Seed-Id 2; der Pi ist „Sensor"). Bewusst über
  den Typnamen statt der Magic-Number-Id. Zusätzlich defensiv: nur `IsEnabled`
  mit nicht-null `IPAddress` (deaktivierte/adresslose Geräte gehören nicht in den
  Befehlsstrom).
- **2a — IP inline.** `CommandPendingDto` (Web) **und** `PendingCommandDto`
  (Worker) bekommen `DeviceIdentifier` (string) + `IPAddress` (string?) — identische
  Property-Namen, sonst bindet `ReadFromJsonAsync` still nicht. „Eine Wahrheit" für
  die IP bleibt die DB; sie reitet pro Befehl mit, statt über einen zweiten Endpunkt.

Verworfene Alternativen: §30.14.

**Konkrete Änderungen.** DTO Web+Worker je +2 Properties; `GetPendingCommands()`
parameterlos, Rückgabe `List<CommandPendingDto>` (nicht mehr nullable, da kein
„unbekanntes Gerät"-Fall mehr), `Include(c => c.Device)`, Filter
`Status==Pending && Device.DeviceType.Name=="SmartPlug" && Device.IsEnabled &&
Device.IPAddress != null`, Projektion um `DeviceIdentifier` + `IPAddress` ergänzt.
Das „erst `ToListAsync()`, dann mappen"-Muster (§23N.6) bleibt; `Include` ist
nötig, weil das Mapping nach der Materialisierung läuft, der Typ-Filter im `Where`
braucht keins (EF baut einen Join). `GetPending()` im Controller parameterlos,
`BadRequest`-Zweig entfällt. Worker-URL ohne Query-Parameter.

**API-Vertragsänderung (breaking).** `/api/commands/pending` nimmt **keinen**
`deviceIdentifier` mehr und kennt keinen 400-„unbekanntes Gerät"-Fall — die
entsprechende Zeile in §23N.5 ist obsolet (historischer v1-Stand, bleibt als
Tagebuch).

**Verifikation (lokal, 2026-06-28).** `/pending` ohne Parameter liefert den
Pending-Befehl mit korrektem `deviceIdentifier: "shelly-filter"` + `ipAddress`; der
`IsEnabled`/IP-Guard greift (Befehl verschwindet bei `IsEnabled=0` bzw.
`IPAddress=NULL`); der Worker empfängt + deserialisiert end-to-end
(`Befehl empfangen: 3 On …`). **Offen:** Der VPS läuft noch v1 — Redeploy nötig,
damit dort dieselbe parameterlose Route gilt.

**Praxisprobleme beim Testen (Symptom → Ursache → Lösung → Lesson).**

1. *Neuer Build bindet nicht / Tests treffen alten Code.* **Symptom:** `dotnet run`
   stürzt mit `address already in use` (Port 5258/7144), gleichzeitig liefert
   `/pending` weiter den alten `deviceIdentifier`-Pflicht-400. **Ursache:** eine
   verwaiste alte Instanz hielt den Port und bediente den alten Controller;
   `Stop-Process dotnet` erwischte sie nicht. **Lösung:** Port-Owner gezielt killen
   (`Get-NetTCPConnection -LocalPort <port> -State Listen | ForEach { Stop-Process -Id $_.OwningProcess -Force }`),
   dann neu starten. **Lesson:** `Build succeeded` ≠ „der neue Stand läuft"; bei
   rätselhaftem Alt-Verhalten zuerst prüfen, ob die laufende Instanz die neue ist.
2. *Schema/Port-Mismatch in beide Richtungen.* **Symptom:** mal „corrupted frame /
   SSL", mal „Unable to connect". **Ursache:** `http`-Port 5258 vs. `https`-Port 7144
   verwechselt — einmal Client `https` gegen HTTP-Port, einmal Worker-`BaseUrl`
   `https://7144` gegen eine nur auf `http://5258` lauschende Web. **Lösung:** Port
   **und** Schema beidseitig angleichen; lokal konsequent `https`/7144 (entspricht
   Pi/VPS). **Lesson:** Kandidat für den Worker-`appsettings.Development/Production`-
   Split, der das manuelle Angleichen abnähme.

---

## 23O. Deployment des Worker-Service auf dem Raspberry Pi

Gegenstück zu §23G/§23H (AQMS.Web auf dem VPS), aber für den Worker auf dem Pi (ARM64,
Debian Trixie). **Bewusste Vorab-Entscheidung:** Der Worker wurde deployt, obwohl
funktional unvollständig (kein Result-Reporting, kein Sensor-Pfad) — um die gesamte Kette
(VPS-Backend v2 → Pi → Hardware) früh end-to-end zu verifizieren und ein Architektur-Risiko
auszuschließen, bevor weiter investiert wird. Ein spät entdeckter Architektur-Fehler wäre
teurer als ein vorgezogener Deployment-Test.

### 23O.1 Publish: self-contained für linux-arm64

```powershell
dotnet publish AQMS.Worker -c Release -r linux-arm64 --self-contained true -o C:\temp\publish-worker
```

*Begründung self-contained* (statt Framework-Dependent wie am VPS §23G.1): Auf dem Pi ist
**keine** .NET-Runtime installiert; self-contained packt sie mit ein — kein Runtime-Install,
keine Versions-Drift (verworfene Alternative §30.15). Kein `-p:PublishTrimmed`: Trimming
kann reflektionsbasiertes Config-Binding still wegschneiden; ~80 MB sind auf der SD-Karte
irrelevant. `linux-arm64`, weil `uname -m` = `aarch64`.

### 23O.2 Production-Overlay für die BaseUrl

`AQMS.Worker/appsettings.Production.json`:

```json
{
  "AqmsApi": {
    "BaseUrl": "https://aqms.aqms.example.com"
  }
}
```

*Begründung:* Die BaseUrl ist „pro Umgebung" (§11.6). Statt die appsettings.json auf
VPS-URL zu editieren (würde auch Dev auf den VPS zeigen — verworfen §30.15), überschreibt
das Overlay nur in Produktion. Greift nur bei `DOTNET_ENVIRONMENT=Production` (§23O.4).

### 23O.3 Transfer auf den Pi

```powershell
ssh kev@10.0.0.222 "rm -rf ~/deploy-worker && mkdir -p ~/deploy-worker"
scp -r C:\temp\publish-worker\* kev@10.0.0.222:/home/kev/deploy-worker/
```

```bash
sudo mkdir -p /opt/aqms-worker
sudo rm -rf /opt/aqms-worker/*
sudo cp -r /home/kev/deploy-worker/* /opt/aqms-worker/
sudo chmod +x /opt/aqms-worker/AQMS.Worker
```

*Begründung Zwei-Schritt (Home → `/opt` per sudo):* dasselbe Upload-Rechte-Muster wie am
VPS (§28) — nach `chown aqms` kann `kev` nicht mehr direkt schreiben. `chmod +x` ist
Pflicht: self-contained erzeugt eine **native** Executable, von Windows kopierte Dateien
tragen kein Ausführungs-Bit (sonst `Permission denied` beim Start).

### 23O.4 Service-User + systemd-Unit

```bash
sudo useradd -r -s /usr/sbin/nologin aqms
sudo chown -R aqms:aqms /opt/aqms-worker
```

`/etc/systemd/system/aqms-worker.service`:

```ini
[Unit]
Description=AQMS Worker Service
After=network-online.target
Wants=network-online.target

[Service]
WorkingDirectory=/opt/aqms-worker
ExecStart=/opt/aqms-worker/AQMS.Worker
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=aqms-worker
User=aqms
Environment=DOTNET_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

*Begründung:* `ExecStart` zeigt auf die **native Executable** (kein `/usr/bin/dotnet …`
wie in der Framework-Dependent-Unit §23H.2). `DOTNET_ENVIRONMENT=Production` — **nicht**
`ASPNETCORE_ENVIRONMENT`: der Generic Host des Workers liest diese Variable und aktiviert
damit das Overlay aus §23O.2.

### 23O.5 ApiKey als geschützte Override

```bash
sudo systemctl edit aqms-worker
```

```ini
[Service]
Environment="ApiKey=<VPS_API_KEY>"
```

*Begründung:* Secret separat von der Unit (§23H.3-Muster), flach als `ApiKey` (nicht
`AqmsApi__ApiKey` — die Doppelunterstrich-Falle §11.6/§23L.8). Derselbe Wert wie in der
aqms-web-Override; Worker und API teilen den Schlüssel.

### 23O.6 Aktivieren

```bash
sudo systemctl daemon-reload
sudo systemctl enable --now aqms-worker
sudo journalctl -u aqms-worker -f
```

Verifiziert am 2026-07-02: `Status: OK` → `Befehl empfangen` → Shelly `200` → `ausgeführt`,
mit Prefix `raspi-aquarium aqms-worker[PID]` (= self-contained unter systemd am Pi). Die
volle Kette lief vom Pi aus.

### 23O.7 Praxisprobleme (Symptom → Ursache → Lösung → Lesson)

1. **Worker pollt `localhost` statt VPS.** *Symptom:* `Connection refused (localhost:5258)`
   am Pi. *Ursache:* die Overlay-Datei hieß `appsettings.prduction.json` (Tippfehler, „o"
   fehlt) — .NET fand bei `DOTNET_ENVIRONMENT=Production` keine passende Datei und fiel auf
   die appsettings.json (Dev-Default localhost) zurück; unter Linux ist Groß-/Kleinschreibung
   relevant (`appsettings.Production.json`, großes P). *Lösung:* Datei korrekt benennen (im
   Projekt **und** am Pi). *Lesson:* Bei „zieht localhost trotz Overlay" zuerst den
   Dateinamen exakt prüfen, dann ob `DOTNET_ENVIRONMENT` greift.
2. **`DOTNET_ENVIRONMENT` vs. `ASPNETCORE_ENVIRONMENT`.** *Symptom:* Overlay lädt nicht.
   *Ursache:* der Worker läuft auf dem Generic Host, der `DOTNET_ENVIRONMENT` liest —
   `ASPNETCORE_ENVIRONMENT` (wie in der aqms-web-Unit) würde er ignorieren. *Lösung:* in der
   Worker-Unit `DOTNET_ENVIRONMENT` setzen. *Lesson:* Web- und Worker-Host nutzen
   unterschiedliche Env-Variablen; die Web-Unit nicht blind kopieren.
3. **Leerer `ApiKey` in der Override.** *Symptom:* `systemctl show … | grep ApiKey` zeigt
   `ApiKey=` (leer) → hätte nach dem BaseUrl-Fix zu `401` geführt. *Ursache:* der Wert landete
   beim `systemctl edit` außerhalb der Marker / ohne `[Service]`-Header. *Lösung:* Override
   korrekt setzen, mit `systemctl show` gegenprüfen. *Lesson:* Nach jedem `systemctl edit` den
   **geladenen** Wert verifizieren, nicht nur die Datei (vgl. §23L.8).
4. **BaseUrl-Overlay-Tippfehler reproduziert sich.** *Symptom:* Nach dem Re-Deploy pollt der
   Pi wieder `localhost:5258` (`Connection refused`). *Ursache:* Die Overlay-Datei hieß im
   **Projekt** weiterhin `appsettings.prduction.json` — beim ersten Auftreten (Punkt 1) nur
   *am Pi* umbenannt, nicht an der Quelle; der nächste `dotnet publish` brachte den falschen
   Namen erneut mit. *Lösung:* im Projekt umbenennen **und** die `.csproj`-Content-Regel
   ergänzen (`<Content Include="appsettings.Production.json" CopyToOutputDirectory="PreserveNewest" />`).
   *Lesson:* Deployment-Fixes gehören in die **Quelle**, nicht ins Deploy-Artefakt — sonst
   reproduziert sie der nächste Build. (Am Symptom flicken kostete hier zwei Deploy-Zyklen.)

### 23O.8 Entscheidung: BaseUrl bleibt in der Overlay-Datei (entschieden 2026-07-12)

Die Produktions-BaseUrl liegt in `appsettings.Production.json` (§23O.2). Diese Datei-Variante
hatte sich im Deployment **zweimal** als fragil erwiesen (§23O.7 Punkt 1 + 4). Alternative wäre
gewesen, die BaseUrl als `Environment=AqmsApi__BaseUrl=https://aqms.aqms.example.com` in die
systemd-Override zu ziehen — kein Dateiname zum Vertippen, kein „Copy-to-output"-Risiko.

**Abwägung:**
- *Für Environment:* robuster im Deployment (beide bisherigen Ausfälle wären nicht passiert);
  Config und Secret lägen am selben Ort (Override).
- *Gegen Environment / für Datei:* kollidiert mit der §11.6-Linie „BaseUrl pro Umgebung in
  appsettings" und führt mit `AqmsApi__BaseUrl` genau die `__`-verschachtelten Keys wieder ein,
  gegen die §11.6/§23L.8 bewusst entschieden wurde (Doppelunterstrich-Falle).

**Entscheidung: die Overlay-Datei bleibt.**

*Begründung.* Die scheinbar starke Evidenz „zwei Deployment-Ausfälle" hält einer genauen Analyse
nicht stand: **beide Ausfälle hatten dieselbe einzige Ursache** — den Dateinamen-Tippfehler
`appsettings.prduction.json`. Diese Ursache ist inzwischen **an der Quelle** behoben (Umbenennung
im Projekt *plus* explizite `.csproj`-Content-Regel, §23O.7 Punkt 4). Damit ist nicht das Symptom
kuriert, sondern die **Fehlerklasse geschlossen** — der Publish kann den falschen Namen nicht mehr
reproduzieren.

Die Evidenz spricht also nicht mehr gegen die Datei-Variante. Ein Wechsel auf `Environment` würde
demgegenüber einen realen Nachteil neu einführen (die bewusst vermiedene
Doppelunterstrich-Falle) und eine dokumentierte Architektur-Linie brechen — für einen Vorteil, der
inzwischen nicht mehr existiert.

*Lesson Learned (methodisch).* Wiederholte Ausfälle sind nur dann ein Argument gegen ein Design,
wenn sie **unabhängige** Ursachen haben. Zwei Ausfälle aus *einer* behobenen Ursache sind ein
Datenpunkt, nicht zwei. Die Versuchung, aus einer schmerzhaften Erfahrung eine
Architekturänderung abzuleiten, muss gegen die Frage geprüft werden, ob die Ursache noch
existiert.

Damit ist dieser Punkt **geschlossen** und aus §33.1 entfernt.

---

## 23P. Befehls-Erstellung, Autorisierung und Dashboard

> **Stand 2026-07-12.** Mit dieser Sektion schließt sich der Regelkreis: bis hierher konnte der
> Worker Befehle *abrufen* und *melden*, aber niemand konnte sie *erzeugen* — `Pending`-Einträge
> mussten von Hand per `sqlcmd` in die DB geschrieben werden. Damit sind **Phase 3 und Phase 4
> funktional abgeschlossen.**

### 23P.1 Anlass

Nach Abschluss von Phase 2 (§11.10) beherrschte das System jede Teilkette außer einer:

| Teilkette | Status vor 23P |
|---|---|
| Pi → VPS: Messwerte (`POST /api/measurements`) | ✓ §11.10 |
| VPS → Pi: Befehle abholen (`GET /api/commands/pending`) | ✓ §23N.8 |
| Pi → Shelly: Schalten | ✓ §11.8 |
| Pi → VPS: Ergebnis melden (`POST /api/commands/result`) | ✓ §11.9 |
| **Mensch → VPS: Befehl *erzeugen*** | **fehlt** |
| **Mensch → VPS: Daten *ansehen*** | **fehlt** |

Die letzten beiden Zeilen sind zugleich die beiden offenen Punkte aus §33.2 (Autorisierung,
Befehls-Erstellung) und der Kern von Phase 4. Sie wurden daher in einem Zug umgesetzt.

### 23P.2 Architekturentscheidung: zwei Zugangswege, zwei Auth-Mechanismen

**Problem.** Die naheliegende Lösung wäre, den Dashboard-Toggle gegen einen neuen Endpunkt
`POST /api/commands` zu schicken — symmetrisch zu den bestehenden API-Routen. **Das wäre ein
Sicherheitsfehler.**

Die `ApiKeyMiddleware` (§23L) schützt **jeden** Pfad unter `/api`. Ein Browser-Aufruf gegen
`/api/commands` müsste den API-Key also mitsenden — und dieser Key müsste dazu in die
ausgelieferte HTML-Seite oder in das JavaScript eingebettet werden. Er wäre damit für jeden
angemeldeten Benutzer im Quelltext lesbar. Ein Benutzer der Rolle `User` (nur lesend gedacht)
könnte den Key auslesen und anschließend beliebige API-Aufrufe *unter Umgehung der
Rollenprüfung* absetzen — der API-Key kennt keine Rollen. Die gesamte Autorisierungsschicht
wäre ausgehebelt.

**Entscheidung.** Strikte Trennung der Zugangswege nach Aufrufer-Typ:

| Route | Aufrufer | Authentifizierung | Autorisierung |
|---|---|---|---|
| `/api/**` | **Maschine** (Pi-Worker) | API-Key im Header (§23L) | keine Rollen — der Worker ist ein Systemakteur |
| `/Dashboard/**` | **Mensch** (Browser) | ASP.NET-Identity-Cookie | `[Authorize]`, Schalten zusätzlich `Roles = "Admin"` |

Der Toggle läuft folglich über eine **normale MVC-Route** (`POST /Dashboard/Toggle`), nicht über
die API. Der API-Key verlässt den Server nie.

*Nebeneffekt (positiv).* Die beiden Mechanismen sind damit auch konzeptionell sauber getrennt:
Der API-Key beantwortet „ist das unsere Maschine?", Identity beantwortet „wer ist dieser Mensch
und was darf er?". Das sind zwei verschiedene Fragen, und sie werden von zwei verschiedenen
Mechanismen beantwortet.

### 23P.3 Service-Erweiterung: `CreateCommandAsync`

Die Erstellung wandert in den bestehenden `CommandService` (§23N) — kein neuer Service, keine
Logik im Controller. Der Service bleibt HTTP-frei und damit ohne ASP.NET testbar (§23Q).

```csharp
public enum CreateCommandResult
{
    Success,
    DeviceNotFound,
    DeviceDisabled,
    AlreadyPending
}

public async Task<CreateCommandResult> CreateCommandAsync(int deviceId, DeviceState action, string? userId)
{
    var device = await _db.Devices.FirstOrDefaultAsync(d => d.Id == deviceId);
    if (device is null) return CreateCommandResult.DeviceNotFound;
    if (!device.IsEnabled) return CreateCommandResult.DeviceDisabled;

    var hasPending = await _db.DeviceCommands
        .AnyAsync(c => c.DeviceId == deviceId && c.Status == CommandStatus.Pending);
    if (hasPending) return CreateCommandResult.AlreadyPending;

    var command = new DeviceCommand
    {
        DeviceId = deviceId,
        Action = action,
        Status = CommandStatus.Pending,
        CreatedAt = DateTime.UtcNow,
        RequestedByUserId = userId
    };

    _db.DeviceCommands.Add(command);
    await _db.SaveChangesAsync();

    return CreateCommandResult.Success;
}
```

**Vier Entscheidungen darin:**

1. **Eigenes Ergebnis-Enum `CreateCommandResult`**, kein Wiederverwenden von `CommandResult`
   (§23N.2). Erstellung und Ergebnis-Verarbeitung haben disjunkte Fehlerfälle (*Gerät* unbekannt/
   deaktiviert vs. *Befehl* unbekannt/schon verarbeitet). Ein gemeinsames Enum hätte in beiden
   Aufrufern tote `switch`-Zweige erzeugt, die nie erreicht werden können — Konsistenz um den
   Preis von Ausdruckskraft ist ein schlechter Tausch.

2. **`userId` wird als Parameter übergeben, nicht aus einem DTO gebunden.** Die Benutzer-ID wird
   im Controller serverseitig aus dem Claims-Principal gezogen (`_userManager.GetUserId(User)`)
   und durchgereicht. Würde sie aus dem Request-Body kommen, könnte ein Angreifer eine fremde
   `UserId` mitschicken und Schaltvorgänge unter falschem Namen protokollieren — der Audit-Trail
   in `StateChanges` wäre wertlos. *Grundsatz: Eigentümer-Fremdschlüssel niemals vom Client
   binden.*

3. **Deaktivierte Geräte werden abgewiesen, nicht eingereiht.** `GetPendingCommands` (§23N.8)
   filtert `IsEnabled == false` heraus — ein Befehl für ein deaktiviertes Gerät würde also **nie**
   abgeholt und bliebe für immer `Pending`. Ihn gar nicht erst anzulegen ist ehrlicher als eine
   Karteileiche zu erzeugen, die einen Fehler vortäuscht, der keiner ist.

4. **Idempotenz-Riegel `AlreadyPending`.** Ist der Pi offline, würde jeder weitere Klick einen
   weiteren `Pending`-Befehl erzeugen. Beim Wiederanlauf arbeitete der Worker sie alle
   nacheinander ab — das Relais würde mehrfach hin- und herschalten (*Relais-Flattern*), mit
   Verschleiß an der Hardware und einer sinnlosen Kaskade in `StateChanges`. Ein offener Befehl
   pro Gerät genügt. Dies ist das Gegenstück zum Idempotenz-Riegel auf der Melde-Seite (§23N.2),
   der doppelte `POST /result` abweist.

*Technischer Hinweis.* Der Vergleich `c.Status == CommandStatus.Pending` ist von EF Core nach SQL
übersetzbar, weil `HasConversion<string>()` im `DbContext` konfiguriert ist. Nicht übersetzbar
wäre `c.Status.ToString() == "Pending"` — das ist die in §23N.6 dokumentierte Falle.

### 23P.4 `DashboardController`

```csharp
[Authorize]                                   // ganze Seite nur eingeloggt
public class DashboardController : Controller
{
    private static readonly TimeZoneInfo Wien = TimeZoneInfo.FindSystemTimeZoneById("Europe/Vienna");

    // Konstruktor-Injektion: AqmsDbContext, CommandService, UserManager<IdentityUser>

    [HttpGet]
    public async Task<IActionResult> Index() { /* ViewModel bauen, siehe 23P.5 */ }

    [HttpPost]
    [Authorize(Roles = IdentitySeeder.AdminRole)]   // Schalten nur Admin
    [ValidateAntiForgeryToken]                      // CSRF-Schutz
    public async Task<IActionResult> Toggle(int deviceId, DeviceState action)
    {
        var userId = _userManager.GetUserId(User);          // serverseitig, siehe 23P.3 Punkt 2
        var result = await _commandService.CreateCommandAsync(deviceId, action, userId);

        TempData["Meldung"] = result switch { /* Übersetzung des Enums in Klartext */ };

        return RedirectToAction(nameof(Index));             // Post/Redirect/Get
    }
}
```

**Drei Entscheidungen:**

1. **`[Authorize]` auf Klassenebene, `Roles = "Admin"` nur auf `Toggle`.** Ein Benutzer der Rolle
   `User` darf das Dashboard *sehen* (Temperatur, Status, Verlauf), aber nicht *schalten*. Die
   Toggle-Buttons werden in der View für Nicht-Admins gar nicht erst gerendert — der
   `[Authorize(Roles = …)]`-Filter ist die **eigentliche** Absicherung, das Ausblenden in der View
   ist reine Benutzerführung. *Client-seitiges Verstecken ist keine Autorisierung.*

2. **`[ValidateAntiForgeryToken]`.** Ohne CSRF-Schutz könnte eine beliebige fremde Webseite ein
   verstecktes Formular gegen `/Dashboard/Toggle` absenden; der Browser des eingeloggten Admins
   würde das Session-Cookie automatisch mitschicken und das Aquarium würde schalten. Das
   Razor-`<form>`-Tag-Helper erzeugt das Token automatisch.

3. **Post/Redirect/Get.** Nach dem POST wird auf `Index` umgeleitet, die Rückmeldung reist per
   `TempData` mit (überlebt genau einen Redirect). Ohne dieses Muster würde ein `F5` des Benutzers
   den POST wiederholen und einen zweiten Schaltbefehl erzeugen.

### 23P.5 ViewModel statt Entity

Die View erhält ein `DashboardViewModel` (`DeviceRow`, Chart-Labels/-Werte, aktuelle Temperatur),
nicht die `Device`-Entities. Begründung: Entities schleppen Navigation-Properties mit; deren
Zugriff in der View würde entweder Nachlade-Queries auslösen oder auf nicht geladene Daten
laufen. Das ViewModel enthält exakt die Felder, die gerendert werden — nicht mehr.

**Zeitzonen-Behandlung.** Die Chart-Beschriftungen werden **serverseitig** von UTC nach
`Europe/Vienna` umgerechnet und als fertige `HH:mm`-Strings übergeben. Das umgeht den in §32.2
notierten Bug, dass der `Z`-Marker im JSON-Output verloren geht — der Browser bekommt gar keinen
Zeitstempel mehr zu interpretieren.

### 23P.6 Praxisproblem: `Device.LastSeen` wurde nie gesetzt

*Symptom.* Beim Bau des Online-Indikators fiel auf, dass er für alle Geräte dauerhaft „offline"
angezeigt hätte.

*Ursache.* `Device.LastSeen` wurde **ausschließlich** im Erfolgszweig von `ProcessCommandResult`
gesetzt (§23N.2) — also nur, wenn ein Shelly geschaltet wurde. Der Raspberry Pi selbst führt
keine Schaltbefehle aus; sein `LastSeen` wäre **niemals** gesetzt worden, obwohl er im
Sekundentakt Messwerte liefert. Der `[NotMapped]`-Ausdruck `IsOnline` (30-Sekunden-Fenster) wäre
für den Pi permanent `false` gewesen.

*Lösung.* Im `MeasurementsController` wird beim Empfang eines Messwerts `device.LastSeen`
mitgesetzt — die Entity ist ohnehin schon geladen, Change Tracking schreibt das Update im selben
`SaveChanges` mit (kein zusätzlicher Roundtrip):

```csharp
_db.Measurements.Add(measurement);
device.LastSeen = DateTime.UtcNow;   // ein eingehender Messwert IST ein Lebenszeichen
await _db.SaveChangesAsync();
```

*Bewusste Einschränkung.* Die **Shellys** haben weiterhin kein echtes Lebenszeichen — sie melden
sich nie von selbst, der Worker spricht sie nur beim Schalten an. Ein „online"-Badge für sie wäre
eine Lüge. Das Dashboard zeigt für Shellys daher ehrlich **„zuletzt geschaltet"** statt eines
Online-Status. Ein echter Health-Check (Worker pingt die Shellys zyklisch) wäre möglich, wurde
aber aus Scope-Gründen verworfen (§30.16).

*Lesson Learned.* Ein Feld, das an genau **einer** Stelle geschrieben wird, ist nur so
aussagekräftig wie diese eine Stelle. `LastSeen` hieß semantisch „zuletzt gesehen", war
implementiert als „zuletzt geschaltet" — die Lücke fiel erst auf, als das Feld erstmals *gelesen*
wurde. Schreibende und lesende Nutzung eines Feldes sollten gemeinsam entworfen werden.

### 23P.7 Verifikation (Produktion, 2026-07-12)

Deployment nach dem etablierten Verfahren (§23G.1 Publish, §23H.7 Zwei-Schritt-Deploy). **Keine
Migration nötig** — es wurde kein Entity geändert (`DashboardViewModel` ist kein Entity, die
`LastSeen`-Zeile schreibt in eine bestehende Spalte).

**Verifizierte Kette (end-to-end, gegen reale Hardware):**

1. Aufruf von `https://aqms.aqms.example.com/` ohne Login → `302` auf die Login-Seite
   (`[Authorize]` greift).
2. Login als Admin → Dashboard rendert; Temperatur-Kachel und Chart.js-Verlauf sind mit realen
   DS18B20-Werten gefüllt.
3. Klick auf „Einschalten" → `Pending`-Eintrag in `DeviceCommands`, `RequestedByUserId` mit der
   Admin-GUID belegt (Beweis der serverseitigen Zuordnung, 23P.3 Punkt 2).
4. Im Pi-Log (`journalctl -u aqms-worker -f`) binnen eines Poll-Intervalls: Befehl empfangen →
   Shelly-Dispatch → Befehl ausgeführt.
5. **Der reale Shelly hat geschaltet.**
6. Result-Reporting zurück an den VPS → Status `Executed`, `StateChanges`-Eintrag, `CurrentState`
   aktualisiert.
7. Dashboard neu geladen → Status-Badge steht auf `EIN`.

Damit ist die **vollständige Regelkette** bewiesen: Browser → HTTPS → Nginx → Kestrel → Identity
→ CommandService → SQL Server → Poll vom Pi → Shelly → Result → DB → Dashboard. Der Vorgang wurde
zur Beweissicherung **gefilmt**.

### 23P.8 Revision: bedingter Auto-Reload (2026-07-12, nachträglich)

**Ursprüngliche Position.** Zwischen Klick und Schaltung liegt bis zu ein Poll-Intervall. Das
Dashboard zeigte in dieser Zeit „Befehl wird ausgeführt …" und aktualisierte sich **nicht** von
selbst — bewusst, mit der Begründung: Die Latenz ist eine unvermeidliche Konsequenz des
Polling-Patterns (§30.14, der Pi sitzt hinter NAT), und sie sollte *sichtbar gemacht statt
kaschiert* werden.

**Anlass der Revision.** Beim Testen starrte der Entwickler selbst mehrere Minuten auf eine
scheinbar tote Oberfläche, bevor ihm einfiel, dass die Seite neu geladen werden muss. Damit ist
die ursprüngliche Position empirisch widerlegt: Wenn der Autor der Anwendung in die eigene Falle
läuft, ist das kein „ehrlich abgebildeter Systemzustand", sondern ein **UX-Defekt**. Ein
Benutzer, der nicht weiß, dass er neu laden muss, hält das System für kaputt.

**Was an der alten Begründung richtig bleibt.** Die Latenz darf nicht *kaschiert* werden — der
Fix muss sie **auflösen**, nicht verstecken, und darf keinen Dauer-Poll im Browser einführen.

**Lösung: bedingter, selbstterminierender Reload.** Das Reload-Skript wird **nur dann gerendert**,
wenn tatsächlich ein Befehl offen ist:

```html
@if (Model.Devices.Any(d => d.HasPendingCommand))
{
    <script>
        setTimeout(() => location.reload(), 3000);
    </script>
}
```

Zusätzlich ein Bootstrap-Spinner am „Befehl wird ausgeführt …"-Label, damit der Wartezustand als
*aktiv* und nicht als *hängend* gelesen wird.

**Warum genau diese Variante:**

| Verworfene Alternative | Grund |
|---|---|
| `<meta http-equiv="refresh">` im Layout | lädt **immer** neu, auch im Ruhezustand — reißt jede Interaktion weg, erzeugt Dauerlast |
| Permanentes `setInterval`-Polling gegen einen JSON-Endpunkt | zusätzlicher Endpunkt, zusätzliche Auth-Frage, dauerhafte Last — für ein Ereignis, das pro Klick genau einmal eintritt |
| SignalR | löst das Problem **nicht**: der Pi pollt weiterhin (NAT), die Latenz bliebe. Nur die Architektur würde komplexer (§30.16) |

**Die entscheidende Eigenschaft: Selbstterminierung.** Sobald der Worker `POST /result` gemeldet
hat, ist der Status `Executed`, `HasPendingCommand` wird `false` — und das `@if` rendert das
Skript nicht mehr. Die Schleife endet **von allein**. Kein Abbruchkriterium, kein Zähler, kein
Aufräum-Code. Im Ruhezustand enthält die Seite kein einziges Timer-Skript.

**Bekannter Randfall (bewusst getragen).** Ist der Pi offline, meldet der Worker nie — die Seite
lädt dann alle 3 Sekunden endlos neu. Bei laufendem Pi und 30-Sekunden-Poll-Takt unkritisch, aber
real. Eine Absicherung (z. B. Reload nur, wenn der Befehl jünger als 5 Minuten ist) wäre möglich
und ist als offener Punkt geführt (§33.3).

**Status:** implementiert; **VPS-Verifikation steht aus** (Stand dieser Eintragung).

**Lesson Learned (methodisch).** „Das Verhalten ist eine ehrliche Abbildung des Systemzustands"
ist eine *Entwickler*-Perspektive. Sie rechtfertigt keine Oberfläche, die für den Benutzer wie ein
Defekt aussieht. Der Test, ob eine Design-Entscheidung trägt, ist nicht ihre theoretische
Sauberkeit, sondern ob jemand — auch der Autor selbst — an ihr scheitert. Eine Position unter
Druck zu halten ist richtig; sie unter **neuer Evidenz** zu revidieren ebenfalls.

---

## 23Q. Unit-Tests mit xUnit

> **Stand 2026-07-12.** Erste automatisierte Tests. 16 Tests, alle grün.

### 23Q.1 Testrahmen und Provider-Entscheidung

Das Projekt `AQMS.Tests` (xUnit) referenziert `AQMS.Web` und `AQMS.Worker`. Für die
Service-Tests wird `Microsoft.EntityFrameworkCore.InMemory` verwendet.

**Warum InMemory und nicht SQLite-in-Memory oder eine echte SQL-Server-Testinstanz:**

- Der `AqmsDbContext` enthält bewusst **relationale** Konfiguration: `HasDefaultValueSql("GETUTCDATE()")`
  und den gefilterten Index `HasFilter("[Status] = 'Pending'")`. SQLite kennt weder `GETUTCDATE()`
  noch gefilterte Indizes in dieser Form — das `EnsureCreated()` würde am DDL scheitern.
- Der InMemory-Provider **ignoriert** relationale Annotationen. Das ist hier kein Mangel, sondern
  genau das Gewünschte: getestet werden soll die **Service-Logik**, nicht das DB-Schema.
- Eine echte SQL-Server-Testinstanz (Testcontainers) wäre die realistischste Variante, kostet aber
  Laufzeit und Infrastruktur — für den Umfang dieser Arbeit unverhältnismäßig.

**Ehrliche Abgrenzung (gehört ins Manuskript):** Die Tests beweisen, dass der `CommandService`
korrekt entscheidet. Sie beweisen **nicht**, dass Fremdschlüssel-Constraints, `CHECK`-Bedingungen
oder Unique-Indizes greifen — der InMemory-Provider erzwingt sie nicht. Diese Ebene wird durch
die Produktionsverifikation (§23M.7, §23N.5, §23P.7) abgedeckt, nicht durch Unit-Tests.

*Testisolation:* Jeder Test erhält eine eigene InMemory-Datenbank (`Guid.NewGuid()` als Name), da
xUnit Testklassen parallel ausführt. `EnsureCreated()` spielt dabei die `HasData`-Seed-Daten ein
(6 Devices) — die Tests arbeiten also gegen dasselbe Stammdatenbild wie die Produktion.

### 23Q.2 `Ds18b20Reader.ParseTemperature` (7 Tests)

Die Methode ist bewusst `static` und IO-frei (§11.10) — genau diese Trennung von *Lesen* und
*Parsen* macht sie ohne Sensor, ohne Pi und ohne Dateisystem testbar. Der Entwurfsentscheidung
aus §11.10 zahlt sich hier unmittelbar aus.

| Fall | Eingabe | Erwartung |
|---|---|---|
| Gültige Rohdaten | `crc=1a YES` + `t=24937` | `24.937` |
| CRC fehlgeschlagen | `crc=1a NO` | `null` |
| Kein `t=`-Marker | Zeile 2 ohne `t=` | `null` |
| Nur eine Zeile | abgeschnittener Read | `null` |
| Leerer Inhalt | `""` | `null` |
| Unparsbarer Wert (`[Theory]`) | `t=abc`, `t=` | `null` |
| Negative Temperatur | `t=-3250` | `-3.25` |

Der Negativ-Fall ist kein akademischer: Der DS18B20 misst bis −55 °C, und `int.TryParse` muss das
Vorzeichen mitnehmen. Der Test sichert das ab, obwohl der Fall im Aquarium nie eintreten wird —
er sichert die *Methode*, nicht den *Anwendungsfall*.

### 23Q.3 `CommandService` (8 Tests)

Deckt beide Richtungen des Befehlszyklus ab:

| Test | Prüft |
|---|---|
| `CreateCommandAsync` — gültiges Gerät | `Pending`-Eintrag, `RequestedByUserId` durchgereicht |
| `CreateCommandAsync` — unbekanntes Gerät | `DeviceNotFound`, **kein** Eintrag angelegt |
| `CreateCommandAsync` — deaktiviertes Gerät | `DeviceDisabled`, **kein** Eintrag angelegt |
| `CreateCommandAsync` — bereits offener Befehl | `AlreadyPending` (Idempotenz-Riegel, 23P.3) |
| `GetPendingCommands` | liefert nur SmartPlugs mit IP — der Sensor-Befehl wird gefiltert |
| `ProcessCommandResult` — Erfolg | `Executed`, `StateChange` geschrieben, `CurrentState` gesetzt |
| `ProcessCommandResult` — zweite Meldung | `AlreadyProcessed`, **kein zweiter** `StateChange` |
| `ProcessCommandResult` — unbekannte ID | `CommandNotFound` |

Die beiden fett hervorzuhebenden Tests sind die **Negativ-Tests**: „kein Eintrag angelegt" und
„kein zweiter StateChange". Ein Test, der nur den Happy Path prüft, hätte die Idempotenz-Riegel
(§23N.2, §23P.3) nicht abgesichert — und genau diese Riegel schützen die Hardware.

### 23Q.4 Ergebnis

```
Passed! - Failed: 0, Passed: 16, Skipped: 0, Total: 16, Duration: 800 ms
```

Offen bleibt (§33.4): Tests für den `Worker` selbst (HTTP-Client mit Fake-Handler), Integrations-
und Lasttests, 24-h-Dauerlauf.

---

## 23R. Messintervall und 24-h-Dauerlauf

> **Stand 2026-07-12/13.** Betriebsparameter und erster Stabilitätsnachweis (Phase 5).

### 23R.1 Messintervall: Abwägung und Entscheidung

Der Konfigurationsschlüssel `MeasurementIntervalSeconds` (§11.10) wurde am 2026-07-12 versuchsweise
auf **3600 Sekunden** (60 Minuten) gesetzt, dann aber **bewusst wieder auf 20 Sekunden
zurückgenommen** — mit dieser Begründung:

**Fachliches Argument für ein langes Intervall.** Wasser hat eine hohe spezifische Wärmekapazität.
Die Temperatur eines Aquariums ändert sich in Sekunden nicht messbar; ein Sekundentakt erzeugt
Datensätze ohne Informationsgewinn. Die Abtastrate sollte der **Änderungsrate der Messgröße**
folgen, nicht der technischen Möglichkeit. Nach diesem Argument wären 5–60 Minuten angemessen.

**Gegenargument für den kurzen Takt (ausschlaggebend im Projektkontext).** Der anstehende
24-h-Dauerlauf (§23R.3) ist ein **Zuverlässigkeitstest pro Zyklus**. Bei 3600 s hätte er
24 Zyklen umfasst — eine Stichprobe, aus der sich über die Stabilität von Polling-Schleife,
Sensor-Lesung, HTTP-Client und Sensor-Health-Eskalation praktisch nichts ableiten lässt. Ein
langes Intervall reduziert die Stichprobe, **ohne die Aussagekraft zu erhöhen**. Bei 20 s
umfasste derselbe Zeitraum **4.136 Zyklen** — ein rund 170-fach härterer Test.

**Entscheidung:** `MeasurementIntervalSeconds = 20` bleibt für die Projektlaufzeit, weil die
Testbarkeit hier höher wiegt als die Datenökonomie. Für einen realen Dauerbetrieb wäre ein Wert
im Minutenbereich (z. B. 300 s) die fachlich richtige Wahl; das ist als Ausblick zu benennen,
nicht als Versäumnis.

*Verifiziert am Pi:*

```bash
grep -ri "interval" /opt/aqms-worker/appsettings*.json
#   "PollIntervalSeconds": 10,
#   "MeasurementIntervalSeconds": 20,
```

*Lesson Learned (methodisch).* Die fachlich „richtige" Abtastrate und die für den **Test**
nützliche Abtastrate sind nicht dasselbe. Ein Parameter, der im Produktivbetrieb sinnvoll ist,
kann einen Testlauf wertlos machen. Diese Spannung gehört bewusst entschieden — nicht implizit
durch den Wert, der gerade in der Config steht.

### 23R.2 Latenter Defekt: `PiOnline`-Schwellwert ist implizit an das Messintervall gekoppelt

*Befund.* Der Online-Schwellwert im `DashboardController` ist hartkodiert:

```csharp
PiOnline = lastTs.HasValue && lastTs > DateTime.UtcNow.AddMinutes(-5)
```

Bei `MeasurementIntervalSeconds = 20` ist dieser Wert **korrekt** — der Pi meldet sich alle 20 s,
das 5-Minuten-Fenster ist großzügig und verkraftet auch mehrere verlorene POSTs. Der Defekt ist
daher **derzeit nicht aktiv**.

*Er wird aktiv, sobald das Intervall steigt.* Bei den zwischenzeitlich gesetzten 3600 s hätte das
Dashboard den Pi **55 von 60 Minuten** fälschlich als „kein aktueller Messwert" angezeigt. Dasselbe
gilt für das `[NotMapped]`-Property `Device.IsOnline` (30-Sekunden-Fenster, §3.2), das schon bei
einem Intervall von 30 s kippt.

*Kern des Problems.* Zwischen der **Worker-Konfiguration** (`MeasurementIntervalSeconds`, ein Wert
im Worker-Projekt) und dem **Web-Code** (Online-Schwellwert) besteht eine **implizite, nirgends
deklarierte Kopplung**. Der Web-Code trifft eine Annahme über das Verhalten eines anderen
Projekts. Ändert jemand die eine Zahl, bricht die Annahme **lautlos** — kein Compiler-Fehler,
kein fehlschlagender Test, keine Exception. Nur eine Oberfläche, die zuverlässig lügt.

*Vorgesehene Lösung.* Schwellwert = 2 × Messintervall + Puffer, als benannte Konstante, deren
Kommentar die Kopplung **explizit macht**:

```csharp
// MUSS größer sein als MeasurementIntervalSeconds des Workers (aktuell 20 s).
// Wird das Intervall erhöht, MUSS dieser Wert mitwachsen - sonst gilt der Pi
// zwischen zwei Messungen faelschlich als offline.
private static readonly TimeSpan PiOnlineFenster = TimeSpan.FromMinutes(5);
```

Sauberer wäre ein Konfigurationswert auf Web-Seite; für die Restlaufzeit ist der explizite
Kommentar der pragmatische Kompromiss und wird als solcher im Manuskript benannt.

*Lessons Learned.*
1. **Ein hartkodierter Zeit-Schwellwert ist eine versteckte Abhängigkeit zu einem anderen
   Systemteil.** Solange die Annahme zufällig stimmt, ist er unsichtbar. Er bricht erst, wenn
   jemand die Gegenseite ändert — und dann ohne jedes Signal.
2. **Latente Defekte sind gefährlicher als aktive.** Dieser hier wurde nur gefunden, weil das
   Intervall *versuchsweise* verändert wurde. Wäre die Änderung erst kurz vor der Abgabe
   erfolgt, wäre der Fehler in der Vorführung aufgetreten.

*Status:* **latent** — nicht aktiv bei der aktuellen Konfiguration, wird aktiv bei jeder Erhöhung
des Messintervalls. Als bedingter Defekt geführt (§33.3).

### 23R.3 Testprotokoll: 24-Stunden-Dauerlauf (Phase 5)

**Aufbau.** Raspberry Pi 3B+ mit DS18B20, Worker als systemd-Service, Messintervall 20 s,
Poll-Intervall 10 s. Ziel: Stabilität von Polling-Schleife, Sensor-Lesung, HTTP-Client und
Sensor-Health-Eskalation über einen realistischen Zeitraum **ohne Entwicklerinteraktion**.

**Zeitraum:** 2026-07-12 17:20 UTC – 2026-07-13 17:20 UTC (24 h, ununterbrochen).

**Ergebnis:**

| Kennzahl | Wert |
|---|---|
| Erfolgreiche Messzyklen | **4.136** |
| Fehlgeschlagene Zyklen | **0** |
| Fehler-/Warn-Einträge im Journal | **0** |
| Lücken in der Zeitreihe | **keine** (Abstand durchgehend ~20,9 s) |
| Service-Status nach 24 h | `active (running)`, Uptime 1 d 4 h |
| Temperatur (Min / Max / Mittel) | 26,25 °C / 28,06 °C / **27,22 °C** |

Auswertung:

```bash
sudo docker exec -i aqms-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -C -I -S localhost -U sa -P '<PASSWORD>' -d AQMS \
  -Q "SELECT COUNT(*) AS Anzahl, MIN(Timestamp) AS Von, MAX(Timestamp) AS Bis, \
      MIN(Value) AS MinC, MAX(Value) AS MaxC, AVG(Value) AS SchnittC \
      FROM Measurements WHERE Timestamp > DATEADD(hour,-24,GETUTCDATE())"
```

```bash
journalctl -u aqms-worker --since "25 hours ago" | grep -icE "error|warn|fehler"
# 0
```

**Bewertung.** Jeder der 4.136 Zyklen umfasst eine Sensor-Lesung über sysfs (inkl. CRC-Prüfung)
und einen HTTPS-`POST` an den VPS über den öffentlichen Internet-Pfad (Pi → NAT → Nginx →
Kestrel → SQL Server). **Keine einzige Fehlmessung, kein einziger fehlgeschlagener POST, kein
Reconnect.** Damit sind belegt:

- Stabilität der `PeriodicTimer`-Schleife über 24 h (kein Drift, kein Verhungern)
- Zuverlässigkeit der 1-Wire-Lesung und der CRC-Prüfung (§11.10)
- Robustheit des `IHttpClientFactory`-Clients über 4.136 Anfragen (kein Socket-Exhaustion,
  kein DNS-Problem — das war die Sorge, die zur Verwendung des benannten Clients führte, §11.6)
- Speicherstabilität des .NET-Prozesses unter systemd (kein Leck, kein OOM-Kill)

**Fachliche Plausibilisierung der Messwerte.** Die Zeitreihe zeigt einen zusammenhängenden
Tag-Nacht-Verlauf: Abfall vom Abend bis zum Minimum von 26,25 °C gegen 05:00 UTC, dann
kontinuierlicher Anstieg bis zum Maximum von 28,06 °C am Nachmittag. Die Kurve ist glatt —
Sprünge zwischen benachbarten Messungen liegen im Bereich der Sensorauflösung (0,0625 °C beim
DS18B20 in 12-Bit-Auflösung). Das ist **kein Rauschen, sondern Physik**: Die Wassertemperatur
folgt der Raumtemperatur mit der für die Wärmekapazität des Beckens typischen Trägheit.

Das ist zugleich der beste Beleg dafür, dass der Sensor tatsächlich misst und nicht etwa
konstante oder zufällige Werte liefert.

**Lessons Learned.**
1. **Ein Dauerlauf, dessen Ergebnis niemand nachrechnet, beweist nichts.** „Der Service läuft
   noch" ist keine Aussage. Erst das Auszählen der Zeitreihe (Anzahl, Abstände, Wertebereich)
   macht aus dem Lauf einen Test. Die Diagnose-Abfrage gehört deshalb *vor* den Lauf geplant,
   nicht danach improvisiert.
2. **Die Zahl der Zyklen ist die Stichprobengröße, nicht die Laufzeit.** Derselbe 24-Stunden-
   Zeitraum liefert bei 3600 s Intervall 24 Datenpunkte und bei 20 s 4.136 — bei identischem
   Aufwand. Die Testparameter sind eine eigene Entscheidung und dürfen nicht aus den
   Produktivwerten übernommen werden (§23R.1).

---

## 23S. Phase 5: Abschluss der Integrations- und Sicherheitstests

**Stand: 2026-08-06.** Mit diesem Tag sind alle neun Feinziele der Phase 5 aus
dem PSP (GZ 5.1 und GZ 5.2) durchgeführt und protokolliert. Diese Sektion
dokumentiert die an diesem Tag nachgeholten Prüfungen; die bereits während der
Entwicklung erbrachten Nachweise stehen in §23L.7 (API-Key), §23M.5
(MeasurementsController), §23N.5 (CommandsController), §23P.7 (End-to-End),
§23Q (Unit-Tests) und §23R.3 (Dauerlauf).

### 23S.1 Testschema und methodische Regeln

Alle Protokolle folgen einem festen Schema: Test-ID, Bezeichnung, Datum,
Umgebung, Vorgehen, Erwartung, Ergebnis, Bewertung, Beleg. Es liegt als eigene
Datei `AQMS_Testprotokolle.md` vor und speist Anhang D des Manuskripts sowie die
Ergebnisspalten der Tabellen 16 und 17 in Kapitel 9.

Drei Regeln prägen den Aufbau; die erste ist die direkte Konsequenz aus der
Lesson Learned in §23R.3:

1. **Erwartung und Auswertungsabfrage werden vor der Durchführung festgelegt.**
   Ein Test, dessen Bewertungsmaßstab erst danach entsteht, liefert eine
   Beobachtung und kein Ergebnis.
2. **Zu jedem Versuch, der eine Schutzmaßnahme prüft, gehört eine Gegenprobe mit
   gültigen Werten.** Ohne sie bleibt offen, ob der Schutz gegriffen hat oder ob
   der Versuch schlicht falsch angesetzt war — der Datenbestand sieht in beiden
   Fällen gleich aus.
3. **Der Nachweis wird, wo möglich, auf die Ursache gestützt und nicht auf die
   ausbleibende Wirkung.** Beim Einschleusungsversuch heißt das: das erzeugte SQL
   statt „es ist nichts passiert".

### 23S.2 Testumgebung: eigener Datenbank-Container

Für die lokalen Prüfungen wurde ein zweiter SQL-Server-Container
`aqms_DB_seedtest` auf Port **1434** gestartet, getrennt vom
Entwicklungscontainer auf 1433. Beide Datenbanken heißen `aqms` und unterscheiden
sich nur über den Port. Damit bleibt der Entwicklungsbestand unangetastet, und
der Migrationstest läuft nachweislich gegen eine leere Datenbank.

Die Verbindung wurde über `dotnet ef --connection` beziehungsweise über die
Umgebungsvariable `ConnectionStrings__DefaultConnection` übergeben. **Keine
Konfigurationsdatei und keine User Secrets wurden dafür verändert** — das ist
der Grund, diesen Weg dem Editieren von `appsettings` vorzuziehen.

### 23S.3 Migrationen auf leerer Datenbank (PSP 5.1.5)

Ausgangszustand über `dotnet ef migrations list --connection …`: zwei Migrationen
im Zustand `(Pending)`, `__EFMigrationsHistory` nicht vorhanden. Nach
`dotnet ef database update --verbose` enthält sie genau diese zwei Einträge.

| Prüfung | Ergebnis |
|---|---|
| Migrationen laufen durch | ✓ beide, fehlerfrei |
| Tabellen | 14 (8 Identity inkl. History, 6 fachlich) |
| Startdaten aus `HasData` | 2 Gerätetypen, 2 Messgrößen, 6 Geräte |
| Seeder nach App-Start | 2 Rollen, 2 Konten (`Admin`, `User`), beide Logins ok |

Der Test belegt **über die Erwartung hinaus** die Aussage aus §22, dass
Gerätetypen, Messgrößen und Smart Plugs über Migrationen mitversioniert
eingespielt werden und die Einrichtung damit auf jedem System reproduzierbar ist
— erstmals auf einer nachweislich leeren Datenbank.

### 23S.4 Rollentrennung: Aufruf der Schalt-Route unter Umgehung der Oberfläche (PSP 5.2.2)

**Die Falle.** `Dashboard/Toggle` trägt neben `[Authorize(Roles = AdminRole)]`
auch `[ValidateAntiForgeryToken]` (§23P.4). Ein naiver POST ohne Token wäre
abgewiesen worden — aber vom CSRF-Schutz, nicht von der Rollenprüfung. Der Test
hätte eine Ablehnung geliefert und über die Rollen nichts ausgesagt. Welcher der
beiden Autorisierungsfilter zuerst greift, wurde bewusst nicht ergründet: der
Test ist so gebaut, dass die Frage keine Rolle spielt.

**Vorgehen.** Der Antiforgery-Token hängt an der Sitzung, nicht am Formular. In
der Sitzung des Standardbenutzers zeigt das Dashboard zwar keine Schaltflächen,
das Abmelden-Formular des Identity-Layouts enthält aber ein gültiges
`__RequestVerificationToken`. Dieses wurde in einen `fetch`-Aufruf aus der
Browserkonsole übernommen.

| | Standardbenutzer (`User`) | Gegenprobe Administrator |
|---|---|---|
| Antwort | Umleitung auf `/Identity/Account/AccessDenied` | Umleitung auf `/Dashboard` |
| `DeviceCommands` | **keine** neue Zeile | genau eine Zeile, `Pending` |

**Erwartung korrigiert.** Der Testplan nannte „Statuscode 403". Das ist bei
Cookie-Anmeldung falsch: der Cookie-Handler leitet bei fehlender Berechtigung auf
`AccessDeniedPath` um, statt abzulehnen. Für die Schutzwirkung macht das keinen
Unterschied, für die Formulierung des Erwartungswerts sehr wohl. Das tragfähige
Kriterium ist der **Datenbestand**, nicht der Statuscode.

**Warum die Ablehnung eindeutig der Rollenprüfung zuzuordnen ist.** Eine
fehlgeschlagene Antiforgery-Prüfung endet mit 400 und leitet **nie** auf
`AccessDenied` um — dieser Pfad gehört ausschließlich dem Cookie-Handler und wird
nur bei angemeldeten, aber nicht berechtigten Anfragen angesteuert.

**Nebenbefund.** Der Aufruf enthielt keine Benutzerkennung im Rumpf,
`RequestedByUserId` ist dennoch korrekt gefüllt. Damit ist die Entscheidung aus
§23P.3 (UserId serverseitig, nie aus dem Request) empirisch belegt.

### 23S.5 Einschleusungsversuch (PSP 5.2.4)

Zwei Ebenen, weil ein Angriffsversuch ohne Wirkung für sich genommen nichts
belegt.

**Ebene 1 — strukturell.** Suche über den gesamten Projektcode nach
`FromSqlRaw`, `FromSqlInterpolated`, `ExecuteSqlRaw`, `ExecuteSqlInterpolated`,
`SqlQueryRaw` und `SqlCommand`: **kein einziger Treffer**. Es existiert kein
Codepfad, auf dem eine Zeichenkette Bestandteil einer Abfrage werden könnte.

**Ebene 2 — Angriffsversuch.** Nur zwei Zeichenketten aus einer Anfrage
erreichen überhaupt eine Abfrage: `deviceIdentifier` und `typeName` in
`GET /api/measurements`.

| Nutzlast | Ziel | Antwort |
|---|---|---|
| `shelly-filter' OR '1'='1` | `deviceIdentifier` | 400, „Unknown Device-Identifier: …" |
| `Temperature' OR '1'='1` | `typeName` | 400, „Unknown Measurement-Type: …" |
| `x'; DROP TABLE Measurements;--` | `deviceIdentifier` | 400, Tabellenzahl unverändert 14 |
| `5;DROP TABLE Measurements` | `limit` (int) | 400 aus der Modellbindung, ProblemDetails-JSON |
| gültige Werte (Gegenprobe) | — | 200 |

Die erste Antwort ist aussagekräftiger als ein bloßer Fehlercode: Wäre die
Einschleusung gelungen, hätte die angehängte Bedingung die Abfrage wahr gemacht
und ein beliebiges Gerät geliefert. Stattdessen meldet die Anwendung, ein Gerät
mit *genau dieser Zeichenkette* sei unbekannt — der Wert wurde vollständig als
Inhalt verglichen.

**Der eigentliche Beweis** steht im Anwendungsprotokoll:

```
Executed DbCommand [Parameters=[@deviceIdentifier='?' (Size = 100)], …]
SELECT TOP(1) … FROM [Devices] AS [d] WHERE [d].[DeviceIdentifier] = @deviceIdentifier
```

Die Nutzlast kommt im Abfragetext an keiner Stelle vor. Zwei Nebenbefunde: auch
die Obergrenze wird parametrisiert (`TOP(@p)`), und die Parameter tragen die
Längenangaben aus der Fluent-API (`Size = 100` / `Size = 50`, §19) — die
Modellkonfiguration wirkt bis in den Datenbanktreiber hinein.

**Anmeldeformular.** `' OR '1'='1` als E-Mail wird doppelt abgewiesen: durch die
Formularprüfung (ungültige Adresse) und durch Identity mit einer allgemein
gehaltenen Meldung, die nicht verrät, ob ein Konto existiert.

**Fehlerdarstellung.** Dieser Teil ließ sich an den Einschleusungsversuchen
nicht prüfen — sie erzeugten sämtlich saubere 400er, es gab nichts zu verbergen.
Geprüft wurde deshalb an einer *echten* Ausnahme: Anwendung in
Produktionskonfiguration gestartet, Datenbankcontainer im laufenden Betrieb
gestoppt, Aufruf abgesetzt. Ergebnis: **500** mit der allgemeinen Fehlerseite,
ohne Stack-Trace, ohne Dateipfade, ohne Bestandteile des Connection-Strings. Der
unmittelbar davor abgesetzte Kontrollaufruf lieferte 200.

**Nebenbefund (offen).** Die Fehlerantwort kommt als `text/html`, obwohl der
Aufruf an `/api/...` ging. Ein Client, der JSON erwartet, erhält im Fehlerfall
eine vollständige Webseite. Der Worker übersteht das wegen seiner
Exception-Behandlung (§11.6), es steht aber quer zum Statuscode-Vertrag aus
§23M.2. Aufgenommen in §33.

### 23S.6 Transportverschlüsselung (PSP 5.2.1)

Qualys SSL Labs gegen `aqms.aqms.example.com`, 2026-08-06 09:27 UTC:

| Kriterium | Ergebnis |
|---|---|
| Gesamtnote | **A** |
| Certificate / Protocol Support | 100 / 100 |
| Key Exchange / Cipher Strength | ~90 / ~90 |
| Besonderheiten | TLS 1.3, PQC-Key-Exchange unterstützt |

Die volle Punktzahl bei Protocol Support bedeutet, dass die veralteten
Protokollfassungen abgeschaltet sind — damit ist auch der zweite Teil der
Erwartung („keine veralteten Verfahren") erfüllt. Die Note A statt A+ ist
gemessen an der Erwartung kein Mangel; naheliegende Ursache ist die
HSTS-Gültigkeitsdauer (ASP.NET Core setzt 30 Tage, A+ verlangt 180). **Vermutung,
nicht nachgewiesen.**

### 23S.7 Schaltdurchlauf über alle fünf Geräte (PSP 5.1.3)

Ausgeführt am Produktivsystem über das Dashboard. Vor dem Durchlauf wurden
Startmarke und Auswertungsabfrage festgelegt (Regel 1 aus §23S.1). Zwischen den
Geräten wurde der neue Zustand abgewartet — der Idempotenz-Riegel (§23P.3) lässt
für dasselbe Gerät keinen zweiten offenen Befehl zu, zu schnelles Klicken würde
also den Riegel statt den Durchlauf messen.

**Ergebnis:** 9 Befehle über alle 5 Geräte, sämtlich `Executed`, keiner offen,
keiner fehlgeschlagen; 9 passende `StateChanges`-Einträge mit dem auslösenden
Admin-Konto. Der Filter wurde nur eingeschaltet (Technikschonung), seine
Schaltfähigkeit war zuvor mehrfach belegt (§7.5). Physische Bestätigung durch
hörbares Relais-Klicken an jedem Gerät.

**Erste Messung der Schaltlatenz.**

| Anzahl | Min | Max | Mittel |
|---|---|---|---|
| 9 | 5 s | 9 s | 6,9 s |

§3.7 des Manuskripts leitet rechnerisch 5 s im Mittel und 10 s maximal her. Der
gemessene Höchstwert liegt innerhalb dieser Grenze, das Mittel darüber. **Kein
Widerspruch, sondern eine andere Messgröße:** die Herleitung beschreibt allein
die Wartezeit bis zum *Abholen*, die Messung reicht bis zum *Verbuchen* und
enthält zusätzlich den Shelly-Aufruf und die Rückmeldung. Hinzu kommt, dass die
Schleife im selben Durchlauf auch den Sensor liest (§11.10), wodurch der
tatsächliche Takt geringfügig über dem Intervall liegt. Bei n=9 ist das Mittel
eine Größenordnung, keine belastbare Kennzahl.

Damit ist zugleich die Nachvollziehbarkeit aus §23P.3 (StateChange je Vorgang mit
UserId) erstmals über eine Serie belegt.

### 23S.8 Prüfung der Programmierschnittstelle (PSP 5.1.4)

Dieses Feinziel war im Kern bereits während der Entwicklung erledigt; die
Verifikationstabellen in §23M.5 (2026-05-31 lokal, 2026-06-01 VPS) und §23N.5
(2026-06-04 lokal + VPS) decken den vollständigen Statuscode-Vertrag aus §23M.2
ab: 401 ohne Schlüssel, 201 beim Anlegen, 200 beim Abruf, 400 bei unbekanntem
fachlichem Identifier, 404 bei unbekannter `CommandId`, 409 bei doppelter
Ergebnismeldung.

**Der 409 ist der wichtigste Einzelfall**, weil das Worker-Verhalten daran hängt
(§11.9): eine doppelt zugestellte Meldung wird als Erfolg behandelt, sobald das
Backend so antwortet. Die Unit-Tests (§23Q.3) decken die Entscheidung im Service
ab, **nicht** deren Übersetzung in einen HTTP-Statuscode — diese geschieht erst
im Controller und wird ausschließlich durch diese Prüfung erfasst.

Ergänzt am 2026-08-06 wurden die beiden Randfälle beim **Anlegen** eines
Messwerts (unbekannter `DeviceIdentifier`, unbekannter `MeasurementTypeName`,
jeweils 400).

**Werkzeug-Abweichung.** Der PSP nennt „Postman-Collection oder xUnit
Integrationstests". Ausgeführt wurde mit `Invoke-WebRequest` und `curl`. Geprüft
wurde derselbe Gegenstand auf demselben Weg über HTTP; die Abweichung betrifft
allein das Werkzeug und ist im Manuskript offenzulegen.

### 23S.9 Praxisprobleme

**1. `launchSettings.json` überschreibt die gesetzte Umgebung.**
*Symptom:* `$env:ASPNETCORE_ENVIRONMENT = "Production"` gesetzt, `dotnet run`
gestartet — die Startausgabe meldet trotzdem `Hosting environment: Development`,
und die App lauscht auf einem anderen Port als erwartet.
*Ursache:* `dotnet run` verwendet standardmäßig ein Startprofil aus
`AQMS.Web/Properties/launchSettings.json`; dessen `environmentVariables`-Block
wird auf den Prozess angewendet und gewinnt gegen die Sitzungsvariablen. Die
erste Zeile der Ausgabe sagt es sogar explizit.
*Lösung:* `dotnet run --project AQMS.Web --no-launch-profile`.
*Lessons Learned:* `launchSettings.json` ist eine reine Entwicklerdatei, wird
beim Publish nicht mitgeliefert und existiert weder auf dem VPS noch auf dem Pi.
Dort setzt systemd die Variablen (§23H.3, §23O). Derselbe Mechanismus, einmal von
Visual Studio und einmal von systemd bedient — wer das nicht weiß, misst lokal
etwas anderes als im Betrieb.

**2. Fehlender und falscher API-Key sind von außen ununterscheidbar.**
*Symptom:* Alle `/api/...`-Aufrufe im Produktionslauf mit 401, obwohl der
Schlüssel korrekt im Header stand.
*Ursache:* Die Middleware wirft 401, wenn **entweder** der mitgeschickte
**oder** der konfigurierte Schlüssel fehlt (§23L.3). In der Produktionsumgebung
werden User Secrets nicht geladen — `config["ApiKey"]` war leer.
*Lösung:* Für Produktionsläufe alle Schlüssel als Umgebungsvariablen setzen
(`ApiKey`, `AdminBenutzer__*`, `ConnectionStrings__*`).
*Lessons Learned:* Die Ununterscheidbarkeit ist **beabsichtigtes**
Sicherheitsverhalten — andernfalls verriete die Antwort, ob der Server überhaupt
einen Schlüssel erwartet. Für die eigene Fehlersuche ist sie unbequem; das ist
der akzeptierte Preis. Verwandt mit §23L.8: dieselbe Klasse „Konfiguration nicht
angekommen, Symptom aber weit entfernt von der Ursache".

**3. Anwendung im Vordergrund beendet.**
*Symptom:* `curl` meldet „Could not connect to server", obwohl die App zuvor
gestartet wurde.
*Ursache:* `dotnet run` lief im Vordergrund desselben PowerShell-Fensters; mit
Rückkehr der Eingabeaufforderung war der Prozess beendet.
*Lösung:* Zwei Fenster — eines für den Dienst, eines für die Aufrufe.
*Lessons Learned:* Trivial, aber im Protokoll wäre der Fehlschlag ohne
Kontrollaufruf fälschlich der Datenbank zugeschrieben worden. Genau dafür ist
Regel 2 aus §23S.1 da.

### 23S.10 Lessons Learned

1. **Ein Test kann am falschen Filter scheitern und trotzdem „bestanden"
   aussehen.** Beim Rollentest hätte der CSRF-Schutz dieselbe Beobachtung erzeugt
   wie die Rollenprüfung. Wer nur „es wurde abgewiesen" protokolliert, weiß
   nicht, *wer* abgewiesen hat.
2. **Die Umgebung ist Teil des Testaufbaus.** Die Prüfung auf ausbleibende
   Fehlerdetails hätte in Development das genaue Gegenteil des
   Betriebsverhaltens gezeigt. Ein Testergebnis ohne Angabe der Umgebung ist bei
   ASP.NET Core wertlos.
3. **Der stärkste Nachweis ist die Ursache, nicht die ausbleibende Wirkung.**
   Das erzeugte SQL belegt, *warum* keine Einschleusung möglich ist; die
   unveränderte Tabellenzahl belegt nur, dass diesmal nichts passiert ist.
4. **Prüfungen fördern Belege für Aussagen zutage, die vorher nur behauptet
   waren.** Der Schaltdurchlauf lieferte die erste Messung zur hergeleiteten
   Schaltlatenz (§3.7 des Manuskripts), der Migrationstest den ersten Nachweis
   der reproduzierbaren Einrichtung, das SQL-Protokoll den Beleg, dass die
   Fluent-API-Längen bis in den Treiber wirken.

---

## 23. VPS-Vorbereitung und Runtime-Installation

> **HISTORISCH — alter VPS (Debian 12).** Diese Sektion beschreibt die
> Einrichtung des ersten VPS und gilt nicht mehr für den aktuellen
> Server-Stand. Aktuelle Server-Einrichtung: §23A–§23F. Die Sektion
> bleibt als Versionsstand und Lernmaterial erhalten.

### 23.1 Ausgangslage

VPS bereits vorhanden, betreibt mehrere Webseiten unter
`aqms.example.com` über Nginx. Ziel: AQMS-Webanwendung als zusätzliche
Subdomain hosten, ohne bestehende Dienste zu stören.

| Eckdatum | Wert |
|---|---|
| Domain | aqms.example.com → 203.0.113.11 |
| OS | Debian 12 (bookworm) |
| User | deployuser |
| Bereits aktive Dienste | Nginx (80/443), mehrere Node-Dienste (8000, 8080, 8081) |

### 23.2 DNS-Prüfung

```bash
nslookup aqms.example.com
```

Bestätigt IP `203.0.113.11`. Subdomains (`aqms.aqms.example.com`) per
Wildcard-A-Record auf gleiche IP geroutet.

### 23.3 SSH-Verbindung

```bash
ssh deployuser@203.0.113.11
```

Erste Verbindung: Host-Fingerprint bestätigen → in `known_hosts`.

### 23.4 OS prüfen

```bash
cat /etc/os-release
```

Bestätigt: Debian 12 (bookworm). Wichtig für die richtige Microsoft-Paketquelle.

### 23.5 System aktualisieren

```bash
sudo apt update && sudo apt upgrade -y
```

Sicherheitsupdates und Kernel-Update bei dieser Gelegenheit installiert.

### 23.6 Microsoft-Paketquelle einbinden

```bash
wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt update
```

Damit kennt apt die `dotnet-`/`aspnetcore-`-Pakete.

### 23.7 ASP.NET Core Runtime 10.0 installieren

```bash
sudo apt install -y aspnetcore-runtime-10.0
```

Installierte Komponenten:

- dotnet-hostfxr-10.0
- dotnet-runtime-deps-10.0
- dotnet-runtime-10.0
- aspnetcore-runtime-10.0

Verifikation:

```bash
dotnet --list-runtimes
dotnet --info
```

Output zeigt:

- `Microsoft.AspNetCore.App 10.0.5`
- `Microsoft.NETCore.App 10.0.5`

**Kein SDK** auf dem Server — Build erfolgt auf dem
Entwicklungsrechner, nur die Runtime ist nötig.

---

## 24. Deploy-Verzeichnis und manueller Test

> **HISTORISCH — alter VPS (Debian 12).** Aktueller Server-Stand: §23A–§23F.

### 24.1 Deploy-Verzeichnis anlegen

```bash
sudo mkdir -p /var/www/aqms
sudo chown -R deployuser:deployuser /var/www/aqms
```

Eigenes Verzeichnis pro Anwendung — saubere Trennung von anderen
Webprojekten auf dem VPS.

### 24.2 Test-DLL hochladen

Vom Windows-Entwicklungsrechner per FileZilla oder `scp`. Ein einfaches
ASP.NET-Core-Projekt wurde mit `dotnet publish` veröffentlicht und in
`/var/www/aqms` abgelegt.

### 24.3 Manueller Start zum Testen

```bash
cd /var/www/aqms
dotnet TestApplication.dll
```

Ausgabe:

- `Now listening on: http://localhost:5000`
- `Hosting environment: Production`
- `Content root path: /var/www/aqms`

**Damit nachgewiesen:**

- Runtime ist korrekt installiert
- Veröffentlichte DLL ist ausführbar
- Anwendung startet auf dem VPS

Warnungen (nicht kritisch für Test):

- Fehlende `wwwroot`-Struktur (Test-App hatte keine)
- Unverschlüsselte Speicherung des Data-Protection-Schlüssels (für Test ok,
  für Produktion zu beheben)

---

## 25. systemd-Service und 217/USER-Problem

> **HISTORISCH — alter VPS (Debian 12).** Aktueller Server-Stand: §23A–§23F.
> Der systemd-Service auf dem neuen Server wird nach Durchführung ergänzt.

### 25.1 Ziel

Anwendung soll **dauerhaft als Linux-Dienst** laufen, automatisch nach
Reboot starten. Standard-Mechanismus auf Debian: systemd.

### 25.2 Erste Service-Datei und Fehler

Erste systemd-Konfiguration angelegt unter `/etc/systemd/system/aqms-web.service`.

```bash
sudo systemctl daemon-reload
sudo systemctl enable aqms-web
sudo systemctl start aqms-web
sudo systemctl status aqms-web
```

**Fehler:** `status=217/USER`

Detail:

```
Failed to determine user credentials
Failed at step USER spawning /usr/bin/dotnet
```

### 25.3 Diagnose: das Problem ist die Service-Datei, nicht die App

Der manuelle Start mit `dotnet TestApplication.dll` funktionierte ja. Der
Fehler `217/USER` deutet auf ein Problem bei der **User-Auflösung** in der
systemd-Service-Datei hin — der konfigurierte User existierte nicht oder
war nicht für Service-Betrieb geeignet.

### 25.4 Lösung: dedizierter Service-User

```bash
sudo useradd -r -s /usr/sbin/nologin aqms
```

Erklärung:

- `-r` → System-User (kein normaler Login-User)
- `-s /usr/sbin/nologin` → kann sich nicht einloggen
- `aqms` → User-Name

Dieser User existiert ausschließlich, um den Dienst auszuführen — wird
nicht für Login oder andere Zwecke verwendet.

Anschließend Verzeichnisrechte angepasst:

```bash
sudo chown -R aqms:aqms /var/www/aqms
sudo chmod -R 755 /var/www/aqms
```

### 25.5 Finale Service-Datei

Service-Datei (Auszug) `/etc/systemd/system/aqms-web.service`:

```ini
[Unit]
Description=AQMS Web Backend
After=network.target

[Service]
Type=simple
WorkingDirectory=/var/www/aqms
ExecStart=/usr/bin/dotnet /var/www/aqms/AQMS.Web.dll
Restart=always
RestartSec=10
User=aqms
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000

[Install]
WantedBy=multi-user.target
```

Reload und Restart:

```bash
sudo systemctl daemon-reload
sudo systemctl restart aqms-web
sudo systemctl status aqms-web
```

Status: `Active: active (running)`. Anwendung lauscht auf
`http://0.0.0.0:5000` — auf allen Netzwerk-Interfaces.

### 25.6 Lerneffekt für die Diplomarbeit

> *Der erste systemd-Versuch scheiterte mit `status=217/USER`. Das war
> kein Problem der Anwendung selbst — der manuelle Test mit
> `dotnet TestApplication.dll` lief problemlos durch. Die Ursache lag
> ausschließlich in der Dienstkonfiguration: der konfigurierte User
> existierte nicht. Die Lösung bestand im Anlegen eines dedizierten
> System-Users `aqms` ohne Login-Shell. Diese Trennung folgt dem Prinzip
> der minimalen Privilegien — die Webanwendung läuft unter einem User
> ohne Login-Möglichkeit, das reduziert die Angriffsfläche.*

---

## 26. Nginx Reverse Proxy

> **HISTORISCH — alter VPS (Debian 12).** Aktueller Server-Stand: §23A–§23F.
> Die Nginx-Konfiguration auf dem neuen Server wird nach Durchführung ergänzt.

### 26.1 Port-Analyse vor Eingriff

```bash
sudo ss -tulpn | grep ':80\|:443\|:5000'
```

Ergebnis:

- AQMS-App auf `0.0.0.0:5000`
- Nginx auf `0.0.0.0:80` (HTTP)
- Nginx auf `0.0.0.0:443` (HTTPS)
- Mehrere Node.js-Dienste auf 8000, 8080, 8081

**Erkenntnis:** Port 5000 ist für AQMS frei. Nginx auf 80/443 darf nicht
ersetzt werden — nur erweitert.

### 26.2 Bestehende Nginx-Konfiguration prüfen

Aktive Sites:

- `aqms.example.com`
- `www.aqms.example.com`
- `vorschau.aqms.example.com`

**Entscheidung:** AQMS bekommt eine **eigene Subdomain**, statt in
bestehende Konfigs eingebaut zu werden. Vorteile:

- klare fachliche und technische Abgrenzung
- saubere Reverse-Proxy-Konfiguration
- saubere Grundlage für TLS-Zertifikat
- geringes Risiko für Kollisionen

### 26.3 Subdomain `aqms.aqms.example.com`

DNS war bereits vorhanden (Wildcard auf VPS-IP). Nginx-Site angelegt:

```bash
sudo nano /etc/nginx/sites-available/aqms.aqms.example.com
sudo ln -s /etc/nginx/sites-available/aqms.aqms.example.com /etc/nginx/sites-enabled/aqms.aqms.example.com
sudo nginx -t        # Syntax-Check
sudo systemctl reload nginx
```

### 26.4 Reverse-Proxy-Konfiguration (Auszug)

```nginx
server {
    listen 80;
    server_name aqms.aqms.example.com;

    location / {
        proxy_pass http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

**Hosting-Modell:**

```
Browser → Nginx (Port 443, HTTPS) → Kestrel (127.0.0.1:5000, HTTP intern)
```

Nginx terminiert HTTPS und leitet an Kestrel weiter. Kestrel kennt nur
HTTP intern — Nginx kümmert sich um TLS.

---

## 27. HTTPS mit Let's Encrypt

> **HISTORISCH — alter VPS (Debian 12).** Aktueller Server-Stand: §23A–§23F.
> Das HTTPS-Setup auf dem neuen Server wird nach Durchführung ergänzt.

### 27.1 Zertifikat ausstellen

Mit Certbot (war bereits installiert):

```bash
sudo certbot --nginx -d aqms.aqms.example.com
```

Certbot:

- prüfte HTTP-Erreichbarkeit der Subdomain
- forderte ein Zertifikat von Let's Encrypt an
- baute die HTTPS-Konfiguration automatisch in die Nginx-Site ein
- konfigurierte HTTP→HTTPS-Redirect

Bestätigung:

- Zertifikat erfolgreich erhalten
- Zertifikat in Nginx eingebunden
- HTTPS aktiviert für `https://aqms.aqms.example.com`
- Gültig bis **20.06.2026**

### 27.2 Auto-Renewal

Certbot installiert automatisch einen systemd-Timer, der Zertifikate vor
Ablauf erneuert:

```bash
sudo systemctl status certbot.timer
sudo certbot renew --dry-run    # Test-Erneuerung
```

Damit ist HTTPS dauerhaft ohne manuellen Aufwand abgesichert.

### 27.3 Resultat

Vollständige HTTPS-Hosting-Kette:

```
Internet
   │  https://aqms.aqms.example.com (TLS, Port 443)
   ▼
Nginx (Reverse Proxy + TLS-Terminator)
   │  http://127.0.0.1:5000 (intern, plaintext)
   ▼
systemd-Service aqms-web (User: aqms)
   │
   ▼
dotnet /var/www/aqms/AQMS.Web.dll
```

---

## 28. Praxisproblem: Upload-Rechte für deployuser

> **HISTORISCH — alter VPS (Debian 12).** Aktueller Server-Stand: §23A–§23F.
> Das hier beschriebene Praxisproblem ist beim Deployment auf dem neuen
> Server zu beachten.

### 28.1 Problem

Nach `sudo chown -R aqms:aqms /var/www/aqms` (für systemd-Service)
konnte `deployuser` keine Dateien mehr ins Deploy-Verzeichnis hochladen
(z.B. via FileZilla). Logisch: dem User `deployuser` fehlt die
Schreibberechtigung, weil das Verzeichnis nun `aqms` gehört.

### 28.2 Trade-Off

| Option | Vorteil | Nachteil |
|---|---|---|
| `aqms` als Owner behalten | systemd-konform, Best Practice | manueller Upload nicht mehr möglich |
| `deployuser` als Owner | Upload möglich | Service muss trotzdem lesen können |

### 28.3 Pragmatische Lösung

```bash
sudo chown -R deployuser:deployuser /var/www/aqms
sudo chmod -R 755 /var/www/aqms
```

Mit `chmod 755` haben:

- Owner (`deployuser`): rwx (Lesen, Schreiben, Ausführen)
- Group: r-x (Lesen, Ausführen)
- Others: r-x (Lesen, Ausführen)

Das bedeutet: `deployuser` kann **uploaden**, der Service-User `aqms` kann
**lesen und ausführen** — beide Welten zufrieden.

### 28.4 Kompromiss-Bewertung

In einem sicherheitskritischen Produktionssystem wäre die saubere Lösung:

- Deploy-Pipeline mit GitHub Actions oder ähnlichem (kein manueller Upload)
- Upload nur über CI/CD-User mit eingeschränkten Rechten
- `aqms` bleibt strikter Owner

Für AQMS als Diplomarbeitsprojekt mit manuellem Workflow ist die `755`-
Lösung mit Owner `deployuser` akzeptabel und pragmatisch.

---

# Teil F — Diplomarbeit-Material

## 29. Argumentationen für die Verteidigung

Vorgefertigte Antworten auf typische Fragen:

### 29.1 Zur Code-First-Entscheidung

> *Die Datenbank wurde nach dem Code-First-Ansatz von Entity Framework Core
> modelliert. Die C#-Entitätsklassen sind die maßgebliche Beschreibung des
> Schemas; aus ihnen werden mit dem EF-Core-Tooling versionierte
> Migrationsskripte generiert. Vorteile: Schema-Änderungen sind im
> Source-Control versioniert, reproduzierbar auf jedem System, und die
> Konsistenz zwischen Anwendungs- und Datenmodell ist durch den Compiler
> garantiert.*

### 29.2 Zur Vererbung von IdentityDbContext

> *Die DbContext-Klasse erbt von IdentityDbContext mit dem Generic-Parameter
> IdentityUser. Damit werden die sieben ASP.NET-Identity-Tabellen automatisch
> in das Schema integriert, und die Authentifizierungslogik nutzt dieselbe
> Datenbank-Verbindung wie die fachlichen Entitäten.*

### 29.3 Zu Fluent API über Annotations

> *Die Persistenzkonfiguration erfolgt zentral in OnModelCreating mittels
> EF-Core-Fluent-API. Diese Wahl ergibt sich aus dem Bedarf an
> Konfigurationsmöglichkeiten, die nur per Fluent API verfügbar sind:
> Composite-Indizes, gefilterte Indizes, OnDelete-Verhalten,
> Enum-Konvertierungen und DB-seitige Default-Werte.*

### 29.4 Zu OnDelete Restrict

> *Alle Pflichtbeziehungen verwenden DeleteBehavior.Restrict statt des
> EF-Core-Defaults Cascade. Damit wird verhindert, dass das Löschen eines
> Parent-Datensatzes zur kaskadierenden Vernichtung historischer Children
> führt. In einer Anwendung, die über Monate Sensordaten und Steuerbefehle
> akkumuliert, ist Datensicherheit gegen versehentliche Löschungen
> essenziell.*

### 29.5 Zur Surrogate-Key + Natural-Key Strategie

> *Das Device-Modell verwendet zwei eindeutige Schlüsselattribute: einen
> technischen Surrogate Key Id (int, IDENTITY) und einen fachlichen Natural
> Key DeviceIdentifier (nvarchar UNIQUE). Der Surrogate Key dient als PK und
> FK in Beziehungen wegen seiner Performance-Eigenschaften. Der Natural Key
> dient der externen Adressierung wegen seiner semantischen Bedeutung und
> Stabilität über DB-Resets. Beide sind direkt vom PK funktional abhängig
> — keine transitive Abhängigkeit, daher 3NF-konform.*

### 29.6 Zur Trennung Measurements und StateChanges

> *Stetige Sensorwerte und diskrete Zustandsänderungen werden in zwei
> getrennten Tabellen modelliert. Stetige Werte sind aggregierbar
> (Durchschnitte, Verläufe), Schaltereignisse sind Domain-Events mit
> anderen analytischen Anforderungen (Häufigkeit, Audit-Trail). Diese
> Trennung folgt dem Single-Responsibility-Prinzip auf Schema-Ebene und
> vermeidet die in IoT-Systemen häufige Vermischung von Messung und
> Ereignis.*

### 29.7 Zur Enum-String-Konvertierung

> *State-Werte sind in C# als typsichere Enums modelliert, werden aber als
> nvarchar gespeichert mittels HasConversion. Diese Strategie verbindet
> Typsicherheit der Anwendungsschicht mit Lesbarkeit der DB-Repräsentation.
> Der gefilterte Index auf Status='Pending' setzt diese
> String-Speicherung voraus.*

### 29.8 Zum filtered Index

> *Der Pi pollt alle zehn Sekunden die DeviceCommands-Tabelle nach
> ausstehenden Befehlen. Da die meisten Befehle bereits bearbeitet wurden
> (Status 'Executed' oder 'Failed'), wäre ein normaler Index auf Status
> ineffizient. Der gefilterte Index indiziert nur die wenigen
> 'Pending'-Zeilen und liefert sie unabhängig von der Gesamttabellengröße in
> O(log n).*

### 29.9 Zur Drei-Spalten-Strategie für Schaltzustände

> *Das Schema unterscheidet drei Konzepte: IsEnabled (System-Verwaltung),
> CurrentState (aktueller Zustand als Performance-Cache), StateChanges
> (vollständige Event-Historie). CurrentState ist eine bewusste
> Denormalisierung im Sinne der CQRS-Idee — ableitbar aus StateChanges,
> aber persistent für Read-Performance auf dem Live-Dashboard.*

### 29.10 Zum Polling-Pattern

> *Der Pi sitzt im privaten Heimnetz hinter NAT und ist vom Backend nicht
> direkt erreichbar. Statt eines Push-Patterns mit VPN oder Reverse-Tunnel
> wurde Polling gewählt: der Pi fragt regelmäßig beim Backend nach. Das ist
> die einfachste, robusteste und am besten dokumentierbare Variante. Der
> filtered Index auf Status='Pending' macht das Polling effizient.*

### 29.11 Zur Shelly-API-Wahl

> *Die Steuerung der Shelly Smart Plugs erfolgt über eine lokale HTTP-
> bzw. RPC-basierte API. Die verwendeten Endpunkte und Methoden basieren
> auf der offiziellen Shelly-API-Dokumentation (Shelly, 2024), insbesondere
> den Komponenten "Switch" sowie dem JSON-RPC-Protokoll. Der klassische
> HTTP-Aufruf wurde für den schnellen Hardwaretest genutzt, während das
> moderne RPC-Modell die offizielle API-Struktur aktueller
> Shelly-OS-Geräte darstellt.*

### 29.12 Zum Reverse-Proxy-Setup

> *Auf dem Linux-VPS wird Kestrel als interner Webserver auf Port 5000
> betrieben und von Nginx auf Port 443 mit TLS-Terminierung als Reverse
> Proxy vorgeschaltet. Dieses Modell entspricht der offiziellen
> Microsoft-Empfehlung für Linux-Hosting und ist Standard in
> Produktionsumgebungen. Vorteile: zentrale TLS-Verwaltung über
> Let's Encrypt, mehrere Domains/Subdomains parallel auf einem Server,
> verbesserte Sicherheit gegenüber direkter Kestrel-Exposition.*

---

## 30. Reflexion: Verworfene Alternativen

In der Diplomarbeit-Verteidigung punktet, wer **bewusst verworfene Alternativen**
benennen kann.

### 30.1 State als Measurement (verworfen)

**Idee:** State als MeasurementType "State" mit Wert 0.0/1.0 in der
Measurements-Tabelle.

**Verworfen wegen:** semantischer Vermischung — Schaltereignisse sind keine
Messungen. Bool als float speichern fühlt sich falsch an. AVG(State) macht
fachlich Sinn (Anteil Einschaltzeit), aber der Standard-IoT-Approach hat
diese Mischung trotz besserer Aggregationsfähigkeit oft kritisiert.

**Stattdessen:** eigene `StateChanges`-Tabelle.

### 30.2 Guid statt int als PK (verworfen)

**Idee:** Guid als PK für alle Entities — distributed-friendly,
professioneller Eindruck.

**Verworfen wegen:**

- bei sechs Devices und einer einzigen Schreibstelle (VPS-DB) Overkill
- ERD und DB-Doku waren bereits mit `int` ausgearbeitet
- `int` ist schneller in Joins und kompakter in Indizes

**Stattdessen:** `int` für fachliche Tabellen, `long` für Time-Series-Tabellen.

### 30.3 Pi nicht als Device modellieren (verworfen)

**Idee:** Pi außerhalb der Datenbank, Measurements ohne Device-Bezug.

**Verworfen wegen:** asymmetrisch, der Pi ist genauso ein steuerbares
Gerät mit IP wie die Shellys.

**Stattdessen:** Pi als Device mit DeviceType "Sensor".

### 30.4 Eigene Capability-Tabelle (verworfen)

**Idee:** n:m-Tabelle DeviceCapabilities, die festlegt welcher DeviceType
welche Messwerttypen liefern kann.

**Verworfen wegen:** Overkill für 6 Geräte. Sinnvoll in größeren
IoT-Systemen, aber unverhältnismäßig.

**Stattdessen:** Disziplin im Worker-Code (Pi liefert Temperature, Shellys
liefern Power).

### 30.5 Nur DbContext, ohne IdentityDbContext (initial verworfen)

**Idee initial:** AqmsDbContext erbt von DbContext, separate User-Tabelle.

**Verworfen wegen:** ASP.NET Identity bringt funktionierende Auth, Login,
Passwort-Hashing, Rollen-System mit. Keinen Sinn das selbst zu bauen.

**Stattdessen:** Erbung von `IdentityDbContext<IdentityUser>`.

### 30.6 Hardware-spezifische Spalte ShellyIP (umgestellt)

**Idee initial:** Spalte heißt `ShellyIP`.

**Umgestellt zu** `IPAddress` weil:

- semantisch falsch wenn der Pi seine IP da rein bekommt
- generischer Name erlaubt zukünftige Geräte (Tasmota, anderer Pi) ohne
  Schemaänderung

### 30.7 DeviceTypes nach Funktion (umgestellt)

**Idee initial:** 5 DeviceTypes nach Funktion (Filter, Licht, CO2-Anlage,
Surface Skimmer, Heizstab).

**Umgestellt zu** 2 DeviceTypes nach Hardware-Klasse (Sensor, SmartPlug)
weil:

- Die Funktion ist eine Property des konkreten Devices, nicht des Typs
- Pi und Shellys sind funktional identisch im Sinne der DB
  (haben IP, LastSeen, Measurements...)
- Erweiterbar: ein zweiter Sensor-Typ-Pi würde sich einfach einreihen

### 30.8 Command/Status mit CHECK-Constraints (umgestellt)

**Idee initial:** `Command nvarchar(10)` und `Status nvarchar(20)` mit
expliziten CHECK-Constraints `IN ('on','off')` und
`IN ('pending','executed','failed')`.

**Umgestellt zu** typed Enums mit `HasConversion<string>()`:

- Action: DeviceState (On/Off)
- Status: CommandStatus (Pending/Executed/Failed)

**Begründung:** Typsicherheit auf C#-Seite (IntelliSense, Refactoring,
Compile-Zeit-Fehler), Lesbarkeit auf DB-Seite, kein Wertebereichs-Abgleich
zwischen Code und CHECK-Constraint nötig.

### 30.9 SignalR / WebSocket statt Polling (verworfen)

**Idee:** VPS pusht Befehle direkt an den Pi via WebSocket.

**Verworfen wegen:** NAT — VPS kann die private Pi-IP nicht erreichen.

**Stattdessen:** Polling (siehe §3.2).

### 30.10 Direktes Hosting ohne Reverse Proxy (verworfen)

**Idee:** Kestrel direkt auf Port 443.

**Verworfen wegen:**

- Kestrel ist nicht für direkte Internet-Exposition designed
- TLS-Verwaltung kompliziert
- mehrere Subdomains auf einem Server nicht möglich
- entgegen offizieller Microsoft-Empfehlung

**Stattdessen:** Nginx Reverse Proxy mit Let's Encrypt.

### 30.11 Swap-File bzw. PostgreSQL-Wechsel statt neuem Server (verworfen)

**Kontext:** Der erste VPS (Debian 12, 1,9 GB RAM) konnte SQL Server für
Linux nicht betreiben — die Engine verlangt mindestens 2 GB RAM. Drei
Optionen standen zur Wahl (ausführlich in §23A.2):

**Idee A — Swap-File einrichten:** 4 GB Swap auf dem alten VPS, damit die
SQL-Server-Engine trotz knappem RAM startet.

*Verworfen wegen:* Swap liegt auf der Festplatte, nicht im RAM. Unter Last
hätte die Datenbank spürbar verzögert reagiert; für den geplanten
24h-Dauerlauf (Phase 5) hätte das die Messergebnisse durch
speicherbedingte Latenzspitzen verfälscht. Zudem wäre nach Abzug des
Engine-Bedarfs kein Puffer für `AQMS.Web` und die Bestandsdienste
geblieben.

**Idee B — Wechsel zu PostgreSQL:** PostgreSQL läuft in ~200–300 MB und
hätte ohne Swap auf den alten VPS gepasst.

*Verworfen wegen:* Ein Provider-Wechsel hätte den Austausch von
`UseSqlServer` gegen `Npgsql` bedeutet und ein Neugenerieren sämtlicher
Migrations erfordert — das Schema enthält provider-spezifisches SQL
(`GETUTCDATE()` als DB-Default, siehe §21). Außerdem wäre eine Diskrepanz
zwischen Entwicklungsdatenbank (LocalDB / SQL Server auf Windows) und
Produktivdatenbank entstanden. Die früh getroffene
SQL-Server-Festlegung wäre damit aufgegeben worden.

**Stattdessen:** neuer Server mit 8 GB RAM (Option C). Da ein RAM-Upgrade
des alten VPS beim Anbieter nicht möglich und ein Server-Wechsel ohnehin
nötig war, entfiel der einzige Nachteil von C (Mehrkosten) weitgehend.
Der gesamte Tech-Stack bleibt unverändert: SQL Server, EF-Core-Migrations
und die Übereinstimmung von Entwicklungs- und Produktivdatenbank sind
erhalten. Details der Migration: §23A–§23F.

### 30.12 Generisches Repository-Pattern (verworfen)

**Idee:** Ein klassisches `IRepository<T>` mit konkreten Implementierungen
(`DeviceRepository`, `MeasurementRepository` usw.) als Datenzugriffs-
schicht zwischen Controller und `DbContext` — in einer frühen
Roadmap-Fassung ursprünglich so vorgesehen, dann verworfen (dieser
Punkt wurde aus den „Nächsten Schritten" §33 entfernt).

**Verworfen wegen:** EF Cores `DbContext` ist bereits ein Unit-of-Work,
und jedes `DbSet<T>` ist bereits ein Repository — beide
Pattern-Konzepte sind im Provider eingebaut. Ein eigenes `IRepository<T>`
darüber wäre eine zweite, gleichgeartete Abstraktionsebene über einer
bereits abstrahierten — in der C#-/EF-Core-Community als „Repository over
Repository" verbreitet kritisiert. Konkrete Nachteile bei
AQMS-Datenmengen (sechs Geräte, überschaubare Queries): zusätzliche
Klassen ohne fachlichen Mehrwert, eingeschränkter Zugriff auf EF-spezifische
Query-Features (`.Include`, `.AsNoTracking`), und doppelte Abstraktion,
die Lesbarkeit und Wartbarkeit verschlechtert.

**Stattdessen:** schlanker Service-Layer direkt auf `DbContext`. Die
Controller rufen Anwendungs-Services auf, die Services nutzen den
`DbContext` direkt. Drei Schichten statt vier, ohne Funktionalitätsverlust.

**Begründung als Diplomarbeit-Argument:** Die bewusste *Ablehnung* eines
gängigen Patterns mit Bezug auf die konkrete Projektgröße und die
Eigenschaften des verwendeten ORMs ist ein verteidigbarer Standpunkt —
„Pattern X anwenden, weil es im Lehrbuch steht" wäre die schwächere
Position.

---

### 30.13 `ICommandService`-Interface (verworfen)

Beim Bau des `CommandService` wurde bewusst **kein** `ICommandService`-Interface
angelegt. Ein Interface mit genau einem Implementierer ist dieselbe
Abstraktion-ohne-Mehrwert wie das in §30.12 verworfene generische
Repository-Pattern: Der Service kapselt bereits die Geschäftslogik über dem
DbContext, und seine Methoden lassen sich ohne Interface testen (EF-Core-
InMemory bzw. SQLite gegen die echte LINQ-Logik, was sogar aussagekräftiger ist
als ein gemockter Vertrag). Ein Interface wird erst dann ergänzt, wenn ein
zweiter Implementierer oder ein konkreter Mock-basierter Controller-Test real
gebraucht wird — beides ist aktuell nicht der Fall. Begründung konsistent mit
der Projektlinie „keine Abstraktion auf Vorrat".

### 30.14 Befehls-Poll-Varianten (verworfen zugunsten 1a+2a, §23N.8)

- **Per-Shelly-Polling.** Der Worker fragt jeden Shelly einzeln
  (`/pending?deviceIdentifier=shelly-X`). Vorteil: keine Backend-Änderung, Zielgerät
  implizit klar. Verworfen wegen 5 (fast immer leeren) Calls pro Intervall statt
  einem und fehlendem konsistenten Snapshot aller offenen Befehle. *(Das „5 Calls
  sind zu langsam"-Argument trägt für sich nicht — bei 5 s Intervall über eine
  gepoolte Verbindung vernachlässigbar; das Snapshot-Argument ist das tragende.)*
- **Explizite Controller-Beziehung (`ControllerId`-Self-FK am `Device`).** „Befehle
  für Geräte, die `raspberry-pi` steuert" über eine modellierte Beziehung statt über
  den Typ. Sauberer/allgemeiner, aber Modell + Migration für einen bei *einem* Pi und
  fest 5 Shellys trivialen Fall. Verworfen als Abstraktion auf Vorrat (vgl.
  §30.12/§30.13).
- **Separater Geräte-Registry-Endpunkt für die IP.** Der Worker holt
  einmal/periodisch `/api/devices` (identifier→IP) und cached; `/pending` taggt nur
  den Identifier. Sauberere Trennung Befehlsstrom/Geräteregister, aber mehr bewegliche
  Teile (Fetch + Cache + Invalidierung). Verworfen zugunsten der inline-IP (2a) — der
  Worker braucht die IP nur zum direkten LAN-Schalten und verteilt sie nicht weiter.

### 30.15 Deployment-Varianten Worker/Pi (verworfen zugunsten §23O)

- **Framework-Dependent (Runtime am Pi installieren).** Schlankeres Paket, aber verlangt
  `dotnet-runtime-10.0` auf dem Pi plus Versions-Pflege. Verworfen zugunsten self-contained:
  ein bewegliches Teil weniger, keine Runtime-Installation/-Drift auf einem Appliance-Gerät
  mit einem einzigen Dienst. Der Trade-off (größeres Paket, Neu-Deploy bei .NET-Updates) ist
  auf der SD-Karte irrelevant.
- **BaseUrl in der appsettings.json editieren (statt Production-Overlay).** Einfacher, aber
  der Dev-Lauf würde dann ebenfalls auf den VPS zeigen — genau der Dev/Prod-Swap, der beim
  lokalen Testen schon zu Schema/Port-Fehlern geführt hatte (§23N.8). Verworfen zugunsten
  `appsettings.Production.json` + `DOTNET_ENVIRONMENT` (§23O.2/.4): Dev bleibt auf localhost,
  Produktion auf VPS, ohne manuelles Umschalten.

---

### 30.16 Verworfene Alternativen: Dashboard-Scope (2026-07-12)

Bei knapper Restlaufzeit bis zur Abgabe (31.08.2026) wurde der Frontend-Scope bewusst
beschnitten. Verworfen wurden:

- **Permanentes AJAX-Polling im Browser** (Dauer-`setInterval` gegen einen JSON-Endpunkt).
  *Grund:* zusätzlicher Endpunkt, zusätzliche Auth-Frage, dauerhafte Last — für ein Ereignis, das
  pro Klick genau einmal eintritt.
  > **Revision 2026-07-12:** Der ursprüngliche Verzicht auf **jede** Form von Auto-Refresh wurde
  > **zurückgenommen** — er führte zu einer Oberfläche, die wie ein Defekt aussieht (§23P.8).
  > Umgesetzt ist nun ein **bedingter, selbstterminierender Reload**, der nur gerendert wird,
  > solange ein Befehl offen ist. Permanentes Polling bleibt verworfen.
- **SignalR / WebSockets fürs Dashboard.** *Grund:* würde eine Push-Verbindung *zwischen Server
  und Browser* etablieren — das ändert nichts daran, dass der Pi hinter NAT nur *pollen* kann
  (§30.14). Der Flaschenhals bliebe bestehen, die Architektur würde ohne Nutzen komplexer.
- **Zyklischer Health-Check der Shellys** (Worker pingt sie unabhängig vom Schalten an).
  *Grund:* echter Mehrwert, aber zusätzliche Last, zusätzlicher Code und zusätzliche
  Fehlerbehandlung für ein Feature, das kein Pflichtziel ist. Stattdessen zeigt das Dashboard
  ehrlich „zuletzt geschaltet" statt eines Online-Status, den es nicht belegen kann (§23P.6).
- **Polly-Retry im Worker.** *Grund:* das manuelle `try/catch` mit Interval-Guard ist
  dokumentiert, verifiziert und für ein System mit 30-Sekunden-Poll-Takt ausreichend — ein
  fehlgeschlagener Poll wird schlicht beim nächsten Durchlauf wiederholt. Der Bibliotheks-Aufwand
  zahlt sich hier nicht aus.

*Übergeordnetes Prinzip.* Jeder dieser Punkte ist im Manuskript als **begründete Abgrenzung** zu
führen, nicht als Lücke. Eine dokumentierte Scope-Entscheidung ist ein Qualitätsmerkmal; ein
unerwähntes Loch ist ein Mangel.

---

## 31. Glossar

| Begriff | Erklärung |
|---|---|
| **1-Wire** | Bus-Protokoll von Maxim/Dallas; vom DS18B20 verwendet |
| **AspNetUsers** | von Identity erzeugte Tabelle für Benutzer |
| **BCNF** | Boyce-Codd-Normalform — verschärfte 3NF |
| **Code-First** | EF-Core-Modus, in dem C#-Klassen die DB definieren |
| **Composite Index** | Index über mehrere Spalten in fester Reihenfolge |
| **CQRS** | Command Query Responsibility Segregation — getrennte Lese-/Schreib-Pfade |
| **DbContext** | Brücke zwischen C# und DB, Verwalter der Entities |
| **DbSet<T>** | Property im DbContext, repräsentiert eine Tabelle |
| **DDD** | Domain-Driven Design |
| **Denormalisierung** | bewusste Redundanz für Performance |
| **DHCP-Reservierung** | Router weist einer MAC immer dieselbe IP zu |
| **DI** | Dependency Injection — Klassen bekommen Abhängigkeiten injiziert |
| **DS18B20** | digitaler Temperatursensor von Maxim, 1-Wire |
| **DTO** | Data Transfer Object — schlanke Klasse für HTTP-Transport |
| **Entity** | C#-Klasse, die eine DB-Tabelle abbildet |
| **EF Core** | Entity Framework Core — Microsofts ORM |
| **Filtered Index** | Index nur über Zeilen mit bestimmtem WHERE-Kriterium |
| **Fluent API** | Methodenketten zur Konfiguration in OnModelCreating |
| **GETUTCDATE()** | SQL-Server-Funktion: aktueller UTC-Zeitstempel |
| **HasData** | EF-Core-Methode zum Definieren von Seed-Daten |
| **ICollection<T>** | Sammlungs-Interface, Standard für Navigation-Listen |
| **IDENTITY** | SQL-Server-Eigenschaft: Spalte wird automatisch hochgezählt |
| **Identity** | ASP.NET-Bibliothek für Auth, Login, Rollen, Hashing |
| **IdentityDbContext** | DbContext-Variante mit Identity-Tabellen |
| **IoT** | Internet of Things — vernetzte physische Geräte |
| **kebab-case** | Schreibweise mit Bindestrichen (`shelly-filter`) |
| **Kestrel** | eingebauter Webserver von ASP.NET Core |
| **Let's Encrypt** | kostenlose, automatisierte TLS-Zertifikate |
| **LINQ** | Language Integrated Query — C#-Syntax für Datenabfragen |
| **LocalDB** | leichtgewichtige Variante von SQL Server für Entwicklung |
| **Lookup-Tabelle** | kleine Tabelle mit Referenz-Werten (z.B. DeviceTypes) |
| **MARS** | Multiple Active Result Sets — SQL-Server-Feature |
| **Migration** | versionierte Schema-Änderung |
| **MVC** | Model-View-Controller — Architekturmuster |
| **NAT** | Network Address Translation — Router macht private IPs nach außen unsichtbar |
| **Natural Key** | fachlicher eindeutiger Schlüssel (z.B. DeviceIdentifier) |
| **Navigation Property** | C#-Property, die eine Beziehung ausdrückt |
| **NetworkManager** | modernes Netzwerk-Management auf Linux |
| **Nginx** | Webserver / Reverse Proxy auf Linux |
| **NRT** | Nullable Reference Types — C#-Feature für null-Sicherheit |
| **null!** | Null-Forgiving Operator — "Compiler vertrau mir" |
| **OnDelete** | Verhalten beim Löschen eines Parent-Datensatzes |
| **OnModelCreating** | DbContext-Methode für Schema-Konfiguration |
| **Polling** | regelmäßiges Abfragen einer Ressource |
| **Primary Constructor** | C# 12 Feature: Konstruktor in Klassendeklaration |
| **PSP** | Projektstrukturplan |
| **Razor** | Template-Engine von ASP.NET Core (`.cshtml`-Dateien) |
| **Restrict** | OnDelete-Verhalten: Parent-Delete blockieren |
| **Reverse Proxy** | vorgeschalteter Server, der Anfragen weiterleitet |
| **Seed-Daten** | Initialdaten, mit Migration in DB geschrieben |
| **SetNull** | OnDelete-Verhalten: FK auf null setzen |
| **Shelly** | Marke für IoT-Smart-Plugs / WLAN-Schalter |
| **SignalR** | ASP.NET-Bibliothek für Echtzeit-Kommunikation (WebSocket) |
| **SSH** | Secure Shell — verschlüsselte Remote-Verbindung |
| **Surrogate Key** | technischer eindeutiger Schlüssel (z.B. Id int IDENTITY) |
| **systemd** | Standard-Init-System auf Linux |
| **TLS** | Transport Layer Security — Verschlüsselung für HTTPS |
| **UNIQUE-Index** | Index, der gleichzeitig Eindeutigkeit erzwingt |
| **UTC** | Universal Time Coordinated — Welt-Standardzeit |
| **VPS** | Virtual Private Server |
| **WPA2-PSK** | WLAN-Verschlüsselung mit Pre-Shared Key |
| **xUnit** | C#-Unit-Testing-Framework |
| **yield return** | C#-Feature für Lazy Evaluation in IEnumerable |

---

# Teil G — Status und Roadmap

## 32. Aktueller technischer Stand

**Stand: 2026-08-06 · KW 32**

### 32.1 Was funktioniert

| Bereich | Status | Anmerkung |
|---|---|---|
| Projektplanung (PSP, Architektur) | ✓ | komplett dokumentiert |
| Hardware (Pi, DS18B20, 5 Shellys) | ✓ | im LAN erreichbar, getestet |
| Pi-Netzwerk (10.0.0.222 via NetworkManager) | ✓ | autostart, autoreconnect |
| Shelly-API-Tests | ✓ | klassisch + RPC validiert |
| Solution-Struktur (AQMS.Web/Worker/Tests) | ✓ | Punkt-Notation, .slnx |
| EF Core + Identity konfiguriert | ✓ | Program.cs sauber |
| Persistenzschicht-Modell (6 Entities, DbContext) | ✓ | 3NF/BCNF-konform |
| Migrations + Seeds auf LocalDB | ✓ | `AddDomainEntities` angewendet |
| Neuer VPS (Debian 13, x86-64, 8 GB RAM) | ✓ | `aqms.aqms.example.com` → 203.0.113.10 |
| .NET 10 Runtime auf neuem VPS | ✓ | ASP.NET Core Runtime 10.0.8 |
| Docker auf neuem VPS | ✓ | Docker CE aus offiziellem Repo |
| SQL Server 2022 (Docker-Container) | ✓ | Express, Port nur auf 127.0.0.1 |
| Schema + Seeds auf VPS-DB `aqms` | ✓ | per `Script-Migration` eingespielt, verifiziert (14 Tabellen, 6 Devices) |
| AQMS.Web auf VPS deployed | ✓ | `/var/www/aqms`, Framework-Dependent |
| systemd-Service `aqms-web` | ✓ | dedizierter User `aqms`, Connection-String per Override |
| Nginx Reverse Proxy `aqms.aqms.example.com` | ✓ | Erstinstallation auf neuem VPS |
| HTTPS via Let's Encrypt | ✓ | gültiges Zertifikat, HTTP→HTTPS-Redirect |
| DB-Verbindung App→SQL Server | ✓ | über Login-Seite verifiziert |
| GitHub-Repo | ✓ | mit `.gitignore`, Initial-Commits |
| CommandsController (GET pending / POST result) + Service-Layer CommandService | ✓ | lokal + VPS verifiziert (2026-06-04), §23N |
| Worker-Gerüst: Config + named HttpClient `aqms-api` registriert | ✓ | baut + läuft (2026-06-07), §11.6 |
| Worker: Polling-Loop `GET /api/commands/pending` (mit Resilienz) | ✓ | try/catch + Interval-Guard, lokal + VPS verifiziert (2026-06-28), §11.6/§32.3 |
| Worker: Befehls-Empfang v2 (Deserialisierung, Device+IP inline) | ✓ | lokal + VPS verifiziert (Empfang 2026-06-28, VPS-Redeploy 2026-07-02), §23N.8 |
| Worker: Dispatch (Shelly-Steuerung, klassische API) | ✓ | end-to-end gegen echte Hardware verifiziert (2026-07-02), §11.8 |
| Worker auf Pi deployed (self-contained arm64, systemd) | ✓ | `/opt/aqms-worker`, `aqms-worker.service`, vom Pi verifiziert (2026-07-02), §23O |
| Worker: Result-Reporting (`POST /result`, Skip-Fälle als Failed) | ✓ | lokal + Pi verifiziert (2026-07-02), §11.9 |
| Worker: Sensor-Pfad DS18B20 → `POST /measurements` | ✓ | am Pi gegen VPS verifiziert (2026-07-07), §11.10 |
| Worker: Sensor-Health-Eskalation (Zähler + Error bei Dauerausfall) | ✓ | §11.10 |
| Befehls-Erstellung (`CommandService.CreateCommandAsync`) | ✓ | Idempotenz-Riegel + serverseitige UserId, VPS-verifiziert (2026-07-12), §23P.3 |
| Rollenbasierte Autorisierung (`[Authorize]`, `Roles = "Admin"`) | ✓ | Dashboard nur eingeloggt, Schalten nur Admin, §23P.4 |
| Web-Dashboard (Razor View, Status-Badges, Toggle-Buttons) | ✓ | VPS-verifiziert (2026-07-12), §23P |
| Chart.js-Temperaturverlauf (letzte 50 Messwerte) | ✓ | Zeitzone serverseitig aufgelöst, §23P.5 |
| **Vollständige Regelkette Browser → VPS → Pi → Shelly → DB → Dashboard** | ✓ | **gegen reale Hardware verifiziert und gefilmt (2026-07-12), §23P.7** |
| Unit-Tests (xUnit, 16 Tests) | ✓ | `ParseTemperature` + `CommandService`, alle grün (2026-07-12), §23Q |
| Bedingter Auto-Reload im Dashboard (selbstterminierend) | 🟡 | implementiert, **VPS-Verifikation offen** (2026-07-12), §23P.8 |
| Messintervall 20 s (bewusste Entscheidung: Testbarkeit vor Datenökonomie) | ✓ | am Pi verifiziert, §23R.1 |
| **24-h-Dauerlauf: 4.136 Zyklen, 0 Fehler, 0 Lücken** | ✓ | **bestanden (2026-07-13), §23R.3** |
| IdentitySeeder: zweites Konto in Rolle `User`, Admin als Pflichtkonto | ✓ | `EnsureUserAsync`, Startabbruch bei fehlender/ungültiger Admin-Config, §23K.4 |
| Migrationen auf leerer DB (eigener Container, Port 1434) | ✓ | 14 Tabellen, Startdaten vollständig, bestanden (2026-08-06), §23S.3 |
| Rollentrennung serverseitig (Aufruf unter Umgehung der Oberfläche) | ✓ | Zugriff verweigert, kein Befehl angelegt, Gegenprobe Admin ok, §23S.4 |
| SQL-Injection: struktureller + empirischer Nachweis | ✓ | kein Roh-SQL im Projekt, Nutzlast als Parameter, §23S.5 |
| Fehlerdarstellung in Produktion (kein Stack-Trace) | ✓ | an echter Ausnahme geprüft, 500 ohne interne Details, §23S.5 |
| **SSL Labs: Gesamtnote A, TLS 1.3** | ✓ | **bestanden (2026-08-06), §23S.6** |
| **Schaltdurchlauf über alle 5 Geräte, 9 Befehle, 0 Fehler** | ✓ | **bestanden (2026-08-06), §23S.7**, Schaltlatenz 5–9 s (Mittel 6,9 s) |
| **Phase 5 (Testing) vollständig abgeschlossen** | ✓ | alle 9 Feinziele aus GZ 5.1/5.2 durchgeführt und protokolliert, §23S |

### 32.2 Was noch fehlt

| Bereich | Status | Phase |
|---|---|---|
| API-Controller `/api/measurements` | ✓ | §23M, POST + GET lokal (2026-05-31) und auf VPS (2026-06-01) verifiziert |
| Repository-Layer | – | bewusst verworfen zugunsten schlankem Service-Layer (siehe §30) |
| Identity-Seeder (Admin-User + Rollen) | ✓ | §23K.3, lokal und auf VPS verifiziert |
| API-Key-Middleware | ✓ | §23L, in allen drei Umgebungen aktiv, lokal (2026-05-29) und auf VPS (2026-06-01) verifiziert |
| Lokales Setup auf zwei Geräten (User Secrets) | ✓ | Firmen-Gerät (Docker) + Privat-Gerät (LocalDB) verifiziert, §11.7 |
| Razor Views: Login (UI-Anpassungen) | ⌛ | Phase 4 (Issue #38) |
| **Latenter Defekt: `PiOnline`-Schwellwert implizit an Messintervall gekoppelt** | 🟡 | bei 20 s Takt korrekt, bricht lautlos bei jeder Erhöhung; §23R.2 |
| Worker-Tests (HTTP-Client mit Fake-Handler) | ⌛ | Phase 5 |
| Diplomarbeits-Manuskript | 🟡 | Phase 6 (Vorlage existiert) — **kritischer Pfad, Abgabe 31.08.2026** |

### 32.3 Verlaufs-Updates (chronologisch)

> Dieser Abschnitt ist eine **chronologische Update-Historie** (Tagebuch-
> Charakter). Die Einträge bauen aufeinander auf; spätere Updates können
> frühere Aussagen überholen. Für den aktuellen Gesamtstand siehe §32.1/§32.2
> oben bzw. die Projektstand-Doku.

Der ursprüngliche VPS (Debian 12, 1,9 GB RAM) wurde durch einen neuen
Server ersetzt (Debian 13, x86-64, 8 GB RAM, IP 203.0.113.10). Grund war
der RAM-Bedarf von SQL Server für Linux, der auf dem alten VPS nicht
gedeckt werden konnte — siehe §30. Auf dem neuen Server sind .NET-10-
Runtime, Docker und der SQL-Server-Container eingerichtet, das DB-Schema
inkl. Seeds ist eingespielt und verifiziert.

> *(Historischer Stand bei Anlage dieses Abschnitts, inzwischen überholt —
> siehe Update 19.05.2026 direkt darunter:)* Noch ausstehend auf dem neuen
> Server: Deployment der `AQMS.Web`, systemd-Service, Nginx Reverse Proxy
> und HTTPS.

**Update 19.05.2026:** Diese Schritte sind abgeschlossen (§23G–§23J).
`https://aqms.aqms.example.com` ist öffentlich und verschlüsselt erreichbar
und zeigt die echte AQMS.Web. Der Backend-Stack auf dem neuen VPS ist
vollständig: Debian 13 → Docker / SQL Server → AQMS.Web (systemd) →
Nginx → HTTPS. Die Registrierung eines Identity-Users wurde real
durchgespielt und verifiziert (User in `AspNetUsers` persistiert) — dabei
trat das in §23H.6 dokumentierte SQL-Login-Problem auf und wurde gelöst.
Damit sind alle Sofortmaßnahmen aus §33.1 abgeschlossen.

**Update 20.05.2026:** Phase 3 angeschnitten. Der IdentitySeeder (§23K.3)
ist als erste Architekturkomponente der Phase 3 implementiert und sowohl
lokal als auch auf dem VPS verifiziert — Rollen `Admin` und `User` sowie
der Admin-Benutzer werden idempotent beim App-Start angelegt. Damit ist
das in §23K.1 beschriebene manuelle DB-Update-Provisorium obsolet. In
derselben Iteration wurde die Architekturentscheidung gegen ein
generisches Repository-Pattern getroffen und in §30.12 begründet
dokumentiert; an dessen Stelle tritt ein schlanker Service-Layer
(noch ausstehend).

**Update 2026-05-22:** Konfigurations-Setup auf beiden
Entwicklungsgeräten konsolidiert und §11-konform. Anlass war ein lokal
versehentlich in `appsettings.json` eingetragener Connection-String mit
Passwort — niemals gepusht, aber doku-widersprüchlich. Aufräumarbeit:
`ConnectionStrings`-Block aus `appsettings.json` entfernt,
`appsettings.Development.json` gelöscht, auf Firmen-Gerät (Docker) und
Privat-Gerät (LocalDB) jeweils die drei nötigen User Secrets gesetzt
(`ConnectionStrings:DefaultConnection`, `AdminBenutzer:Email`,
`AdminBenutzer:Passwort`). Beide Geräte verifiziert: App startet,
Migrations laufen via `Update-Database`, IdentitySeeder läuft beim ersten
App-Start, Login erfolgreich. Lehre: `Update-Database` und Laufzeit-Seeder
sind getrennte Schritte — bei einer Neuinstallation ist nach
`Update-Database` zwingend ein App-Start nötig, sonst fehlen Rollen und
Admin-User. Workflow und Begründung in §11.7 festgehalten.

**Update 2026-05-29:** Phase 3 Punkt 2 erledigt — `ApiKeyMiddleware`
(§23L) selbst geschrieben, in `Program.cs` registriert und lokal mit drei
Test-Szenarien verifiziert (kein Header → 401, falscher Header → 401,
korrekter Header → 200). Architekturentscheidungen dokumentiert:
ein Schlüssel statt mehrerer, Header `X-API-Key` statt Query-String,
konstantzeitiger Vergleich gegen Timing-Attacks, Pipeline-Position vorn
direkt nach `UseHttpsRedirection`. Nebenbefund: `app.UseAuthentication()`
fehlte bislang in der Pipeline und wurde nachgezogen — Identity-Login
funktionierte zwar weiter durch die Identity-UI-Endpunkte, spätere
`[Authorize]`-Attribute hätten aber nicht gegriffen. `ApiKey` als
User-Secret auf Firmen- und Privat-Gerät gesetzt (pro Umgebung anderer
Wert); auf dem VPS noch zu setzen, wird mit dem Worker-Setup (Phase 2)
synchronisiert.

**Update 2026-05-31:** Phase 3 Punkt 3 erledigt — `MeasurementsController`
(§23M) implementiert. Bewusst kein Voll-CRUD, sondern nur POST (Schreibweg
für den Worker) und GET (Leseweg für die spätere Dashboard-UI) — Messwerte
sind unveränderliche historische Fakten, daher kein Update und kein Delete
(append-only-Modell). Zwei DTO-Klassen eingeführt: `CreateMeasurementDto`
für die Eingabe, `MeasurementResponseDto` für die Antwort. Dritter DI-Stil
im Projekt verwendet: Konstruktor-Injektion für den DbContext.
**Praxisproblem während der Entwicklung:** Beim ersten GET-Aufruf brach
die Serialisierung mit `JsonException` ab — direkte Rückgabe der
`Measurement`-Entity erzeugte einen Endlos-Zyklus über die
Navigation-Property `Device → Measurements → Device → …`. Lösung: das
Response-DTO mit `.Select(...)`-Projektion in der LINQ-Kette. Damit hat
sich die DTO-Theorie aus §23M.2 praktisch bestätigt — verteidigungsfähig
als „selbst durchlebt, nicht zitiert". Lokal mit POST und GET verifiziert
(2026-05-31, Firmen-Gerät).

**Privat-Gerät an gleichem Tag durchgezogen:** ApiKey-User-Secret mit
eigenem Zufallswert gesetzt (anders als Firmen-Gerät, gemäß §11.7-Strategie).
`AdminBenutzer:Email` und `AdminBenutzer:Passwort` waren noch nicht gesetzt —
nachgezogen, weil ohne sie der IdentitySeeder beim App-Start sofort in seine
Guard Clause aussteigt (Rollen werden angelegt, aber kein Admin-User).
DB-Schema mit `Update-Database` aufgebaut; **Lehrmoment**: nach erfolgreichem
`Update-Database` war die DB zwar mit Tabellen und HasData-Seeds gefüllt,
aber `AspNetUsers`/`AspNetRoles` waren leer — `Update-Database` führt nur
Migrations + HasData aus, der IdentitySeeder läuft erst beim App-Start
(§23K.3). Nach F5 hat der Seeder beim ersten Start sauber durchgelaufen,
Login auf Privat-Gerät verifiziert. Die Sequenz „User Secrets → Update-Database
→ App-Start → Login" ist damit zum zweiten Mal real durchgespielt — der
verbindliche Workflow aus §11.7 hält.

**Reste / offene Punkte aus dem Tag:**
- Timestamp-Zeitzonen-Marker im JSON-Output verloren (TZ-Info nicht in der
  Antwort, technisch UTC aber nicht markiert — Polishing-Phase).
- Fachliche Wertvalidierung erst im Service-Layer (§33.2, Phase 3).
- VPS-Verifikation steht aus (mit Worker-Setup).
- Middleware-Verifikation auf Privat-Gerät war **nicht** Teil des heutigen
  Tests — dort wurde nur Login durchgespielt; die drei API-Key-Szenarien
  aus §23L.6 wären beim nächsten Privat-Gerät-Lauf nachzuholen (siehe
  §23L.7).

**Update 2026-06-01:** VPS-Verifikation für API-Key-Middleware und
MeasurementsController vorgezogen und durchgezogen. Begründung für die
Vorziehung: ohne praktische Bestätigung, dass der Backend-Stack unter
Production-Bedingungen funktioniert, wäre der Weiterbau (CommandsController,
Service-Layer) ein Bauen auf Verdacht — *„wenn die ersten beiden Komponenten
auf dem VPS nicht laufen, gibt es keinen Grund, weitere darauf zu bauen"*.
**Deploy-Verfahren:** lokales `dotnet publish` (Release), per Filezilla
nach `/home/deployuser/deploy-tmp/`, Zwei-Schritt-Deploy nach `/var/www/aqms/`
mit `chown aqms:aqms` (siehe §23H.7), Service-Restart. **API-Key auf VPS:**
neu erzeugt mit `openssl rand -base64 32`, in systemd-Override
(§23L.5) hinterlegt, gilt zukünftig auch für den Pi-Worker (Phase 2).
**Praxisproblem auf dem Weg:** Erste Test-Iteration scheiterte mit 401
auch beim Positiv-Test — Ursache war ein syntaktischer Fehler in der
systemd-Override (`Environment="<wert>"` statt `Environment="ApiKey=<wert>"`,
ohne erkennbare Fehlermeldung), siehe §23L.8 für vollständige Symptom/
Ursache/Lösung/Lessons. Nach Korrektur alle drei Test-Szenarien gegen
`https://aqms.aqms.example.com` bestanden — sechs-Komponenten-Stack (DNS →
Nginx → HTTPS → Kestrel → Middleware → Controller → DB) zum ersten Mal
End-to-End in Production bewiesen. Damit ist der API-Key-Stack in allen
drei Umgebungen verifiziert (Firmen-Gerät, VPS; Privat-Gerät weiterhin
nur Login geprüft, Middleware-Verifikation dort weiterhin offen — siehe
§23L.7).

**Update 2026-06-04:** CommandsController und erster Service-Layer
(`CommandService`) gebaut und **lokal + VPS** verifiziert (Build grün; vier
HTTP-Szenarien 200/400/404/409 plus Happy-Path mit StateChange-Insert und
CurrentState-Update; VPS-Test gegen `https://aqms.aqms.example.com`). Erste
Update-Logik im Backend (Statusübergang `Pending → Executed/Failed`) und
erste Geschäftslogik-Schicht. Damit ist der Backend-Schreibweg für Befehle
end-to-end produktiv. Details und Lessons Learned in §23N (inkl. der
PS-5.1-Eigenheit zu `-SkipHttpErrorCheck`).

**Update 2026-06-07:** Phase 2 begonnen. Worker-**Gerüst** gebaut und
verifiziert (baut + läuft): named `HttpClient` `"aqms-api"` in
[`AQMS.Worker/Program.cs`](../AQMS.Worker/Program.cs) registriert
(BaseAddress + `X-API-Key`-Header aus Config), Worker-Konfigurationsschlüssel
angelegt (`AqmsApi:BaseUrl`, `Worker:DeviceIdentifier`,
`Worker:PollIntervalSeconds` in appsettings; `ApiKey` flach in User Secrets),
Paket `Microsoft.Extensions.Http` ergänzt. Begründungen: HttpClient-Factory
gegen Socket-Erschöpfung (§10.2), flacher `ApiKey` gegen die
systemd-Doppelunterstrich-Falle (§11.6 mit Verweis auf §23L.8).
Praxisproblem `AddHttpClient` nicht gefunden → fehlendes Paket im Worker-SDK
(§10.5, Symptom/Ursache/Lösung/Lessons). Noch offen in Phase 2: der
eigentliche Polling-Loop in `Worker.cs` (GET `/api/commands/pending`),
DS18B20-Auslesung, Shelly-Steuerung, Result-Reporting, systemd-Unit.

**Update 2026-06-28 (Polling-Loop):** Polling-Loop in `Worker.cs` implementiert —
erste produktive Worker-Logik. Die `while`-Schleife pollt über den named
`HttpClient` `"aqms-api"` den Endpunkt `GET /api/commands/pending`. Zwei
Resilienz-Entscheidungen (Details §11.6): try/catch nur um den Request (Schleife
läuft bei `HttpRequestException` weiter, statt über
`BackgroundServiceExceptionBehavior.StopHost` den Host zu beenden; gezielt nur
`HttpRequestException`, `Task.Delay` außerhalb des try) + Interval-Guard gegen
`Task.Delay(0)`-Tight-Loop. **Verifikation (lokal + VPS):** lokal pollt der Worker
bei gestoppter API weiter, der Host bleibt am Leben; VPS-Poll gegen
`aqms.aqms.example.com` liefert wiederholt `Status: OK` (volle Kette produktiv).
Offene Resilienz-Punkte bewusst verschoben (Timeout-Cancellation, Polly-Backoff,
dreifaches Logging).

**Update 2026-06-28 (Befehls-Empfang v2):** `/pending` auf geräte-übergreifenden
Poll umgebaut — parameterlos, Scoping über `DeviceType.Name == "SmartPlug"` plus
`IsEnabled`/IP-Guard, je Befehl `DeviceIdentifier` + `IPAddress` inline (§23N.8,
verworfene Alternativen §30.14). **API-Vertragsänderung:** kein `deviceIdentifier`,
kein 400-„unbekanntes Gerät" mehr. DTO Web+Worker je +2 Properties; Worker
deserialisiert + loggt empfangene Befehle. Lokal end-to-end verifiziert; VPS läuft
noch v1, Redeploy offen. Zwei Praxisprobleme beim Testen (verwaiste Instanz hält
Port → neuer Build bindet nicht; http/https-Port-Mismatch in beide Richtungen).

**Update 2026-07-02 (Dispatch + Pi-Deployment):** Dispatch implementiert (§11.8) — `switch`
auf `Action` → klassische Shelly-API `GET /relay/0?turn=on|off` (§7.4) über einen separaten
Default-Client (nicht `aqms-api`, kein Key-Leak), zweischichtige Fehlerbehandlung (breiter
Loop-Filter + per-Befehl-Catch) und 3s-Timeout mit Timeout-vs-Shutdown-Unterscheidung. Worker
self-contained (linux-arm64) auf den Pi deployt (§23O): `/opt/aqms-worker`,
`aqms-worker.service`, `DOTNET_ENVIRONMENT=Production`, `ApiKey`-Override. **Verifikation
(echte Hardware):** vom Pi aus Poll gegen VPS → `Befehl empfangen: 2 On` (shelly-filter + IP)
→ `GET http://10.0.0.227/relay/0?turn=on` → 200 → `Befehl 2 ausgeführt`. Der reale Shelly
schaltete; die ganze Kette (VPS v2 → Pi → Hardware) ist bewiesen. Bewusst deployt trotz
funktionaler Unvollständigkeit (Result-Reporting + Sensor fehlen), um das Architektur-Risiko
früh auszuschließen. Drei Deployment-Praxisprobleme dokumentiert (§23O.7). Damit ist auch der
VPS-Redeploy des v2-Vertrags (offener Punkt aus 2.3) erledigt.

---

**Update 2026-07-12 (Befehls-Erstellung, Autorisierung, Dashboard, Tests — Phase 3 + 4
abgeschlossen):** Der Regelkreis schließt sich. `CommandService.CreateCommandAsync` (§23P.3)
erzeugt `Pending`-Befehle — mit Idempotenz-Riegel gegen Relais-Flattern und serverseitig
gesetzter `RequestedByUserId`. Der Toggle läuft bewusst über eine **MVC-Route**
(`POST /Dashboard/Toggle`, Cookie-Auth + `[Authorize(Roles = "Admin")]` + Antiforgery), **nicht**
über `/api` — sonst müsste der API-Key in die Browser-Seite eingebettet werden und die
Rollenprüfung wäre umgehbar (§23P.2). Dashboard mit Temperatur-Kachel, Chart.js-Verlauf und
Status-Badges (§23P.5). Praxisproblem gefunden und behoben: `Device.LastSeen` wurde nur beim
Schalten gesetzt, nie beim Messwert-Empfang → der Pi wäre dauerhaft „offline" gewesen (§23P.6).
16 xUnit-Tests, alle grün (§23Q). §23O.8 entschieden: BaseUrl bleibt in der Overlay-Datei, weil
beide Deployment-Ausfälle **eine** inzwischen an der Quelle behobene Ursache hatten.
**Verifikation (Produktion, reale Hardware):** Login → Dashboard → Klick → Pending in DB →
Pi-Poll → realer Shelly geschaltet → Result → Status `Executed` → Badge `EIN`. Gefilmt.

**Update 2026-07-12/13 (Auto-Reload-Revision, Messintervall, 24-h-Dauerlauf bestanden):** §23P.8
**revidiert**: Der bewusste Verzicht auf jeden Auto-Refresh wurde zurückgenommen, nachdem der
Entwickler selbst minutenlang auf die scheinbar tote Oberfläche starrte — eine Design-Entscheidung,
an der ihr eigener Autor scheitert, ist widerlegt. Umgesetzt: **bedingter, selbstterminierender
Reload** (Skript wird nur gerendert, solange `HasPendingCommand`; endet von allein, sobald der
Worker gemeldet hat). Permanentes AJAX-Polling und SignalR bleiben verworfen (§30.16). §23R neu:
**Messintervall-Entscheidung** (§23R.1) — versuchsweise auf 3600 s gesetzt (fachlich richtig:
Wasser ist träge), aber **bewusst auf 20 s zurückgenommen**, weil der Dauerlauf ein
Zuverlässigkeitstest *pro Zyklus* ist: 3600 s hätten 24 Zyklen ergeben, 20 s ergaben 4.136 — ein
170-fach härterer Test bei identischem Aufwand. **§23R.3: 24-h-Dauerlauf bestanden** — 4.136
Messzyklen, **0 Fehler, 0 Warnungen, 0 Lücken**, Service nach 24 h stabil; Temperaturkurve zeigt
einen glatten, physikalisch plausiblen Tag-Nacht-Verlauf (26,25–28,06 °C). §23R.2: **latenter
Defekt** dokumentiert — der hartkodierte 5-Minuten-`PiOnline`-Schwellwert ist implizit an
`MeasurementIntervalSeconds` gekoppelt; bei 20 s korrekt, bricht **lautlos** bei jeder Erhöhung
(kein Compiler-Fehler, kein roter Test, nur eine Oberfläche, die lügt).

**Update 2026-08-06 (Issue-Board mit dem Doku-Stand abgeglichen):** Das GitHub-Board
war seit Phase 1 nicht mehr nachgezogen worden und zeigte 56 offene Aufgaben, obwohl
die Phasen 2 bis 5 laut §32.1 und §33 abgeschlossen sind. Jedes Feinziel wurde gegen
die Master-Doku geprüft und, wo belegt, mit Verweis auf die belegende Sektion
geschlossen: 42 Aufgaben als erledigt, eine als verworfen (PSP 3.3.2
Repository-Pattern, §30.12). Offen bleiben bewusst vier Sacharbeiten — Polly-Retry
(§33.1 Punkt 1, zurückgestellt), Worker-seitige Tests mit Fake-Handler (§33.1
Punkt 5), die optische Gestaltung der Login-Seite (§32.2) — sowie die zehn
Aufgaben der Phase 6. Der Abgleich hat zwei Status-Drifts in §32.2 aufgedeckt:
„24h Lauftest" und „Sicherheitstests (SSL Labs etc.)" standen dort noch als offen,
obwohl §32.1 sie seit dem 13.07. bzw. 06.08. als bestanden führt — beide Zeilen
entfernt. Methodische Lesson: ein Board, das nicht mitgepflegt wird, wird zur
zweiten, widersprechenden Statusquelle neben der Doku; der Abgleich gehört an
jeden Phasenabschluss.

---

## 33. Nächste Schritte

> Diese Liste enthält ausschließlich **noch offene** Arbeiten. Erledigte
> Punkte werden hier entfernt (nicht abgehakt stehengelassen), um keine
> konkurrierende Status-Quelle zu §32 zu erzeugen. Was bereits umgesetzt
> ist, steht in §32.1 und in der Projektstand-Doku.

### 33.1 Phase 2 (Worker auf Pi)

Erledigt (siehe §32.1, §11.6, §11.8, §11.9, §11.10, §23N.8, §23O): Worker-Gerüst,
**Polling-Loop** inkl. Resilienz, **Befehls-Empfang v2** (Device+IP inline), der **Dispatch**
(Shelly-Steuerung, hardware-verifiziert 2026-07-02), das **Result-Reporting** (§11.9), das
**Pi-Deployment** (self-contained arm64, §23O) und der **Sensor-Pfad** (DS18B20 →
`/api/measurements`, am Pi verifiziert 2026-07-07, §11.10). **Phase 2 ist damit funktional
abgeschlossen** — der Pi macht Steuerung *und* Monitoring. Offen bleiben nur noch Politur- und
Test-Punkte (nicht funktional blockierend):

1. Retry-Logik mit Polly (ergänzt das manuelle try/catch) — **bewusst zurückgestellt**, das
   manuelle try/catch ist dokumentiert und ausreichend (§11.6); kein Blocker für die Abgabe
2. Dreifaches Fehler-Logging runterdrehen (`System.Net.Http.HttpClient` auf `Warning`)
3. Config-Key-Tippfehler `MaxContinousSensorErrors` → `MaxContinuous…` korrigieren (Code + appsettings)
4. Sensor-Health Option B (Ausfall in DB/Dashboard sichtbar) — Ausblick (§11.10)
5. Worker-seitige Tests (HTTP-Client mit Fake-Handler) — die Sensor-Logik ist getestet (§23Q.2),
   der Worker-Loop selbst noch nicht
6. Messintervall: **entschieden** — 20 s bleiben für die Projektlaufzeit (Testbarkeit vor
   Datenökonomie, §23R.1). Eine Erhöhung auf Minutenbereich ist als *Ausblick* für den realen
   Dauerbetrieb zu benennen; sie **erfordert zwingend** die Anpassung des `PiOnline`-Schwellwerts
   (§23R.2)

**Erledigt seit 2026-07-12:** xUnit-Tests für `ParseTemperature` (§23Q.2) und die Entscheidung
zur Produktions-BaseUrl (§23O.8 — die Overlay-Datei bleibt).

### 33.2 Phase 3 (Backend) — abgeschlossen

**Phase 3 ist mit dem 2026-07-12 vollständig abgeschlossen.** Die beiden letzten offenen Punkte
— rollenbasierte Autorisierung und Befehls-Erstellung — sind umgesetzt und auf dem VPS gegen
reale Hardware verifiziert (§23P).

Umfang (siehe §32.1): API-Key-Middleware (§23L), MeasurementsController inkl. DTOs (§23M),
IdentitySeeder (§23K.3), CommandsController + Service-Layer `CommandService` (§23N),
Befehls-Erstellung `CreateCommandAsync` (§23P.3), Autorisierung per `[Authorize]` /
`Roles = "Admin"` (§23P.4).

### 33.3 Phase 4 (Frontend) — funktional abgeschlossen

**Umgesetzt und VPS-verifiziert (2026-07-12, §23P):** Dashboard-View mit Temperatur-Kachel,
Chart.js-Verlaufsdiagramm (letzte 50 Messwerte), Geräteliste mit Status-Badges und
Toggle-Buttons, Navigation im `_Layout.cshtml`.

**Bewusst nicht umgesetzt** (Scope-Entscheidung, §30.16 — im Manuskript als Abgrenzung zu
begründen, nicht als Lücke):

1. Auto-Refresh / AJAX-Polling im Browser — die Poll-Latenz wird sichtbar gemacht statt kaschiert
   (§23P.8)
2. Zeitraum-Filter (24 h / 7 d) für das Diagramm — fixe Fenstergröße von 50 Messwerten
3. Multi-Sensor-Ansicht — es existiert genau ein Sensor
4. Benutzerverwaltungs-UI — Benutzer werden per `IdentitySeeder` administrativ angelegt (§23K)

**Offene Punkte in Phase 4:**

1. **Latenter Defekt: `PiOnline`-Schwellwert** — bei 20 s Messintervall korrekt, bricht aber
   lautlos, sobald das Intervall erhöht wird (§23R.2). Fix: benannte Konstante mit explizitem
   Kopplungs-Kommentar. *Nicht dringend, aber vor jeder Intervall-Änderung zwingend.*
2. **VPS-Verifikation des bedingten Auto-Reloads** (§23P.8) steht aus.
3. Randfall Auto-Reload: bei dauerhaft offline-Pi lädt die Seite endlos alle 3 s neu — mögliche
   Absicherung: Reload nur, wenn der offene Befehl jünger als 5 Minuten ist (§23P.8).

### 33.4 Phase 5 (Tests) — abgeschlossen

**Phase 5 ist mit dem 2026-08-06 vollständig abgeschlossen.** Alle neun Feinziele
aus GZ 5.1 und GZ 5.2 sind durchgeführt und protokolliert (§23S). Die Protokolle
liegen in `AQMS_Testprotokolle.md` und speisen Anhang D des Manuskripts.

Umfang: Testplan und Protokoll-Vorlage (§23S.1), 24-h-Dauerlauf (§23R.3),
Schaltdurchlauf über alle fünf Geräte (§23S.7), Prüfung der Programmier-
schnittstelle (§23M.5, §23N.5, ergänzt §23S.8), Migrationen auf leerer Datenbank
(§23S.3), SSL Labs (§23S.6), Zugriff ohne Login (§23P.4), API-Key-Schutz
(§23L.7), Einschleusungsversuch (§23S.5).

**Nicht Teil der Phase, weiterhin offen:** Worker-seitige Tests mit einem
austauschbaren `HttpMessageHandler` (§33.1 Punkt 5) — die Fehlerbehandlung des
Loops ist durch keinen Test abgedeckt, weil sich ein nicht erreichbarer Server
oder ein Timeout nur künstlich herbeiführen lässt.

**Neu aufgenommen aus §23S.5:** Fehlerantworten unter `/api/...` werden als
`text/html` ausgeliefert statt als JSON. Der Worker übersteht das (§11.6), es
steht aber quer zum Statuscode-Vertrag aus §23M.2. Politur, kein Blocker.

### 33.5 Phase 6 (Doku & Abgabe)

1. Diplomarbeit-Manuskript aus Master-Doku und Projektstand-Doku ableiten
2. Diagramme finalisieren (ERD, Architektur, Klassen)
3. Testprotokolle einarbeiten
4. Korrekturlesen, Präsentation, Abgabe 31.08.2026

---

## 34. Doku-Versionierung und veraltete Dateien

### 34.1 Rollenteilung: Tagebuch und Projektstand

Seit dem 01.06.2026 gilt eine bewusste Aufteilung in zwei Dokumente:

- **`AQMS_Masterdoku.md`** (diese Datei) — das **chronologische Tagebuch**.
  Lückenloser Verlauf, alle Designentscheidungen mit Begründung, verworfene
  Alternativen, Praxisprobleme mit Symptom/Ursache/Lösung/Lessons,
  einschließlich historischer (überholter) Stände. Autoritativ für „wie kam
  es dazu" und als Quellmaterial für den Methodik-/Verlaufsteil der
  Diplomarbeit.
- **`AQMS_Projektstand_<datum>.md`** — die bereinigte **Ist-Stand-Doku**.
  Beschreibt komponentenorientiert, was *aktuell* gebaut und konfiguriert
  ist, ohne historische Drift. **Single Source of Truth für den
  Ist-Stand.** Bei größeren Änderungen wird eine neue datierte Fassung
  erstellt.

Davor (ab 09.05.2026) war diese Master-Doku alleinige zentrale
Dokumentation; die Aufteilung wurde eingeführt, weil Tagebuch- und
Ist-Stand-Funktion in einer Datei zu konkurrierenden Status-Angaben
geführt hatten.

### 34.2 Veraltete Einzeldokumente

Die folgenden Dateien wurden in diese Master-Doku konsolidiert. Sie sind
als **VERALTET** markiert (Hinweis am Anfang jeder Datei) und dienen nur
noch als historische Versionsstände. **Inhaltlich nicht mehr referenzieren.**

**docs/Projektmanagement/**
- `AQMS_PSP.md` — Projektstrukturplan (Inhalt → §2 Master)

**docs/VS_Setup/**
- `AQMS_TechStack.md` — Tech-Stack-Erklärung (Inhalt → §4 Master)
- `AQMS_Vollstaendiges_VS_Setup_Dokumentation.md` — VS-Setup-Doku (Inhalt → §8–§12 Master)
- `AQMS_Coding_Guide.md` — Schritt-für-Schritt-Coding-Anleitung *(behalten als Lernmaterial für Phase 2-4, nicht ersetzt)*

**docs/VPS_Setup/**
- `linux_aspNet_setup.md` — VPS-Einrichtung (Inhalt → §23–§28 Master)

**docs/Hardware/**
- `Raspberry_Pi_WLAN_Setup_NetworkManager.md` — generisch (Inhalt → §6 Master)
- `Raspberry_Pi_WLAN_Setup_HomeWLAN_10.0.0.222.md` — projektspezifisch (Inhalt → §6 Master)
- `Raspberry_Pi_WLAN_Setup_HomeWLAN_10.0.0.222_Diploma (1).docx` — Word-Variante (Inhalt → §6 Master, deckungsgleich mit der `.md`-Variante)
- `AQMS_Shelly_Dokumentation_Diplomarbeit.md` — Shelly Diplomarbeits-Niveau (Inhalt → §7 Master)
- `AQMS_Shelly_Dokumentation_mit_API_Befehlen.md` — Shelly mit API (Inhalt → §7 Master)
- `Aquarium_Projekt_Dokumentation_Bis_DS18B20.docx` — DS18B20-Inbetriebnahme inkl. Pull-Up-Fehler und Lösung (Inhalt → §5.2–§5.7 Master)
- `LK-TEMP2_ANLEITUNG_2021-12-02.pdf` — Joy-IT-Datenblatt für das LK-Temp2-Modul, Referenzmaterial für die DS18B20-Verkabelung und Python-Auslesung (Inhalt → §5.4 + §5.7 Master). Wurde nicht direkt verwendet, da das Projekt den rohen DS18B20 mit externem Pull-Up nutzt.

> Hinweis: Die `.docx` und `.pdf` können nicht mit einem VERALTET-Banner
> versehen werden (Binärformate). Sie sind aber inhaltlich vollständig in
> die Master-Doku übernommen und gelten ebenfalls als historische
> Quelldokumente. Für die Diplomarbeit und alle weiteren Referenzen
> die Master-Doku verwenden.

**docs/DB_Setup/**
- `AQMS_DB_Setup_Doku.md` — erste DB-Setup-Anleitung (Iteration 1, → §14 Master)
- `AQMS_DB_Schema_3NF.md` — frühe 3NF-Doku (Iteration 3, → §15 Master)
- `AQMS_DB_Schema_Dokumentation_v2.md` — frühe DB-Doku (Iteration 2, → §14 Master)
- `AQMS_Datenbankdokumentation.md` — finalere Variante mit MinValue/MaxValue (Iteration 4, → §14 Master)
- `AQMS_Persistenzschicht_Masterdoku.md` — bisheriger DB-Master (vollständig integriert in §13–§22)

**docs/db_schema/**
- `AQMS_Datenbankdokumentation.md` — Doppel der DB_Setup-Variante (→ §14 Master)
- `AQMS_DbContext_Lerndoku.md` — DbContext-Detail-Lerndoku (vollständig integriert in §18–§21)
- `MIGRATION_ANLEITUNG.md` — Migrations-Anleitung der Iteration 4 (→ §22 Master)

### 34.3 Was bleibt aktiv

Aktiv und gepflegt bleiben:

- **Diese Master-Doku** ([docs/AQMS_Masterdoku.md](AQMS_Masterdoku.md))
- **Diplomarbeit-Vorlage und -Manuskript** ([docs/Diplomarbeit/](Diplomarbeit/))
- **PDF-Versionen** der ERDs und Schema-Diagramme (Bildmaterial für die Diplomarbeit)
- **Coding-Guide** ([docs/VS_Setup/AQMS_Coding_Guide.md](VS_Setup/AQMS_Coding_Guide.md)) als Lernmaterial für die noch ausstehenden Phasen 2–4

Alle Word-Dokumente (`.docx`), PDFs, Screenshots und HTML-Versionen
bleiben unverändert — sie sind Bildmaterial bzw. Lieferdokumente.

### 34.4 Versionsstand dieser Datei

| Datum | Version | Änderung |
|---|---|---|
| 09.05.2026 | 1.0 | Initiale Konsolidierung aus 16+ Einzeldokumenten |
| 09.05.2026 | 1.1 | §5 erweitert: Pi-OS-Installation, DS18B20-Pull-Up-Problem mit Diagnose+Lösung, 1-Wire-Aktivierung, Sensor-Auslesung (Joy-IT-Referenz + C#-Skelett). §7 erweitert: Shelly-App-Erst-Inbetriebnahme, A1-Router-DHCP-Reservierung, Test-Loop für alle 5 Shellys. §11 umstrukturiert: appsettings.json wird leer eingecheckt, Connection-String per User Secrets (Dev) oder Umgebungsvariable (Prod). |
| 19.05.2026 | 1.2 | VPS-Migration vollständig dokumentiert: neue Sektionsgruppe §23A–§23J (Anlass/Entscheidung, DNS-Umstellung, Server-Grundeinrichtung, .NET 10, Docker, SQL Server in Docker, Deployment AQMS.Web, systemd-Service, Nginx, HTTPS — alle real durchgeführten Schritte mit Begründung und Verifikation). Bisherige §23–§28 als historischer Stand (alter Debian-12-VPS) markiert. §22.7 ergänzt: zwei Praxisprobleme beim Einspielen des Schema-Skripts via `sqlcmd` (BOM, QUOTED_IDENTIFIER) mit verbindlichem Einspiel-Befehl. §30.11 ergänzt: verworfene Alternativen Swap-File / PostgreSQL-Wechsel. §32 aktualisiert: Backend-Stack auf neuem VPS vollständig, DB-Verbindung verifiziert. Offen aus §33.1: erster Identity-User / Login durchspielen. |
| 19.05.2026 | 1.3 | §23K ergänzt: erster Identity-User angelegt und per DB bestätigt, Login End-to-End verifiziert; öffentliche Registrierung deaktiviert (Routing-Umleitung in Program.cs) mit Begründung. §22.7 erweitert: `-I`-Schalter gilt für jeden schreibenden `sqlcmd`-Zugriff auf die `aqms`-DB, nicht nur fürs Schema-Skript. §23H.6 ergänzt: Praxisproblem SQL-Login 18456 (Passwort-Abgleich Container/Override). §23H.7 ergänzt: Zwei-Schritt-Deploy-Verfahren wegen Verzeichnisrechten des Service-Users (§28-Problem auf neuem VPS). |
| 20.05.2026 | 1.4 | §23K.3 ergänzt: IdentitySeeder als dauerhafte Architekturkomponente (statische Klasse, idempotent, Rollen + Admin beim App-Start) mit vollständiger Begründung (warum nicht HasData, warum eigene Klasse, warum DI-Scope), Aufbau, Integration in Program.cs (`.AddRoles<IdentityRole>()` + Scope-Aufruf) und Konfigurations-Setup (User Secrets lokal / systemd-Override in Produktion). §30.12 ergänzt: bewusste Verwerfung des generischen Repository-Patterns zugunsten schlankem Service-Layer (Begründung mit EF-Core-Eigenschaften und Projektgröße). §32 aktualisiert: Identity-Seeder ✓, Repository-Layer als verworfen markiert. |
| 2026-05-22 08:45 | 1.5 | §11.7 ergänzt: Mehrgerät-Realität dokumentiert (Firmen-Gerät → Docker, Privat-Gerät → LocalDB, VPS → Docker) und bewusste Entscheidung gegen `appsettings.Development.json` als Secret-Träger wegen OneDrive-Sync, Lessons Learned aus Cleanup vom 22.05., verbindlicher Workflow „App auf neuem Gerät einrichten". §11.4 entschärft (verweist auf §11.7). §32 ergänzt: neue Statuszeile „Lokales Setup auf zwei Geräten via User Secrets verifiziert". §32.3 Update 2026-05-22 ergänzt. Ab dieser Version: Doku-Versionseinträge mit Datum **und** Uhrzeit (ISO-Format) für bessere PM-Nachvollziehbarkeit; ältere Einträge unverändert. |
| 2026-05-29 13:20 | 1.6 | §23L ergänzt: API-Key-Middleware als Architekturkomponente vollständig dokumentiert (Anlass, vier Architekturentscheidungen — ein Schlüssel, X-API-Key-Header, konstantzeitiger Vergleich gegen Timing-Attacks, Pipeline-Position vorn —, Klassen-Aufbau mit Code, Integration in Program.cs inkl. nachgezogenem `UseAuthentication`, Konfigurations-Setup pro Umgebung, drei Test-Szenarien als Verifikation, Status pro Umgebung). §11.7 erweitert: `ApiKey` im Workflow „App auf neuem Gerät einrichten" plus Hinweis „pro Umgebung anderer Wert" und PowerShell-Generator. §32 aktualisiert: Statuszeile API-Key-Middleware ✓ (ersetzt alte ⌛-Zeile). §32.3 Update 2026-05-29 ergänzt mit Nebenbefund `UseAuthentication` und Verifikations-Status. |
| 2026-05-31 20:30 | 1.7 | §23M ergänzt: MeasurementsController als erster produktiver API-Controller vollständig dokumentiert (Anlass mit Schreibweg Worker→DB und Leseweg DB→Dashboard, sechs Architekturentscheidungen — append-only ohne Update/Delete, Klassen-Attribute, Konstruktor-Injektion als dritter DI-Stil, DTO-Pattern für Eingabe und Ausgabe, globale Exception-Behandlung statt try/catch, 400 für unbekannte fachliche Identifier —, Klassen-Aufbau mit beiden DTOs und Controller-Code, Integration in Program.cs ohne Anpassungen, Verifikation mit POST- und GET-Aufruf, Praxisproblem JSON-Zyklus mit Symptom/Ursache/Lösung/Lessons-Learned, vier bekannte offene Punkte). §32 aktualisiert: Statuszeile `/api/measurements` ✓. §32.3 Update 2026-05-31 ergänzt — inklusive Privat-Gerät-Setup am selben Tag (eigener ApiKey-Wert, AdminBenutzer-Secrets nachgezogen, DB-Schema und Identity-Seed real durchgespielt, Lehrmoment „Update-Database ≠ Seeder beim App-Start" zum zweiten Mal bestätigt) und expliziter Vermerk, dass Middleware-Verifikation auf Privat-Gerät noch nachzuholen ist. |
| 2026-06-01 13:19 | 1.8 | VPS-Verifikation für API-Key-Middleware und MeasurementsController durchgezogen. §23L.7 aktualisiert: VPS-Wert gesetzt, drei Test-Szenarien bestanden, Übergangs-Vermerk entfernt. §23L.8 neu: Praxisproblem „stiller systemd-Tippfehler beim ApiKey-Override" mit vollständigem Symptom/Ursache/Lösung/Lessons-Block — Override-Zeile war `Environment="<wert>"` statt `Environment="ApiKey=<wert>"`, systemd akzeptiert syntaktisch ohne Fehlermeldung, Konfigurationsschlüssel war nie befüllt, alle `/api/...`-Anfragen mit 401 — plus Diagnose-Befehl `systemctl show --property=Environment`. §23M.5 aufgeteilt in „Lokal" und „Production (VPS, 2026-06-01)" mit Test-Tabelle und Beweis des Sechs-Komponenten-Stacks anhand `Server: nginx`-Header. §23M.7 entschlackt: „VPS-Verifikation steht aus" entfernt. §32 erweitert: zwei Statuszeilen mit VPS-Verifikations-Datum. §32.3 Update 2026-06-01 ergänzt mit Deploy-Verfahren (Filezilla + Zwei-Schritt-Deploy) und Praxisproblem-Verweis. |
| 2026-06-01 (Cleanup) | 1.9 | **Status-Drift bereinigt und Dokumentation in zwei Rollen getrennt.** Neue Datei `AQMS_Projektstand_2026-06-01.md` als bereinigte, komponentenorientierte Ist-Stand-Doku angelegt (Single Source of Truth für „wie ist es jetzt"); diese Master-Doku als chronologisches Tagebuch klargestellt (Kopf-Hinweis + §34.1 neu gefasst). Behobene Widersprüche: §33.3 listete „Repository-Pattern" als geplant, obwohl §30.12 es als bewusst verworfen dokumentiert — Punkt entfernt, Verweis auf §30.12 ergänzt. §33.1 „Sofortmaßnahmen KW 19–20" (alle erledigt) und erledigte §33.3-Punkte (API-Key-Middleware, MeasurementsController, IdentitySeeder) entfernt; §33 enthält jetzt nur noch echt Offenes, in Phasenreihenfolge neu nummeriert (33.1 Phase 2 … 33.5 Phase 6). §2.2 (eingefrorener Phasenstand 09.05.) durch Verweis auf §32/Projektstand ersetzt. §32.3 als „Verlaufs-Updates (chronologisch)" gekennzeichnet, überholter „noch ausstehend"-Absatz als historisch markiert. §1.3 VPS-IP korrigiert (aktuell 203.0.113.10, alt 203.0.113.11 als stillgelegt gekennzeichnet). §34.1 von „Master-Doku ist SSOT" zu „Rollenteilung Tagebuch/Projektstand" umgeschrieben. |
| 2026-06-04 21:17 | 2.0 | §23N neu: CommandsController + Service-Layer (`CommandService`) als erste Service-Schicht vollständig dokumentiert — Anlass, neun Architekturentscheidungen (kein Interface, HTTP-freier Service mit Result-Enum, Result-Pattern, Multi-Entity-Transaktion in einem SaveChanges, Idempotenz-Riegel, LastSeen nur im Erfolgszweig, Status-Mapping inkl. 404-vs-400-Abgrenzung und 409 für Zustandskonflikt, Scoped-Registrierung), Klassen-Aufbau mit Code (zwei DTOs, Service, Controller), Integration in Program.cs, **Verifikation lokal + VPS** (vier Szenarien + Happy-Path, VPS-Test gegen `aqms.aqms.example.com` am 2026-06-04, inkl. funktionierender PowerShell-5.1-Testbefehle und sqlcmd), vier Lessons Learned (enum.ToString nicht SQL-übersetzbar → load-then-map; neue Entity ohne Add wird still nicht persistiert; await bindet um die ganze Kette; `-SkipHttpErrorCheck` existiert in PS 5.1 nicht → try/catch). §30.13 neu: `ICommandService`-Interface verworfen (analog §30.12). §33.2 bereinigt: CommandsController, Service-Layer **und** VPS-Verifikation als erledigt entfernt; offen bleiben Autorisierung und Befehls-Erstellung. §32.1 ergänzt: Command-Kette als voll verifiziert; §32.2 Command-Platzhalterzeile entfernt; §32.3 Update 2026-06-04. Nachtrag `AQMS_Masterdoku_Nachtrag_2026-06-04.md` in diese Datei eingearbeitet (Single Source of Truth, keine separate Nachtrag-Datei mehr nötig). |

| 2026-06-07 | 2.1 | Phase 2 begonnen, Worker-Gerüst dokumentiert. §10.2 aktualisiert: `Microsoft.Extensions.Http` von „kommt voraussichtlich" in die echte PackageReference verschoben, mit Begründung HttpClient-Factory gegen Socket-Erschöpfung. §10.4 ergänzt: `dotnet add`-Befehl. §10.5 neu: Praxisproblem „`AddHttpClient` nicht gefunden" (Symptom/Ursache/Lösung/Lessons — Web-SDK bündelt das Paket, Worker-SDK nicht). §11.6 aktualisiert: Worker/Program.cs vom Skelett auf registrierten named `HttpClient` `"aqms-api"`, Worker-Konfigurationsschlüssel-Tabelle, Begründung flacher `ApiKey` (Konsistenz mit §23L + systemd-Doppelunterstrich-Falle §23L.8). §32.1 ergänzt: Worker-Gerüst ✓; §32.2 HttpClient-Zeile auf „Polling-Loop + API-Calls" verengt; §32.3 Update 2026-06-07. §33.1 bereinigt: HttpClient-Factory als erledigt entfernt, neu nummeriert, Polling-Loop als nächster Schritt markiert. Projektstand-Fassung `AQMS_Projektstand_2026-06-07.md` angelegt. |
| 2026-06-28 | 2.2 | Polling-Loop in `Worker.cs` implementiert und **lokal + VPS verifiziert**. §11.6 erweitert: produktiver Polling-Loop (`GET /api/commands/pending` über named `HttpClient` `"aqms-api"`) mit zwei Resilienz-Entscheidungen (try/catch nur um den Request, gezielter `HttpRequestException`-Fang wegen `BackgroundServiceExceptionBehavior.StopHost`; Interval-Guard gegen Tight-Loop), Verifikation lokal (gestoppte API → geloggt + Worker läuft weiter) und VPS (`200` gegen `aqms.aqms.example.com`), drei offene Resilienz-Punkte (Timeout-vs-Shutdown-Cancellation, Polly-Backoff, dreifaches Logging). §32.1 +Polling-Loop ✓; §32.2 verengt; §32.3 Update 2026-06-28 (Polling-Loop); §33.1 Polling-Loop entfernt. Projektstand-Fassung `AQMS_Projektstand_2026-06-28.md` angelegt. |
| 2026-06-28 | 2.3 | Befehls-Empfang v2 (geräte-übergreifender Poll). §23N.8 neu: Anlass (Worker pollt als `raspberry-pi`, Schaltbefehle hängen an den Shellys; v1-DTO ohne Ziel/IP), Entscheidung 1a (SmartPlug-Typ-Scoping, `IsEnabled`+IP-Guard) + 2a (IP inline in DTO), konkrete Änderungen (DTO +2 Props Web+Worker, parameterloser Service/Controller, nicht-nullable Rückgabe), **API-Vertragsänderung** (`/pending` ohne `deviceIdentifier`, kein 400-Fall mehr — §23N.5-Zeile obsolet), Verifikation lokal (Device+IP kommen mit, Guard greift, Worker deserialisiert end-to-end; VPS-Redeploy offen), zwei Praxisprobleme (verwaiste Instanz/Port, http-https-Mismatch). §30.14 neu: verworfene Alternativen (per-Shelly-Polling, `ControllerId`-Beziehung, Geräte-Registry-Endpunkt). §32.1 +Befehls-Empfang v2 ✓ (lokal); §32.2 auf Dispatch + Schreibwege verengt; §32.3 Update 2026-06-28 (v2); §33.1 Deserialisieren erledigt → Dispatch nächster Schritt; §33.2 +VPS-Redeploy v2. Projektstand `AQMS_Projektstand_2026-06-28.md` (§2/§4.2/§8/§9) nachgezogen. |
| 2026-07-02 | 2.4 | Dispatch (Shelly-Steuerung) + Pi-Deployment, beide **hardware-/Pi-verifiziert**. §11.8 neu: Dispatch (klassische Shelly-API §7.4, separater Default-Client statt `aqms-api` gegen Key-Leak, zweischichtige Exception-Behandlung, 3s-Timeout + Timeout-vs-Shutdown-Filter), end-to-end gegen echten Shelly bewiesen. §23O neu: Worker-Deployment auf dem Pi (self-contained linux-arm64, `/opt/aqms-worker`, Service-User `aqms`, native ExecStart, `DOTNET_ENVIRONMENT=Production`, `ApiKey`-Override), inkl. bewusster Vorab-Deployment-Entscheidung und drei Praxisproblemen (`appsettings.prduction`-Tippfehler → localhost-Fallback; `DOTNET_ENVIRONMENT` ≠ `ASPNETCORE_ENVIRONMENT`; leerer `ApiKey`-Override). §30.15 neu: verworfene Deployment-Alternativen (Framework-Dependent, appsettings-Editieren). §32.1 +Dispatch ✓ +Pi-Deployment ✓, Befehls-Empfang v2 auf VPS-verifiziert; §32.2 Dispatch/Shelly/systemd-Unit raus, auf API-Schreibwege verengt; §32.3 Update 2026-07-02; §33.1 Dispatch+systemd erledigt → Result-Reporting nächster Schritt; §33.2 VPS-Redeploy erledigt entfernt. TOC: §23O ergänzt. |
| 2026-07-02 | 2.5 | Result-Reporting + zwei geschlossene Resilienz-Punkte. §11.9 neu: `ReportResultAsync` (POST `/result` über `aqms-api`-Client), Skip-Fälle (kein IP / unbekannte Action) als `Failed` gemeldet, at-least-once (kein throw bei Melde-Fehler, 409 benign). Zwei Resilienz-Fixes: eigener 10-s-Timeout für den `aqms-api`-Client + Timeout-vs-Shutdown-Filter auf Poll UND Melden (spezifischer Catch vor dem breiten). §11.6 Resilienz-Punkt (a) auf gelöst; §11.8 „offen (a) kein Result-Reporting" → Anschluss §11.9. §23O.7 +Praxisproblem 4 (Overlay-Tippfehler reproduziert, weil nur am Pi statt an der Quelle geflickt). §23O.8 neu: offene Entscheidung BaseUrl-Overlay-Datei vs. systemd-`Environment` (zwei Deployment-Ausfälle als Evidenz, bewusst offen). §32.1 +Result-Reporting ✓; §32.2 auf `POST /measurements` verengt; §33.1 Result-Reporting erledigt → DS18B20 nächster Schritt, +offene BaseUrl-Entscheidung. Projektstand §6.4/§8/§9 nachgezogen. |
| 2026-07-07 | 2.6 | Sensor-Pfad (DS18B20 → `/api/measurements`) — **Phase 2 funktional abgeschlossen** (Pi macht Steuerung + Monitoring). §11.10 neu: `Ds18b20Reader` (sysfs-Glob `28-*`, `w1_slave`-Read, `static ParseTemperature` mit CRC-Gate + `t=`-Parse + `/1000`, `null` bei ungültig, unit-testbar; `AddSingleton`), `ReportMeasurementAsync` (POST `/measurements` über `aqms-api`-Client, `DeviceIdentifier="raspberry-pi"`, `MeasurementTypeName="Temperature"`), bewusste Kein-Retry-Semantik (Gegensatz zum Result-Reporting), Sensor-Health-Eskalation (Zähler + `LogError` bei Dauerausfall, Option A), Kadenz (`MeasurementIntervalSeconds`, Zeitvergleich im Loop statt zweitem Timer), Ein-Loop-Trade-off (Mess-Read blockiert kurz den Poll, Kadenz ans Poll-Raster gequantelt) als bewusstes „Warum", Option B (Health in DB/Dashboard) als Phase-4-Ausblick. Verifikation am Pi: POST 201, `Messung gesendet: 25.125 °C`, `Measurements`-Tabelle füllt sich. §32.1 +Sensor-Pfad ✓ +Sensor-Health ✓; §32.2 DS18B20/measurements-Zeilen raus; §33.1 Phase 2 funktional abgeschlossen → nur Politur/Tests offen. Projektstand-Fassung `AQMS_Projektstand_2026-07-07.md` angelegt. |

| 2026-07-12 | 2.7 | **Phase 3 und Phase 4 funktional abgeschlossen — vollständige Regelkette gegen reale Hardware verifiziert und gefilmt.** §23P neu: Befehls-Erstellung, Autorisierung und Dashboard — Anlass (der Regelkreis war offen: niemand konnte Befehle *erzeugen*), zentrale Architekturentscheidung „zwei Zugangswege, zwei Auth-Mechanismen" (Toggle über MVC-Route mit Cookie-Auth statt `/api`, weil der API-Key sonst in die Browser-Seite müsste und die Rollenprüfung umgehbar wäre), `CreateCommandAsync` mit vier begründeten Entscheidungen (eigenes Ergebnis-Enum, serverseitige `userId`, Abweisung deaktivierter Geräte, Idempotenz-Riegel gegen Relais-Flattern), `DashboardController` (`[Authorize]` + `Roles = "Admin"` + Antiforgery + Post/Redirect/Get), ViewModel statt Entity inkl. serverseitiger Zeitzonen-Auflösung, Praxisproblem §23P.6 (`Device.LastSeen` wurde nur beim Schalten gesetzt, nie beim Messwert-Empfang → Pi wäre dauerhaft „offline"; Symptom/Ursache/Lösung/Lesson), Produktionsverifikation §23P.7 (7 Schritte, realer Shelly geschaltet, gefilmt), bewusste Einschränkung §23P.8 (kein Auto-Refresh). §23Q neu: Unit-Tests mit xUnit (16 Tests, alle grün) — Provider-Entscheidung InMemory mit ehrlicher Abgrenzung (testet Service-Logik, nicht DB-Constraints), `ParseTemperature` (7 Fälle inkl. `[Theory]` und Negativ-Temperatur), `CommandService` (9 Fälle, inkl. der beiden Negativ-Tests, die die Idempotenz-Riegel absichern). §23O.8 von „offene Entscheidung" auf **entschieden**: BaseUrl bleibt in der Overlay-Datei — beide Deployment-Ausfälle hatten *eine* Ursache (Dateiname-Tippfehler), die an der Quelle behoben ist; methodische Lesson Learned dazu ergänzt. §30.16 neu: verworfene Alternativen zum Dashboard-Scope (Auto-Refresh, SignalR, Shelly-Health-Check, Polly) mit Begründungen. §32 Kopf auf 2026-07-12; §32.1 +6 Zeilen (Befehls-Erstellung, Autorisierung, Dashboard, Chart.js, vollständige Regelkette, Unit-Tests); §32.2 Phase-4-Zeilen entfernt, Manuskript als kritischer Pfad markiert; §32.3 Update 2026-07-12. §33.1 xUnit-Punkt + BaseUrl-Entscheidung als erledigt entfernt, Polly als bewusst zurückgestellt markiert; §33.2 Phase 3 abgeschlossen; §33.3 Phase 4 funktional abgeschlossen + Abgrenzungsliste; §33.4 Unit-Tests als erledigt. TOC: §23P, §23Q ergänzt. Projektstand-Fassung `AQMS_Projektstand_2026-07-12.md` angelegt. |

| 2026-07-12 | 2.8 | **Auto-Reload-Revision, Messintervall, Dauerlauf gestartet.** §23P.8 von „bewusste Einschränkung: kein Auto-Refresh" auf **Revision** umgeschrieben: Position zurückgenommen (Anlass: der Entwickler lief selbst in die Falle → UX-Defekt, nicht „ehrliche Abbildung"), Lösung bedingter **selbstterminierender** Reload (`@if (HasPendingCommand)` → `setTimeout(reload, 3000)`; endet von allein, sobald der Worker gemeldet hat), drei verworfene Alternativen tabelliert (`meta refresh`, Dauer-`setInterval`, SignalR), bekannter Randfall (offline-Pi → Endlos-Reload) als offener Punkt, methodische Lesson Learned (eine Position unter Druck zu halten ist richtig, sie unter *neuer Evidenz* zu revidieren ebenfalls). Status ehrlich als „implementiert, VPS-Verifikation offen" geführt. §30.16 entsprechend korrigiert: nicht mehr „Auto-Refresh verworfen", sondern „permanentes AJAX-Polling verworfen" + Revisionshinweis. §23R neu: Messintervall 3600 s mit fachlicher Begründung (Wärmekapazität; Abtastrate folgt der Änderungsrate der Messgröße), Konsequenzen für Datenmenge, Visualisierung (50 Werte ≈ 2 Tage → Vorlauf nötig) und Sensor-Health-Eskalation; **Praxisproblem §23R.2** (hartkodierter 5-min-`PiOnline`-Schwellwert vs. 60-min-Takt → Pi 55/60 Minuten fälschlich „offline"; Kern: implizite, nirgends deklarierte Kopplung zwischen Worker-Config und Web-Code, bricht lautlos ohne Compiler-Fehler oder roten Test) mit Fix-Vorschlag und zwei Lessons Learned; §23R.3 24-h-Dauerlauf gestartet, Auswertungsbefehle hinterlegt, Ergebnis-Platzhalter. §32.1 +3 Zeilen (Auto-Reload 🟡, Messintervall ✓, Dauerlauf 🟡); §32.2 +DEFEKT-Zeile `PiOnline`-Schwellwert; §32.3 Update 2026-07-12 (abends). §33.1 +Messintervall final festlegen; §33.3 +3 offene Punkte (Defekt, VPS-Verifikation, Reload-Randfall); §33.4 Dauerlauf als gestartet markiert. TOC: §23R ergänzt. Projektstand `AQMS_Projektstand_2026-07-12.md` synchron nachgezogen. |

| 2026-07-13 | 2.9 | **24-h-Dauerlauf bestanden — Phase 5 weitgehend abgeschlossen.** §23R.1 neu gefasst: Die Messintervall-Entscheidung ist eine **Abwägung zwischen zwei berechtigten Positionen** — fachlich wäre ein langes Intervall richtig (Wasser ist träge, die Abtastrate sollte der Änderungsrate der Messgröße folgen), aber der Dauerlauf ist ein Zuverlässigkeitstest *pro Zyklus*: 3600 s hätten 24 Zyklen ergeben, 20 s ergaben 4.136 — ein 170-fach härterer Test bei identischem Aufwand. Entscheidung: **20 s, Testbarkeit vor Datenökonomie**; Minutenbereich als Ausblick für den realen Dauerbetrieb benannt. Methodische Lesson: die fachlich richtige und die für den Test nützliche Abtastrate sind nicht dasselbe. §23R.2 von „aktiver Defekt" auf **latenter Defekt** korrigiert: der 5-min-`PiOnline`-Schwellwert ist bei 20 s Takt korrekt, bricht aber **lautlos** bei jeder Intervall-Erhöhung — implizite, nirgends deklarierte Kopplung zwischen Worker-Config und Web-Code; Lesson: latente Defekte sind gefährlicher als aktive, weil sie erst auffallen, wenn jemand die Gegenseite ändert. **§23R.3: vollständiges Testprotokoll** — 4.136 Messzyklen in 24 h, **0 Fehler, 0 Warnungen, 0 Lücken**, Service nach 24 h stabil (`active (running)`); belegt Schleifen-Stabilität, CRC-Zuverlässigkeit, `IHttpClientFactory`-Robustheit über 4.136 HTTPS-POSTs (kein Socket-Exhaustion) und Speicherstabilität unter systemd. Fachliche Plausibilisierung der Messwerte: glatter Tag-Nacht-Verlauf 26,25–28,06 °C, Sprünge im Bereich der 12-Bit-Sensorauflösung (0,0625 °C) — kein Rauschen, sondern Physik. Lesson: ein Dauerlauf, dessen Ergebnis niemand nachrechnet, beweist nichts; die Auswertungsabfrage gehört *vor* den Lauf geplant. §32.1 Dauerlauf ✓ + Messintervall korrigiert; §32.2 Defekt-Zeile auf 🟡 latent; §32.3 Update zusammengefasst; §33.1 Messintervall entschieden; §33.3 Defekt entschärft; §33.4 Dauerlauf erledigt. Projektstand §6.4/§6.6/§9.3/§10/§11 synchron. |
| 2026-08-06 | 3.0 | **Phase 5 abgeschlossen, Seeder erweitert.** §23K.4 neu: zweites Konto in der Rolle `User` über die Konfigurationssektion `StandardBenutzer`, Anlegevorgang in `EnsureUserAsync` ausgelagert, Admin als Pflichtkonto mit Startabbruch bei fehlender **oder ungültiger** Konfiguration (Unterscheidung „nicht konfiguriert" vs. „konfiguriert und trotzdem nicht anlegbar"), Auswertung von `CreateAsync` als Korrektur am Bestand, Kopplung `EmailConfirmed` ↔ `RequireConfirmedAccount` festgehalten, bewusster Preis der Neustartschleife unter `Restart=always` begründet. §23S neu: Abschluss der Phase 5 mit Testschema und drei methodischen Regeln, eigener Testcontainer auf Port 1434, Migrationstest, Rollentrennung (CSRF-Falle, Erwartung „403" als sachlich falsch korrigiert — Cookie-Handler leitet auf AccessDenied um), Einschleusungsversuch auf zwei Ebenen (Codesuche ohne Treffer + erzeugtes SQL als eigentlicher Beweis), Fehlerdarstellung an echter Ausnahme in Produktionskonfiguration, SSL Labs Note A, Schaltdurchlauf über fünf Geräte mit **erster Messung der Schaltlatenz** (5–9 s, Mittel 6,9 s) und Einordnung gegen die Herleitung in §3.7, Schnittstellenprüfung aus §23M.5/§23N.5 zusammengeführt, drei Praxisprobleme (launchSettings überschreibt Umgebung; fehlender vs. falscher API-Key von außen ununterscheidbar; App im Vordergrund beendet) und vier Lessons Learned. §23Q.3 korrigiert: Titel nannte 9 Tests, die Tabelle listet 8 — `Total: 16` ergibt sich aus 7 Sensor-Methoden (8 Fälle wegen einer `[Theory]`) plus 8 CommandService-Tests. §32 auf Stand 2026-08-06 gesetzt und um 8 Statuszeilen ergänzt. §33.4 von Offen-Liste auf „abgeschlossen" umgeschrieben; offen bleiben Worker-seitige Tests (§33.1) und der neue Nebenbefund HTML-Fehlerantworten unter `/api`. Neue Datei `AQMS_Testprotokolle.md` als Quelle für Anhang D. |

| 2026-08-06 | 3.1 | **Issue-Board mit dem Doku-Stand abgeglichen.** 43 GitHub-Issues geschlossen (42 erledigt, 1 verworfen), jedes mit Verweis auf die belegende Doku-Sektion; offen bleiben nur Polly-Retry, Worker-Tests, Login-Seiten-Gestaltung und die Phase-6-Aufgaben. §32.2 bereinigt: die Zeilen „24h Lauftest" und „Sicherheitstests (SSL Labs etc.)" standen als offen, obwohl §32.1 sie als bestanden führt — entfernt; die Login-Zeile trägt jetzt die Issue-Nummer. §32.3 Update 2026-08-06 ergänzt (Abgleich, aufgedeckte Status-Drift, methodische Lesson). |

Künftige Änderungen werden hier dokumentiert.

---

*Ende der Master-Doku.*
