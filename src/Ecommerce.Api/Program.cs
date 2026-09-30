using Ecommerce.Infrastructure.DependencyInjection;
using Ecommerce.Api.Endpoints.Products;
using Ecommerce.Application.DependencyInjection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure persistence (SQL Server) for development by default. The connection
// string is read from the DefaultConnection entry in configuration. This keeps
// Program.cs small and lets environment-specific appsettings override values.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    // Register Infrastructure persistence using SQL Server when a connection string is present
    // Configure SQL Server with resilient retries and an increased command timeout to
    // avoid transient failures during migrations.
    builder.Services.AddInfrastructurePersistence(options =>
        options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure().CommandTimeout(180)));
}

// Register Application layer services (MediatR, Validators, etc.)
builder.Services.AddApplicationServices();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map API endpoints
app.MapProductEndpoints();

app.Run();
