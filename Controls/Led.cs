using LoRaMoteConfig.General;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LoRaMoteConfig.Controls
{
	public class Led : Control
	{
		public static readonly DependencyProperty CheckedProperty = DependencyProperty.Register("Checked", typeof(bool), typeof(Led), new PropertyMetadata((object)false, new PropertyChangedCallback(Led.OnCheckedPropertyChanged)));
		public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Color), typeof(Led), new PropertyMetadata((object)Colors.Green, new PropertyChangedCallback(Led.OnColorPropertyChanged)));
		public static readonly DependencyProperty ToolTipToDisplayProperty = DependencyProperty.Register("ToolTipToDisplay", typeof(object), typeof(Led), new PropertyMetadata((PropertyChangedCallback)null));
		public static readonly RoutedEvent CheckedChangedEvent = EventManager.RegisterRoutedEvent("CheckChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<string>), typeof(Led));
		public static readonly RoutedEvent ColorChangedEvent = EventManager.RegisterRoutedEvent("ColorChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<string>), typeof(Led));

		public bool Checked
		{
			get
			{
				return (bool)GetValue(Led.CheckedProperty);
			}
			set
			{
				SetValue(Led.CheckedProperty, value);
			}
		}

		public Color Color
		{
			get
			{
				return (Color)GetValue(Led.ColorProperty);
			}
			set
			{
				SetValue(Led.ColorProperty, (object)value);
			}
		}

		public object ToolTipToDisplay
		{
			get
			{
				return (object)(string)GetValue(Led.ToolTipToDisplayProperty);
			}
			set
			{
				SetValue(Led.ToolTipToDisplayProperty, value);
			}
		}

		public event RoutedPropertyChangedEventHandler<string> CheckedChanged
		{
			add
			{
				AddHandler(Led.CheckedChangedEvent, (Delegate)value);
			}
			remove
			{
				RemoveHandler(Led.CheckedChangedEvent, (Delegate)value);
			}
		}

		public event RoutedPropertyChangedEventHandler<string> ColorChanged
		{
			add
			{
				AddHandler(Led.ColorChangedEvent, (Delegate)value);
			}
			remove
			{
				RemoveHandler(Led.ColorChangedEvent, (Delegate)value);
			}
		}

		static Led()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(Led), (PropertyMetadata)new FrameworkPropertyMetadata((object)typeof(Led)));
		}

		protected virtual void OnCheckedChanged(string oldValue, string newValue)
		{
			RoutedPropertyChangedEventArgs<string> changedEventArgs = new RoutedPropertyChangedEventArgs<string>(oldValue, newValue);
			changedEventArgs.RoutedEvent = Led.CheckedChangedEvent;
			RaiseEvent((RoutedEventArgs)changedEventArgs);
		}

		private static void OnCheckedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			bool flag1 = (bool)e.OldValue;
			bool flag2 = (bool)e.NewValue;
			Led led = d as Led;
			if (led == null)
				return;
			led.SetValue(Led.CheckedProperty, flag2);
			led.InvalidateVisual();
			led.OnCheckedChanged(flag1.ToString(), flag2.ToString());
		}

		protected virtual void OnColorChanged(string oldValue, string newValue)
		{
			RoutedPropertyChangedEventArgs<string> changedEventArgs = new RoutedPropertyChangedEventArgs<string>(oldValue, newValue);
			changedEventArgs.RoutedEvent = Led.ColorChangedEvent;
			RaiseEvent((RoutedEventArgs)changedEventArgs);
		}

		private static void OnColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Color color1 = (Color)e.OldValue;
			Color color2 = (Color)e.NewValue;
			Led led = d as Led;
			if (led == null)
				return;
			led.SetValue(Led.ColorProperty, (object)color2);
			led.InvalidateVisual();
			led.OnColorChanged(color1.ToString(), color2.ToString());
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);
			double angle = 50.0 - 15.0 * (1.0 - Width / Height);
			Rect rect = new Rect(RenderSize);
			if (IsEnabled)
			{
				SolidColorBrush solidColorBrush = !Checked ? new SolidColorBrush(LightenColorExtension.Lighten(Color, 0.5f)) : new SolidColorBrush(LightenColorExtension.Lighten(Color, 1.25f));
				drawingContext.DrawEllipse((Brush)solidColorBrush, new Pen((Brush)solidColorBrush, 1.0), new Point(rect.Width / 2.0, rect.Height / 2.0), rect.Width / 2.0, rect.Height / 2.0);
			}
			LinearGradientBrush linearGradientBrush1 = new LinearGradientBrush(Color.FromArgb((byte)150, byte.MaxValue, byte.MaxValue, byte.MaxValue), Colors.Transparent, angle);
			LinearGradientBrush linearGradientBrush2 = new LinearGradientBrush(Color.FromArgb((byte)100, byte.MaxValue, byte.MaxValue, byte.MaxValue), Color.FromArgb((byte)100, byte.MaxValue, byte.MaxValue, byte.MaxValue), angle);
			drawingContext.DrawEllipse((Brush)linearGradientBrush2, new Pen((Brush)linearGradientBrush2, 1.0), new Point(rect.Width * 1.0 / 4.0, rect.Height * 1.0 / 4.0), rect.Width * 10.0 / 100.0, rect.Height * 10.0 / 100.0);
			drawingContext.DrawEllipse((Brush)linearGradientBrush1, new Pen((Brush)linearGradientBrush1, 1.0), new Point(rect.Width / 2.0, rect.Height / 2.0), rect.Width / 2.0, rect.Height / 2.0);
			ToolTip = ToolTipToDisplay;
		}

		[StructLayout(LayoutKind.Sequential, Size = 1)]
		private struct Angle
		{
			public const float Up = 50f;
			public const float Down = 230f;
		}
	}
}
