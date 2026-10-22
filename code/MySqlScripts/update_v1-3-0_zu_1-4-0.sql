-- ============================================================================
-- Update von Version 1.3.x auf 1.4.0
--
-- Die Einstellung "Bestaetigungsmail bei Einzahlungen" hat jetzt drei Stufen
-- statt Ja/Nein:  0 = nie,  1 = pro Einzahlung waehlbar,  2 = immer.
-- Vorhandene Werte bleiben erhalten: true (1) wird "pro Einzahlung waehlbar",
-- false (0) wird "nie". Das Skript aendert nur den Spaltentyp (BOOLEAN ist in MySQL
-- ein TINYINT(1)). Ausserdem bekommt die Einstellungstabelle fuenf neue Spalten (Schnellbetraege,
-- Farbschema, Sprache, Sortierung von Personen und Anbietern). Das Skript kann beliebig oft ausgefuehrt werden.
--
--   1. Vorher eine Sicherung erstellen (z. B. POST api/backup/newNow oder mysqldump).
--   2. Ausfuehren, z. B. im Docker-Container:
--        docker exec -i EssenskasseDB mysql -uroot -p<DB_ROOT_PASSWORD> essenskasse < MySqlScripts/update_v1-3-0_zu_1-4-0.sql
-- ============================================================================

ALTER TABLE essenskasse.SettingsUser
    MODIFY COLUMN sendConfirmationMail TINYINT NOT NULL DEFAULT 1;

-- Neue Einstellungsfelder (Schnellbetraege, Farbschema, Sprache, Sortierung). MySQL kennt kein "ADD COLUMN IF NOT EXISTS",
-- deshalb wird jede Spalte nur angelegt, wenn sie fehlt; das Skript bleibt beliebig oft ausfuehrbar.
SET @sql = (SELECT IF(COUNT(*) = 0,
    'ALTER TABLE essenskasse.SettingsUser ADD COLUMN quickAmounts VARCHAR(255) NOT NULL DEFAULT ''''',
    'SELECT 1')
  FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = 'essenskasse' AND TABLE_NAME = 'SettingsUser' AND COLUMN_NAME = 'quickAmounts');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = (SELECT IF(COUNT(*) = 0,
    'ALTER TABLE essenskasse.SettingsUser ADD COLUMN theme VARCHAR(16) NOT NULL DEFAULT ''''',
    'SELECT 1')
  FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = 'essenskasse' AND TABLE_NAME = 'SettingsUser' AND COLUMN_NAME = 'theme');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = (SELECT IF(COUNT(*) = 0,
    'ALTER TABLE essenskasse.SettingsUser ADD COLUMN language VARCHAR(16) NOT NULL DEFAULT ''''',
    'SELECT 1')
  FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = 'essenskasse' AND TABLE_NAME = 'SettingsUser' AND COLUMN_NAME = 'language');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = (SELECT IF(COUNT(*) = 0,
    'ALTER TABLE essenskasse.SettingsUser ADD COLUMN personSort VARCHAR(32) NOT NULL DEFAULT ''''',
    'SELECT 1')
  FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = 'essenskasse' AND TABLE_NAME = 'SettingsUser' AND COLUMN_NAME = 'personSort');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = (SELECT IF(COUNT(*) = 0,
    'ALTER TABLE essenskasse.SettingsUser ADD COLUMN providerSort VARCHAR(32) NOT NULL DEFAULT ''''',
    'SELECT 1')
  FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = 'essenskasse' AND TABLE_NAME = 'SettingsUser' AND COLUMN_NAME = 'providerSort');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SELECT sendConfirmationMail AS Modus, COUNT(*) AS Anzahl
FROM essenskasse.SettingsUser
GROUP BY sendConfirmationMail;
