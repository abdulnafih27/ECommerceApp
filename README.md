# ECommerceApp

An e-commerce REST API built with ASP.NET Core and SQL Server. Customers browse products, fill a cart and place orders. Admins manage the catalog and view all orders. Orders cannot oversell stock, even when two customers buy the last unit at the same moment.

**Stack:** .NET 10, ASP.NET Core Web API, Entity Framework Core 10, SQL Server 2022, JWT Bearer Authentication, OpenAPI, Swagger UI, AutoMapper, BCrypt, MSTest, Docker Compose.

## Run it

You need Docker Desktop (Linux containers). Nothing else has to be installed.

```bash
cd ECommerceApp
cp .env.example .env        # on Windows PowerShell: copy .env.example .env
# edit .env and set your own values (see the table below)
docker compose up --build
```

The API listens on `http://localhost:8080`. On first start it creates the database, applies the migrations and creates the admin account from your `.env` values.

To stop it, run `docker compose down`. Data is kept in a Docker volume. Run `docker compose down -v` to delete the data and start clean.

### Configuration (`.env`)

| Variable                                          | Purpose                                                                                                                                                      |
| ------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `SA_PASSWORD`                                     | SQL Server password. Must meet SQL Server's password policy (8+ characters, upper and lower case, a digit and a symbol). Do not use `;`, `=`, `$` or quotes. |
| `JWT_KEY`                                         | Signing key for tokens. Use at least 32 random characters.                                                                                                   |
| `JWT_ISSUER`, `JWT_AUDIENCE`                      | Any consistent strings.                                                                                                                                      |
| `JWT_EXPIRY_MINUTES`                              | Token lifetime.                                                                                                                                              |
| `ADMIN_EMAIL`, `ADMIN_USERNAME`, `ADMIN_PASSWORD` | The first admin account, created at startup if no admin exists.                                                                                              |

`.env` is git-ignored. Only `.env.example` is committed.
If you change `SA_PASSWORD` after the first start, run `docker compose down -v` to reset the database volume.

## Try it

1. Log in as admin using `POST /api/auth/login`.
2. Create a category and product as admin.
3. Register a customer using `POST /api/auth/signup`.
4. Log in as the customer and copy the access token.
5. Add the token through Swagger UI's **Authorize** button.
6. Add a product to the cart and place an order.

### Swagger UI

Available at: `http://localhost:8080/swagger`

Use the Authorize button to test protected endpoints.

### Postman

Import `postman/ECommerceApp.postman_collection.json`.

Set the base URL to `http://localhost:8080` and execute the login request. The collection saves the token automatically for subsequent requests.

## Endpoints

| Area       | Endpoints                                                                                                    | Access    |
| ---------- | ------------------------------------------------------------------------------------------------------------ | --------- |
| Auth       | `POST /api/auth/signup`, `POST /api/auth/login`                                                              | Public    |
| Account    | `GET /api/user/me`, `PATCH /api/user/me/update`, `PATCH /api/user/me/password`, `DELETE /api/user/me/delete` | Logged in |
| Users      | `GET /api/admin/users`                                                                                       | Admin     |
| Categories | `GET /api/category`, `GET /api/category/{id}`                                                                | Public    |
|            | `POST /api/category`, `PATCH /api/category/{id}/update`, `DELETE /api/category/{id}/delete`                  | Admin     |
| Products   | `GET /api/product` (paged, optional `categoryId`), `GET /api/product/{id}`                                   | Public    |
|            | `POST /api/product`, `PATCH /api/product/{id}/update`, `DELETE /api/product/{id}/delete`                     | Admin     |
| Cart       | `GET /api/cart`, `POST /api/cart/add`, `PATCH /api/cart/{id}/update`, `DELETE /api/cart/{id}/delete`         | Customer  |
| Orders     | `POST /api/order`, `GET /api/order/me`, `GET /api/order/me/{id}`                                             | Customer  |
|            | `GET /api/order`, `GET /api/order/{id}`                                                                      | Admin     |

List endpoints take `?page=` and `?pageSize=` (defaults 1 and 10, maximum page size 20). Out-of-range values are corrected, and unparseable values return 400.

## Design decisions

- **No overselling.** Placing an order runs in one transaction. Stock is decremented with a guarded update (`StockQuantity >= quantity`) and the affected-row count is checked. A database check constraint (`StockQuantity >= 0`) is the last line of defense. An automated test fires two orders for the last unit at once, 20 rounds, and expects exactly one winner.
- **Price and name snapshot.** Each order line stores the product's name and unit price at purchase time, so order history does not change when a product changes or is retired.
- **Orders are built from the server-side cart.** The client never sends prices, totals or items when ordering.
- **Soft delete.** Users and products are flagged as deleted and hidden with global query filters. A deleted user's email and username stay reserved.
- **Authorization.** Roles (`Admin`, customer) plus an `ActiveStatus` policy that checks the database on each request, so a suspended or deleted user is blocked immediately and not only when the token expires.
- **Consistent errors.** Every failure returns the standard problem-details shape with a `traceId` that also appears in the server log. Unexpected exceptions return a generic message and never leak internals.
- **Services return results, not exceptions.** Expected failures (not found, conflict, validation) travel as `ServiceResult` values and are mapped to HTTP status codes in one place.
- **Duplicates are checked twice.** The service checks first for a clean message, and unique indexes in the database catch races.
- **UTC everywhere.** Timestamps are stored and returned in UTC, and a model-wide EF converter marks them as UTC when read.
- **Concurrency.** Products carry a `RowVersion` token, so a conflicting save returns a 409.
- **Startup.** The app applies migrations and seeds the admin on start, and refuses to start if its configuration is wrong.

## Tests

Integration tests run the real app in memory against a real SQL Server database (not an in-memory fake), because the stock guarantee comes from the database.

```bash
dotnet test
```

The tests use a separate `ECommerce_Test` database on a local SQL Server (the test factory points at `localhost\SQLEXPRESS`; change the connection string there to use another server). Tests run one at a time on purpose, because the database is reset before each test.

They cover registration and login rules, authorization boundaries, and the order flow: the happy path, insufficient stock, retired products, ownership of orders, and the two-buyers race.

## Limitations (chosen on purpose)

- Cash on delivery only. There is no payment integration.
- Orders stay in the `Placed` status. There is no cancellation, shipping or delivery workflow.
- No image upload. Products store an image URL.
- The demo uses the `sa` database login. A real deployment should use a login with limited rights.
- The concurrency token protects only against a collision during the save. A client does not send it back, so an edit based on stale data can still overwrite a newer change.
- The wrong-password path runs a bcrypt check and the unknown-user path does not, so the two responses differ slightly in timing.
- No password reset or email flow.
