using LoRaMoteConfig.Properties;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Navigation;
using System.Xml;

namespace LoRaMoteConfig
{
	public partial class SettingsView : UserControl, INotifyPropertyChanged, IComponentConnector
	{
		private string firmwareVersion = "Not connected";
		private XmlDocument xmlDoc = (XmlDocument)null;
		private const string propertyNameTitle = "Title";
		private const string propertyNameDescription = "Description";
		private const string propertyNameProduct = "Product";
		private const string propertyNameCopyright = "Copyright";
		private const string propertyNameCompany = "Company";
		private const string xPathRoot = "ApplicationInfo/";
		private const string xPathTitle = "ApplicationInfo/Title";
		private const string xPathVersion = "ApplicationInfo/Version";
		private const string xPathDescription = "ApplicationInfo/Description";
		private const string xPathProduct = "ApplicationInfo/Product";
		private const string xPathCopyright = "ApplicationInfo/Copyright";
		private const string xPathCompany = "ApplicationInfo/Company";
		private const string xPathLink = "ApplicationInfo/Link";
		private const string xPathLinkUri = "ApplicationInfo/Link/@Uri";
		private const string xPathUsersGuide = "ApplicationInfo/UsersGuide";
		private const string xPathUsersGuideUri = "ApplicationInfo/UsersGuide/@Uri";

		public string FirmwareVersion
		{
			get
			{
				return firmwareVersion;
			}
			set
			{
				if (!(firmwareVersion != value))
					return;
				firmwareVersion = value;
				OnPropertyChanged("FirmwareVersion");
			}
		}

		public string ProductTitle
		{
			get
			{
				string str = CalculatePropertyValue<AssemblyTitleAttribute>("Title", "ApplicationInfo/Title");
				if (string.IsNullOrEmpty(str))
					str = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
				return str;
			}
		}

		public string Version
		{
			get
			{
				string str = string.Empty;
				System.Version version = Assembly.GetExecutingAssembly().GetName().Version;
				return !(version != (System.Version)null) ? GetLogicalResourceString("ApplicationInfo/Version") : string.Format("{0}.{1}", (object)version.Major.ToString(), (object)version.Minor.ToString());
			}
		}

		public string DeviceConfigVersion
		{
			get
			{
				return Settings.Default.DevConfigVersion;
			}
		}

		public string Description
		{
			get
			{
				return CalculatePropertyValue<AssemblyDescriptionAttribute>("Description", "ApplicationInfo/Description");
			}
		}

		public string Product
		{
			get
			{
				return CalculatePropertyValue<AssemblyProductAttribute>("Product", "ApplicationInfo/Product");
			}
		}

		public string Copyright
		{
			get
			{
				return CalculatePropertyValue<AssemblyCopyrightAttribute>("Copyright", "ApplicationInfo/Copyright");
			}
		}

		public string Company
		{
			get
			{
				return CalculatePropertyValue<AssemblyCompanyAttribute>("Company", "ApplicationInfo/Company");
			}
		}

		public string LinkText
		{
			get
			{
				return GetLogicalResourceString("ApplicationInfo/Link");
			}
		}

		public string LinkUri
		{
			get
			{
				return GetLogicalResourceString("ApplicationInfo/Link/@Uri");
			}
		}

		public string UsersGuideText
		{
			get
			{
				return GetLogicalResourceString("ApplicationInfo/UsersGuide");
			}
		}

		public string UsersGuideUri
		{
			get
			{
				return GetLogicalResourceString("ApplicationInfo/UsersGuide/@Uri");
			}
		}

		protected virtual XmlDocument ResourceXmlDocument
		{
			get
			{
				if (xmlDoc == null)
				{
					XmlDataProvider xmlDataProvider = TryFindResource((object)"aboutProvider") as XmlDataProvider;
					if (xmlDataProvider != null)
						xmlDoc = xmlDataProvider.Document;
				}
				return xmlDoc;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public SettingsView()
		{
			InitializeComponent();
			FirmwareVersion = "Not connected";
		}

		private void hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			if (!(e.Uri != (Uri)null) || string.IsNullOrEmpty(e.Uri.OriginalString))
				return;
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}

		private string CalculatePropertyValue<T>(string propertyName, string xpathQuery)
		{
			string str = string.Empty;
			object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(T), false);
			if ((uint)customAttributes.Length > 0U)
			{
				PropertyInfo property = ((T)customAttributes[0]).GetType().GetProperty(propertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
				if (property != (PropertyInfo)null)
					str = property.GetValue(customAttributes[0], (object[])null) as string;
			}
			if (str == string.Empty)
				str = GetLogicalResourceString(xpathQuery);
			return str;
		}

		protected virtual string GetLogicalResourceString(string xpathQuery)
		{
			string str = string.Empty;
			XmlDocument resourceXmlDocument = ResourceXmlDocument;
			if (resourceXmlDocument != null)
			{
				XmlNode xmlNode = resourceXmlDocument.SelectSingleNode(xpathQuery);
				if (xmlNode != null)
					str = !(xmlNode is XmlAttribute) ? xmlNode.InnerText : xmlNode.Value;
			}
			return str;
		}

		public void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}
