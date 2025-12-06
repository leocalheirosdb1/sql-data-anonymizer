# 🐳 Docker Compose - Ambientes de Teste

Este Docker Compose cria **3 bancos de dados** para testes do SQL Data Anonymizer:
- **SQL Server 2022**
- **MySQL 8.0**
- **Oracle XE 21**

Cada banco possui **3 tabelas** com dados sensíveis já populados.

## 🚀 Como Usar

### 1. Iniciar os Containers

```bash
docker-compose up -d
```

### 2.  Verificar Status

```bash
docker-compose ps
```

### 3.  Acessar Logs

```bash
# Todos os containers
docker-compose logs -f

# Container específico
docker-compose logs -f sqlserver
docker-compose logs -f mysql
docker-compose logs -f oracle
```

### 4. Parar os Containers

```bash
docker-compose down
```

### 5. Remover Tudo (incluindo volumes)

```bash
docker-compose down -v
```

---

## 🔗 Credenciais de Acesso

### SQL Server
- **Host:** `localhost`
- **Port:** `1433`
- **User:** `sa`
- **Password:** `SqlServer@2024`
- **Database:** `TestDB`

### MySQL
- **Host:** `localhost`
- **Port:** `3306`
- **User:** `root`
- **Password:** `MySql@2024`
- **Database:** `TestDB`

### Oracle
- **Host:** `localhost`
- **Port:** `1521`
- **User:** `testuser`
- **Password:** `TestUser@2024`
- **SID/Service:** `TESTDB`

---

## 🌐 Adminer (Interface Web)

Acesse: **http://localhost:8080**

**Para SQL Server:**
- Sistema: `MS SQL`
- Servidor: `sqlserver`
- Usuário: `sa`
- Senha: `SqlServer@2024`
- Base de dados: `TestDB`

**Para MySQL:**
- Sistema: `MySQL`
- Servidor: `mysql`
- Usuário: `root`
- Senha: `MySql@2024`
- Base de dados: `TestDB`

**Para Oracle:**
- Sistema: `Oracle (beta)`
- Servidor: `oracle:1521/TESTDB`
- Usuário: `testuser`
- Senha: `TestUser@2024`

---

## 📊 Estrutura das Tabelas

Cada banco possui as mesmas 3 tabelas:

### 1. **Clientes**
- `ClienteID` (PK)
- `Nome` (Sensível)
- `Email` (Sensível)
- `CPF` (Sensível)
- `Telefone` (Sensível)
- `DataCadastro`

**10 registros**

### 2. **Funcionarios**
- `FuncionarioID` (PK)
- `NomeCompleto` (Sensível)
- `EmailCorporativo` (Sensível)
- `DocumentoCPF` (Sensível)
- `TelefoneContato` (Sensível)
- `Endereco` (Sensível)
- `Cargo`
- `Salario`
- `DataAdmissao`

**5 registros**

### 3. **Pedidos**
- `PedidoID` (PK)
- `ClienteNome` (Sensível)
- `ClienteEmail` (Sensível)
- `ClienteCPF` (Sensível)
- `ClienteTelefone` (Sensível)
- `EnderecoEntrega` (Sensível)
- `ValorTotal`
- `DataPedido`

**5 registros**

---

## 🧪 Testar a API

### SQL Server

```bash
curl -X POST http://localhost:5000/api/anonimizar \
  -H "Content-Type: application/json" \
  -d '{
    "servidor": "localhost",
    "banco": "TestDB",
    "tipoBanco": "SqlServer"
  }'
```

### MySQL

```bash
curl -X POST http://localhost:5000/api/anonimizar \
  -H "Content-Type: application/json" \
  -d '{
    "servidor": "localhost",
    "banco": "TestDB",
    "tipoBanco": "MySql"
  }'
```

### Oracle

```bash
curl -X POST http://localhost:5000/api/anonimizar \
  -H "Content-Type: application/json" \
  -d '{
    "servidor": "localhost",
    "banco": "TESTDB",
    "tipoBanco": "Oracle"
  }'
```

---

## 🛠️ Troubleshooting

### SQL Server não inicia

```bash
docker-compose logs sqlserver
```

Verifique se a porta 1433 não está em uso.

### MySQL charset issues

Os scripts já estão configurados com `utf8mb4`.

### Oracle demora para iniciar

O Oracle XE pode demorar **1-2 minutos** para ficar pronto.  Aguarde o healthcheck.

---

## 📝 Notas

- Os dados são **fictícios** e criados apenas para testes
- Os volumes persistem os dados entre reinicializações
- Use `docker-compose down -v` para limpar tudo

---

Desenvolvido com 🐳 e ☕