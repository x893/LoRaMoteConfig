using LoRaMoteConfig.Data;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace LoRaMoteConfig.Controls
{
	public partial class AppRadioCoverageTesterView : UserControl, INotifyPropertyChanged, IComponentConnector
	{
		private byte id = (byte)0;
		private byte nbPackets = (byte)1;
		private bool downlinkTestOn = false;
		private List<object> IsInvalidElements = new List<object>();

		public bool HasErrors
		{
			get
			{
				return IsInvalidElements.Count != 0;
			}
		}

		public byte Id
		{
			get
			{
				return id;
			}
			set
			{
				if ((int)value != (int)id)
				{
					id = value;
					OnPropertyChanged("Id");
				}
				tBoxId.Text = id.ToString();
			}
		}

		public byte NbPackets
		{
			get
			{
				return nbPackets;
			}
			set
			{
				if ((int)value != (int)nbPackets)
				{
					nbPackets = value;
					OnPropertyChanged("NbPackets");
				}
				tBoxNbPackets.Text = nbPackets.ToString();
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

		public event PropertyChangedEventHandler PropertyChanged;

		public AppRadioCoverageTesterView()
		{
			InitializeComponent();
			tBoxId.DataContext = (object)new PropertiesViewModel();
			tBoxNbPackets.DataContext = (object)new PropertiesViewModel();
			tBoxId.Text = "0";
			tBoxNbPackets.Text = "1";
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
			if (textBox == tBoxId && !IsInvalidElements.Contains(sender))
			{
				id = Convert.ToByte(textBox.Text);
				OnPropertyChanged("Id");
			}
			else
			{
				if (textBox != tBoxNbPackets || IsInvalidElements.Contains(sender))
					return;
				nbPackets = Convert.ToByte(textBox.Text);
				OnPropertyChanged("NbPackets");
			}
		}

		public void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}