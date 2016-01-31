using System.ComponentModel;

namespace LoRaMoteConfig.Data
{
	public class AppSensorsGpsDemoParam : INotifyPropertyChanged
	{
		public uint TxDutyCycle = 5000000U;
		public uint TxDutyCycleRnd = 1000000U;
		public bool AdrOn = false;
		public bool DownlinkTestOn = false;

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}
