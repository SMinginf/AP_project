using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TestGeneratorClient.Utils
{
    public class PubblicaToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool b && b) ? "Pubblica" : "Privata";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}