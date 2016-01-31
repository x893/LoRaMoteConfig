using System.ComponentModel;

namespace LoRaMoteConfig.Data
{
	public class ActivationPa : INotifyPropertyChanged
	{
		public uint DevAddr = 287454020U;
		public event PropertyChangedEventHandler PropertyChanged;

		public byte[] NwkSKey = new byte[16]
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

		public byte[] AppSKey = new byte[16]
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

		public void OnPropertyChanged(string name)
		{
			if (PropertyChanged == null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}
