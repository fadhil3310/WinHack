using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WinHack.Core.Displays;
using WinHack.Test.Demos;
using WinHack.WindowHook;

namespace WinHack.Test
{
		/// <summary>
		/// Interaction logic for MainWindow.xaml
		/// </summary>
		public partial class MainWindow : Window
		{
				//public CollectionViewSource windowListView = new();


				public MainWindow()
				{
						InitializeComponent();

						WindowHookOptions.HookPipeName = "WinHackTest";
						WindowHookOptions.Surrogate32PipeName = "WinHackTestSurrogate";
				}

				private void windowingButton_Click(object sender, RoutedEventArgs e)
				{
						var window = new WindowingDemo();
						window.Show();
				}

				private void monitorButton_Click(object sender, RoutedEventArgs e)
				{
						var window = new MonitorDemo();
						window.Show();
				}
  }
}