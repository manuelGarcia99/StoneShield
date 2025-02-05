create database StoneShield;

create user manel;

use StoneShield;

grant all privileges ON StoneShield.* TO manel;

ALTER USER manel IDENTIFIED BY 'password2025#';



DROP TABLE REFS;

CREATE TABLE REFS(
    ID INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci UNIQUE,
    Velocidade DOUBLE,
    Cut BOOL,
    Batch INT,
    Clean BOOL,
    Image VARCHAR(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
    CHECK (Velocidade >= 0.01 AND Velocidade <= 1.00 AND Batch >= 1 AND Batch <= 100)
);

-- Insert a valid record into REFS
INSERT INTO REFS(Nome, Velocidade, Cut, Batch, Clean, Image)  
VALUES ("garra", 0.01, 1, 1, 1, "C:/Users/manec/OneDrive/Ambiente de Trabalho/garra.webp");

INSERT INTO REFS(Nome, Velocidade, Cut, Batch, Clean, Image)  
VALUES ("serrote", 1, 1, 1, 1, "C:/Users/manec/OneDrive/Ambiente de Trabalho/serrote.webp");

truncate REFS;

