-- ============================================================================
-- Update von Version 1.2.x auf 1.3.0
--
-- Dieses Skript ist ADDITIV: es loescht, aendert oder verschiebt keine Daten.
-- Es legt nur Indizes und Fremdschluessel an und kann beliebig oft ausgefuehrt werden.
--
--   1. Vorher eine Sicherung erstellen (z. B. POST api/backup/newNow oder mysqldump).
--   2. Ausfuehren, z. B. im Docker-Container:
--        docker exec -i EssenskasseDB mysql -uroot -p<DB_ROOT_PASSWORD> essenskasse < MySqlScripts/update_v1-2-0_zu_1-3-0.sql
--   3. Die Ausgabe lesen: Teil 1 zeigt Auffaelligkeiten in den Daten, Teil 3 meldet je
--      Fremdschluessel, ob er angelegt wurde.
--
-- Ein Fremdschluessel wird NUR angelegt, wenn es keine verwaisten Datensaetze gibt
-- (z. B. eine Einzahlung, deren Person nicht mehr existiert). Gibt es welche, bleibt die
-- Datenbank unveraendert und Teil 1 zeigt, wie viele es sind.
-- ============================================================================

-- ---------------------------------------------------------------------------
-- Teil 1: Pruefung (nur Anzeige, aendert nichts)
-- ---------------------------------------------------------------------------

-- Verwaiste Datensaetze: sie verhindern den jeweiligen Fremdschluessel
SELECT 'Einzahlungen ohne Person' AS Pruefung, COUNT(*) AS Anzahl
FROM essenskasse.Payment p
LEFT JOIN essenskasse.Person x ON x.Guid = p.ReferencedPerson
WHERE x.Guid IS NULL
UNION ALL
SELECT 'Bestellungen ohne Person', COUNT(*)
FROM essenskasse.`Order` o
LEFT JOIN essenskasse.Person x ON x.Guid = o.ReferencedPerson
WHERE x.Guid IS NULL
UNION ALL
SELECT 'Bestellungen ohne Anbieter', COUNT(*)
FROM essenskasse.`Order` o
LEFT JOIN essenskasse.Provider v ON v.Guid = o.ProviderGuid
WHERE v.Guid IS NULL;

-- Personen, deren gespeicherter Saldo nicht zu Einzahlungen minus Bestellungen passt.
-- (Ursache koennen frueher geloeschte oder geaenderte Buchungen sein. In der PWA gibt es dafuer
-- auf dem Dashboard "Salden neu berechnen"; hier wird nichts veraendert.)
SELECT x.Name,
       (x.SaldoEuro * 100 + x.SaldoCent) / 100 AS GespeicherterSaldo,
       (COALESCE(pay.Summe, 0) - COALESCE(ord.Summe, 0)) / 100 AS BerechneterSaldo
FROM essenskasse.Person x
LEFT JOIN (SELECT ReferencedPerson, SUM(AmountEuro * 100 + AmountCent) AS Summe
           FROM essenskasse.Payment GROUP BY ReferencedPerson) pay ON pay.ReferencedPerson = x.Guid
LEFT JOIN (SELECT ReferencedPerson, SUM(AmountEuro * 100 + AmountCent) AS Summe
           FROM essenskasse.`Order` GROUP BY ReferencedPerson) ord ON ord.ReferencedPerson = x.Guid
WHERE (x.SaldoEuro * 100 + x.SaldoCent) <> (COALESCE(pay.Summe, 0) - COALESCE(ord.Summe, 0));

-- ---------------------------------------------------------------------------
-- Teil 2: Indizes fuer die Abfragen "Buchungen einer Person" / "Bestellungen eines Anbieters"
-- ---------------------------------------------------------------------------

SET @sql = IF(EXISTS(SELECT 1 FROM information_schema.statistics
                     WHERE table_schema = 'essenskasse' AND table_name = 'Payment' AND index_name = 'idx_Payment_ReferencedPerson'),
              'SELECT ''idx_Payment_ReferencedPerson bereits vorhanden'' AS Hinweis',
              'CREATE INDEX idx_Payment_ReferencedPerson ON essenskasse.Payment (ReferencedPerson)');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(EXISTS(SELECT 1 FROM information_schema.statistics
                     WHERE table_schema = 'essenskasse' AND table_name = 'Order' AND index_name = 'idx_Order_ReferencedPerson'),
              'SELECT ''idx_Order_ReferencedPerson bereits vorhanden'' AS Hinweis',
              'CREATE INDEX idx_Order_ReferencedPerson ON essenskasse.`Order` (ReferencedPerson)');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(EXISTS(SELECT 1 FROM information_schema.statistics
                     WHERE table_schema = 'essenskasse' AND table_name = 'Order' AND index_name = 'idx_Order_ProviderGuid'),
              'SELECT ''idx_Order_ProviderGuid bereits vorhanden'' AS Hinweis',
              'CREATE INDEX idx_Order_ProviderGuid ON essenskasse.`Order` (ProviderGuid)');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ---------------------------------------------------------------------------
-- Teil 3: Fremdschluessel (nur wenn noch nicht vorhanden und ohne verwaiste Datensaetze)
-- ---------------------------------------------------------------------------

-- Payment.ReferencedPerson -> Person.Guid
SET @orphans = (SELECT COUNT(*) FROM essenskasse.Payment p
                LEFT JOIN essenskasse.Person x ON x.Guid = p.ReferencedPerson WHERE x.Guid IS NULL);
SET @sql = IF(EXISTS(SELECT 1 FROM information_schema.table_constraints
                     WHERE constraint_schema = 'essenskasse' AND table_name = 'Payment'
                       AND constraint_name = 'fk_Payment_Person' AND constraint_type = 'FOREIGN KEY'),
              'SELECT ''fk_Payment_Person bereits vorhanden'' AS Hinweis',
              IF(@orphans > 0,
                 'SELECT ''fk_Payment_Person NICHT angelegt: es gibt Einzahlungen ohne Person (siehe Teil 1)'' AS Hinweis',
                 'ALTER TABLE essenskasse.Payment ADD CONSTRAINT fk_Payment_Person FOREIGN KEY (ReferencedPerson) REFERENCES essenskasse.Person (Guid)'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Order.ReferencedPerson -> Person.Guid
SET @orphans = (SELECT COUNT(*) FROM essenskasse.`Order` o
                LEFT JOIN essenskasse.Person x ON x.Guid = o.ReferencedPerson WHERE x.Guid IS NULL);
SET @sql = IF(EXISTS(SELECT 1 FROM information_schema.table_constraints
                     WHERE constraint_schema = 'essenskasse' AND table_name = 'Order'
                       AND constraint_name = 'fk_Order_Person' AND constraint_type = 'FOREIGN KEY'),
              'SELECT ''fk_Order_Person bereits vorhanden'' AS Hinweis',
              IF(@orphans > 0,
                 'SELECT ''fk_Order_Person NICHT angelegt: es gibt Bestellungen ohne Person (siehe Teil 1)'' AS Hinweis',
                 'ALTER TABLE essenskasse.`Order` ADD CONSTRAINT fk_Order_Person FOREIGN KEY (ReferencedPerson) REFERENCES essenskasse.Person (Guid)'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Order.ProviderGuid -> Provider.Guid
SET @orphans = (SELECT COUNT(*) FROM essenskasse.`Order` o
                LEFT JOIN essenskasse.Provider v ON v.Guid = o.ProviderGuid WHERE v.Guid IS NULL);
SET @sql = IF(EXISTS(SELECT 1 FROM information_schema.table_constraints
                     WHERE constraint_schema = 'essenskasse' AND table_name = 'Order'
                       AND constraint_name = 'fk_Order_Provider' AND constraint_type = 'FOREIGN KEY'),
              'SELECT ''fk_Order_Provider bereits vorhanden'' AS Hinweis',
              IF(@orphans > 0,
                 'SELECT ''fk_Order_Provider NICHT angelegt: es gibt Bestellungen ohne Anbieter (siehe Teil 1)'' AS Hinweis',
                 'ALTER TABLE essenskasse.`Order` ADD CONSTRAINT fk_Order_Provider FOREIGN KEY (ProviderGuid) REFERENCES essenskasse.Provider (Guid)'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
