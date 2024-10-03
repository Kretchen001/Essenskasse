-- Entferne die alte Tabelle Test, falls sie existiert
DROP TABLE IF EXISTS essenskasse.Test;

-- Entferne die alte Tabelle TokenForChange, falls sie existiert
DROP TABLE IF EXISTS essenskasse.TokenForChange;

-- Erstelle die Tabelle für Benutzer
CREATE TABLE IF NOT EXISTS essenskasse.User (
    Guid CHAR(36) PRIMARY KEY,
    Name VARCHAR(255),
    UserName VARCHAR(255),
    Pwd VARCHAR(255),
    Email VARCHAR(255),
    Role SMALLINT DEFAULT 100,
    TokenForChange CHAR(36) DEFAULT "00000000-0000-0000-0000-000000000000"
);

-- Erstelle die Tabelle für die SessionToken
CREATE TABLE IF NOT EXISTS essenskasse.SessionToken (
    Guid CHAR(36) PRIMARY KEY,
    UserGuid CHAR(36),
    FOREIGN KEY (UserGuid) REFERENCES essenskasse.User(Guid),
    ExpaireDate BIGINT
);

-- Erstelle die Tabelle für die Benutzer-Einstellungen
CREATE TABLE IF NOT EXISTS essenskasse.SettingsUser (
    UserGuid CHAR(36) PRIMARY KEY,
    isSingleListWishDashboard BOOLEAN DEFAULT false,
    loadAmountPayment INT,
    loadAmountOrder INT,
    filterShowProvider BOOLEAN DEFAULT true,
    dashboardMoneyLimit VARCHAR(255), -- ToString to encode, ToMoney to decode
    filterShowPerson BOOLEAN DEFAULT true,
    accessToken CHAR(36) DEFAULT "00000000-0000-0000-0000-000000000000",
    adminEmail VARCHAR(255),
    noticeableOnRightSide BOOLEAN DEFAULT false,
    sendConfirmationMail BOOLEAN DEFAULT true
);