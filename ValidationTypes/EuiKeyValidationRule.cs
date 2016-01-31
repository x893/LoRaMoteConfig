using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace LoRaMoteConfig.ValidationTypes
{
	public class EuiKeyValidationRule : ValidationRule
	{
		private int _digits;

		public int Digits
		{
			get
			{
				return this._digits;
			}
			set
			{
				this._digits = value;
			}
		}

		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			string input;
			switch (this._digits)
			{
				case 8:
					input = Regex.Replace((value ?? (object)"00-00-00-00").ToString(), "[ :.-]", "");
					break;
				case 16:
					input = Regex.Replace((value ?? (object)"00-00-00-00-00-00-00-00").ToString(), "[ :.-]", "");
					break;
				case 32:
					input = Regex.Replace((value ?? (object)"00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00").ToString(), "[ :.-]", "");
					break;
				default:
					input = Regex.Replace((value ?? (object)"").ToString(), "[ :.-]", "");
					break;
			}
			Match match = new Regex("^[0-9a-f]+$", RegexOptions.IgnoreCase).Match(input);
			if (!match.Success)
				return new ValidationResult(false, (object)"The value is not valid.");
			if (match.Length == this._digits)
				return ValidationResult.ValidResult;
			return new ValidationResult(false, (object)("Value must have " + this._digits.ToString() + " HEX digits"));
		}
	}
}
