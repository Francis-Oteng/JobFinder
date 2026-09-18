# JobFinder
A smart job-matching platform that connects candidates with relevant job opportunities based on skills, experience, and preferences.

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![Status](https://img.shields.io/badge/status-active-brightgreen.svg)

## Overview

JobMatch helps job seekers find roles that fit their skills and career goals, and helps employers find qualified candidates faster. The platform uses a matching algorithm that scores candidates against job postings based on skills, experience level, location preferences, and salary expectations.

## Features

- **Smart Matching** — Ranks job postings for each candidate using a weighted scoring algorithm (skills, experience, location, salary fit)
- **Candidate Profiles** — Resume parsing, skills tagging, and preference settings
- **Employer Dashboard** — Post jobs, review ranked candidate matches, and manage applications
- **Search & Filters** — Full-text search with filters for location, salary range, job type, and remote options
- **Notifications** — Email/in-app alerts when new matches appear
- **Application Tracking** — Status pipeline (applied → reviewed → interview → offer → hired/rejected)

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | C#, ASP.NET Core Web API |
| Database | SQL Server (Entity Framework Core) |
| Auth | JWT Bearer Authentication |
| Testing | xUnit |
| Hosting | Docker, Azure App Service |

## Getting Started

### Prerequisites

- .NET SDK ≥ 8.0
- SQL Server (LocalDB, Express, or full instance)
- Entity Framework Core CLI tools (`dotnet tool install --global dotnet-ef`)

### Installation

```bash
# Clone the repository
git clone https://github.com/your-org/JobFinder.git
cd JobFinder

# Restore dependencies
dotnet restore

# Copy configuration template
cp appsettings.example.json appsettings.Development.json
```

### Configuration

Update `appsettings.Development.json` with your local settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=JobFinderDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Issuer": "JobFinderApi",
    "Audience": "JobFinderClient",
    "Key": "your-secret-key-min-32-characters",
    "ExpiryMinutes": 60
  }
}
```

### Database Setup

```bash
# Apply migrations
dotnet ef database update

# (Optional) Seed sample data
dotnet run --project src/JobFinder.Api -- --seed
```

### Running the App

```bash
# Start the API
dotnet run --project src/JobFinder.Api

# API will be available at https://localhost:5001
# Swagger UI at https://localhost:5001/swagger
```

## Project Structure

```
jobmatch/
├── src/
│   ├── JobFinder.Api/            # Controllers, Program.cs, middleware, JWT config
│   ├── JobFinder.Core/           # Domain models, interfaces, matching logic
│   ├── JobFinder.Infrastructure/ # EF Core DbContext, repositories, migrations
│   └── JobFinder.Services/       # Business logic (matching algorithm, etc.)
├── tests/
│   ├── JobFinder.UnitTests/
│   └── JobFinder.IntegrationTests/
├── docs/                        # Additional documentation
└── JobMatch.sln
```

## API Reference

### Authentication

```
POST   /api/auth/register       Register a new user
POST   /api/auth/login          Log in and receive a JWT bearer token
POST   /api/auth/refresh        Refresh an expired access token
```

Protected endpoints require an `Authorization: Bearer <token>` header.

### Candidates

```
GET    /api/candidates/:id           Get candidate profile
PUT    /api/candidates/:id           Update candidate profile
GET    /api/candidates/:id/matches   Get ranked job matches
```

### Jobs

```
GET    /api/jobs                 List/search job postings
POST   /api/jobs                 Create a job posting (employer)
GET    /api/jobs/:id              Get job details
GET    /api/jobs/:id/candidates   Get ranked candidate matches (employer)
```

### Applications

```
POST   /api/applications              Submit an application
GET    /api/applications/:id          Get application status
PATCH  /api/applications/:id/status   Update application status (employer)
```

## Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true

# Run a specific test project
dotnet test tests/JobMatch.UnitTests
```

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/your-feature`)
3. Commit your changes (`git commit -m 'Add some feature'`)
4. Push to the branch (`git push origin feature/your-feature`)
5. Open a Pull Request

Please make sure tests pass and follow the existing code style before submitting.

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

## Contact

Questions or feedback? Open an issue or reach out at **team@JobFinder.example.com**.
