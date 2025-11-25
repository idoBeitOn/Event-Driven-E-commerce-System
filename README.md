# Event-Driven E-Commerce System

A simple but fully functional event-driven microservices architecture built with C# .NET, RabbitMQ, and Docker Compose.
The project demonstrates asynchronous communication between services, message publishing/consuming, and clean service separation.


#### Cart-Service creates a random, fully populated OrderDTO and publishes it.

#### RabbitMQ routes the message to a queue.

#### Order-Service consumes messages, processes orders, calculates shipping, and saves them as files.


##  Tech Stack

* .NET 8

* C#

* RabbitMQ (fanout exchange)

* Docker & Docker Compose

* ASP.NET Web API

* Factory Pattern (Order generation)

* Background Consumer Service


## Repository Structure
Cart-Service

Order-Service

SharedDTOs

RabbitMQConsumeBase

docker-compose.yml




## How to Run
### Requirements:

* Docker

* Docker Compose

 Start the whole system
docker-compose up --build


#### This will start:

cart-service (port 8080)

order-service (port 8081)

rabbitmq (management UI on port 15672)

 RabbitMQ Management UI

#### Go to:

http://localhost:15672


User: guest
Pass: guest

You will see:

Exchange: order-exchange

Queue: order-queue

Messages flowing through the system



### Creating an Order (API Request)

Send a POST request to the Cart-Service:

POST http://localhost:8080/api/cart/create-order

Content-Type: application/json



Example body:

{
  
  "orderId": "ORD-123",
  
  "itemsNum": 3

}





### Retrieving order date from consumer service (API Request)

Send a GET request to the Order-Service:

GET http://localhost:8081/api/orders/ORD-029




























































