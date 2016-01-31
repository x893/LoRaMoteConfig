using System;
using System.Runtime.InteropServices;

namespace LoRaMoteConfig.Events
{
	[ComVisible(true)]
	[Serializable]
	public delegate void StringEventHandler(object sender, StringEventArg e);
}
