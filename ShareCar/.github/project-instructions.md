# Copilot Instructions

## General Guidelines
- Always use **UTF-8** encoding for all files.
- Always use **Windows-style line endings (CR LF)** when editing or creating files (`\r\n`).
- Always use **2 spaces** for indentation in all files (`.cs`, `.cshtml`, `.csproj`, `.json`, etc.). Never use tabs or 4-space indentation.
- Always use **target-typed `new()` syntax** where the type can be inferred from context (e.g., property assignments, constructor arguments). Do not use it in LINQ `Select()` lambdas or `var` declarations where the type cannot be inferred.
- Always use **collection expression syntax** (`[]`) for initializing collections where possible (e.g., `int[] numbers = [1, 2, 3];`, `List<string> names = ["a", "b"];`, empty collections `[]` instead of `Array.Empty<T>()` or `new List<T>()`).
- Always use **collection initializer syntax** (`{ items }`) for adding items to collection properties in object initializers, especially for protobuf repeated fields (e.g., `PlaceIds = { source.PlaceIds }` instead of manually calling `AddRange`).
- Always use **curly brackets for `if` statements**, even for single-line bodies. Never use braceless `if`.
- Always add an **empty line before a `return` statement** when it is preceded by any other statement or block in the same scope.
- **Nullable reference types** are enabled globally (`<Nullable>enable</Nullable>`).
- **Implicit usings** are enabled globally (`<ImplicitUsings>enable</ImplicitUsings>`).

## Solution Structure

The solution follows a **layered architecture** with clear separation of concerns. Each service is split into multiple projects:

### Backend Service Projects
- **`BackendService.App`** — The host application. Contains `Program.cs`, the gRPC service implementation, CQRS command/query handlers, mappings, configuration, and resources. Classes here are `internal sealed` by default.
- **`BackendService.Contracts`** — Contains the `.proto` file defining the gRPC contract and auto-generated code. Also contains the **C# contract records** (request/response types and the `IBackendServiceClient` interface) consumed by other services.
- **`BackendService.Contracts.GrpcClient`** — The gRPC client implementation (`BackendServiceGrpcClient`) of `IBackendServiceClient`. Also contains `ServiceCollectionExtensions` for DI registration and mapping extensions for converting between contract records and protobuf types.
- **`BackendService.Domain`** — Pure domain layer. Contains domain model classes, repository interfaces, service interfaces, and configuration interfaces. **No infrastructure dependencies.**
- **`BackendService.Domain.Repositories`** — Entity Framework Core repository implementations. Contains `DbContext`, entity type configurations, `UnitOfWork`, and concrete repository classes.
- **`BackendService.Domain.Services`** — Domain service implementations.

### Blazor Client Projects
- **`BlazorClient.App`** — The Blazor WebAssembly application. Contains pages, components, Fluxor states, and ViewModels.
- **`BlazorClientGatewayService.App`** — The BFF (Backend For Frontend) gateway service that proxies requests from the Blazor client to backend services.
- **`BlazorClientGatewayService.Contracts`** — The gRPC contracts between the Blazor client and the gateway service.

## Architecture Patterns

### CQRS with Mediator
- Use the **Mediator pattern** (`IMediator` / `IRequestHandler<TRequest, TResponse>`) for handling commands and queries.
- **Queries** go in `Queries/Handlers/` — named `Get{Entity}QueryHandler`.
- **Commands** go in `Commands/Handlers/` — named `{Verb}{Entity}CommandHandler`.
- Handlers are `internal sealed` classes.
- All handler methods log entry with `_logger.LogTraceIn(nameof(Handle), ...)`.
- All constructor dependencies are validated with `?? throw new ArgumentNullException(nameof(parameter))`.

### gRPC Service Implementation
- The gRPC service class inherits from the generated `ServiceBase` class.
- Each method follows the same pattern:
  1. Map the gRPC request to a domain query/command using `request.MapToQuery()` or `request.MapToCommand()`.
  2. Send it through the mediator: `await _mediator.Send(query, context.CancellationToken)`.
  3. Map the result back to gRPC: `result.MapToGrpc()`.
  4. Catch `DomainException` and generic `Exception` separately.

### Mapping Conventions
- Mappings are implemented as **static extension methods** in dedicated `*Mapping.cs` files.
- From gRPC to domain: `MapToQuery()`, `MapToCommand()`, `MapFromGrpc()`.
- From domain to gRPC: `MapToGrpc()`.
- Gateway mappings: `MapToSmartMapServiceClientRequest()`, `MapToGatewayServiceResponse()`.

### Protobuf / gRPC Contract Design
- Proto messages use the **Select/Where** pattern: every request has a `Select` (fields to return) and `Where` (filter criteria) nested message.
- Use `google.protobuf.wrappers` (`StringValue`, `Int32Value`, etc.) for nullable fields.
- Use `google.protobuf.Timestamp` for date/time fields.
- C# contract records use constructor parameters and `Types` nested classes matching the proto structure.
- gRPC client implementations use Polly retry policies for transient error handling.
- DI registration goes in `ServiceCollectionExtensions` as `Add{ServiceName}GrpcClient(...)` methods.

### Repository Pattern
- Repository **interfaces** live in `BackendService.Domain/Repositories/`.
- Repository **implementations** live in `BackendService.Domain.Repositories/` and are `internal sealed`.
- Repositories receive `ILogger<T>` and `DbContext` via constructor injection.
- For complex queries requiring raw SQL, use **Dapper** with `IDbConnectionProvider` and `SqlTemplate`/`SqlTemplateConditions` for conditional SQL branching.
- EF Core is used for entity loading and writes; Dapper for complex read queries.
- `UnitOfWork` wraps `DbContext.SaveChangesAsync()`.

### Entity Framework Core Conventions
- `DbContext` is custom (not `Microsoft.EntityFrameworkCore.DbContext` directly — it extends it).
- Entity type configurations implement `IEntityTypeConfiguration<T>` and are `internal sealed`.
- Configurations are applied via `modelBuilder.ApplyConfigurationsFromAssembly(typeof(DbContext).Assembly)`.
- Navigation properties use `AutoInclude()` where appropriate.
- Domain model classes have **getter-only properties** and a `private` parameterless constructor for EF.
- Collections in domain models are initialized with `[]` (collection expressions).

### Domain Model Conventions
- Domain model classes are plain C# classes (not records) with **getter-only properties**.
- Private parameterless constructor (`private Area() { }`) for EF Core materialization.
- No setters — state changes go through domain methods.
- Enums for types/states (e.g., `AreaType`, `StockType`, `BorderStyle`, `ItemRotation`).

### Dependency Injection Conventions
- Each infrastructure project exposes a `ServiceCollectionExtensions` class with `Add{Feature}(...)` extension methods.
- These are called in `Program.cs` to compose the application.
- Pattern: `builder.Services.AddDomainRepositories()`, `builder.Services.AddDomainServices()`, etc.
- Constructor injection with null-checks: `_field = parameter ?? throw new ArgumentNullException(nameof(parameter))`.

## Blazor WebAssembly Patterns

### Components
- Blazor components use **code-behind** pattern: `Component.razor` + `Component.razor.cs` (partial class).
- Dependencies are injected with `[Inject] private ServiceType ServiceName { get; set; } = default!;`.
- Component parameters use `[Parameter]` and `[Parameter, EditorRequired]`.
- Event callbacks use `EventCallback` and `EventCallback<T>`.
- Components subscribe to state changes in `OnInitialized`/`OnInitializedAsync` and unsubscribe in `Dispose`/`DisposeAsyncCore`.

### Fluxor State Management
- States are **records** decorated with `[FeatureState]`.
- States have a parameterless constructor (private if not persisted to storage, public if serialized to local storage).
- Actions are defined as **records** in a separate `Actions.cs` file per feature, one record per action.
- Reducers are `internal static` classes with `[ReducerMethod]`-decorated static methods in `Reducers.cs`.
- Effects are `internal` classes with `[EffectMethod]`-decorated instance or static methods in `Effects.cs`.
- Fluxor state folders: `States/{FeatureName}/` containing `{FeatureName}State.cs`, `Actions.cs`, `Reducers.cs`, `Effects.cs`.
- Page state persistence uses `Blazored.LocalStorage` with a key pattern: `"{ApplicationName}:{AppName}:{StateName}"`.

### ViewModel Pattern
- Pages use **ViewModels** injected via `[Inject]` (e.g., `MapPageViewModel`).
- ViewModels are registered in DI via `builder.Services.AddViewModels(typeof(Program).Assembly)`.
- ViewModels expose events (e.g., `IsBusyChanged`, `SelectedDatabarTabChanged`) that components subscribe to.
- Page ViewModels extend `StatePageViewModel<TState, TLoadAction>` for automatic page state loading.

## Logging
- Use **Serilog** for backend services.
- Use **structured logging** with message templates: `_logger.LogInformation("{ClientContextId} Executing {MethodName}()", ...)`.
- Use `_logger.LogTraceIn(nameof(Method), ...)` at the start of handler methods.

## Configuration
- `appsettings.json` with environment-specific overrides: `appsettings.Development.json`, `appsettings.Production.json`, `appsettings.Test.json`, `appsettings.Training.json`.
- gRPC client URLs configured under `GrpcClients:{ServiceName}:Url`.
- Backend services run as Windows Services (`builder.Host.UseWindowsService()`).

## Naming Conventions
- Async methods are suffixed with `Async`.
- Private fields use `_camelCase` prefix.
- Query/Command types match their handler: `GetPackagesQuery` → `GetPackagesQueryHandler`. 
- Handler methods are named `Handle`.
- Internal query result types are private `record class` types nested or at the end of the handler file (e.g., `private record class QueryRow(...)`).
