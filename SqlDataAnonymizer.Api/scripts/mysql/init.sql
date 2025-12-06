-- ==========================================
-- MYSQL - Criação e População de Dados
-- ==========================================

USE TestDB;

-- ==========================================
-- TABELA 1: Clientes
-- ==========================================
DROP TABLE IF EXISTS Clientes;

CREATE TABLE Clientes (
                          ClienteID INT PRIMARY KEY AUTO_INCREMENT,
                          Nome VARCHAR(100) NOT NULL,
                          Email VARCHAR(100) NOT NULL,
                          CPF VARCHAR(14) NOT NULL,
                          Telefone VARCHAR(20),
                          DataCadastro TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Popular Clientes
INSERT INTO Clientes (Nome, Email, CPF, Telefone) VALUES
                                                      ('Carolina Vieira', 'carolina.vieira@email.com', '123.987.456-11', '(11) 98111-2222'),
                                                      ('Leandro Silva', 'leandro.silva@email.com', '234.876.543-22', '(21) 97222-3333'),
                                                      ('Simone Costa', 'simone.costa@email.com', '345.765.432-33', '(31) 96333-4444'),
                                                      ('Fábio Santos', 'fabio.santos@email.com', '456.654.321-44', '(41) 95444-5555'),
                                                      ('Cristina Lima', 'cristina.lima@email.com', '567.543.210-55', '(51) 94555-6666'),
                                                      ('Marcos Rocha', 'marcos.rocha@email.com', '678.432.109-66', '(61) 93666-7777'),
                                                      ('Sandra Alves', 'sandra. alves@email.com', '789.321.098-77', '(71) 92777-8888'),
                                                      ('Alex Pereira', 'alex.pereira@email.com', '890.210.987-88', '(81) 91888-9999'),
                                                      ('Roberta Martins', 'roberta.martins@email.com', '901.109.876-99', '(91) 90999-0000'),
                                                      ('Igor Gomes', 'igor.gomes@email.com', '012.098.765-00', '(11) 98000-1111');

-- ==========================================
-- TABELA 2: Funcionarios
-- ==========================================
DROP TABLE IF EXISTS Funcionarios;

CREATE TABLE Funcionarios (
                              FuncionarioID INT PRIMARY KEY AUTO_INCREMENT,
                              NomeCompleto VARCHAR(100) NOT NULL,
                              EmailCorporativo VARCHAR(100) NOT NULL,
                              DocumentoCPF VARCHAR(14) NOT NULL,
                              TelefoneContato VARCHAR(20),
                              Endereco VARCHAR(200),
                              Cargo VARCHAR(50),
                              Salario DECIMAL(10,2),
                              DataAdmissao DATE DEFAULT (CURRENT_DATE)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Popular Funcionarios
INSERT INTO Funcionarios (NomeCompleto, EmailCorporativo, DocumentoCPF, TelefoneContato, Endereco, Cargo, Salario) VALUES
                                                                                                                       ('Monica Oliveira', 'monica.oliveira@empresa.com', '111.999.888-11', '(11) 98765-1111', 'Rua ABC, 100 - Centro, Curitiba - PR', 'Analista', 4800.00),
                                                                                                                       ('Vinicius Souza', 'vinicius. souza@empresa.com', '222.888.777-22', '(21) 97654-2222', 'Avenida XYZ, 200 - Batel, Curitiba - PR', 'Coordenador', 7000.00),
                                                                                                                       ('Natália Ferreira', 'natalia.ferreira@empresa.com', '333.777.666-33', '(31) 96543-3333', 'Rua DEF, 300 - Água Verde, Curitiba - PR', 'Desenvolvedora', 5500.00),
                                                                                                                       ('Leonardo Costa', 'leonardo.costa@empresa.com', '444.666.555-44', '(41) 95432-4444', 'Alameda GHI, 400 - Centro, Porto Alegre - RS', 'Analista', 5200.00),
                                                                                                                       ('Tatiana Lima', 'tatiana.lima@empresa.com', '555.555.444-55', '(51) 94321-5555', 'Avenida JKL, 500 - Moinhos de Vento, Porto Alegre - RS', 'Gerente', 9000.00);

-- ==========================================
-- TABELA 3: Pedidos
-- ==========================================
DROP TABLE IF EXISTS Pedidos;

CREATE TABLE Pedidos (
                         PedidoID INT PRIMARY KEY AUTO_INCREMENT,
                         ClienteNome VARCHAR(100),
                         ClienteEmail VARCHAR(100),
                         ClienteCPF VARCHAR(14),
                         ClienteTelefone VARCHAR(20),
                         EnderecoEntrega VARCHAR(200),
                         ValorTotal DECIMAL(10,2),
                         DataPedido TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Popular Pedidos
INSERT INTO Pedidos (ClienteNome, ClienteEmail, ClienteCPF, ClienteTelefone, EnderecoEntrega, ValorTotal) VALUES
                                                                                                              ('André Martins', 'andre.martins@email.com', '666.444.333-66', '(61) 93210-6666', 'Rua MNO, 600 - Centro, Brasília - DF', 175.00),
                                                                                                              ('Priscila Santos', 'priscila. santos@email.com', '777.333.222-77', '(71) 92109-7777', 'Avenida PQR, 700 - Asa Sul, Brasília - DF', 290.50),
                                                                                                              ('Paulo Oliveira', 'paulo. oliveira@email.com', '888.222.111-88', '(81) 91098-8888', 'Rua STU, 800 - Centro, Salvador - BA', 125.75),
                                                                                                              ('Adriana Costa', 'adriana. costa@email.com', '999.111.000-99', '(91) 90987-9999', 'Alameda VWX, 900 - Barra, Salvador - BA', 450.00),
                                                                                                              ('Renato Silva', 'renato.silva@email.com', '000.999.888-00', '(11) 98876-0000', 'Travessa YZ, 1000 - Pelourinho, Salvador - BA', 89.00);

SELECT 'MySQL: Banco TestDB criado e populado com sucesso!' AS Message;