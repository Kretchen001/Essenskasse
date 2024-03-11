CREATE TABLE IF NOT EXISTS essenskasse.Test (
    id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    UserName VARCHAR(30) NOT NULL,
    Hobbies VARCHAR(30) NOT NULL,
    Location VARCHAR(50)
);

INSERT INTO Test (UserName, Hobbies)
VALUES
('Finn', 'Tafelwischen'),
('Albert', 'Bouling'),
('Albert', 'Tafelsfwischen'),
('Albert', 'Tafelwasischen'),
('Albert', 'Tafelwischen'),
('Albert', 'sd');

CREATE TABLE IF NOT EXISTS essenskasse.Person (
    Guid CHAR(36) PRIMARY KEY,
    Name VARCHAR(255),
    Email VARCHAR(255),
    SaldoEuro BIGINT,
    SaldoCent SMALLINT
);

-- Füge Dummy-Daten für Personen ein
INSERT INTO Person (Guid, Name, Email, SaldoEuro, SaldoCent)
VALUES
    ('e6a78252-9a4c-4c9e-aa60-2bfc9ee54b01', 'Max Mustermann', 'max@example.com', 1000, 50),
    ('8d13c2e2-9c3f-4b7d-aa3a-36a6c2e25c8a', 'Maria Musterfrau', 'maria@example.com', 500, 75);

-- Erstelle die Tabelle für Payment
CREATE TABLE IF NOT EXISTS essenskasse.Payment (
    DateTime BIGINT PRIMARY KEY,
    AmountEuro BIGINT,
    AmountCent SMALLINT,
    ReferencedPerson CHAR(36),
    Comment TEXT
);

-- Füge Dummy-Daten für Payment ein
INSERT INTO Payment (DateTime, AmountEuro, AmountCent, ReferencedPerson, Comment)
VALUES
    (1637020800000, 2000, 25, 'e6a78252-9a4c-4c9e-aa60-2bfc9ee54b01', 'Payment for services'),
    (1637107200000, 1500, 50, '8d13c2e2-9c3f-4b7d-aa3a-36a6c2e25c8a', 'Product purchase');

-- Erstelle die Tabelle für Provider
CREATE TABLE IF NOT EXISTS essenskasse.Provider (
    Guid CHAR(36) PRIMARY KEY,
    Designation VARCHAR(255)
);

-- Füge Dummy-Daten für Provider ein
INSERT INTO Provider (Guid, Designation)
VALUES
    ('0a8f5378-9e5d-4ae8-b127-4d0be89b4c56', 'Provider A'),
    ('12c70c0d-87c2-4a2d-8e56-598ed8bb13bc', 'Provider B');

-- Erstelle die Tabelle für Order
CREATE TABLE IF NOT EXISTS essenskasse.Order (
    DateTime BIGINT PRIMARY KEY,
    AmountEuro BIGINT,
    AmountCent SMALLINT,
    ProviderGuid CHAR(36),
    ReferencedPerson CHAR(36),
    FOREIGN KEY (ProviderGuid) REFERENCES Provider(Guid)
);

-- Füge Dummy-Daten für Order ein
INSERT INTO `Order` (DateTime, AmountEuro, AmountCent, ProviderGuid, ReferencedPerson)
VALUES
    (1637020800000, 2000, 25, '0a8f5378-9e5d-4ae8-b127-4d0be89b4c56', 'e6a78252-9a4c-4c9e-aa60-2bfc9ee54b01'),
    (1637107200000, 1500, 50, '12c70c0d-87c2-4a2d-8e56-598ed8bb13bc', '8d13c2e2-9c3f-4b7d-aa3a-36a6c2e25c8a'),
    (1637193600000, 1800, 75, '0a8f5378-9e5d-4ae8-b127-4d0be89b4c56', 'e6a78252-9a4c-4c9e-aa60-2bfc9ee54b01'),
    (1637280000000, 1200, 30, '12c70c0d-87c2-4a2d-8e56-598ed8bb13bc', '8d13c2e2-9c3f-4b7d-aa3a-36a6c2e25c8a');

-- Erstelle die Tabelle für TransactionProcedure
CREATE TABLE IF NOT EXISTS essenskasse.TransactionProcedure (
    DateTime BIGINT PRIMARY KEY,
    AmountEuro BIGINT,
    AmountCent SMALLINT,
    Comment TEXT
);

-- Füge Dummy-Daten für TransactionProcedure ein
INSERT INTO TransactionProcedure (DateTime, AmountEuro, AmountCent, Comment)
VALUES
    (1637020800000, 2000, 25, 'Transaction A'),
    (1637107200000, 1500, 50, 'Transaction B'),
    (1637193600000, 1800, 75, 'Transaction C'),
    (1637280000000, 1200, 30, 'Transaction D');
