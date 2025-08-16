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
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.Input;
using DependencyPropertyGenerator;
using Windows.Win32.Foundation;
using WinHack.WindowHook;
using WinHack.WindowHook.Local;
using HackWindow = WinHack.Core.Windowing.HackWindow;


namespace WinHack.Test.Sheets.WindowProperties.Tabs
{
		/// <summary>
		/// Interaction logic for CommandsTab.xaml
		/// </summary>
		[DependencyProperty<HackWindow>("SelectedWindow")]
		public partial class CommandsTab : UserControl
		{
				public CommandsTab()
				{
						InitializeComponent();
				}

				[RelayCommand]
				public void Minimize() => SelectedWindow!.Minimize();
				[RelayCommand]
				public void Maximize() => SelectedWindow!.Maximize();
				[RelayCommand]
				public void Restore() => SelectedWindow!.Restore();
				[RelayCommand]
				public void Hide() => SelectedWindow!.Hide();
				[RelayCommand]
				public void Show() => SelectedWindow!.Show();

				[RelayCommand]
				public void SetPosition()
				{
						int x = int.Parse(newPosXTextBox.Text);
						int y = int.Parse(newPosXTextBox.Text);
						SelectedWindow!.SetPosition(x, y);
				}


				[RelayCommand]
				public void HookMessage()
				{
						var hook = CallWNDHook.CreateLocal(
								SelectedWindow!,
								(nCode, lParam) =>
								{
										Debug.WriteLine($"Got message: {nCode} {lParam.hwnd} {lParam.message}.");
										return 0;
								}, 
								() =>
								{
										Debug.WriteLine("Hook ended.");
								});
				}
		}
}
