-- ==========================================
-- SQL SERVER - Criação e População de Dados
-- ==========================================

USE master;
GO

-- Criar database se não existir
IF NOT EXISTS (SELECT name FROM sys. databases WHERE name = 'TestDB')
BEGIN
    CREATE DATABASE TestDB;
END
GO

USE TestDB;
GO

-- ==========================================
-- TABELA 1: Clientes
-- ==========================================
IF OBJECT_ID('dbo. Clientes', 'U') IS NOT NULL
DROP TABLE dbo. Clientes;
GO

CREATE TABLE dbo.Clientes (
                              ClienteID INT PRIMARY KEY IDENTITY(1,1),
                              Nome VARCHAR(100) NOT NULL,
                              Email VARCHAR(100) NOT NULL,
                              CPF VARCHAR(14) NOT NULL,
                              Telefone VARCHAR(20),
                              DataCadastro DATETIME DEFAULT GETDATE()
);
GO

-- Popular Clientes
INSERT INTO dbo. Clientes (Nome, Email, CPF, Telefone) VALUES
('João da Silva', 'joao.silva@email.com', '123.456.789-09', '(11) 98765-4321'),
('Maria Santos', 'maria.santos@email.com', '987.654.321-00', '(21) 99876-5432'),
('Pedro Oliveira', 'pedro.oliveira@email.com', '456.789.123-45', '(31) 97654-3210'),
('Ana Costa', 'ana.costa@email.com', '789.123.456-78', '(41) 96543-2109'),
('Carlos Souza', 'carlos.souza@email.com', '321.654.987-12', '(51) 95432-1098'),
('Juliana Lima', 'juliana.lima@email.com', '654.987.321-34', '(61) 94321-0987'),
('Rafael Gomes', 'rafael.gomes@email.com', '147.258.369-56', '(71) 93210-9876'),
('Camila Rocha', 'camila.rocha@email.com', '258.369.147-67', '(81) 92109-8765'),
('Lucas Martins', 'lucas.martins@email.com', '369.147.258-89', '(91) 91098-7654'),
('Fernanda Alves', 'fernanda.alves@email.com', '741.852.963-90', '(11) 90987-6543');
GO

-- ==========================================
-- TABELA 2: Funcionarios
-- ==========================================
IF OBJECT_ID('dbo. Funcionarios', 'U') IS NOT NULL
DROP TABLE dbo.Funcionarios;
GO

CREATE TABLE dbo.Funcionarios (
                                  FuncionarioID INT PRIMARY KEY IDENTITY(1,1),
                                  NomeCompleto VARCHAR(100) NOT NULL,
                                  EmailCorporativo VARCHAR(100) NOT NULL,
                                  DocumentoCPF VARCHAR(14) NOT NULL,
                                  TelefoneContato VARCHAR(20),
                                  Endereco VARCHAR(200),
                                  Cargo VARCHAR(50),
                                  Salario DECIMAL(10,2),
                                  DataAdmissao DATE DEFAULT GETDATE()
);
GO

-- Popular Funcionarios
INSERT INTO dbo. Funcionarios (NomeCompleto, EmailCorporativo, DocumentoCPF, TelefoneContato, Endereco, Cargo, Salario) VALUES
('Bruno Ferreira', 'bruno.ferreira@empresa.com', '111.222.333-44', '(11) 98888-7777', 'Rua das Flores, 123 - Centro, São Paulo - SP', 'Analista', 5000.00),
('Patricia Santos', 'patricia.santos@empresa.com', '222.333.444-55', '(21) 97777-6666', 'Avenida Paulista, 1000 - Bela Vista, São Paulo - SP', 'Gerente', 8000.00),
('Rodrigo Lima', 'rodrigo.lima@empresa.com', '333.444.555-66', '(31) 96666-5555', 'Rua Augusta, 500 - Consolação, São Paulo - SP', 'Desenvolvedor', 6000.00),
('Amanda Costa', 'amanda.costa@empresa.com', '444.555.666-77', '(41) 95555-4444', 'Alameda Santos, 800 - Jardim Paulista, São Paulo - SP', 'Designer', 4500.00),
('Gustavo Rocha', 'gustavo.rocha@empresa.com', '555.666.777-88', '(51) 94444-3333', 'Avenida Brasil, 2000 - Centro, Rio de Janeiro - RJ', 'Analista', 5500.00);
GO

-- ==========================================
-- TABELA 3: Pedidos
-- ==========================================
IF OBJECT_ID('dbo. Pedidos', 'U') IS NOT NULL
DROP TABLE dbo.Pedidos;
GO

CREATE TABLE dbo.Pedidos (
                             PedidoID INT PRIMARY KEY IDENTITY(1,1),
                             ClienteNome VARCHAR(100),
                             ClienteEmail VARCHAR(100),
                             ClienteCPF VARCHAR(14),
                             ClienteTelefone VARCHAR(20),
                             EnderecoEntrega VARCHAR(200),
                             ValorTotal DECIMAL(10,2),
                             DataPedido DATETIME DEFAULT GETDATE()
);
GO

-- Popular Pedidos
INSERT INTO dbo. Pedidos (ClienteNome, ClienteEmail, ClienteCPF, ClienteTelefone, EnderecoEntrega, ValorTotal) VALUES
('Thiago Alves', 'thiago.alves@email.com', '666.777.888-99', '(61) 93333-2222', 'Rua do Sol, 300 - Copacabana, Rio de Janeiro - RJ', 150.50),
('Gabriela Martins', 'gabriela.martins@email.com', '777.888.999-00', '(71) 92222-1111', 'Avenida Atlântica, 400 - Ipanema, Rio de Janeiro - RJ', 250.00),
('Diego Pereira', 'diego. pereira@email.com', '888.999.000-11', '(81) 91111-0000', 'Rua das Palmeiras, 150 - Leblon, Rio de Janeiro - RJ', 89.90),
('Isabela Santos', 'isabela. santos@email.com', '999.000.111-22', '(91) 90000-9999', 'Travessa do Comércio, 50 - Centro, Belo Horizonte - MG', 320.75),
('Fernando Costa', 'fernando.costa@email.com', '000.111.222-33', '(11) 99999-8888', 'Praça da Sé, 1 - Centro, São Paulo - SP', 199.99);
GO

PRINT 'SQL Server: Banco TestDB criado e populado com sucesso! ';
GO