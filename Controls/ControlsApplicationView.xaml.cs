using LoRaMoteConfig.Data;
using LoRaMoteConfig.Enums;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace LoRaMoteConfig.Controls
{
	public partial class ApplicationView : UserControl, INotifyPropertyChanged, IComponentConnector
	{
		public event EventHandler SettingsUiUpdated;
		public event PropertyChangedEventHandler PropertyChanged;

		private AppSettings appSettings = new AppSettings();

		public AppSettings AppSettings
		{
			get { return appSettings; }
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
				return appGpsTrackingDemoView.HasErrors || appRadioCoverageTesterView.HasErrors;
			}
		}

		public ApplicationView()
		{
			InitializeComponent();
			appGpsTrackingDemoView.PropertyChanged += new PropertyChangedEventHandler(appGpsTrackingDemoView_PropertyChanged);
			appRadioCoverageTesterView.PropertyChanged += new PropertyChangedEventHandler(appRadioCoverageTesterView_PropertyChanged);
		}

		private void OnSettingsUiUpdated()
		{
			if (SettingsUiUpdated != null)
				SettingsUiUpdated(this, EventArgs.Empty);
		}

		public void SettingsUiUpdate()
		{
			cBoxAppMode.SelectedIndex = (int)appSettings.AppMode;
			switch (appSettings.AppMode)
			{
				case AppModes.APP_GPS_TRACKING_DEMO:
					appGpsTrackingDemoView.TxDutyCycle = appSettings.Params.GpsTrackingDemo.TxDutyCycle;
					appGpsTrackingDemoView.TxDutyCycleRnd = appSettings.Params.GpsTrackingDemo.TxDutyCycleRnd;
					appGpsTrackingDemoView.DownlinkTestOn = appSettings.Params.GpsTrackingDemo.DownlinkTestOn;
					appGpsTrackingDemoView.DownlinkTestVisibility = Visibility.Visible;
					appGpsTrackingDemoView.Visibility = Visibility.Visible;
					appRadioCoverageTesterView.Visibility = Visibility.Collapsed;
					break;
				case AppModes.APP_RADIO_COVERAGE_TESTER:
					appRadioCoverageTesterView.Id = appSettings.Params.RadioCoverageTester.Id;
					appRadioCoverageTesterView.NbPackets = appSettings.Params.RadioCoverageTester.NbPackets;
					appRadioCoverageTesterView.DownlinkTestOn = appSettings.Params.RadioCoverageTester.DownlinkTestOn;
					appGpsTrackingDemoView.DownlinkTestVisibility = Visibility.Visible;
					appGpsTrackingDemoView.Visibility = Visibility.Collapsed;
					appRadioCoverageTesterView.Visibility = Visibility.Visible;
					break;
				default:
					appGpsTrackingDemoView.TxDutyCycle = appSettings.Params.SensorsGpsDemo.TxDutyCycle;
					appGpsTrackingDemoView.TxDutyCycleRnd = appSettings.Params.SensorsGpsDemo.TxDutyCycleRnd;
					appGpsTrackingDemoView.DownlinkTestOn = appSettings.Params.SensorsGpsDemo.DownlinkTestOn;
					appGpsTrackingDemoView.DownlinkTestVisibility = Visibility.Visible;
					appGpsTrackingDemoView.Visibility = Visibility.Visible;
					appRadioCoverageTesterView.Visibility = Visibility.Collapsed;
					break;
			}
			OnSettingsUiUpdated();
		}

		private void appRadioCoverageTesterView_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			string propertyName = e.PropertyName;
			if (!(propertyName == "Id"))
			{
				if (!(propertyName == "NbPackets"))
				{
					if (!(propertyName == "DownlinkTestOn"))
					{
						if (!(propertyName == "HasErrors"))
							return;
						OnPropertyChanged("HasErrors");
					}
					else
						appSettings.Params.RadioCoverageTester.DownlinkTestOn = appRadioCoverageTesterView.DownlinkTestOn;
				}
				else
					appSettings.Params.RadioCoverageTester.NbPackets = appRadioCoverageTesterView.NbPackets;
			}
			else
				appSettings.Params.RadioCoverageTester.Id = appRadioCoverageTesterView.Id;
		}

		private void appGpsTrackingDemoView_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (appSettings.AppMode)
			{
				case AppModes.APP_SENSORS_GPS_DEMO:
					string propertyName1 = e.PropertyName;
					if (!(propertyName1 == "TxDutyCycle"))
					{
						if (!(propertyName1 == "TxDutyCycleRnd"))
						{
							if (!(propertyName1 == "DownlinkTestOn"))
							{
								if (!(propertyName1 == "HasErrors"))
									break;
								OnPropertyChanged("HasErrors");
								break;
							}
							appSettings.Params.SensorsGpsDemo.DownlinkTestOn = appGpsTrackingDemoView.DownlinkTestOn;
							break;
						}
						appSettings.Params.SensorsGpsDemo.TxDutyCycleRnd = appGpsTrackingDemoView.TxDutyCycleRnd;
						break;
					}
					appSettings.Params.SensorsGpsDemo.TxDutyCycle = appGpsTrackingDemoView.TxDutyCycle;
					break;
				case AppModes.APP_GPS_TRACKING_DEMO:
					string propertyName2 = e.PropertyName;
					if (!(propertyName2 == "TxDutyCycle"))
					{
						if (!(propertyName2 == "TxDutyCycleRnd"))
						{
							if (!(propertyName2 == "DownlinkTestOn"))
							{
								if (!(propertyName2 == "HasErrors"))
									break;
								OnPropertyChanged("HasErrors");
								break;
							}
							appSettings.Params.GpsTrackingDemo.DownlinkTestOn = appGpsTrackingDemoView.DownlinkTestOn;
							break;
						}
						appSettings.Params.GpsTrackingDemo.TxDutyCycleRnd = appGpsTrackingDemoView.TxDutyCycleRnd;
						break;
					}
					appSettings.Params.GpsTrackingDemo.TxDutyCycle = appGpsTrackingDemoView.TxDutyCycle;
					break;
			}
		}

		private void cBoxAppMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			appSettings.AppMode = (AppModes)cBoxAppMode.SelectedIndex;
			if (appSettings.AppMode == AppModes.APP_RADIO_COVERAGE_TESTER)
				appSettings.Mac.ChannelsNbTrans = 1;
			if (appGpsTrackingDemoView == null || appRadioCoverageTesterView == null)
				return;
			SettingsUiUpdate();
		}

		public void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}
