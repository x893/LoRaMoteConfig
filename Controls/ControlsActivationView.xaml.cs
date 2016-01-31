using LoRaMoteConfig.Data;
using LoRaMoteConfig.General;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace LoRaMoteConfig.Controls
{
	public partial class ActivationView : UserControl, INotifyPropertyChanged, IComponentConnector
	{
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

		public event EventHandler SettingsUiUpdated;

		public event PropertyChangedEventHandler PropertyChanged;

		public ActivationView()
		{
			InitializeComponent();
			tBoxAppEui.DataContext = (object)new PropertiesViewModel();
			tBoxAppKey.DataContext = (object)new PropertiesViewModel();
			tBoxDevAddr.DataContext = (object)new PropertiesViewModel();
			tBoxNwkSKey.DataContext = (object)new PropertiesViewModel();
			tBoxAppSKey.DataContext = (object)new PropertiesViewModel();
			tBoxAppEui.Text = "00-00-00-00-00-00-00-00";
			tBoxAppKey.Text = "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00";
			tBoxDevAddr.Text = "00-00-00-00";
			tBoxNwkSKey.Text = "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00";
			tBoxAppSKey.Text = "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00";
		}

		private void OnSettingsUiUpdated()
		{
			if (SettingsUiUpdated != null)
				SettingsUiUpdated(this, EventArgs.Empty);
		}

		public void SettingsUiUpdate()
		{
			rBtnOta.IsChecked = new bool?(appSettings.OtaOn);
			rBtnPa.IsChecked = new bool?(!appSettings.OtaOn);
			if (appSettings.OtaOn)
			{
				pnlOta.IsEnabled = true;
				pnlPa.IsEnabled = false;
			}
			else
			{
				pnlOta.IsEnabled = false;
				pnlPa.IsEnabled = true;
			}
			tBoxAppEui.Text = Regex.Replace(Utilities.Ba2Str(appSettings.Activation.Ota.AppEui, 0, appSettings.Activation.Ota.AppEui.Length), ".{2}(?!$)", "$0-");
			bool? isChecked = cboxAutoAppKey.IsChecked;
			bool flag = true;
			if (isChecked.GetValueOrDefault() == flag && isChecked.HasValue)
			{
				for (int index = 0; index < 8; ++index)
				{
					appSettings.Activation.Ota.AppKey[index] = (byte)((uint)appSettings.DevEui[index] ^ 170U);
					appSettings.Activation.Ota.AppKey[index + 8] = (byte)((uint)appSettings.DevEui[index] ^ 85U);
				}
			}
			tBoxAppKey.Text = Regex.Replace(Utilities.Ba2Str(appSettings.Activation.Ota.AppKey, 0, appSettings.Activation.Ota.AppKey.Length), ".{2}(?!$)", "$0-");
			tBoxDevAddr.Text = Regex.Replace(appSettings.Activation.Pa.DevAddr.ToString("X08"), ".{2}(?!$)", "$0-");
			tBoxNwkSKey.Text = Regex.Replace(Utilities.Ba2Str(appSettings.Activation.Pa.NwkSKey, 0, appSettings.Activation.Pa.NwkSKey.Length), ".{2}(?!$)", "$0-");
			tBoxAppSKey.Text = Regex.Replace(Utilities.Ba2Str(appSettings.Activation.Pa.AppSKey, 0, appSettings.Activation.Pa.AppSKey.Length), ".{2}(?!$)", "$0-");
			OnSettingsUiUpdated();
		}

		private void rBtnActivation_Checked(object sender, RoutedEventArgs e)
		{
			appSettings.OtaOn = rBtnOta.IsChecked.Value;
			if (pnlOta == null || pnlPa == null)
				return;
			SettingsUiUpdate();
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
			TextBox textBox = sender as TextBox;
			textBox.Text = Regex.Replace((textBox.Text ?? "").ToString(), "[ :.-]", "");
		}

		private void tBox_LostFocus(object sender, RoutedEventArgs e)
		{
			TextBox textBox = sender as TextBox;
			if (!textBox.IsReadOnly)
			{
				if (textBox == tBoxAppEui && !IsInvalidElements.Contains(sender))
					appSettings.Activation.Ota.AppEui = Utilities.HexStr2Ba(textBox.Text);
				else if (textBox == tBoxAppKey && !IsInvalidElements.Contains(sender))
					appSettings.Activation.Ota.AppKey = Utilities.HexStr2Ba(textBox.Text);
				else if (textBox == tBoxDevAddr && !IsInvalidElements.Contains(sender))
					appSettings.Activation.Pa.DevAddr = Convert.ToUInt32(textBox.Text, 16);
				else if (textBox == tBoxNwkSKey && !IsInvalidElements.Contains(sender))
					appSettings.Activation.Pa.NwkSKey = Utilities.HexStr2Ba(textBox.Text);
				else if (textBox == tBoxAppSKey && !IsInvalidElements.Contains(sender))
					appSettings.Activation.Pa.AppSKey = Utilities.HexStr2Ba(textBox.Text);
			}
			textBox.Text = Regex.Replace((textBox.Text ?? "").ToString(), ".{2}(?!$)", "$0-");
		}

		public void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}