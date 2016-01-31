using LoRaMoteConfig.Controls;
using LoRaMoteConfig.Data;
using LoRaMoteConfig.Enums;
using LoRaMoteConfig.Events;
using LoRaMoteConfig.General;
using LoRaMoteConfig.Properties;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace LoRaMoteConfig
{
	public partial class ConfigView : UserControl, INotifyPropertyChanged, IComponentConnector
	{
		private AppSettings appSettings = new AppSettings();
		private byte[] tstData = new byte[0];
		private List<object> IsInvalidElements = new List<object>();
		private const int FRAME_HEADER_SIZE = 3;
		private SerialPortView serialPortView;
		private ControlProtocol ctrlProtocol;

		public SerialPortView SerialPortView
		{
			get
			{
				return serialPortView;
			}
			set
			{
				if (serialPortView == value)
					return;
				serialPortView = value;
				serialPortView.Opened += new EventHandler(serialPortView_Opened);
				serialPortView.Closed += new EventHandler(serialPortView_Closed);
			}
		}

		private ControlProtocol CtrlProtocol
		{
			get
			{
				return ctrlProtocol;
			}
			set
			{
				if (ctrlProtocol == value)
					return;
				ctrlProtocol = value;
			}
		}

		private ObservableCollection<ChannelParams> ChannelsList { get; set; }

		public AppSettings AppSettings
		{
			get
			{
				return appSettings;
			}
		}

		public bool HasErrors
		{
			get
			{
				return IsInvalidElements.Count != 0;
			}
		}

		public event StringEventHandler FirmwareVersionChanged;

		public event DevicePhyEventHandler DevicePhyChanged;

		public event PropertyChangedEventHandler PropertyChanged;

		public ConfigView()
		{
			InitializeComponent();
			ChannelsList = new ObservableCollection<ChannelParams>();
			for (int id = 0; id < 16; ++id)
				ChannelsList.Add(new ChannelParams(id, 0U, Datarates.DR_0, Datarates.DR_0, Bands.BAND_G1_0));
			activationView.AppSettings = appSettings;
			activationView.SettingsUiUpdated += new EventHandler(configViews_SettingsUiUpdated);
			activationView.PropertyChanged += new PropertyChangedEventHandler(Views_PropertyChanged);
			macLayerView.AppSettings = appSettings;
			macLayerView.SettingsUiUpdated += new EventHandler(configViews_SettingsUiUpdated);
			macLayerView.PropertyChanged += new PropertyChangedEventHandler(Views_PropertyChanged);
			channelsView.ChannelsList = ChannelsList;
			channelsView.AppSettings = AppSettings;
			channelsView.SettingsUiUpdated += new EventHandler(configViews_SettingsUiUpdated);
			applicationView.AppSettings = appSettings;
			applicationView.SettingsUiUpdated += new EventHandler(configViews_SettingsUiUpdated);
			applicationView.SettingsUiUpdated += new EventHandler(applicationView_SettingsUiUpdated);
			applicationView.PropertyChanged += new PropertyChangedEventHandler(Views_PropertyChanged);
			PropertyChanged += new PropertyChangedEventHandler(Views_PropertyChanged);
			tBoxDevEui.DataContext = (object)new PropertiesViewModel();
		}

		private void Views_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (!(e.PropertyName == "HasErrors"))
				return;
			btnWrite.IsEnabled = !HasErrors && !activationView.HasErrors && !macLayerView.HasErrors && !applicationView.HasErrors;
		}

		private void OnFirmwareVersionChanged(string value)
		{
			if (FirmwareVersionChanged != null)
				FirmwareVersionChanged(this, new StringEventArg(value));
		}

		private void OnDevicePhyChangedChanged(DevicePhys phy)
		{
			if (DevicePhyChanged != null)
				DevicePhyChanged(this, new DevicePhyEventArg(phy));
		}

		private void serialPortView_Opened(object sender, EventArgs e)
		{
			ctrlProtocol = new ControlProtocol();
			ctrlProtocol.SerialPort = SerialPortView.SerialPort;
			ctrlProtocol.Open();

			System.Version softFwVersion = new System.Version(Settings.Default.FirmwareVersion);
			System.Version deviceFwVersion = ctrlProtocol.GetFirmwareVersion();
			if (softFwVersion != null
			&& deviceFwVersion != null
			&& (deviceFwVersion.Major < softFwVersion.Major || deviceFwVersion.Minor < softFwVersion.Minor)
				)
			{
				serialPortView.Close();
				MessageBox.Show("Wrong version of the firmware.\nPlease flash the latest version of the firmware.", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
			else
			{
				System.Version softEepromVersion = new System.Version(Settings.Default.DevConfigVersion);
				System.Version deviceEepromVersion = ctrlProtocol.GetEepromVersion();
				if (softFwVersion != null
				&& deviceFwVersion != null
				&& (deviceEepromVersion.Major != softEepromVersion.Major || deviceEepromVersion.Minor != softEepromVersion.Minor)
					)
				{
					serialPortView.Close();
					MessageBox.Show("Wrong version of the EEPROM layout.\nPlease flash the latest version of the firmware.", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
				}
				else
				{
					OnFirmwareVersionChanged(deviceFwVersion.ToString());

					DevicePhys devicePhy = ctrlProtocol.GetDevicePhy();
					OnDevicePhyChangedChanged(devicePhy);
					appSettings.Mac.DevicePhy = devicePhy;
					if (appSettings.Mac.Channels.Length > ChannelsList.Count)
					{
						for (int count = ChannelsList.Count; count < appSettings.Mac.Channels.Length; ++count)
							ChannelsList.Add(new ChannelParams(count, 0U, Datarates.DR_0, Datarates.DR_0, Bands.BAND_G1_0));
					}

					byte[] data = ctrlProtocol.ReadSettings();
					if (data == null)
					{
						serialPortView.Close();
						MessageBox.Show("Unable to read EEPROM data", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
					}
					else
					{
						Log(data);
						SettingsUpdateFrom(data);
					}
				}
			}
		}

		private void serialPortView_Closed(object sender, EventArgs e)
		{
			if (ctrlProtocol != null)
				ctrlProtocol.Close();
			OnFirmwareVersionChanged("Not Connected");
		}

		private uint ComputeChecksum(byte[] data)
		{
			uint num = 0U;
			for (int index = 12; index < data.Length; ++index)
				num += (uint)data[index];
			return num;
		}

		private byte[] SettingsUpdateTo()
		{
			byte[] array = new byte[1024];
			int num1 = 0;
			byte[] numArray1 = array;
			int num2 = num1;
			int num3 = num2 + 1;
			int index1 = num2;
			int num4 = (int)(byte)((uint)appSettings.Header.Magic & (uint)byte.MaxValue);
			numArray1[index1] = (byte)num4;
			byte[] numArray2 = array;
			int num5 = num3;
			int num6 = num5 + 1;
			int index2 = num5;
			int num7 = (int)(byte)((int)appSettings.Header.Magic >> 8 & (int)byte.MaxValue);
			numArray2[index2] = (byte)num7;
			int num8 = num6 + 2 + 4;
			byte[] numArray3 = array;
			int num9 = num8;
			int num10 = num9 + 1;
			int index3 = num9;
			int num11 = (int)appSettings.Header.Ver.Major;
			numArray3[index3] = (byte)num11;
			byte[] numArray4 = array;
			int num12 = num10;
			int num13 = num12 + 1;
			int index4 = num12;
			int num14 = (int)appSettings.Header.Ver.Minor;
			numArray4[index4] = (byte)num14;
			byte[] numArray5 = array;
			int num15 = num13;
			int num16 = num15 + 1;
			int index5 = num15;
			int num17 = (int)appSettings.Header.FwVer.Major;
			numArray5[index5] = (byte)num17;
			byte[] numArray6 = array;
			int num18 = num16;
			int num19 = num18 + 1;
			int index6 = num18;
			int num20 = (int)appSettings.Header.FwVer.Minor;
			numArray6[index6] = (byte)num20;
			Array.Copy((Array)appSettings.DevEui, 0, (Array)array, num19, appSettings.DevEui.Length);
			Array.Reverse((Array)array, num19, appSettings.DevEui.Length);
			int num21 = num19 + appSettings.DevEui.Length;
			byte[] numArray7 = array;
			int num22 = num21;
			int num23 = num22 + 1;
			int index7 = num22;
			int num24 = appSettings.OtaOn ? 1 : 0;
			numArray7[index7] = (byte)num24;
			int destinationIndex1 = num23 + 3;
			int num25;
			if (appSettings.OtaOn)
			{
				byte[] numArray8 = array;
				int num26 = destinationIndex1;
				int num27 = num26 + 1;
				int index8 = num26;
				int num28 = (int)(byte)(appSettings.Activation.Ota.DutyCycle & (uint)byte.MaxValue);
				numArray8[index8] = (byte)num28;
				byte[] numArray9 = array;
				int num29 = num27;
				int num30 = num29 + 1;
				int index9 = num29;
				int num31 = (int)(byte)(appSettings.Activation.Ota.DutyCycle >> 8 & (uint)byte.MaxValue);
				numArray9[index9] = (byte)num31;
				byte[] numArray10 = array;
				int num32 = num30;
				int num33 = num32 + 1;
				int index10 = num32;
				int num34 = (int)(byte)(appSettings.Activation.Ota.DutyCycle >> 16 & (uint)byte.MaxValue);
				numArray10[index10] = (byte)num34;
				byte[] numArray11 = array;
				int num35 = num33;
				int num36 = num35 + 1;
				int index11 = num35;
				int num37 = (int)(byte)(appSettings.Activation.Ota.DutyCycle >> 24 & (uint)byte.MaxValue);
				numArray11[index11] = (byte)num37;
				Array.Copy((Array)appSettings.Activation.Ota.AppEui, 0, (Array)array, num36, appSettings.Activation.Ota.AppEui.Length);
				Array.Reverse((Array)array, num36, appSettings.Activation.Ota.AppEui.Length);
				int destinationIndex2 = num36 + appSettings.Activation.Ota.AppEui.Length;
				Array.Copy((Array)appSettings.Activation.Ota.AppKey, 0, (Array)array, destinationIndex2, appSettings.Activation.Ota.AppKey.Length);
				num25 = destinationIndex2 + (appSettings.Activation.Ota.AppKey.Length + 8);
			}
			else
			{
				Array.Copy((Array)appSettings.Activation.Pa.NwkSKey, 0, (Array)array, destinationIndex1, appSettings.Activation.Pa.NwkSKey.Length);
				int destinationIndex2 = destinationIndex1 + appSettings.Activation.Pa.NwkSKey.Length;
				Array.Copy((Array)appSettings.Activation.Pa.AppSKey, 0, (Array)array, destinationIndex2, appSettings.Activation.Pa.AppSKey.Length);
				int num26 = destinationIndex2 + appSettings.Activation.Pa.AppSKey.Length;
				byte[] numArray8 = array;
				int num27 = num26;
				int num28 = num27 + 1;
				int index8 = num27;
				int num29 = (int)(byte)(appSettings.Activation.Pa.DevAddr & (uint)byte.MaxValue);
				numArray8[index8] = (byte)num29;
				byte[] numArray9 = array;
				int num30 = num28;
				int num31 = num30 + 1;
				int index9 = num30;
				int num32 = (int)(byte)(appSettings.Activation.Pa.DevAddr >> 8 & (uint)byte.MaxValue);
				numArray9[index9] = (byte)num32;
				byte[] numArray10 = array;
				int num33 = num31;
				int num34 = num33 + 1;
				int index10 = num33;
				int num35 = (int)(byte)(appSettings.Activation.Pa.DevAddr >> 16 & (uint)byte.MaxValue);
				numArray10[index10] = (byte)num35;
				byte[] numArray11 = array;
				int num36 = num34;
				num25 = num36 + 1;
				int index11 = num36;
				int num37 = (int)(byte)(appSettings.Activation.Pa.DevAddr >> 24 & (uint)byte.MaxValue);
				numArray11[index11] = (byte)num37;
			}
			for (int index8 = 0; index8 < appSettings.Mac.Channels.Length; ++index8)
			{
				byte[] numArray8 = array;
				int num26 = num25;
				int num27 = num26 + 1;
				int index9 = num26;
				int num28 = (int)(byte)(appSettings.Mac.Channels[index8].Frequency & (uint)byte.MaxValue);
				numArray8[index9] = (byte)num28;
				byte[] numArray9 = array;
				int num29 = num27;
				int num30 = num29 + 1;
				int index10 = num29;
				int num31 = (int)(byte)(appSettings.Mac.Channels[index8].Frequency >> 8 & (uint)byte.MaxValue);
				numArray9[index10] = (byte)num31;
				byte[] numArray10 = array;
				int num32 = num30;
				int num33 = num32 + 1;
				int index11 = num32;
				int num34 = (int)(byte)(appSettings.Mac.Channels[index8].Frequency >> 16 & (uint)byte.MaxValue);
				numArray10[index11] = (byte)num34;
				byte[] numArray11 = array;
				int num35 = num33;
				int num36 = num35 + 1;
				int index12 = num35;
				int num37 = (int)(byte)(appSettings.Mac.Channels[index8].Frequency >> 24 & (uint)byte.MaxValue);
				numArray11[index12] = (byte)num37;
				byte[] numArray12 = array;
				int num38 = num36;
				int num39 = num38 + 1;
				int index13 = num38;
				int num40 = (int)(byte)appSettings.Mac.Channels[index8].DatarateRange.Value;
				numArray12[index13] = (byte)num40;
				byte[] numArray13 = array;
				int num41 = num39;
				int num42 = num41 + 1;
				int index14 = num41;
				int num43 = (int)(byte)appSettings.Mac.Channels[index8].Band;
				numArray13[index14] = (byte)num43;
				num25 = num42 + 2;
			}
			byte[] numArray14 = array;
			int num44 = num25;
			int num45 = num44 + 1;
			int index15 = num44;
			int num46 = (int)(byte)(appSettings.Mac.Rx2Channel.Frequency & (uint)byte.MaxValue);
			numArray14[index15] = (byte)num46;
			byte[] numArray15 = array;
			int num47 = num45;
			int num48 = num47 + 1;
			int index16 = num47;
			int num49 = (int)(byte)(appSettings.Mac.Rx2Channel.Frequency >> 8 & (uint)byte.MaxValue);
			numArray15[index16] = (byte)num49;
			byte[] numArray16 = array;
			int num50 = num48;
			int num51 = num50 + 1;
			int index17 = num50;
			int num52 = (int)(byte)(appSettings.Mac.Rx2Channel.Frequency >> 16 & (uint)byte.MaxValue);
			numArray16[index17] = (byte)num52;
			byte[] numArray17 = array;
			int num53 = num51;
			int num54 = num53 + 1;
			int index18 = num53;
			int num55 = (int)(byte)(appSettings.Mac.Rx2Channel.Frequency >> 24 & (uint)byte.MaxValue);
			numArray17[index18] = (byte)num55;
			byte[] numArray18 = array;
			int num56 = num54;
			int num57 = num56 + 1;
			int index19 = num56;
			int num58 = (int)(byte)appSettings.Mac.Rx2Channel.Datarate;
			numArray18[index19] = (byte)num58;
			int num59 = num57 + 3;
			byte[] numArray19 = array;
			int num60 = num59;
			int num61 = num60 + 1;
			int index20 = num60;
			int num62 = (int)(byte)appSettings.Mac.ChannelsTxPower;
			numArray19[index20] = (byte)num62;
			byte[] numArray20 = array;
			int num63 = num61;
			int num64 = num63 + 1;
			int index21 = num63;
			int num65 = (int)(byte)appSettings.Mac.ChannelsDatarate;
			numArray20[index21] = (byte)num65;
			byte[] numArray21 = array;
			int num66 = num64;
			int num67 = num66 + 1;
			int index22 = num66;
			int num68 = (int)(byte)((uint)appSettings.Mac.ChannelsMask[0] & (uint)byte.MaxValue);
			numArray21[index22] = (byte)num68;
			byte[] numArray22 = array;
			int num69 = num67;
			int num70 = num69 + 1;
			int index23 = num69;
			int num71 = (int)(byte)((int)appSettings.Mac.ChannelsMask[0] >> 8 & (int)byte.MaxValue);
			numArray22[index23] = (byte)num71;
			byte[] numArray23 = array;
			int num72 = num70;
			int num73 = num72 + 1;
			int index24 = num72;
			int num74 = (int)(byte)((uint)appSettings.Mac.ChannelsMask[1] & (uint)byte.MaxValue);
			numArray23[index24] = (byte)num74;
			byte[] numArray24 = array;
			int num75 = num73;
			int num76 = num75 + 1;
			int index25 = num75;
			int num77 = (int)(byte)((int)appSettings.Mac.ChannelsMask[1] >> 8 & (int)byte.MaxValue);
			numArray24[index25] = (byte)num77;
			byte[] numArray25 = array;
			int num78 = num76;
			int num79 = num78 + 1;
			int index26 = num78;
			int num80 = (int)(byte)((uint)appSettings.Mac.ChannelsMask[2] & (uint)byte.MaxValue);
			numArray25[index26] = (byte)num80;
			byte[] numArray26 = array;
			int num81 = num79;
			int num82 = num81 + 1;
			int index27 = num81;
			int num83 = (int)(byte)((int)appSettings.Mac.ChannelsMask[2] >> 8 & (int)byte.MaxValue);
			numArray26[index27] = (byte)num83;
			byte[] numArray27 = array;
			int num84 = num82;
			int num85 = num84 + 1;
			int index28 = num84;
			int num86 = (int)(byte)((uint)appSettings.Mac.ChannelsMask[3] & (uint)byte.MaxValue);
			numArray27[index28] = (byte)num86;
			byte[] numArray28 = array;
			int num87 = num85;
			int num88 = num87 + 1;
			int index29 = num87;
			int num89 = (int)(byte)((int)appSettings.Mac.ChannelsMask[3] >> 8 & (int)byte.MaxValue);
			numArray28[index29] = (byte)num89;
			byte[] numArray29 = array;
			int num90 = num88;
			int num91 = num90 + 1;
			int index30 = num90;
			int num92 = (int)(byte)((uint)appSettings.Mac.ChannelsMask[4] & (uint)byte.MaxValue);
			numArray29[index30] = (byte)num92;
			byte[] numArray30 = array;
			int num93 = num91;
			int num94 = num93 + 1;
			int index31 = num93;
			int num95 = (int)(byte)((int)appSettings.Mac.ChannelsMask[4] >> 8 & (int)byte.MaxValue);
			numArray30[index31] = (byte)num95;
			byte[] numArray31 = array;
			int num96 = num94;
			int num97 = num96 + 1;
			int index32 = num96;
			int num98 = (int)(byte)((uint)appSettings.Mac.ChannelsMask[5] & (uint)byte.MaxValue);
			numArray31[index32] = (byte)num98;
			byte[] numArray32 = array;
			int num99 = num97;
			int num100 = num99 + 1;
			int index33 = num99;
			int num101 = (int)(byte)((int)appSettings.Mac.ChannelsMask[5] >> 8 & (int)byte.MaxValue);
			numArray32[index33] = (byte)num101;
			byte[] numArray33 = array;
			int num102 = num100;
			int num103 = num102 + 1;
			int index34 = num102;
			int num104 = (int)appSettings.Mac.ChannelsNbTrans;
			numArray33[index34] = (byte)num104;
			int num105 = num103 + 1;
			byte[] numArray34 = array;
			int num106 = num105;
			int num107 = num106 + 1;
			int index35 = num106;
			int num108 = (int)(byte)(appSettings.Mac.MaxRxWindow & (uint)byte.MaxValue);
			numArray34[index35] = (byte)num108;
			byte[] numArray35 = array;
			int num109 = num107;
			int num110 = num109 + 1;
			int index36 = num109;
			int num111 = (int)(byte)(appSettings.Mac.MaxRxWindow >> 8 & (uint)byte.MaxValue);
			numArray35[index36] = (byte)num111;
			byte[] numArray36 = array;
			int num112 = num110;
			int num113 = num112 + 1;
			int index37 = num112;
			int num114 = (int)(byte)(appSettings.Mac.MaxRxWindow >> 16 & (uint)byte.MaxValue);
			numArray36[index37] = (byte)num114;
			byte[] numArray37 = array;
			int num115 = num113;
			int num116 = num115 + 1;
			int index38 = num115;
			int num117 = (int)(byte)(appSettings.Mac.MaxRxWindow >> 24 & (uint)byte.MaxValue);
			numArray37[index38] = (byte)num117;
			byte[] numArray38 = array;
			int num118 = num116;
			int num119 = num118 + 1;
			int index39 = num118;
			int num120 = (int)(byte)(appSettings.Mac.ReceiveDelay1 & (uint)byte.MaxValue);
			numArray38[index39] = (byte)num120;
			byte[] numArray39 = array;
			int num121 = num119;
			int num122 = num121 + 1;
			int index40 = num121;
			int num123 = (int)(byte)(appSettings.Mac.ReceiveDelay1 >> 8 & (uint)byte.MaxValue);
			numArray39[index40] = (byte)num123;
			byte[] numArray40 = array;
			int num124 = num122;
			int num125 = num124 + 1;
			int index41 = num124;
			int num126 = (int)(byte)(appSettings.Mac.ReceiveDelay1 >> 16 & (uint)byte.MaxValue);
			numArray40[index41] = (byte)num126;
			byte[] numArray41 = array;
			int num127 = num125;
			int num128 = num127 + 1;
			int index42 = num127;
			int num129 = (int)(byte)(appSettings.Mac.ReceiveDelay1 >> 24 & (uint)byte.MaxValue);
			numArray41[index42] = (byte)num129;
			byte[] numArray42 = array;
			int num130 = num128;
			int num131 = num130 + 1;
			int index43 = num130;
			int num132 = (int)(byte)(appSettings.Mac.ReceiveDelay2 & (uint)byte.MaxValue);
			numArray42[index43] = (byte)num132;
			byte[] numArray43 = array;
			int num133 = num131;
			int num134 = num133 + 1;
			int index44 = num133;
			int num135 = (int)(byte)(appSettings.Mac.ReceiveDelay2 >> 8 & (uint)byte.MaxValue);
			numArray43[index44] = (byte)num135;
			byte[] numArray44 = array;
			int num136 = num134;
			int num137 = num136 + 1;
			int index45 = num136;
			int num138 = (int)(byte)(appSettings.Mac.ReceiveDelay2 >> 16 & (uint)byte.MaxValue);
			numArray44[index45] = (byte)num138;
			byte[] numArray45 = array;
			int num139 = num137;
			int num140 = num139 + 1;
			int index46 = num139;
			int num141 = (int)(byte)(appSettings.Mac.ReceiveDelay2 >> 24 & (uint)byte.MaxValue);
			numArray45[index46] = (byte)num141;
			byte[] numArray46 = array;
			int num142 = num140;
			int num143 = num142 + 1;
			int index47 = num142;
			int num144 = (int)(byte)(appSettings.Mac.JoinAcceptDelay1 & (uint)byte.MaxValue);
			numArray46[index47] = (byte)num144;
			byte[] numArray47 = array;
			int num145 = num143;
			int num146 = num145 + 1;
			int index48 = num145;
			int num147 = (int)(byte)(appSettings.Mac.JoinAcceptDelay1 >> 8 & (uint)byte.MaxValue);
			numArray47[index48] = (byte)num147;
			byte[] numArray48 = array;
			int num148 = num146;
			int num149 = num148 + 1;
			int index49 = num148;
			int num150 = (int)(byte)(appSettings.Mac.JoinAcceptDelay1 >> 16 & (uint)byte.MaxValue);
			numArray48[index49] = (byte)num150;
			byte[] numArray49 = array;
			int num151 = num149;
			int num152 = num151 + 1;
			int index50 = num151;
			int num153 = (int)(byte)(appSettings.Mac.JoinAcceptDelay1 >> 24 & (uint)byte.MaxValue);
			numArray49[index50] = (byte)num153;
			byte[] numArray50 = array;
			int num154 = num152;
			int num155 = num154 + 1;
			int index51 = num154;
			int num156 = (int)(byte)(appSettings.Mac.JoinAcceptDelay2 & (uint)byte.MaxValue);
			numArray50[index51] = (byte)num156;
			byte[] numArray51 = array;
			int num157 = num155;
			int num158 = num157 + 1;
			int index52 = num157;
			int num159 = (int)(byte)(appSettings.Mac.JoinAcceptDelay2 >> 8 & (uint)byte.MaxValue);
			numArray51[index52] = (byte)num159;
			byte[] numArray52 = array;
			int num160 = num158;
			int num161 = num160 + 1;
			int index53 = num160;
			int num162 = (int)(byte)(appSettings.Mac.JoinAcceptDelay2 >> 16 & (uint)byte.MaxValue);
			numArray52[index53] = (byte)num162;
			byte[] numArray53 = array;
			int num163 = num161;
			int num164 = num163 + 1;
			int index54 = num163;
			int num165 = (int)(byte)(appSettings.Mac.JoinAcceptDelay2 >> 24 & (uint)byte.MaxValue);
			numArray53[index54] = (byte)num165;
			byte[] numArray54 = array;
			int num166 = num164;
			int num167 = num166 + 1;
			int index55 = num166;
			int num168 = appSettings.Mac.DutyCycleOn ? 1 : 0;
			numArray54[index55] = (byte)num168;
			byte[] numArray55 = array;
			int num169 = num167;
			int num170 = num169 + 1;
			int index56 = num169;
			int num171 = appSettings.Mac.PublicNetwork ? 1 : 0;
			numArray55[index56] = (byte)num171;
			byte[] numArray56 = array;
			int num172 = num170;
			int num173 = num172 + 1;
			int index57 = num172;
			int num174 = (int)(byte)appSettings.Mac.DeviceClass;
			numArray56[index57] = (byte)num174;
			byte[] numArray57 = array;
			int num175 = num173;
			int num176 = num175 + 1;
			int index58 = num175;
			int num177 = (int)(byte)appSettings.Mac.DevicePhy;
			numArray57[index58] = (byte)num177;
			byte[] numArray58 = array;
			int num178 = num176;
			int num179 = num178 + 1;
			int index59 = num178;
			int num180 = (int)(byte)appSettings.AppMode;
			numArray58[index59] = (byte)num180;
			int num181 = num179 + 3;
			int newSize;
			switch (appSettings.AppMode)
			{
				case AppModes.APP_GPS_TRACKING_DEMO:
					byte[] numArray59 = array;
					int num182 = num181;
					int num183 = num182 + 1;
					int index60 = num182;
					int num184 = (int)(byte)(appSettings.Params.GpsTrackingDemo.TxDutyCycle & (uint)byte.MaxValue);
					numArray59[index60] = (byte)num184;
					byte[] numArray60 = array;
					int num185 = num183;
					int num186 = num185 + 1;
					int index61 = num185;
					int num187 = (int)(byte)(appSettings.Params.GpsTrackingDemo.TxDutyCycle >> 8 & (uint)byte.MaxValue);
					numArray60[index61] = (byte)num187;
					byte[] numArray61 = array;
					int num188 = num186;
					int num189 = num188 + 1;
					int index62 = num188;
					int num190 = (int)(byte)(appSettings.Params.GpsTrackingDemo.TxDutyCycle >> 16 & (uint)byte.MaxValue);
					numArray61[index62] = (byte)num190;
					byte[] numArray62 = array;
					int num191 = num189;
					int num192 = num191 + 1;
					int index63 = num191;
					int num193 = (int)(byte)(appSettings.Params.GpsTrackingDemo.TxDutyCycle >> 24 & (uint)byte.MaxValue);
					numArray62[index63] = (byte)num193;
					byte[] numArray63 = array;
					int num194 = num192;
					int num195 = num194 + 1;
					int index64 = num194;
					int num196 = (int)(byte)(appSettings.Params.GpsTrackingDemo.TxDutyCycleRnd & (uint)byte.MaxValue);
					numArray63[index64] = (byte)num196;
					byte[] numArray64 = array;
					int num197 = num195;
					int num198 = num197 + 1;
					int index65 = num197;
					int num199 = (int)(byte)(appSettings.Params.GpsTrackingDemo.TxDutyCycleRnd >> 8 & (uint)byte.MaxValue);
					numArray64[index65] = (byte)num199;
					byte[] numArray65 = array;
					int num200 = num198;
					int num201 = num200 + 1;
					int index66 = num200;
					int num202 = (int)(byte)(appSettings.Params.GpsTrackingDemo.TxDutyCycleRnd >> 16 & (uint)byte.MaxValue);
					numArray65[index66] = (byte)num202;
					byte[] numArray66 = array;
					int num203 = num201;
					int num204 = num203 + 1;
					int index67 = num203;
					int num205 = (int)(byte)(appSettings.Params.GpsTrackingDemo.TxDutyCycleRnd >> 24 & (uint)byte.MaxValue);
					numArray66[index67] = (byte)num205;
					byte[] numArray67 = array;
					int num206 = num204;
					int num207 = num206 + 1;
					int index68 = num206;
					int num208 = appSettings.Params.GpsTrackingDemo.AdrOn ? 1 : 0;
					numArray67[index68] = (byte)num208;
					byte[] numArray68 = array;
					int num209 = num207;
					int num210 = num209 + 1;
					int index69 = num209;
					int num211 = appSettings.Params.GpsTrackingDemo.DownlinkTestOn ? 1 : 0;
					numArray68[index69] = (byte)num211;
					newSize = num210 + 2;
					break;
				case AppModes.APP_RADIO_COVERAGE_TESTER:
					byte[] numArray69 = array;
					int num212 = num181;
					int num213 = num212 + 1;
					int index70 = num212;
					int num214 = (int)appSettings.Params.RadioCoverageTester.Id;
					numArray69[index70] = (byte)num214;
					byte[] numArray70 = array;
					int num215 = num213;
					int num216 = num215 + 1;
					int index71 = num215;
					int num217 = (int)appSettings.Params.RadioCoverageTester.NbPackets;
					numArray70[index71] = (byte)num217;
					byte[] numArray71 = array;
					int num218 = num216;
					int num219 = num218 + 1;
					int index72 = num218;
					int num220 = appSettings.Params.RadioCoverageTester.DownlinkTestOn ? 1 : 0;
					numArray71[index72] = (byte)num220;
					newSize = num219 + 2 + 7;
					break;
				default:
					byte[] numArray72 = array;
					int num221 = num181;
					int num222 = num221 + 1;
					int index73 = num221;
					int num223 = (int)(byte)(appSettings.Params.SensorsGpsDemo.TxDutyCycle & (uint)byte.MaxValue);
					numArray72[index73] = (byte)num223;
					byte[] numArray73 = array;
					int num224 = num222;
					int num225 = num224 + 1;
					int index74 = num224;
					int num226 = (int)(byte)(appSettings.Params.SensorsGpsDemo.TxDutyCycle >> 8 & (uint)byte.MaxValue);
					numArray73[index74] = (byte)num226;
					byte[] numArray74 = array;
					int num227 = num225;
					int num228 = num227 + 1;
					int index75 = num227;
					int num229 = (int)(byte)(appSettings.Params.SensorsGpsDemo.TxDutyCycle >> 16 & (uint)byte.MaxValue);
					numArray74[index75] = (byte)num229;
					byte[] numArray75 = array;
					int num230 = num228;
					int num231 = num230 + 1;
					int index76 = num230;
					int num232 = (int)(byte)(appSettings.Params.SensorsGpsDemo.TxDutyCycle >> 24 & (uint)byte.MaxValue);
					numArray75[index76] = (byte)num232;
					byte[] numArray76 = array;
					int num233 = num231;
					int num234 = num233 + 1;
					int index77 = num233;
					int num235 = (int)(byte)(appSettings.Params.SensorsGpsDemo.TxDutyCycleRnd & (uint)byte.MaxValue);
					numArray76[index77] = (byte)num235;
					byte[] numArray77 = array;
					int num236 = num234;
					int num237 = num236 + 1;
					int index78 = num236;
					int num238 = (int)(byte)(appSettings.Params.SensorsGpsDemo.TxDutyCycleRnd >> 8 & (uint)byte.MaxValue);
					numArray77[index78] = (byte)num238;
					byte[] numArray78 = array;
					int num239 = num237;
					int num240 = num239 + 1;
					int index79 = num239;
					int num241 = (int)(byte)(appSettings.Params.SensorsGpsDemo.TxDutyCycleRnd >> 16 & (uint)byte.MaxValue);
					numArray78[index79] = (byte)num241;
					byte[] numArray79 = array;
					int num242 = num240;
					int num243 = num242 + 1;
					int index80 = num242;
					int num244 = (int)(byte)(appSettings.Params.SensorsGpsDemo.TxDutyCycleRnd >> 24 & (uint)byte.MaxValue);
					numArray79[index80] = (byte)num244;
					byte[] numArray80 = array;
					int num245 = num243;
					int num246 = num245 + 1;
					int index81 = num245;
					int num247 = appSettings.Params.SensorsGpsDemo.AdrOn ? 1 : 0;
					numArray80[index81] = (byte)num247;
					byte[] numArray81 = array;
					int num248 = num246;
					int num249 = num248 + 1;
					int index82 = num248;
					int num250 = appSettings.Params.SensorsGpsDemo.DownlinkTestOn ? 1 : 0;
					numArray81[index82] = (byte)num250;
					newSize = num249 + 2;
					break;
			}
			Array.Resize<byte>(ref array, newSize);
			uint checksum = ComputeChecksum(array);
			array[4] = (byte)(checksum & (uint)byte.MaxValue);
			array[5] = (byte)(checksum >> 8 & (uint)byte.MaxValue);
			array[6] = (byte)(checksum >> 16 & (uint)byte.MaxValue);
			array[7] = (byte)(checksum >> 24 & (uint)byte.MaxValue);
			return array;
		}

		private void SettingsUpdateFrom(byte[] data)
		{
			try
			{
				if (data == null)
					return;
				int index1 = 0;
				appSettings.Header.Magic = (ushort)((uint)data[index1] | (uint)data[index1 + 1] << 8);
				int index2 = index1 + 2 + 2;
				appSettings.Header.Crc = (uint)((int)data[index2] | (int)data[index2 + 1] << 8 | (int)data[index2 + 2] << 16 | (int)data[index2 + 3] << 24);
				int num1 = index2 + 4;
				if ((int)appSettings.Header.Crc != (int)ComputeChecksum(data))
				{
					Console.WriteLine("ERROR: Wrong checksum!!!");
				}
				else
				{
					Header.Version version1 = appSettings.Header.Ver;
					byte[] numArray1 = data;
					int num2 = num1;
					int num3 = num2 + 1;
					int index3 = num2;
					int num4 = (int)numArray1[index3];
					version1.Major = (byte)num4;
					Header.Version version2 = appSettings.Header.Ver;
					byte[] numArray2 = data;
					int num5 = num3;
					int num6 = num5 + 1;
					int index4 = num5;
					int num7 = (int)numArray2[index4];
					version2.Minor = (byte)num7;
					Header.Version version3 = appSettings.Header.FwVer;
					byte[] numArray3 = data;
					int num8 = num6;
					int num9 = num8 + 1;
					int index5 = num8;
					int num10 = (int)numArray3[index5];
					version3.Major = (byte)num10;
					Header.Version version4 = appSettings.Header.FwVer;
					byte[] numArray4 = data;
					int num11 = num9;
					int num12 = num11 + 1;
					int index6 = num11;
					int num13 = (int)numArray4[index6];
					version4.Minor = (byte)num13;
					Array.Reverse((Array)data, num12, appSettings.DevEui.Length);
					Array.Copy((Array)data, num12, (Array)appSettings.DevEui, 0, appSettings.DevEui.Length);
					int num14 = num12 + appSettings.DevEui.Length;
					AppSettings appSettings1 = appSettings;
					byte[] numArray5 = data;
					int num15 = num14;
					int num16 = num15 + 1;
					int index7 = num15;
					int num17 = (int)numArray5[index7] == 0 ? 0 : 1;
					appSettings1.OtaOn = num17 != 0;
					int sourceIndex1 = num16 + 3;
					int index8;
					if (appSettings.OtaOn)
					{
						appSettings.Activation.Ota.DutyCycle = (uint)((int)data[sourceIndex1] | (int)data[sourceIndex1 + 1] << 8 | (int)data[sourceIndex1 + 2] << 16 | (int)data[sourceIndex1 + 3] << 24);
						int num18 = sourceIndex1 + 4;
						Array.Reverse((Array)data, num18, appSettings.Activation.Ota.AppEui.Length);
						Array.Copy((Array)data, num18, (Array)appSettings.Activation.Ota.AppEui, 0, appSettings.Activation.Ota.AppEui.Length);
						int sourceIndex2 = num18 + appSettings.Activation.Ota.AppEui.Length;
						Array.Copy((Array)data, sourceIndex2, (Array)appSettings.Activation.Ota.AppKey, 0, appSettings.Activation.Ota.AppKey.Length);
						index8 = sourceIndex2 + (appSettings.Activation.Ota.AppKey.Length + 8);
					}
					else
					{
						Array.Copy((Array)data, sourceIndex1, (Array)appSettings.Activation.Pa.NwkSKey, 0, appSettings.Activation.Pa.NwkSKey.Length);
						int sourceIndex2 = sourceIndex1 + appSettings.Activation.Pa.NwkSKey.Length;
						Array.Copy((Array)data, sourceIndex2, (Array)appSettings.Activation.Pa.AppSKey, 0, appSettings.Activation.Pa.AppSKey.Length);
						int index9 = sourceIndex2 + appSettings.Activation.Pa.AppSKey.Length;
						appSettings.Activation.Pa.DevAddr = (uint)((int)data[index9] | (int)data[index9 + 1] << 8 | (int)data[index9 + 2] << 16 | (int)data[index9 + 3] << 24);
						index8 = index9 + 4;
					}
					ChannelParams channelParams1 = new ChannelParams();
					for (int index9 = 0; index9 < appSettings.Mac.Channels.Length; ++index9)
					{
						ChannelsList[index9].Frequency = appSettings.Mac.Channels[index9].Frequency = (uint)((int)data[index8] | (int)data[index8 + 1] << 8 | (int)data[index8 + 2] << 16 | (int)data[index8 + 3] << 24);
						int num18 = index8 + 4;
						DrRange datarateRange1 = ChannelsList[index9].DatarateRange;
						DrRange datarateRange2 = appSettings.Mac.Channels[index9].DatarateRange;
						byte[] numArray6 = data;
						int num19 = num18;
						int num20 = num19 + 1;
						int index10 = num19;
						int num21;
						sbyte num22 = (sbyte)(num21 = (int)(sbyte)numArray6[index10]);
						datarateRange2.Value = (sbyte)num21;
						int num23 = (int)num22;
						datarateRange1.Value = (sbyte)num23;
						ChannelParams channelParams2 = ChannelsList[index9];
						ChannelParams channelParams3 = appSettings.Mac.Channels[index9];
						byte[] numArray7 = data;
						int num24 = num20;
						int num25 = num24 + 1;
						int index11 = num24;
						int num26;
						Bands bands = (Bands)(num26 = (int)numArray7[index11]);
						channelParams3.Band = (Bands)num26;
						int num27 = (int)bands;
						channelParams2.Band = (Bands)num27;
						index8 = num25 + 2;
					}
					appSettings.Mac.Rx2Channel.Frequency = (uint)((int)data[index8] | (int)data[index8 + 1] << 8 | (int)data[index8 + 2] << 16 | (int)data[index8 + 3] << 24);
					int num28 = index8 + 4;
					Rx2ChannelParams rx2ChannelParams = appSettings.Mac.Rx2Channel;
					byte[] numArray8 = data;
					int num29 = num28;
					int num30 = num29 + 1;
					int index12 = num29;
					int num31 = (int)numArray8[index12];
					rx2ChannelParams.Datarate = (Datarates)num31;
					int num32 = num30 + 3;
					Mac mac1 = appSettings.Mac;
					byte[] numArray9 = data;
					int num33 = num32;
					int num34 = num33 + 1;
					int index13 = num33;
					int num35 = (int)numArray9[index13];
					mac1.ChannelsTxPower = (Powers)num35;
					Mac mac2 = appSettings.Mac;
					byte[] numArray10 = data;
					int num36 = num34;
					int index14 = num36 + 1;
					int index15 = num36;
					int num37 = (int)numArray10[index15];
					mac2.ChannelsDatarate = (Datarates)num37;
					appSettings.Mac.ChannelsMask[0] = (ushort)((uint)data[index14] | (uint)data[index14 + 1] << 8);
					int index16 = index14 + 2;
					appSettings.Mac.ChannelsMask[1] = (ushort)((uint)data[index16] | (uint)data[index16 + 1] << 8);
					int index17 = index16 + 2;
					appSettings.Mac.ChannelsMask[2] = (ushort)((uint)data[index17] | (uint)data[index17 + 1] << 8);
					int index18 = index17 + 2;
					appSettings.Mac.ChannelsMask[3] = (ushort)((uint)data[index18] | (uint)data[index18 + 1] << 8);
					int index19 = index18 + 2;
					appSettings.Mac.ChannelsMask[4] = (ushort)((uint)data[index19] | (uint)data[index19 + 1] << 8);
					int index20 = index19 + 2;
					appSettings.Mac.ChannelsMask[5] = (ushort)((uint)data[index20] | (uint)data[index20 + 1] << 8);
					int num38 = index20 + 2;
					Mac mac3 = appSettings.Mac;
					byte[] numArray11 = data;
					int num39 = num38;
					int num40 = num39 + 1;
					int index21 = num39;
					int num41 = (int)numArray11[index21];
					mac3.ChannelsNbTrans = (byte)num41;
					int index22 = num40 + 1;
					appSettings.Mac.MaxRxWindow = (uint)((int)data[index22] | (int)data[index22 + 1] << 8 | (int)data[index22 + 2] << 16 | (int)data[index22 + 3] << 24);
					int index23 = index22 + 4;
					appSettings.Mac.ReceiveDelay1 = (uint)((int)data[index23] | (int)data[index23 + 1] << 8 | (int)data[index23 + 2] << 16 | (int)data[index23 + 3] << 24);
					int index24 = index23 + 4;
					appSettings.Mac.ReceiveDelay2 = (uint)((int)data[index24] | (int)data[index24 + 1] << 8 | (int)data[index24 + 2] << 16 | (int)data[index24 + 3] << 24);
					int index25 = index24 + 4;
					appSettings.Mac.JoinAcceptDelay1 = (uint)((int)data[index25] | (int)data[index25 + 1] << 8 | (int)data[index25 + 2] << 16 | (int)data[index25 + 3] << 24);
					int index26 = index25 + 4;
					appSettings.Mac.JoinAcceptDelay2 = (uint)((int)data[index26] | (int)data[index26 + 1] << 8 | (int)data[index26 + 2] << 16 | (int)data[index26 + 3] << 24);
					int num42 = index26 + 4;
					Mac mac4 = appSettings.Mac;
					byte[] numArray12 = data;
					int num43 = num42;
					int num44 = num43 + 1;
					int index27 = num43;
					int num45 = (int)numArray12[index27] == 0 ? 0 : 1;
					mac4.DutyCycleOn = num45 != 0;
					Mac mac5 = appSettings.Mac;
					byte[] numArray13 = data;
					int num46 = num44;
					int num47 = num46 + 1;
					int index28 = num46;
					int num48 = (int)numArray13[index28] == 0 ? 0 : 1;
					mac5.PublicNetwork = num48 != 0;
					Mac mac6 = appSettings.Mac;
					byte[] numArray14 = data;
					int num49 = num47;
					int num50 = num49 + 1;
					int index29 = num49;
					int num51 = (int)numArray14[index29];
					mac6.DeviceClass = (DeviceClasses)num51;
					Mac mac7 = appSettings.Mac;
					byte[] numArray15 = data;
					int num52 = num50;
					int num53 = num52 + 1;
					int index30 = num52;
					int num54 = (int)numArray15[index30];
					mac7.DevicePhy = (DevicePhys)num54;
					AppSettings appSettings2 = appSettings;
					byte[] numArray16 = data;
					int num55 = num53;
					int num56 = num55 + 1;
					int index31 = num55;
					int num57 = (int)numArray16[index31];
					appSettings2.AppMode = (AppModes)num57;
					int index32 = num56 + 3;
					int num58;
					switch (appSettings.AppMode)
					{
						case AppModes.APP_GPS_TRACKING_DEMO:
							appSettings.Params.GpsTrackingDemo.TxDutyCycle = (uint)((int)data[index32] | (int)data[index32 + 1] << 8 | (int)data[index32 + 2] << 16 | (int)data[index32 + 3] << 24);
							int index33 = index32 + 4;
							appSettings.Params.GpsTrackingDemo.TxDutyCycleRnd = (uint)((int)data[index33] | (int)data[index33 + 1] << 8 | (int)data[index33 + 2] << 16 | (int)data[index33 + 3] << 24);
							int num59 = index33 + 4;
							AppGpsTrackingDemoParam trackingDemoParam1 = appSettings.Params.GpsTrackingDemo;
							byte[] numArray17 = data;
							int num60 = num59;
							int num61 = num60 + 1;
							int index34 = num60;
							int num62 = (int)numArray17[index34] == 0 ? 0 : 1;
							trackingDemoParam1.AdrOn = num62 != 0;
							AppGpsTrackingDemoParam trackingDemoParam2 = appSettings.Params.GpsTrackingDemo;
							byte[] numArray18 = data;
							int num63 = num61;
							num58 = num63 + 1;
							int index35 = num63;
							int num64 = (int)numArray18[index35] == 0 ? 0 : 1;
							trackingDemoParam2.DownlinkTestOn = num64 != 0;
							break;
						case AppModes.APP_RADIO_COVERAGE_TESTER:
							AppRadioCoverageTesterParam coverageTesterParam1 = appSettings.Params.RadioCoverageTester;
							byte[] numArray19 = data;
							int num65 = index32;
							int num66 = num65 + 1;
							int index36 = num65;
							int num67 = (int)numArray19[index36];
							coverageTesterParam1.Id = (byte)num67;
							AppRadioCoverageTesterParam coverageTesterParam2 = appSettings.Params.RadioCoverageTester;
							byte[] numArray20 = data;
							int num68 = num66;
							int num69 = num68 + 1;
							int index37 = num68;
							int num70 = (int)numArray20[index37];
							coverageTesterParam2.NbPackets = (byte)num70;
							AppRadioCoverageTesterParam coverageTesterParam3 = appSettings.Params.RadioCoverageTester;
							byte[] numArray21 = data;
							int num71 = num69;
							int num72 = num71 + 1;
							int index38 = num71;
							int num73 = (int)numArray21[index38] == 0 ? 0 : 1;
							coverageTesterParam3.DownlinkTestOn = num73 != 0;
							num58 = num72 + 1 + 7;
							break;
						default:
							appSettings.Params.SensorsGpsDemo.TxDutyCycle = (uint)((int)data[index32] | (int)data[index32 + 1] << 8 | (int)data[index32 + 2] << 16 | (int)data[index32 + 3] << 24);
							int index39 = index32 + 4;
							appSettings.Params.SensorsGpsDemo.TxDutyCycleRnd = (uint)((int)data[index39] | (int)data[index39 + 1] << 8 | (int)data[index39 + 2] << 16 | (int)data[index39 + 3] << 24);
							int num74 = index39 + 4;
							AppSensorsGpsDemoParam sensorsGpsDemoParam1 = appSettings.Params.SensorsGpsDemo;
							byte[] numArray22 = data;
							int num75 = num74;
							int num76 = num75 + 1;
							int index40 = num75;
							int num77 = (int)numArray22[index40] == 0 ? 0 : 1;
							sensorsGpsDemoParam1.AdrOn = num77 != 0;
							AppSensorsGpsDemoParam sensorsGpsDemoParam2 = appSettings.Params.SensorsGpsDemo;
							byte[] numArray23 = data;
							int num78 = num76;
							num58 = num78 + 1;
							int index41 = num78;
							int num79 = (int)numArray23[index41] == 0 ? 0 : 1;
							sensorsGpsDemoParam2.DownlinkTestOn = num79 != 0;
							break;
					}
					SettingsUiUpdate();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		private void configViews_SettingsUiUpdated(object sender, EventArgs e)
		{
			tBoxDevEui.Text = Regex.Replace(Utilities.Ba2Str(appSettings.DevEui, 0, appSettings.DevEui.Length), ".{2}(?!$)", "$0-");
		}

		private void activationView_SettingsUiUpdated(object sender, EventArgs e)
		{
		}

		private void macLayerView_SettingsUiUpdated(object sender, EventArgs e)
		{
		}

		private void channelsView_SettingsUiUpdated(object sender, EventArgs e)
		{
		}

		private void applicationView_SettingsUiUpdated(object sender, EventArgs e)
		{
			macLayerView.SettingsUiUpdate();
		}

		private void SettingsUiUpdate()
		{
			tBoxDevEui.Text = Regex.Replace(Utilities.Ba2Str(appSettings.DevEui, 0, appSettings.DevEui.Length), ".{2}(?!$)", "$0-");
			activationView.SettingsUiUpdate();
			macLayerView.SettingsUiUpdate();
			channelsView.SettingsUiUpdate();
			applicationView.SettingsUiUpdate();
		}

		private void Log(byte[] data)
		{
			Log(data, 0);
		}

		private void Log(byte[] data, int offset)
		{
			if (data == null)
				return;
			Console.WriteLine();
			int num = offset;
			for (int index = 0; index < data.Length; ++index)
			{
				Console.Write(data[index].ToString("X02") + " ");
				if ((num + 1) % 16 == 0)
					Console.WriteLine();
				++num;
			}
			Console.WriteLine();
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
			if (!textBox.IsReadOnly && (textBox == tBoxDevEui && !IsInvalidElements.Contains(sender)))
				appSettings.DevEui = Utilities.HexStr2Ba(textBox.Text);
			textBox.Text = Regex.Replace((textBox.Text ?? "").ToString(), ".{2}(?!$)", "$0-");
		}

		public void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		private void btnWrite_Click(object sender, RoutedEventArgs e)
		{
			byte[] data = SettingsUpdateTo();
			if (data != null)
			{
				Log(data);
				int num = (int)ctrlProtocol.WriteSettings(data);
			}
			else
			{
				int num1 = (int)MessageBox.Show("Unable to write EEPROM.", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
		}

		private void btnRead_Click(object sender, RoutedEventArgs e)
		{
			byte[] data = ctrlProtocol.ReadSettings();
			if (data != null)
			{
				Log(data);
				SettingsUpdateFrom(data);
			}
			else
			{
				int num = (int)MessageBox.Show("Unable to read EEPROM.", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
		}

		private void btnSetEepromDefault_Click(object sender, RoutedEventArgs e)
		{
			int num1 = (int)ctrlProtocol.SetEepromDefaults();
			Thread.Sleep(500);
			byte[] data = ctrlProtocol.ReadSettings();
			if (data != null)
			{
				SettingsUpdateFrom(data);
			}
			else
			{
				int num2 = (int)MessageBox.Show("Unable to set EEPROM defaults.", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
		}

		private void btnStartTestDevice_Click(object sender, RoutedEventArgs e)
		{
			int num = (int)ctrlProtocol.Reset();
			serialPortView.Close();
		}

	}
}
