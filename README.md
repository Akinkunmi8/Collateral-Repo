# Collateral Management Portal

ASP.NET Core 8 (Clean Architecture, CQRS via MediatR) backend for the Bank's
collateral management workflow, per the "Automation of Collateral Management
Process on Newgen" BRD. Frontend (TypeScript/React) lives in `frontend/`
(to be scaffolded next).

## Solution layout

```
src/
  CollateralPortal.Domain          entities, enums, domain events (no dependencies)
  CollateralPortal.Application     CQRS commands/queries, integration port interfaces, DTOs
  CollateralPortal.Infrastructure  EF Core, mock + real integration adapters, Azure Service Bus
  CollateralPortal.Api             controllers, Program.cs, Dockerfile
tests/
  CollateralPortal.UnitTests
docker/
  servicebus-emulator/config.json  queue definitions for the local Service Bus emulator
docker-compose.yml                 SQL Server + Service Bus emulator + API
```

## Third-party integrations — current status

| System | Status |
|---|---|
| Newgen (Loan Management) | **Mocked** — `MockLoanManagementClient` |
| Vision (Core Banking) | **Mocked** — `MockCoreBankingClient` |
| Credit Risk System / CLOS | **Mocked** — `MockCreditRiskClient` |
| Credit Bureau (CRC / CreditRegistry / First Central) | **Mocked**, real adapter stubbed in `Integrations/Real/CreditBureauAdapter.cs` pending API Integration Kit |
| NGX Market Data | **Mocked**, real adapter stubbed in `Integrations/Real/MarketDataAdapter.cs` pending API subscription |
| Valuation Portal | **Real** — `ValuationPortalAdapter.cs`, endpoint/contract placeholder pending the actual API spec |

Every integration sits behind an interface in `Application/Common/Interfaces`.
Switching a mock for a real adapter is a one-line change in
`Infrastructure/DependencyInjection.cs` (or an `Integrations:*:UseMock` config
flag for Credit Bureau / Market Data) — no other layer changes.

## Running locally (Docker)

```bash
docker compose up --build
```

This starts:
- `sqlserver` — the application database (port 1433)
- `servicebus-sql-edge` + `servicebus-emulator` — local Azure Service Bus emulator (AMQP on 5672), pre-configured with the `collateral-notifications` queue
- `api` — the ASP.NET Core API (http://localhost:5080/swagger)

On first run, the API applies EF Core migrations automatically in the
Development environment.

## Running locally (no Docker)

You'll need SQL Server and an Azure Service Bus namespace (or the emulator)
reachable, then:

```bash
dotnet restore
dotnet ef database update --project src/CollateralPortal.Infrastructure --startup-project src/CollateralPortal.Api
dotnet run --project src/CollateralPortal.Api
```

## Workflow endpoints (mirrors the BRD use cases)

| Endpoint | Use case |
|---|---|
| `POST /api/collaterals` | UC-001 Collateral data capture |
| `POST /api/collaterals/{id}/valuation` | UC-002 Valuation (calls Valuation Portal, checks coverage ratio) |
| `POST /api/collaterals/{id}/perfection` | UC-003 Legal perfection |
| `POST /api/collaterals/{id}/insurance` | UC-004 Insurance (approval moves to System Hold) |
| `POST /api/collaterals/{id}/release` | UC-006 Release + archive (checks repayment via Vision) |
| `GET /api/collaterals` | Collateral register (MDM) view |
| `GET /api/collaterals/{id}` | Single collateral detail |

## Not yet built (next steps)
- EF Core migrations (run `dotnet ef migrations add InitialCreate` once you can build locally)
- Frontend (React + TypeScript)
- Real Credit Bureau / NGX adapters once credentials are issued
- Auth (Entra ID / JWT) — currently open endpoints
- Insurance renewal reminder background job (3 months before expiry, then monthly)

## Frontend synchronization (2026-09-15)
The React frontend uses `VITE_API_BASE_URL=http://localhost:5080/api`.
Added Admin user-management endpoints under `/api/users`, live `/api/summary`, report upload/download under `/api/collaterals/{id}/documents`, Auditor role/access, inactive-user login protection, and Admin-only registration.
Legal completion now requires a LegalReport upload; Insurance approval requires an InsuranceReport upload.
Allowed report formats: PDF, PNG, JPEG; maximum 10 MB.
