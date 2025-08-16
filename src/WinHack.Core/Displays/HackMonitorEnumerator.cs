using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using WinHack.Core.Windowing;

namespace WinHack.Core.Displays
{
		public static class HackMonitorEnumerator
		{
				public static Collection<HackMonitor> GetAllDisplays()
				{
						Collection<HackMonitor> monitors = [];

						//int index = 0;
						unsafe
						{
								PInvoke.EnumDisplayMonitors(HDC.Null, lpfnEnum: (HMONITOR hmonitor, HDC _, RECT* _, LPARAM _) =>
								{
										HackMonitor screen = new(hmonitor);
										monitors.Add(screen);
										return true;
								}, dwData: 0);
						}

						return monitors;
				}

				public static HackMonitor GetDisplayFromWindow(HackWindow window, MONITOR_FROM_FLAGS flags = MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST)
				{
						HMONITOR hmonitor = PInvoke.MonitorFromWindow(window, flags);
						//int index = -1;
						//FindRawMonitor((HMONITOR x) =>
						//{
						//		index++;
						//		return x == hmonitor;
						//});
						return new HackMonitor(hmonitor);
				}

				//public static HMONITOR FindRawMonitor(Func<HMONITOR, bool> predicate)
				//{
				//		HMONITOR hmonitor;
				//		unsafe
				//		{
				//				PInvoke.EnumDisplayMonitors(HDC.Null, lpfnEnum: (HMONITOR enumHMonitor, HDC _, RECT* _, LPARAM _) =>
				//				{
				//						if (predicate(enumHMonitor)) return false;
				//						hmonitor = enumHMonitor;
				//						return true;
				//				}, dwData: 0);
				//		}
				//		return hmonitor;
				//}
		}
}
