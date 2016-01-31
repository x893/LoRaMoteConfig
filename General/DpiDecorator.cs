using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LoRaMoteConfig.General
{
	public class DpiDecorator : Decorator
	{
		public DpiDecorator()
		{
			this.Loaded += (RoutedEventHandler)((s, e) =>
			{
				Matrix transformToDevice = PresentationSource.FromVisual((Visual)this).CompositionTarget.TransformToDevice;
				ScaleTransform scaleTransform = new ScaleTransform(1.0 / transformToDevice.M11, 1.0 / transformToDevice.M22);
				if (scaleTransform.CanFreeze)
					scaleTransform.Freeze();
				this.LayoutTransform = (Transform)scaleTransform;
			});
		}
	}
}
