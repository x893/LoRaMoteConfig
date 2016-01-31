using LoRaMoteConfig;
using LoRaMoteConfig.Properties;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

namespace LoRaMoteConfig.Controls
{
	public partial class SerialPortView : UserControl, IDisposable, IComponentConnector
	{

		public event EventHandler Opened;
		public event EventHandler Closed;
		public event SerialDataReceivedEventHandler DataReceived;
		public event PropertyChangedEventHandler PropertyChanged;

		private SerialPortEx serialPort;

		public SerialPortEx SerialPort
		{
			get
			{
				return serialPort;
			}
			private set
			{
				serialPort = value;
			}
		}

		public bool IsOpen
		{
			get
			{
				return serialPort.IsOpen;
			}
		}

		public SerialPortView()
		{
			InitializeComponent();
			try
			{
				serialPort = new SerialPortEx();
				serialPort.BaudRate = 115200;
				serialPort.ReadTimeout = 2000;
				serialPort.WriteTimeout = 1000;
				serialPort.ReadBufferSize = 8192;
				serialPort.DtrEnable = true;
				serialPort.Disposed += new EventHandler(serialPort_Disposed);
				serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
				cBoxSerialPort.Items.Clear();
				foreach (object newItem in SerialPortEx.GetPortNamesEx())
					cBoxSerialPort.Items.Add(newItem);
				cBoxSerialPort.SelectionChanged += new SelectionChangedEventHandler(cBoxSerialPort_SelectionChanged);
				if (cBoxSerialPort.Items.Count <= 0)
					SerialToggle.IsEnabled = false;
				cBoxSerialPort.SelectedIndex = Convert.ToInt32(Settings.Default.SerialPortIndex);
			}
			catch (Exception ex)
			{
				Console.WriteLine("serial port : {0}", (object)ex);
			}
		}

		private void OnOpened()
		{
			if (Opened == null)
				return;
			Opened(this, EventArgs.Empty);
		}

		private void OnClosed()
		{
			if (Closed != null)
				Closed(this, EventArgs.Empty);
		}

		private void OnDataReceived(SerialDataReceivedEventArgs e)
		{
			if (DataReceived != null)
				DataReceived(this, e);
		}

		private void serialPort_Disposed(object sender, EventArgs e)
		{
			Close();
		}

		private bool Open(string name)
		{
			try
			{
				serialPort.PortName = name;
				if (serialPort.IsOpen)
					serialPort.Close();
				serialPort.Open();
				if (IsOpen)
				{
					cBoxSerialPort.IsEnabled = false;
					SerialToggle.IsChecked = true;
					OnOpened();
					return true;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("serial port : {0}", (object)ex);
				if (IsOpen)
				{
					serialPort.Close();
					cBoxSerialPort.IsEnabled = true;
					SerialToggle.IsChecked = new bool?(false);
				}
			}
			return false;
		}

		public bool Close()
		{
			if (serialPort.IsOpen)
				serialPort.Close();
			cBoxSerialPort.IsEnabled = true;
			SerialToggle.IsChecked = new bool?(false);
			OnClosed();
			return true;
		}

		public void Send(byte[] buffer)
		{
			try
			{
				serialPort.Write(buffer, 0, buffer.Length);
			}
			catch (Exception ex)
			{
				Console.WriteLine("serial port : {0}", (object)ex);
			}
		}

		private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			OnDataReceived(e);
		}

		public void Dispose()
		{
			if (!IsOpen)
				return;
			Close();
		}

		private void OnPropertyChanged(string propName)
		{
			if (PropertyChanged == null)
				return;
			PropertyChanged(this, new PropertyChangedEventArgs(propName));
		}

		private void cBoxSerialPort_DropDownOpened(object sender, EventArgs e)
		{
			object selectedItem = cBoxSerialPort.SelectedItem;
			int selectedIndex = cBoxSerialPort.SelectedIndex;
			cBoxSerialPort.SelectionChanged -= new SelectionChangedEventHandler(cBoxSerialPort_SelectionChanged);
			cBoxSerialPort.Items.Clear();

			foreach (object newItem in SerialPortEx.GetPortNamesEx())
				cBoxSerialPort.Items.Add(newItem);

			cBoxSerialPort.SelectionChanged += new SelectionChangedEventHandler(cBoxSerialPort_SelectionChanged);
			cBoxSerialPort.SelectedItem = selectedItem;
			if (cBoxSerialPort.Items.Count <= 0)
				SerialToggle.IsEnabled = false;
			else
				SerialToggle.IsEnabled = true;
		}

		private void SerialToggle_Click(object sender, RoutedEventArgs e)
		{
			bool? isChecked = ((ToggleButton)sender).IsChecked;
			if (isChecked.HasValue && isChecked.GetValueOrDefault())
			{
				try
				{
					if (!Open(new Regex("COM[0-9]+").Match(cBoxSerialPort.SelectedValue.ToString()).Value))
					{
						MessageBox.Show("Please verify if the selected COM port is correct.", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
						throw new Exception("Serial Port: Unable to open com port");
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("serial port : {0}", ex);
					Close();
				}
			}
			else
				Close();
		}

		private void cBoxSerialPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Settings.Default.SerialPortIndex = cBoxSerialPort.SelectedIndex;
		}
	}
}
