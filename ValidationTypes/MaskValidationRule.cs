using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace LoRaMoteConfig.ValidationTypes
{
	public class MaskValidationRule : ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			Match match = new Regex("^[0-9a-f]+$", RegexOptions.IgnoreCase).Match((value ?? (object)"0000").ToString());
			if (!match.Success)
				return new ValidationResult(false, (object)"The value is not valid.");
			if (match.Length == 4)
				return ValidationResult.ValidResult;
			return new ValidationResult(false, (object)"Value must have 4 HEX digits");
		}
	}
}
