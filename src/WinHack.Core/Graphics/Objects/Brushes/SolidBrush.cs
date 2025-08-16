using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Graphics.Gdi;
using WinHack.Core.Graphics.DeviceContexts;
using WinHack.Core.Utility;
using static WinHack.Core.Utility.Thrower;

namespace WinHack.Core.Graphics.Objects.Brushes
{
		public class SolidBrush : IBrush
		{
				private bool disposedValue;
				private HBRUSH _handle;

				public HBRUSH Handle => _handle;


				public SolidBrush(System.Drawing.Color color) 
				{
						_handle = PInvoke.CreateSolidBrush(GraphicUtility.ColorToCOLORREF(color));
				}

				public bool Use(DeviceContextBase deviceContext, bool throwIfError = false)
				{
						//Debug.WriteLine($"Brush thread ID: {Environment.CurrentManagedThreadId}");
						//Debug.WriteLine($"Handle: {_handle.DangerousGetHandle()}");
						//DeleteObjectSafeHandle
						var result = PInvoke.SelectObject(deviceContext, _handle);
						if (HandleUtility.IsInvalid(result))
								return ThrowWin32(throwIfError, "Failed using brush.", _handle.ToString());
						return true;
				}


				protected void Dispose(bool disposing)
				{
						if (!disposedValue)
						{
								Debug.WriteLine("Disposed!!");
								//_handle.Dispose();
								PInvoke.DeleteObject(_handle);
								disposedValue = true;
						}
				}
				//~SolidBrush()
				//{
				//		Debug.WriteLine("Finalized!!");
				//		Dispose(disposing: false);
				//}
				public void Dispose()
				{
						Dispose(disposing: true);
						GC.SuppressFinalize(this);
				}
		}
}
