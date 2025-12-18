DROP TABLE IF EXISTS TestAnonymization100k_Composite;

CREATE TABLE TestAnonymization100k_Composite (
                                                 lote_id INT NOT NULL,
                                                 registro_id INT NOT NULL,

                                                 cpf VARCHAR(14) NOT NULL,
                                                 email VARCHAR(100) NOT NULL,
                                                 telefone VARCHAR(15) NOT NULL,
                                                 nome VARCHAR(100) NOT NULL,
                                                 created_at DATETIME DEFAULT CURRENT_TIMESTAMP,

                                                 PRIMARY KEY (lote_id, registro_id)
);

INSERT INTO TestAnonymization100k_Composite
(
    lote_id,
    registro_id,
    cpf,
    email,
    telefone,
    nome
)
SELECT
    -- 100 lotes
    FLOOR((n - 1) / 1000) + 1 AS lote_id,

    -- 1.000 registros por lote
    ((n - 1) % 1000) + 1 AS registro_id,

    -- CPF: ~50k únicos
    LPAD((n % 50000) + 10000000000, 11, '0') AS cpf,

    -- EMAIL: único
    CONCAT('usuario', n, '@teste.com.br') AS email,

    -- TELEFONE: ~30k únicos
    CONCAT('(11) 9', LPAD((n % 30000) + 10000000, 8, '0')) AS telefone,

    -- NOME: ~20k únicos
    CONCAT('Usuario Teste ', (n % 20000)) AS nome
FROM (
    SELECT
    a.n + b.n * 10 + c.n * 100 + d.n * 1000 + e.n * 10000 + 1 AS n
    FROM
    (SELECT 0 n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4
    UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) a
    CROSS JOIN
    (SELECT 0 n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4
    UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) b
    CROSS JOIN
    (SELECT 0 n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4
    UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) c
    CROSS JOIN
    (SELECT 0 n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4
    UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) d
    CROSS JOIN
    (SELECT 0 n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4
    UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) e
    ) nums
WHERE n <= 100000;

CREATE INDEX IX_TestAnonymization100k_Composite_CPF
    ON TestAnonymization100k_Composite (cpf);

CREATE INDEX IX_TestAnonymization100k_Composite_EMAIL
    ON TestAnonymization100k_Composite (email);

SELECT
    'Total de registros' AS Metrica,
    COUNT(*) AS Valor
FROM TestAnonymization100k_Composite

UNION ALL
SELECT
    'Chaves compostas únicas',
    COUNT(DISTINCT CONCAT(lote_id, '-', registro_id))
FROM TestAnonymization100k_Composite;
