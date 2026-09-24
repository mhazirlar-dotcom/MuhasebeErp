using Accounting.Desktop.ViewModels;
using DevExpress.Xpf.Core;
using System;
using System.Windows;

namespace Accounting.Desktop.Windows;

public partial class FirstSetupWindow : ThemedWindow
{
    #region Constructor
    public FirstSetupWindow(FirstSetupViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.SetupCompleted += OnSetupCompleted;
        viewModel.Cancelled += OnCancelled;
    }
    #endregion Constructor

    #region Helpers
    private void OnSetupCompleted(object sender , EventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void OnCancelled(object sender , EventArgs e)
    {
        DialogResult = false;
        Close();
    }
    #endregion Helpers
}