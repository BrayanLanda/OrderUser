/ =============================================================
//  MongoDB Init Script - Auth Service
//  Se ejecuta automáticamente al crear el contenedor
// =============================================================

// Cambiar a la base de datos de auth
db = db.getSiblingDB("authdb");

// Crear usuario específico para el microservicio
db.createUser({
    user: "auth_user",
    pwd: "auth_pass",
    roles: [{ role: "readWrite", db: "authdb" }],
});

// Crear colección de usuarios con validación básica
db.createCollection("users", {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["email", "passwordHash", "isActive", "createdAt"],
            properties: {
                email: { bsonType: "string" },
                passwordHash: { bsonType: "string" },
                isActive: { bsonType: "bool" },
                createdAt: { bsonType: "date" },
            },
        },
    },
});

// Índice único en email
db.users.createIndex({ email: 1 }, { unique: true });

// Colección de refresh tokens
db.createCollection("refreshTokens");
db.refreshTokens.createIndex({ userId: 1 });
db.refreshTokens.createIndex({ token: 1 }, { unique: true });
db.refreshTokens.createIndex({ expiresAt: 1 }, { expireAfterSeconds: 0 }); // TTL index

// Colección de auditoría de actividad (eventos de orders, etc.)
db.createCollection("activityLogs");
db.activityLogs.createIndex({ userId: 1, createdAt: -1 });

// Colección outbox para MassTransit (Outbox Pattern)
db.createCollection("outbox.messages");
db.createCollection("outbox.states");

// Seed: usuario de prueba (password: Test123!)
db.users.insertOne({
    _id: new ObjectId(),
    email: "test@example.com",
    passwordHash:
        "$2a$11$rBNPyXRHETiINhFQfYb0XuQGfFQJ3Z9HvJqBFJ0G4C4H2oHqMgVcO", // bcrypt de Test123!
    firstName: "Usuario",
    lastName: "De Prueba",
    isActive: true,
    roles: ["customer"],
    createdAt: new Date(),
    updatedAt: new Date(),
});

print("✅ MongoDB inicializado correctamente para Auth Service");
print("   - Colecciones: users, refreshTokens, activityLogs, outbox");
print("   - Usuario de prueba: test@example.com / Test123!");




