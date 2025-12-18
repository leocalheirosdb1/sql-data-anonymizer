-- ============================================
-- SCRIPT DE TESTE: 100.000 REGISTROS
-- Banco: MySQL
-- Dados: CPF, Email, Telefone variados
-- ============================================

-- 1. Criar tabela de teste
DROP TABLE IF EXISTS TestAnonymization100k;

CREATE TABLE TestAnonymization100k (
                                       ID INT AUTO_INCREMENT PRIMARY KEY,
                                       CPF VARCHAR(14) NOT NULL,
                                       EMAIL VARCHAR(100) NOT NULL,
                                       TELEFONE VARCHAR(15) NOT NULL,
                                       NOME VARCHAR(100) NOT NULL,
                                       CREATED_AT DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- 2. Criar procedure para inserir 100k registros
DELIMITER $$

DROP PROCEDURE IF EXISTS InsertTestData100k$$

CREATE PROCEDURE InsertTestData100k()
BEGIN
    DECLARE i INT DEFAULT 1;
    DECLARE batch_size INT DEFAULT 1000;
    DECLARE sql_text TEXT;
    
    WHILE i <= 100000 DO
        SET sql_text = 'INSERT INTO TestAnonymization100k (CPF, EMAIL, TELEFONE, NOME) VALUES ';
        
        SET @j = 0;
        WHILE @j < batch_size AND i <= 100000 DO
            IF @j > 0 THEN
                SET sql_text = CONCAT(sql_text, ',');
END IF;
            
            SET sql_text = CONCAT(sql_text, '(',
                '''', LPAD(FLOOR((i % 50000) / 100), 3, '0'), '.', 
                      LPAD(FLOOR((i % 100)), 3, '0'), '.', 
                      LPAD(FLOOR((i % 1000) / 10), 3, '0'), '-',
                      LPAD((i % 100), 2, '0'), ''',',
                '''usuario', i, '@teste.com.br'',',
                '''(11) 9', LPAD((i % 30000) + 10000000, 8, '0'), ''',',
                '''Usuario Teste ', (i % 20000), '''',
                ')');
            
            SET i = i + 1;
            SET @j = @j + 1;
END WHILE;
        
        SET @sql = sql_text;
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Log progresso a cada 10k
IF i % 10000 = 0 THEN
SELECT CONCAT('Inseridos:  ', i, ' registros') AS Progresso;
END IF;
END WHILE;

SELECT 'Concluído:  100.000 registros inseridos!' AS Status;
END$$

DELIMITER ;

-- 3. Executar procedure
CALL InsertTestData100k();

-- 4. Criar índices para performance
CREATE INDEX IX_TestAnonymization100k_CPF ON TestAnonymization100k(CPF);
CREATE INDEX IX_TestAnonymization100k_EMAIL ON TestAnonymization100k(EMAIL);

-- 5. Verificar distribuição
SELECT 'Total de registros' as Metrica, COUNT(*) as Valor
FROM TestAnonymization100k

UNION ALL

SELECT 'CPFs únicos', COUNT(DISTINCT CPF)
FROM TestAnonymization100k

UNION ALL

SELECT 'Emails únicos', COUNT(DISTINCT EMAIL)
FROM TestAnonymization100k

UNION ALL

SELECT 'Telefones únicos', COUNT(DISTINCT TELEFONE)
FROM TestAnonymization100k;

-- 6. Exemplo de registros
SELECT * FROM TestAnonymization100k ORDER BY ID LIMIT 10;

-- 7. Limpar procedure
DROP PROCEDURE IF EXISTS InsertTestData100k;


-- ============================================
-- SCRIPT DE TESTE: 10.000 REGISTROS
-- Banco: MySQL
-- Dados: CPF, Email, Telefone variados
-- ============================================

-- 1. Criar tabela de teste
DROP TABLE IF EXISTS TestAnonymization10k;

CREATE TABLE TestAnonymization10k (
                                      ID INT AUTO_INCREMENT PRIMARY KEY,
                                      CPF VARCHAR(14) NOT NULL,
                                      EMAIL VARCHAR(100) NOT NULL,
                                      TELEFONE VARCHAR(15) NOT NULL,
                                      NOME VARCHAR(100) NOT NULL,
                                      CREATED_AT DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- 2. Criar procedure para inserir 10k registros
DELIMITER $$

DROP PROCEDURE IF EXISTS InsertTestData10k$$

CREATE PROCEDURE InsertTestData10k()
BEGIN
    DECLARE i INT DEFAULT 1;
    DECLARE batch_size INT DEFAULT 1000;
    DECLARE sql_text TEXT;
    
    WHILE i <= 10000 DO
        SET sql_text = 'INSERT INTO TestAnonymization10k (CPF, EMAIL, TELEFONE, NOME) VALUES ';
        
        SET @j = 0;
        WHILE @j < batch_size AND i <= 10000 DO
            IF @j > 0 THEN
                SET sql_text = CONCAT(sql_text, ',');
END IF;
            
            SET sql_text = CONCAT(sql_text, '(',
                '''', LPAD(FLOOR((i % 5000) / 100), 3, '0'), '.', 
                      LPAD(FLOOR((i % 100)), 3, '0'), '.', 
                      LPAD(FLOOR((i % 1000) / 10), 3, '0'), '-',
                      LPAD((i % 100), 2, '0'), ''',',
                '''usuario', i, '@teste.com.br'',',
                '''(11) 9', LPAD((i % 3000) + 10000000, 8, '0'), ''',',
                '''Usuario Teste ', (i % 2000), '''',
                ')');
            
            SET i = i + 1;
            SET @j = @j + 1;
END WHILE;
        
        SET @sql = sql_text;
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
END WHILE;

SELECT 'Concluído:  10.000 registros inseridos!' AS Status;
END$$

DELIMITER ;

-- 3. Executar procedure
CALL InsertTestData10k();

-- 4. Criar índices
CREATE INDEX IX_TestAnonymization10k_CPF ON TestAnonymization10k(CPF);
CREATE INDEX IX_TestAnonymization10k_EMAIL ON TestAnonymization10k(EMAIL);

-- 5. Verificar distribuição
SELECT 'Total de registros' as Metrica, COUNT(*) as Valor
FROM TestAnonymization10k

UNION ALL

SELECT 'CPFs únicos', COUNT(DISTINCT CPF)
FROM TestAnonymization10k

UNION ALL

SELECT 'Emails únicos', COUNT(DISTINCT EMAIL)
FROM TestAnonymization10k

UNION ALL

SELECT 'Telefones únicos', COUNT(DISTINCT TELEFONE)
FROM TestAnonymization10k;

-- 6. Exemplo de registros
SELECT * FROM TestAnonymization10k ORDER BY ID LIMIT 10;

-- 7. Limpar procedure
DROP PROCEDURE IF EXISTS InsertTestData10k;


-- ============================================
-- SCRIPT DE LIMPEZA (MySQL)
-- ============================================

DROP TABLE IF EXISTS TestAnonymization100k;
DROP TABLE IF EXISTS TestAnonymization10k;
DROP PROCEDURE IF EXISTS InsertTestData100k;
DROP PROCEDURE IF EXISTS InsertTestData10k;

SELECT 'Todas as tabelas de teste foram removidas!' AS Status;