using Accounting.Core.Business.Interfaces.CrossCutting;
using Accounting.Core.Domain.Entities;
using DevExpress.Xpf.Editors;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Accounting.Desktop.Behaviors;

public static class MinFoundationDateBehavior
{
    #region Attached Property
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(MinFoundationDateBehavior),
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
        if (d is not DateEdit dateEdit)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            dateEdit.Validate += OnValidate;
        }
        else
        {
            dateEdit.Validate -= OnValidate;
        }
    }

    private static void OnValidate(object sender , ValidationEventArgs e)
    {
        if (sender is not DateEdit dateEdit)
        {
            return;
        }

        if (e.Value is not DateTime selectedDate)
        {
            return;
        }

        DateTime foundationDate = ResolveFoundationDate();

        if (foundationDate == DateTime.MinValue)
        {
            return;
        }

        if (selectedDate == DateTime.MinValue)
        {
            return;
        }

        if (selectedDate.Date >= foundationDate.Date)
        {
            return;
        }

        string fieldLabel = dateEdit.Tag as string ?? "Tarih";

        e.IsValid = false;
        e.ErrorContent = $"{fieldLabel} ({selectedDate:dd.MM.yyyy}) kuruluş tarihinden ({foundationDate:dd.MM.yyyy}) önce olamaz.";
    }

    private static DateTime ResolveFoundationDate()
    {
        if (Application.Current is null)
        {
            return DateTime.MinValue;
        }

        ITenantContext? tenantContext = App.Services.GetService<ITenantContext>();

        if (tenantContext is null || !tenantContext.HasCompany)
        {
            return DateTime.MinValue;
        }

        Company company = tenantContext.CurrentCompany;

        return company.FoundationDate;
    }
    #endregion Helpers
}