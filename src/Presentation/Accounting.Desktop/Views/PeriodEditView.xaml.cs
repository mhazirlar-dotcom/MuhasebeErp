using Accounting.Desktop.ViewModels;
using System.Windows.Controls;

namespace Accounting.Desktop.Views;

public partial class PeriodEditView : UserControl
{
    #region Events
    public event EventHandler SaveCompleted = delegate { };
    public event EventHandler Cancelled = delegate { };
    #endregion Events

    #region Constructor
    public PeriodEditView(PeriodEditViewModel viewModel , ShellViewModel shellViewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.SaveCompleted += OnSaveCompleted;
        viewModel.Cancelled += OnCancelled;

        Loaded += (_ , _) => shellViewModel.SetFooterContent(viewModel);
        Unloaded += (_ , _) => shellViewModel.ClearFooterContent();
    }
    #endregion Constructor

    #region Helpers
    private void OnSaveCompleted(object sender , EventArgs e)
    {
        SaveCompleted.Invoke(this , EventArgs.Empty);
    }

    private void OnCancelled(object sender , EventArgs e)
    {
        Cancelled.Invoke(this , EventArgs.Empty);
    }
    #endregion Helpers
}