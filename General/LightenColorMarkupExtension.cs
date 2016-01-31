using System;
using System.Windows.Markup;
using System.Windows.Media;

namespace LoRaMoteConfig.General
{
	public class LightenColorMarkupExtension : MarkupExtension
	{
		public Color Color { get; set; }

		public float LightFactor { get; set; }

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return (object)LightenColorExtension.Lighten(this.Color, this.LightFactor);
		}
	}
}