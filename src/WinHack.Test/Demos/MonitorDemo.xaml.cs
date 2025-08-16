using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WinHack.Core.Displays;
using WinHack.Test.Sheets.MonitorProperties;
using DependencyPropertyGenerator;

using HackMonitor = WinHack.Core.Displays.HackMonitor;

namespace WinHack.Test.Demos
{
		/// <summary>
		/// Interaction logic for MonitorDemo.xaml
		/// </summary>
		[DependencyProperty<ObservableCollection<MonitorListModel>>("MonitorList")]		
		public partial class MonitorDemo : Window
		{
				public MonitorDemo()
				{
						EnumerateMonitors();
						InitializeComponent();
				}

				[RelayCommand]
				public void EnumerateMonitors()
				{
						var monitors = HackMonitorEnumerator.GetAllDisplays();
						MonitorList = [.. monitors.Select(x => new MonitorListModel(this, x))];
				}

				public partial class MonitorListModel
				{
						private MonitorDemo parent;

						public HackMonitor Monitor { get; private set; }

						public int Width { get; private set; }
						public int Height { get; private set; }

						public MonitorListModel(MonitorDemo parent, HackMonitor monitor)
						{
								this.parent = parent;
								Monitor = monitor;

								var resolution = monitor.GetResolution();
								Width = resolution.Width;
								Height = resolution.Height;
						}

						[RelayCommand]
						public void OpenProperties()
						{
								var window = new MonitorProperties(Monitor);
								window.Show();
						}
				}
		}
}
