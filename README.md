# Essenskasse

## Inhaltsverzeichnis / Table of Content
- [Inhaltsverzeichnis / Table of Content](#inhaltsverzeichnis--table-of-content)
- [Beschreibung Deutsch](#beschreibung-deutsch)
- [Description English](#description-english)
- [Installation and Run](#installation-and-run)

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
