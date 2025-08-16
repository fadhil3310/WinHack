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
using System.Windows.Shapes;
using DependencyPropertyGenerator;

using HackMonitor = WinHack.Core.Displays.HackMonitor;

namespace WinHack.Test.Sheets.MonitorProperties
{
		/// <summary>
		/// Interaction logic for MonitorProperties.xaml
		/// </summary>
		[DependencyProperty<HackMonitor>("SelectedMonitor")]
		public partial class MonitorProperties : Window
		{
				public MonitorProperties(HackMonitor monitor)
				{
						SelectedMonitor = monitor;
						InitializeComponent();
				}
		}
}
