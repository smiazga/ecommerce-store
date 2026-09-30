namespace Ecommerce.Application.Common.Persistence;

/// <summary>
/// Abstraction for managing transaction boundaries.
/// Per ADR-002, handlers determine transaction scope and commit changes through this interface.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Commits all pending changes to the database in a single transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    Task SaveAsync(CancellationToken cancellationToken = default);
}
