using LoRaMoteConfig.Enums;
using System.ComponentModel;

namespace LoRaMoteConfig.Data
{
	public class Mac : INotifyPropertyChanged
	{
		private DevicePhys devicePhy = DevicePhys.EU868;
		public const int LORA_MAX_NB_CHANNELS = 16;
		public ChannelParams[] Channels;
		public Rx2ChannelParams Rx2Channel;
		public Powers ChannelsTxPower;
		public Datarates ChannelsDatarate;
		public ushort[] ChannelsMask;
		public byte ChannelsNbTrans;
		public uint MaxRxWindow;
		public uint ReceiveDelay1;
		public uint ReceiveDelay2;
		public uint JoinAcceptDelay1;
		public uint JoinAcceptDelay2;
		public bool DutyCycleOn;
		public bool PublicNetwork;
		public DeviceClasses DeviceClass;

		public DevicePhys DevicePhy
		{
			get
			{
				return devicePhy;
			}
			set
			{
				if (devicePhy == value)
					return;
				devicePhy = value;
				switch (devicePhy)
				{
					case DevicePhys.US915:
					case DevicePhys.US915H:
						Channels = new ChannelParams[72];
						break;
					default:
						Channels = new ChannelParams[16];
						break;
				}
				for (int id = 0; id < Channels.Length; ++id)
					Channels[id] = new ChannelParams(id, 0U, Datarates.DR_0, Datarates.DR_0, Bands.BAND_G1_0);
				OnPropertyChanged("DevicePhy");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public Mac()
		{
			Channels = new ChannelParams[16];
			for (int id = 0; id < Channels.Length; ++id)
				Channels[id] = new ChannelParams(id, 0U, Datarates.DR_0, Datarates.DR_0, Bands.BAND_G1_0);
			Rx2Channel = new Rx2ChannelParams(0U, Datarates.DR_0);
			ChannelsMask = new ushort[6];
			Channels[0].Frequency = 868100000U;
			Channels[0].DatarateRange.Value = (sbyte)80;
			Channels[0].Band = Bands.BAND_G1_1;
			Channels[1].Frequency = 868300000U;
			Channels[1].DatarateRange.Value = (sbyte)96;
			Channels[1].Band = Bands.BAND_G1_1;
			Channels[2].Frequency = 868500000U;
			Channels[2].DatarateRange.Value = (sbyte)80;
			Channels[2].Band = Bands.BAND_G1_1;
			Channels[3].Frequency = 867100000U;
			Channels[3].DatarateRange.Value = (sbyte)80;
			Channels[3].Band = Bands.BAND_G1_0;
			Channels[4].Frequency = 867300000U;
			Channels[4].DatarateRange.Value = (sbyte)80;
			Channels[4].Band = Bands.BAND_G1_0;
			Channels[5].Frequency = 867500000U;
			Channels[5].DatarateRange.Value = (sbyte)80;
			Channels[5].Band = Bands.BAND_G1_0;
			Channels[6].Frequency = 867700000U;
			Channels[6].DatarateRange.Value = (sbyte)80;
			Channels[6].Band = Bands.BAND_G1_0;
			Channels[7].Frequency = 867900000U;
			Channels[7].DatarateRange.Value = (sbyte)80;
			Channels[7].Band = Bands.BAND_G1_0;
			Channels[8].Frequency = 868800000U;
			Channels[8].DatarateRange.Value = (sbyte)119;
			Channels[8].Band = Bands.BAND_G1_2;
			ChannelsMask[0] = (ushort)7;
			ChannelsMask[1] = (ushort)0;
			ChannelsMask[2] = (ushort)0;
			ChannelsMask[3] = (ushort)0;
			ChannelsMask[4] = (ushort)0;
			ChannelsMask[5] = (ushort)0;
			ChannelsTxPower = Powers.TX_POWER_14_DBM;
			ChannelsDatarate = Datarates.DR_0;
			ChannelsNbTrans = (byte)1;
			MaxRxWindow = 3000000U;
			ReceiveDelay1 = 1000000U;
			ReceiveDelay2 = 2000000U;
			JoinAcceptDelay1 = 5000000U;
			JoinAcceptDelay2 = 6000000U;
			DutyCycleOn = true;
			PublicNetwork = true;
			DeviceClass = DeviceClasses.CLASS_A;
			DevicePhy = DevicePhys.EU868;
		}

		public void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}
