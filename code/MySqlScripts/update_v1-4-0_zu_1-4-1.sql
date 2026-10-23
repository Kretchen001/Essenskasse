-- ============================================================================
-- Update von Version 1.4.0 auf 1.4.1
--
-- Die Waehrung der Anzeige ist jetzt eine Einstellung (z. B. EUR, USD, CHF). Dafuer bekommt die Einstellungstabelle
-- die Spalte currency. Gespeicherte Betraege bleiben unveraendert (es wird nichts umgerechnet).
-- MySQL kennt kein "ADD COLUMN IF NOT EXISTS", deshalb wird die Spalte nur angelegt, wenn sie fehlt;
-- das Skript kann beliebig oft ausgefuehrt werden. Die API spielt es beim Start selbst ein.
-- ============================================================================

SET @sql = (SELECT IF(COUNT(*) = 0,
    'ALTER TABLE essenskasse.SettingsUser ADD COLUMN currency VARCHAR(3) NOT NULL DEFAULT \'\'',
    'SELECT 1')
  FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = 'essenskasse' AND TABLE_NAME = 'SettingsUser' AND COLUMN_NAME = 'currency');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
