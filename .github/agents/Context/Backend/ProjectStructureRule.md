# PROJECT STRUCTURE RULE

## 1. Core Principles

- **Microservices:**
  - Structure the project by independent modules
- **Clean Artchitecture:**
  - Structure the module by independent layers
  - Dependency flows from the inside out
- **Dependency Inversion:**
  - Logic is expressed through 2 layers: Constract and Implement
  - Implement layers depend on Constract layers - no opposite

## 2. Standard Module Directory Template

```text
ModuleFolder/
├── Module.AppHost/                  # Module start up
│   ├── Configuration/               # Configuration Extensions
│   ├── DependencyInjection/         # Service Extensions
│   ├── Migrations/                  # Code First
│   ├── Properties/                  # Launch settings
│   └── Program.cs                   # Entry point
│
├── Module.BusinessContract/         # Business Abstractions
│   ├── DTOs/                        # Request & Response models
│   ├── Enums/                       # Constant sets
│   └── Services/                    # Service Interfaces
│
├── Module.BusinessImplement/        # Pure Business Logic
│   ├── Errors/                      # Service Error Defination
│   ├── Helpers/                     # Concrete Helpers
│   ├── Options/                     # Config key values
│   ├── Services/                    # Concrete Services
│   └── Templates/                   # Static Template Files
│
├── Module.Controllers/              # HTTP API Endpoints
│
├── Module.RepositoryContract/       # Core Abstractions
│   ├── Entity/                      # Core Entities
│   └── Repository/                  # Repository Interfaces
│
├── Module.RepositoryImplement/      # Pure Core Logic
│   ├── Context/                     # EF Core Database Context
│   └── Repository/                  # Concrete Repositories
│
├── Module.ToolsContract/            # Infrastructure Abstractions
│   ├── Messaging/                   # Publisher Interfaces
│   └── Services/                     # Service Interfaces (Events is DTOs at the Business layer)
│
├── Module.ToolsImplement/           # Pure Infrastructure (no Azure Functions)
│   ├── Queue/                       # In-memory Queues
│   ├── Consumers/                   # Consumer Workers (It call Business Service method to run logic)
│   ├── Jobs/                        # Job Workers      (It call Business Service method to run logic)
│   ├── Messaging/                   # Concrete Publisher
│   ├── Helpers/                     # Concrete Helpers
│   ├── Options/                     # Config key values
│   └── Services/                    # Concrete Services
│
└── Module.Tests/                    # Http, Unit & Integration tests
    ├── HttpTests/                   # Http tests
    ├── IntegrationTests/            # Integration tests
    └── UnitTests/                   # Unit tests
```

## 3. File Naming

| File type            | File name format      |
| -------------------- | --------------------- |
| Service Interface    | I{Business}Service    |
| Service Implement    | {Business}ServiceImpl |
| DTO                  | {Business}DTO         |
| Enum                 | {Business}Enum        |
| Helper               | {Business}Helper      |
| Option               | {Business}Option      |
| Template             | {Business}Template    |
| Entity               | {Entity}              |
| Repository Interface | I{Business}Repository |
| Repository Implement | {Business}Repository  |
| Controller           | {Business}Controller  |
| Queue                | {Business}Queue       |
| Consumer             | {Business}Consumer    |
| Job                  | {Business}Job         |
| Publisher Interface  | I{Business}Publisher  |
| Publisher Implement  | {Business}Publisher   |
| Test                 | {Business}Tests       |
