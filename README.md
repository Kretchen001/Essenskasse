# Essenskasse

## Inhaltsverzeichnis / Table of Content
- [Inhaltsverzeichnis / Table of Content](#inhaltsverzeichnis--table-of-content)
- [Beschreibung Deutsch](#beschreibung-deutsch)
- [Description English](#description-english)
- [Installation and Run](#installation-and-run)
- [Usage hints](#usage-hints)

## Beschreibung Deutsch

## Description English

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

## Usage hints
