using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.Foundation;
using Windows.Win32;

namespace WinHack.Core.Graphics.DeviceContexts
{
		public abstract class DeviceContextBase : IDisposable
		{
				protected bool disposedValue;

				public HDC HDC { get; set; }

				protected DeviceContextBase() { }
				protected DeviceContextBase(HDC hdc) => HDC = hdc;

				public virtual RECT GetSize()
				{
						RECT rect = new(
								0, 0, 
								PInvoke.GetDeviceCaps(HDC, GET_DEVICE_CAPS_INDEX.HORZSIZE), 
								PInvoke.GetDeviceCaps(HDC, GET_DEVICE_CAPS_INDEX.VERTSIZE)
						);
						return rect;
				}

				public static implicit operator HDC(DeviceContextBase dc) => dc.HDC;


				protected abstract void Dispose(bool disposing);
				~DeviceContextBase()
				{
						Dispose(disposing: false);
				}
				public void Dispose()
				{
						Dispose(disposing: true);
						GC.SuppressFinalize(this);
				}
		}
}
