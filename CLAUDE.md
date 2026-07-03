# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

ASP.NET Core 8.0 Web API backend for "Pastelería Nancy's" — a bakery management system covering security/users, inventory, POS/cash-register (caja), and web orders (pedidos). Domain code and DB comments are in Spanish; keep new domain code consistent with that convention.

## Commands

Run all commands from the repo root (where `PasteleriaNancys.Backend.sln` lives).

```
dotnet restore PasteleriaNancys.Backend.sln
dotnet build PasteleriaNancys.Backend.sln
dotnet run --project PasteleriaNancys.Api
```

There are currently no automated tests in the solution (no test project exists yet).

### EF Core migrations

Migrations live in `PasteleriaNancys.Infrastructure/Migrations` and target `ApplicationDbContext` (in `PasteleriaNancys.Infrastructure/Data`). Run migration commands with the Infrastructure project as `--project` and Api as `--startup-project` (Api owns the connection string and DI wiring):

```
dotnet ef migrations add <Name> --project PasteleriaNancys.Infrastructure --startup-project PasteleriaNancys.Api
dotnet ef database update --project PasteleriaNancys.Infrastructure --startup-project PasteleriaNancys.Api
```

The connection string is in `PasteleriaNancys.Api/appsettings.json` (`ConnectionStrings:DefaultConnection`, SQL Server, Integrated Security). It currently points at a local `DESKTOP-A3Q9KL4\ADMDB` instance — update it for your own environment rather than committing machine-specific values.

**The database, not the C# model, is the source of truth.** `PasteleriaNancysDB` was hand-designed and created directly in SQL Server (schemas `Seguridad`, `Inventario`, `Caja`, `Web`, 15 tables total) before any EF model existed. `20260703062655_InitialCreate` is an intentionally **empty baseline migration** (`Up`/`Down` are no-ops) that only creates `__EFMigrationsHistory` and marks itself applied — it does not create or alter any table. Never `dotnet ef migrations add` a change that would `CreateTable` for something that already exists in the real DB without first checking the actual schema (`INFORMATION_SCHEMA.COLUMNS` / `sys.tables` via `sqlcmd`) — the C# entities can silently drift from what's really in SQL Server (see gap below).

## Architecture

Layered solution, four projects wired via project references:

- **PasteleriaNancys.Domain** — POCO entity classes only, grouped by bounded context folder (`Seguridad`, `Inventario`, `Caja`, `Pedidos`). No EF or framework references. This is the dependency root; everything else depends on it.
- **PasteleriaNancys.Application** — application/business logic. References Domain plus `Microsoft.Extensions.DependencyInjection.Abstractions` (for its own DI registration extension methods — no EF or ASP.NET Core references). Organized per bounded context (currently `Seguridad`), each with `Dtos/`, `Interfaces/` (repository + service contracts), and `Services/` (concrete business logic). Cross-cutting business exceptions live in `Common/Exceptions` (`NoEncontradoException` → 404, `ConflictoException` → 409, `ReglaNegocioException` → 400, `CredencialesInvalidasException` → 401 — mapped by `ExceptionHandlingMiddleware` in Api).
- **PasteleriaNancys.Infrastructure** — EF Core implementation. `Data/ApplicationDbContext.cs` is the single `DbContext` and is the source of truth for how each entity maps to its SQL schema/table (via `OnModelCreating`, not attributes). References Domain and Application.
- **PasteleriaNancys.Api** — ASP.NET Core Web API host (`Program.cs`, controllers, Swagger). References Application and Infrastructure. This is the startup project and owns configuration (`appsettings*.json`).

### Domain model / DB schema mapping

Each Domain folder corresponds to a SQL schema, configured explicitly in `ApplicationDbContext.OnModelCreating`:

| Domain folder | SQL schema | Entities | Matches real DB? |
|---|---|---|---|
| `Seguridad` | `Seguridad` | `Rol`, `Usuario` (Usuario → Rol FK, restrict delete) | ✅ Yes — verified column-by-column against SQL Server |
| `Inventario` | `Inventario` | `CatalogoItem`, `LotePeps` | ❌ No — real tables are `Item_Catalogo`, `Lote_PEPS`, plus `Proveedor`, `Evento_Festivo`, `Viaje_Despacho`, `Viaje_Detalle` which have no C# entity yet |
| `Caja` | `Caja` | `TurnoCaja`, `VentaPos` | ❌ No — real tables are `Turno`, `Venta_POS`, plus `Venta_Detalle`, `Gasto_Extra` which have no C# entity yet |
| `Pedidos` | `Web` | `PedidoWeb` | ❌ No — real tables are `Pedido_Cliente`, `Pedido_Configuracion` |

All entity primary/foreign keys are `Guid` except `Rol.IdRol`, which is `int`. When adding/fixing an entity, follow the `Seguridad` pattern: plain POCO in Domain, `DbSet` + fluent `ToTable`/schema/key/precision/index config added to `OnModelCreating` **matched against the actual SQL Server schema** (`sqlcmd -S <instance> -d PasteleriaNancysDB -E -C -Q "..."` against `INFORMATION_SCHEMA.COLUMNS`, `sys.foreign_keys`, `sys.indexes`), then a migration.

**Known gap**: `Inventario`, `Caja`, and `Pedidos` (`Web`) entities above are the original scaffolded guesses and use different table/column names than the real database. They compile but do not reflect reality — don't trust them, and don't run a migration that would `CreateTable` for them until they've been reconciled against the real schema the same way `Seguridad` was.

`Rol` is seeded via `HasData` with the three roles that actually exist in the DB (`IdRol=1` "Administrador", `IdRol=2` "Vendedora", `IdRol=3` "Encargado de Almacen") — this seed only takes effect on a fresh database created purely from migrations; it changed nothing in the existing local DB (see baseline migration note above).

## Módulo de Seguridad (referencia para módulos futuros)

The `Seguridad` module (`Usuario`/`Rol`) is the first vertical slice built end-to-end and is the template to follow for the remaining modules (`Inventario`, `Caja`, `Pedidos`):

- **Wiring pattern**: each layer exposes an `IServiceCollection` extension method per bounded context — `AddSeguridadApplication()` (Application, registers services) and `AddSeguridadInfrastructure(configuration)` (Infrastructure, registers repositories + security implementations + binds `JwtSettings` from the `Jwt` config section). `Program.cs` just calls both. Repeat this pattern (`Add<Modulo>Application`/`Add<Modulo>Infrastructure`) for new modules instead of registering types ad hoc in `Program.cs`.
- **Repositories**: interfaces (`IUsuarioRepository`, `IRolRepository`) live in Application; EF Core implementations live in `Infrastructure/Seguridad/Repositories`, operating on `ApplicationDbContext` directly (no generic repository/UoW abstraction).
- **Usuario ↔ DB quirk**: the `Usuario.Id` (`Guid`) property is mapped to the real column name `IdUsuario` via `.HasColumnName("IdUsuario")` in `OnModelCreating` — the C# property itself stays `Id` for consistency with the other entities. `Usuario` also has `FechaCreacion` (set explicitly in `UsuarioService.CrearAsync`, mirroring how `Id` is generated in C# rather than relying on the DB's `newid()`/`sysutcdatetime()` defaults). `Correo` (max 100) and `Rol.Nombre` (max 50) both have unique indexes matching real `UQ_*` constraints in SQL Server — enforce uniqueness in the service layer too (already done) rather than relying on the DB constraint to surface as a clean error.
- **Auth**: `AuthController.Login` (`POST /api/auth/login`, anonymous) validates credentials via `IPasswordHasher` (BCrypt, `Infrastructure/Seguridad/Security/PasswordHasher.cs`) and issues a JWT via `IJwtTokenGenerator` (`Infrastructure/Seguridad/Security/JwtTokenGenerator.cs`). The token carries `ClaimTypes.Role` set to `Usuario.Rol.Nombre`, so endpoint authorization uses the built-in `[Authorize(Roles = "Administrador")]` — no custom policy/claims code needed.
- **Bootstrap gap**: `POST /api/usuarios` (create user) is intentionally `[AllowAnonymous]` so the very first admin account can be created without already holding a token. Every other Usuario/Rol mutation requires `[Authorize(Roles = "Administrador")]`. If this API is exposed beyond local dev, lock down or remove that endpoint's anonymous access.
- **Error handling**: services throw the `Common/Exceptions` types instead of returning error codes; `ExceptionHandlingMiddleware` (`Api/Middleware`) converts them to `ProblemDetails` responses. Reuse the same exceptions for new modules rather than inventing per-module error types.
- **Jwt config**: `PasteleriaNancys.Api/appsettings.json` → `Jwt:Key/Issuer/Audience/ExpireMinutes`. The committed key is a placeholder for local dev, following the same pattern as the committed `DefaultConnection` string — replace both with real secrets (user-secrets/environment variables) before any non-local deployment.

### Note on the top-level `PasteleriaNancys/` folder

There is a second, unrelated project at the repo root named `PasteleriaNancys` (with its own `Program.cs`, `WeatherForecastController`, and `Dockerfile`). It is **not** referenced by `PasteleriaNancys.Backend.sln` and appears to be a leftover scaffold from initial project creation — the real API is `PasteleriaNancys.Api`. Don't add features there.
