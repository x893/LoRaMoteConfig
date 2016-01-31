using LoRaMoteConfig.Data;
using LoRaMoteConfig.Enums;
using MahApps.Metro.Controls;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace LoRaMoteConfig.Controls
{
	public partial class ChannelView : UserControl, IComponentConnector
	{
		private ChannelParams channel = new ChannelParams();

		public ChannelParams Channel
		{
			get
			{
				return this.channel;
			}
			set
			{
				if (value == this.channel)
					return;
				this.channel = value;
				this.channel.PropertyChanged += new PropertyChangedEventHandler(this.channel_PropertyChanged);
				this.nuFrequency.Value = new double?((double)this.channel.Frequency / 1000000.0);
				this.cBoxDatarateMin.SelectedIndex = (int)this.channel.DatarateRange.Min;
				this.cBoxDatarateMax.SelectedIndex = (int)this.channel.DatarateRange.Max;
				this.cBoxBands.SelectedIndex = (int)this.channel.Band;
			}
		}

		public ChannelView()
		{
			this.InitializeComponent();
		}

		private void channel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			try
			{
				this.VerifyAccess();
				string propertyName = e.PropertyName;
				if (!(propertyName == "Frequency"))
				{
					if (!(propertyName == "DatarateRange"))
					{
						if (!(propertyName == "Band"))
							return;
						this.cBoxBands.SelectedIndex = (int)(sender as ChannelParams).Band;
					}
					else
					{
						this.cBoxDatarateMin.SelectedIndex = (int)(sender as ChannelParams).DatarateRange.Min;
						this.cBoxDatarateMax.SelectedIndex = (int)(sender as ChannelParams).DatarateRange.Max;
					}
				}
				else
				{
					this.nuFrequency.ValueChanged -= new RoutedPropertyChangedEventHandler<double?>(this.nuFrequency_ValueChanged);
					this.nuFrequency.Value = new double?((double)(sender as ChannelParams).Frequency / 1000000.0);
					this.nuFrequency.ValueChanged += new RoutedPropertyChangedEventHandler<double?>(this.nuFrequency_ValueChanged);
				}
			}
			catch (InvalidOperationException)
			{
				this.Dispatcher.BeginInvoke((Action)(() =>
				{
					string propertyName = e.PropertyName;
					if (!(propertyName == "Frequency"))
					{
						if (!(propertyName == "DatarateRange"))
						{
							if (!(propertyName == "Band"))
								return;
							this.cBoxBands.SelectedIndex = (int)(sender as ChannelParams).Band;
						}
						else
						{
							this.cBoxDatarateMin.SelectedIndex = (int)(sender as ChannelParams).DatarateRange.Min;
							this.cBoxDatarateMax.SelectedIndex = (int)(sender as ChannelParams).DatarateRange.Max;
						}
					}
					else
					{
						this.nuFrequency.ValueChanged -= new RoutedPropertyChangedEventHandler<double?>(this.nuFrequency_ValueChanged);
						this.nuFrequency.Value = new double?((double)(sender as ChannelParams).Frequency / 1000000.0);
						this.nuFrequency.ValueChanged += new RoutedPropertyChangedEventHandler<double?>(this.nuFrequency_ValueChanged);
					}
				}));
			}
		}

		private void nuFrequency_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
		{
			this.Channel.Frequency = (uint)(e.NewValue.Value * 1000000.0);
		}

		private void cBoxDatarateMin_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			this.Channel.DatarateRange.Min = (Datarates)this.cBoxDatarateMin.SelectedIndex;
		}

		private void cBoxDatarateMax_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			this.Channel.DatarateRange.Max = (Datarates)this.cBoxDatarateMax.SelectedIndex;
		}

		private void cBoxBands_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			this.Channel.Band = (Bands)this.cBoxBands.SelectedIndex;
		}
	}
}

