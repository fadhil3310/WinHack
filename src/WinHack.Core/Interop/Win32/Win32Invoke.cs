using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.Graphics.Gdi;
using WinHack.Core.Interop.Win32;

namespace WinHack.Core.Interop
{
		public static partial class Win32Invoke
		{
				[DllImport("user32.dll", CharSet = CharSet.Auto)]
				public static extern int GetMonitorInfo(HMONITOR hMonitor, ref W_MONITORINFOEX lpmi);
		}
}
