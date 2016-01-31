using System.ComponentModel;

namespace LoRaMoteConfig.Data
{
	public class Header : INotifyPropertyChanged
	{
		public ushort Magic = (ushort)42330;
		public Header.Version Ver = new Header.Version();
		public Header.Version FwVer = new Header.Version();
		private const int APP_SETTINGS_MAGIC = 42330;
		public uint Crc;

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		public class Version
		{
			public byte Major { get; set; }

			public byte Minor { get; set; }

			public Version()
			{
				Major = (byte)0;
				Minor = (byte)0;
			}

			public override string ToString()
			{
				return (string)(object)Major + (object)"." + (string)(object)Minor;
			}
		}
	}
}
