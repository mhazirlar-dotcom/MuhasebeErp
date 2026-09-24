using Accounting.Desktop.ViewModels;
using DevExpress.Xpf.Core;

namespace Accounting.Desktop.Windows;

public partial class AddPeriodDialog : ThemedWindow
{
    #region Constructor
    public AddPeriodDialog(AddPeriodDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.Confirmed += OnConfirmed;
        viewModel.Cancelled += OnCancelled;
    }
    #endregion Constructor

    #region Helpers
    private void OnConfirmed(object sender , EventArgs e)
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