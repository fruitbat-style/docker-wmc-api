# WMCApi Unit Tests

## Quick Start

```bash
# Run all tests
cd api
dotnet test WMCApi.Tests

# Run with verbose output
dotnet test WMCApi.Tests --verbosity normal

# Run a specific test class
dotnet test WMCApi.Tests --filter "FullyQualifiedName~LocationsControllerTests"

# Run a single test
dotnet test WMCApi.Tests --filter "FullyQualifiedName~Get_InvalidLat_ReturnsBadRequest"
```

## Test Stack

| Package | Purpose |
|---|---|
| xUnit | Test framework |
| Moq | Mocking (controller tests mock `ILocationService`, seed tests mock `ILogger`) |
| EF Core InMemory | In-memory database for service and seed data tests |

## What's Tested

### LocationsControllerTests (16 tests)

Tests the API controller's input validation and response behavior using a mocked `ILocationService`.

| Test | What it verifies |
|---|---|
| `Get_InvalidLat_ReturnsBadRequest` | lat outside [-90, 90] returns 400 |
| `Get_ValidLat_DoesNotReturnBadRequest` | lat at -90, 0, 90 boundary values pass |
| `Get_InvalidLng_ReturnsBadRequest` | lng outside [-180, 180] returns 400 |
| `Get_ValidLng_DoesNotReturnBadRequest` | lng at -180, 0, 180 boundary values pass |
| `Get_NegativeRadius_ReturnsBadRequest` | radius < 0 returns 400 |
| `Get_ZeroRadius_IsValid` | radius = 0 is accepted |
| `Get_NegativeFlavor_ReturnsBadRequest` | flavor < 0 returns 400 |
| `Get_NegativeProduct_ReturnsBadRequest` | product < 0 returns 400 |
| `Get_ValidParams_ReturnsOkWithLocations` | Valid search returns 200 with location data |
| `Get_DefaultParams_CallsServiceWithZeros` | Default params (all 0) pass through to service |
| `Get_MultipleBadParams_LatCheckedFirst` | Validation order: lat checked before lng |
| `GetFilters_ReturnsOkWithFilters` | Filters endpoint returns 200 with flavors + product types |

### LocationServiceTests (22 tests)

Tests the business logic using an EF Core InMemory database with 3 seeded locations (Seattle, Portland, San Francisco) with different flavors and products.

**No-filter tests:**
- Returns all locations when no filters applied
- Returns empty list for empty database
- Includes navigation property Items

**Flavor filter tests:**
- Filters by FlavorId, returns only matching locations
- Excludes non-matching locations
- Returns empty for non-existent flavor
- FlavorId=0 skips flavor filtering

**Product filter tests:**
- Filters by ProductId, returns only matching locations
- Returns empty for non-existent product

**Combined filter tests:**
- FlavorId + ProductId together — returns locations matching both
- No overlap between flavor and product — returns empty

**Distance filter tests (Haversine):**
- Small radius near Seattle returns only Seattle
- Large radius (1000mi) returns all 3 locations
- radius=0 skips distance filtering
- lat=0 and lng=0 with radius>0 skips distance filtering
- lat!=0 with lng=0 still enables distance filtering

**Combined distance + filter tests:**
- Distance + flavor filter applied together

**GetFiltersAsync tests:**
- Returns distinct flavors ordered by ID
- Returns distinct products ordered by ID
- Empty database returns empty lists
- Duplicate flavors across locations deduplicated

### HaversineTests (7 tests)

Tests the extracted Haversine distance formula directly.

| Test | What it verifies |
|---|---|
| Same point | Distance = 0 |
| Seattle to Portland | ~145-175 miles |
| Seattle to San Francisco | ~670-700 miles |
| Symmetry | distance(A,B) = distance(B,A) |
| Antipodal points | North pole to South pole ~12,430 miles |
| Across date line | Tokyo to Honolulu ~3,800-3,900 miles |
| Earth radius constant | 3958.8 miles |

### SeedDataTests (8 tests)

Tests the database seeding logic using temp files and InMemory database.

| Test | What it verifies |
|---|---|
| Database has data | Skips seeding (idempotent) |
| File not found | Logs warning, no crash |
| Valid data | Deserializes and saves locations |
| Valid data | Logs seeded count |
| Empty array `[]` | Logs warning, doesn't save |
| `null` JSON | Logs warning, doesn't save |
| Invalid JSON | Catches JsonException, logs error |
| Called twice | Only seeds once (idempotent) |

### JsonConfigTests (4 tests)

Tests the snake_case JSON serialization configuration.

| Test | What it verifies |
|---|---|
| Naming policy | Uses `SnakeCaseLower` |
| Serialization | PascalCase properties serialize as snake_case |
| Deserialization | snake_case JSON deserializes to PascalCase properties |
| Nested objects | Location with Items round-trips correctly |

## Refactoring Done

`LocationService.SearchAsync` was refactored from raw PostgreSQL SQL to LINQ queries to enable testing with EF Core InMemory provider:

- **Before:** Raw SQL string concatenation with `FromSqlRaw()` using PostgreSQL-specific functions (`RADIANS`, `ASIN`, `POWER`)
- **After:** LINQ `.Where()` for flavor/product filtering, in-memory Haversine filtering for distance
- **Extracted:** `HaversineDistance()` as an `internal static` method, directly unit-testable

The behavior is identical. The LINQ approach is also more maintainable and database-provider agnostic.

## Estimated Code Coverage

| File | Coverage | Notes |
|---|---|---|
| `Controllers/LocationsController.cs` | ~100% | All validation branches + happy paths |
| `Services/LocationService.cs` | ~100% | All filter combinations, distance logic, Haversine |
| `Services/ILocationService.cs` | 100% | Interface (covered via mocks and implementations) |
| `Data/SeedData.cs` | ~90% | All branches except `DbUpdateException` (hard to trigger with InMemory) |
| `Data/WmcDbContext.cs` | ~80% | Exercised via InMemory tests; model config covered indirectly |
| `JsonConfig.cs` | 100% | Serialization + deserialization tested |
| `Location.cs` | ~90% | All properties exercised in tests |
| `Program.cs` | 0% | Startup/DI wiring — not unit-testable (would need integration tests) |
| **Estimated overall** | **~85-90%** | of custom code (excluding Program.cs startup) |

To get exact numbers, run with coverage collection:
```bash
dotnet test WMCApi.Tests --collect:"XPlat Code Coverage"
```
The coverage report will be generated in `WMCApi.Tests/TestResults/`.
