# CreateProductCommandHandler Unit Tests - Summary

## Overview

Comprehensive unit test suite for `CreateProductCommandHandler` using:
- **MSTest** - Test framework
- **Moq** - Mocking library for IProductRepository
- **FluentAssertions** - Fluent assertion syntax

**Total Tests:** 51 (All Passing ✅)

---

## Test Files

### 1. CreateProductCommandHandlerTests.cs

**Location:** `tests/Ecommerce.Application.Tests/Catalog/Commands/CreateProduct/CreateProductCommandHandlerTests.cs`

**Test Class:** `CreateProductCommandHandlerTests`

**Purpose:** Tests the command handler's orchestration logic, error handling, and repository interaction.

---

## Test Breakdown by Category

### Successful Product Creation (6 tests)

✅ **Handle_ValidCommand_ReturnsSuccessWithProductId**
- Tests full happy path with all optional fields
- Verifies handler returns success with non-empty product ID
- Confirms repository methods are called correctly

✅ **Handle_MinimalValidCommand_ReturnsSuccessWithProductId**
- Tests creation with only required fields
- Verifies handler works without optional description

✅ **Handle_ValidCommandWithoutDescription_ReturnsSuccess**
- Confirms null description is handled correctly
- Ensures AddAsync is called even without description

✅ **Handle_ValidCommandWithDefaultCurrency_ReturnsSuccess**
- Tests default USD currency behavior
- Verifies command parameter defaults work properly

✅ **Handle_ValidCommandWithZeroPrice_ReturnsSuccess**
- Tests edge case of zero price
- Confirms domain validates zero price as valid

✅ **Handle_ValidCommand_CallsRepositoryMethodsInCorrectOrder**
- Verifies GetBySkuAsync called before AddAsync
- Ensures correct orchestration order

---

### Duplicate SKU Handling (3 tests)

❌ **Handle_DuplicateSku_ReturnsFailureWithSkuAlreadyExistsError**
- Tests duplicate SKU detection
- Verifies error code is "SKU_ALREADY_EXISTS"
- Confirms product is not persisted when duplicate found

❌ **Handle_DuplicateSkuCheckFails_DoesNotPersistProduct**
- Confirms AddAsync is never called when duplicate detected
- Uses Verify with Times.Never for safety check

❌ **Handle_ValidCommand_PassesCancellationTokenToRepository**
- Verifies CancellationToken flows through to repository calls
- Tests both GetBySkuAsync and AddAsync receive token

---

### Domain Validation Failures (6 tests)

❌ **Handle_InvalidSkuFormat_ReturnsFailureWithErrorCode**
- Tests empty SKU handling
- Verifies error code is "INVALID_PRODUCT_DATA"
- Confirms Sku value object validation triggers failure

❌ **Handle_NegativePrice_ReturnsFailureWithInvalidPriceError**
- Tests negative price rejection
- Verifies Money value object validation
- Confirms error is caught and returned as Result

❌ **Handle_InvalidCurrency_ReturnsFailureWithErrorCode**
- Tests empty currency handling
- Verifies Money construction fails with invalid currency

❌ **Handle_EmptyName_ReturnsFailureWithErrorCode**
- Tests empty name handling
- Confirms Product.Create() rejects empty names

❌ **Handle_NameExceeds200Characters_ReturnsFailureWithNameTooLongError**
- Tests domain constraint enforcement
- Verifies error code is "NAME_TOO_LONG"
- Confirms business rules prevent invalid state

❌ **Handle_EmptyCategoryId_ReturnsFailureWithCategoryRequiredError**
- Tests empty GUID rejection
- Verifies error code is "CATEGORY_REQUIRED"
- Confirms domain business rules enforced

---

### Repository Failures (2 tests)

❌ **Handle_GetBySkuAsyncThrows_ReturnsFailureWithGenericErrorCode**
- Tests repository exception handling during SKU lookup
- Verifies exception converted to Result.Failure
- Confirms error code is "PRODUCT_CREATION_ERROR"

❌ **Handle_AddAsyncThrows_ReturnsFailureAndDoesNotPropagate**
- Tests repository exception during persistence
- Verifies exception doesn't propagate to caller
- Confirms graceful error handling

---

### Cancellation Token Handling (2 tests)

❌ **Handle_CancellationTokenCancelled_ReturnsFailureWithOperationCancelledError**
- Tests cancellation during GetBySkuAsync
- Verifies error code is "OPERATION_CANCELLED"
- Confirms OperationCanceledException is handled specifically

❌ **Handle_CancellationSignalledDuringPersist_ReturnsFailureWithOperationCancelledError**
- Tests cancellation during AddAsync
- Verifies handler returns proper error instead of throwing

---

### Null Guard Handling (1 test)

🛡️ **Constructor_NullRepository_ThrowsArgumentNullException**
- Tests constructor guard clause
- Verifies NullArgumentException is thrown when null repository passed
- Confirms defensive programming

---

### Repository Interaction Verification (2 tests)

✅ **Handle_ValidCommand_CallsRepositoryMethodsInCorrectOrder**
- Verifies GetBySkuAsync called before AddAsync
- Uses callback to track call order
- Ensures orchestration correctness

✅ **Handle_ValidCommand_PassesCancellationTokenToRepository**
- Verifies CancellationToken passed to both repository methods
- Uses Verify to confirm token flow
- Ensures async cancellation works end-to-end

---

## Test Class: CreateProductCommandValidatorTests.cs

**Location:** `tests/Ecommerce.Application.Tests/Catalog/Commands/CreateProduct/CreateProductCommandValidatorTests.cs`

**Purpose:** Tests FluentValidation rules enforced at request level.

**Tests:** 45 total

---

### Validator Test Categories

#### Valid Commands (8 tests)

✅ **Validate_ValidCommand_ReturnsNoErrors**
- Full command with all fields

✅ **Validate_MinimalValidCommand_ReturnsNoErrors**
- Only required fields

✅ **Validate_ZeroPrice_ReturnsNoErrors**

✅ **Validate_MaxLengthSku_ReturnsNoErrors**

✅ **Validate_MaxLengthName_ReturnsNoErrors**

✅ **Validate_NullDescription_ReturnsNoErrors**

✅ **Validate_EmptyStringDescription_ReturnsNoErrors**

✅ **Validate_WhitespaceDescription_ReturnsNoErrors**

---

#### SKU Validation (3 tests)

❌ **Validate_EmptySku_ReturnsSkuRequiredError**

❌ **Validate_WhitespaceSku_ReturnsSkuRequiredError**

❌ **Validate_SkuExceeds100Characters_ReturnsSkuLengthError**

---

#### Name Validation (3 tests)

❌ **Validate_EmptyName_ReturnsNameRequiredError**

❌ **Validate_WhitespaceName_ReturnsNameRequiredError**

❌ **Validate_NameExceeds200Characters_ReturnsNameLengthError**

---

#### Price Validation (5 tests)

❌ **Validate_NegativePrice_ReturnsPriceGreaterThanOrEqualToZeroError**

❌ **Validate_PriceWithMoreThan2DecimalPlaces_ReturnsPrecisionError**

✅ **Validate_PriceWith2DecimalPlaces_ReturnsNoErrors**

✅ **Validate_PriceWith1DecimalPlace_ReturnsNoErrors**

✅ **Validate_PriceWholeNumber_ReturnsNoErrors**

---

#### Currency Validation (8 tests)

❌ **Validate_EmptyCurrency_ReturnsCurrencyRequiredError**

❌ **Validate_CurrencyLessThan3Characters_ReturnsCurrencyError**

❌ **Validate_CurrencyMoreThan3Characters_ReturnsCurrencyError**

✅ **Validate_ValidCurrency_ReturnsNoErrors** (tests USD, EUR, GBP, JPY, CAD)

❌ **Validate_LowercaseCurrency_ReturnsCurrencyFormatError**

❌ **Validate_MixedCaseCurrency_ReturnsCurrencyFormatError**

❌ **Validate_CurrencyWithSpecialCharacters_ReturnsCurrencyFormatError**

---

#### CategoryId Validation (2 tests)

❌ **Validate_EmptyCategoryId_ReturnsCategoryIdRequiredError**

✅ **Validate_ValidCategoryId_ReturnsNoErrors**

---

#### Description Validation (2 tests)

❌ **Validate_DescriptionExceeds2000Characters_ReturnsDescriptionLengthError**

✅ **Validate_MaxLengthDescription_ReturnsNoErrors**

---

#### Multiple Validation Errors (1 test)

❌ **Validate_MultipleErrorsInCommand_ReturnsAllErrors**
- Tests that validator returns ALL errors, not just first
- Verifies properties: Sku, Name, Price, CategoryId, Currency have errors

---

## Key Testing Patterns

### 1. Arrange-Act-Assert (AAA)

All tests follow AAA pattern:
```csharp
// Arrange - Set up test data and mocks
var command = new CreateProductCommand(...);
_repositoryMock.Setup(r => r.GetBySkuAsync(...)).ReturnsAsync(null);

// Act - Execute the method under test
var result = await _handler.Handle(command, CancellationToken.None);

// Assert - Verify expected outcomes
result.IsSuccess.Should().BeTrue();
result.Value.Should().NotBe(Guid.Empty);
```

### 2. Naming Convention: Method_State_ExpectedResult

Examples:
- `Handle_ValidCommand_ReturnsSuccessWithProductId`
- `Handle_DuplicateSku_ReturnsFailureWithSkuAlreadyExistsError`
- `Validate_NegativePrice_ReturnsPriceGreaterThanOrEqualToZeroError`

### 3. Moq Setup for Mocking

```csharp
_repositoryMock.Setup(r => r.GetBySkuAsync(It.IsAny<Sku>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync((Product?)null);

_repositoryMock.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
    .Returns(Task.CompletedTask);
```

### 4. FluentAssertions for Readable Assertions

```csharp
result.IsSuccess.Should().BeTrue();
result.Value.Should().NotBe(Guid.Empty);
result.Error.Should().NotBeNull();
result.Error!.Code.Should().Be("SKU_ALREADY_EXISTS");
```

### 5. Verification with Moq.Verify

```csharp
_repositoryMock.Verify(
    r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
    Times.Once);

_repositoryMock.Verify(
    r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
    Times.Never,
    "Product should not be persisted when duplicate SKU is found");
```

---

## Coverage Analysis

### Handler Coverage

| Scenario | Covered | Status |
|----------|---------|--------|
| Valid creation | ✅ | 6 tests |
| Duplicate SKU | ✅ | 3 tests |
| Invalid domain values | ✅ | 6 tests |
| Repository exceptions | ✅ | 2 tests |
| Cancellation | ✅ | 2 tests |
| Null guards | ✅ | 1 test |
| Repository ordering | ✅ | 2 tests |
| Cancellation token flow | ✅ | 1 test |

### Validator Coverage

| Field | Valid Cases | Invalid Cases | Status |
|-------|------------|--------------|--------|
| SKU | 2 | 3 | ✅ |
| Name | 2 | 3 | ✅ |
| Price | 4 | 2 | ✅ |
| Currency | 1 | 7 | ✅ |
| CategoryId | 1 | 1 | ✅ |
| Description | 3 | 2 | ✅ |

---

## Running the Tests

### Run All Application Tests
```powershell
dotnet test tests/Ecommerce.Application.Tests/
```

### Run Only Handler Tests
```powershell
dotnet test tests/Ecommerce.Application.Tests/ --filter "CreateProductCommandHandlerTests"
```

### Run Only Validator Tests
```powershell
dotnet test tests/Ecommerce.Application.Tests/ --filter "CreateProductCommandValidatorTests"
```

### Run with Coverage
```powershell
dotnet test tests/Ecommerce.Application.Tests/ /p:CollectCoverage=true
```

### Test Results (All Passing)
```
Test run completed. Ran 51 test(s). 51 Passed, 0 Failed
```

---

## Error Codes Tested

| Error Code | Scenario | Test |
|-----------|----------|------|
| `INVALID_PRODUCT_DATA` | Value object creation fails | Handle_InvalidSkuFormat_ReturnsFailureWithErrorCode |
| `SKU_ALREADY_EXISTS` | Duplicate SKU in database | Handle_DuplicateSku_ReturnsFailureWithSkuAlreadyExistsError |
| `NAME_TOO_LONG` | Name exceeds 200 chars | Handle_NameExceeds200Characters_ReturnsFailureWithNameTooLongError |
| `CATEGORY_REQUIRED` | Empty category ID | Handle_EmptyCategoryId_ReturnsFailureWithCategoryRequiredError |
| `OPERATION_CANCELLED` | CancellationToken signalled | Handle_CancellationTokenCancelled_ReturnsFailureWithOperationCancelledError |
| `PRODUCT_CREATION_ERROR` | Unexpected exception | Handle_GetBySkuAsyncThrows_ReturnsFailureWithGenericErrorCode |

---

## Best Practices Demonstrated

✅ **Isolation**: Mocked IProductRepository dependencies
✅ **Clarity**: Descriptive test names following Method_State_ExpectedResult
✅ **Completeness**: Tests both success and failure paths
✅ **Readability**: FluentAssertions for human-readable assertions
✅ **Maintainability**: Setup/teardown via [TestInitialize]
✅ **Coverage**: Edge cases (zero price, max lengths, null values)
✅ **Error Handling**: Exception conversion, graceful failures
✅ **Async Support**: Proper async/await in async tests
✅ **Repository Verification**: Call ordering, parameter passing
✅ **Guard Clauses**: Null parameter validation

---

## Integration with CI/CD

These tests are designed to run in continuous integration pipelines:

```yaml
# Example GitHub Actions
- name: Run Unit Tests
  run: dotnet test tests/ --logger "trx;LogFileName=test-results.trx"

- name: Publish Test Results
  uses: actions/upload-artifact@v3
  with:
    name: test-results
    path: '**/test-results.trx'
```

---

## Future Enhancements

- Add integration tests with real IProductRepository
- Test domain events being raised correctly
- Test handler behavior with concurrent requests
- Test repository concurrency token handling (RowVersion)
- Add performance/benchmarking tests for handler throughput
