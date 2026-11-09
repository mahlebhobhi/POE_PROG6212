# RaceDay — Part 3 MVC Web Application

ASP.NET Core 8 MVC front end for the RaceDay platform. The MVC application consumes the Part 2 REST API for every data operation; it does not connect to SQL Server or Azure Blob Storage directly.

## Solution structure

```text
RaceDay.sln
├── RaceDay.Api/                 # Part 2 REST API + Azure Blob upload endpoints
├── RaceDay.Mvc/                 # Part 3 ASP.NET Core MVC front end
│   ├── Controllers/             # Account, organiser, participant, profile
│   ├── Models/                  # MVC view models / API response models
│   ├── Services/RaceDayApiClient.cs # API client and API session-cookie forwarding
│   ├── Views/                   # Razor pages
│   └── wwwroot/css/site.css     # RaceDay theme
├── RaceDay.Tests/               # Part 2 API tests
├── docs/part1/                  # Original Part 1 ERD, SQL and endpoint plan
├── .github/workflows/dotnet.yml # CI build and test
├── docker-compose.yml
└── README.md
```

## Requirements
- .NET 8 SDK (Visual Studio 2022 with ASP.NET and web development workload is suitable)
- SQL Server / LocalDB for local API development
- Docker Desktop with Docker Compose for container deployment
- An Azure Storage account for live image uploads

## Run locally (Visual Studio)

1. Open `RaceDay.sln`.
2. Configure `RaceDay.Api/appsettings.json` connection string for your SQL Server. The starter API uses EF Core Code First and its current model expects `Users`, `Events`, `Categories`, `Enrollments`, and `Results` tables.
3. Configure the API database using EF Core migrations:
   ```bash
   dotnet tool install --global dotnet-ef
   dotnet ef migrations add InitialCreate --project RaceDay.Api
   dotnet ef database update --project RaceDay.Api
   ```
4. Run `RaceDay.Api` using its included launch profile at `https://localhost:7043` (or `http://localhost:5043`). Confirm `https://localhost:7043/` reports that the API is running.
5. Keep `RaceDay.Mvc/appsettings.json` `Api:BaseUrl` set to `https://localhost:7043/`.
6. Run `RaceDay.Mvc` and browse to the MVC URL shown by Visual Studio.
7. Register a Participant and an Organiser account. Test each role in a separate browser session.

### API session forwarding
The API issues an HTTP-only `.RaceDay.Session` cookie at login. The MVC API client stores that API cookie inside the MVC server-side session and forwards it on subsequent API requests. The MVC project does not store the user's password or connect to the database.

## Docker — single command

From the solution root:

```bash
docker compose up --build
```

Open **http://localhost:8080**. The Compose stack starts SQL Server, the API, and MVC. In this local Compose profile, `Database:EnsureCreated=true` creates the API's current EF model on first start. Do not use the sample SA password in a public or production environment. Override it before running:

```bash
MSSQL_SA_PASSWORD='Use-A-Long-Unique-Password-123!' docker compose up --build
```

The MVC container uses `Api:BaseUrl=http://api:8080/` so it can reach the API by its Compose service name. The SQL Server port is exposed at `localhost:1433` for local inspection.

### Standalone Docker run command (MVC image)
If the Part 2 API is already running and reachable from the Docker host, build and run MVC separately:

```bash
docker build -t raceday-mvc -f RaceDay.Mvc/Dockerfile RaceDay.Mvc
docker run --rm -p 8080:8080 -e Api__BaseUrl=http://host.docker.internal:8081/ raceday-mvc
```

In this standalone example, expose your API on host port `8081` and adjust `Api__BaseUrl` if needed. For a complete stack, use the Compose command above.


## Azure Blob Storage configuration

Create an Azure Storage account and a container (the default container name is `raceday-media`). Do not commit the connection string. Set it as an environment variable / user secret:

```bash
dotnet user-secrets init --project RaceDay.Api
dotnet user-secrets set "AzureBlobStorage:ConnectionString" "<your-storage-connection-string>" --project RaceDay.Api
dotnet user-secrets set "AzureBlobStorage:ContainerName" "raceday-media" --project RaceDay.Api
```

For Docker Compose, export `AZURE_BLOB_CONNECTION_STRING` before `docker compose up`. The API accepts JPG, PNG and WebP images up to 5 MB at `POST /api/events/{id}/banner` and `POST /api/users/me/picture`, checks the current session and role, uploads the bytes to Azure, and stores the resulting URL in the API database. MVC sends the file to the API; MVC never contacts Azure directly. The container is configured for blob-readable URLs so event banners can render in browsers. For production, prefer private containers and short-lived SAS URLs if required by the deployment security policy.

## Features implemented in MVC

### Public / account
- Public event listing and event summary cards
- Registration for Participant or Organiser through the API
- Login and logout through the API
- Server-side session state for user ID, name, role and API cookie

### Organiser
- Dashboard with event count, enrolment count and upcoming events
- Create, edit and delete owned events
- Upload event banner through the API
- Add age / distance categories
- View event enrolments and participant/category information
- Record finishing time and position; view published results

### Participant
- Upcoming event list with search and event-type filter
- Event detail and available categories
- Enrol in a selected category
- View personal enrolments and confirmation status
- View personal race results
- Update profile details and upload profile picture through API

### UI / validation
- Bootstrap 5 responsive layout and consistent RaceDay sporting theme
- Role-specific navigation
- Form validation, feedback messages and confirmation before event deletion
- Distinct status styling for confirmed entries

## API endpoints used by MVC

| Method | Endpoint | MVC use |
|---|---|---|
| POST | `/api/auth/register` | Registration |
| POST | `/api/auth/login` | Login / create API session |
| POST | `/api/auth/logout` | Logout |
| GET / PUT | `/api/users/me` | View / update profile |
| POST | `/api/users/me/picture` | Upload profile image |
| GET | `/api/events` | Public and role-specific event lists |
| GET | `/api/events/{id}` | Event details |
| POST / PUT / DELETE | `/api/events` and `/api/events/{id}` | Event CRUD |
| POST | `/api/events/{id}/banner` | Upload event banner |
| GET / POST | `/api/events/{id}/categories` | Category management |
| POST | `/api/enrollments/events/{id}` | Participant enrolment |
| GET | `/api/enrollments/mine` | Participant's entries |
| GET | `/api/enrollments/events/{id}` | Organiser's event entries |
| POST | `/api/results` | Record race result |
| GET | `/api/results/mine` | Participant's result history |
| GET | `/api/results/events/{id}` | Organiser's event results |

## CI and tests

Run locally:

```bash
dotnet restore RaceDay.sln
dotnet build RaceDay.sln --configuration Release
dotnet test RaceDay.sln --configuration Release
```

GitHub Actions runs restore, build and tests on pushes and pull requests. After pushing this repository to your GitHub account:
1. Open **Actions** and confirm the workflow completes with a green check.
2. Capture a screenshot of the successful run and save it as `docs/ci-build.png`.
3. Record a demonstration video and add its unlisted link below.

**CI screenshot:** add `docs/ci-build.png` after your own GitHub workflow has run.

**Demonstration video:** add your own unlisted video URL after recording the application.

## Important Part 1 database compatibility note — resolve before final submission

The supplied Part 1 SQL/ERD is preserved in `docs/part1/`. It defines `UserAccount`, separate `Organiser` and `Participant` profile tables, `Event`, reusable `Category`, join table `EventCategory`, `Enrolment`, and `Result`. The supplied Part 2 API currently uses a different simplified EF Core model (`User`, `RaceEvent`, event-owned `EventCategory`, `Enrollment`, `RaceResult`). These are **not the same physical schema**. Therefore, the Part 2 API's current EF model cannot simply be pointed at the Part 1 `RaceDayDB` created by the SQL script. For the final integrated submission, the API entities / EF mappings and API identity handling must be migrated to the Part 1 schema (including mapping `UserAccount.UserID` to the correct OrganiserID / ParticipantID and mapping the EventCategory composite key) OR the lecturer must explicitly approve the Part 2 model as a replacement. Do not run the Part 1 SQL script and expect the current API to work against it without this migration.

## Final assessment checklist

- [ ] Configure a real SQL Server connection and verify the approved schema / migration.
- [ ] Configure Azure Blob Storage secrets and test event and profile image uploads.
- [ ] Test login, logout, session expiry, and direct URL protection for both roles.
- [ ] Test every organiser and participant workflow with real records.
- [ ] Run `dotnet build` and `dotnet test` successfully on a machine with .NET 8.
- [ ] Run `docker compose up --build` and verify the same workflows at `http://localhost:8080`.
- [ ] Push to your GitHub repository and make at least 20 meaningful development commits (do not fabricate commits).
- [ ] Capture the actual green GitHub Actions screenshot.
- [ ] Record the full application demo including MVC pages, Blob uploads, API integration and Docker.

> This archive is a prepared implementation package, not a verified 100% result. The current environment does not have the .NET SDK, SQL Server or Azure credentials, so compilation, live database integration, cloud uploads, Docker execution and GitHub CI could not be run here. Complete the checklist and address the schema compatibility note before submitting.
