using DevExpress.Xpf.Editors;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Threading;

namespace Accounting.Desktop.Behaviors;

public static class ProperCaseBehavior
{
    #region Attached Property
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(ProperCaseBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsEnabledProperty);
    }

    public static void SetIsEnabled(DependencyObject obj , bool value)
    {
        obj.SetValue(IsEnabledProperty , value);
    }
    #endregion Attached Property

    #region Helpers
    private static void OnIsEnabledChanged(DependencyObject d , DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextEdit textEdit)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            textEdit.EditValueChanged += OnEditValueChanged;
        }
        else
        {
            textEdit.EditValueChanged -= OnEditValueChanged;
        }
    }

    private static void OnEditValueChanged(object sender , EditValueChangedEventArgs e)
    {
        if (sender is not TextEdit textEdit)
        {
            return;
        }

        if (e.NewValue is not string newValue || string.IsNullOrEmpty(newValue))
        {
            return;
        }

        string properCased = ToProperCase(newValue);
        if (properCased == newValue)
        {
            return;
        }

        int caretPosition = textEdit.SelectionStart;

        textEdit.Dispatcher.BeginInvoke(new Action(() =>
        {
            textEdit.EditValue = properCased;
            textEdit.Select(caretPosition , 0);
        }) , DispatcherPriority.Background);
    }

    private static string ToProperCase(string input)
    {
        CultureInfo culture = new("tr-TR");
        string lower = input.ToLower(culture);
        return culture.TextInfo.ToTitleCase(lower);
    }
    #endregion Helpers
}