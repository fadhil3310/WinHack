using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using WinHack.Core.Base;
using WinHack.Core.Graphics.DeviceContexts;
using WinHack.Core.Graphics.Objects.Brushes;
using WinHack.Core.Graphics.Objects.Drawables;
using WinHack.Core.Windowing;
using static WinHack.Core.Utility.Thrower;

namespace WinHack.Core.Graphics
{
		public class Graphic : IDisposable
		{
				//public Window? Window { get; private set; }
				public DeviceContextBase DeviceContext { get; protected set; }
				//public HDC SecondaryDC { get; private set; }
				//public DeleteObjectSafeHandle HBitmap { get; private set; }

				//public int Width { get; private set; }
				//public int Height { get; private set; }

				//public bool IsCompatibleDC { get; private set; }


				protected static object _lock = new();


				//public Graphic(Window window)
				//{
				//		lock (_lock)
				//		{
								
				//		}
				//}

				public Graphic(DeviceContextBase deviceContext)
				{
						DeviceContext = deviceContext;
				}
				protected Graphic() { }

				// ========================= PUBLIC FUNCTIONS =========================
				// ============================== Drawer ==============================
				//public void DrawRectangle(int x, int y, int width, int height, System.Drawing.Color fillColor, bool throwIfError = false)
				//{
				//		lock (_lock)
				//		{
				//				// Use secondaryDC if user chose to draw to secondaryDC when constructing class.
				//				//var context = SecondaryDC.IsNull ? HDC : SecondaryDC;

				//				// Create brush.
				//				var brush = PInvoke.CreateSolidBrush_SafeHandle();
				//				if (brush.IsInvalid)
				//						ThrowWin32(throwIfError, "Failed creating brush.");
								
				//				// Draw rect.
							

				//				// Finalize drawing.
				//				brush.Dispose();
				//				//CopySecondaryToWindowDC(width, height, throwIfError);
				//		}
				//}

				public virtual bool Draw(IDrawableObject drawableObject, bool throwIfError = false)
				{
						lock (_lock)
						{
								return drawableObject.Draw(DeviceContext, throwIfError);
						}
				}

				public virtual bool UseBrush(IBrush brush, bool throwIfError = false)
				{
						lock (_lock)
						{
								return brush.Use(DeviceContext, throwIfError);
						}
				}

				/// <summary>
				/// Copy the Graphic's DeviceContext content to destination DeviceContext
				/// </summary>
				/// <param name="destDC"></param>
				/// <param name="destX"></param>
				/// <param name="destY"></param>
				/// <param name="winX"></param>
				/// <param name="winY"></param>
				/// <param name="width"></param>
				/// <param name="height"></param>
				/// <param name="rasterOperation"></param>
				/// <param name="throwIfError"></param>
				public virtual void BlitTo(DeviceContextBase destDC, int destX, int destY, int winX, int winY, int width, int height, ROP_CODE rasterOperation = ROP_CODE.SRCCOPY, bool throwIfError = false)
				{
						lock (_lock)
						{
								if (PInvoke.BitBlt(destDC, destX, destY, width, height, DeviceContext, winX, winY, rasterOperation) == 0)
										ThrowWin32(throwIfError, "Failed BitBlt-ing window HDC to destination HDC.");
						}
				}

				/// <summary>
				/// Copy the source DeviceContext content to Graohic's DeviceContext
				/// </summary>
				/// <param name="destDC"></param>
				/// <param name="destX"></param>
				/// <param name="destY"></param>
				/// <param name="winX"></param>
				/// <param name="winY"></param>
				/// <param name="width"></param>
				/// <param name="height"></param>
				/// <param name="rasterOperation"></param>
				/// <param name="throwIfError"></param>
				public virtual void BlitFrom(DeviceContextBase srcDC, int srcX, int srcY, int winX, int winY, int width, int height, ROP_CODE rasterOperation = ROP_CODE.SRCCOPY, bool throwIfError = false)
				{
						lock (_lock)
						{
								if (PInvoke.BitBlt(DeviceContext, winX, winY, width, height, srcDC, srcX, srcY, rasterOperation) == 0)
										ThrowWin32(throwIfError, "Failed BitBlt-ing source HDC to window HDC.");
						}
				}

				//public void CopySecondaryToWindowDC(int? width = null, int? height = null, bool throwIfError = false)
				//{
				//		lock (_lock)
				//		{
				//				if (!SecondaryDC.IsNull)
				//						TransferToWindow(SecondaryDC, 0, 0, 0, 0, width ?? Width, height ?? Height, ROP_CODE.SRCCOPY, throwIfError);
				//				else
				//						ThrowInvalidOperation(throwIfError, "Secondary device context is null.");
				//		}
				//}

				//public void CopyWindowToSecondaryDC(int? width = null, int? height = null, bool throwIfError = false)
				//{
				//		lock (_lock)
				//		{
				//				if (!SecondaryDC.IsNull)
				//						TransferFromWindow(SecondaryDC, 0, 0, 0, 0, width ?? Width, height ?? Height, ROP_CODE.SRCCOPY, throwIfError);
				//				else
				//						ThrowInvalidOperation(throwIfError, "Secondary device context is null.");
				//		}
				//}
				// ========================= END Drawer =========================


				// ========================= Get Data =========================
				//public RECT GetSecondaryDCSize()
				//{
				//		RECT rect = new()
				//		{
				//				right = PInvoke.GetDeviceCaps(SecondaryDC, GET_DEVICE_CAPS_INDEX.HORZSIZE),
				//				bottom = PInvoke.GetDeviceCaps(SecondaryDC, GET_DEVICE_CAPS_INDEX.VERTSIZE)
				//		};
				//		return rect;
				//}
				// ======================== END Get Data Functions ========================
				// ========================= END PUBLIC FUNCTIONS =========================


				// ========================= PRIVATE FUNCTIONS =========================
				//private bool FinalizeDraw(int width, int height, bool? throwIfErrorOverride)
				//{
				//		if (!SecondaryDC.IsNull)
				//				TransferToWindow(SecondaryDC, 0, 0, 0, 0, width, height, ROP_CODE.SRCCOPY, throwIfErrorOverride);
				//		return true;
				//}
				// ========================= END PRIVATE FUNCTIONS =========================

				public virtual void Dispose()
				{
						//if (!HBitmap.IsInvalid)
						//		HBitmap.Dispose();
						//if (!SecondaryDC.IsNull)
						//		PInvoke.DeleteDC(SecondaryDC);
						//DeviceContext.Dispose();
						GC.SuppressFinalize(this);
				}

				public const uint CLR_INVALID = 0xFFFFFFFF;
		}
}
