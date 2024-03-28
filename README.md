# Essenskasse

[![CI-CD](https://github.com/Kretchen001/Essenskasse/actions/workflows/build-and-test.yml/badge.svg?branch=master)](https://github.com/Kretchen001/Essenskasse/actions/workflows/build-and-test.yml)
[![GitHub issues](https://img.shields.io/github/issues/Kretchen001/Essenskasse)](https://github.com/Kretchen001/Essenskasse/issues)

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

After editing these two files can the build be started.
For that is [Docker](https://www.docker.com/) required.

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
