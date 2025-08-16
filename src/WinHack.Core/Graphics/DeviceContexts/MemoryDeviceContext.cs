using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using WinHack.Core.Graphics.Objects;
using WinHack.Core.Utility;
using WinHack.Core.Windowing;
using static WinHack.Core.Utility.Thrower;

namespace WinHack.Core.Graphics.DeviceContexts
{
		public class MemoryDeviceContext : DeviceContextBase
		{
				public Bitmap? Bitmap { get; private set; }

				public MemoryDeviceContext(HDC hdc, HBITMAP? hbitmap = null) : base(hdc)
				{
						if (hbitmap != null)
						{
								var bitmap = new Bitmap((HBITMAP)hbitmap, this);
								Bitmap = bitmap;
						}
				}

				public static MemoryDeviceContext CreateFromDeviceContext(DeviceContextBase referenceDC)
				{
						var newHDC = PInvoke.CreateCompatibleDC(referenceDC);
						if (newHDC.IsNull)
								ThrowWin32(true, "Failed creating secondary device context.");

						var size = referenceDC.GetSize();
						Debug.WriteLine($"Size? {size.Width} {size.Height}");
						
						var hBitmap = PInvoke.CreateCompatibleBitmap(referenceDC, size.Width, size.Height);
						if (HandleUtility.IsInvalid(hBitmap))
								ThrowWin32(true, "Failed creating bitmap.");
						if (HandleUtility.IsInvalid(PInvoke.SelectObject(newHDC, hBitmap)))
								ThrowWin32(true, "Failed selecting object.");

						MemoryDeviceContext memoryDC = new(newHDC, hBitmap);
						return memoryDC;
				}


				protected override void Dispose(bool disposing)
				{
						if (!disposedValue)
						{
								Bitmap?.Dispose();
								if (!HDC.IsNull)
										PInvoke.DeleteDC(HDC);
								disposedValue = true;
						}
				}
		}
}
