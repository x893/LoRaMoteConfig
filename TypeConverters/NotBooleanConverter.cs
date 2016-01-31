using System;
using System.Globalization;
using System.Windows.Data;

namespace LoRaMoteConfig.TypeConverters
{
	internal class NotBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool)
				return (object)(!(bool)value ? true : false);
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool)
				return (object)(!(bool)value ? true : false);
			return value;
		}
	}
}
