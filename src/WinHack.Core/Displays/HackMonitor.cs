using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using WinHack.Core.Interop.Win32;

namespace WinHack.Core.Displays
{
		public class HackMonitor
		{
				//public int Index { get; private set; }
				public HMONITOR Handle { get; private set; }

				public string Name { get; private set; }

				public HackMonitor(HMONITOR hmonitor)
				{
						Handle = hmonitor;

						W_MONITORINFOEX info = GetRawInfo();
						Name = info.DeviceName;
				}

				// ================= PUBLIC FUNCTIONS =================

				public bool ChangeResolution(uint width, uint height)
				{
						var settings = GetRawSettings();
						settings.dmPelsWidth = width;
						settings.dmPelsHeight = height;
						settings.dmFields = DEVMODE_FIELD_FLAGS.DM_PELSWIDTH | DEVMODE_FIELD_FLAGS.DM_PELSHEIGHT;
						return HMonitorUtility.ChangeSettings(Name, settings, true);
				}

				public bool ChangeOrientation(DEVMODE_DISPLAY_ORIENTATION newOrientation)
				{
						var settings = GetRawSettings();
						var oldOrientation = settings.Anonymous1.Anonymous2.dmDisplayOrientation;
						if (oldOrientation == DEVMODE_DISPLAY_ORIENTATION.DMDO_DEFAULT || oldOrientation == DEVMODE_DISPLAY_ORIENTATION.DMDO_180)
						{
								if (newOrientation == DEVMODE_DISPLAY_ORIENTATION.DMDO_90 || newOrientation == DEVMODE_DISPLAY_ORIENTATION.DMDO_270)
								{
										uint oldWidth = settings.dmPelsWidth;
										uint oldHeight = settings.dmPelsHeight;
										settings.dmPelsWidth = oldHeight;
										settings.dmPelsHeight = oldWidth;
								}
						}
						else
						{
								if (newOrientation == DEVMODE_DISPLAY_ORIENTATION.DMDO_DEFAULT || newOrientation == DEVMODE_DISPLAY_ORIENTATION.DMDO_180)
								{
										uint oldWidth = settings.dmPelsWidth;
										uint oldHeight = settings.dmPelsHeight;
										settings.dmPelsWidth = oldHeight;
										settings.dmPelsHeight = oldWidth;
								}
						}

						settings.Anonymous1.Anonymous2.dmDisplayOrientation = newOrientation;
						settings.dmFields =
								DEVMODE_FIELD_FLAGS.DM_PELSWIDTH |
								DEVMODE_FIELD_FLAGS.DM_PELSHEIGHT |
								DEVMODE_FIELD_FLAGS.DM_DISPLAYORIENTATION;

						return HMonitorUtility.ChangeSettings(Name, settings, true);
				}

				public bool ChangeRefreshRate(uint refreshRate, bool throwIfError = false)
				{
						var settings = GetRawSettings();
						settings.dmDisplayFrequency = refreshRate;
						return HMonitorUtility.ChangeSettings(Name, settings, throwIfError);
				}

				public Size GetResolution(bool throwIfError = false) => GetRawInfo(throwIfError).Monitor.Size;

				public Collection<Size> GetSupportedResolutions(bool throwIfError = false)
						=> [.. GetAllRawSettings(throwIfError).Select(x => new Size((int)x.dmPelsWidth, (int)x.dmPelsHeight))];

				public Size GetWorkArea(bool throwIfError = false) => GetRawInfo(throwIfError).WorkArea.Size;

				public DEVMODE_DISPLAY_ORIENTATION GetOrientation(bool throwIfError = false)
						=> GetRawSettings(throwIfError).Anonymous1.Anonymous2.dmDisplayOrientation;

				public uint GetRefreshRate(bool throwIfError = false) => GetRawSettings(throwIfError).dmDisplayFrequency;

				public Collection<uint> GetSupportedRefreshRates(bool throwIfError = false)
						=> [.. GetAllRawSettings(throwIfError).Select(x => x.dmDisplayFrequency).Distinct()];

				public W_MONITORINFOEX GetRawInfo(bool throwIfError = false) => HMonitorUtility.GetInfo(Handle, throwIfError);
				public DEVMODEW GetRawSettings(bool throwIfError = false) => HMonitorUtility.GetSettings(Handle, throwIfError);
				public Collection<DEVMODEW> GetAllRawSettings(bool throwIfError = false) => HMonitorUtility.GetAllSettings(Handle, throwIfError);

				// ================= END PUBLIC FUNCTIONS =================

				public static implicit operator HMONITOR(HackMonitor monitor) => monitor.Handle;
		}
}
