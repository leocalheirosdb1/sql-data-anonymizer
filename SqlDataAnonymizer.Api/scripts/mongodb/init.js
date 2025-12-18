db = db.getSiblingDB('SqlDataAnonymizerDb');

db.createUser({
    user: 'anonymizer_user',
    pwd: 'Anonymizer@2024',
    roles: [
        {
            role: 'readWrite',
            db: 'SqlDataAnonymizerDb'
        }
    ]
});

db.createCollection('AnonymizationJobs', {
    validator: {
        $jsonSchema: {
            bsonType: 'object',
            required: ['_id', 'server', 'database', 'status', 'startedAt'],
            properties: {
                _id: {
                    bsonType: 'string',
                    description: 'Job ID (GUID)'
                },
                server: {
                    bsonType:  'string',
                    description:  'Nome do servidor'
                },
                database: {
                    bsonType: 'string',
                    description: 'Nome do banco de dados'
                },
                databaseType: {
                    bsonType: 'string',
                    enum: ['SqlServer', 'MySql'],
                    description: 'Tipo do banco de dados'
                },
                status: {
                    bsonType: 'string',
                    enum: ['Queued', 'Running', 'Completed', 'Failed'],
                    description: 'Status do job'
                },
                startedAt: {
                    bsonType: 'date',
                    description: 'Data/hora de início'
                },
                completedAt: {
                    bsonType: ['date', 'null'],
                    description: 'Data/hora de conclusão'
                },
                logs: {
                    bsonType:  'array',
                    items: {
                        bsonType:  'string'
                    },
                    description: 'Logs do job'
                },
                errorMessage: {
                    bsonType: ['string', 'null'],
                    description: 'Mensagem de erro'
                }
            }
        }
    }
});

print('MongoDB inicializado com sucesso! ');