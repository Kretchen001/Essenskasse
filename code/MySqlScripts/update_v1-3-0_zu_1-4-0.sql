-- ============================================================================
-- Update von Version 1.3.x auf 1.4.0
--
-- Die Einstellung "Bestaetigungsmail bei Einzahlungen" hat jetzt drei Stufen
-- statt Ja/Nein:  0 = nie,  1 = pro Einzahlung waehlbar,  2 = immer.
-- Vorhandene Werte bleiben erhalten: true (1) wird "pro Einzahlung waehlbar",
-- false (0) wird "nie". Das Skript aendert nur den Spaltentyp (BOOLEAN ist in MySQL
-- ein TINYINT(1)) und kann beliebig oft ausgefuehrt werden.
--
--   1. Vorher eine Sicherung erstellen (z. B. POST api/backup/newNow oder mysqldump).
--   2. Ausfuehren, z. B. im Docker-Container:
--        docker exec -i EssenskasseDB mysql -uroot -p<DB_ROOT_PASSWORD> essenskasse < MySqlScripts/update_v1-3-0_zu_1-4-0.sql
-- ============================================================================

ALTER TABLE essenskasse.SettingsUser
    MODIFY COLUMN sendConfirmationMail TINYINT NOT NULL DEFAULT 1;

SELECT sendConfirmationMail AS Modus, COUNT(*) AS Anzahl
FROM essenskasse.SettingsUser
GROUP BY sendConfirmationMail;
