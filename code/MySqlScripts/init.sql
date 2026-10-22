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
    Comment TEXT,
    INDEX idx_Payment_ReferencedPerson (ReferencedPerson),
    CONSTRAINT fk_Payment_Person FOREIGN KEY (ReferencedPerson) REFERENCES essenskasse.Person (Guid)
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
    ReferencedPerson CHAR(36),
    INDEX idx_Order_ReferencedPerson (ReferencedPerson),
    INDEX idx_Order_ProviderGuid (ProviderGuid),
    CONSTRAINT fk_Order_Person FOREIGN KEY (ReferencedPerson) REFERENCES essenskasse.Person (Guid),
    CONSTRAINT fk_Order_Provider FOREIGN KEY (ProviderGuid) REFERENCES essenskasse.Provider (Guid)
);

-- Erstelle die Tabelle für TransactionProcedure
CREATE TABLE IF NOT EXISTS essenskasse.TransactionProcedure (
    DateTime BIGINT PRIMARY KEY,
    AmountEuro BIGINT,
    AmountCent SMALLINT,
    Comment TEXT
);

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

-- Erstelle die Tabelle für die SessionToken
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
