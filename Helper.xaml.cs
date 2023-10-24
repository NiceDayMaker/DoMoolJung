using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.IO;
using System.Windows;

namespace DoMoolJung
{
	public partial class Helper : Window
	{
		public Helper()
		{
			InitializeComponent();
			Dispatcher.Invoke(() => Init());
		}

		async void Init()
		{
			var udf = CoreWebView2Environment.CreateAsync(userDataFolder: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp")).Result;

			await webview.EnsureCoreWebView2Async(environment: udf);
			webview.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
			webview.Source = new Uri(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DoMoolJung", "Help", "index.html"));
		}
	}
}
