# Spec: Authentication Backend Foundation

## Objective

Provide a runnable local authentication backend and Unity client integration for account registration and login without adding presentation-layer code.

## Tech Stack

- ASP.NET Core 8 minimal API
- Entity Framework Core with SQLite for local development
- JWT bearer access tokens and rotating refresh tokens
- xUnit integration tests
- Unity `UnityWebRequest` client code in the existing `HotUpdate` assembly

## Commands

```powershell
dotnet restore Backend/AChen.Backend.sln
dotnet build Backend/AChen.Backend.sln --no-restore
dotnet test Backend/AChen.Backend.sln --no-build
dotnet run --project Backend/src/AChen.Backend.Api
```

## Project Structure

```text
Backend/src/AChen.Backend.Api/Features/Auth/  Authentication contract and behavior
Backend/src/AChen.Backend.Api/Data/           EF Core context and migrations
Backend/tests/AChen.Backend.Api.Tests/        HTTP integration tests
Assets/Scripts/Network/                       Unity HTTP and authentication client
```

## Code Style

Use direct, descriptive C# with guard clauses and feature-owned types. Do not introduce interfaces or wrappers until they represent a real boundary used by the current feature.

```csharp
if (user is null || !passwordHasher.Verify(user, password))
{
    throw new AuthException("INVALID_CREDENTIALS", "Invalid username or password.");
}
```

## API Contract

| Method | Path | Authentication | Success |
|---|---|---|---|
| POST | `/api/auth/register` | Anonymous | `201 AuthResponse` |
| POST | `/api/auth/login` | Anonymous | `200 AuthResponse` |
| POST | `/api/auth/refresh` | Anonymous | `200 AuthResponse` |
| POST | `/api/auth/logout` | Anonymous, refresh token in body | `204` |
| GET | `/api/auth/me` | Bearer access token | `200 UserResponse` |
| GET | `/health` | Anonymous | `200` |
| GET | `/ready` | Anonymous | `200/503` |

All JSON fields use camelCase. Error responses use ASP.NET Problem Details with a stable `code` extension and request trace ID.

## Authentication Rules

- Username: 3-24 characters; ASCII letters, digits and underscore.
- Email: normalized to lowercase and validated as an email address.
- Password: 8-128 characters.
- Usernames and emails are unique case-insensitively.
- Passwords use ASP.NET Core Identity's versioned PBKDF2 password hasher.
- Access token lifetime is 15 minutes.
- Refresh token lifetime is 7 days, stored only as a SHA-256 hash, and rotated on use.
- Logout revokes the supplied refresh session.
- Unity keeps tokens in memory only in this phase.

## Testing Strategy

HTTP integration tests use an isolated temporary SQLite database. Tests cover successful registration/login, duplicate accounts, invalid credentials, refresh rotation, logout revocation and authenticated `/me` access.

## Boundaries

- Always: validate external input, hash passwords and refresh tokens, read secrets from environment/configuration, use parameterized EF Core queries.
- Ask first: adding OAuth, email delivery, roles, UI, PostgreSQL deployment or persistent Unity credential storage.
- Never: commit real secrets, store plaintext passwords/tokens, trust client identity, expose internal exception details.

## Success Criteria

- The backend builds, migrates its local database and starts without Docker.
- Registration, login, refresh, logout and `/me` pass integration tests.
- A running service passes manual HTTP smoke tests.
- Unity authentication code compiles in the `HotUpdate` assembly and exposes register/login/refresh/logout methods without UI dependencies.
- Configuration and local run instructions are documented.

## Open Questions

None for this slice. PostgreSQL and persistent platform-secure token storage remain later decisions.
