-- ==========================================
-- ORACLE - Criação e População de Dados
-- ==========================================

-- Conectar como testuser
CONNECT testuser/TestUser@2024@localhost:1521/TESTDB;

-- ==========================================
-- TABELA 1: Clientes
-- ==========================================
BEGIN
EXECUTE IMMEDIATE 'DROP TABLE Clientes CASCADE CONSTRAINTS';
EXCEPTION
   WHEN OTHERS THEN NULL;
END;
/

CREATE TABLE Clientes (
                          ClienteID NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                          Nome VARCHAR2(100) NOT NULL,
                          Email VARCHAR2(100) NOT NULL,
                          CPF VARCHAR2(14) NOT NULL,
                          Telefone VARCHAR2(20),
                          DataCadastro TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Popular Clientes
INSERT INTO Clientes (Nome, Email, CPF, Telefone) VALUES
    ('Daniel Ribeiro', 'daniel.ribeiro@email.com', '123. 456.111-22', '(11) 98111-9999');

INSERT INTO Clientes (Nome, Email, CPF, Telefone) VALUES
    ('Bianca Costa', 'bianca.costa@email.com', '234.567.222-33', '(21) 97222-8888');

INSERT INTO Clientes (Nome, Email, CPF, Telefone) VALUES
    ('Eduardo Silva', 'eduardo.silva@email.com', '345.678. 333-44', '(31) 96333-7777');

INSERT INTO Clientes (Nome, Email, CPF, Telefone) VALUES
    ('Vanessa Lima', 'vanessa.lima@email.com', '456.789.444-55', '(41) 95444-6666');

INSERT INTO Clientes (Nome, Email, CPF, Telefone) VALUES
    ('Marcelo Santos', 'marcelo. santos@email.com', '567.890.555-66', '(51) 94555-5555');

INSERT INTO Clientes (Nome, Email, CPF, Telefone) VALUES
    ('Aline Rocha', 'aline.rocha@email.com', '678.901.666-77', '(61) 93666-4444');

INSERT INTO Clientes (Nome, Email, CPF, Telefone) VALUES
    ('Ricardo Alves', 'ricardo. alves@email.com', '789.012.777-88', '(71) 92777-3333');

INSERT INTO Clientes (Nome, Email, CPF, Telefone) VALUES
    ('Daniela Pereira', 'daniela.pereira@email.com', '890.123.888-99', '(81) 91888-2222');

INSERT INTO Clientes (Nome, Email, CPF, Telefone) VALUES
    ('Felipe Martins', 'felipe.martins@email.com', '901. 234.999-00', '(91) 90999-1111');

INSERT INTO Clientes (Nome, Email, CPF, Telefone) VALUES
    ('Letícia Gomes', 'leticia.gomes@email.com', '012.345.000-11', '(11) 98000-9999');

COMMIT;

-- ==========================================
-- TABELA 2: Funcionarios
-- ==========================================
BEGIN
EXECUTE IMMEDIATE 'DROP TABLE Funcionarios CASCADE CONSTRAINTS';
EXCEPTION
   WHEN OTHERS THEN NULL;
END;
/

CREATE TABLE Funcionarios (
                              FuncionarioID NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                              NomeCompleto VARCHAR2(100) NOT NULL,
                              EmailCorporativo VARCHAR2(100) NOT NULL,
                              DocumentoCPF VARCHAR2(14) NOT NULL,
                              TelefoneContato VARCHAR2(20),
                              Endereco VARCHAR2(200),
                              Cargo VARCHAR2(50),
                              Salario NUMBER(10,2),
                              DataAdmissao DATE DEFAULT SYSDATE
);

-- Popular Funcionarios
INSERT INTO Funcionarios (NomeCompleto, EmailCorporativo, DocumentoCPF, TelefoneContato, Endereco, Cargo, Salario) VALUES
    ('Beatriz Oliveira', 'beatriz.oliveira@empresa.com', '111.222. 999-00', '(11) 98765-0001', 'Rua Oracle, 100 - Centro, Recife - PE', 'DBA', 8500.00);

INSERT INTO Funcionarios (NomeCompleto, EmailCorporativo, DocumentoCPF, TelefoneContato, Endereco, Cargo, Salario) VALUES
    ('Thiago Costa', 'thiago.costa@empresa.com', '222.333.888-11', '(21) 97654-0002', 'Avenida Java, 200 - Boa Viagem, Recife - PE', 'Arquiteto', 10000.00);

INSERT INTO Funcionarios (NomeCompleto, EmailCorporativo, DocumentoCPF, TelefoneContato, Endereco, Cargo, Salario) VALUES
    ('Larissa Silva', 'larissa.silva@empresa.com', '333.444.777-22', '(31) 96543-0003', 'Rua Python, 300 - Centro, Fortaleza - CE', 'Desenvolvedora', 6500.00);

INSERT INTO Funcionarios (NomeCompleto, EmailCorporativo, DocumentoCPF, TelefoneContato, Endereco, Cargo, Salario) VALUES
    ('Gabriel Lima', 'gabriel.lima@empresa.com', '444.555. 666-33', '(41) 95432-0004', 'Alameda CSharp, 400 - Meireles, Fortaleza - CE', 'Tech Lead', 11000.00);

INSERT INTO Funcionarios (NomeCompleto, EmailCorporativo, DocumentoCPF, TelefoneContato, Endereco, Cargo, Salario) VALUES
    ('Camila Rocha', 'camila.rocha@empresa.com', '555.666.555-44', '(51) 94321-0005', 'Avenida SQL, 500 - Centro, Manaus - AM', 'Analista', 5800.00);

COMMIT;

-- ==========================================
-- TABELA 3: Pedidos
-- ==========================================
BEGIN
EXECUTE IMMEDIATE 'DROP TABLE Pedidos CASCADE CONSTRAINTS';
EXCEPTION
   WHEN OTHERS THEN NULL;
END;
/

CREATE TABLE Pedidos (
                         PedidoID NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                         ClienteNome VARCHAR2(100),
                         ClienteEmail VARCHAR2(100),
                         ClienteCPF VARCHAR2(14),
                         ClienteTelefone VARCHAR2(20),
                         EnderecoEntrega VARCHAR2(200),
                         ValorTotal NUMBER(10,2),
                         DataPedido TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Popular Pedidos
INSERT INTO Pedidos (ClienteNome, ClienteEmail, ClienteCPF, ClienteTelefone, EnderecoEntrega, ValorTotal) VALUES
    ('Lucas Ferreira', 'lucas.ferreira@email.com', '666. 777.444-55', '(61) 93210-0006', 'Rua Docker, 600 - Centro, Belém - PA', 350.00);

INSERT INTO Pedidos (ClienteNome, ClienteEmail, ClienteCPF, ClienteTelefone, EnderecoEntrega, ValorTotal) VALUES
    ('Fernanda Santos', 'fernanda. santos@email.com', '777.888.333-66', '(71) 92109-0007', 'Avenida Kubernetes, 700 - Nazaré, Belém - PA', 275.50);

INSERT INTO Pedidos (ClienteNome, ClienteEmail, ClienteCPF, ClienteTelefone, EnderecoEntrega, ValorTotal) VALUES
    ('Rafael Costa', 'rafael.costa@email.com', '888.999.222-77', '(81) 91098-0008', 'Rua Git, 800 - Centro, Goiânia - GO', 189.90);

INSERT INTO Pedidos (ClienteNome, ClienteEmail, ClienteCPF, ClienteTelefone, EnderecoEntrega, ValorTotal) VALUES
    ('Juliana Lima', 'juliana.lima@email.com', '999.000.111-88', '(91) 90987-0009', 'Alameda Azure, 900 - Setor Sul, Goiânia - GO', 425.00);

INSERT INTO Pedidos (ClienteNome, ClienteEmail, ClienteCPF, ClienteTelefone, EnderecoEntrega, ValorTotal) VALUES
    ('Carlos Silva', 'carlos.silva@email.com', '000.111.000-99', '(11) 98876-0010', 'Travessa AWS, 1000 - Centro, Campo Grande - MS', 159.99);

COMMIT;

-- Mensagem de sucesso
DECLARE
v_message VARCHAR2(100) := 'Oracle: Banco TESTDB criado e populado com sucesso!';
BEGIN
    DBMS_OUTPUT.PUT_LINE(v_message);
END;
/