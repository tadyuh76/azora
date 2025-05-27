using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace AvaloniaAzora.ViewModels.Student
{
    public class TestTypeToColorConverter : IValueConverter
    {
        public static readonly TestTypeToColorConverter Instance = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string testType)
            {
                return testType.ToLower() switch
                {
                    "quiz" => new SolidColorBrush(Color.Parse("#3188CA")), // Blue for quiz
                    "exam" => new SolidColorBrush(Color.Parse("#9C27B0")), // Purple for exam
                    _ => new SolidColorBrush(Color.Parse("#6B7280")) // Default gray
                };
            }

            return new SolidColorBrush(Color.Parse("#6B7280"));
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}