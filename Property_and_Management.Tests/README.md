# Property_and_Management.Tests

Unit and integration tests for the BoardRent application.

## Running

Unit tests only (no database needed):

```
dotnet test Property_and_Management.Tests/Property_and_Management.Tests.csproj --filter "TestCategory!=Integration"
```

All tests, including repository integration tests that hit a real SQL Server:

```
dotnet test Property_and_Management.Tests/Property_and_Management.Tests.csproj
```

## Test database setup (integration tests only)

The integration tests use the shared `BoardRent` connection string from the
root-level `App.config`:

```
Data Source=localhost\SQLEXPRESS;Initial Catalog=BoardRent;...
```

Before running integration tests for the first time:

1. Make sure SQL Server Express is running.
2. Create an empty database named `BoardRent`. The
   `DatabaseInitializer` will create all tables on first run.
3. Adjust `Data Source` in `App.config` if your local instance name differs.

If SQL Server is unreachable, `DatabaseTestBase.InitializeDatabase` calls
`Assert.Ignore` so the integration tests skip gracefully.

## Project structure

- `Service/` — Osherove-style mocked unit tests for services.
- `Viewmodels/` — Mocked unit tests for view models (including a test
  subclass for the `PagedViewModel<T>` base).
- `Repository/` — Integration tests (marked `[Category("Integration")]`).

## Frameworks

- **NUnit 4** — test runner.
- **Moq** — mocking framework. All collaborators are mocked; tests never
  touch disk or a real database (except integration tests).
- **FluentAssertions** — readable assertions.
