# Essenskasse

[![CI-CD](https://github.com/Kretchen001/Essenskasse/actions/workflows/build-and-test.yml/badge.svg?branch=master)](https://github.com/Kretchen001/Essenskasse/actions/workflows/build-and-test.yml)
[![GitHub issues](https://img.shields.io/github/issues/Kretchen001/Essenskasse)](https://github.com/Kretchen001/Essenskasse/issues)

## Inhaltsverzeichnis / Table of Contents
- [Beschreibung / Description](#beschreibung--description)
- [Funktionen / Features](#funktionen--features)
- [Installation und Start / Installation and Run](#installation-und-start--installation-and-run)
- [Datenbank-Updates / Database updates](#datenbank-updates--database-updates)
- [Update auf 1.4.2 / Upgrade to 1.4.2](#update-auf-142--upgrade-to-142)
- [Update auf 1.4.1 / Upgrade to 1.4.1](#update-auf-141--upgrade-to-141)
- [Update auf 1.4.0 / Upgrade to 1.4.0](#update-auf-140--upgrade-to-140)
- [Update auf 1.3.0 / Upgrade to 1.3.0](#update-auf-130--upgrade-to-130)
- [Betrieb im Container / Operation in containers](#betrieb-im-container--operation-in-containers)
- [Entwicklung und Tests / Development and tests](#entwicklung-und-tests--development-and-tests)
- [Texte und Sprachen / Texts and languages](#texte-und-sprachen--texts-and-languages)
- [MySQL aktualisieren / Upgrading MySQL](#mysql-aktualisieren-optional-manuell--upgrading-mysql-optional)
- [Contribution](#contribution)

## Beschreibung / Description
**Deutsch:** In dieser Kasse können mehrere Anbieter und Personen eingespeichert werden, um die Verwaltung des Mittagsgeldes des Personals zentral zu gestalten.
Aufgrund der MySQL-Datenbank sind erweiterte statistische Auswertungen bei vorhandenen Kenntnissen zu SQL leicht möglich.

Da Einzahlungen und Bestellungen getrennt voneinander gespeichert werden, ist eine separate Aufstellung und Verarbeitung möglich.

Sobald ein Anbieter oder eine Person nicht mehr genutzt werden soll, lässt sie sich über eine Schaltfläche ausblenden. Sie wird dabei nur in der Datenbank als unsichtbar markiert, steht für die Auswertungen weiterhin zur Verfügung und kann wiederhergestellt werden.

**English:** Several providers and persons can be stored in this cash register in order to centralise the administration of staff lunch money.
Due to the MySQL database, extended statistical analyses are easily possible with existing knowledge of SQL.

As deposits and orders are stored separately, separate listing and processing is possible.

As soon as a provider or person is no longer used, it can be hidden via a button. It is only marked as invisible in the database, so it is still available for analyses and can be restored.

## Funktionen / Features
**Deutsch**
- **Dashboard:** Kassenstand und Saldo jeder Person; Personen unterhalb eines einstellbaren Grenzwerts werden als auffällig hervorgehoben (rechts/unten oder einspaltig, Spalten vertauschbar).
- **Einzahlungen:** mit einstellbaren Schnellbeträgen. Optional wird eine Bestätigungsmail im lokalen Mailprogramm vorbereitet (nie / pro Einzahlung wählbar / immer).
- **Bestellungen:** pro Tag und Anbieter, mit Auswahl der Tage der aktuellen Woche, Übersicht je Tag und Person und Warnung bei bereits vorhandenen Bestellungen.
- **Kaffeekasse:** getrennte Kasse mit Ein- und Auszahlungen; ohne Anmeldung nur lesbar.
- **Änderungen:** vorhandene Einzahlungen und Bestellungen nachträglich ändern oder stornieren. Dafür wird ein Änderungstoken benötigt, jede Änderung wird samt Initiator im Log protokolliert.
- **Sammel-Zahlungsaufforderungen:** Auf dem Dashboard bereitet man die Zahlungsaufforderungen für alle auffälligen Personen mit E-Mail-Adresse Person für Person vor (ein Klick, eine Mail).\n- **Export:** Einzahlungen und Bestellungen eines Zeitraums (optional einer Person) als CSV-Datei für Excel.
- **Personen und Anbieter:** anlegen, bearbeiten, suchen, sortieren, ausblenden und wiederherstellen. Jede Person hat eine Sprache für Mails.
- **Mails:** Zahlungsaufforderung, Einzahlungsbestätigung und Fehlermeldung an den Admin gehen zweisprachig raus: zuerst in der Sprache der Person (bei der Fehlermeldung Deutsch), danach in der Sprache des angemeldeten Benutzers. Sind beide gleich, steht der Text nur einmal in der Mail.
- **Sprachen und Aussehen:** Deutsch, Englisch, Spanisch, Russisch (Wechsel sofort, ohne Neuladen); helles und dunkles Farbschema.
- **Währung:** in den Einstellungen wählbar (z. B. EUR, USD, CHF) und gilt für alle Beträge. Es ändert sich nur die Anzeige, gespeicherte Beträge werden nicht umgerechnet.
- **Einstellungen je Benutzer:** werden lokal gespeichert und lassen sich mit dem Server abgleichen (Schnellbeträge, Farbschema, Sprache, Währung, Sortierung).
- **PWA:** installierbar, nach dem ersten Laden auch offline startbar; neue Versionen werden per Hinweis angeboten.

**English**
- **Dashboard:** cash balance and balance of every person; people below a configurable limit are highlighted (right/bottom or single column, columns can be swapped).
- **Payments:** with configurable quick amounts. Optionally a confirmation email is prepared in the local mail program (never / selectable per payment / always).
- **Orders:** per day and provider, with buttons for the days of the current week, an overview per day and person, and a warning if orders already exist.
- **Coffee fund:** separate fund with deposits and payouts; read-only without signing in.
- **Changes:** edit or reverse existing payments and orders afterwards. This requires a change token; every change including who made it is written to the log.
- **Collective payment requests:** On the dashboard the payment requests for all highlighted people with an email address are prepared one person at a time (one click, one email).\n- **Export:** payments and orders of a period (optionally of one person) as a CSV file for Excel.
- **People and providers:** add, edit, search, sort, hide and restore. Every person has a language for emails.
- **Emails:** payment request, payment confirmation and the error report to the admin are sent bilingually: first in the language of the person (German for the error report), then in the language of the signed-in user. If both are the same, the text appears only once.
- **Languages and look:** German, English, Spanish, Russian (switching takes effect immediately, no reload); light and dark theme.
- **Currency:** selectable in the settings (e.g. EUR, USD, CHF) and applies to all amounts. Only the display changes, stored amounts are not converted.
- **Settings per user:** stored locally and can be synchronised with the server (quick amounts, theme, language, currency, sorting).
- **PWA:** installable, starts offline after the first load; new versions are offered through a notice.

## Installation und Start / Installation and Run
**Deutsch:** Voraussetzung ist [Docker](https://www.docker.com/) (mit `docker-compose`). Nur für die Entwicklung wird zusätzlich das .NET-10-SDK benötigt.

**English:** [Docker](https://www.docker.com/) (with `docker-compose`) is required. Only for development the .NET 10 SDK is needed in addition.

1. **`.env`-Datei anlegen / create the `.env` file** in `code/` (not part of the repository / gehört nicht ins Repository):
   ```env
   DB_ROOT_PASSWORD=root
   DB_DATABASE=essenskasse
   DB_USERNAME=u
   DB_PASSWORD=p
   ```
2. **API-`appsettings.json`** (`code/EssenskasseAPI/`) – **Deutsch:** wird nur für den lokalen Start der API ohne Container gebraucht; im Container setzt `docker-compose.yml` den Connection String aus der `.env`. **English:** only needed when running the API locally without a container; in the container `docker-compose.yml` sets the connection string from the `.env`.
   ```json
   "ConnectionStrings": {
       "essenskasse": "Server=database;Port=3306;Uid=u;Pwd=p;Database=essenskasse;SslMode=None;"
   }
   ```
   **Deutsch:** `Server` ist der Dienstname des MySQL-Containers (`database`), bei lokal gestarteter API `localhost` mit Port `8008`. `Uid` und `Pwd` sind Benutzer und Passwort aus der `.env`.
   **English:** `Server` is the service name of the MySQL container (`database`), for a locally started API use `localhost` with port `8008`. `Uid` and `Pwd` are the user and password from the `.env`.
3. **API-Adresse der PWA / API address of the PWA** (`code/Essenskasse/wwwroot/appsettings.json`) – **Deutsch:** muss vom Browser aus erreichbar sein. **English:** must be reachable from the browser.
   ```json
   {
     "API-Adress": "http://localhost:8010/api/"
   }
   ```
4. **Starten / Start** (im Ordner `code` / in the `code` folder):
   ```bash
   cd code
   docker-compose up -d --build
   ```
   **Deutsch:** Dabei wird die MySQL-Datenbank heruntergeladen, ihr Container angelegt und danach werden API und PWA gebaut. Beim ersten Start legt `MySqlScripts/init.sql` die Tabellen an.
   **English:** This downloads the MySQL database, creates its container and then builds the API and the PWA. On the first start `MySqlScripts/init.sql` creates the tables.
5. **Erreichbarkeit / Ports** (laut `docker-compose.yml`):

   | Dienst / Service | Adresse / Address |
   |---|---|
   | PWA | `http://localhost:8009` |
   | API (Swagger unter `/swagger`) | `http://localhost:8010` |
   | MySQL (nur lokal / local only) | `127.0.0.1:8008` |

   ```cmd
   start http://localhost:8009
   ```
   **Deutsch:** Das ruft das Dashboard auf.
   **English:** This opens the dashboard.
6. **Ersten Benutzer anlegen / create the first user** – **Deutsch:** Es gibt keinen Standardbenutzer. Der allererste Benutzer wird ohne Anmeldung über die API angelegt (danach nur noch durch Administratoren). **English:** There is no default user. The very first user is created through the API without signing in (afterwards only administrators can create users).
   ```bash
   curl -X POST http://localhost:8010/api/user/new -H "name: Admin" -H "userName: admin" -H "pwd: <Passwort>" -H "email: admin@example.org" -H "role: 1"
   ```
   **Deutsch:** Danach kann man sich in der PWA anmelden. **English:** Afterwards you can sign in in the PWA.

## Datenbank-Updates / Database updates
**Deutsch:** Die Update-Skripte in `code/MySqlScripts/` (`update_vX-Y-Z_zu_A-B-C.sql`) spielt die API beim Start selbst ein. Welche schon gelaufen sind, steht in der Tabelle `essenskasse.SchemaMigration`. Ein Update besteht also aus **Sicherung erstellen** und `docker-compose up -d --build`; manuelles Ausführen ist nicht nötig.
- Läuft ein Skript nicht durch, startet die API nicht und das Skript gilt nicht als angewendet. Die Meldung steht in der Logdatei der API (`/app/Log` im Container).
- Abschalten: Umgebungsvariable `Database__AutoMigrate=false`.
- Der Datenbankbenutzer braucht das Recht, Tabellen anzulegen und zu ändern (mit den Einstellungen in `docker-compose.yml` ist das gegeben).
- Neue Updates: eine Datei nach dem Namensmuster ergänzen. Sie muss mehrfach ausführbar sein, denn auf bestehenden Datenbanken laufen beim ersten Start nach der Umstellung einmalig alle Skripte (auch auf einer neuen Datenbank, die `init.sql` schon im Endzustand angelegt hat).
- Die Ausgaben der Skripte (z. B. die Diagnose verwaister Datensätze im Skript 1.2.0 → 1.3.0) werden von der API nicht angezeigt. Wer sie sehen möchte, führt das Skript vorher von Hand aus (siehe unten).
- **Sicherung:** Das Backup der API (`POST api/backup/newNow`, nur Administratoren) schreibt JSON-Dateien in den Ordner `Backups/` neben dem Ordner `code` (Volume `../Backups:/app/backup` in `docker-compose.yml`); er überlebt einen Neubau des Containers. Das Backup enthält Personen, Anbieter, Einzahlungen, Bestellungen, Kaffeekasse und Einstellungen, aber keine Benutzer. Für eine vollständige Sicherung besser einen Dump der Datenbank erstellen (siehe [MySQL aktualisieren](#mysql-aktualisieren-optional-manuell--upgrading-mysql-optional)).

**English:** The update scripts in `code/MySqlScripts/` (`update_vX-Y-Z_zu_A-B-C.sql`) are applied by the API itself at start-up. The table `essenskasse.SchemaMigration` records which ones have run. An update is therefore: **create a backup** and `docker-compose up -d --build`; running scripts by hand is not necessary.
- If a script fails, the API does not start and the script is not marked as applied. The message is in the API log file (`/app/Log` in the container).
- Switch off with the environment variable `Database__AutoMigrate=false`.
- The database user needs the right to create and alter tables (the settings in `docker-compose.yml` provide this).
- New updates: add a file following the naming pattern. It must be safe to run more than once, because on existing databases all scripts run once at the first start after this change (also on a new database whose `init.sql` already created the final state).
- The output of the scripts (e.g. the diagnosis of orphaned records in the 1.2.0 → 1.3.0 script) is not shown by the API. If you want to see it, run the script by hand beforehand (see below).
- **Backups:** the API backup (`POST api/backup/newNow`, administrators only) writes JSON files to the `Backups/` folder next to the `code` folder (volume `../Backups:/app/backup` in `docker-compose.yml`); it survives rebuilding the container. The backup contains people, providers, payments, orders, coffee fund and settings, but no users. For a complete backup better create a database dump (see [Upgrading MySQL](#mysql-aktualisieren-optional-manuell--upgrading-mysql-optional)).

**Skript von Hand ausführen / run a script by hand** (`<DB_ROOT_PASSWORD>` = value from the `.env`):
```bash
cd code
docker exec -i EssenskasseDB mysql -uroot -p<DB_ROOT_PASSWORD> essenskasse < MySqlScripts/update_v1-2-0_zu_1-3-0.sql
```
PowerShell: `Get-Content MySqlScripts/update_v1-2-0_zu_1-3-0.sql -Raw | docker exec -i EssenskasseDB mysql -uroot -p<DB_ROOT_PASSWORD> essenskasse`

## Update auf 1.4.2 / Upgrade to 1.4.2
**Deutsch:** Sicherung erstellen und `docker-compose up -d --build`. Die Datenbank-Änderungen spielt die API beim Start selbst ein. Was sich ändert:
- Einzahlungen und Bestellungen lassen sich stornieren: Statt sie zu löschen, legt die API eine Gegenbuchung mit umgekehrtem Betrag an (Zeitpunkt der Buchung plus ein Tick, damit beide in Tages- und Anbieterauswertungen zusammen null ergeben) und korrigiert den Saldo. Beide bleiben in der Liste sichtbar (die stornierte durchgestrichen). Stornierte Buchungen und Stornobuchungen sind nicht mehr änderbar. Dafür ergänzt das Skript `update_v1-4-1_zu_1-4-2.sql` die Spalten `ReversalOf` und `ReversedBy` in den Tabellen `Payment` und `Order`; das Backup enthält sie.\n- Auf dem Dashboard lassen sich die Zahlungsaufforderungen für alle auffälligen Personen nacheinander vorbereiten (nur angemeldet).\n- Neue Seite „Export“ (nur angemeldet): Einzahlungen und Bestellungen eines Zeitraums als CSV-Datei, wahlweise für eine Person. Bestellungen haben einen negativen Betrag.

**English:** Create a backup and run `docker-compose up -d --build`. The API applies the database changes itself at start-up. What changes:
- Payments and orders can be reversed: instead of deleting them, the API creates an offsetting entry with the opposite amount (booking time plus one tick, so that both add up to zero in daily and provider reports) and corrects the balance. Both stay visible in the list (the reversed one struck through). Reversed bookings and reversal entries can no longer be changed. The script `update_v1-4-1_zu_1-4-2.sql` adds the columns `ReversalOf` and `ReversedBy` to the tables `Payment` and `Order`; the backup contains them.\n- On the dashboard the payment requests for all highlighted people can be prepared one after the other (signed in only).\n- New page "Export" (signed in only): payments and orders of a period as a CSV file, optionally for one person. Orders have a negative amount.

## Update auf 1.4.1 / Upgrade to 1.4.1
**Deutsch:** Sicherung erstellen und `docker-compose up -d --build`. Die API spielt das Skript `update_v1-4-0_zu_1-4-1.sql` beim Start selbst ein. Was sich ändert:
- Die Währung der Anzeige ist in den Einstellungen wählbar (je Benutzer, wird mit dem Server abgeglichen). Dafür ergänzt das Skript die Spalte `currency`. Gespeicherte Beträge werden nicht umgerechnet.
- Jede Person hat eine Sprache für Mails (Spalte `Language`, vorhandene Personen: Deutsch). Mails gehen zweisprachig raus (Person, danach angemeldeter Benutzer). Der Mailtext darf jetzt bis etwa 8000 Zeichen lang sein (vorher 2000, wodurch längere Texte abgeschnitten wurden); Sonderzeichen wie `&` werden korrekt übergeben.

**English:** Create a backup and run `docker-compose up -d --build`. The API applies the script `update_v1-4-0_zu_1-4-1.sql` itself at start-up. What changes:
- The display currency can be chosen in the settings (per user, synchronised with the server). The script adds the column `currency` for this. Stored amounts are not converted.
- Every person has a language for emails (column `Language`; existing people: German). Emails are sent bilingually (person first, then the signed-in user). The email text may now be up to about 8000 characters (before: 2000, which cut off longer texts); special characters such as `&` are passed on correctly.

## Update auf 1.4.0 / Upgrade to 1.4.0
**Deutsch:** Sicherung erstellen und `docker-compose up -d --build`. Die Datenbank-Änderungen spielt die API beim Start selbst ein (siehe oben). Was sich ändert:
- Die Anwendung läuft auf .NET 10 (die Docker-Images werden beim Bauen neu geladen).
- Die Bestätigungsmail bei Einzahlungen hat drei Stufen (nie / pro Einzahlung wählbar / immer). Vorhandene Werte bleiben erhalten: Ja → „wählbar“, Nein → „nie“.
- Einstellungen gelten je Benutzer und lassen sich mit dem Server abgleichen. Dafür ergänzt das Skript `update_v1-3-0_zu_1-4-0.sql` fünf Spalten (Schnellbeträge, Farbschema, Sprache, Sortierung von Personen und Anbietern).
- Neue Farbschemata, durchgehend übersetzte Oberfläche (Deutsch, Englisch, Spanisch, Russisch) und modernisierte Seiten.

**English:** Create a backup and run `docker-compose up -d --build`. The API applies the database changes itself at start-up (see above). What changes:
- The application runs on .NET 10 (the Docker images are pulled again when building).
- The payment confirmation email has three levels (never / selectable per payment / always). Existing values are kept: yes → selectable, no → never.
- Settings apply per user and can be synchronised with the server. The script `update_v1-3-0_zu_1-4-0.sql` adds five columns for this (quick amounts, theme, language, sorting of people and providers).
- New colour themes, a fully translated interface (German, English, Spanish, Russian) and modernised pages.

## Update auf 1.3.0 / Upgrade to 1.3.0
**Deutsch**
1. **Sicherung erstellen** (am besten ein `mysqldump`, siehe Hinweis zum API-Backup oben).
2. **Neu bauen und starten:** `docker-compose up -d --build`. Das Skript `update_v1-2-0_zu_1-3-0.sql` läuft dabei automatisch (additiv, ändert keine Daten). Ein Fremdschlüssel wird nur angelegt, wenn es keine verwaisten Datensätze gibt. Die Diagnose (verwaiste Datensätze, abweichende Salden) zeigt nur der manuelle Aufruf des Skripts.

Was sich ändert:
- Die API verlangt für alle Endpunkte ein Sitzungs-Token (`Authorization: Bearer {Token}`). Die PWA sendet es automatisch nach dem Login. Ohne Anmeldung bleiben nur die Anmeldung, das Dashboard (`GET api/person/requestAll`) und die Ansicht der Kaffeekasse (`GET api/transactionprocedure/all`) offen. Eigene Clients/Skripte müssen sich anpassen (Login: `POST api/user/newTokenForUser`).
- Passwörter werden als Hash gespeichert. Bestehende Klartext-Passwörter werden beim ersten erfolgreichen Login automatisch umgestellt.
- Den allerersten Benutzer legt man ohne Anmeldung an (`POST api/user/new` mit den Headern `name`, `userName`, `pwd`, `email`, `role`); danach dürfen das nur Administratoren.
- Buchungen (Bestellung/Einzahlung und Saldo) werden atomar gespeichert; Löschen und Ändern korrigiert den Saldo.
- Optionale Einstellungen der API (z. B. als Umgebungsvariablen im `docker-compose.yml`):

  | Einstellung | Wirkung |
  |---|---|
  | `Security__AdminMaxRole` | Benutzer mit `Role` bis zu diesem Wert sind Administratoren (Backup, Benutzerverwaltung). Standard 100 = jeder angemeldete Benutzer. |
  | `Cors__AllowedOrigins__0` | Erlaubte Adresse der PWA, z. B. `http://meinserver:8009`. Ohne Angabe sind alle Origins erlaubt. |
  | `Swagger__Enabled` | `false` schaltet Swagger ab (Standard: an, erreichbar unter `/swagger`; über „Authorize“ das Token eintragen). |
  | `Database__AutoMigrate` | `false` schaltet das automatische Einspielen der Datenbank-Updates ab. |

**English**
1. **Create a backup** (preferably a `mysqldump`, see the note about the API backup above).
2. **Rebuild and start:** `docker-compose up -d --build`. The script `update_v1-2-0_zu_1-3-0.sql` runs automatically (additive, never changes data). A foreign key is only created if there are no orphaned records. The diagnosis (orphaned records, differing balances) is only shown when the script is run by hand.

What changes:
- All API endpoints require a session token (`Authorization: Bearer {token}`). The PWA sends it automatically after the login. Without signing in, only the login, the dashboard (`GET api/person/requestAll`) and the view of the coffee fund (`GET api/transactionprocedure/all`) stay open. Your own clients/scripts must adapt (login: `POST api/user/newTokenForUser`).
- Passwords are stored hashed; existing plaintext passwords are converted on the next successful login.
- The very first user can be created without a login (`POST api/user/new` with the headers `name`, `userName`, `pwd`, `email`, `role`); afterwards only administrators can do that.
- Bookings (order/payment and balance) are saved atomically; deleting and changing correct the balance.
- Optional API settings (e.g. as environment variables in `docker-compose.yml`): `Security__AdminMaxRole` (users with a `Role` up to this value are administrators, default 100 = every signed-in user), `Cors__AllowedOrigins__0` (allowed address of the PWA, default: all origins), `Swagger__Enabled` (`false` switches Swagger off), `Database__AutoMigrate` (`false` switches off the automatic database updates).

## Betrieb im Container / Operation in containers
**Deutsch**
- Die API im Container bekommt ihren Connection String aus dem `docker-compose.yml` (`ConnectionStrings__essenskasse`, Werte aus `.env`). Die `appsettings.json` darf lokal auf `localhost:8008` zeigen, ohne dass der Container davon beeinflusst wird.
- Der DB-Port `8008` ist nur auf `127.0.0.1` erreichbar (SQL-Client, lokal gestartete API). Die API startet erst, wenn die Datenbank gesund ist.
- PWA-Updates: Der Service Worker nutzt pro Build einen neuen Cache; `appsettings.json` wird nie gecacht. Ist eine neue Version geladen, erscheint der Hinweis „Neue Version verfügbar“ mit „Jetzt aktualisieren“. Ohne Klick wird sie aktiv, sobald alle Tabs der PWA einmal geschlossen waren.
- Der Webserver der PWA (nginx) liefert vorkomprimierte Dateien (`gzip_static`) und prüft Programmdateien bei jedem Aufruf nur auf Änderungen (kein langes Caching, damit nach einem Update nichts Altes hängen bleibt). Bilder werden eine Woche zwischengespeichert.

**English**
- The API in the container gets its connection string from `docker-compose.yml` (`ConnectionStrings__essenskasse`, values from `.env`). The `appsettings.json` may point to `localhost:8008` locally without affecting the container.
- The DB port `8008` is only reachable on `127.0.0.1` (SQL client, locally started API). The API only starts once the database is healthy.
- PWA updates: the service worker uses a new cache per build; `appsettings.json` is never cached. Once a new version is loaded, a notice “A new version is available” with “Update now” appears. Without a click it becomes active as soon as all tabs of the PWA have been closed once.
- The web server of the PWA (nginx) serves pre-compressed files (`gzip_static`) and only checks program files for changes on every request (no long-lived caching, so nothing outdated stays after an update). Images are cached for one week.

## Entwicklung und Tests / Development and tests
**Deutsch:** Mit dem .NET-10-SDK im Ordner `code`: `dotnet build` und `dotnet test` (die Tests brauchen keine Datenbank, das automatische Einspielen der Datenbank-Updates ist in ihnen abgeschaltet). Den Coverage-Report erzeugt `code/analyze.bat` lokal; er gehört nicht ins Repository. Die GitHub-Action `build-and-test` baut und testet bei jedem Push und Pull Request auf `master`.

**English:** With the .NET 10 SDK in the `code` folder: `dotnet build` and `dotnet test` (the tests need no database; the automatic database updates are switched off in them). The coverage report is generated locally by `code/analyze.bat` and is not part of the repository. The GitHub action `build-and-test` builds and tests on every push and pull request to `master`.

### Texte und Sprachen / Texts and languages
**Deutsch:** Alle Texte der Oberfläche stehen in `code/Essenskasse/wwwroot/i18n/{de-DE,en-US,es-ES,ru}.json`. Beim Start wird nur die Startsprache geladen (Standard Deutsch), die anderen holt die App erst beim Öffnen der Einstellungen nach; danach wirkt ein Sprachwechsel sofort. Ein neuer Text braucht einen Eintrag in `Ressources/RessourceAppStrings.cs` und in allen vier Dateien (Platzhalter wie `{0}` müssen in jeder Sprache vorkommen). Der Test `TranslationFilesTest` meldet fehlende oder überzählige Einträge.

**English:** All interface texts are in `code/Essenskasse/wwwroot/i18n/{de-DE,en-US,es-ES,ru}.json` and At start-up only the start language is loaded (German by default); the app fetches the others when the settings are opened, after which a language change takes effect immediately. A new text needs an entry in `Ressources/RessourceAppStrings.cs` and in all four files (placeholders such as `{0}` must appear in every language). The test `TranslationFilesTest` reports missing or surplus entries.

## MySQL aktualisieren (optional, manuell) / Upgrading MySQL (optional)
**Deutsch:** Das Image `mysql:8.0.2` ist eine frühe Vorabversion. Ein einfaches Hochsetzen des Tags auf den vorhandenen Datenordner wird von MySQL für diese Version **nicht unterstützt** und kann die Datenbank unbrauchbar machen. Deshalb wird es nicht automatisch geändert. Sicherer Weg über einen Dump:
1. Sicherung der Daten: `docker exec EssenskasseDB mysqldump -uroot -p<DB_ROOT_PASSWORD> --databases essenskasse --routines --single-transaction > essenskasse_dump.sql` (Datei prüfen, nicht leer?). In Windows PowerShell erzeugt `>` eine UTF-16-Datei; dort besser in der `cmd` ausführen oder den Dump im Container erzeugen (`--result-file=/tmp/dump.sql`) und mit `docker cp` herauskopieren.
2. `docker-compose down`, dann das Volume sichern bzw. unter anderem Namen aufheben (`docker volume ls`; **nicht** löschen, bis alles läuft).
3. Im `docker-compose.yml` das Image auf eine aktuelle 8.0-Version setzen (z. B. `mysql:8.0`) und mit einem neuen, leeren Volume starten: `docker-compose up -d database`.
4. Dump einspielen: `docker exec -i EssenskasseDB mysql -uroot -p<DB_ROOT_PASSWORD> < essenskasse_dump.sql`, dann `docker-compose up -d --build`.
5. Funktioniert alles (Anmeldung, Salden, Buchungen), kann das alte Volume später entfernt werden.

**English:** The image `mysql:8.0.2` is an early preview version. Simply raising the tag on the existing data folder is **not supported** by MySQL for this version and may make the database unusable, so it is not changed automatically. Safer way via a dump:
1. Back up the data: `docker exec EssenskasseDB mysqldump -uroot -p<DB_ROOT_PASSWORD> --databases essenskasse --routines --single-transaction > essenskasse_dump.sql` (check that the file is not empty). In Windows PowerShell `>` creates a UTF-16 file; run it in `cmd` instead, or create the dump inside the container (`--result-file=/tmp/dump.sql`) and copy it out with `docker cp`.
2. `docker-compose down`, then back up the volume or keep it under another name (`docker volume ls`; do **not** delete it until everything works).
3. In `docker-compose.yml` set the image to a current 8.0 version (e.g. `mysql:8.0`) and start with a new, empty volume: `docker-compose up -d database`.
4. Import the dump: `docker exec -i EssenskasseDB mysql -uroot -p<DB_ROOT_PASSWORD> < essenskasse_dump.sql`, then `docker-compose up -d --build`.
5. If everything works (sign-in, balances, bookings), the old volume can be removed later.

## Contribution
Source of the icon: [www.flaticon.com/free-icon](https://www.flaticon.com/free-icon/cost_11645186?related_id=11645144&origin=search)
