using Accounting.Desktop.ViewModels;
using System;
using System.Windows.Controls;

namespace Accounting.Desktop.Views;

public partial class CompanyEditView : UserControl
{
    #region Events
    public event EventHandler SaveCompleted = delegate { };
    public event EventHandler Cancelled = delegate { };
    #endregion Events

    #region Fields
    private readonly CompanyEditViewModel _viewModel;
    private readonly ShellViewModel _shellViewModel;
    private bool _periodListViewInitialized = false;
    #endregion Fields

    #region Constructor
    public CompanyEditView(CompanyEditViewModel viewModel , ShellViewModel shellViewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        _viewModel = viewModel;
        _shellViewModel = shellViewModel;

        viewModel.SaveCompleted += OnSaveCompleted;
        viewModel.Cancelled += OnCancelled;

        viewModel.PeriodList.AddRequested += async (_ , _) => await shellViewModel.OpenPeriodEditViewAsync(Guid.Empty);
        viewModel.PeriodList.EditRequested += async (_ , periodId) => await shellViewModel.OpenPeriodEditViewAsync(periodId);

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }
    #endregion Constructor

    #region Helpers
    private void OnLoaded(object sender , System.Windows.RoutedEventArgs e)
    {
        EnsurePeriodListViewInitialized();
        _shellViewModel.SetFooterContent(_viewModel);
    }

    private void OnUnloaded(object sender , System.Windows.RoutedEventArgs e)
    {
        _shellViewModel.ClearFooterContent();
    }

    private void EnsurePeriodListViewInitialized()
    {
        if (_periodListViewInitialized)
        {
            return;
        }

        PeriodListView periodListView = new(_viewModel.PeriodList);
        DonemContent.Content = periodListView;
        _periodListViewInitialized = true;
    }

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