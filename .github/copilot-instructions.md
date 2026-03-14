# Regira Codebase — Copilot Instructions

**Primary goal:** When a request relates to a specific module below, consult the corresponding instruction file for detailed, context-specific guidance. Fall back to the [General Instructions](#general-instructions) only when no module instruction file applies.

Don't assume syntax or signatures! 
When you have to guess and the instruction files don't give you the required information: 
- Stop and describe the problem!
- ASK feedback from the user!

---

## Module Instructions

This codebase provides specialized AI instruction sets for its modules. **When a user's request relates to a Regira module, load the corresponding instruction file and follow its guidance precisely.** Follow the module's instruction file as the primary source. Apply General Instructions only for topics not covered by the module guide, or when strictly necessary to fulfil the request.

### Available Modules

| Module | Namespace | Covers | Instructions |
|--------|-----------|--------|--------------| 
| **Project Templates** | *(scaffolding)* | Scaffolding new projects from reusable starter templates | `./project.setup.md` |
| **Entities** | `Regira.Entities` | CRUD Services with built-in extras | `./entities.instructions.md` |
| **IO.Storage** | `Regira.IO.Storage` | File storage: local filesystem, Azure Blob, SFTP/SSH, GitHub, TCP, compression | *(not yet available)* |
| **Office** | `Regira.Office` | Document processing: Excel, Word, PDF, OCR, Barcodes, CSV, vCards | *(not yet available)* |
| **Media** | `Regira.Media` | Image & video processing: drawing, resize/crop/rotate, FFmpeg | *(not yet available)* |
| **Invoicing** | `Regira.Invoicing` | Invoice creation and integrations (Billit, UBL, ViaAdValvas) | *(not yet available)* |
| **Payments** | `Regira.Payments` | Payment provider integrations (Mollie, Pom) | *(not yet available)* |
| **Security** | `Regira.Security` | Authentication, hashing (BCrypt), cryptography | *(not yet available)* |
| **Web** | `Regira.Web` | Web utilities: HTML generation (RazorLight), Swagger, mail providers | *(not yet available)* |
| **System** | `Regira.System` | Hosting utilities, project/solution file tooling | *(not yet available)* |

When a module's instruction file is not yet available, apply general .NET best practices and the conventions in this file.

---

### Project Templates

**Instructions:** `./project.setup.md`

Reusable starter templates for bootstrapping new .NET projects. Covers four templates:

- **`ConsoleWithLogging`** — standalone console app with Serilog, DI, and configuration
- **`BasicApi`** — standard ASP.NET Core API (IIS / Azure / Docker) with controllers and Minimal API endpoints, Serilog, and Scalar UI
- **`SelfHostingMinimalApi`** — self-hosted Kestrel API with Minimal API endpoints, Swagger + Scalar, and optional Windows Service support
- **`SelfHostingApiWithAuth`** — self-hosted API with API key and JWT Bearer authentication, OpenAPI security transformers, and Swagger + Scalar

**Load these instructions when the user asks to** scaffold a new project, create a new console app or API, set up a Windows Service, or choose between hosting and authentication options.

---

### Regira Entities

**Instructions:** `./entities.instructions.md`

A generic, extensible framework for building ASP.NET Core APIs with standardized CRUD operations on top of EF Core. It covers the full stack from database to HTTP endpoint:

- **Entity Project setup** — scaffolding a new ASP.NET Core Web API with EF Core, NuGet config, `DbContext`, `Program.cs`, and DI wiring
- **Entity modeling** — POCO entity classes, marker interfaces (`IHasTimestamps`, `IArchivable`, `ISortable`, …), `SearchObject` for filtering, `SortBy` enum, `Includes` flags enum, and DTOs
- **CRUD service layer** — `IEntityService` / `EntityRepository` backed by EF Core; custom query filters, sorting, navigation-property includes, processors (post-fetch enrichment), preppers (pre-save preparation), and primers (EF Core `SaveChanges` interceptors)
- **DTO mapping** — Mapster or AutoMapper integration with inline or class-based `AfterMapper` for computed properties
- **API controllers** — `EntityControllerBase` variants that expose standardized List / Search / Details / Save / Delete endpoints
- **Text search & normalization** — `[Normalized]` attribute, `IEntityNormalizer`, and keyword helpers for diacritic-insensitive full-text search
- **File attachments** — `EntityAttachment` model, attachment controllers, and pluggable file storage backends (local filesystem, Azure Blob, SFTP)
- **Soft delete, audit trails, caching** — built-in and custom wrappers using `EntityWrappingServiceBase`
- **Troubleshooting** — diagnosing runtime issues in Entities-based projects

**Load these instructions when the user's request is about any of the above**, including when building a new data-driven API, adding or changing an entity in an existing project, or fixing issues in an Entities-based solution.

---

## General Instructions

Apply the following conventions when no module instruction file applies, or as a supplement when an instruction file does not cover the topic.

### Project Conventions

- **Target frameworks**: Use the latest .NET framework for new projects unless the user specifies otherwise.
- **Language version**: use the latest C# features (primary constructors, collection expressions, pattern matching, etc.)
- **File-scoped namespaces**: `namespace My.Something;` (not block form)
- **NuGet feed**: Regira packages are published at `https://packages.regira.com/v3/index.json` — add this source to `NuGet.Config` alongside the default nuget.org feed

### Naming

Follow standard C# naming conventions. Additionally:
- Names must be **descriptive** but concise — avoid cryptic abbreviations and overly long identifiers
- Generic type parameters: use a meaningful `T`-prefixed name (`TEntity`, `TKey`, `TDto`) rather than a bare single-letter `T` where context allows
- Variable and Property names can be generic, when the context is clear. 
  For example, in a `ProductService` class, a method parameter named `item` or `Items` is perfectly clear. This even enables to use of generic interfaces.
  However, when referring to a specific entity type, it's mandatory to use the entity name (e.g. `category`).
 
### Dependency Injection

- Use `Microsoft.Extensions.DependencyInjection`
- Register services via extension methods on `IServiceCollection`, grouped by feature
- Avoid `ServiceLocator` patterns unless a framework explicitly requires it

### Testing

- Choose the best-suited framework for the purpose and context
- Keep tests small and focused

### SOLID Principles

Always apply SOLID as a default design guide:

| Principle | Rule |
|-----------|------|
| **S** — Single Responsibility | A class or method does one thing only. Split when concerns diverge. |
| **O** — Open / Closed | Extend behavior through new classes or configuration; avoid modifying existing stable code. |
| **L** — Liskov Substitution | Subtypes must be usable wherever the base type is expected — no surprising behavior changes. |
| **I** — Interface Segregation | Prefer small, focused interfaces over large, catch-all ones. Only expose what callers need. |
| **D** — Dependency Inversion | Depend on abstractions (`IService`), not concrete implementations. Inject dependencies; don't `new` them up inside business logic. |

### Code Quality

**Keep it simple.** The simplest solution that correctly solves the problem is always preferred. Avoid over-engineering, premature abstraction, and unnecessary indirection.

- If a solution feels complex, it probably is — step back and look for the simpler path
- Solve the current problem; do not add flexibility or abstractions for imagined future requirements
- Use `null!` only when the value is guaranteed by DI/framework (e.g. `DbSet<T>` properties)
- Apply `[Required]`, `[MaxLength]`, `[Range]` on entity/DTO properties — these drive both validation and EF schema
