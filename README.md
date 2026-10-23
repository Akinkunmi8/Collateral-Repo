# CollateralSoluFrontend
React + TypeScript frontend synchronized with CollateralPortal ASP.NET Core 8 API.

## Run
1. Start backend/Docker stack so API is on `http://localhost:5080`.
2. Copy `.env.example` to `.env` (default API URL is already `http://localhost:5080/api`).
3. Run `npm install` then `npm run dev`.
4. Open `http://localhost:5173`.

## Development login
The backend seeds `admin@collateralportal.local` in Development. Use the development bootstrap password configured in the backend seeder and change/remove it before production.

## Connected features
JWT login; role-aware navigation; Admin user list/create/disable/delete; dashboard summary; collateral register/manual capture; Valuation/Legal/Insurance queues; Legal/Insurance report upload; Archive/Release report download; Audit list and CSV export.

## Security notes
Do not ship the development bootstrap admin/password to production. Store JWT secrets outside appsettings for production. Legal and Insurance reports are limited by the API to PDF/PNG/JPEG and 10 MB.
