using System.Globalization;

namespace LoRaMoteConfig.General
{
	public class Utilities
	{
		public static string Ba2Str(byte[] data)
		{
			return Utilities.Ba2Str(data, 0, data.Length);
		}

		public static string Ba2Str(byte[] data, int off, int len)
		{
			string str = "";
			for (int index = off; index < off + len; ++index)
				str += data[index].ToString("X02");
			return str;
		}

		public static byte[] HexStr2Ba(string s)
		{
			int length = s.Length;
			byte[] numArray = new byte[length / 2];
			int startIndex = 0;
			while (startIndex < length)
			{
				numArray[startIndex / 2] = byte.Parse(s.Substring(startIndex, 2), NumberStyles.HexNumber);
				startIndex += 2;
			}
			return numArray;
		}

		public static string BaEui2Str(byte[] eui)
		{
			string str = "";
			int index;
			for (index = eui.Length - 1; index > 0; --index)
				str = str + eui[index].ToString("X02") + "-";
			return str + eui[index].ToString("X02");
		}
	}
}
