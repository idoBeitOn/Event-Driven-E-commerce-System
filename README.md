# Event-Driven E-Commerce System

A production-ready **event-driven microservices architecture** built with **C# .NET 8**, **RabbitMQ**, **PostgreSQL**, and **Docker Compose**.  
This project demonstrates asynchronous communication, database persistence, resilience patterns, and clean service separation.

---

## Architecture

```
┌─────────────┐         ┌──────────────┐         ┌─────────────┐         ┌──────────────┐
│ Cart Service│         │   RabbitMQ   │         │Order Service │        │  PostgreSQL │
│  (Port 8080)│────────▶│  (Port 5672) │────────▶│ (Port 8081) │────────▶│  (Port 5432)│
│             │ Publish │              │ Consume │              │  Save  │              │
│  - Creates  │  Order  │  - Exchange: │  Order  │  - Calculates│ Orders │  - Orders    │
│    Orders   │  Events │    order-    │  Events │    Shipping  │        │  - OrderItems│
│  - Publishes│         │    exchange  │         │  - Persists  │        │              │
│    to Queue │         │  - Queue:    │         │    to DB     │        │              │
│             │         │    order-queue│        │              │        │              │
└─────────────┘         └──────────────┘         └─────────────┘         └──────────────┘
```

### Flow

1. **Cart Service** receives POST request → creates `OrderDTO` → publishes to RabbitMQ
2. **RabbitMQ** routes message via fanout exchange to `order-queue`
3. **Order Service** consumes message → calculates shipping (10% of total) → saves to PostgreSQL
4. **PostgreSQL** persists orders with automatic migrations on startup
5. **Order Service** exposes GET endpoint to retrieve order summaries from database

---

## Tech Stack

### Core Technologies
- **.NET 8** - Modern C# framework
- **C#** - Primary programming language
- **RabbitMQ** - Message broker (fanout exchange pattern)
- **PostgreSQL** - Relational database with Entity Framework Core
- **Docker & Docker Compose** - Containerization and orchestration

### Patterns & Practices
- **Event-Driven Architecture** - Asynchronous message-based communication
- **Factory Pattern** - Order generation
- **Repository Pattern** - Data access via EF Core
- **Dependency Injection** - Loose coupling and testability
- **Resilience Patterns** - Polly retry with exponential backoff
- **Health Checks** - Production-ready monitoring endpoints
- **Structured Logging** - Serilog with file and console sinks

### Infrastructure
- **Entity Framework Core** - ORM for database operations
- **Automatic Migrations** - Database schema updates on startup
- **Adminer** - Web-based database management UI

---

## Features

✅ **Event-Driven Communication** - Asynchronous order processing via RabbitMQ  
✅ **Database Persistence** - PostgreSQL with EF Core and automatic migrations  
✅ **Resilience** - Retry policies for transient failures  
✅ **Health Monitoring** - Health check endpoints for orchestration  
✅ **Structured Logging** - Serilog with contextual logging  
✅ **Unit & Integration Tests** - xUnit tests with InMemory database  
✅ **Docker Compose** - One-command deployment  
✅ **API Documentation** - Swagger/OpenAPI

---

## How to Run

### Prerequisites

- **Docker Desktop** (or Docker + Docker Compose)
- **.NET 8 SDK** (optional, for local development)

### Quick Start

1. **Clone the repository**
   ```bash
   git clone <your-repo-url>
   cd Event-Driven-E-commerce-System
   ```

2. **Start all services**
   ```bash
   docker-compose up --build
   ```

   This will start:
   - **PostgreSQL** (port 5432) - Database with automatic migrations
   - **RabbitMQ** (ports 5672, 15672) - Message broker
   - **Adminer** (port 8082) - Database UI
   - **Cart Service** (port 8080) - Order creation API
   - **Order Service** (port 8081) - Order processing API

3. **Verify services are running**
   - Cart Service: http://localhost:8080/swagger
   - Order Service: http://localhost:8081/swagger
   - RabbitMQ Management: http://localhost:15672 (guest/guest)
   - Adminer: http://localhost:8082 (Server: postgres, DB: ordersdb, User: postgres, Pass: postgres)

---

## API Endpoints

### Cart Service (Port 8080)

**Create Order**
```http
POST /api/cart/create-order
Content-Type: application/json

{
  "orderId": "ORD-123",
  "itemsNum": 3
}
```

**Health Check**
```http
GET /health
```

### Order Service (Port 8081)

**Get Order Summary**
```http
GET /api/orders/{orderId}
```

**Health Checks**
```http
GET /health          # Basic health check
GET /health/ready    # Readiness check (includes database)
```

---

## Testing

### Run Tests

```bash
cd OrderService/Tests
dotnet test
```

### Test Coverage

- **OrderConsumer Tests** - Verifies order processing, duplicate detection, shipping calculation
- **OrdersController Tests** - Verifies API endpoints return correct data

---

## Database

### Access Database

**Via Adminer (Web UI)**
- URL: http://localhost:8082
- System: PostgreSQL
- Server: `postgres`
- Database: `ordersdb`
- Username: `postgres`
- Password: `postgres`

**Via Command Line**
```bash
docker exec -it postgres psql -U postgres -d ordersdb
```

### Schema

- **Orders** - Main order table with customer, payment, shipping, and totals
- **OrderItems** - Line items linked to orders via foreign key

### Migrations

Migrations are **automatically applied on startup**. No manual migration steps required.

---

## Project Structure

```
Event-Driven-E-commerce-System/
├── Cart Service/          # Order creation and publishing service
│   ├── Controllers/       # API endpoints
│   ├── Services/          # OrderFactory, OrderPublisher
│   └── Program.cs         # Service configuration
│
├── OrderService/          # Order processing and persistence service
│   ├── Controllers/       # Order retrieval API
│   ├── Data/             # EF Core DbContext and entities
│   │   ├── Entities/     # Order, OrderItem entities
│   │   └── Migrations/   # Database migrations
│   ├── Logic/            # OrderConsumer (RabbitMQ consumer)
│   ├── Services/         # Background services
│   └── Tests/            # Unit and integration tests
│
├── SharedDTOs/           # Shared data transfer objects
├── RabbitMQConsume/      # Base consumer implementation
└── docker-compose.yml    # Service orchestration
```

---

## Key Implementation Details

### Resilience

- **Polly Retry Policy** - Automatic retry with exponential backoff (1s, 2s, 4s) for RabbitMQ operations
- **Duplicate Detection** - Idempotent order processing (skips duplicates)

### Observability

- **Structured Logging** - Serilog with contextual properties
- **Health Checks** - Kubernetes/Docker Swarm compatible endpoints
- **Request Tracing** - OrderId correlation in logs

### Data Persistence

- **Entity Framework Core** - Type-safe database access
- **Automatic Migrations** - Schema updates on service startup
- **Connection Pooling** - Efficient database connection management

---

## Development

### Running Tests Locally

```bash
cd OrderService/Tests
dotnet test --verbosity normal
```

### Viewing Logs

```bash
# Order Service logs
docker-compose logs -f orderservice

# Cart Service logs
docker-compose logs -f cartservice
```

### Stopping Services

```bash
docker-compose down
```

**Note:** Data persists in Docker volumes. Use `docker-compose down -v` to remove volumes and reset database.

---

## Future Enhancements

- [ ] CI/CD pipeline (GitHub Actions)
- [ ] Additional microservices (Payment, Notification)
- [ ] Caching layer (Redis)
- [ ] API Gateway (Ocelot/YARP)
- [ ] Distributed tracing (OpenTelemetry)
- [ ] Authentication & Authorization (JWT)

---

## License

This project is for educational/portfolio purposes.
