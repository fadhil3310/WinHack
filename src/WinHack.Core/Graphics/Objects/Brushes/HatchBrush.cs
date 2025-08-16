using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using WinHack.Core.Graphics.DeviceContexts;
using static WinHack.Core.Utility.Thrower;

namespace WinHack.Core.Graphics.Objects.Brushes
{
		public class HatchBrush : IBrush
		{
				private bool disposedValue;
				private HBRUSH _handle;
				public HBRUSH Handle => _handle;

				public HATCH_BRUSH_STYLE Style { get; private set; }

				public Color LineColor { get; private set; }

				private COLORREF _backgroundColorRef;
				private Color? _backgroundColor;

				public Color? BackgroundColor
				{
						get => _backgroundColor;
						set
						{
								_backgroundColor = value;
								if (value != null) {
										_backgroundColorRef = GraphicUtility.ColorToCOLORREF((Color)value);
								}
						}
				}

				public HatchBrush(HATCH_BRUSH_STYLE style, Color lineColor, Color? backgroundColor = null)
				{
						_handle = PInvoke.CreateHatchBrush(style, GraphicUtility.ColorToCOLORREF(lineColor));
						Style = style;
						LineColor = lineColor;
						BackgroundColor = backgroundColor;
				}

				public bool Use(DeviceContextBase deviceContext, bool throwIfError = false)
				{
						//if (BackgroundColor != null)
						//{
						//		if (PInvoke.SetBkColor(deviceContext, _backgroundColorRef) == Graphic.CLR_INVALID)
						//				return ThrowWin32(throwIfError, "Failed setting background color.");
						//}

						//if (PInvoke.SelectObject(deviceContext, _handle).IsInvalid)
						//		return ThrowWin32(throwIfError, "Failed using brush.");
						return true;
				}


				protected void Dispose(bool disposing)
				{
						if (!disposedValue)
						{
								//_handle.Dispose();
								PInvoke.DeleteObject(_handle);
								disposedValue = true;
						}
				}
				//~HatchBrush()
				//{
				//		Dispose(disposing: false);
				//}
				public void Dispose()
				{
						Dispose(disposing: true);
						GC.SuppressFinalize(this);
				}
		}
}
