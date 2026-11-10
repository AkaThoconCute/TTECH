# PROJECT STRUCTURE RULE

## 1. Core Principles

- **Microservices:**
  - Structure the system by independent services
- **Clean Architecture:**
  - Structure each service by independent layers
  - Dependency flows from the outside in
- **Dependency Inversion:**
  - Logic is expressed through 2 layers: Contract and Implement
  - Implement layers depend on Contract layers - no opposite

## 2. Standard Service Directory Template

```text
ServiceFolder/
├── Service.AppHost/                 # Service start up
│   ├── Configuration/               # Configuration Extensions
│   ├── DependencyInjection/         # Service Extensions
│   ├── Migrations/                  # Code First
│   ├── Properties/                  # Launch settings
│   └── Program.cs                   # Entry point
│
├── Service.BusinessContract/        # Business Abstractions
│   ├── DTOs/                        # Request & Response models
│   ├── Enums/                       # Constant sets
│   └── Services/                    # Service Interfaces
│
├── Service.BusinessImplement/       # Pure Business Logic
│   ├── Errors/                      # Service Error Defination
│   ├── Helpers/                     # Concrete Helpers
│   ├── Jobs/                        # Scheduled Jobs (BackgroundService)
│   ├── Options/                     # Config key values
│   ├── Services/                    # Concrete Services
│   └── Templates/                   # Static Template Files
│
├── Service.Controllers/             # HTTP API Endpoints
│
├── Service.RepositoryContract/      # Core Abstractions
│   ├── Entity/                      # Core Entities
│   └── Repository/                  # Repository Interfaces
│
├── Service.RepositoryImplement/     # Pure Core Logic
│   ├── Context/                     # EF Core Database Context
│   └── Repository/                  # Concrete Repositories
│
├── Service.ToolsContract/           # Infrastructure Abstractions
│   └── Services/                    # Tool Service Interfaces (messages are DTOs at the Business layer)
│
├── Service.ToolsImplement/          # Pure Infrastructure (no Azure Functions)
│   ├── Services/                    # Concrete Tool Services (command entry point, e.g. write to a Channel)
│   ├── Channels/                    # In-memory Channels (technology-named, shared by Service and Worker)
│   ├── Workers/                     # BackgroundService workers: drain a Channel (call Business Service method to run logic)
│   ├── Helpers/                     # Concrete Helpers (static, pure)
│   └── Options/                     # Config key values
│
└── Service.Tests/                   # Http, Unit & Integration tests
    ├── HttpTests/                   # Http tests
    ├── IntegrationTests/            # Integration tests (may be empty)
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
| Options              | {Business}Options     |
| Errors               | {Business}Errors      |
| Template             | {Business}Template    |
| Entity               | {Entity}              |
| Repository Interface | I{Business}Repository |
| Repository Implement | {Business}Repository  |
| Controller           | {Business}Controller  |
| Channel              | {Business}Channel     |
| Worker               | {Business}Worker      |
| Job                  | {Business}Job         |
| Test                 | {Target}Tests         |

- `{Target}` is the class or component under test, e.g. `OtpServiceTests`, `RetryUtilTests`.
- Template folders may be empty or absent when the service has nothing for them (e.g. `IntegrationTests/`).
