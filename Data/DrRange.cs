using LoRaMoteConfig.Enums;
using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace LoRaMoteConfig.Data
{
	public class DrRange : INotifyPropertyChanged
	{
		private Datarates _min;
		private Datarates _max;

		public Datarates Min
		{
			get			{				return _min;			}
			set
			{
				if (_min == value)
					return;
				_min = value;
				OnPropertyChanged("Min");
			}
		}

		public Datarates Max
		{
			get			{				return _max;			}
			set
			{
				if (_max == value)
					return;
				_max = value;
				OnPropertyChanged("Max");
			}
		}

		[JsonIgnore]
		public sbyte Value
		{
			get
			{
				return (sbyte)((Datarates)((int)_max << 4) | _min & (Datarates)255);
			}
			set
			{
				if ((int)Value == (int)value)
					return;
				OnPropertyChanged("Value");
				Max = (Datarates)((int)value >> 4 & 15);
				Min = (Datarates)((int)value & 15);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public DrRange(Datarates min, Datarates max)
		{
			_min = min;
			_max = max;
		}

		public override string ToString()
		{
			return Enum.GetName(typeof(Datarates), _min) + ", " + Enum.GetName(typeof(Datarates), _max);
		}

		public void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}
