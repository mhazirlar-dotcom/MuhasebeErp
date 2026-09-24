using Accounting.Desktop.ViewModels;
using DevExpress.Xpf.Core;

namespace Accounting.Desktop.Windows;

public partial class LoginWindow : ThemedWindow
{
    #region Constructor
    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.LoginSucceeded += OnLoginSucceeded;
        viewModel.ExitRequested += OnExitRequested;
    }
    #endregion Constructor

    #region Helpers
    private void OnLoginSucceeded(object sender , EventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void OnExitRequested(object sender , EventArgs e)
    {
        DialogResult = false;
        Close();
    }
    #endregion Helpers
}