using LoRaMoteConfig.Enums;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace LoRaMoteConfig
{
	public class ControlProtocol
	{
		public event EventHandler Opened;
		public event EventHandler Closed;
		public event SerialDataReceivedEventHandler DataReceived;

		private enum FrameCmds
		{
			CMD_STATUS = 0,
			CMD_WR_SETTINGS = 1,
			CMD_RD_SETTINGS = 2,
			CMD_LOG = 3,
			CMD_RESET = 4,
			CMD_SET_EEPROM_DEFAULT = 5,
			CMD_GET_EEPROM_VERSION = 6,
			CMD_GET_FIRMWARE_VERSION = 7,
			CMD_GET_DEVICE_PHY = 8,
			CMD_UNKNOWN = 255,
		}

		private object comThreadSync = new object();
		private ControlProtocol.FrameCmds expectedCmd = ControlProtocol.FrameCmds.CMD_UNKNOWN;
		private byte[] rxFrame = new byte[2048];
		private AutoResetEvent autoEvent = new AutoResetEvent(false);
		private Thread comThread;
		private SerialPortEx serialPort;
		private const int FRAME_HEADER_SIZE = 3;

		public SerialPortEx SerialPort
		{
			get { return serialPort; }
			set { serialPort = value; }
		}

		public bool IsOpen
		{
			get { return serialPort.IsOpen; }
		}

		private void OnOpened()
		{
			if (Opened != null)
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

		public bool Open()
		{
			try
			{
				if (IsOpen)
				{
					comThread = new Thread(new ThreadStart(ComThread));
					comThread.Start();
					OnOpened();
					return true;
				}
			}
			catch (Exception) { }
			return false;
		}

		public bool Close()
		{
			if (serialPort.IsOpen)
			{
				serialPort.Close();
				comThread.Join();
			}
			OnClosed();
			return true;
		}

		private void OnDataReceived(byte[] frame)
		{
			if (expectedCmd == (ControlProtocol.FrameCmds)frame[2])
			{
				Array.Resize<byte>(ref rxFrame, frame.Length);
				Array.Copy((Array)frame, (Array)rxFrame, frame.Length);
				autoEvent.Set();
			}
		}

		private byte[] Send(byte[] frame)
		{
			try
			{
				bool lockTaken = false;
				try
				{
					Monitor.Enter(comThreadSync, ref lockTaken);
					expectedCmd = (ControlProtocol.FrameCmds)frame[2];
				}
				finally
				{
					if (lockTaken)
						Monitor.Exit(comThreadSync);
				}
				byte[] response = new byte[0];
				serialPort.Write(frame, 0, frame.Length);
				bool flag = true;
				if (autoEvent.WaitOne(100))
				{
					flag = false;
					response = rxFrame;
				}

				if (flag)
					throw new Exception("Serial port " + serialPort.PortName + " timed out sending command " + expectedCmd.ToString());

				if (response.Length == 0 || response.Length != (((int)response[0] << 8) + (int)response[1]))
				{
					throw new Exception(string.Concat(
						"Error frame length (",
						response.Length.ToString(),
						", expected ",
						(((int)response[0] << 8) + (int)response[1]).ToString(),
						")"
						));
				}
				if (expectedCmd != (ControlProtocol.FrameCmds)response[2])
					throw new Exception("Wrong command received (" + ((ControlProtocol.FrameCmds)response[2]).ToString() + ", expected " + expectedCmd.ToString() + ")");
			}
			catch (Exception ex)
			{
				Console.WriteLine((object)ex);
				return null;
			}
			return rxFrame;
		}

		public byte GetStatus()
		{
			byte[] response = Send(new byte[3] { 0, FRAME_HEADER_SIZE, (byte)FrameCmds.CMD_STATUS });
			if (response != null)
				return response[3];
			return byte.MaxValue;
		}

		public byte WriteSettings(byte[] data)
		{
			int num = 3 + data.Length;
			byte[] response = Send(new byte[3] {
				(byte) (num >> 8),
				(byte) num,
				(byte) FrameCmds.CMD_WR_SETTINGS
				});
			if (response != null)
				return response[3];
			return byte.MaxValue;
		}

		public byte[] ReadSettings()
		{
			byte[] response = Send(new byte[3] { 0, FRAME_HEADER_SIZE, (byte)FrameCmds.CMD_RD_SETTINGS });
			if (response != null)
				return Enumerable.ToArray<byte>(Enumerable.Skip<byte>((IEnumerable<byte>)response, 3));
			return (byte[])null;
		}

		public byte SetEepromDefaults()
		{
			byte[] response = Send(new byte[3] { 0, FRAME_HEADER_SIZE, (byte)FrameCmds.CMD_SET_EEPROM_DEFAULT });
			if (response != null)
				return response[3];
			return byte.MaxValue;
		}

		public byte Reset()
		{
			byte[] response = Send(new byte[3] { 0, FRAME_HEADER_SIZE, (byte)FrameCmds.CMD_RESET });
			if (response != null)
				return response[3];
			return byte.MaxValue;
		}

		public Version GetFirmwareVersion()
		{
			byte[] response = Send(new byte[3] { 0, FRAME_HEADER_SIZE, (byte)FrameCmds.CMD_GET_FIRMWARE_VERSION });
			if (response != null)
				return new Version((int)response[3], (int)response[4]);
			return new Version(0, 0);
		}

		public Version GetEepromVersion()
		{
			byte[] response = Send(new byte[3] { 0, FRAME_HEADER_SIZE, (byte)FrameCmds.CMD_GET_EEPROM_VERSION });
			if (response != null)
				return new Version((int)response[3], (int)response[4]);
			return new Version(0, 0);
		}

		public DevicePhys GetDevicePhy()
		{
			byte[] response = Send(new byte[3] { 0, FRAME_HEADER_SIZE, (byte)FrameCmds.CMD_GET_DEVICE_PHY });
			if (response != null)
				return (DevicePhys)response[3];
			return DevicePhys.UNKNOWN;
		}

		private void ComThread()
		{
			try
			{
				byte state = 0;
				byte[] frame = new byte[0];
				int length = 0;
				int idxFrame = 0;

				while (serialPort.IsOpen)
				{
					int bytesToRead = serialPort.BytesToRead;
					if (bytesToRead == 0)
					{
						Thread.Sleep(0);
					}
					else
					{
						byte[] buffer = new byte[bytesToRead];
						serialPort.Read(buffer, 0, bytesToRead);
						int idxBuffer = 0;
						while (idxBuffer < bytesToRead)
						{
							switch (state)
							{
								case 0:
									length = (int)buffer[idxBuffer] << 8;
									idxBuffer++;
									state = 1;
									break;
								case 1:
									length |= (int)buffer[idxBuffer];
									idxBuffer++;
									frame = new byte[length];
									frame[0] = (byte)(length >> 8);
									idxFrame = 2;
									frame[1] = (byte)(length & (int)byte.MaxValue);
									state = 2;
									break;
								case 2:
									frame[idxFrame] = buffer[idxBuffer];
									idxFrame++;
									idxBuffer++;
									if (idxFrame >= length)
									{
										bool lockTaken = false;
										try
										{
											Monitor.Enter(comThreadSync, ref lockTaken);
											OnDataReceived(frame);
											if (idxFrame == frame.Length)
												state = 0;
											else
												frame = null;
											idxFrame = 0;
											break;
										}
										finally
										{
											if (lockTaken)
												Monitor.Exit(comThreadSync);
										}
									}
									else
										break;
							}
						}
						Thread.Sleep(1);
					}
				}
			}
			catch (Exception) { }
		}

		public void Dispose()
		{
			if (!IsOpen)
				return;
			Close();
		}
	}
}