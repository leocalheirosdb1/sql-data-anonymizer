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

---

## 🛠️ Troubleshooting

### SQL Server não inicia

```bash
docker-compose logs sqlserver
```

Verifique se a porta 1433 não está em uso.

### MySQL charset issues

Os scripts já estão configurados com `utf8mb4`.

---

## 📝 Notas

- Os dados são precisam ser criados pelos scripts em `./scripts/`
- Os volumes persistem os dados entre reinicializações
- Use `docker-compose down -v` para limpar tudo

---