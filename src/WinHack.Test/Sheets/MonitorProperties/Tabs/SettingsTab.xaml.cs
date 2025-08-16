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
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.Input;
using DependencyPropertyGenerator;
using Windows.Win32.Graphics.Gdi;
using WinHack.Core.Utility;
using HackMonitor = WinHack.Core.Displays.HackMonitor;

using Size = System.Drawing.Size;

namespace WinHack.Test.Sheets.MonitorProperties.Tabs
{
		/// <summary>
		/// Interaction logic for SettingsTab.xaml
		/// </summary>
		[DependencyProperty<HackMonitor>("SelectedMonitor")]
		[DependencyProperty<Collection<Size>>("SupportedResolutions")]
		[DependencyProperty<Collection<uint>>("SupportedRefreshRates")]
		[DependencyProperty<OrientationListModel>("SelectedOrientation")]
		public partial class SettingsTab : UserControl
		{
				public Collection<OrientationListModel> OrientationModes { get; } =
						[
								new(DEVMODE_DISPLAY_ORIENTATION.DMDO_DEFAULT, "0"),
								new(DEVMODE_DISPLAY_ORIENTATION.DMDO_90, "90"),
								new(DEVMODE_DISPLAY_ORIENTATION.DMDO_180, "180"),
								new(DEVMODE_DISPLAY_ORIENTATION.DMDO_270, "270"),
						];

				public SettingsTab()
				{
						InitializeComponent();

						Loaded += (_, _) =>
						{
								SupportedResolutions = SelectedMonitor!.GetSupportedResolutions();
								SupportedRefreshRates = SelectedMonitor!.GetSupportedRefreshRates();
								SelectedOrientation = OrientationModes.First(x => x.Orientation == SelectedMonitor!.GetOrientation());
						};
				}

				[RelayCommand]
				public void SetResolution()
				{
						try
						{
								SelectedMonitor!.ChangeResolution(
											uint.Parse(resWidthTextBox.Text),
											uint.Parse(resHeightTextBox.Text)
									);
						}
						catch (WinHackCoreException ex)
						{
								MessageBox.Show(ex.ToString(), "Failed setting resolution.");
						}
				}

				[RelayCommand]
				public void SetOrientation()
				{
						try
						{
								SelectedMonitor!.ChangeOrientation(SelectedOrientation!.Orientation);
						}
						catch (WinHackCoreException ex)
						{
								MessageBox.Show(ex.ToString(), "Failed setting orientation.");
						}
				}

				[RelayCommand]
				public void SetRefreshRate()
				{
						try
						{
								SelectedMonitor!.ChangeRefreshRate(uint.Parse(refreshRateTextBox.Text), true);
						}
						catch (WinHackCoreException ex)
						{
								MessageBox.Show(ex.ToString(), "Failed setting refresh rate.");
						}
				}

				public record OrientationListModel(DEVMODE_DISPLAY_ORIENTATION Orientation, string Text);
		}
}
