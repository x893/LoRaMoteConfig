using LoRaMoteConfig;
using LoRaMoteConfig.Data;
using LoRaMoteConfig.Enums;
using LoRaMoteConfig.Properties;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

namespace LoRaMoteConfig.Controls
{
	public partial class ChannelsView : UserControl, IComponentConnector, IStyleConnector
	{
		private AppSettings appSettings = new AppSettings();

		public ObservableCollection<ChannelParams> ChannelsList { get; set; }

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
				ChannelsList.CollectionChanged += new NotifyCollectionChangedEventHandler(ChannelsList_CollectionChanged);
				lvChannels.ItemsSource = (IEnumerable)ChannelsList;
			}
		}

		public event EventHandler SettingsUiUpdated;

		public ChannelsView()
		{
			InitializeComponent();
		}

		private void OnSettingsUiUpdated()
		{
			if (SettingsUiUpdated != null)
				SettingsUiUpdated(this, EventArgs.Empty);
		}

		public void SettingsUiUpdate()
		{
			OnSettingsUiUpdated();
		}

		private void lvBtnEdit_Click(object sender, RoutedEventArgs e)
		{
			int id = ((sender as Button).CommandParameter as ChannelParams).Id;
			ChannelEditorWnd channelEditorWnd = new ChannelEditorWnd(appSettings.Mac.Channels[id]);
			channelEditorWnd.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			channelEditorWnd.Owner = Application.Current.MainWindow;
			bool? nullable = channelEditorWnd.ShowDialog();
			bool flag = true;
			if (nullable.GetValueOrDefault() == flag && nullable.HasValue)
			{
				ChannelsList[id].Frequency = appSettings.Mac.Channels[id].Frequency = channelEditorWnd.Channel.Frequency;
				ChannelsList[id].DatarateRange = appSettings.Mac.Channels[id].DatarateRange = channelEditorWnd.Channel.DatarateRange;
				ChannelsList[id].Band = appSettings.Mac.Channels[id].Band = channelEditorWnd.Channel.Band;
			}
			ResetColumnWidths();
		}

		private void lvBtnDisable_Click(object sender, RoutedEventArgs e)
		{
			int id = ((sender as Button).CommandParameter as ChannelParams).Id;
			if (id < 3)
			{
				if (MessageBox.Show("Do you really want to disable the default channel?", "LoRaMoteConfig", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
					ChannelsList[id] = appSettings.Mac.Channels[id] = new ChannelParams(id, 0U, Datarates.DR_0, Datarates.DR_0, Bands.BAND_G1_0);
			}
			else
				ChannelsList[id] = appSettings.Mac.Channels[id] = new ChannelParams(id, 0U, Datarates.DR_0, Datarates.DR_0, Bands.BAND_G1_0);
			ResetColumnWidths();
		}

		private void ResetColumnWidths()
		{
			if (gvChannels == null)
				return;
			foreach (GridViewColumn gridViewColumn in (Collection<GridViewColumn>)gvChannels.Columns)
			{
				if (double.IsNaN(gridViewColumn.Width))
					gridViewColumn.Width = gridViewColumn.ActualWidth;
				gridViewColumn.Width = double.NaN;
			}
		}

		private void ChannelsList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			ResetColumnWidths();
		}

		private void btnLoad_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				OpenFileDialog openFileDialog = new OpenFileDialog();
				openFileDialog.DefaultExt = ".json";
				openFileDialog.Filter = "JSON files (.json)|*.json";
				openFileDialog.InitialDirectory = Settings.Default.ChannelsFilePath;
				openFileDialog.FileName = Settings.Default.ChannelsFileName;
				bool? nullable = openFileDialog.ShowDialog();
				bool flag = true;
				if (nullable.GetValueOrDefault() != flag || !nullable.HasValue)
					return;
				ObservableCollection<ChannelParams> observableCollection;
				using (StreamReader streamReader = File.OpenText(openFileDialog.FileName))
					observableCollection = (ObservableCollection<ChannelParams>)new JsonSerializer().Deserialize((TextReader)streamReader, typeof(ObservableCollection<ChannelParams>));
				foreach (ChannelParams channelParams in (Collection<ChannelParams>)observableCollection)
				{
					ChannelsList[channelParams.Id].Frequency = appSettings.Mac.Channels[channelParams.Id].Frequency = channelParams.Frequency;
					ChannelsList[channelParams.Id].DatarateRange = appSettings.Mac.Channels[channelParams.Id].DatarateRange = channelParams.DatarateRange;
					ChannelsList[channelParams.Id].Band = appSettings.Mac.Channels[channelParams.Id].Band = channelParams.Band;
				}
				ResetColumnWidths();
			}
			catch (Exception ex)
			{
				Console.WriteLine("ChannelsView: {0}", (object)ex);
			}
		}

		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				SaveFileDialog saveFileDialog = new SaveFileDialog();
				saveFileDialog.DefaultExt = "*.json";
				saveFileDialog.Filter = "JSON files (*.json)|*.json";
				saveFileDialog.InitialDirectory = Settings.Default.ChannelsFilePath;
				saveFileDialog.FileName = Settings.Default.ChannelsFileName;
				if (File.Exists(saveFileDialog.FileName))
				{
					if (MessageBox.Show("Do you want to overwrite the current config file?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
					{
						saveFileDialog.OverwritePrompt = false;
						bool? nullable = saveFileDialog.ShowDialog();
						bool flag = true;
						if (nullable.GetValueOrDefault() != flag || !nullable.HasValue)
							return;
						string[] strArray = saveFileDialog.FileName.Split('\\');
						string str1 = strArray[strArray.Length - 1];
						string str2 = "";
						int index;
						for (index = 0; index < strArray.Length - 2; ++index)
							str2 = str2 + strArray[index] + "\\";
						string str3 = str2 + strArray[index];
						Settings.Default.ChannelsFilePath = saveFileDialog.InitialDirectory;
						Settings.Default.ChannelsFileName = saveFileDialog.FileName;
					}
				}
				else
				{
					bool? nullable = saveFileDialog.ShowDialog();
					bool flag = true;
					if (nullable.GetValueOrDefault() != flag || !nullable.HasValue)
						return;
					string[] strArray = saveFileDialog.FileName.Split('\\');
					string str1 = strArray[strArray.Length - 1];
					string str2 = "";
					int index;
					for (index = 0; index < strArray.Length - 2; ++index)
						str2 = str2 + strArray[index] + "\\";
					Settings.Default.ChannelsFilePath = str2 + strArray[index];
					Settings.Default.ChannelsFileName = saveFileDialog.FileName;
				}
				using (FileStream fileStream = File.Open(saveFileDialog.FileName, FileMode.OpenOrCreate))
				{
					using (StreamWriter streamWriter = new StreamWriter((Stream)fileStream))
					{
						using (JsonWriter jsonWriter = (JsonWriter)new JsonTextWriter((TextWriter)streamWriter))
						{
							jsonWriter.Formatting = Formatting.Indented;
							new JsonSerializer().Serialize(jsonWriter, (object)ChannelsList);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("ChannelsView: {0}", (object)ex);
			}
		}

		void IStyleConnector.Connect(int connectionId, object target)
		{
			switch (connectionId)
			{
				case 5:
					((ButtonBase)target).Click += new RoutedEventHandler(lvBtnEdit_Click);
					break;
				case 6:
					((ButtonBase)target).Click += new RoutedEventHandler(lvBtnDisable_Click);
					break;
			}
		}
	}
}
