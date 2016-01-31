using LoRaMoteConfig.Properties;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;

namespace LoRaMoteConfig
{
	public partial class App : Application
	{
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			int result = 0;
			for (int idx = 0; idx < e.Args.Length; ++idx)
			{
				if (e.Args[idx] == "/devclass")
				{
					idx = idx + 1;
					if (idx < e.Args.Length)
					{
						int.TryParse(e.Args[idx], out result);
						Settings.Default.DevClassVisibility = Convert.ToBoolean(result);
					}
				}
			}
			this.StartupUri = new Uri("MainWnd.xaml", UriKind.Relative);
		}

		private void Application_Exit(object sender, ExitEventArgs e)
		{
			Settings.Default.Save();
		}

		[STAThread]
		public static void Main()
		{
			App app = new App();
			app.InitializeComponent();
			app.Run();
		}
	}
}