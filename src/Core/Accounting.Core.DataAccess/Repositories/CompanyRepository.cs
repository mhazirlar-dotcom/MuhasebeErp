using Accounting.Core.Business.Interfaces.Repositories;
using Accounting.Core.DataAccess.Persistence.Master;
using Accounting.Core.Domain.Entities;
using Accounting.Shared.Results;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Core.DataAccess.Repositories;

public sealed class CompanyRepository(MasterDbContext context) : RepositoryBase<Company , MasterDbContext>(context), ICompanyRepository
{
    #region Operations
    public async Task<Result<IReadOnlyList<Company>>> GetByUserIdAsync(
        Guid userId ,
        bool includeActive = true ,
        bool includePassive = false ,
        CancellationToken cancellationToken = default)
    {
        IQueryable<UserCompany> query = _context.Set<UserCompany>()
            .AsNoTracking()
            .Where(uc => uc.UserId == userId);

        if (includeActive && !includePassive)
        {
            query = query.Where(uc => uc.Company.IsActive);
        }
        else if (!includeActive && includePassive)
        {
            query = query.Where(uc => !uc.Company.IsActive);
        }
        else if (!includeActive && !includePassive)
        {
            return Result<IReadOnlyList<Company>>.Success([]);
        }

        List<Company> companies = await query
            .OrderByDescending(uc => uc.IsDefault)
            .ThenBy(uc => uc.Company.ShortName)
            .Select(uc => uc.Company)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<Company>>.Success(companies);
    }

    public async Task<Result<Company>> AddWithUserAsync(
        Company company ,
        Guid userId ,
        bool isDefault ,
        CancellationToken cancellationToken = default)
    {
        await _context.Set<Company>().AddAsync(company , cancellationToken);

        UserCompany userCompany = new()
        {
            UserId = userId,
            CompanyId = company.Id,
            IsDefault = isDefault
        };

        await _context.Set<UserCompany>().AddAsync(userCompany , cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Company>.Success(company , "Firma eklendi.");
    }

    public override async Task<Result<bool>> HasReferencesAsync(Guid id , CancellationToken cancellationToken = default)
    {
        bool hasUserReferences = await _context.Set<UserCompany>()
            .AnyAsync(x => x.CompanyId == id , cancellationToken);

        return Result<bool>.Success(hasUserReferences);
    }
    #endregion Operations
}