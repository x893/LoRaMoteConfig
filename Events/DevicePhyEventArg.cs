using LoRaMoteConfig.Enums;
using System;

namespace LoRaMoteConfig.Events
{
	public class DevicePhyEventArg : EventArgs
	{
		private DevicePhys _phy;

		public DevicePhys Phy
		{
			get
			{
				return _phy;
			}
		}

		public DevicePhyEventArg(DevicePhys phy)
		{
			_phy = phy;
		}
	}
}