# GSheetsCommander

A reusable Unity package for communicating with Google Sheets through Google Apps Script.

## Features

- Google Sheets access from Unity through a Google Apps Script HTTP API
- Async `Task`-based API and `UnityWebRequest` transport
- ScriptableObject configuration
- Strongly typed responses and centralized `GoogleSheetsException` handling
- Sheet, cell, range, and row operations
- Unity Test Framework tests and an opt-in integration flow
- UPM and Git installation

## Architecture

```text
Unity
  ↓
GSheetsCommander
  ↓
HTTP POST
  ↓
Google Apps Script
  ↓
Google Sheets
```

## Installation

In Unity, open **Window → Package Manager → + → Add package from git URL**, then enter:

```text
https://github.com/<username>/GSheetsCommander.git
```

The package declares its `com.unity.nuget.newtonsoft-json` dependency automatically.

## Configuration

Create an asset through **Create → GSheetsCommander → Google Sheets Config**. Set its Google Apps Script URL, API Key, Timeout Seconds, and Enable Logging values. No endpoint or key is hardcoded in the package.

## Usage

```csharp
using GSheetsCommander;
using UnityEngine;

public class Example : MonoBehaviour
{
    [SerializeField] private GoogleSheetsConfig config;
    private GoogleSheetsClient client;

    private void Awake()
    {
        client = new GoogleSheetsClient(config);
    }

    private async void Start()
    {
        var cell = await client.GetCellAsync("Users", "A1");
        await client.SetCellAsync("Users", "A1", "Updated");
        var range = await client.GetRangeAsync("Users", "A1:B2");
        await client.SetRangeAsync("Users", "A1:B2", new object[][] { new object[] { "A", "B" } });
        await client.AppendRowAsync("Users", new object[] { "Ada", "active" });
        await client.UpdateRowAsync("Users", 2, new object[] { "Ada", "inactive" });
        if (await client.SheetExistsAsync("Archive"))
            Debug.Log("Archive already exists");
        else
            await client.CreateSheetAsync("Archive");
        var sheets = await client.ListSheetsAsync();
    }
}
```

Every request is an HTTP POST envelope containing `action`, a unique `requestId`, the API key, and a `payload`. Use `SendAsync<T>` when extending the backend with an operation not represented by the convenience methods.

Use `SheetExistsAsync("Sheet name")` to check for a tab without handling a missing-sheet exception. `TryGetSheetAsync("Sheet name")` returns its `SheetInfo`, or `null` when it does not exist. Both methods only handle the expected `SHEET_NOT_FOUND` response; configuration, access, and network errors are still returned as exceptions.

## Error handling

```csharp
try
{
    var result = await client.GetCellAsync("Users", "A1");
}
catch (GoogleSheetsException ex)
{
    Debug.LogError($"{ex.Code}: {ex.Message}");
}
```

Known backend error codes, including `INVALID_REQUEST`, `INVALID_API_KEY`, `SHEET_NOT_FOUND`, `INVALID_RANGE`, and `ROW_NOT_FOUND`, remain available through `GoogleSheetsException.Code`. Malformed responses and transport failures are converted to predictable `INVALID_RESPONSE` and `NETWORK_ERROR` exceptions.

When logging is enabled, request metadata and responses are logged with the `[GSheetsCommander]` prefix. The API key is never logged.

## Integration tests

The runtime tests include an explicit, opt-in end-to-end flow. Set `GSHEETS_COMMANDER_URL` and `GSHEETS_COMMANDER_API_KEY` in the test environment, then run the explicit `GoogleSheetsIntegrationTests.FullBackendFlow` test. It creates a uniquely named sheet and exercises health, cell, range, row, list, rename, and delete operations without relying on production sheet data.

## Security note

An API key embedded in a Unity client is not a secret: users can potentially extract it from a built application. It is only an additional API access layer and must not be treated as secure user authentication.

## Roadmap

```text
[x] Basic HTTP client
[x] Google Sheets CRUD
[x] ScriptableObject configuration

[ ] Generic Record API
[ ] Authentication helpers
[ ] Session management
[ ] Local caching
[ ] Synchronization utilities
```
