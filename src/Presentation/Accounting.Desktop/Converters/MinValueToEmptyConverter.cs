using System.Globalization;
using System.Windows.Data;

namespace Accounting.Desktop.Converters;

public sealed class MinValueToEmptyConverter : IValueConverter
{
    #region Operations
    public object Convert(object value , Type targetType , object parameter , CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue)
            {
                return string.Empty;
            }

            return dateTime.ToString("dd.MM.yyyy" , culture);
        }

        return string.Empty;
    }

    public object ConvertBack(object value , Type targetType , object parameter , CultureInfo culture)
    {
        if (value is not string text || string.IsNullOrWhiteSpace(text))
        {
            return DateTime.MinValue;
        }

        if (DateTime.TryParseExact(text , "dd.MM.yyyy" , culture , DateTimeStyles.None , out DateTime parsed))
        {
            return parsed;
        }

        return DateTime.MinValue;
    }
    #endregion Operations
}