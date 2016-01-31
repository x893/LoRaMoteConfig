using LoRaMoteConfig.Enums;
using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace LoRaMoteConfig.Data
{
	public class ChannelParams : INotifyPropertyChanged
	{
		private DrRange dataRange = new DrRange(Datarates.DR_0, Datarates.DR_0);
		private uint frequency = 0;
		private Bands band;

		public int Id { get; set; }

		[JsonIgnore]
		public uint Frequency
		{
			get
			{
				return frequency;
			}
			set
			{
				if ((int)value == (int)this.frequency)
					return;
				this.frequency = value;
				this.OnPropertyChanged("Frequency");
				this.OnPropertyChanged("FrequencyMHz");
			}
		}

		public double FrequencyMHz
		{
			get
			{
				return (double)Frequency / 1000000.0;
			}
			set
			{
				if (value * 1000000.0 == (double)this.Frequency)
					return;
				this.Frequency = Convert.ToUInt32(value * 1000000.0);
			}
		}

		public DrRange DatarateRange
		{
			get
			{
				return this.dataRange;
			}
			set
			{
				if ((int)value.Value == (int)this.dataRange.Value)
					return;
				this.dataRange.Value = value.Value;
				this.OnPropertyChanged("DatarateRange");
			}
		}

		public Bands Band
		{
			get
			{
				return this.band;
			}
			set
			{
				if (value == this.band)
					return;
				this.band = value;
				this.OnPropertyChanged("Band");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public ChannelParams()
		{
			this.Id = 0;
			this.frequency = 0U;
			this.dataRange.Value = (sbyte)0;
			this.band = Bands.BAND_G1_0;
		}

		public ChannelParams(int id, uint frequency, Datarates min, Datarates max, Bands band)
		{
			this.Id = id;
			this.frequency = frequency;
			this.dataRange.Min = min;
			this.dataRange.Max = max;
			this.band = band;
		}

		public void OnPropertyChanged(string name)
		{
			if (this.PropertyChanged == null)
				return;
			this.PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}