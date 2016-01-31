using System;

namespace LoRaMoteConfig.TypeConverters
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class EnumDisplayNameAttribute : Attribute
	{
		public string DisplayName { get; set; }

		public EnumDisplayNameAttribute(string displayName)
		{
			DisplayName = displayName;
		}
	}
}
