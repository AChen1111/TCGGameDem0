# Implementation Plan: Authentication Backend Foundation

## Architecture Decisions

- Use a feature-first ASP.NET Core minimal API so authentication behavior stays together without controller/service/repository ceremony.
- Use SQLite locally because Docker is unavailable; isolate EF Core access so PostgreSQL can replace the provider later.
- Use explicit Unity DTOs and `UnityWebRequest`; OpenAPI generation is deferred until the API surface grows enough to justify it.
- Use short-lived JWT access tokens plus server-stored, rotating refresh sessions.
- Use Problem Details and request trace IDs for one predictable error contract.

## Dependency Order

```text
Backend scaffold
  -> persistence and configuration
    -> registration
      -> login
        -> refresh/logout/me
          -> Unity client integration
            -> runtime smoke test and review
```

## Risks and Mitigations

| Risk | Impact | Mitigation |
|---|---|---|
| SQLite behavior differs from production PostgreSQL | Medium | Keep provider-specific code out of auth behavior and use EF migrations |
| Refresh token theft | High | Store only hashes, rotate on every refresh, revoke on logout |
| Brute-force login attempts | High | Apply a strict built-in rate limiter to auth endpoints |
| Unity token persistence leaks credentials | High | Keep sessions in memory for this phase |
| HotUpdate compatibility | Medium | Use Unity APIs and Newtonsoft JSON already present in the project |

## Checkpoints

1. Backend scaffold builds and database migration applies.
2. Each authentication behavior has a failing integration test before implementation and passes afterward.
3. Unity code recompiles through the project pipeline.
4. Full tests, build, runtime smoke test, security review and simplification pass complete.

## Result

Completed on 2026-08-22. Backend integration tests pass 7/7, the Release build and Unity `HotUpdate` compilation have zero warnings/errors from changed code, and a live-process authentication smoke test passed.
