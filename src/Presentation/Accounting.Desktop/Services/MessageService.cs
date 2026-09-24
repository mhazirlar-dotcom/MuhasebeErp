using Accounting.Core.Business.Interfaces.CrossCutting;
using Accounting.Desktop.Enums;
using Accounting.Desktop.ViewModels;
using Accounting.Desktop.Windows;
using Accounting.Shared.Markers;
using Accounting.Shared.Results;
using System.Runtime.Versioning;
using System.Windows;

namespace Accounting.Desktop.Services;

[SupportedOSPlatform("windows")]
public sealed class MessageService : IMessageService, ISingletonService
{
    #region Operations
    public void ShowSuccess(string message)
    {
        ShowSuccess("Başarılı" , message);
    }

    public void ShowSuccess(string title , string message)
    {
        ShowMessage(MessageType.Success , title , message , null , isConfirm: false);
    }

    public void ShowInfo(string message)
    {
        ShowInfo("Bilgi" , message);
    }

    public void ShowInfo(string title , string message)
    {
        ShowMessage(MessageType.Info , title , message , null , isConfirm: false);
    }

    public void ShowWarning(string message)
    {
        ShowWarning("Uyarı" , message);
    }

    public void ShowWarning(string title , string message)
    {
        ShowMessage(MessageType.Warning , title , message , null , isConfirm: false);
    }

    public void ShowError(string message)
    {
        ShowError("Hata" , message);
    }

    public void ShowError(string title , string message)
    {
        ShowMessage(MessageType.Error , title , message , null , isConfirm: false);
    }

    public void ShowErrors(string title , IReadOnlyList<Error> errors)
    {
        ShowMessage(MessageType.Error , title , string.Empty , errors , isConfirm: false);
    }

    public bool Confirm(string title , string message)
    {
        bool? result = ShowMessage(MessageType.Confirm , title , message , null , isConfirm: true);
        return result == true;
    }
    #endregion Operations

    #region Helpers
    private static bool? ShowMessage(MessageType messageType , string title , string message , IReadOnlyList<Error>? errors , bool isConfirm)
    {
        MessageViewModel viewModel = new(messageType , title , message , isConfirm);

        if (errors is not null && errors.Count > 0)
        {
            viewModel.SetErrors(errors);
        }

        MessageWindow window = new(viewModel);

        Window? owner = ResolveOwner();
        if (owner is not null && !ReferenceEquals(owner , window))
        {
            window.Owner = owner;
        }

        return window.ShowDialog();
    }

    private static Window? ResolveOwner()
    {
        if (Application.Current is null)
        {
            return null;
        }

        foreach (Window window in Application.Current.Windows)
        {
            if (window.IsActive)
            {
                return window;
            }
        }

        return Application.Current.MainWindow;
    }
    #endregion Helpers
}