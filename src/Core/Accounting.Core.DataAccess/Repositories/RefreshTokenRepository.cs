using Accounting.Core.Business.Interfaces.Repositories;
using Accounting.Core.DataAccess.Extensions;
using Accounting.Core.DataAccess.Persistence.Master;
using Accounting.Core.Domain.Entities;
using Accounting.Shared.Results;

namespace Accounting.Core.DataAccess.Repositories;

public sealed class RefreshTokenRepository(MasterDbContext context)
    : RepositoryBase<RefreshToken , MasterDbContext>(context), IRefreshTokenRepository
{
    #region Operations
    public async Task<Result<RefreshToken>> GetByTokenHashAsync(
        string tokenHash ,
        CancellationToken cancellationToken = default)
    {
        return await _set
            .FirstOrNotFoundAsync(
                x => x.TokenHash == tokenHash ,
                "Refresh token bulunamadı." ,
                cancellationToken);
    }

    public override async Task<Result<bool>> HasReferencesAsync(
        Guid id ,
        CancellationToken cancellationToken = default)
    {
        // Refresh token başka bir tablo tarafından referans edilmiyor.
        // İptal ve temizlik akışlarında fiziksel silinebilir.
        await Task.CompletedTask;
        return Result<bool>.Success(false);
    }
    #endregion Operations
}