using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Management;

namespace LoRaMoteConfig
{
	public class SerialPortEx : SerialPort
	{
		public SerialPortEx()
		{
		}

		public SerialPortEx(IContainer iC)
			: base(iC)
		{
		}

		public static List<string> GetPortNamesEx()
		{
			try
			{
				List<string> list = new List<string>();
				foreach (ManagementObject managementObject in new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity").Get())
				{
					if (managementObject["Caption"] != null && managementObject["Caption"].ToString().Contains("(COM"))
						list.Add(managementObject["Caption"].ToString());
				}
				list.Sort();
				return list;
			}
			catch (ManagementException ex)
			{
				Console.WriteLine("Serial port list: {0}", (object)ex.ToString());
				return new List<string>();
			}
		}

		public new void Open()
		{
			try
			{
				base.Open();
				GC.SuppressFinalize((object)this.BaseStream);
			}
			catch
			{
				throw;
			}
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && this.Container != null)
					this.Container.Dispose();
				GC.ReRegisterForFinalize((object)this.BaseStream);
			}
			catch { }

			try
			{
				base.Dispose(disposing);
			}
			catch { }
		}
	}
}