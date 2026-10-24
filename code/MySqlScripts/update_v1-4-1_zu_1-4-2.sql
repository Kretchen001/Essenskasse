-- ============================================================================
-- Update von Version 1.4.1 auf 1.4.2
--
-- Stornobuchungen: Einzahlungen und Bestellungen werden nicht mehr geloescht, sondern durch eine Gegenbuchung storniert.
--   ReversalOf = bei der Gegenbuchung der DateTime der stornierten Buchung
--   ReversedBy = bei der stornierten Buchung der DateTime der Gegenbuchung
-- Jede Spalte wird nur angelegt, wenn sie fehlt; das Skript kann beliebig oft ausgefuehrt werden.
-- Die API spielt es beim Start selbst ein.
-- ============================================================================

SET @sql = (SELECT IF(COUNT(*) = 0,
    'ALTER TABLE essenskasse.`Payment` ADD COLUMN ReversalOf BIGINT NULL',
    'SELECT 1')
  FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = 'essenskasse' AND TABLE_NAME = 'Payment' AND COLUMN_NAME = 'ReversalOf');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = (SELECT IF(COUNT(*) = 0,
    'ALTER TABLE essenskasse.`Payment` ADD COLUMN ReversedBy BIGINT NULL',
    'SELECT 1')
  FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = 'essenskasse' AND TABLE_NAME = 'Payment' AND COLUMN_NAME = 'ReversedBy');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = (SELECT IF(COUNT(*) = 0,
    'ALTER TABLE essenskasse.`Order` ADD COLUMN ReversalOf BIGINT NULL',
    'SELECT 1')
  FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = 'essenskasse' AND TABLE_NAME = 'Order' AND COLUMN_NAME = 'ReversalOf');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = (SELECT IF(COUNT(*) = 0,
    'ALTER TABLE essenskasse.`Order` ADD COLUMN ReversedBy BIGINT NULL',
    'SELECT 1')
  FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = 'essenskasse' AND TABLE_NAME = 'Order' AND COLUMN_NAME = 'ReversedBy');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
