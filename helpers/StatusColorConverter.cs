using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AvaloniaAzora.Helpers
{
    public class StatusTextConverter : IValueConverter
    {
        public static readonly StatusTextConverter Instance = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string role)
            {
                return role.StartsWith("DEACTIVATED_") ? "Inactive" : "Active";
            }
            return "Unknown";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
