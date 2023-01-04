using System;
using System.Globalization;
using System.Windows.Data;

namespace ConnectClient.Gui.Converter
{
    public class StringBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null)
            {
                return false;
            }

            return value switch
            {
                "True" => true,
                "False" => false,
                _ => false
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
