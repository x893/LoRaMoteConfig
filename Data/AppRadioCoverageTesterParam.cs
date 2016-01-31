using System.ComponentModel;

namespace LoRaMoteConfig.Data
{
	public class AppRadioCoverageTesterParam : INotifyPropertyChanged
	{
		public byte Id = (byte)0;
		public byte NbPackets = (byte)1;
		public bool DownlinkTestOn = false;

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}
