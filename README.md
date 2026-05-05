# 🛒 MicroserviceShop

A **production-ready E-commerce platform** built on a **Microservices Architecture** using **.NET 8**. The system is designed for high scalability, fault isolation, and maintainability — with each business domain running as an independent, deployable service.

---

## 📐 System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                          CLIENT (Browser)                       │
│                    Angular 18 + SSR + NgRx                      │
└────────────────────────────┬────────────────────────────────────┘
                             │ HTTP
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                        API GATEWAY (Ocelot)                     │
│         Routing, JWT Validation, Rate Limiting                  │
└─────┬──────────┬──────────┬──────────┬──────────┬──────────────┘
      │          │          │          │          │
      ▼          ▼          ▼          ▼          ▼
 ┌─────────┐ ┌────────┐ ┌────────┐ ┌────────┐ ┌──────────┐
 │  Auth   │ │Product │ │ Order  │ │Payment │ │Customer  │
 │Service  │ │Service │ │Service │ │Service │ │ Service  │
 └────┬────┘ └───┬────┘ └───┬────┘ └───┬────┘ └────┬─────┘
      │          │          │          │            │
      └──────────┴──────────┴──────────┴────────────┘
                             │
                    ┌────────┴────────┐
                    │  RabbitMQ       │  ← MassTransit (Event Bus)
                    │  (MassTransit)  │
                    └────────┬────────┘
                             │ Events
                    ┌────────┴──────────────┐
                    │                       │
             ┌──────┴──────┐     ┌──────────┴──────┐
             │Notification │     │  Report          │
             │  Service    │     │  Service         │
             └─────────────┘     └─────────────────┘

         ┌──────────────┐   ┌───────┐   ┌───────┐   ┌───────┐
         │  PostgreSQL  │   │ Redis │   │ MinIO │   │Aggreg.│
         │  (per svc)   │   │Cache  │   │ (S3)  │   │Service│
         └──────────────┘   └───────┘   └───────┘   └───────┘
```

---

## 🚀 Services Overview

| Service | Responsibility | Key Technologies |
|---|---|---|
| **ApiGateway** | Single entry point — request routing, JWT enforcement | Ocelot, ASP.NET Core |
| **AggregatorService** | Aggregates data from multiple services into one response | HttpClient, ASP.NET Core |
| **AuthService** | Registration, login, JWT issuing, roles & permissions | JWT, BCrypt, EF Core, MassTransit Outbox |
| **ProductService** | Product catalog, categories, image upload | EF Core, MinIO, MassTransit |
| **OrderService** | Order lifecycle management, order events | EF Core, MassTransit, JWT Auth |
| **PaymentService** | Payment processing and status tracking | EF Core, MassTransit |
| **CustomerService** | Customer profile management | EF Core |
| **NotificationService** | Email notifications triggered by domain events | MailKit, MimeKit, MassTransit |
| **ReportService** | Business reporting and statistics | EF Core |
| **CommonService** | Shared library: base repository, caching, exceptions, filters, events | Redis, EF Core, MinIO, JWT |

---

## 🏗️ Key Architectural Patterns

### ✅ Transactional Outbox Pattern
**AuthService** implements the **Outbox Pattern** via `MassTransit.EntityFrameworkCore`:

```csharp
x.AddEntityFrameworkOutbox<AuthDbContext>(o =>
{
    o.UsePostgres();  // Store events in DB — same transaction as domain write
    o.UseBusOutbox(); // Background worker reliably forwards events to RabbitMQ
});
```
> Events are **never lost** — even if RabbitMQ is temporarily unavailable at the moment of publishing.

### ✅ Repository & Specification Pattern
Each service uses a consistent data access layer with **Generic Repository** and **Specification Pattern** (defined in `CommonService`) to encapsulate complex query logic and keep controllers clean.

### ✅ Event-Driven Communication
Services communicate asynchronously via **RabbitMQ** using **MassTransit**. Domain events (e.g., `OrderPlaced`, `PaymentCompleted`) are published and consumed by the appropriate services (e.g., `NotificationService`, `ReportService`) without tight coupling.

### ✅ Distributed Caching
**Redis** (via `StackExchange.Redis`) is integrated through `CommonService` as a shared distributed cache layer, reducing database load for frequently accessed data.

### ✅ Centralized Shared Library
`CommonService` acts as a shared kernel, providing:
- `BaseRepository<T>` — generic CRUD operations
- `ISpecification<T>` — query specification contracts
- `ApiResponse<T>` — standardized API response wrapper
- `FormatResponseFilter` — global response formatter
- `PermissionInterceptor` — global permission enforcement filter
- `UseCommonErrorHandling()` — centralized exception middleware

---

## 🛠️ Tech Stack

### Backend
| Category | Technology |
|---|---|
| Runtime | .NET 8, C# |
| Web Framework | ASP.NET Core Web API |
| API Gateway | Ocelot |
| ORM | Entity Framework Core 8 |
| Database | PostgreSQL (per-service database) |
| Message Broker | RabbitMQ + MassTransit 8 |
| Distributed Cache | Redis (StackExchange.Redis) |
| Object Storage | MinIO (S3-compatible) |
| Authentication | JWT Bearer + BCrypt password hashing |
| Email | MailKit + MimeKit |
| API Documentation | Swagger / Swashbuckle |
| Containerization | Docker + Docker Compose |

### Frontend (Separate Repository)
| Category | Technology |
|---|---|
| Framework | Angular 18 |
| Rendering | Angular SSR (Server-Side Rendering) |
| State Management | NgRx (Store, Effects, DevTools) |
| UI Library | NG-ZORRO (Ant Design for Angular) |
| Rich Text | ngx-quill |
| HTTP | Angular HttpClient + Interceptors |
| Language | TypeScript |

---

## 🗂️ Project Structure

```
MicroserviceShop/
│
├── ApiGateway/                 # Ocelot API Gateway
│   ├── ocelot.json             # Route definitions for all services
│   └── Program.cs
│
├── AggregatorService/          # Data aggregation across services
│
├── AuthService/                # Authentication & authorization
│   ├── Config/                 # JWT & app configuration
│   ├── Controllers/            # Auth, Role, Permission, User endpoints
│   ├── Data/                   # AuthDbContext
│   ├── Repository/             # User, Role, Permission repositories
│   └── Services/               # Business logic (AuthService, TokenService...)
│
├── ProductService/             # Product catalog & media management
│   ├── Consumers/              # RabbitMQ event consumers
│   ├── Utils/                  # MinIO helpers
│   └── Specifications/         # Query specification objects
│
├── OrderService/               # Order management & processing
│   └── Consumers/              # Handles events from other services
│
├── PaymentService/             # Payment processing
│   └── Consumers/
│
├── CustomerService/            # Customer profile management
│
├── NotificationService/        # Email notification via events
│   ├── Consumers/              # Subscribes to domain events
│   ├── Services/               # Email sending logic
│   └── Templates/              # HTML email templates
│
├── ReportService/              # Business analytics & reporting
│
├── CommonService/              # Shared library (NuGet-style project reference)
│   ├── Annotations/            # Custom data annotations
│   ├── Caching/                # Redis cache abstraction
│   ├── Events/                 # Shared domain event contracts
│   ├── Exceptions/             # Custom exception types & middleware
│   ├── Filters/                # Action filters (response formatter, permission)
│   ├── Interface/              # Shared interfaces
│   ├── Models/                 # ApiResponse<T>, pagination models
│   ├── Repository/             # Generic base repository
│   └── Specifications/         # ISpecification<T> base types
│
└── docker-compose.yml          # Full infrastructure stack
```

---

## ⚙️ Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Node.js 20+](https://nodejs.org/) *(for frontend)*

### 1. Start Infrastructure

Spin up all required infrastructure services (PostgreSQL, RabbitMQ, Redis, MinIO) with a single command:

```bash
docker-compose up -d
```

| Service | URL | Credentials |
|---|---|---|
| PostgreSQL | `localhost:5555` | `postgres` / `password` |
| RabbitMQ Management UI | `http://localhost:15673` | `guest` / `guest` |
| Redis | `localhost:6379` | — |
| MinIO Console | `http://localhost:9001` | `minioadmin` / `minioadmin` |

### 2. Apply Database Migrations

Run migrations for each service that has a database:

```bash
# AuthService
dotnet ef database update --project AuthService

# ProductService
dotnet ef database update --project ProductService

# OrderService
dotnet ef database update --project OrderService

# PaymentService
dotnet ef database update --project PaymentService

# CustomerService
dotnet ef database update --project CustomerService
```

### 3. Run the Services

Open a terminal for each service and run:

```bash
cd ApiGateway && dotnet run
cd AuthService && dotnet run
cd ProductService && dotnet run
cd OrderService && dotnet run
cd PaymentService && dotnet run
cd CustomerService && dotnet run
cd NotificationService && dotnet run
cd ReportService && dotnet run
cd AggregatorService && dotnet run
```

> 💡 Use the included `.http` files (e.g., `ProductService.http`) in VS Code with the REST Client extension for quick API testing.

---

## 📬 API Testing

A full **Postman collection** is included at the root of the project:

```
MicroserviceShop.postman_collection.json
```

Import it into [Postman](https://www.postman.com/) to test all available endpoints across all services.

---

## 🔐 Authentication Flow

```
1. POST /api/auth/login  →  Returns Access Token (JWT) + Refresh Token
2. Client stores tokens  →  Sends Authorization: Bearer <token> on every request
3. API Gateway validates JWT  →  Forwards authenticated request to target service
4. Access Token expires  →  Client calls POST /api/auth/refresh to get a new token
5. Refresh Token expires →  User must log in again
```

> The **Transactional Outbox Pattern** in `AuthService` ensures that registration events (e.g., welcome email triggers) are **guaranteed to be published** to RabbitMQ, even in the face of transient failures.

---

## 📄 License

This project is intended for educational and portfolio purposes.
