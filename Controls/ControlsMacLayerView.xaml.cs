using LoRaMoteConfig.Data;
using LoRaMoteConfig.Enums;
using MahApps.Metro.Controls;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Shapes;

namespace LoRaMoteConfig.Controls
{
	public partial class MacLayerView : UserControl, INotifyPropertyChanged, IComponentConnector
	{
		public event EventHandler SettingsUiUpdated;
		public event PropertyChangedEventHandler PropertyChanged;

		private AppSettings appSettings = new AppSettings();
		private List<object> IsInvalidElements = new List<object>();

		public AppSettings AppSettings
		{
			get
			{
				return appSettings;
			}
			set
			{
				if (appSettings == value)
					return;
				appSettings = value;
			}
		}

		public bool HasErrors
		{
			get
			{
				return IsInvalidElements.Count != 0;
			}
		}

		public MacLayerView()
		{
			InitializeComponent();
			tBoxChannelMask0.DataContext = (object)new PropertiesViewModel();
			tBoxChannelMask1.DataContext = (object)new PropertiesViewModel();
			tBoxChannelMask2.DataContext = (object)new PropertiesViewModel();
			tBoxChannelMask3.DataContext = (object)new PropertiesViewModel();
			tBoxChannelMask4.DataContext = (object)new PropertiesViewModel();
			tBoxChannelMask5.DataContext = (object)new PropertiesViewModel();
			tBoxNbTrans.DataContext = (object)new PropertiesViewModel();
			tBoxChannelMask0.Text = "0000";
			tBoxChannelMask1.Text = "0000";
			tBoxChannelMask2.Text = "0000";
			tBoxChannelMask3.Text = "0000";
			tBoxChannelMask4.Text = "0000";
			tBoxChannelMask5.Text = "0000";
			tBoxNbTrans.Text = "1";
		}

		private void OnSettingsUiUpdated()
		{
			if (SettingsUiUpdated != null)
				SettingsUiUpdated(this, EventArgs.Empty);
		}

		public void SettingsUiUpdate()
		{
			cBoxTxPower.SelectedIndex = (int)appSettings.Mac.ChannelsTxPower;
			cBoxDatarate.SelectedIndex = (int)appSettings.Mac.ChannelsDatarate;
			tBoxChannelMask0.Text = appSettings.Mac.ChannelsMask[0].ToString("X04");
			tBoxChannelMask1.Text = appSettings.Mac.ChannelsMask[1].ToString("X04");
			tBoxChannelMask2.Text = appSettings.Mac.ChannelsMask[2].ToString("X04");
			tBoxChannelMask3.Text = appSettings.Mac.ChannelsMask[3].ToString("X04");
			tBoxChannelMask4.Text = appSettings.Mac.ChannelsMask[4].ToString("X04");
			tBoxChannelMask5.Text = appSettings.Mac.ChannelsMask[5].ToString("X04");
			tBoxNbTrans.Text = appSettings.Mac.ChannelsNbTrans.ToString();
			nuRx2ChannelFrequency.Value = new double?((double)appSettings.Mac.Rx2Channel.Frequency / 1000000.0);
			cBoxRx2ChannelDatarate.SelectedIndex = (int)appSettings.Mac.Rx2Channel.Datarate;
			nuMaxRxWindow.Value = new double?((double)appSettings.Mac.MaxRxWindow / 1000000.0);
			nuReceiveDelay1.Value = new double?((double)appSettings.Mac.ReceiveDelay1 / 1000000.0);
			nuReceiveDelay2.Value = new double?((double)appSettings.Mac.ReceiveDelay2 / 1000000.0);
			nuJoinAcceptDelay1.Value = new double?((double)appSettings.Mac.JoinAcceptDelay1 / 1000000.0);
			nuJoinAcceptDelay2.Value = new double?((double)appSettings.Mac.JoinAcceptDelay2 / 1000000.0);
			rBtnDutyCycleOn.IsChecked = new bool?(appSettings.Mac.DutyCycleOn);
			rBtnDutyCycleOff.IsChecked = new bool?(!appSettings.Mac.DutyCycleOn);
			rBtnPublicNetwork.IsChecked = new bool?(appSettings.Mac.PublicNetwork);
			rBtnPrivateNetwork.IsChecked = new bool?(!appSettings.Mac.PublicNetwork);
			cBoxDeviceClass.SelectedIndex = (int)appSettings.Mac.DeviceClass;
			switch (appSettings.AppMode)
			{
				case AppModes.APP_GPS_TRACKING_DEMO:
					rBtnAdrOn.IsChecked = new bool?(appSettings.Params.GpsTrackingDemo.AdrOn);
					rBtnAdrOff.IsChecked = new bool?(!appSettings.Params.GpsTrackingDemo.AdrOn);
					pnlDatarate.Visibility = Visibility.Visible;
					pnlDatarateRadioCoverageTester.Visibility = Visibility.Collapsed;
					tBoxChannelMask0.IsEnabled = true;
					tBoxChannelMask1.IsEnabled = true;
					tBoxChannelMask2.IsEnabled = true;
					tBoxChannelMask3.IsEnabled = true;
					tBoxChannelMask4.IsEnabled = true;
					tBoxChannelMask5.IsEnabled = true;
					pnlAdr.Visibility = Visibility.Visible;
					tBoxNbTrans.IsEnabled = true;
					break;
				case AppModes.APP_RADIO_COVERAGE_TESTER:
					pnlDatarate.Visibility = Visibility.Collapsed;
					pnlDatarateRadioCoverageTester.Visibility = Visibility.Visible;
					tBoxChannelMask0.IsEnabled = false;
					tBoxChannelMask1.IsEnabled = false;
					tBoxChannelMask2.IsEnabled = false;
					tBoxChannelMask3.IsEnabled = false;
					tBoxChannelMask4.IsEnabled = false;
					tBoxChannelMask5.IsEnabled = false;
					pnlAdr.Visibility = Visibility.Collapsed;
					tBoxNbTrans.IsEnabled = false;
					appSettings.Mac.ChannelsNbTrans = (byte)1;
					break;
				default:
					rBtnAdrOn.IsChecked = new bool?(appSettings.Params.SensorsGpsDemo.AdrOn);
					rBtnAdrOff.IsChecked = new bool?(!appSettings.Params.SensorsGpsDemo.AdrOn);
					pnlDatarate.Visibility = Visibility.Visible;
					pnlDatarateRadioCoverageTester.Visibility = Visibility.Collapsed;
					tBoxChannelMask0.IsEnabled = true;
					tBoxChannelMask1.IsEnabled = true;
					tBoxChannelMask2.IsEnabled = true;
					tBoxChannelMask3.IsEnabled = true;
					tBoxChannelMask4.IsEnabled = true;
					tBoxChannelMask5.IsEnabled = true;
					pnlAdr.Visibility = Visibility.Visible;
					tBoxNbTrans.IsEnabled = true;
					break;
			}
			if (appSettings.Mac.DevicePhy == DevicePhys.US915 || appSettings.Mac.DevicePhy == DevicePhys.US915H)
			{
				pnlDutyCycle.Visibility = Visibility.Collapsed;
				tBoxChannelMask0.Visibility = Visibility.Visible;
				tBoxChannelMask1.Visibility = Visibility.Visible;
				tBoxChannelMask2.Visibility = Visibility.Visible;
				tBoxChannelMask3.Visibility = Visibility.Visible;
				tBoxChannelMask4.Visibility = Visibility.Visible;
				tBoxChannelMask5.Visibility = Visibility.Visible;
			}
			else
			{
				pnlDutyCycle.Visibility = Visibility.Visible;
				tBoxChannelMask0.Visibility = Visibility.Visible;
				tBoxChannelMask1.Visibility = Visibility.Collapsed;
				tBoxChannelMask2.Visibility = Visibility.Collapsed;
				tBoxChannelMask3.Visibility = Visibility.Collapsed;
				tBoxChannelMask4.Visibility = Visibility.Collapsed;
				tBoxChannelMask5.Visibility = Visibility.Collapsed;
			}
			if (appSettings.Mac.DevicePhy == DevicePhys.US915H)
			{
				tBoxChannelMask0.IsEnabled = false;
				tBoxChannelMask1.IsEnabled = false;
				tBoxChannelMask2.IsEnabled = false;
				tBoxChannelMask3.IsEnabled = false;
				tBoxChannelMask4.IsEnabled = false;
				tBoxChannelMask5.IsEnabled = false;
			}
			OnSettingsUiUpdated();
		}

		private void tBox_Error(object sender, ValidationErrorEventArgs e)
		{
			if (IsInvalidElements.Contains(sender))
				return;
			IsInvalidElements.Add(sender);
			OnPropertyChanged("HasErrors");
		}

		private void tBox_BindingTargetUpdated(object sender, DataTransferEventArgs e)
		{
			if (!IsInvalidElements.Contains(sender))
				return;
			IsInvalidElements.Remove(sender);
			OnPropertyChanged("HasErrors");
		}

		private void tBox_BindingSourceUpdated(object sender, DataTransferEventArgs e)
		{
			if (!IsInvalidElements.Contains(sender))
				return;
			IsInvalidElements.Remove(sender);
			OnPropertyChanged("HasErrors");
		}

		private void tBox_GotFocus(object sender, RoutedEventArgs e)
		{
		}

		private void tBox_LostFocus(object sender, RoutedEventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			if (textBox == tBoxChannelMask0 && !IsInvalidElements.Contains(sender))
				appSettings.Mac.ChannelsMask[0] = Convert.ToUInt16(textBox.Text, 16);
			else if (textBox == tBoxChannelMask1 && !IsInvalidElements.Contains(sender))
				appSettings.Mac.ChannelsMask[1] = Convert.ToUInt16(textBox.Text, 16);
			else if (textBox == tBoxChannelMask2 && !IsInvalidElements.Contains(sender))
				appSettings.Mac.ChannelsMask[2] = Convert.ToUInt16(textBox.Text, 16);
			else if (textBox == tBoxChannelMask3 && !IsInvalidElements.Contains(sender))
				appSettings.Mac.ChannelsMask[3] = Convert.ToUInt16(textBox.Text, 16);
			else if (textBox == tBoxChannelMask4 && !IsInvalidElements.Contains(sender))
				appSettings.Mac.ChannelsMask[4] = Convert.ToUInt16(textBox.Text, 16);
			else if (textBox == tBoxChannelMask5 && !IsInvalidElements.Contains(sender))
			{
				appSettings.Mac.ChannelsMask[5] = Convert.ToUInt16(textBox.Text, 16);
			}
			else
			{
				if (textBox != tBoxNbTrans || IsInvalidElements.Contains(sender))
					return;
				appSettings.Mac.ChannelsNbTrans = Convert.ToByte(textBox.Text);
			}
		}

		public void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		private void cBoxDeviceClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (cBoxDeviceClass.SelectedIndex == 1)
			{
				MessageBox.Show("LoRaWAN class B devices aren't yet supported.\n Reverting back to previous selected device class.", "Information", MessageBoxButton.OK, MessageBoxImage.Asterisk);
				cBoxDeviceClass.SelectedIndex = (int)appSettings.Mac.DeviceClass;
			}
			else
				appSettings.Mac.DeviceClass = (DeviceClasses)cBoxDeviceClass.SelectedIndex;
		}

		private void cBoxTxPower_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			appSettings.Mac.ChannelsTxPower = (Powers)cBoxTxPower.SelectedIndex;
		}

		private void cBoxDatarate_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			appSettings.Mac.ChannelsDatarate = (Datarates)cBoxDatarate.SelectedIndex;
		}

		private void rBtnAdr_Checked(object sender, RoutedEventArgs e)
		{
			bool flag = rBtnAdrOn.IsChecked.Value;
			appSettings.Params.GpsTrackingDemo.AdrOn = flag;
			appSettings.Params.SensorsGpsDemo.AdrOn = flag;
			if (cBoxDatarate != null)
			{
				cBoxDatarate.IsEnabled = !flag;
				cBoxDatarate.SelectedIndex = 0;
			}
			if (imgAdrWarning == null)
				return;
			imgAdrWarning.Visibility = flag ? Visibility.Visible : Visibility.Hidden;
		}

		private void rBtnDutyCycle_Checked(object sender, RoutedEventArgs e)
		{
			appSettings.Mac.DutyCycleOn = rBtnDutyCycleOn.IsChecked.Value;
			if (imgDcWarning == null)
				return;
			imgDcWarning.Visibility = !appSettings.Mac.DutyCycleOn ? Visibility.Visible : Visibility.Hidden;
		}

		private void rBtnNetworkType_Checked(object sender, RoutedEventArgs e)
		{
			appSettings.Mac.PublicNetwork = rBtnPublicNetwork.IsChecked.Value;
		}

		private void nuRx2ChannelFrequency_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
		{
			appSettings.Mac.Rx2Channel.Frequency = (uint)(e.NewValue.Value * 1000000.0);
		}

		private void tBoxRx2ChannelDatarate_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			appSettings.Mac.Rx2Channel.Datarate = (Datarates)cBoxRx2ChannelDatarate.SelectedIndex;
		}

		private void nuDelay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
		{
			NumericUpDown numericUpDown = (NumericUpDown)sender;
			if (numericUpDown == nuMaxRxWindow)
				appSettings.Mac.MaxRxWindow = (uint)(e.NewValue.Value * 1000000.0);
			if (numericUpDown == nuReceiveDelay1)
				appSettings.Mac.ReceiveDelay1 = (uint)(e.NewValue.Value * 1000000.0);
			if (numericUpDown == nuReceiveDelay2)
				appSettings.Mac.ReceiveDelay2 = (uint)(e.NewValue.Value * 1000000.0);
			if (numericUpDown == nuJoinAcceptDelay1)
				appSettings.Mac.JoinAcceptDelay1 = (uint)(e.NewValue.Value * 1000000.0);
			if (numericUpDown != nuJoinAcceptDelay2)
				return;
			appSettings.Mac.JoinAcceptDelay2 = (uint)(e.NewValue.Value * 1000000.0);
		}
	}
}
