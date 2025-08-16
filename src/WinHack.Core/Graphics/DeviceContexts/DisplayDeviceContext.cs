using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32;
using Windows.Win32.Foundation;
using WinHack.Core.Displays;
using WinHack.Core.Windowing;

namespace WinHack.Core.Graphics.DeviceContexts
{
		public class DisplayDeviceContext : DeviceContextBase
		{
				public HackMonitor Display { get; set; }

				public DisplayDeviceContext(HDC hdc, HackMonitor display) : base(hdc)
				{
						Display = display;
				}
				public DisplayDeviceContext(HackMonitor display)
				{
						var hdc = PInvoke.CreateDCW(null, display.Name, null, null);
						if (hdc.IsNull)
								throw new InvalidOperationException("Failed to obtain device context for the provided monitor.");

						HDC = hdc;
						Display = display;
				}


				protected override void Dispose(bool disposing)
				{
						if (!disposedValue)
						{
								if (!HDC.IsNull)
										PInvoke.DeleteDC(HDC);
								disposedValue = true;
						}
				}
		}
}
