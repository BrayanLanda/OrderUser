# 🚀 Kafka Saga — Auth · Orders · Products

Proyecto de aprendizaje para explorar **Kafka + MassTransit + Saga Pattern** en .NET 8  
con persistencia real: MongoDB (Auth), PostgreSQL (Orders y Products).

---

## 🏗️ Arquitectura

```
Cliente
  │
  ├─ POST /orders  ──►  Orders.API
  │                          │
  │            ┌─────────────▼──────────────────────────────┐
  │            │           SAGA (Orders.Infrastructure)     │
  │            │                                            │
  │            │  AWAITING_USER_VALIDATION                  │
  │            │    │ publica: order.validation.requested   │
  │            │    ▼                                       │
  │            │  (Auth valida usuario)                     │
  │            │    │ consume: user.validated / rejected    │
  │            │    ▼                                       │
  │            │  PENDING                                   │
  │            │    │ publica: order.created                │
  │            │    ▼                                       │
  │            │  (Products verifica stock)                 │
  │            │    │ consume: stock.reserved / insufficient│
  │            │    ▼                                       │
  │            │  CONFIRMED / CANCELLED                     │
  │            └────────────────────────────────────────────┘
  │
  └─ Auth.API  ──►  consume: order.validation.requested
                    publica: user.validated / user.rejected
                    consume: order.confirmed → registra actividad

  └─ Products.API ► consume: order.created
                    publica: stock.reserved / stock.insufficient
                    consume: order.cancelled → libera stock
```

---

## 📦 Stack

| Componente | Tecnología |
|---|---|
| Framework | .NET 8 |
| Mensajería | MassTransit 8 + Kafka (Confluent) |
| Auth BD | MongoDB 7 |
| Orders BD | PostgreSQL 16 |
| Products BD | PostgreSQL 16 |
| ORM | Entity Framework Core 8 (Orders/Products) |
| ODM | MongoDB.Driver (Auth) |
| Outbox Pattern | MassTransit Outbox (nativo) |
| Observabilidad | Kafka UI (http://localhost:8080) |

---

## 🐳 Levantar la infraestructura

```bash
# Desde la raíz del proyecto
docker compose up -d

# Verificar que todo esté saludable
docker compose ps
```

### Servicios y puertos

| Servicio | Puerto | URL/Conexión |
|---|---|---|
| Kafka (externo) | 9092 | `localhost:9092` |
| Kafka UI | 8080 | http://localhost:8080 |
| MongoDB | 27017 | `mongodb://auth_user:auth_pass@localhost:27017/authdb` |
| PostgreSQL Orders | 5432 | `Host=localhost;Port=5432;Database=ordersdb;Username=orders_user;Password=orders_pass` |
| PostgreSQL Products | 5433 | `Host=localhost;Port=5433;Database=productsdb;Username=products_user;Password=products_pass` |

---

## 📁 Estructura del proyecto

```
/
├── docker-compose.yml
├── docker/
│   └── mongo-init.js          # Seed inicial de MongoDB
│
└── src/
    ├── Shared.Contracts/       # Eventos compartidos entre micros
    │   └── Events/
    │       ├── Auth/           # AuthEvents.cs
    │       ├── Orders/         # OrderEvents.cs
    │       └── Products/       # ProductEvents.cs
    │
    ├── Auth.Domain/            # Entidades: User, RefreshToken
    ├── Auth.Application/       # Casos de uso, interfaces
    ├── Auth.Infrastructure/    # MongoDB, consumers Kafka, JWT
    ├── Auth.API/               # Controllers, Program.cs
    │
    ├── Orders.Domain/          # Entidades: Order, OrderItem
    ├── Orders.Application/     # Casos de uso, interfaces
    ├── Orders.Infrastructure/  # EF Core, SAGA STATE MACHINE, Outbox
    ├── Orders.API/             # Controllers, Program.cs
    │
    ├── Products.Domain/        # Entidades: Product, StockReservation
    ├── Products.Application/   # Casos de uso, interfaces
    ├── Products.Infrastructure/# EF Core, consumers Kafka
    └── Products.API/           # Controllers, Program.cs
```

---

## 🔄 Tópicos de Kafka

| Tópico | Publicado por | Consumido por | Particiones |
|---|---|---|---|
| `order.validation.requested` | Orders | Auth | 3 |
| `user.validated` | Auth | Orders | 3 |
| `user.rejected` | Auth | Orders | 3 |
| `order.created` | Orders | Products | 3 |
| `stock.reserved` | Products | Orders | 3 |
| `stock.insufficient` | Products | Orders | 3 |
| `order.confirmed` | Orders | Auth | 3 |
| `order.cancelled` | Orders | Auth, Products | 3 |

---

## 🔐 Estados de la Saga (Orders)

```
AWAITING_USER_VALIDATION  →  UserValidated      →  PENDING
AWAITING_USER_VALIDATION  →  UserRejected       →  CANCELLED
PENDING                   →  StockReserved      →  CONFIRMED
PENDING                   →  StockInsufficient  →  CANCELLED
```

---

## 🧪 Usuario de prueba (seed MongoDB)

```
Email:    test@example.com
Password: Test123!
```

---

## 📋 Orden de implementación sugerido

1. **Infraestructura** ✅ (este docker-compose)
2. **Shared.Contracts** ✅ (eventos ya definidos)
3. **Domain layers** — entidades y value objects
4. **Auth micro** — JWT + MongoDB + Consumer de validación
5. **Products micro** — Catálogo + Consumer de stock
6. **Orders micro** — EF Core + **Saga State Machine** + Outbox
7. **Pruebas end-to-end** con Kafka UI para ver el flujo

---

## 💡 Conceptos clave que vas a ver en acción

- **Saga Coreografiada** — cada micro reacciona a eventos sin un orquestador central
- **Transacciones compensatorias** — si falla el stock, la orden se cancela y se libera todo
- **Outbox Pattern** — garantiza que el evento se publica aunque Kafka esté momentáneamente caído
- **CorrelationId** — el hilo que une todos los eventos de una misma transacción
- **Consumer Groups** — cada micro tiene su propio grupo, Kafka los balancea automáticamente