using LoRaMoteConfig.Controls;
using LoRaMoteConfig.Events;
using LoRaMoteConfig.Properties;
using MahApps.Metro.Controls;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

namespace LoRaMoteConfig
{
	public partial class MainWnd : MetroWindow, IComponentConnector
	{
		public MainWnd()
		{
			InitializeComponent();
			Dispatcher.ShutdownStarted += new EventHandler(Dispatcher_ShutdownStarted);
			ConfigView.SerialPortView = serialPortView;
			ConfigView.FirmwareVersionChanged += new StringEventHandler(ConfigView_FirmwareVersionChanged);
			ConfigView.DevicePhyChanged += new DevicePhyEventHandler(ConfigView_DevicePhyChanged);
		}

		private void ConfigView_DevicePhyChanged(object sender, DevicePhyEventArg e)
		{
			tBoxTitle.Text = "LoRaMote config - LoRaWAN - " + e.Phy.ToString();
		}

		private void ConfigView_FirmwareVersionChanged(object sender, StringEventArg e)
		{
			SettingsView.FirmwareVersion = e.Value;
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			if (Top < 0.0 || Left < 0.0)
				WindowStartupLocation = WindowStartupLocation.CenterScreen;
			if (Top + Height / 2.0 > SystemParameters.VirtualScreenHeight)
				Top = SystemParameters.VirtualScreenHeight - Height;
			if (Left + Width / 2.0 > SystemParameters.VirtualScreenWidth)
				Left = SystemParameters.VirtualScreenWidth - Width;

			if (Settings.Default.ChannelsFileName == "")
				Settings.Default.ChannelsFileName = "channels.josn";
			if (Settings.Default.ChannelsFilePath == "")
				Settings.Default.ChannelsFilePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		}

		private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
		{
			serialPortView.Close();
		}

		private void serialPortView_Opened(object sender, EventArgs e)
		{
			ConfigView.IsEnabled = true;
		}

		private void serialPortView_Closed(object sender, EventArgs e)
		{
			ConfigView.IsEnabled = false;
		}
	}
}
