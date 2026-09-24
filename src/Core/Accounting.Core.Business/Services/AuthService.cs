using Accounting.Core.Business.Extensions;
using Accounting.Core.Business.Interfaces;
using Accounting.Core.Business.Interfaces.Repositories;
using Accounting.Core.Business.Interfaces.Services;
using Accounting.Core.Domain.Entities;
using Accounting.Shared.Dtos.Auth;
using Accounting.Shared.Markers;
using Accounting.Shared.Results;
using FluentValidation;

namespace Accounting.Core.Business.Services;

public sealed class AuthService(IUserRepository userRepository , IPasswordHasher passwordHasher , IClock clock , IValidator<LoginRequest> loginValidator , IValidator<FirstSetupRequest> firstSetupValidator) : IAuthService, IScopedService
{
    #region Operations
    public async Task<Result<bool>> HasAnyUserAsync(CancellationToken cancellationToken = default)
    {
        return await userRepository.HasAnyAsync(cancellationToken);
    }

    public async Task<Result<User>> LoginAsync(LoginRequest request , string ipAddress , CancellationToken cancellationToken = default)
    {
        return await request
            .ValidateAsync(loginValidator , cancellationToken)
            .ThenAsync(_ => userRepository.GetByUserNameAsync(request.UserName , cancellationToken))
            .MapFailureAsync("Kullanıcı adı veya şifre hatalı." , ResultStatus.Unauthorized)
            .EnsureAsync(user => user.IsActive , "Kullanıcı hesabı aktif değil." , ResultStatus.Forbidden)
            .EnsureAsync(user => passwordHasher.Verify(request.Password , user.PasswordHash) , "Kullanıcı adı veya şifre hatalı." , ResultStatus.Unauthorized)
            .ThenAsync(user => UpdateLastLoginAsync(user , cancellationToken));
    }

    public async Task<Result<User>> CreateFirstAdminAsync(FirstSetupRequest request , CancellationToken cancellationToken = default)
    {
        return await request
            .ValidateAsync(firstSetupValidator , cancellationToken)
            .ThenAsync(_ => userRepository.HasAnyAsync(cancellationToken))
            .EnsureFalseAsync("Sistemde zaten kullanıcı mevcut. İlk kurulum tekrar çalıştırılamaz." , ResultStatus.Conflict)
            .ThenAsync(() => EnsureEmailNotUsedAsync(request.Email , cancellationToken))
            .ThenAsync(() => CreateAdminAsync(request , cancellationToken));
    }
    #endregion Operations

    #region Helpers
    private async Task<Result<User>> UpdateLastLoginAsync(User user , CancellationToken cancellationToken)
    {
        user.LastLoginAt = clock.UtcNow;
        await userRepository.SaveChangesAsync(cancellationToken);

        return Result<User>.Success(user , "Giriş başarılı.");
    }

    private async Task<Result> EnsureEmailNotUsedAsync(string email , CancellationToken cancellationToken)
    {
        Result<User> emailCheckResult = await userRepository.GetByEmailAsync(email , cancellationToken);

        if (emailCheckResult.IsSuccess)
        {
            return Result.Conflict("Bu e-posta adresi zaten kullanılıyor.");
        }

        return Result.Success();
    }

    private async Task<Result<User>> CreateAdminAsync(FirstSetupRequest request , CancellationToken cancellationToken)
    {
        User user = new()
        {
            UserName = request.UserName,
            FullName = request.FullName,
            Email = request.Email,
            Phone = string.Empty,
            PasswordHash = passwordHasher.Hash(request.Password),
            IsActive = true,
            LastLoginAt = clock.UtcNow
        };

        return await userRepository.CreateAsync(user , cancellationToken);
    }
    #endregion Helpers
}