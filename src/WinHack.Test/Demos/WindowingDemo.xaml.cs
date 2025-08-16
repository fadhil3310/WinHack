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
using Windows.Win32.Foundation;
using WinHack.Core.Graphics;
using WinHack.Test.Sheets.WindowProperties;

using HackWindow = WinHack.Core.Windowing.HackWindow;
using Window = System.Windows.Window;
using WinHack.Core.Global;
using WinHack.Core.Graphics.Drawer;
using WinHack.Core.Windowing;
using WinHack.Core.Graphics.DeviceContexts;

namespace WinHack.Test
{
		/// <summary>
		/// Interaction logic for WindowingDemo.xaml
		/// </summary>
		public partial class WindowingDemo : Window
		{

				ObservableCollection<WindowListModel> windowList = [];

				public WindowingDemo()
				{
						InitializeComponent();

						WinHackSettings.Get().ThrowIfError = true;
						
						DataContext = this;
						testList.ItemsSource = windowList;
						EnumerateTopLevelWindows();

						Unloaded += (_, _) =>
						{
								foreach (var window in windowList)
								{
										window.Window.Dispose();
								}
								windowList.Clear();
						};
				}

				// ======================= EVENTS =======================
				private void Window_Loaded(object sender, RoutedEventArgs e)
				{
						CollectionViewSource.GetDefaultView(testList.ItemsSource).Filter = WindowListView_Filter;
				}
				private bool WindowListView_Filter(object item)
				{
						if (string.IsNullOrEmpty(testListSearch.Text))
						{
								return true;
						}

						WindowListModel? data = item as WindowListModel;
						if (data != null)
						{
								string? title = data.Window.Title?.ToLower();
								string? className = data.Window.Title?.ToLower();

								if ((title?.Contains(testListSearch.Text.ToLower()) ?? false) || (className?.Contains(testListSearch.Text.ToLower()) ?? false))
										return true;
								else
										return false;
						}

						return false;
				}
				private void testButton_Click(object sender, RoutedEventArgs e)
				{
						EnumerateTopLevelWindows();
				}

				private void testListSearch_TextChanged(object sender, TextChangedEventArgs e)
				{
						CollectionViewSource.GetDefaultView(testList.ItemsSource).Refresh();
				}
				// ======================= END EVENTS =======================

				public async void EnumerateTopLevelWindows()
				{
						var windows = await HackWindowEnumerator.GetTopLevelWindows();
						windowList.Clear();

						foreach (var window in windows)
						{
								var windowModel = new WindowListModel(this, window);
								windowList.Add(windowModel);
						}
				}

				public async Task<IEnumerable<WindowListModel>> EnumerateWindowChildren(HWND hwndParent, bool recursive = false)
				{
						var windows = await HackWindowEnumerator.GetWindowChildren(hwndParent, recursive);
						return windows.Select(x => new WindowListModel(this, x));
				}


				public partial class WindowListModel
				{
						private readonly WindowingDemo parent;

						public HackWindow Window { get; private set; }

						public ObservableCollection<WindowListModel> Children { get; set; } = [];

						public WindowListModel(WindowingDemo parent, HackWindow window)
						{
								this.parent = parent;
								Window = window;

								foreach (var childWindowData in window.Children)
								{
										var model = new WindowListModel(this.parent, childWindowData);
										Children.Add(model);
								}
						}

						[RelayCommand]
						public async Task QueryChildWindows() 
						{ 
								var windows = await parent.EnumerateWindowChildren(Window);
								foreach (var window in windows)
								{
										Children.Add(window);
								}
						}

						[RelayCommand]
						public async Task QueryChildWindowsRecursive()
						{
								var windows = await parent.EnumerateWindowChildren(Window, true);
								foreach (var window in windows)
								{
										Children.Add(window);
								}
						}

						[RelayCommand]
						public void DrawTest()
						{
								//using var windowDC = new WindowDeviceContext(Window);

								//using DrawerLocalTiming drawer = new(windowDC);
								//var rectangle = Rectangle
								//drawer.DrawRectangle(0, 0, 500, 500, System.Drawing.Color.CornflowerBlue);
						}

						[RelayCommand]
						public void Properties()
						{
								WindowProperties window = new(Window);
								window.Show();
								
						}
				}

				~WindowingDemo()
				{
						foreach (var window in windowList)
						{
								window.Window.Dispose();
						}
						windowList.Clear();
				}
		}
}
