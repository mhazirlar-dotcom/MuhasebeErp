using Accounting.Desktop.ViewModels;
using System.Windows.Controls;

namespace Accounting.Desktop.Views;

public partial class WelcomeView : UserControl
{
    #region Constructor
    public WelcomeView(WelcomeViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        Loaded += (_ , _) => viewModel.Load();
    }
    #endregion Constructor
}