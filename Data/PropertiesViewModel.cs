using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LoRaMoteConfig.Data
{
	public class PropertiesViewModel : INotifyPropertyChanged
	{
		private string _name;

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
				OnPropertyChanged("Name");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
