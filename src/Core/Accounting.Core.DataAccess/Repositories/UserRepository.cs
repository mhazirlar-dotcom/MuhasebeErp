using Accounting.Core.Business.Interfaces.Repositories;
using Accounting.Core.DataAccess.Extensions;
using Accounting.Core.DataAccess.Persistence.Master;
using Accounting.Core.Domain.Entities;
using Accounting.Shared.Results;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Core.DataAccess.Repositories;

public class UserRepository(MasterDbContext context) : RepositoryBase<User , MasterDbContext>(context), IUserRepository
{
    #region Operations
    public async Task<Result<User>> GetByUserNameAsync(string userName , CancellationToken cancellationToken = default)
    {
        return await _set
            .FirstOrNotFoundAsync(x => x.UserName == userName ,
                $"Kullanıcı bulunamadı. Kullanıcı adı: {userName}" , cancellationToken);
    }

    public async Task<Result<User>> GetByEmailAsync(string email , CancellationToken cancellationToken = default)
    {
        return await _set
            .AsNoTracking()
            .FirstOrNotFoundAsync(x => x.Email == email ,
                $"Kullanıcı bulunamadı. E-posta: {email}" , cancellationToken);
    }

    public override async Task<Result<bool>> HasReferencesAsync(Guid id , CancellationToken cancellationToken = default)
    {
        bool hasRoleReferences = await _context.Set<UserRole>()
            .AnyAsync(x => x.UserId == id , cancellationToken);

        if (hasRoleReferences)
        {
            return Result<bool>.Success(true);
        }

        bool hasCompanyReferences = await _context.Set<UserCompany>()
            .AnyAsync(x => x.UserId == id , cancellationToken);

        return Result<bool>.Success(hasCompanyReferences);
    }
    #endregion Operations
}