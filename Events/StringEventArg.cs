using System;

namespace LoRaMoteConfig.Events
{
	public class StringEventArg : EventArgs
	{
		private string _value;

		public string Value
		{
			get { return _value; }
		}

		public StringEventArg(string value)
		{
			_value = value;
		}
	}
}