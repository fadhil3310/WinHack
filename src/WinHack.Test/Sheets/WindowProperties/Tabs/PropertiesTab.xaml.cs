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
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.Input;
using DependencyPropertyGenerator;
using Windows.Win32.Foundation;
using HackWindow = WinHack.Core.Windowing.HackWindow;
using System.Drawing;
using Rectangle = System.Drawing.Rectangle;

namespace WinHack.Test.Sheets.WindowProperties.Tabs
{

		/// <summary>
		/// Interaction logic for PropertiesTab.xaml
		/// </summary>
		[DependencyProperty<HackWindow>("SelectedWindow")]
		[DependencyProperty<HWND>("HWND")]
		[DependencyProperty<string>("Title")]
		[DependencyProperty<string>("ClassName")]
		[DependencyProperty<Rectangle>("WindowSize")]
		public partial class PropertiesTab : UserControl
		{
				//public Window SelectedWindow
				//{
				//		get { return (Window)GetValue(SelectedWindowProperty); }
				//		set { SetValue(SelectedWindowProperty, value); }
				//}

				//// Using a DependencyProperty as the backing store for SelectedWindow.  This enables animation, styling, binding, etc...
				//public static readonly DependencyProperty SelectedWindowProperty =
				//				DependencyProperty.Register("SelectedWindow", typeof(Window), typeof(PropertiesTab), new PropertyMetadata());

				//public string HexHWND { get => SelectedWindow?.HWND.ToString() ?? "Empty"; }

				//public RECT WindowSize { get; private set; }

				public PropertiesTab()
				{
						InitializeComponent();

						Loaded += (_, _) =>
						{
								GetData();
						};
				}

				[RelayCommand]
				public void GetData()
				{
						HWND = SelectedWindow!.Handle;
						Title = SelectedWindow!.Title;
						ClassName = SelectedWindow!.ClassName;
						WindowSize = SelectedWindow!.GetDimensions();
				}
		}
}
