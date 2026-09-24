using Accounting.Core.Business.Interfaces.Repositories;
using Accounting.Core.Business.Interfaces.Services;
using Accounting.Core.Domain.Entities;
using Accounting.Shared.Results;
using FluentValidation;

namespace Accounting.Core.Business.Services;

public sealed class LookupValueService(ILookupValueRepository lookupValueRepository , IValidator<LookupValue> validator) : ServiceBase<LookupValue , ILookupValueRepository>(lookupValueRepository , validator), ILookupValueService
{
    #region Operations
    public async Task<Result<IReadOnlyList<LookupValue>>> GetByTypeAsync(string type , bool includeInactive = false , CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            return Result<IReadOnlyList<LookupValue>>.Failure("Lookup tipi zorunludur." , ResultStatus.ValidationError);
        }

        return await _repository.GetByTypeAsync(type , includeInactive , cancellationToken);
    }

    public async Task<Result<LookupValue>> GetByTypeAndCodeAsync(string type , string code , CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            return Result<LookupValue>.Failure("Lookup tipi zorunludur." , ResultStatus.ValidationError);
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            return Result<LookupValue>.Failure("Lookup kodu zorunludur." , ResultStatus.ValidationError);
        }

        return await _repository.GetByTypeAndCodeAsync(type , code , cancellationToken);
    }

    public async Task<Result<IReadOnlyList<LookupValue>>> GetChildrenAsync(Guid parentId , bool includeInactive = false , CancellationToken cancellationToken = default)
    {
        if (parentId == Guid.Empty)
        {
            return Result<IReadOnlyList<LookupValue>>.Failure("Parent Id zorunludur." , ResultStatus.ValidationError);
        }

        return await _repository.GetChildrenAsync(parentId , includeInactive , cancellationToken);
    }

    public override async Task<Result> UpdateAsync(LookupValue entity , CancellationToken cancellationToken = default)
    {
        // İŞ KURALI: Type ve Code immutable (İSTİSNA L)
        Result<LookupValue> existingResult = await _repository.GetByIdForUpdateAsync(entity.Id , cancellationToken);
        if (existingResult.IsFailure)
        {
            return Result.NotFound(existingResult.Message);
        }

        LookupValue existing = existingResult.Data;

        if (!string.Equals(existing.Type , entity.Type , StringComparison.Ordinal))
        {
            return Result.BusinessRuleViolation("Lookup tipi değiştirilemez.");
        }

        if (!string.Equals(existing.Code , entity.Code , StringComparison.Ordinal))
        {
            return Result.BusinessRuleViolation("Lookup kodu değiştirilemez.");
        }

        return await base.UpdateAsync(entity , cancellationToken);
    }
    #endregion Operations

    #region Helpers
    protected override void CopyFields(LookupValue target , LookupValue source)
    {
        // İSTİSNA L: Type ve Code IMMUTABLE — burada atlanır (UpdateAsync'te zaten bloke edildi).
        target.Name = source.Name;
        target.ParentId = source.ParentId;
        target.DisplayOrder = source.DisplayOrder;
        target.IsActive = source.IsActive;
    }
    #endregion Helpers
}