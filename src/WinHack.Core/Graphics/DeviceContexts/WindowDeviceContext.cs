using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32;
using Windows.Win32.Foundation;
using WinHack.Core.Windowing;

namespace WinHack.Core.Graphics.DeviceContexts
{
		public class WindowDeviceContext : DeviceContextBase
		{
				public HackWindow Window { get; set; }

				public WindowDeviceContext(HDC hdc, HackWindow window) : base(hdc)
				{
						Window = window;
				}
				public WindowDeviceContext(HackWindow window)
				{
						var windowDimensions = window.GetDimensions();

						var hdc = PInvoke.GetDC(window);
						if (hdc.IsNull)
								throw new InvalidOperationException("Failed to obtain device context for the provided window handle.");

						HDC = hdc;
						Window = window;
				}

				//public override RECT GetSize()
				//{
				//		return Window.GetDimensions();
				//}

				protected override void Dispose(bool disposing)
				{
						if (!disposedValue)
						{
								if (!HDC.IsNull)
										PInvoke.ReleaseDC(Window, HDC);
								disposedValue = true;
						}
				}
		}
}
