using LoRaMoteConfig.Controls;
using LoRaMoteConfig.Data;
using MahApps.Metro.Controls;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace LoRaMoteConfig
{
  public partial class ChannelEditorWnd : MetroWindow, IComponentConnector
  {
    private ChannelParams channel = new ChannelParams();

    public ChannelParams Channel
    {
      get
      {
        return this.channel;
      }
    }

    public ChannelEditorWnd()
    {
      this.InitializeComponent();
    }

    public ChannelEditorWnd(ChannelParams channel)
    {
      this.InitializeComponent();
      this.channel.Frequency = channel.Frequency;
      this.channel.DatarateRange = channel.DatarateRange;
      this.channel.Band = channel.Band;
      this.channelView.Channel = this.channel;
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
    }

    private void btnOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = new bool?(true);
    }
  }
}
