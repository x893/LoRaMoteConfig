using LoRaMoteConfig.TypeConverters;
using System.ComponentModel;

namespace LoRaMoteConfig.Enums
{
	[TypeConverter(typeof(EnumTypeConverter))]
	public enum AppModes
	{
		[EnumDisplayName("Sensors GPS demo")]
		APP_SENSORS_GPS_DEMO,
		[EnumDisplayName("GPS tracking demo")]
		APP_GPS_TRACKING_DEMO,
		[EnumDisplayName("Radio coverage tester")]
		APP_RADIO_COVERAGE_TESTER,
	}

	[TypeConverter(typeof(EnumTypeConverter))]
	public enum Bands
	{
		[EnumDisplayName("865-868 MHz DC-1%")]
		BAND_G1_0,
		[EnumDisplayName("868-868.6 MHz DC-1%")]
		BAND_G1_1,
		[EnumDisplayName("868.7-869.2 MHz DC-0.1%")]
		BAND_G1_2,
		[EnumDisplayName("869.4-869.65 MHz DC-10%")]
		BAND_G1_3,
		[EnumDisplayName("869.7-870 MHz DC-1%")]
		BAND_G1_4,
	}

	[TypeConverter(typeof(EnumTypeConverter))]
	public enum ChannelDutyCycles
	{
		[EnumDisplayName("100.000")]
		DC_100_000,
		[EnumDisplayName("50.000")]
		DC_50_000,
		[EnumDisplayName("25.000")]
		DC_25_000,
		[EnumDisplayName("12.500")]
		DC_12_500,
		[EnumDisplayName("6.250")]
		DC_6_250,
		[EnumDisplayName("3.125")]
		DC_3_125,
		[EnumDisplayName("1.563")]
		DC_1_563,
		[EnumDisplayName("0.781")]
		DC_0_781,
		[EnumDisplayName("0.391")]
		DC_0_391,
		[EnumDisplayName("0.195")]
		DC_0_195,
		[EnumDisplayName("0.098")]
		DC_0_098,
		[EnumDisplayName("0.049")]
		DC_0_049,
		[EnumDisplayName("0.024")]
		DC_0_024,
		[EnumDisplayName("0.012")]
		DC_0_012,
		[EnumDisplayName("0.006")]
		DC_0_006,
		[EnumDisplayName("0.003")]
		DC_0_003,
	}

	[TypeConverter(typeof(EnumTypeConverter))]
	public enum Datarates
	{
		[EnumDisplayName("DR0")]
		DR_0,
		[EnumDisplayName("DR1")]
		DR_1,
		[EnumDisplayName("DR2")]
		DR_2,
		[EnumDisplayName("DR3")]
		DR_3,
		[EnumDisplayName("DR4")]
		DR_4,
		[EnumDisplayName("DR5")]
		DR_5,
		[EnumDisplayName("DR6")]
		DR_6,
		[EnumDisplayName("DR7")]
		DR_7,
	}

	[TypeConverter(typeof(EnumTypeConverter))]
	public enum DeviceClasses
	{
		[EnumDisplayName("Class A")]
		CLASS_A,
		[EnumDisplayName("Class B")]
		CLASS_B,
		[EnumDisplayName("Class C")]
		CLASS_C,
	}

	[TypeConverter(typeof(EnumTypeConverter))]
	public enum DevicePhys
	{
		[EnumDisplayName("EU 868 MHz")]
		EU868 = 0,
		[EnumDisplayName("US 915 MHz")]
		US915 = 1,
		[EnumDisplayName("EU 433 MHz")]
		EU433 = 2,
		[EnumDisplayName("CN 780 MHz")]
		CN780 = 3,
		[EnumDisplayName("US 915 MHz Hybrid")]
		US915H = 128,
		[EnumDisplayName("Unknown")]
		UNKNOWN = 255,
	}

	[TypeConverter(typeof(EnumTypeConverter))]
	public enum Frames
	{
		[EnumDisplayName("Status")]
		CMD_STATUS,
		[EnumDisplayName("Write settings")]
		CMD_WR_SETTINGS,
		[EnumDisplayName("Read settings")]
		CMD_RD_SETTINGS,
		[EnumDisplayName("Log")]
		CMD_LOG,
		[EnumDisplayName("Reset")]
		CMD_RESET,
		[EnumDisplayName("Set EEPROM default")]
		CMD_SET_EEPROM_DEFAULT,
		[EnumDisplayName("Get EEPROM version")]
		CMD_GET_EEPROM_VERSION,
		[EnumDisplayName("Get firmware version")]
		CMD_GET_FIRMWARE_VERSION,
	}

	[TypeConverter(typeof(EnumTypeConverter))]
	public enum Powers
	{
		[EnumDisplayName("20")]
		TX_POWER_20_DBM,
		[EnumDisplayName("14")]
		TX_POWER_14_DBM,
		[EnumDisplayName("11")]
		TX_POWER_11_DBM,
		[EnumDisplayName("8")]
		TX_POWER_08_DBM,
		[EnumDisplayName("5")]
		TX_POWER_05_DBM,
		[EnumDisplayName("2")]
		TX_POWER_02_DBM,
	}
}
