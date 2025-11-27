# Event-Driven E-Commerce System

A simple but fully functional **event-driven microservices architecture** built with **C# .NET**, **RabbitMQ**, and **Docker Compose**.  
This project demonstrates asynchronous communication between services, message publishing/consuming, and clean service separation.

---

## How It Works

1. **Cart-Service** creates a random, fully populated `OrderDTO` and publishes it.  
2. **RabbitMQ** routes the message to a queue.  
3. **Order-Service** consumes messages, processes orders, calculates shipping, and saves them as files.

---

## Tech Stack

- **.NET 8**  
- **C#**  
- **RabbitMQ** (fanout exchange)  
- **Docker & Docker Compose**  
- **ASP.NET Web API**  
- **Factory Pattern** (Order generation)  
- **Background Consumer Service**


---

## How to Run

### Requirements

- Docker  
- Docker Compose  

### Start the system


docker-compose up --build

### Services Started

When you run `docker-compose up --build`, the following services will start:

- **cart-service** (port 8080)  
- **order-service** (port 8081)  
- **rabbitmq** (management UI on port 15672)

### RabbitMQ Management UI

You can access the RabbitMQ management interface at: [http://localhost:15672](http://localhost:15672)  

**Credentials:**

- **User:** `guest`  
- **Password:** `guest`  

In the management UI, you will see:

- **Exchange:** `order-exchange`  
- **Queue:** `order-queue`  
- **Messages** flowing through the system



---

### Creating an Order (API Request)

Send a POST request to the **Cart-Service**:

```bash
POST http://localhost:8080/api/cart/create-order
Content-Type: application/json

{
  "orderId": "ORD-123",
  "itemsNum": 3
}

```

### Retrieving Order Data from Consumer Service (API Request)

Send a GET request to the **Order-Service**:

```bash
GET http://localhost:8081/api/orders/ORD-029


