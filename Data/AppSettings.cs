using LoRaMoteConfig.Enums;
using System.ComponentModel;

namespace LoRaMoteConfig.Data
{
	public class AppSettings : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public Header Header = new Header();
		public byte[] DevEui = new byte[8]
	    {
			17,
			34,
			51,
			68,
			85,
			102,
			119,
			136
		};
		public bool OtaOn = true;
		public Activation Activation = new Activation();
		public Mac Mac = new Mac();
		public AppModes AppMode = AppModes.APP_GPS_TRACKING_DEMO;
		public Params Params = new Params();

		public void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}
