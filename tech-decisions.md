
# Decisões técnicas

Este documento explica as principais decisões técnicas do projeto e as razões por trás delas.

## 1. Processamento Assíncrono
### Decisão
API retorna imediatamente (`HTTP 202 Accepted`) e processa jobs em background usando **Channels** + **BackgroundProcessor**. 

### Por quê
- Anonimização pode levar minutos dependendo do volume de dados
- Evita timeout HTTP (limite de 30-60s)
- Cliente não fica bloqueado aguardando processamento
- Escalável:  múltiplos jobs podem ser enfileirados

### Trade Off:
Cliente precisa fazer polling para saber quando terminou (não recebe resposta imediata com resultado).

## 2. Anonimização Por Coluna
### Decisão
Um **UPDATE** separado para cada coluna sensível, não todas de uma vez.

### Por quê
- Segurança: Minimiza risco de lock escalation
- Produção 24/7: Sistema pode ter usuários ativos durante anonimização
- Previsibilidade: Comportamento consistente e controlado

### Trade Off:
Mais lento que atualizar todas as colunas de uma vez (múltiplos scans da tabela), mas significativamente mais seguro.

## 3. Temp Table + Batch Update
### Decisão
Criar temp table (#TempXXX), inserir valores anonimizados em batches (1000 registros), fazer UPDATE com JOIN.

### Por quê
- Performance: Batch update é ordens de magnitude mais rápido que linha por linha
- ROWLOCK: Mantém lock de linha (não trava tabela)
- Transacional: Rollback automático em caso de erro
- Otimização: OPTION (OPTIMIZE FOR UNKNOWN) melhora plano de execução

## 4. MongoDB para registro de execuções dos jobs
### Decisão
Logs de jobs (status, logs, timestamps) no MongoDB. Dados sensíveis permanecem em SQL/MySQL/Oracle.

### Por quê
- Schema flexível: Logs são arrays dinâmicos que crescem durante execução
- Writes rápidos: Append-only, otimizado para inserções
- Observabilidade: Fácil consultar histórico e logs