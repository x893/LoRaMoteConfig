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
	public partial class AppGpsTrackingDemoView : UserControl, INotifyPropertyChanged, IComponentConnector
	{
		private uint txDutyCycle = 0U;
		private uint txDutyCycleRnd = 0U;
		private bool downlinkTestOn = false;

		public bool HasErrors
		{
			get
			{
				return false;
			}
		}

		public uint TxDutyCycle
		{
			get
			{
				return txDutyCycle;
			}
			set
			{
				if ((int)value == (int)txDutyCycle)
					return;
				txDutyCycle = value;
				nuTxDutyCycle.Value = new double?((double)value / 1000000.0);
				OnPropertyChanged("TxDutyCycle");
			}
		}

		public uint TxDutyCycleRnd
		{
			get
			{
				return txDutyCycleRnd;
			}
			set
			{
				if ((int)value == (int)txDutyCycleRnd)
					return;
				txDutyCycleRnd = value;
				nuTxDutyCycleRnd.Value = new double?((double)value / 1000000.0);
				OnPropertyChanged("TxDutyCycleRnd");
			}
		}

		public bool DownlinkTestOn
		{
			get
			{
				return downlinkTestOn;
			}
			set
			{
				if (value == downlinkTestOn)
					return;
				downlinkTestOn = value;
				rBtnDownlinkTestOn.IsChecked = new bool?(value);
				rBtnDownlinkTestOff.IsChecked = new bool?(!value);
				OnPropertyChanged("DownlinkTestOn");
			}
		}

		public Visibility DownlinkTestVisibility
		{
			get
			{
				return pnlDownlinkTest.Visibility;
			}
			set
			{
				if (pnlDownlinkTest.Visibility == value)
					return;
				pnlDownlinkTest.Visibility = value;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public AppGpsTrackingDemoView()
		{
			InitializeComponent();
			DataContext = (object)this;
		}

		private void nuDelay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
		{
			NumericUpDown numericUpDown = (NumericUpDown)sender;
			if (numericUpDown == nuTxDutyCycle)
			{
				txDutyCycle = (uint)(e.NewValue.Value * 1000000.0);
				OnPropertyChanged("TxDutyCycle");
			}
			else
			{
				if (numericUpDown != nuTxDutyCycleRnd)
					return;
				txDutyCycleRnd = (uint)(e.NewValue.Value * 1000000.0);
				OnPropertyChanged("TxDutyCycleRnd");
			}
		}

		private void rBtnrBtnDownlinkTestOn_Checked(object sender, RoutedEventArgs e)
		{
			downlinkTestOn = rBtnDownlinkTestOn.IsChecked.Value;
			OnPropertyChanged("DownlinkTestOn");
		}

		private void rBtnrBtnDownlinkTestOff_Checked(object sender, RoutedEventArgs e)
		{
			downlinkTestOn = rBtnDownlinkTestOn.IsChecked.Value;
			OnPropertyChanged("DownlinkTestOn");
		}

		public void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}
