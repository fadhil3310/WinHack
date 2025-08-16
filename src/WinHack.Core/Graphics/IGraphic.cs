using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.Graphics.Gdi;
using WinHack.Core.Graphics.DeviceContexts;
using WinHack.Core.Graphics.Objects.Brushes;
using WinHack.Core.Graphics.Objects.Drawables;

namespace WinHack.Core.Graphics
{
		public interface IGraphic
		{
				public bool Draw(IDrawableObject drawableObject, bool throwIfError = false);
				public bool UseBrush(IBrush brush, bool throwIfError = false);
				public void BlitTo(DeviceContextBase destDC, int destX, int destY, int winX, int winY, int width, int height, ROP_CODE rasterOperation = ROP_CODE.SRCCOPY, bool throwIfError = false);
				public void BlitFrom(DeviceContextBase srcDC, int srcX, int srcY, int winX, int winY, int width, int height, ROP_CODE rasterOperation = ROP_CODE.SRCCOPY, bool throwIfError = false);
		}
}
