# Banking Service

An ASP.NET Core 10 REST API for processing bank account withdrawals. Deducts funds from a SQL Server database and publishes a withdrawal event to AWS SNS.

## Architecture

```
HTTP POST /bank/withdraw
        │
        ▼
BankAccountController
        │
        ▼
WithdrawalService
    ├── AccountRepository   →  SQL Server (balance deduction)
    └── SnsEventPublisher   →  AWS SNS (withdrawal event)
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server with a `BankingDb` database
- AWS account with an SNS topic for withdrawal events

## Database Setup

The `accounts` table must exist with at least the following columns:

```sql
CREATE TABLE accounts (
    id      BIGINT         PRIMARY KEY,
    balance DECIMAL(18, 2) NOT NULL
);
```

## Configuration

Update `appsettings.json` (or use environment variables / user secrets) with your connection string and SNS details:

```json
{
  "ConnectionStrings": {
    "BankingDb": "Server=localhost;Database=BankingDb;Trusted_Connection=True;"
  },
  "Sns": {
    "Region": "af-south-1",
    "TopicArn": "arn:aws:sns:af-south-1:123456789:withdrawals-topic"
  }
}
```

AWS credentials are resolved via the standard credential chain (environment variables, `~/.aws/credentials`, IAM role, etc.).

## Running the API

```bash
dotnet run --project BankingService
```

The app will print the local URL on startup, e.g. `http://localhost:5000`.

## API Reference

### POST /bank/withdraw

Deducts the specified amount from the account balance and publishes a withdrawal event to SNS.

**Query parameters**

| Parameter   | Type    | Required | Description                                      |
|-------------|---------|----------|--------------------------------------------------|
| `accountId` | `long`  | Yes      | ID of the account to debit. Must be greater than 0. |
| `amount`    | `decimal` | Yes    | Amount to withdraw. Must be positive with at most 2 decimal places. |

**Example request**

```bash
curl -X POST "http://localhost:5000/bank/withdraw?accountId=42&amount=250.00"
```

**Responses**

| Status | Meaning |
|--------|---------|
| `200 OK` | Withdrawal successful. Returns `message` and `eventId`. |
| `400 Bad Request` | Invalid `accountId` or `amount`. |
| `422 Unprocessable Entity` | Insufficient funds or account not found. |
| `500 Internal Server Error` | Unexpected error. |

**Success response body**

```json
{
  "message": "Withdrawal successful.",
  "eventId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
}
```

## SNS Event Schema

On a successful withdrawal the following event is published to the configured SNS topic:

```json
{
  "eventId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "eventType": "WITHDRAWAL",
  "schemaVersion": "1.0",
  "accountId": 42,
  "amount": 250.00,
  "status": "SUCCESSFUL",
  "occurredAt": "2026-06-03T10:00:00+00:00"
}
```

> **Note:** If the SNS publish fails after a successful balance deduction, the withdrawal is **not** rolled back. The service returns HTTP 200 with the message `"Withdrawal successful. Event notification delayed."` and logs a `CRITICAL` alert for manual reconciliation.

## Swagger UI

Swagger is available in the Development environment. To enable the interactive UI, install the SwaggerUI package and uncomment `app.UseSwaggerUI()` in `Program.cs`:

```bash
dotnet add package Swashbuckle.AspNetCore.SwaggerUI
```

```csharp
// Program.cs
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

Then navigate to `http://localhost:<port>/swagger`.
