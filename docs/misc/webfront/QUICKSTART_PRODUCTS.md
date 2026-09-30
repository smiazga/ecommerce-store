# Quick Start Guide: Ecommerce.Web Products

## Running the Application

### Prerequisites
- .NET 10 SDK
- Visual Studio 2026 or VS Code
- Ecommerce.Api running on `https://localhost:7001`

### Step 1: Start the API

```powershell
cd src/Ecommerce.Api
dotnet run
```

The API will start on:
- HTTPS: `https://localhost:7001`
- HTTP: `http://localhost:5001`

### Step 2: Start the Web App

In a new terminal:

```powershell
cd src/Ecommerce.Web
dotnet run
```

The Web app will start on:
- HTTPS: `https://localhost:7000`
- HTTP: `http://localhost:5000`

### Step 3: Navigate to Products

1. Open https://localhost:7000 in your browser
2. Click **"Products"** in the navigation menu
3. Browse, create, and view products

## Key Workflows

### View All Products
1. Navigate to `/products`
2. Products are displayed in a grid layout
3. Use pagination controls to navigate pages
4. Click "View Details" on any product card

### View Product Details
1. Click "View Details" on a product card OR
2. Navigate to `/products/{ProductId}` directly
3. View complete product information
4. See Edit and Delete buttons (Delete is placeholder)

### Create a Product (Placeholder)
1. Click "Create Product" button on products page
2. Navigate to `/products/create` (component not yet created)
3. Fill in product form:
   - SKU (required)
   - Name (required)
   - Description (optional)
   - Price (required)
   - Currency (default: USD)
   - Category ID (required)

## Configuration

### API URL
Edit `appsettings.json`:
```json
{
  "ApiSettings": {
    "ProductsApiUrl": "https://localhost:7001/api/products"
  }
}
```

### Logging
Edit `appsettings.Development.json` to adjust log levels:
```json
{
  "Logging": {
    "LogLevel": {
      "Ecommerce.Web": "Debug"
    }
  }
}
```

## Troubleshooting

### 404 Not Found - Products Not Showing
- Verify API is running on correct port
- Check `appsettings.json` API URL
- Check browser console for network errors

### HTTP 500 - Server Error
- Check Visual Studio Output window for exceptions
- Verify API database connectivity
- Check log level settings

### Slow Performance
- Check browser network tab (F12)
- Verify API response times
- Check for timeout issues (30 second default)

## Project Files Reference

| File | Purpose |
|------|---------|
| `Clients/IProductApiClient.cs` | Typed HttpClient interface |
| `Clients/ProductApiClient.cs` | HTTP implementation |
| `Services/Products/IProductService.cs` | Business service interface |
| `Services/Products/ProductService.cs` | DTO mapping & logic |
| `ViewModels/Products/*` | UI data models |
| `Components/Products/*.razor` | Blazor components |
| `Extensions/ServiceCollectionExtensions.cs` | DI setup |
| `Program.cs` | App configuration |

## Next Steps

1. **Create Form** - Implement `/products/create` with EditForm
2. **Delete Method** - Add DELETE endpoint to API and service
3. **Update Method** - Add PUT endpoint for editing products
4. **Search** - Add search functionality with query parameter
5. **Filtering** - Add status/category filters
6. **Caching** - Implement response caching
7. **Validation** - Add client-side form validation
8. **Tests** - Write unit and integration tests

## Common Development Tasks

### Adding a New Service Method

1. Add method signature to `IProductApiClient`
2. Implement in `ProductApiClient`
3. Add method to `IProductService`
4. Implement translation logic in `ProductService`
5. Create/update ViewModel if needed
6. Update component to call new method

### Adding a New Razor Component

1. Create `.razor` file in `Components/Products/`
2. Add `@page` directive if routable
3. Inject `IProductService` if needed
4. Follow component naming conventions
5. Keep logic minimal (use services)

### Debugging

- Set breakpoints in service methods
- Use browser F12 Developer Tools
- Check Application Output window in VS
- Enable Debug logging in appsettings

## Documentation References

- [Dependency Rules](../docs/architecture/dependency-rules.md)
- [Coding Standards](../docs/coding-standards.md)
- [Implementation Summary](./IMPLEMENTATION_SUMMARY.md)
- [System Architecture](../docs/architecture/system-architecture.md)
