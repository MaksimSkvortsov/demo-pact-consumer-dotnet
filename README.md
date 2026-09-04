# Demo Pact .NET Consumer

This repository owns the `dotnet-consumer` contract for the `customer-provider` API.

The consumer calls:

```http
GET /customers/{id}
```

The typed client is `CustomerClient`, and its model is `CustomerSummary`. This consumer only depends on:

```json
{
  "id": 123,
  "email": "john@example.com"
}
```

It does not depend on `name` or `phone`. Therefore, removing `name` or `phone` from the provider response should not break this consumer contract.

## Pact Contract

The Pact JSON file is generated from the consumer test. It is an artifact of the test, not a manually authored file.

Run:

```powershell
dotnet restore
dotnet build --no-restore
dotnet test --no-build
```

On Windows ARM64, run the tests with the x64 .NET host because PactNet `5.0.1` ships a Windows x64 native Pact FFI library:

```powershell
& 'C:\Program Files\dotnet\x64\dotnet.exe' test
```

The generated Pact is written to:

```text
pacts/dotnet-consumer-customer-provider.json
```

The contract test uses Pact matchers for response field types, so the provider must return a numeric `id` and string `email`, but the test is not coupled to one exact `email` value when type matching is enough.

## GitHub Actions

The workflow in `.github/workflows/generate-pact.yml` runs the consumer test, confirms that `pacts/dotnet-consumer-customer-provider.json` exists, and commits generated Pact changes back to this repository.

Required workflow permissions:

```yaml
permissions:
  contents: write
```

This permission is already configured in the workflow. In the repository settings, ensure GitHub Actions is allowed to read and write repository contents.

The commit step is skipped when the current commit message starts with `chore: update generated Pact files` to avoid an infinite workflow loop.
