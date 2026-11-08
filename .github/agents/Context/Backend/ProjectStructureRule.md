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
│   └── Service/                     # Tool Service Interfaces (messages are DTOs at the Business layer)
│
├── Service.ToolsImplement/          # Pure Infrastructure (no Azure Functions)
│   ├── Service/                     # Concrete Tool Services (command entry point, e.g. write to a Channel)
│   ├── Channels/                    # In-memory Channels (technology-named, shared by Service and Worker)
│   ├── Worker/                      # BackgroundService workers: drain a Channel (call Business Service method to run logic)
│   ├── Helper/                      # Concrete Helpers (static, pure)
│   └── Options/                     # Config key values
│
└── Service.Tests/                   # Http, Unit & Integration tests
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
| Channel              | {Business}Channel     |
| Worker               | {Business}Worker      |
| Job                  | {Business}Job         |
| Test                 | {Business}Tests       |

## 4. Tools Layer Rules

- **Folders are grouped by technical role** (`Service`, `Channels`, `Worker`, `Helper`, `Options`), not by feature.
- **Tools is a low layer: name by technology/role** (`Channel`, `Worker`), not by a generic word like `Queue`.
- **One interface per capability:** the Business layer calls `I{Business}Service` in `ToolsContract/Service`. Do not add extra interfaces (sender, producer, event wrapper) for a single implementation.
- **No pass-through wrappers:** the Service writes to the Channel directly.
- **Channel:** only the Service (writer) and the Worker (reader) use it. Registered as a singleton.
- **Worker:** a `BackgroundService` that reads from a Channel. Delivery details (retry, transport call) stay inside the Worker and Helper; recipients and payload bodies are never logged.
- **Contract assemblies hold only interfaces, DTOs and enums**, never concrete classes (a Channel is a concrete class, so it lives in `ToolsImplement`).
- **Command vs event naming:**
  - A command ("do this", one handler) is named by capability: `I{Business}Service.{Action}Async`.
  - A real domain event ("this happened", 0..n subscribers) is named `I{Business}EventPublisher` with a past-tense event DTO, placed in `ToolsContract/Service` and `BusinessContract/DTOs`.
