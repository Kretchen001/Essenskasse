-- Füge Dummy-Daten für Personen ein
INSERT INTO Person (Guid, Name, Email, SaldoEuro, SaldoCent)
VALUES
    ('e6a78252-9a4c-4c9e-aa60-2bfc9ee54b01', 'Max Mustermann', 'max@example.com', 100, 50),
    ('8d13c2e2-9c3f-4b7d-aa3a-36a6c2e25c8a', 'Maria Musterfrau', 'maria@example.com', 50, 0);

-- Füge Dummy-Daten für Payment ein
INSERT INTO Payment (DateTime, AmountEuro, AmountCent, ReferencedPerson, Comment)
VALUES
    (1637020800000, 200, 50, 'e6a78252-9a4c-4c9e-aa60-2bfc9ee54b01', 'Payment for services'),
    (1637021800000, -100, 0, 'e6a78252-9a4c-4c9e-aa60-2bfc9ee54b01', 'Auszahlung'),
    (1637107200000, 50, 50, '8d13c2e2-9c3f-4b7d-aa3a-36a6c2e25c8a', 'Product purchase');

-- Füge Dummy-Daten für Provider ein
INSERT INTO Provider (Guid, Designation)
VALUES
    ('0a8f5378-9e5d-4ae8-b127-4d0be89b4c56', 'Provider A'),
    ('12c70c0d-87c2-4a2d-8e56-598ed8bb13bc', 'Provider B');

-- Füge Dummy-Daten für Order ein
INSERT INTO `Order` (DateTime, AmountEuro, AmountCent, ProviderGuid, ProviderDesignation, ReferencedPerson)
VALUES
    (1637020800000, 20, 25, '0a8f5378-9e5d-4ae8-b127-4d0be89b4c56', 'Provider A', 'e6a78252-9a4c-4c9e-aa60-2bfc9ee54b01'),
    (1637107200000, 150, 50, '12c70c0d-87c2-4a2d-8e56-598ed8bb13bc', 'Provider B', '8d13c2e2-9c3f-4b7d-aa3a-36a6c2e25c8a'),
    (1637193600000, 80, 75, '0a8f5378-9e5d-4ae8-b127-4d0be89b4c56', 'Provider A', 'e6a78252-9a4c-4c9e-aa60-2bfc9ee54b01'),
    (1637280000000, 120, 30, '12c70c0d-87c2-4a2d-8e56-598ed8bb13bc', 'Provider B', '8d13c2e2-9c3f-4b7d-aa3a-36a6c2e25c8a');

-- Füge Dummy-Daten für TransactionProcedure ein
INSERT INTO TransactionProcedure (DateTime, AmountEuro, AmountCent, Comment)
VALUES
    (1637020800000, 2000, 25, 'Transaction A'),
    (1637107200000, 10, 50, ''),
    (1637193600000, 800, 75, ''),
    (1637280000000, -1200, 30, 'Jacobs Kaffee');
