using System.ComponentModel;

namespace LoRaMoteConfig.Data
{
	public class Activation : INotifyPropertyChanged
	{
		public ActivationOta Ota = new ActivationOta();
		public ActivationPa Pa = new ActivationPa();

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}
