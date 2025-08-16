using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using WinHack.Core.Interop;
using WinHack.Core.Interop.Win32;
using static WinHack.Core.Utility.Thrower;

namespace WinHack.Core.Displays
{
		public static class HMonitorUtility
		{
				public static W_MONITORINFOEX GetInfo(HMONITOR hmonitor, bool throwIfError = false)
				{
						W_MONITORINFOEX monitorInfo = new();
						if (Win32Invoke.GetMonitorInfo(hmonitor, ref monitorInfo) == 0)
								ThrowWin32(throwIfError, "Failed getting monitor info.");

						//DEVMODEW devmode = new();
						//if (PInvoke.EnumDisplaySettings(monitorInfo.DeviceName, ENUM_DISPLAY_SETTINGS_MODE.ENUM_CURRENT_SETTINGS, ref devmode) == 0)
						//		if (!ThrowWin32(throwIfError, "Failed getting monitor settings.")) return new RECT();

						return monitorInfo;
				}

				public static DEVMODEW GetSettings(HMONITOR hmonitor, bool throwIfError = false)
				{
						W_MONITORINFOEX info = GetInfo(hmonitor, throwIfError);

						DEVMODEW devMode = new();
						if (PInvoke.EnumDisplaySettings(info.DeviceName, ENUM_DISPLAY_SETTINGS_MODE.ENUM_CURRENT_SETTINGS, ref devMode) == 0)
								ThrowWin32(throwIfError, "Failed getting monitor settings.");

						return devMode;
				}

				public static Collection<DEVMODEW> GetAllSettings(HMONITOR hmonitor, bool throwIfError = false)
				{
						W_MONITORINFOEX info = GetInfo(hmonitor, throwIfError);

						Collection<DEVMODEW> devModeList = [];
						DEVMODEW devMode = new();
						for (int iModeNum = 0; PInvoke.EnumDisplaySettings(info.DeviceName, (ENUM_DISPLAY_SETTINGS_MODE)iModeNum, ref devMode) != 0; iModeNum++)
						{
								devModeList.Add(devMode);
						}

						return devModeList;
				}

				public static bool ChangeSettings(string deviceName, DEVMODEW devMode, bool throwIfError = false)
				{
						unsafe
						{
								var result = PInvoke.ChangeDisplaySettingsEx(deviceName, devMode, 0, (void*)0);
								if (result != DISP_CHANGE.DISP_CHANGE_SUCCESSFUL)
										return ThrowWin32(throwIfError, $"Failed changing monitor settings", result.ToString());
						}
						return true;
				}
		}
}
