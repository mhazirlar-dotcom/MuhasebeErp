using Accounting.Core.Business.Interfaces.CrossCutting;
using Accounting.Core.Business.Interfaces.Services;
using Accounting.Core.Domain.Entities;
using Accounting.Shared.Dtos.Auth;
using Accounting.Shared.Markers;
using Accounting.Shared.Results;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Accounting.Desktop.ViewModels;

public partial class FirstSetupViewModel(IAuthService authService , IMessageService messageService) : ObservableObject, IScopedService
{
    #region Events
    public event EventHandler SetupCompleted = delegate { };
    public event EventHandler Cancelled = delegate { };
    #endregion Events

    #region Properties
    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public bool IsNotBusy => !IsBusy;
    #endregion Properties

    #region Operations
    [RelayCommand]
    private async Task SaveAsync()
    {
        // UI istisnası: şifre tekrarı form bütünlüğü kontrolü (API DTO'sunda yok)
        if (Password != ConfirmPassword)
        {
            messageService.ShowWarning("Şifreler eşleşmiyor.");
            return;
        }

        IsBusy = true;

        FirstSetupRequest request = new(UserName , FullName , Email , Password);

        Result<User> result = await authService.CreateFirstAdminAsync(request);

        IsBusy = false;

        if (result.IsFailure)
        {
            messageService.ShowErrors("İlk kurulum başarısız" , result.Errors);
            return;
        }

        messageService.ShowSuccess("Yönetici hesabı oluşturuldu.");
        SetupCompleted.Invoke(this , EventArgs.Empty);
    }

    [RelayCommand]
    private void Cancel()
    {
        Cancelled.Invoke(this , EventArgs.Empty);
    }
    #endregion Operations

    #region Helpers
    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotBusy));
    }
    #endregion Helpers
}