IF OBJECT_ID('dbo.TestAnonymization100k_Composite', 'U') IS NOT NULL
DROP TABLE dbo.TestAnonymization100k_Composite;

CREATE TABLE dbo.TestAnonymization100k_Composite (
                                                     LOTE_ID INT NOT NULL,
                                                     REGISTRO_ID INT NOT NULL,

                                                     CPF VARCHAR(14) NOT NULL,
                                                     EMAIL VARCHAR(100) NOT NULL,
                                                     TELEFONE VARCHAR(15) NOT NULL,
                                                     NOME VARCHAR(100) NOT NULL,
                                                     CREATED_AT DATETIME DEFAULT GETDATE(),

                                                     CONSTRAINT PK_TestAnonymization100k_Composite
                                                         PRIMARY KEY (LOTE_ID, REGISTRO_ID)
);

WITH Numbers AS (
    SELECT TOP 100000
           ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS Num
    FROM sys.all_objects a
             CROSS JOIN sys.all_objects b
)

INSERT INTO dbo.TestAnonymization100k_Composite
(
    LOTE_ID,
    REGISTRO_ID,
    CPF,
    EMAIL,
    TELEFONE,
    NOME
)
SELECT
    -- 100 lotes (1 a 100)
    ((Num - 1) / 1000) + 1 AS LOTE_ID,

    -- 1.000 registros por lote
    ((Num - 1) % 1000) + 1 AS REGISTRO_ID,

    -- CPF: ~50k únicos
    FORMAT((Num % 50000) + 10000000000, '000\.000\.000\-00') AS CPF,

    -- EMAIL: único global
    'usuario' + CAST(Num AS VARCHAR(10)) + '@teste.com.br' AS EMAIL,

    -- TELEFONE: ~30k únicos
    '(11) 9' + FORMAT((Num % 30000) + 10000000, '0000\-0000') AS TELEFONE,

    -- NOME: ~20k únicos
    'Usuario Teste ' + CAST((Num % 20000) AS VARCHAR(10)) AS NOME
FROM Numbers;

CREATE INDEX IX_TestAnonymization100k_Composite_CPF
    ON dbo.TestAnonymization100k_Composite (CPF);

CREATE INDEX IX_TestAnonymization100k_Composite_EMAIL
    ON dbo.TestAnonymization100k_Composite (EMAIL);

SELECT
    'Total de registros' AS Metrica,
    COUNT(*) AS Valor
FROM dbo.TestAnonymization100k_Composite

UNION ALL
SELECT
    'Chaves compostas únicas',
    COUNT(DISTINCT CAST(LOTE_ID AS VARCHAR) + '-' + CAST(REGISTRO_ID AS VARCHAR))
FROM dbo.TestAnonymization100k_Composite

UNION ALL
SELECT
    'CPFs únicos',
    COUNT(DISTINCT CPF)
FROM dbo.TestAnonymization100k_Composite

UNION ALL
SELECT
    'Emails únicos',
    COUNT(DISTINCT EMAIL)
FROM dbo.TestAnonymization100k_Composite;

SELECT TOP 10 *
FROM dbo.TestAnonymization100k_Composite
ORDER BY LOTE_ID, REGISTRO_ID;
