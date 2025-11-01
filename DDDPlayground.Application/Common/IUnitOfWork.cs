namespace DDDPlayground.Application.Common;

/// <summary>
/// Unit of Work pattern interface.
/// Represents a transactional boundary for persisting changes.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Save all changes made in this unit of work.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
