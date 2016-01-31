using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace LoRaMoteConfig.ValidationTypes
{
	public class NumericValidationRule : ValidationRule
	{
		private int min = 0;
		private int max = (int)byte.MaxValue;
		private int _default = 0;

		public int Min
		{
			get
			{
				return this.min;
			}
			set
			{
				this.min = value;
			}
		}

		public int Max
		{
			get
			{
				return this.max;
			}
			set
			{
				this.max = value;
			}
		}

		public int Default
		{
			get
			{
				return this._default;
			}
			set
			{
				this._default = value;
			}
		}

		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			Regex regex = new Regex("^[0-9]+$", RegexOptions.IgnoreCase);
			string str = (value ?? (object)this._default.ToString()).ToString();
			if (!regex.Match(str).Success)
				return new ValidationResult(false, (object)"Value must be a number.");
			int result = this._default;
			if (!int.TryParse(str, out result))
				return new ValidationResult(false, (object)"Value must be a number.");
			if (result >= this.min && result <= this.max)
				return ValidationResult.ValidResult;
			return new ValidationResult(0 != 0, (object)("Value must be a number between " + this.min.ToString() + " and " + this.max.ToString() + "."));
		}
	}
}
