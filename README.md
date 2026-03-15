# Lendora

A production-grade fintech lending platform backend built with **.NET 10**, **PostgreSQL**, and **Redis**. Lendora models the complete lifecycle of a loan application -- from submission and risk evaluation through approval, repayment tracking, and delinquency detection.

## Table of Contents

- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Domain Model](#domain-model)
- [Loan Lifecycle State Machine](#loan-lifecycle-state-machine)
- [Risk Engine](#risk-engine)
- [API Endpoints](#api-endpoints)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Running with Docker](#running-with-docker)
- [Database Migrations](#database-migrations)
- [Central Package Management](#central-package-management)
- [Key Design Decisions](#key-design-decisions)

---

## Architecture

Lendora follows **Clean Architecture** organized as a **Modular Monolith** with strict layer separation:

```
┌─────────────────────────────────────────────────────┐
│                   Lendora.Api                       │
│          Controllers, Middleware, Contracts          │
├─────────────────────────────────────────────────────┤
│               Lendora.Application                   │
│      CQRS Commands/Queries, Handlers, Validators    │
├─────────────────────────────────────────────────────┤
│              Lendora.Infrastructure                 │
│    EF Core, Repositories, Redis, Background Jobs    │
├─────────────────────────────────────────────────────┤
│                 Lendora.Domain                      │
│     Entities, State Machines, Events, Exceptions    │
└─────────────────────────────────────────────────────┘
```

**Data flow:**

```
HTTP Request → Controller → MediatR Send → Handler → Domain Entity → Repository → Database
                                              ↓
                                    Domain Events → MediatR Publish
```

**Dependency rule:** Dependencies point inward. Domain has zero framework dependencies (except Stateless). Application depends only on Domain. Infrastructure implements Application interfaces. API wires everything together.

---

## Tech Stack

| Technology | Purpose |
|---|---|
| .NET 10 / ASP.NET Core | Runtime & web framework |
| PostgreSQL 16 | Primary database |
| Redis 7 | Distributed caching |
| Entity Framework Core 10 | ORM & data access |
| MediatR 14 | CQRS command/query dispatch & domain event publishing |
| FluentValidation 12 | Request validation via MediatR pipeline |
| Stateless 5 | State machine for loan lifecycle transitions |
| Serilog 10 | Structured logging |
| Swashbuckle 10 | OpenAPI / Swagger documentation |
| Docker & Docker Compose | Containerized deployment |

---

## Project Structure

```
Lendora/
├── Directory.Build.props           # Shared MSBuild properties (TFM, nullable, etc.)
├── Directory.Packages.props        # Central NuGet package version management
├── Lendora.slnx                    # Solution file
├── nuget.config                    # NuGet source configuration
├── Dockerfile                      # Multi-stage Docker build
├── docker-compose.yml              # Full stack: API + PostgreSQL + Redis
├── .dockerignore
│
└── src/
    ├── Lendora.Domain/             # Entities, enums, state machines, domain events
    │   ├── Common/                 # BaseEntity, AggregateRoot, IDomainEvent
    │   ├── Entities/               # LoanApplication, Loan, RepaymentInstallment, Payment, RiskAssessment
    │   ├── Enums/                  # LoanApplicationStatus, LoanStatus, InstallmentStatus
    │   ├── Events/                 # 8 domain event records
    │   └── Exceptions/             # DomainException, InvalidStateTransitionException
    │
    ├── Lendora.Application/        # CQRS handlers, validators, pipeline behaviors
    │   ├── Common/
    │   │   ├── Behaviors/          # ValidationBehavior, LoggingBehavior
    │   │   └── Interfaces/         # IRepository, IUnitOfWork, ICacheService, etc.
    │   ├── LoanApplications/
    │   │   ├── Commands/           # Submit, EvaluateRisk, Approve, Reject
    │   │   └── Queries/            # GetLoanApplication
    │   ├── Loans/
    │   │   ├── Commands/           # RegisterPayment, MarkInstallmentAsLate
    │   │   └── Queries/            # GetLoanStatus, GetCustomerLoans, GetRepaymentSchedule
    │   ├── Validators/             # FluentValidation validators
    │   └── DependencyInjection.cs
    │
    ├── Lendora.Infrastructure/     # Data access, caching, background services
    │   ├── Persistence/
    │   │   ├── Configurations/     # EF Core Fluent API entity configurations
    │   │   ├── Repositories/       # LoanApplicationRepository, LoanRepository
    │   │   ├── LendoraDbContext.cs  # DbContext with domain event dispatch
    │   │   └── DomainEventNotification.cs
    │   ├── Caching/                # RedisCacheService
    │   ├── Services/               # DateTimeProvider
    │   ├── BackgroundJobs/         # LatePaymentMonitoringWorker
    │   └── DependencyInjection.cs
    │
    └── Lendora.Api/                # HTTP layer
        ├── Controllers/            # LoanApplications, Loans, Payments
        ├── Contracts/Requests/     # Request DTOs
        ├── Middleware/             # GlobalExceptionHandlerMiddleware
        ├── Program.cs
        ├── appsettings.json
        └── Dockerfile
```

---

## Domain Model

### Entities

| Entity | Type | Description |
|---|---|---|
| **LoanApplication** | Aggregate Root | Tracks a customer's loan request through evaluation and approval |
| **Loan** | Aggregate Root | Represents an approved, active loan with repayment schedule |
| **RepaymentInstallment** | Entity | A single scheduled payment within a loan's amortization schedule |
| **Payment** | Entity | Records an actual payment made against a loan |
| **RiskAssessment** | Entity | Stores the risk evaluation result for an application |

### Entity Relationship Diagram

```
┌─────────────────────┐         ┌──────────────────────┐
│   LoanApplication   │ 1───N   │   RiskAssessment     │
│─────────────────────│         │──────────────────────│
│ CustomerId           │         │ ApplicationId        │
│ RequestedAmount      │         │ Score                │
│ RequestedTermMonths  │         │ Decision             │
│ AnnualIncome         │         │ EvaluatedAt          │
│ ExistingMonthlyDebt  │         └──────────────────────┘
│ Status               │
│ RiskScore            │
│ ApprovedAmount       │
│ InterestRate         │
└─────────┬───────────┘
          │ 1:1 (on approval)
          ▼
┌─────────────────────┐         ┌──────────────────────┐
│       Loan          │ 1───N   │ RepaymentInstallment │
│─────────────────────│         │──────────────────────│
│ ApplicationId        │         │ LoanId               │
│ CustomerId           │         │ InstallmentNumber    │
│ ApprovedAmount       │  1───N  │ DueDate              │
│ InterestRate         │────┐    │ PrincipalAmount      │
│ TermMonths           │    │    │ InterestAmount       │
│ MonthlyPayment       │    │    │ TotalAmount          │
│ StartDate            │    │    │ PaidAmount           │
│ Status               │    │    │ Status               │
└─────────────────────┘    │    └──────────────────────┘
                            │
                            │    ┌──────────────────────┐
                            └──▶ │      Payment         │
                                 │──────────────────────│
                                 │ LoanId               │
                                 │ Amount               │
                                 │ PaidAt               │
                                 │ PaymentReference     │
                                 └──────────────────────┘
```

---

## Loan Lifecycle State Machine

Lendora uses the [Stateless](https://github.com/dotnet-state-machine/stateless) library to enforce valid state transitions at the domain level.

### Application Lifecycle

```
┌───────────┐   Evaluate   ┌────────────────┐   Approve   ┌──────────┐
│ Submitted │──────────────▶│ RiskEvaluating │────────────▶│ Approved │
└───────────┘               └───────┬────────┘             └──────────┘
                                    │ Reject
                                    ▼
                              ┌──────────┐
                              │ Rejected │
                              └──────────┘
```

### Loan Lifecycle

```
                    ┌──────────────┐
              ┌────▶│  Delinquent  │────┐
              │     └──────────────┘    │
              │ MarkDelinquent     Close│
              │                         ▼
        ┌─────┴──┐                ┌──────────┐
        │ Active │───────────────▶│  Closed  │
        └────────┘     Close      └──────────┘
```

Invalid transitions throw `InvalidStateTransitionException`, which the API maps to **409 Conflict**.

---

## Risk Engine

The built-in risk scoring engine evaluates applications based on the customer's financial profile:

```
Score = 850 - (DebtToIncomeRatio x 200) - (AmountToIncomeRatio x 100) - TermPenalty
```

| Factor | Calculation |
|---|---|
| **Debt-to-Income Ratio** | `ExistingMonthlyDebt / (AnnualIncome / 12)` |
| **Amount-to-Income Ratio** | `RequestedAmount / AnnualIncome` |
| **Term Penalty** | 0 (<=12mo), 10 (<=36mo), 25 (<=60mo), 50 (<=120mo), 75 (<=240mo), 100 (>240mo) |

The score is clamped to **300 - 850** and produces an automated decision:

| Score Range | Decision |
|---|---|
| >= 650 | **Approve** -- eligible for approval |
| 500 - 649 | **Manual Review** -- stays in RiskEvaluating |
| < 500 | **Reject** -- automatically rejected |

---

## API Endpoints

All endpoints are prefixed with `/api/v1`.

### Loan Applications

| Method | Endpoint | Description | Success |
|---|---|---|---|
| `POST` | `/loan-applications` | Submit a new application | `201 Created` |
| `POST` | `/loan-applications/{id}/evaluate` | Trigger risk evaluation | `200 OK` |
| `POST` | `/loan-applications/{id}/approve` | Approve with terms | `200 OK` |
| `POST` | `/loan-applications/{id}/reject` | Reject with reason | `200 OK` |
| `GET` | `/loan-applications/{id}` | Get application details | `200 OK` |

### Loans

| Method | Endpoint | Description | Success |
|---|---|---|---|
| `GET` | `/loans/{id}` | Get loan status & balance | `200 OK` |
| `GET` | `/loans/{id}/schedule` | Get repayment schedule | `200 OK` |
| `GET` | `/loans/customer/{customerId}` | Get all loans for a customer | `200 OK` |

### Payments

| Method | Endpoint | Description | Success |
|---|---|---|---|
| `POST` | `/payments` | Register a payment | `201 Created` |

### Error Responses

All errors return [RFC 9457 Problem Details](https://www.rfc-editor.org/rfc/rfc9457):

| Status | Cause |
|---|---|
| `400 Bad Request` | Validation failure or domain rule violation |
| `404 Not Found` | Entity not found |
| `409 Conflict` | Invalid state transition |
| `500 Internal Server Error` | Unhandled exception |

### Example: Full Loan Lifecycle

```bash
# 1. Submit application
curl -X POST http://localhost:8080/api/v1/loan-applications \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "550e8400-e29b-41d4-a716-446655440000",
    "requestedAmount": 25000,
    "requestedTermMonths": 36,
    "annualIncome": 85000,
    "existingMonthlyDebt": 500
  }'
# Returns: { "id": "<application-id>" }

# 2. Evaluate risk
curl -X POST http://localhost:8080/api/v1/loan-applications/<application-id>/evaluate
# Returns: { "score": 712, "decision": "Approve", "status": "RiskEvaluating" }

# 3. Approve application (creates loan + repayment schedule)
curl -X POST http://localhost:8080/api/v1/loan-applications/<application-id>/approve \
  -H "Content-Type: application/json" \
  -d '{
    "approvedBy": "underwriter-001",
    "interestRate": 8.5,
    "approvedAmount": 25000,
    "approvedTermMonths": 36
  }'
# Returns: { "applicationId": "...", "loanId": "<loan-id>" }

# 4. View repayment schedule
curl http://localhost:8080/api/v1/loans/<loan-id>/schedule

# 5. Register a payment
curl -X POST http://localhost:8080/api/v1/payments \
  -H "Content-Type: application/json" \
  -d '{
    "loanId": "<loan-id>",
    "amount": 789.50,
    "paidAt": "2026-04-01T10:00:00Z",
    "paymentReference": "PAY-20260401-001"
  }'

# 6. Check loan status
curl http://localhost:8080/api/v1/loans/<loan-id>
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://www.docker.com/get-started) & Docker Compose (for PostgreSQL and Redis)

### Run Locally

```bash
# 1. Start infrastructure (PostgreSQL + Redis)
docker compose up -d postgres redis

# 2. Apply EF Core migrations
dotnet ef database update --project src/Lendora.Infrastructure --startup-project src/Lendora.Api

# 3. Run the API
dotnet run --project src/Lendora.Api
```

The API will be available at `http://localhost:5000` (or the port configured in `launchSettings.json`).

Swagger UI: `http://localhost:5000/swagger`

### Build

```bash
dotnet build
```

### Run Tests

```bash
dotnet test
```

---

## Configuration

Configuration is managed via `appsettings.json` and environment variables:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=lendora;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning"
      }
    }
  },
  "LatePaymentMonitoring": {
    "IntervalMinutes": 60
  }
}
```

Environment variable overrides follow the standard `__` separator pattern:

```bash
ConnectionStrings__DefaultConnection="Host=myserver;..."
ConnectionStrings__Redis="redis-server:6379"
```

---

## Running with Docker

The full stack runs with a single command:

```bash
docker compose up --build
```

This starts:

| Service | Port | Description |
|---|---|---|
| `lendora-api` | `8080` | ASP.NET Core API |
| `postgres` | `5432` | PostgreSQL 16 with `lendora` database |
| `redis` | `6379` | Redis 7 Alpine |

Swagger UI: `http://localhost:8080/swagger`

Health check: `http://localhost:8080/health`

To stop:

```bash
docker compose down        # Stop containers
docker compose down -v     # Stop and remove volumes (resets database)
```

---

## Database Migrations

Migrations use EF Core CLI tools:

```bash
# Create a new migration
dotnet ef migrations add <MigrationName> \
  --project src/Lendora.Infrastructure \
  --startup-project src/Lendora.Api \
  --output-dir Persistence/Migrations

# Apply migrations
dotnet ef database update \
  --project src/Lendora.Infrastructure \
  --startup-project src/Lendora.Api

# Revert last migration
dotnet ef migrations remove \
  --project src/Lendora.Infrastructure \
  --startup-project src/Lendora.Api
```

---

## Central Package Management

All NuGet package versions are managed centrally using [Central Package Management (CPM)](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management).

**`Directory.Build.props`** -- Shared MSBuild properties applied to all projects:

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
</Project>
```

**`Directory.Packages.props`** -- Single source of truth for all package versions:

```xml
<Project>
  <ItemGroup Label="Domain">
    <PackageVersion Include="Stateless" Version="5.20.1" />
  </ItemGroup>
  <ItemGroup Label="Application">
    <PackageVersion Include="MediatR" Version="14.1.0" />
    <PackageVersion Include="FluentValidation" Version="12.1.1" />
    ...
  </ItemGroup>
  <ItemGroup Label="Infrastructure">
    <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="10.0.5" />
    <PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.1" />
    ...
  </ItemGroup>
  <ItemGroup Label="Api">
    <PackageVersion Include="Serilog.AspNetCore" Version="10.0.0" />
    <PackageVersion Include="Swashbuckle.AspNetCore" Version="10.1.5" />
    ...
  </ItemGroup>
</Project>
```

Individual `.csproj` files reference packages **without versions** -- the version is resolved from `Directory.Packages.props`:

```xml
<PackageReference Include="MediatR" />
```

To update a package version, change it in `Directory.Packages.props` only.

---

## Key Design Decisions

### CQRS via MediatR

Commands and queries are dispatched through MediatR with two pipeline behaviors:

1. **`ValidationBehavior`** -- Runs FluentValidation validators before the handler executes. Throws `ValidationException` on failure.
2. **`LoggingBehavior`** -- Logs request name, start time, and elapsed duration.

Commands return result types; queries return DTOs. Domain entities never leak to the API surface.

### State Machine in Domain Entities

The Stateless state machine lives inside the aggregate root (`LoanApplication`, `Loan`), not in the Application layer. Entities expose intent-revealing methods (`Approve()`, `Reject()`, `MarkAsDelinquent()`) that fire triggers internally. This keeps state transition logic co-located with the domain invariants it protects.

### Domain Event Dispatch

Domain events implement `IDomainEvent` (standalone interface -- no MediatR dependency in the Domain layer). The `LendoraDbContext.SaveChangesAsync()` collects events from tracked aggregates, wraps each in a `DomainEventNotification<T> : INotification`, and publishes via MediatR **after** persistence succeeds.

### Repository Pattern

- `IRepository<T>` for write operations on aggregate roots
- Specialized interfaces (`ILoanApplicationRepository`, `ILoanRepository`) add aggregate-specific query methods
- `IUnitOfWork.SaveChangesAsync()` is called by handlers, not repositories

### Caching Strategy

- Redis caches **read query results** only: loan status (5 min TTL), repayment schedule (1 hr TTL)
- Cache keys: `loan:{id}:status`, `loan:{id}:schedule`
- Invalidation on write: `RegisterPaymentCommandHandler` removes affected cache entries

### Late Payment Monitoring

A `BackgroundService` runs on a configurable interval (default: 60 minutes). It queries installments past due, marks them as `Late`, and transitions the parent loan to `Delinquent` status.

---

## License

This project is for educational and demonstration purposes.
