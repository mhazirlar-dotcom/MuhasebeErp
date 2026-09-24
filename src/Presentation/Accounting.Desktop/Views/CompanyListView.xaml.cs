using Accounting.Desktop.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Accounting.Desktop.Views;

public partial class CompanyListView : UserControl
{
    #region Constructor
    public CompanyListView(CompanyListViewModel viewModel , ShellViewModel shellViewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.NewCompanyRequested += async (_ , _) => await shellViewModel.OpenCompanyEditViewAsync(Guid.Empty);
        viewModel.EditCompanyRequested += async (_ , id) => await shellViewModel.OpenCompanyEditViewAsync(id);
        viewModel.CompaniesChanged += async (_ , _) => await shellViewModel.RefreshCompaniesAsync();

        Loaded += (_ , _) => shellViewModel.SetFooterContent(viewModel);
        Unloaded += (_ , _) => shellViewModel.ClearFooterContent();
    }
    #endregion Constructor

    #region Helpers
    private void OnGridMouseDoubleClick(object sender , MouseButtonEventArgs e)
    {
        if (DataContext is not CompanyListViewModel viewModel)
        {
            return;
        }

        if (!viewModel.HasSelection)
        {
            return;
        }

        viewModel.OpenDetailsCommand.Execute(null);
    }
    #endregion Helpers
}