using System;

namespace LoRaMoteConfig.Events
{
	public class ErrorEventArgs : EventArgs
	{
		private byte _status;
		private string _message;

		public byte Status
		{
			get { return _status; }
		}

		public string Message
		{
			get { return _message; }
		}

		public ErrorEventArgs(byte status, string message)
		{
			_status = status;
			_message = message;
		}
	}
}
