# Essenskasse

[![CI-CD](https://github.com/Kretchen001/Essenskasse/actions/workflows/build-and-test.yml/badge.svg?branch=master)](https://github.com/Kretchen001/Essenskasse/actions/workflows/build-and-test.yml)
[![GitHub issues](https://img.shields.io/github/issues/Kretchen001/Essenskasse)](https://github.com/Kretchen001/Essenskasse/issues)

## Inhaltsverzeichnis / Table of Content
- [Inhaltsverzeichnis / Table of Content](#inhaltsverzeichnis--table-of-content)
- [Beschreibung Deutsch](#beschreibung-deutsch)
- [Description English](#description-english)
- [Installation and Run](#installation-and-run)
- [Update auf 1.3.0 / Upgrade to 1.3.0](#update-auf-130--upgrade-to-130)
- [Contribution](#contribution)

## Beschreibung Deutsch
In dieser Kasse können mehrere Anbieter und Personen eingespeichert werden, um die Verwaltung des Mittagsgeldes des Personals zentral zu gestalten.
Aufgrund der MySQL-Datenbank sind erweiterte statistische Auswertungen bei vorhandenen Kenntnissen zu SQL leicht möglich.

Da Einzahlungen und Bestellungen getrennt voneinander gespeichert werden ist eine separate Aufstellung und Verarbeitung möglich, was die Leistung des Kassensystems verbessert.

Sobald ein Anbieter nicht mehr genutzt werden sollte, ist dieser einfach über eine entsprechende Schaltfläche entfernbar, was ihn jedoch nur in der Datenbank als unsichtbar markiert und somit für die Auswertungen immer noch zu Verfügung steht.

## Description English
Several providers and persons can be stored in this cash register in order to centralise the administration of staff lunch money.
Due to the MySQL database, extended statistical analyses are easily possible with existing knowledge of SQL.

As deposits and orders are stored separately, separate listing and processing is possible, which improves the performance of the POS system.

As soon as a provider is no longer used, it can be easily removed via a corresponding button, which only marks it as invisible in the database and is therefore still available for analyses.

## Installation and Run
Example for the `.env` file:
```env
DB_ROOT_PASSWORD=root
DB_DATABASE=essenskasse
DB_USERNAME=u
DB_PASSWORD=p
```
API-`appsettings.json`
```json
"ConnectionStrings": {
    "essenskasse": "Server=database;Port=3306;Uid=u;Pwd=p;Database=essenskasse;SslMode=None;"
}
```
Server should be the service name of the MySql-Docker-Container.
The Uid is the User and Pwd is the password, that is set in the `.env`-file.

After editing these two files can the build be started.
For that is [Docker](https://www.docker.com/) required.

Also check please the `appsettings.json` in the code/Essenskasse/wwwroot/ folder for the URL of the API
```json
{
  "API-Adress": "http://localhost:8010/api/"
}
```

The command to start the application is the following - but first move to the code folder:
```bash
cd code
docker-compose up -d --build
```
This will start the download of the MySQL-Database, create the Container for this and then build the applications.
Because of the settings in the `docker-compose.yml` the essentiell port for calling the PWA after the docker-compose is finished is the following: (Windows-notation)
```cmd
start http://localhost:8009
```
This will call the Dashboard.

## Update auf 1.3.0 / Upgrade to 1.3.0
**Deutsch**
1. **Sicherung erstellen** (z. B. `mysqldump` oder `POST api/backup/newNow`).
2. **Datenbank migrieren** (additiv, ändert keine Daten, beliebig oft ausführbar):
   ```bash
   cd code
   docker exec -i EssenskasseDB mysql -uroot -p<DB_ROOT_PASSWORD> essenskasse < MySqlScripts/update_v1-2-0_zu_1-3-0.sql
   ```
   Die Ausgabe zeigt verwaiste Datensätze und abweichende Salden an. Ein Fremdschlüssel wird nur angelegt, wenn es keine verwaisten Datensätze gibt.
3. **Neu bauen und starten:** `docker-compose up -d --build`.

Was sich ändert:
- Die API verlangt für alle Endpunkte ein Sitzungs-Token (`Authorization: Bearer {Token}`). Die PWA sendet es automatisch nach dem Login. Ohne Anmeldung bleibt nur das Dashboard (`GET person/requestAll`) offen. Eigene Clients/Skripte müssen sich anpassen (Login: `POST api/user/newTokenForUser`).
- Passwörter werden als Hash gespeichert. Bestehende Klartext-Passwörter werden beim ersten erfolgreichen Login automatisch umgestellt.
- Den allerersten Benutzer legt man ohne Anmeldung an (`POST api/user/new` mit den Headern `name`, `userName`, `pwd`, `email`, `role`); danach dürfen das nur Administratoren.
- Buchungen (Bestellung/Einzahlung und Saldo) werden atomar gespeichert; Löschen und Ändern korrigiert den Saldo.
- Optionale Einstellungen der API (z. B. als Umgebungsvariablen im `docker-compose.yml`):

  | Einstellung | Wirkung |
  |---|---|
  | `Security__AdminMaxRole` | Benutzer mit `Role` bis zu diesem Wert sind Administratoren (Backup, Benutzerverwaltung). Standard 100 = jeder angemeldete Benutzer. |
  | `Cors__AllowedOrigins__0` | Erlaubte Adresse der PWA, z. B. `http://meinserver:8009`. Ohne Angabe sind alle Origins erlaubt. |
  | `Swagger__Enabled` | `false` schaltet Swagger ab (Standard: an, erreichbar unter `/swagger`; über „Authorize“ das Token eintragen). |

**English**
1. Create a backup. 2. Run `code/MySqlScripts/update_v1-2-0_zu_1-3-0.sql` against the database (additive: it never deletes or changes data and can be run repeatedly). 3. Rebuild with `docker-compose up -d --build`.
All API endpoints now require a session token (`Authorization: Bearer {token}`) except `GET person/requestAll` (dashboard) and the login; the PWA sends it automatically. Passwords are stored hashed and existing plaintext passwords are converted on the next successful login. The very first user can be created without a login, afterwards only administrators can create users. Settings: `Security__AdminMaxRole`, `Cors__AllowedOrigins__0`, `Swagger__Enabled`.

### Betrieb im Container / Operation in containers
- Die API im Container bekommt ihren Connection String aus dem `docker-compose.yml` (`ConnectionStrings__essenskasse`, Werte aus `.env`). Die `appsettings.json` darf lokal auf `localhost:8008` zeigen, ohne dass der Container davon beeinflusst wird.
- Der DB-Port `8008` ist nur noch auf `127.0.0.1` erreichbar (SQL-Client, lokal gestartete API). Die API startet erst, wenn die Datenbank gesund ist.
- PWA-Updates: Der Service Worker nutzt pro Build einen neuen Cache; `appsettings.json` wird nie gecacht. Nach einem Update wird die neue Version aktiv, sobald alle Tabs der PWA einmal geschlossen waren (kein hartes Neuladen mehr nötig).
- Tests: `dotnet test` (Authentifizierungs-Tests brauchen keine Datenbank). Den Coverage-Report erzeugt `code/analyze.bat` lokal; er gehört nicht ins Repository.

### MySQL aktualisieren (optional, manuell) / Upgrading MySQL (optional)
Das Image `mysql:8.0.2` ist eine frühe Vorabversion. Ein einfaches Hochsetzen des Tags auf den vorhandenen Datenordner wird von MySQL für diese Version **nicht unterstützt** und kann die Datenbank unbrauchbar machen. Deshalb wird es nicht automatisch geändert. Sicherer Weg über einen Dump:
1. Sicherung der Daten: `docker exec EssenskasseDB mysqldump -uroot -p<DB_ROOT_PASSWORD> --databases essenskasse --routines --single-transaction > essenskasse_dump.sql` (Datei prüfen, nicht leer?).
2. `docker-compose down`, dann das Volume sichern bzw. unter anderem Namen aufheben (`docker volume ls`; **nicht** löschen, bis alles läuft).
3. Im `docker-compose.yml` das Image auf eine aktuelle 8.0-Version setzen (z. B. `mysql:8.0`) und mit einem neuen, leeren Volume starten: `docker-compose up -d database`.
4. Dump einspielen: `docker exec -i EssenskasseDB mysql -uroot -p<DB_ROOT_PASSWORD> < essenskasse_dump.sql`, dann `docker-compose up -d --build`.
5. Funktioniert alles (Anmeldung, Salden, Buchungen), kann das alte Volume später entfernt werden.

## Contribution
Source of the icon: [www.flaticon.com/free-icon](https://www.flaticon.com/free-icon/cost_11645186?related_id=11645144&origin=search)
