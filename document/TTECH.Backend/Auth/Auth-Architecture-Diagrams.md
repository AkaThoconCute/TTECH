# Auth Service – Architecture Diagrams

Derived from the `*.csproj` references and `DependencyInjection.cs` in `source/TTECH.Backend/Auth`.

## 1. Package Diagram (compile-time project references, grouped by layer)

Arrow = "references" (A --> B means A depends on B).

```mermaid
flowchart BT
  subgraph Host["Host layer"]
    AppHost["Auth.AppHost<br/>(composition root, DI, EF migrations)"]
  end

  subgraph Presentation["Presentation layer"]
    Controllers["Auth.Controllers"]
  end

  subgraph Business["Business layer"]
    BC["Auth.BusinessContract<br/>(DTOs, Enums, service interfaces)"]
    BI["Auth.BusinessImplement<br/>(Services, Helpers, Options, Jobs, Errors)"]
  end

  subgraph Tools["Tools / infrastructure layer"]
    TC["Auth.ToolsContract<br/>(IEmailService, ServiceBus)"]
    TI["Auth.ToolsImplement<br/>(Email service, EmailEvent, SendEmailFunction, Options)"]
  end

  subgraph Data["Data layer"]
    RC["Auth.RepositoryContract<br/>(Entities, repository interfaces)"]
    RI["Auth.RepositoryImplement<br/>(EF Core DbContext, repositories)"]
  end

  subgraph Shared["Shared kernel"]
    Common["Common"]
    CommonWeb["Common.Web<br/>(HealthCheck)"]
  end

  AppHost --> Controllers
  AppHost --> BI
  AppHost --> RI
  AppHost --> CommonWeb

  Controllers --> BC
  BI --> BC
  BI --> TC
  BI --> TI
  BC --> RC
  BC --> Common
  TC --> BC
  TI --> TC
  RI --> RC
  RI --> TC
```

Observations:

- Contract/Implement split holds for Business and Repository: Controllers depend only on `BusinessContract`.
- `AppHost` is the only project that references implementations and wires them via DI.
- `BusinessImplement -> ToolsImplement` is a direct implementation reference, which bypasses the contract and breaks the inversion. `ToolsContract -> BusinessContract` also makes the contract layers circular in intent. Worth reviewing.

## 2. C4 Container Diagram (runtime view)

Auth runs as one deployable ASP.NET Core process. The containers below are the runtime units, and the in-process hosted services are shown separately because they run on background threads.

```mermaid
flowchart TB
  user(["Client / Web App<br/>[Person]"])
  smtp[("SMTP server<br/>[External system]<br/>MailKit, STARTTLS :587")]

  subgraph boundary["Auth Service [Software system]"]
    api["Auth API<br/>[Container: ASP.NET Core Web API]<br/>Auth.AppHost + Controllers<br/>Auth / Otp / Health endpoints, JWT issuing"]
    biz["Business logic<br/>[Component set, in-process]<br/>Auth, User, Token, Otp services"]
    otpjob["OtpCleanupJob<br/>[Container: HostedService]<br/>Purges expired OTP codes"]
    mailworker["SendEmailFunction<br/>[Container: HostedService]<br/>Drains EmailQueue, sends mail"]
    queue["EmailQueue<br/>[In-memory queue, singleton]"]
    db[("Auth database<br/>[Container: SQL Server]<br/>Identity users, OTP codes<br/>EF Core")]
  end

  user -- "HTTPS / JSON" --> api
  api -- "calls" --> biz
  biz -- "EF Core / SQL" --> db
  biz -- "enqueue email" --> queue
  queue -- "dequeue" --> mailworker
  mailworker -- "SMTP" --> smtp
  otpjob -- "EF Core / SQL (periodic)" --> db
```

Configuration sources (`appsettings.json`): `ConnectionStrings:Database`, `JWT`, `Otp` (`OtpOptions`), `Email` (`EmailOptions`).
