using System;
using System.Collections.Generic;
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
using DependencyPropertyGenerator;
using HackWindow = WinHack.Core.Windowing.HackWindow;

namespace WinHack.Test.Sheets.WindowProperties
{
		/// <summary>
		/// Interaction logic for WindowProperties.xaml
		/// </summary>
		[DependencyProperty<HackWindow>("SelectedWindow")]
		public partial class WindowProperties : Window
		{
				public WindowProperties(HackWindow window)
				{
						SelectedWindow = window;
						Debug.WriteLine($"Abc {SelectedWindow.Handle.ToString()}");

						InitializeComponent();
				}
		}
}
