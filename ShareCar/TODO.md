- [x] **JWT-secured REST API** — all endpoints require Bearer token; Swagger UI with Bearer definition at `/swagger`
- [x] **Database + ORM** — Entity Framework Core with SQLite; `ShareCarDbContext`, entity configurations, `IDesignTimeDbContextFactory`, auto-migration on startup
- [x] **EF Migrations** — `InitialSchema`, `AddStartParkingLotToBooking`
- [x] **User registration** — `POST /api/auth/register`, password hashed with PBKDF2-SHA256, role set to `User`
- [x] **User login** — `POST /api/auth/login`, returns JWT with claims (id, username, role)
- [x] **Parking lots API** — `GET /api/parkinglots` with computed `AvailableVehicles` count
- [x] **Vehicles API** — `GET /api/vehicles/by-parking-lot/{id}`, `GET /api/vehicles/{id}`, `GET /api/vehicles/{id}/statistics`
- [x] **Bookings API** — rent (`POST /api/bookings/rent`), return (`POST /api/bookings/{id}/return`), active (`GET /api/bookings/active`), history (`GET /api/bookings/my`), vehicle ranges (`GET /api/bookings/vehicle/{id}`)
- [x] **Block/unblock API** — `POST /api/vehicles/{id}/block` and `POST /api/vehicles/{id}/unblock` (Admin role only)
- [x] **Statistics API** — `GET /api/statistics/vehicle/{id}` (last-month trips + distance), `GET /api/statistics/overview`
- [x] **Users API** — `GET /api/users`, `GET /api/users/{id}` (Admin role only)
- [x] **API discovery endpoint** — `GET /api/docs` using **reflection** to enumerate all controllers and actions (HTTP method, route, auth requirement)
- [x] **Domain models** — `Vehicle`, `Booking`, `ParkingLot`, `BlockLog`, `StatusHistory`, `User`, `UserRole`, `VehicleStatus`, `VehicleStatistics`
- [x] **Status history** — automatically recorded on every vehicle status change (rent, return, block, unblock) with timestamp
- [x] **Business rules in domain service** — overlap check (user + vehicle), blocked-vehicle guard, return-to-start-lot enforcement, odometer monotonicity, price calculation
- [x] **Vehicle status only changes via workflow** — rent / return / block / unblock; no manual status endpoint
- [x] **Completed rides immutable** — no edit/delete endpoint for finished bookings
- [x] **Future booking fix** — vehicle stays `Available` when booked for a future time slot; `HasOverlappingBookingAsync` still prevents double-booking
- [x] **MVC Dashboard** — parking lot selector, vehicle list, vehicle detail partial loaded via AJAX modal
- [x] **MVC Login / Register** — forms with server-side `ModelState` validation and `[ValidateAntiForgeryToken]`
- [x] **MVC Profile page** — active rental banner, return-vehicle dialog with odometer input, ride history table
- [x] **Profile AJAX refresh** — `GET /Profile/Data` JSON endpoint; client polls every 30 s and updates DOM without full reload
- [x] **Rent form** — datetime pickers (flatpickr) with blocked-range validation, start + end time inputs
- [x] **Return dialog** — `<dialog>` element with number input for end odometer, min-value enforced
- [x] **Non-trivial forms (2 required)** — current forms are login/register (text only) and rent/return (number + datetime). Add at least one form with a dropdown/select, checkbox, or textarea (e.g. block-vehicle reason with multiline text + dropdown for block type, or a vehicle-creation form for admin)
- [x] **Grid sorting at DB level** — booking history and vehicle list should pass `ORDER BY` to the query; add sort parameters to repository methods and API endpoints
- [x] **Full CRUD grid actions** — profile ride-history grid currently only displays; add a detail view per ride (distance, duration, price breakdown)

- [ ] **ApiDiscoveryController completeness** — extend reflected output to include request body schema, response shape, and query/route parameter descriptions to match OpenAPI spirit
- [ ] **Admin API — Vehicles full CRUD** — `POST /api/vehicles` (create), `PUT /api/vehicles/{id}` (update model/plate/odometer), `DELETE /api/vehicles/{id}`
- [ ] **Admin API — Parking lots full CRUD** — `POST /api/parkinglots`, `PUT /api/parkinglots/{id}`, `DELETE /api/parkinglots/{id}`
- [ ] **Admin API — Users management** — `PUT /api/users/{id}` (update role/email), `DELETE /api/users/{id}`
- [ ] **Admin API — All bookings list** — `GET /api/bookings` (admin sees all users' bookings) with pagination/sorting
- [ ] **Admin API — Block log history** — `GET /api/vehicles/{id}/blocks` returning full block log history for a vehicle

---

## Desktop Application (WPF / MAUI — Administrator)

### ⬜ TODO — entire desktop app not yet started

- [ ] **Project scaffold** — create WPF or MAUI project, add to solution
- [ ] **API client service** — typed `HttpClient` with JWT token attachment; all calls async
- [ ] **Admin token acquisition** — hardcoded admin credentials or config-based login to obtain JWT on startup
- [ ] **Users grid** — list all users, sortable; actions: edit role/email, delete
- [ ] **Vehicles grid** — list all vehicles with status; create, edit, delete; block/unblock with reason
- [ ] **Parking lots grid** — list lots with GPS; create, edit, delete
- [ ] **Bookings grid** — all bookings across users; read-only for completed
- [ ] **Status history view** — per-vehicle timeline of status changes
- [ ] **Block logs view** — per-vehicle list of block records
- [ ] **Statistics view** — overview stats + per-vehicle last-month stats
- [ ] **Form validation** — all input forms validate before API call; show field-level errors
- [ ] **State-aware dialogs** — e.g. vehicle detail disables "block" when already blocked, shows "unblock" instead
