using System.Windows.Media;

namespace LoRaMoteConfig.General
{
	public static class LightenColorExtension
	{
		public static Color Lighten(this Color originalColor, float lightFactor)
		{
			if (LightenColorExtension.TransformationNotNeeded(lightFactor))
				return originalColor;
			if (LightenColorExtension.RealBright(lightFactor))
				return Colors.White;
			if (LightenColorExtension.ShouldDarken(lightFactor))
				return LightenColorExtension.DarkenColor(originalColor, lightFactor);
			return LightenColorExtension.LightenColor(originalColor, lightFactor);
		}

		private static bool TransformationNotNeeded(float lightFactor)
		{
			float num = lightFactor - 1f;
			return (double)num < 0.00999999977648258 && (double)num > -0.00999999977648258;
		}

		private static bool RealBright(float lightFactor)
		{
			return (double)lightFactor >= 2.0;
		}

		private static bool ShouldDarken(float lightFactor)
		{
			return (double)lightFactor < 1.0;
		}

		private static Color DarkenColor(Color color, float lightFactor)
		{
			return Color.FromRgb((byte)((double)color.R * (double)lightFactor), (byte)((double)color.G * (double)lightFactor), (byte)((double)color.B * (double)lightFactor));
		}

		private static Color LightenColor(Color color, float lightFactor)
		{
			float fFactor = lightFactor;
			if ((double)fFactor > 1.0)
				--fFactor;
			return Color.FromRgb(LightenColorExtension.LightenColorComponent(color.R, fFactor), LightenColorExtension.LightenColorComponent(color.G, fFactor), LightenColorExtension.LightenColorComponent(color.B, fFactor));
		}

		private static byte LightenColorComponent(byte colorComponent, float fFactor)
		{
			int num = (int)byte.MaxValue - (int)colorComponent;
			colorComponent += (byte)((double)num * (double)fFactor);
			return (int)colorComponent < (int)byte.MaxValue ? colorComponent : byte.MaxValue;
		}
	}
}
