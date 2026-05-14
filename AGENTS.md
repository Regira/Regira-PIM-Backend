# Regira Consumer Bootstrap

Use this file as the authoritative downstream bootstrap for choosing the project template, selecting Regira packages, and generating code inside a consumer repository.

This file is also the top-level Regira routing guide inside the source repository. In a consumer repository, use this file plus any local `regira/instructions/*.md` guides as the available sources of truth.

## Pre-flight checklist

Run this checklist before any code generation:

- [ ] `NuGet.Config` includes the Regira feed `https://packages.regira.com/v3/index.json` alongside `nuget.org`
- [ ] `dotnet restore` succeeded when package changes or first-time setup required it
- [ ] `dotnet build` succeeded when installed Regira packages were expected to extract local AI guides
- [ ] `.regira/instructions/` was checked for extracted `*.instructions.md` files and relevant setup references at the solution root (or project root when building standalone)
- [ ] If `.regira/instructions/project.setup.md` exists locally, it was read before generating project shape, hosting, logging, authentication, or OpenAPI/UI setup
- [ ] Every extracted primary guide relevant to the selected modules (`project.setup.md`, `shared.setup.md`, matching `*.instructions.md`) was read in full before writing application code in that area
- [ ] Deep references such as `*.setup.md`, `*.examples.md`, `*.signatures.md`, and `*.namespaces.md` were consulted on demand by section when the current task needed them

Only proceed to project scaffolding, infrastructure changes, or domain code once all applicable checks are satisfied.

## Default workflow

Ask the user what they're building if it isn't already clear, then follow the **Code generation workflow** below. For an existing project, inspect current `*.csproj` files before choosing packages or scaffolding. Prefer project-local instructions over shared Regira guidance when both exist, and ask for feedback rather than guessing missing APIs or conventions.

If the project contains `.regira/instructions/*.md`, treat the extracted shared setup guides plus the matching module guides as the primary local instructions. Regira packages that carry AI files embed them inside the NuGet package under `ai/`. During `dotnet build`, their bundled `.props` and `.targets` files ensure those files are not included as project items and copy them into `.regira/instructions/` at the solution root (`$(SolutionDir)`), falling back to the project root when building standalone. Use `Regira.Setup` when the consumer needs local copies of `project.setup.md` and `shared.setup.md` through the same package-extraction workflow.

## Guide loading rules

Use the narrowest relevant guidance instead of loading broad instruction sets up front.

Primary guides (`project.setup.md`, `shared.setup.md`, matching `*.instructions.md`) should be read in full before generating code in that area. Deep references (`*.setup.md`, `*.examples.md`, `*.signatures.md`, `*.namespaces.md`) should be consulted surgically by section when the current task needs them.

1. Never load the whole `ai/` folder when a narrower guide exists.
2. For project scaffolding or app-shape changes, load `project.setup.md`.
3. For shared setup concerns reused across modules, load `shared.setup.md`.
4. For module-specific work, load the matching `*.instructions.md` guide before writing code.
5. Load deep references such as `*.setup.md`, `*.examples.md`, `*.signatures.md`, and `*.namespaces.md` only when the current task needs them.
6. When details are missing, do not guess syntax or signatures. Stop, describe the gap, and ask the user for confirmation.

When working in the Regira source repository itself:

- `src/Common.Setup/ai/project.setup.md` owns reusable project-template and app-shape setup.
- `src/Common.Setup/ai/shared.setup.md` owns shared setup rules reused across modules.
- Module-specific guides live under the owning module's `src/*/ai/` folder.
- Read `ai/learnings.md` before starting substantial work and keep it curated when a task exposes a durable lesson.

## Project template selection

Choose `projectTemplate` before selecting modules when the user is creating a new project or requesting major scaffolding changes.

| Requirement | `projectTemplate` |
|-------------|-------------------|
| Script, batch job, or CLI utility | `ConsoleWithLogging` |
| Standard hosted API, Minimal API and Controllers, no auth | `BasicApi` |
| Lightweight self-hosted internal API, no auth | `SelfHostingApi` |
| Must be deployable as a Windows Service | `SelfHostingApi` |
| API protected by API key and/or JWT Bearer | `SelfHostingApiWithAuth` |
| Controller-based routing with enforced authorization | `SelfHostingApiWithAuth` |
| Minimal API endpoints with authentication | `SelfHostingApiWithAuth` |

For a new project, choose the template before creating files. For an existing project, infer the nearest matching template from the current app structure and stay consistent with it.

## Setup baseline

Keep setup aligned with the selected `projectTemplate`. This file must remain enough for the normal one-file consumer flow even when no extracted local guides exist yet.

- Use the latest stable .NET framework and latest C# features unless the consumer project already targets something else. Current LTS version is **.NET 10**.
- Add the Regira feed to `NuGet.Config` alongside `nuget.org` before restoring Regira packages.
- Keep `Program.cs` thin and move service registration or middleware setup into extension methods.
- Prefer `Microsoft.Extensions.DependencyInjection` and depend on abstractions instead of concrete implementations.
- Use file-scoped namespaces.
- For standard Web APIs, use `app.MapOpenApi()` plus `app.MapScalarApiReference()`. Do not add `Swashbuckle.AspNetCore` or call `UseSwaggerUI()` on the standard Regira API path.
- Ask for feedback instead of guessing missing APIs, namespaces, signatures, or project-specific conventions.

Template consequences:

- `ConsoleWithLogging`: use a host-based console or CLI setup with configuration and structured logging.
- `BasicApi`: use an ASP.NET Core Web API baseline without authentication unless the user explicitly asks for auth.
- `SelfHostingApi`: use a self-hosted internal API baseline and keep it compatible with Windows Service deployment when requested.
- `SelfHostingApiWithAuth`: use self-hosted API scaffolding with API key and/or JWT Bearer authentication, and keep application endpoints protected by default.

## Code generation workflow

1. Choose or confirm the `projectTemplate`.
2. Choose the smallest Regira module set that covers the user's request.
3. Ensure the NuGet feed exists and add the matching packages.
4. Inspect existing `PackageReference` items when the installed Regira package set is part of the decision.
5. Run `dotnet restore` and `dotnet build` when needed so installed Regira packages can extract any embedded `ai/*.md` files from the NuGet package into `.regira/instructions/`.
6. **New project checkpoint**: When creating a project from scratch, stop here after completing steps 1–5. Summarize what was set up (template chosen, packages added, restore/build outcome) and explicitly ask the user whether to continue before writing any application code. This gives the user the opportunity to review the initial setup, switch to plan mode, or adjust the package selection before any code is generated.
7. Before writing any application code, check `.regira/instructions/` for extracted `*.instructions.md` guides, shared setup files, and relevant deep references.
8. If extracted guides exist, read the applicable primary guides in full before generating entity models, services, controllers, DI registrations, or infrastructure code. Use deep references by section when the current task needs exact examples, signatures, namespaces, or setup details. Skipping the relevant primary guides is a workflow violation.
9. If no extracted guides exist, verify the feed is reachable and the restore/build succeeded, then continue with the setup baseline, package mapping tables, and general engineering rules in this file.
10. Generate code that stays consistent with the selected `projectTemplate`, installed Regira packages, any extracted local guides, and local project conventions.

## Installed package routing

When the consumer project already contains Regira packages, inspect the project's `PackageReference` items and map them back to the nearest module family before generating code.

| Installed package pattern | Treat as module |
|---------------------------|-----------------|
| `Regira.Entities*` | `Entities` |
| `Regira.IO.Storage*` | `IO.Storage` |
| `Regira.Office.PDF*` | `Office.PDF` |
| `Regira.Office.Excel*` | `Office.Excel` |
| `Regira.Office.Word*` | `Office.Word` |
| `Regira.Mail.*` | `Office.Mail` |
| `Regira.Office.Csv*` | `Office.CSV` |
| `Regira.Office.Barcodes*` | `Office.Barcodes` |
| `Regira.Office.OCR*` | `Office.OCR` |
| `Regira.Office.VCards*` | `Office.VCards` |
| `Regira.Media*`, `Regira.Drawing.*` | `Media` |
| `Regira.Security*` | `Security` |
| `Regira.Setup` | `Setup` |
| `Regira.Web*` | `Web` |
| `Regira.System*` | `System` |
| `Regira.Invoicing*` | `Invoicing` |
| `Regira.Payments*` | `Payments` |
| `Regira.TreeList` | `TreeList` |

## Primary Regira package families

| Module | Use when | Main packages |
|--------|----------|---------------|
| Entities | CRUD APIs, entity services, DTO mapping, EF Core repositories | `Regira.Entities.DependencyInjection`, `Regira.Entities.Mapping.Mapster` (default mapping); also `Regira.Entities`, `Regira.Entities.EFcore`, `Regira.Entities.Web` |
| IO.Storage | File storage, uploads, Azure Blob, SFTP, ZIP | `Regira.IO.Storage`, `Regira.IO.Storage.Azure`, `Regira.IO.Storage.SSH` |
| Office | Office family overview or when the user still needs to choose between PDF, Excel, Word, Mail, OCR, and related submodules | `Regira.Office` |
| Office.PDF | HTML to PDF, PDF operations, printing | `Regira.Office.PDF.SelectPdf`, `Regira.Office.PDF.Puppeteer` |
| Office.Excel | Excel read and write | `Regira.Office.Excel.ClosedXML`, `Regira.Office.Excel.MiniExcel` |
| Office.Word | Word document generation | `Regira.Office.Word.Spire` |
| Office.Mail | Email sending | `Regira.Mail.SendGrid`, `Regira.Mail.MailGun` |
| Office.CSV | CSV read and write | `Regira.Office.Csv.CsvHelper` |
| Office.Barcodes | Barcode or QR code generation | `Regira.Office.Barcodes.ZXing`, `Regira.Office.Barcodes.QRCoder` |
| Office.OCR | OCR text extraction | `Regira.Office.OCR.Tesseract` |
| Office.VCards | vCard contact files | `Regira.Office.VCards.FolkerKinzel` |
| Media | Image processing, resize, crop, rotate, FFmpeg workflows | `Regira.Media`, `Regira.Drawing.SkiaSharp`, `Regira.Drawing.GDI`, `Regira.Media.FFMpeg` |
| Security | Hashing, JWT, API key authentication, cryptography | `Regira.Security`, `Regira.Security.Authentication`, `Regira.Security.Authentication.Web` |
| Web | Razor rendering, middleware, optional Swagger/OpenAPI auth helpers | `Regira.Web`, `Regira.Web.HTML.RazorEngineCore`; `Regira.Web.Swagger` only for explicit Swagger/OpenAPI auth support |
| System | Windows Service hosting, project tooling | `Regira.System`, `Regira.System.Hosting` |
| Invoicing | Invoice models, UBL, Peppol, AP gateway integrations | `Regira.Invoicing`, `Regira.Invoicing.UblSharp` |
| Payments | Payment providers, payment links, webhooks | `Regira.Payments`, `Regira.Payments.Mollie`, `Regira.Payments.Pom` |
| TreeList | Hierarchical tree structures | `Regira.TreeList` |

For Web APIs, the shared `project.setup.md` guide already standardizes OpenAPI plus Scalar as the default API surface. Swagger is deprecated, only use it when asked.

Use `Office` for the family overview or shared Office conventions. Add one or more concrete `Office.*` modules when the requested capability is already clear.

Modules with multiple provider packages, such as `Office.PDF` or `Office.Excel`, require a deliberate provider choice. Do not guess when the requested behavior is still ambiguous.

## Additional package families

These package families are available from the Regira feed but do not currently have dedicated AI guides. Choose them from user needs, install the matching package, and rely on general project conventions plus package-specific code references.

| Package family | Use when | Main packages |
|----------------|----------|---------------|
| Common | Shared abstractions, utilities, normalizing helpers, base contracts | `Regira.Common` |
| Caching | Runtime caching on top of the common abstractions | `Regira.Caching.Runtime` |
| DAL.EFcore | EF Core extensions and repository utilities | `Regira.DAL.EFcore` |
| DAL.MongoDB | MongoDB connectivity and backup or restore workflows | `Regira.DAL.MongoDB` |
| DAL.MySQL | MySQL or MariaDB connectivity and backup workflows | `Regira.DAL.MySQL`, `Regira.DAL.MySQL.MySqlBackup` |
| DAL.PostgreSQL | PostgreSQL connectivity | `Regira.DAL.PostgreSQL` |
| Globalization | Phone number parsing and formatting | `Regira.Globalization.LibPhoneNumber` |
| Setup | Shared project-template and setup-guide extraction for local AI guidance | `Regira.Setup` |
| Serializing | Newtonsoft.Json-based serialization | `Regira.Serializing.Newtonsoft` |

## Optional local cache

If the application repository already contains local Regira metadata files, use them as extra context:

- `.regira/instructions/*.md` can provide richer shared setup and module-specific guidance. Installed Regira packages that ship AI files can extract them there from their packaged `ai/` content on build via their package props and targets.
- `Regira.Setup` can be installed when the consumer needs `project.setup.md` and `shared.setup.md` extracted locally through the package-based guide flow.

These files are optional. `AGENTS.md` must remain enough for the normal consumer flow.

## General engineering rules

Apply these conventions when no narrower module guide exists, or as a supplement when the module guide does not cover the topic. Reuse the setup baseline above for framework, feed, namespace, and web-API defaults instead of re-stating them elsewhere.

### Project conventions

- Unless the project already constrains you, prefer the latest stable .NET and C# features that fit the local code style.

### Naming

- Follow normal C# naming conventions.
- Keep names descriptive but concise.
- Prefer meaningful generic type names such as `TEntity`, `TKey`, and `TDto` over bare single-letter names when context allows.
- Use generic names like `item` when the surrounding type already makes the meaning obvious.

### Dependency injection

- Prefer `Microsoft.Extensions.DependencyInjection` with feature-focused `IServiceCollection` extension methods.
- Avoid service-locator patterns unless a framework explicitly requires them.

### Testing

- Choose the smallest suitable test surface for the task.
- Keep tests focused and behavior-oriented.

### SOLID and simplicity

- Default to SOLID design principles, but do not introduce abstractions that the current task does not need.
- Prefer the simplest solution that correctly solves the current problem.
- Avoid speculative flexibility and premature indirection.
- Depend on abstractions instead of concrete implementations when defining business logic.
