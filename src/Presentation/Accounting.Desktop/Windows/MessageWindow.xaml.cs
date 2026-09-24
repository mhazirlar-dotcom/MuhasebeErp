using Accounting.Desktop.ViewModels;
using DevExpress.Xpf.Core;
using System;
using System.ComponentModel;
using System.Windows;

namespace Accounting.Desktop.Windows;

public partial class MessageWindow : ThemedWindow
{
    #region Constructor
    public MessageWindow(MessageViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.CloseRequested += OnCloseRequested;

        Closing += OnClosing;
    }
    #endregion Constructor

    #region Helpers
    private void OnCloseRequested(object sender , bool result)
    {
        DialogResult = result;
        Close();
    }

    private void OnClosing(object sender , CancelEventArgs e)
    {
        // X (kapat) butonuna basıldıysa ve dialog sonucu atanmadıysa:
        // Confirm tipinde false, diğer tiplerde true davran.
        if (DialogResult is null)
        {
            MessageViewModel viewModel = (MessageViewModel)DataContext;
            DialogResult = !viewModel.IsConfirm;
        }
    }
    #endregion Helpers
}