using Accounting.Core.Business.Interfaces;
using Accounting.Core.Business.Interfaces.CrossCutting;
using Accounting.Core.Business.Interfaces.Services;
using Accounting.Core.Domain.Entities;
using Accounting.Shared.Dtos.Auth;
using Accounting.Shared.Markers;
using Accounting.Shared.Results;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Accounting.Desktop.ViewModels;

public partial class LoginViewModel(IAuthService authService , ICurrentUser currentUser , IMessageService messageService) : ObservableObject, IScopedService
{
    #region Constants
    private const string LocalIpAddress = "127.0.0.1";
    #endregion Constants

    #region Events
    public event EventHandler LoginSucceeded = delegate { };
    public event EventHandler ExitRequested = delegate { };
    #endregion Events

    #region Properties
    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public bool IsNotBusy => !IsBusy;
    #endregion Properties

    #region Operations
    [RelayCommand]
    private async Task LoginAsync()
    {
        IsBusy = true;

        LoginRequest request = new(UserName , Password);

        Result<User> result = await authService.LoginAsync(request , LocalIpAddress);

        IsBusy = false;

        if (result.IsFailure)
        {
            messageService.ShowErrors("Giriş başarısız" , result.Errors);
            return;
        }

        currentUser.SetUser(result.Data.Id , result.Data.UserName , LocalIpAddress);

        LoginSucceeded.Invoke(this , EventArgs.Empty);
    }

    [RelayCommand]
    private void Exit()
    {
        ExitRequested.Invoke(this , EventArgs.Empty);
    }
    #endregion Operations

    #region Helpers
    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotBusy));
    }
    #endregion Helpers
}