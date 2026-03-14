# WMS (Warehouse MVP)

This repository contains a minimal Warehouse Management backend (ASP.NET Core) and tests.

## Run locally

1. Ensure .NET 10 SDK is installed.
2. Ensure SQL Server LocalDB is available (or set CONNECTIONSTRINGS__DefaultConnection to your SQL Server).
3. From the backend project folder:

   cd src/Backend
   dotnet run

The application will apply migrations and seed sample test data on startup (unless running in the `Testing` environment).

## Swagger / OpenAPI

When the app is running (not in the `Testing` environment), the OpenAPI/Swagger endpoints are exposed. Open a browser and navigate to:

  https://localhost:<port>/openapi

and the UI (if available) at:

  https://localhost:<port>/openapi/ui

Use the Swagger UI to call these endpoints:

- GET /orders/{id}  (e.g., /orders/ORD-10452)
- POST /pick        (confirm a pick)
- GET /inventory/product/{productId}/location/{locationId}

## Seed data

On first run the app seeds sample data:

- Products: USB Cable (SKU 8872), HDMI Cable (SKU 9001)
- Locations: A-7-3 and B-2-1
- Inventory: USB Cable @ A7-3 = 5 units
- Orders: ORD-10452 (USB Cable x2), ORD-10453 (HDMI Cable x3)

## Running tests

From repo root:

   dotnet test

## Change DB connection

Set environment variable `ConnectionStrings__DefaultConnection` to change the SQL Server used by the app.

---

If you want Swagger enabled in additional environments or hosted publicly, say so and I will update Program.cs and add authentication for the UI.