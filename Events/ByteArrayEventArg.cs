using System;

namespace LoRaMoteConfig.Events
{
	public class ByteArrayEventArg : EventArgs
	{
		private byte[] _value;

		public byte[] Value
		{
			get { return _value; }
		}

		public ByteArrayEventArg(byte[] value)
		{
			_value = value;
		}
	}
}
