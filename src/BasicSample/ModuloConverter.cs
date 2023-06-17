namespace BasicSample
{
    using System;
    using System.Globalization;
    using System.Windows.Data;using System.Windows.Markup;

    public class ModuloConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int index)
                return null;

            return $"{index % 10}: {value}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
