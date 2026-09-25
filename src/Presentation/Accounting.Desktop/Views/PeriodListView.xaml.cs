using Accounting.Desktop.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Accounting.Desktop.Views;

public partial class PeriodListView : UserControl
{
    #region Constructor
    public PeriodListView(PeriodListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
    #endregion Constructor

    #region Helpers
    private void OnGridMouseDoubleClick(object sender , MouseButtonEventArgs e)
    {
        if (DataContext is not PeriodListViewModel viewModel)
        {
            return;
        }

        if (!viewModel.HasSelection)
        {
            return;
        }

        viewModel.EditCommand.Execute(null);
    }
    #endregion Helpers
}