using LoRaMoteConfig.Enums;
using System.ComponentModel;

namespace LoRaMoteConfig.Data
{
	public class Rx2ChannelParams : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private Datarates _datarate = Datarates.DR_3;
		private uint _frequency;

		public uint Frequency
		{
			get
			{
				return _frequency;
			}
			set
			{
				if ((int)value == (int)_frequency)
					return;
				_frequency = value;
				OnPropertyChanged("Frequency");
			}
		}

		public Datarates Datarate
		{
			get
			{
				return _datarate;
			}
			set
			{
				if (value == _datarate)
					return;
				_datarate = value;
				OnPropertyChanged("Datarate");
			}
		}

		public Rx2ChannelParams()
		{
			_frequency = 0U;
			_datarate = Datarates.DR_0;
		}

		public Rx2ChannelParams(uint frequency, Datarates datarate)
		{
			_frequency = frequency;
			_datarate = datarate;
		}

		public void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
			PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}
