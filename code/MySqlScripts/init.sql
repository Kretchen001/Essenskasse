CREATE TABLE essenskasse.Users (
id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
UserName VARCHAR(30) NOT NULL,
Hobbies VARCHAR(30) NOT NULL,
Location VARCHAR(50));

INSERT INTO Users (UserName, Hobbies) 
VALUES
('Finn', 'Tafelwischen'),
('Albert', 'Bouling'),
('Albert', 'Tafelsfwischen'),
('Albert', 'Tafelwasischen'),
('Albert', 'Tafelwischen'),
('Albert', 'sd');
