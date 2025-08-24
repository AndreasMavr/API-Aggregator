API Aggregator

This is a .NET 8 Web API that aggregates data from multiple external APIs (OpenWeatherMap, NewsAPI, GitHub) into a single unified endpoint.  
It demonstrates parallel API calls, caching, error handling with fallback, request statistics, and JWT authentication.

Features

- Aggregates weather, news, and GitHub repositories
- Parallel async calls with Task.WhenAll
- Caching with in-memory cache and last-known-good fallback
- Resilience with Polly (retry, timeout, circuit breaker)
- Request statistics endpoint with buckets (fast, medium, slow)
- JWT authentication with /auth/token endpoint
- Swagger UI documentation

Requirements

- .NET SDK 8.0
- API keys for:
  - OpenWeatherMap
  - NewsAPI
  - GitHub (optional, otherwise subject to rate limits)

Configuration

In file `appsettings.Development.json` replace ApiKey with actual API keys and SigningKey with a 32 char token:

```json
{
  "OpenWeatherMap": {
    "BaseUrl": "https://api.openweathermap.org/data/2.5/",
    "ApiKey": "REPLACE_ME",
    "DefaultCity": "Athens",
    "CacheSeconds": 60
  },
  "NewsApi": {
    "BaseUrl": "https://newsapi.org/v2/",
    "ApiKey": "REPLACE_ME",
    "DefaultQuery": "technology",
    "CacheSeconds": 90
  },
  "GitHub": {
    "BaseUrl": "https://api.github.com/",
    "Token": null,
    "CacheSeconds": 120
  },
  "Stats": {
    "FastMs": 100,
    "MediumMs": 200,
    "AnomalyFactor": 1.5
  },
  "Auth": {
    "Issuer": "ApiAggregator",
    "Audience": "ApiAggregator",
    "SigningKey": "CHANGE_ME_TO_A_32+_CHAR_SECRET"
  }
}
```
Do not commit real keys.

- Running
  
cd src/ApiAggregator

dotnet build

ASPNETCORE_ENVIRONMENT=Development dotnet run


Swagger UI will be available at http://localhost:5000/swagger

- Authentication

Call /auth/token?user=demo to get a JWT.

In Swagger click Authorize and paste Bearer <token>.

Call /api/aggregate or /api/stats.


- Endpoints

GET /api/aggregate
Aggregates data from weather, news, and GitHub. Supports filtering and sorting via query parameters.

GET /api/stats
Returns request statistics for each API, including average response times and buckets.

GET /auth/token
Issues a JWT for testing authentication.

- Tests

Run unit tests with:

cd tests/ApiAggregator.Tests

dotnet test
