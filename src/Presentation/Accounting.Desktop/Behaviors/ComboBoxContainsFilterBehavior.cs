using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using DevExpress.Xpf.Editors;

namespace Accounting.Desktop.Behaviors;

public static class ComboBoxContainsFilterBehavior
{
    #region Attached Properties
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(ComboBoxContainsFilterBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    private static readonly DependencyProperty ViewProperty =
        DependencyProperty.RegisterAttached(
            "View",
            typeof(ICollectionView),
            typeof(ComboBoxContainsFilterBehavior),
            new PropertyMetadata(null));
    #endregion Attached Properties

    #region Accessors
    public static bool GetIsEnabled(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsEnabledProperty);
    }

    public static void SetIsEnabled(DependencyObject obj , bool value)
    {
        obj.SetValue(IsEnabledProperty , value);
    }
    #endregion Accessors

    #region Helpers
    private static void OnIsEnabledChanged(DependencyObject d , DependencyPropertyChangedEventArgs e)
    {
        if (d is not ComboBoxEdit comboBox)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            comboBox.Loaded += OnLoaded;
            comboBox.PreviewKeyDown += OnPreviewKeyDown;
        }
        else
        {
            comboBox.Loaded -= OnLoaded;
            comboBox.PreviewKeyDown -= OnPreviewKeyDown;
        }
    }

    private static void OnLoaded(object sender , RoutedEventArgs e)
    {
        if (sender is not ComboBoxEdit comboBox)
        {
            return;
        }

        if (comboBox.ItemsSource is null)
        {
            return;
        }

        ICollectionView view = CollectionViewSource.GetDefaultView(comboBox.ItemsSource);
        comboBox.SetValue(ViewProperty , view);
    }

    private static void OnPreviewKeyDown(object sender , KeyEventArgs e)
    {
        if (sender is not ComboBoxEdit comboBox)
        {
            return;
        }

        comboBox.Dispatcher.BeginInvoke(
            new Action(() => ApplyFilter(comboBox)) ,
            DispatcherPriority.Background);
    }

    private static void ApplyFilter(ComboBoxEdit comboBox)
    {
        if (comboBox.GetValue(ViewProperty) is not ICollectionView view)
        {
            return;
        }

        if (!comboBox.IsPopupOpen)
        {
            return;
        }

        string searchText = comboBox.Text;

        if (string.IsNullOrEmpty(searchText))
        {
            if (view.Filter is not null)
            {
                view.Filter = null;
            }
            return;
        }

        string displayMember = comboBox.DisplayMember;
        view.Filter = item => Matches(item , displayMember , searchText);
    }

    private static bool Matches(object item , string displayMember , string searchText)
    {
        if (item is null)
        {
            return false;
        }

        string itemText = GetItemText(item , displayMember);

        return itemText.Contains(searchText , StringComparison.CurrentCultureIgnoreCase);
    }

    private static string GetItemText(object item , string displayMember)
    {
        if (string.IsNullOrEmpty(displayMember))
        {
            return item.ToString() ?? string.Empty;
        }

        PropertyInfo? property = item.GetType().GetProperty(displayMember);

        if (property is null)
        {
            return item.ToString() ?? string.Empty;
        }

        return property.GetValue(item)?.ToString() ?? string.Empty;
    }
    #endregion Helpers
}