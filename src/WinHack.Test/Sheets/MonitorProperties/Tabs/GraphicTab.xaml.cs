using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using DependencyPropertyGenerator;

using WinHack.Core.Graphics.DeviceContexts;
using WinHack.Core.Graphics.Drawer;
using WinHack.Core.Displays;

namespace WinHack.Test.Sheets.MonitorProperties.Tabs
{
		/// <summary>
		/// Interaction logic for GraphicTab.xaml
		/// </summary>
		[DependencyProperty<HackMonitor>("SelectedMonitor")]
		public partial class GraphicTab : UserControl
		{
				public GraphicTab()
				{
						InitializeComponent();
				}


				[RelayCommand]
				public void AnimMotionBlur()
				{
						//using var displayDC = new DisplayDeviceContext(SelectedMonitor!);
						//using var drawer = new DrawerLocalTiming(displayDC);
				}
		}
}
