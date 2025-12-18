-- ============================================
-- SCRIPT DE TESTE: 100.000 REGISTROS
-- Banco: SQL Server
-- Dados: CPF, Email, Telefone variados
-- ============================================

-- 1. Criar tabela de teste
IF OBJECT_ID('dbo. TestAnonymization100k', 'U') IS NOT NULL
DROP TABLE dbo. TestAnonymization100k;

CREATE TABLE dbo.TestAnonymization100k (
                                           ID INT IDENTITY(1,1) PRIMARY KEY,
                                           CPF VARCHAR(14) NOT NULL,
                                           EMAIL VARCHAR(100) NOT NULL,
                                           TELEFONE VARCHAR(15) NOT NULL,
                                           NOME VARCHAR(100) NOT NULL,
                                           CREATED_AT DATETIME DEFAULT GETDATE()
);

-- 2. Criar tabela auxiliar de números (para gerar 100k registros)
WITH Numbers AS (
    SELECT TOP 100000 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) as Num
    FROM sys.all_objects a
             CROSS JOIN sys. all_objects b
)
-- 3. Inserir 100k registros com dados variados
INSERT INTO dbo. TestAnonymization100k (CPF, EMAIL, TELEFONE, NOME)
SELECT
    -- CPF:  Gera ~50k CPFs únicos (alguns duplicados para teste de consistência)
    FORMAT((Num % 50000) + 10000000000, '000\.000\.000\-00') as CPF,

    -- EMAIL: Único por registro
    'usuario' + CAST(Num as VARCHAR(10)) + '@teste.com.br' as EMAIL,

    -- TELEFONE: ~30k únicos (mais duplicatas)
    '(11) 9' + FORMAT((Num % 30000) + 10000000, '0000\-0000') as TELEFONE,

    -- NOME: ~20k nomes únicos
    'Usuario Teste ' + CAST((Num % 20000) as VARCHAR(10)) as NOME
FROM Numbers;

-- 4. Criar índices para performance
CREATE INDEX IX_TestAnonymization100k_CPF ON dbo.TestAnonymization100k(CPF);
CREATE INDEX IX_TestAnonymization100k_EMAIL ON dbo.TestAnonymization100k(EMAIL);

-- 5. Verificar distribuição
SELECT
    'Total de registros' as Metrica,
    COUNT(*) as Valor
FROM dbo.TestAnonymization100k

UNION ALL

SELECT
    'CPFs únicos',
    COUNT(DISTINCT CPF)
FROM dbo.TestAnonymization100k

UNION ALL

SELECT
    'Emails únicos',
    COUNT(DISTINCT EMAIL)
FROM dbo.TestAnonymization100k

UNION ALL

SELECT
    'Telefones únicos',
    COUNT(DISTINCT TELEFONE)
FROM dbo.TestAnonymization100k;

-- 6. Exemplo de registros
SELECT TOP 10 * FROM dbo. TestAnonymization100k ORDER BY ID;

-- ============================================
-- SCRIPT DE TESTE: 10.000 REGISTROS
-- Banco:  SQL Server
-- Dados: CPF, Email, Telefone variados
-- ============================================

-- 1. Criar tabela de teste
IF OBJECT_ID('dbo.TestAnonymization10k', 'U') IS NOT NULL
DROP TABLE dbo. TestAnonymization10k;

CREATE TABLE dbo.TestAnonymization10k (
                                          ID INT IDENTITY(1,1) PRIMARY KEY,
                                          CPF VARCHAR(14) NOT NULL,
                                          EMAIL VARCHAR(100) NOT NULL,
                                          TELEFONE VARCHAR(15) NOT NULL,
                                          NOME VARCHAR(100) NOT NULL,
                                          CREATED_AT DATETIME DEFAULT GETDATE()
);

-- 2. Criar tabela auxiliar de números
WITH Numbers AS (
    SELECT TOP 10000 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) as Num
    FROM sys.all_objects a
             CROSS JOIN sys.all_objects b
)
-- 3. Inserir 10k registros
INSERT INTO dbo.TestAnonymization10k (CPF, EMAIL, TELEFONE, NOME)
SELECT
    -- CPF: ~5k únicos
    FORMAT((Num % 5000) + 10000000000, '000\.000\.000\-00') as CPF,

    -- EMAIL: Único por registro
    'usuario' + CAST(Num as VARCHAR(10)) + '@teste.com.br' as EMAIL,

    -- TELEFONE: ~3k únicos
    '(11) 9' + FORMAT((Num % 3000) + 10000000, '0000\-0000') as TELEFONE,

    -- NOME: ~2k únicos
    'Usuario Teste ' + CAST((Num % 2000) as VARCHAR(10)) as NOME
FROM Numbers;

-- 4. Criar índices
CREATE INDEX IX_TestAnonymization10k_CPF ON dbo.TestAnonymization10k(CPF);
CREATE INDEX IX_TestAnonymization10k_EMAIL ON dbo.TestAnonymization10k(EMAIL);

-- 5. Verificar distribuição
SELECT
    'Total de registros' as Metrica,
    COUNT(*) as Valor
FROM dbo.TestAnonymization10k

UNION ALL

SELECT
    'CPFs únicos',
    COUNT(DISTINCT CPF)
FROM dbo.TestAnonymization10k

UNION ALL

SELECT
    'Emails únicos',
    COUNT(DISTINCT EMAIL)
FROM dbo.TestAnonymization10k

UNION ALL

SELECT
    'Telefones únicos',
    COUNT(DISTINCT TELEFONE)
FROM dbo.TestAnonymization10k;

-- 6. Exemplo de registros
SELECT TOP 10 * FROM dbo.TestAnonymization10k ORDER BY ID;