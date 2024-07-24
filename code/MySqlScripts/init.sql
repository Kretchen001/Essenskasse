CREATE TABLE IF NOT EXISTS essenskasse.Test (
    id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    UserName VARCHAR(30) NOT NULL,
    Hobbies VARCHAR(30) NOT NULL,
    Location VARCHAR(50)
);

INSERT INTO Test (UserName, Hobbies, Location)
VALUES
('Finn', 'Tafelwischen', 'Mond'),
('Albert', 'Bouling', 'Erde'),
('Odin', 'Wachen', 'Asgard'),
('Rah', 'Scheinen', 'Sonne'),
('Poseidon', 'Schwimmen', 'Olymp'),
('Kain', 'sd', 'Eden');

CREATE TABLE IF NOT EXISTS essenskasse.Person (
    Guid CHAR(36) PRIMARY KEY,
    Name VARCHAR(511),
    Email VARCHAR(511),
    SaldoEuro BIGINT,
    SaldoCent SMALLINT,
    Sichtbar BOOLEAN DEFAULT true
);

-- Erstelle die Tabelle für Payment
CREATE TABLE IF NOT EXISTS essenskasse.Payment (
    DateTime BIGINT PRIMARY KEY,
    AmountEuro BIGINT,
    AmountCent SMALLINT,
    ReferencedPerson CHAR(36),
    Comment TEXT
);

-- Erstelle die Tabelle für Provider
CREATE TABLE IF NOT EXISTS essenskasse.Provider (
    Guid CHAR(36) PRIMARY KEY,
    Designation VARCHAR(255),
    Sichtbar BOOLEAN DEFAULT true
);

-- Erstelle die Tabelle für Order
CREATE TABLE IF NOT EXISTS essenskasse.Order (
    DateTime BIGINT PRIMARY KEY,
    AmountEuro BIGINT,
    AmountCent SMALLINT,
    ProviderGuid CHAR(36),
    ProviderDesignation VARCHAR(255),
    ReferencedPerson CHAR(36)
);

-- Erstelle die Tabelle für TransactionProcedure
CREATE TABLE IF NOT EXISTS essenskasse.TransactionProcedure (
    DateTime BIGINT PRIMARY KEY,
    AmountEuro BIGINT,
    AmountCent SMALLINT,
    Comment TEXT
);

-- Erstelle die Tabelle für TransactionProcedure
CREATE TABLE IF NOT EXISTS essenskasse.TokenForChange (
    DateTime BIGINT PRIMARY KEY,
    Guid CHAR(36),
    Identity VARCHAR(255)
);

-- Erstelle die Tabelle für Benutzer
CREATE TABLE IF NOT EXISTS essenskasse.User (
    Guid CHAR(36) PRIMARY KEY,
    Name VARCHAR(255),
    UserName VARCHAR(255),
    Pwd VARCHAR(255),
    Email VARCHAR(255),
    Role SMALLINT
);

-- Erstelle die Tabelle für die SessionToken
CREATE TABLE IF NOT EXISTS essenskasse.SessionToken (
    Guid CHAR(36) PRIMARY KEY,
    UserGuid CHAR(36),
    FOREIGN KEY (UserGuid) REFERENCES essenskasse.User(Guid),
    ExpaireDate BIGINT
);
