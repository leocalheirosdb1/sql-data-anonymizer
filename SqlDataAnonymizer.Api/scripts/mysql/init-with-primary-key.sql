CREATE TABLE pessoas (
                         id BIGINT AUTO_INCREMENT PRIMARY KEY,
                         cpf CHAR(14) NOT NULL,
                         email VARCHAR(150) NOT NULL,
                         telefone VARCHAR(20),
                         nome VARCHAR(150) NOT NULL,
                         created_at DATETIME NOT NULL
);

-- 2º passo criar procedure
CREATE PROCEDURE popular_pessoas()
BEGIN
    DECLARE i INT DEFAULT 1;

    WHILE i <= 100000 DO
        INSERT INTO pessoas (cpf, email, telefone, nome, created_at)
        VALUES (
            LPAD(i, 11, '0'),
            CONCAT('usuario', i, '@email.com'),
            CONCAT('11', LPAD(FLOOR(RAND() * 999999999), 9, '0')),
            CONCAT('Usuario ', i),
            NOW() - INTERVAL FLOOR(RAND() * 365) DAY
        );

        SET i = i + 1;
END WHILE;
END

-- 3º passo chamar procedure
START TRANSACTION;
CALL popular_pessoas();
COMMIT;