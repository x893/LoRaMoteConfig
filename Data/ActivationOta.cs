using System.ComponentModel;

namespace LoRaMoteConfig.Data
{
	public class ActivationOta : INotifyPropertyChanged
	{
		public uint DutyCycle = 10000000U;
		public byte[] AppEui = new byte[8];
		public byte[] AppKey = new byte[16]
		{
			43,
			126,
			21,
			22,
			40,
			174,
			210,
			166,
			171,
			247,
			21,
			136,
			9,
			207,
			79,
			60
		};

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)

				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}
