using Accounting.Desktop.Enums;
using Accounting.Shared.Results;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace Accounting.Desktop.ViewModels;

public partial class MessageViewModel : ObservableObject
{
    #region Events
    public event EventHandler<bool> CloseRequested = delegate { };
    #endregion Events

    #region Constructor
    public MessageViewModel(MessageType messageType , string title , string message , bool isConfirm)
    {
        MessageType = messageType;
        Title = title;
        Message = message;
        IsConfirm = isConfirm;

        if (isConfirm)
        {
            PrimaryButtonText = "Evet";
            SecondaryButtonText = "Hayır";
            HasSecondaryButton = true;
        }
        else
        {
            PrimaryButtonText = "Tamam";
            SecondaryButtonText = string.Empty;
            HasSecondaryButton = false;
        }
    }
    #endregion Constructor

    #region Properties
    public MessageType MessageType { get; }
    public string Title { get; }
    public string Message { get; }
    public bool IsConfirm { get; }
    public string PrimaryButtonText { get; }
    public string SecondaryButtonText { get; }
    public bool HasSecondaryButton { get; }

    [ObservableProperty]
    private  bool _hasErrors;

    public ObservableCollection<Error> Errors { get; } = [];
    #endregion Properties

    #region Operations
    public void SetErrors(IReadOnlyList<Error> errors)
    {
        Errors.Clear();

        foreach (Error error in errors)
        {
            Errors.Add(error);
        }

        HasErrors = Errors.Count > 0;
    }
    #endregion Operations

    #region Commands
    [RelayCommand]
    private void Primary()
    {
        CloseRequested.Invoke(this , true);
    }

    [RelayCommand]
    private void Secondary()
    {
        CloseRequested.Invoke(this , false);
    }

    [RelayCommand]
    private void Copy()
    {
        StringBuilder builder = new();

        builder.AppendLine(Title);

        if (!string.IsNullOrWhiteSpace(Message))
        {
            builder.AppendLine();
            builder.AppendLine(Message);
        }

        if (Errors.Count > 0)
        {
            builder.AppendLine();
            foreach (Error error in Errors)
            {
                builder.AppendLine($"• {error.Message}");
            }
        }

        Clipboard.SetText(builder.ToString());
    }
    #endregion Commands
}