using System;
using System.Runtime.InteropServices;

namespace LoRaMoteConfig.Events
{
	[ComVisible(true)]
	[Serializable]
	public delegate void DevicePhyEventHandler(object sender, DevicePhyEventArg e);
}
