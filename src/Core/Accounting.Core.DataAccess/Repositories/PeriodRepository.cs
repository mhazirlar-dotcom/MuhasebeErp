using Accounting.Core.Business.Interfaces.Repositories;
using Accounting.Core.DataAccess.Extensions;
using Accounting.Core.DataAccess.Persistence.Master;
using Accounting.Core.Domain.Entities;
using Accounting.Shared.Results;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Core.DataAccess.Repositories;

public sealed class PeriodRepository(MasterDbContext context) : RepositoryBase<Period , MasterDbContext>(context), IPeriodRepository
{
    #region Operations
    public async Task<Result<IReadOnlyList<Period>>> GetByCompanyIdAsync(
        Guid companyId ,
        CancellationToken cancellationToken = default)
    {
        List<Period> periods = await _set
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.Year)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<Period>>.Success(periods);
    }

    public async Task<Result<Period>> GetByCompanyAndYearAsync(
        Guid companyId ,
        int year ,
        CancellationToken cancellationToken = default)
    {
        return await _set
            .AsNoTracking()
            .FirstOrNotFoundAsync(
                x => x.CompanyId == companyId && x.Year == year ,
                $"Dönem bulunamadı. Firma: {companyId}, Yıl: {year}" ,
                cancellationToken);
    }

    public override async Task<Result<bool>> HasReferencesAsync(Guid id , CancellationToken cancellationToken = default)
    {
        // Yevmiye fişi ve diğer modüller geldiğinde buraya eklenecek.
        await Task.CompletedTask;
        return Result<bool>.Success(false);
    }
    #endregion Operations
}