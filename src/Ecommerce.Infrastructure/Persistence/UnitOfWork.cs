namespace Ecommerce.Infrastructure.Persistence;

using Ecommerce.Application.Common.Persistence;

/// <summary>
/// Implementation of IUnitOfWork using EF Core DbContext.
/// Manages transaction boundaries for application handlers.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly EcommerceDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </summary>
    /// <param name="dbContext">The EF Core DbContext managing the database connection.</param>
    public UnitOfWork(EcommerceDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Commits all pending changes to the database in a single transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
