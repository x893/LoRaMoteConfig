using System.ComponentModel;

namespace LoRaMoteConfig.Data
{
	public class Params : INotifyPropertyChanged
	{
		public AppGpsTrackingDemoParam GpsTrackingDemo = new AppGpsTrackingDemoParam();
		public AppRadioCoverageTesterParam RadioCoverageTester = new AppRadioCoverageTesterParam();
		public AppSensorsGpsDemoParam SensorsGpsDemo = new AppSensorsGpsDemoParam();

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}
