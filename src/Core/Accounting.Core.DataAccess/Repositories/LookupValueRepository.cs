using Accounting.Core.Business.Interfaces.Repositories;
using Accounting.Core.DataAccess.Extensions;
using Accounting.Core.DataAccess.Persistence.Master;
using Accounting.Core.Domain.Entities;
using Accounting.Shared.Results;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Core.DataAccess.Repositories;

public class LookupValueRepository(MasterDbContext context) : RepositoryBase<LookupValue , MasterDbContext>(context), ILookupValueRepository
{
    #region Operations
    public async Task<Result<IReadOnlyList<LookupValue>>> GetByTypeAsync(
        string type ,
        bool includeInactive = false ,
        CancellationToken cancellationToken = default)
    {
        IQueryable<LookupValue> query = _set
            .AsNoTracking()
            .Where(x => x.Type == type);

        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        List<LookupValue> list = await query
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<LookupValue>>.Success(list);
    }

    public async Task<Result<LookupValue>> GetByTypeAndCodeAsync(
        string type ,
        string code ,
        CancellationToken cancellationToken = default)
    {
        return await _set
            .AsNoTracking()
            .FirstOrNotFoundAsync(x => x.Type == type && x.Code == code ,
                $"Kayıt bulunamadı. Tür: {type}, Kod: {code}" , cancellationToken);
    }

    public async Task<Result<IReadOnlyList<LookupValue>>> GetChildrenAsync(
        Guid parentId ,
        bool includeInactive = false ,
        CancellationToken cancellationToken = default)
    {
        IQueryable<LookupValue> query = _set
            .AsNoTracking()
            .Where(x => x.ParentId == parentId);

        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        List<LookupValue> list = await query
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<LookupValue>>.Success(list);
    }

    public override async Task<Result<bool>> HasReferencesAsync(Guid id , CancellationToken cancellationToken = default)
    {
        // Şu an LookupValue'lara FK ile bağlı entity yok.
        // Referanslar Code string üzerinden yapılıyor.
        // İleride entity alanları Code + FK'ya geçerse buraya kontrol eklenir.
        await Task.CompletedTask;
        return Result<bool>.Success(false);
    }
    #endregion Operations
}