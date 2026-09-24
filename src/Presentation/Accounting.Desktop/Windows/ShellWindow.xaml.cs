using Accounting.Desktop.ViewModels;
using DevExpress.Xpf.Core;

namespace Accounting.Desktop.Windows;

public partial class ShellWindow : ThemedWindow
{
    #region Constructor
    public ShellWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.LogoutRequested += OnLogoutRequested;

        Loaded += async (_ , _) => await viewModel.LoadAsync();
        Closed += (_ , _) => viewModel.Dispose();
    }
    #endregion Constructor

    #region Helpers
    private void OnLogoutRequested(object sender , EventArgs e)
    {
        DialogResult = true;
        Close();
    }
    #endregion Helpers
}