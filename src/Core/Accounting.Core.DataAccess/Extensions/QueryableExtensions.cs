using Accounting.Shared.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Accounting.Core.DataAccess.Extensions;

public static class QueryableExtensions
{
    #region Operations
    public static async Task<Result<T>> FirstOrNotFoundAsync<T>(
        this IQueryable<T> query ,
        Expression<Func<T , bool>> predicate ,
        string notFoundMessage ,
        CancellationToken cancellationToken = default)
        where T : class
    {
        bool exists = await query.AnyAsync(predicate, cancellationToken);

        if (!exists)
        {
            return Result<T>.NotFound(notFoundMessage);
        }

        T entity = await query.FirstAsync(predicate, cancellationToken);
        return Result<T>.Success(entity);
    }
    #endregion Operations
}