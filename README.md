# Microservices with Eventing

A .NET solution containing three microservices: Account, Audit, and BusinessLogic APIs.

## Project Structure

```
├── Account.API/              # Account management microservice
├── Audit.API/               # Audit logging microservice  
├── BusinessLogic.API/       # Business transaction microservice
├── Shared.Models/           # Shared data models and DTOs
├── docker-compose.yml       # Docker orchestration
└── MicroservicesWithEventing.sln
```

## Services

### Account.API (Port 5001)
- **Purpose**: Manages user accounts and personal information
- **Database**: SQL Server with Entity Framework Core
- **Schema**: `account` schema with User and Person tables
- **User Endpoints**:
  - `GET /api/users` - Get all active users
  - `GET /api/users/{id}` - Get user by ID
  - `POST /api/users` - Create new user
  - `PUT /api/users/{id}` - Update user
  - `DELETE /api/users/{id}` - Soft delete user
- **Person Endpoints**:
  - `GET /api/persons` - Get all persons
  - `GET /api/persons/{id}` - Get person by ID
  - `GET /api/persons/user/{userId}` - Get person by user ID
  - `POST /api/persons` - Create new person
  - `PUT /api/persons/{id}` - Update person
  - `DELETE /api/persons/{id}` - Delete person
- **Legacy Endpoints** (Deprecated):
  - `GET /api/accounts` - Returns empty list (use /api/users instead)
  - `POST /api/accounts` - Returns 501 (use /api/users instead)

### Audit.API (Port 5002)
- **Purpose**: Logs audit events across the system
- **Endpoints**:
  - `GET /api/auditevents` - Get all audit events
  - `GET /api/auditevents/{id}` - Get audit event by ID
  - `POST /api/auditevents` - Create new audit event
  - `GET /api/auditevents/entity/{entityType}/{entityId}` - Get events for specific entity
  - `GET /api/auditevents/user/{userId}` - Get events for specific user

### BusinessLogic.API (Port 5003)
- **Purpose**: Handles business transactions
- **Endpoints**:
  - `GET /api/transactions` - Get all transactions
  - `GET /api/transactions/{id}` - Get transaction by ID
  - `POST /api/transactions` - Create new transaction
  - `GET /api/transactions/account/{accountId}` - Get transactions for account
  - `GET /api/transactions/type/{transactionType}` - Get transactions by type

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB or full instance)
- Docker (optional)

### Running Locally

1. **Build the solution**:
   ```bash
   dotnet build
   ```

2. **Set up the database** (for Account.API):
   ```bash
   dotnet ef database update --project Account.API
   ```

3. **Run all services**:
   ```bash
   dotnet run --project Account.API
   dotnet run --project Audit.API  
   dotnet run --project BusinessLogic.API
   ```

4. **Access Swagger UI**:
   - Account API: https://localhost:5001/swagger
   - Audit API: https://localhost:5002/swagger
   - BusinessLogic API: https://localhost:5003/swagger

### Running with Docker

1. **Build and run all services**:
   ```bash
   docker-compose up --build
   ```

2. **Access services**:
   - Account API: http://localhost:5001
   - Audit API: http://localhost:5002
   - BusinessLogic API: http://localhost:5003

## API Documentation

Each service includes Swagger/OpenAPI documentation available at `/swagger` endpoint when running in development mode.

## Database Schema

### Account.API Database
- **Database**: `AccountDb`
- **Schema**: `account`
- **Tables**:
  - `Users` - User login information (username, email, password hash, etc.)
  - `Persons` - Personal information (name, address, phone, etc.)
  - One-to-one relationship between User and Person

## Shared Models

The `Shared.Models` project contains common data models used across all services:
- `Account` - Account information (legacy)
- `AuditEvent` - Audit logging events
- `BusinessTransaction` - Business transaction data

## Development Notes

- Account.API uses SQL Server with Entity Framework Core
- Audit.API and BusinessLogic.API use in-memory storage for simplicity
- CORS is enabled for cross-origin requests
- Services are configured for development with Swagger UI
- Docker configuration includes multi-stage builds for optimized images
- Database migrations are included for Account.API schema management