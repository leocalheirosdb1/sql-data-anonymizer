-- ============================================
-- SCRIPT DE TESTE:  100.000 REGISTROS
-- Banco: Oracle
-- Dados: CPF, Email, Telefone variados
-- ============================================

-- 1. Criar tabela de teste
BEGIN
EXECUTE IMMEDIATE 'DROP TABLE TestAnonymization100k';
EXCEPTION
    WHEN OTHERS THEN NULL;
END;
/

CREATE TABLE TestAnonymization100k (
                                       ID NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                                       CPF VARCHAR2(14) NOT NULL,
                                       EMAIL VARCHAR2(100) NOT NULL,
                                       TELEFONE VARCHAR2(15) NOT NULL,
                                       NOME VARCHAR2(100) NOT NULL,
                                       CREATED_AT TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 2. Criar procedure para inserir 100k registros
CREATE OR REPLACE PROCEDURE InsertTestData100k AS
    v_batch_size CONSTANT NUMBER := 500;
    v_total CONSTANT NUMBER := 100000;
    v_counter NUMBER := 1;
BEGIN
    WHILE v_counter <= v_total LOOP
        
        -- Inserir em batches de 500 usando INSERT ALL
        DECLARE
v_sql CLOB := 'INSERT ALL ';
            v_end_batch NUMBER := LEAST(v_counter + v_batch_size - 1, v_total);
BEGIN
FOR i IN v_counter..v_end_batch LOOP
                v_sql := v_sql || 
                    'INTO TestAnonymization100k (CPF, EMAIL, TELEFONE, NOME) VALUES (' ||
                    '''' || 
                    LPAD(TO_CHAR(MOD(i, 50000) + 100), 3, '0') || '.' ||
                    LPAD(TO_CHAR(MOD(i, 900) + 100), 3, '0') || '.' ||
                    LPAD(TO_CHAR(MOD(i, 900) + 100), 3, '0') || '-' ||
                    LPAD(TO_CHAR(MOD(i, 90) + 10), 2, '0') || 
                    ''', ' ||
                    '''usuario' || i || '@teste.com. br'', ' ||
                    '''(11) 9' || LPAD(TO_CHAR(MOD(i, 30000) + 10000000), 8, '0') || ''', ' ||
                    '''Usuario Teste ' || TO_CHAR(MOD(i, 20000)) || '''' ||
                    ') ';
END LOOP;
            
            v_sql := v_sql || 'SELECT 1 FROM DUAL';

EXECUTE IMMEDIATE v_sql;
COMMIT;

-- Log progresso a cada 10k
IF MOD(v_end_batch, 10000) = 0 THEN
                DBMS_OUTPUT.PUT_LINE('Inseridos: ' || v_end_batch || ' registros');
END IF;
            
            v_counter := v_end_batch + 1;
END;
END LOOP;
    
    DBMS_OUTPUT.PUT_LINE('Concluído: 100.000 registros inseridos!');
END;
/

-- 3. Executar procedure
SET SERVEROUTPUT ON;
EXEC InsertTestData100k;

-- 4. Criar índices para performance
CREATE INDEX IX_TestAnon100k_CPF ON TestAnonymization100k(CPF);
CREATE INDEX IX_TestAnon100k_EMAIL ON TestAnonymization100k(EMAIL);

-- 5. Verificar distribuição
SELECT 'Total de registros' as Metrica, TO_CHAR(COUNT(*)) as Valor
FROM TestAnonymization100k

UNION ALL

SELECT 'CPFs únicos', TO_CHAR(COUNT(DISTINCT CPF))
FROM TestAnonymization100k

UNION ALL

SELECT 'Emails únicos', TO_CHAR(COUNT(DISTINCT EMAIL))
FROM TestAnonymization100k

UNION ALL

SELECT 'Telefones únicos', TO_CHAR(COUNT(DISTINCT TELEFONE))
FROM TestAnonymization100k;

-- 6. Exemplo de registros
SELECT * FROM TestAnonymization100k WHERE ROWNUM <= 10 ORDER BY ID;

-- 7. Limpar procedure
DROP PROCEDURE InsertTestData100k;


-- ============================================
-- SCRIPT DE TESTE: 10.000 REGISTROS
-- Banco: Oracle
-- Dados: CPF, Email, Telefone variados
-- ============================================

-- 1. Criar tabela de teste
BEGIN
EXECUTE IMMEDIATE 'DROP TABLE TestAnonymization10k';
EXCEPTION
    WHEN OTHERS THEN NULL;
END;
/

CREATE TABLE TestAnonymization10k (
                                      ID NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                                      CPF VARCHAR2(14) NOT NULL,
                                      EMAIL VARCHAR2(100) NOT NULL,
                                      TELEFONE VARCHAR2(15) NOT NULL,
                                      NOME VARCHAR2(100) NOT NULL,
                                      CREATED_AT TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 2. Criar procedure para inserir 10k registros
CREATE OR REPLACE PROCEDURE InsertTestData10k AS
    v_batch_size CONSTANT NUMBER := 500;
    v_total CONSTANT NUMBER := 10000;
    v_counter NUMBER := 1;
BEGIN
    WHILE v_counter <= v_total LOOP
        
        DECLARE
v_sql CLOB := 'INSERT ALL ';
            v_end_batch NUMBER := LEAST(v_counter + v_batch_size - 1, v_total);
BEGIN
FOR i IN v_counter..v_end_batch LOOP
                v_sql := v_sql || 
                    'INTO TestAnonymization10k (CPF, EMAIL, TELEFONE, NOME) VALUES (' ||
                    '''' || 
                    LPAD(TO_CHAR(MOD(i, 5000) + 100), 3, '0') || '.' ||
                    LPAD(TO_CHAR(MOD(i, 900) + 100), 3, '0') || '.' ||
                    LPAD(TO_CHAR(MOD(i, 900) + 100), 3, '0') || '-' ||
                    LPAD(TO_CHAR(MOD(i, 90) + 10), 2, '0') || 
                    ''', ' ||
                    '''usuario' || i || '@teste.com.br'', ' ||
                    '''(11) 9' || LPAD(TO_CHAR(MOD(i, 3000) + 10000000), 8, '0') || ''', ' ||
                    '''Usuario Teste ' || TO_CHAR(MOD(i, 2000)) || '''' ||
                    ') ';
END LOOP;
            
            v_sql := v_sql || 'SELECT 1 FROM DUAL';

EXECUTE IMMEDIATE v_sql;
COMMIT;

v_counter := v_end_batch + 1;
END;
END LOOP;
    
    DBMS_OUTPUT. PUT_LINE('Concluído: 10.000 registros inseridos!');
END;
/

-- 3. Executar procedure
SET SERVEROUTPUT ON;
EXEC InsertTestData10k;

-- 4. Criar índices
CREATE INDEX IX_TestAnon10k_CPF ON TestAnonymization10k(CPF);
CREATE INDEX IX_TestAnon10k_EMAIL ON TestAnonymization10k(EMAIL);

-- 5. Verificar distribuição
SELECT 'Total de registros' as Metrica, TO_CHAR(COUNT(*)) as Valor
FROM TestAnonymization10k

UNION ALL

SELECT 'CPFs únicos', TO_CHAR(COUNT(DISTINCT CPF))
FROM TestAnonymization10k

UNION ALL

SELECT 'Emails únicos', TO_CHAR(COUNT(DISTINCT EMAIL))
FROM TestAnonymization10k

UNION ALL

SELECT 'Telefones únicos', TO_CHAR(COUNT(DISTINCT TELEFONE))
FROM TestAnonymization10k;

-- 6. Exemplo de registros
SELECT * FROM TestAnonymization10k WHERE ROWNUM <= 10 ORDER BY ID;

-- 7. Limpar procedure
DROP PROCEDURE InsertTestData10k;


-- ============================================
-- SCRIPT DE LIMPEZA (Oracle)
-- ============================================

BEGIN
EXECUTE IMMEDIATE 'DROP TABLE TestAnonymization100k';
EXCEPTION
    WHEN OTHERS THEN NULL;
END;
/

BEGIN
EXECUTE IMMEDIATE 'DROP TABLE TestAnonymization10k';
EXCEPTION
    WHEN OTHERS THEN NULL;
END;
/

BEGIN
EXECUTE IMMEDIATE 'DROP PROCEDURE InsertTestData100k';
EXCEPTION
    WHEN OTHERS THEN NULL;
END;
/

BEGIN
EXECUTE IMMEDIATE 'DROP PROCEDURE InsertTestData10k';
EXCEPTION
    WHEN OTHERS THEN NULL;
END;
/

SELECT 'Todas as tabelas de teste foram removidas!' AS Status FROM DUAL;