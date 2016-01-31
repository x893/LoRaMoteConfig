using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace LoRaMoteConfig.TypeConverters
{
	public class EnumTypeConverter : EnumConverter
	{
		private IEnumerable<EnumMapper> _mappings;

		public EnumTypeConverter(Type enumType)
			: base(enumType)
		{
			_mappings = Enumerable.Select<object, EnumMapper>(Enumerable.Cast<object>((IEnumerable)Enum.GetValues(enumType)), (Func<object, EnumMapper>)(enumValue => new EnumMapper(enumValue, GetDisplayName(enumValue))));
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string) && value != null && value.GetType().IsEnum)
				return (object)GetDisplayName(value);
			return base.ConvertTo(context, culture, value, destinationType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string)
			{
				EnumMapper enumMapper = Enumerable.FirstOrDefault<EnumMapper>(_mappings, (Func<EnumMapper, bool>)(mapping => string.Compare(mapping.Description, (string)value, true, culture) == 0));
				if (enumMapper != null)
					return enumMapper.Enum;
			}
			return base.ConvertFrom(context, culture, value);
		}

		private string GetDisplayName(object enumValue)
		{
			EnumDisplayNameAttribute displayNameAttribute = Enumerable.FirstOrDefault<object>((IEnumerable<object>)EnumType.GetField(enumValue.ToString()).GetCustomAttributes(typeof(EnumDisplayNameAttribute), false)) as EnumDisplayNameAttribute;
			if (displayNameAttribute != null)
				return displayNameAttribute.DisplayName;
			return Enum.GetName(EnumType, enumValue);
		}
	}
}
