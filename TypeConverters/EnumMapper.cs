namespace LoRaMoteConfig.TypeConverters
{
	public class EnumMapper
	{
		public object Enum { get; private set; }

		public string Description { get; private set; }

		public EnumMapper(object enumValue, string enumDescription)
		{
			Enum = enumValue;
			Description = enumDescription;
		}
	}
}
