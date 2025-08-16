using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinHack.Core.Graphics.Objects.Brushes;
using Windows.Win32;
using Windows.Win32.Foundation;
using WinHack.Core.Graphics.DeviceContexts;
using static WinHack.Core.Utility.Thrower;

namespace WinHack.Core.Graphics.Objects.Drawables
{
		public class Rectangle : IDrawableObject
		{
				private int left;
				private int top;
				private int right;
				private int bottom;

				public int X {
						get => left;
						set
						{
								left = value;
								right = left + _width;
						}
				}

				public int Y {
						get => top;
						set
						{
								top = value;
								bottom = top + _height;
						}
				}

				private int _width;
				public int Width { 
						get => _width;
						set
						{
								_width = value;
								right = left + _width;
						}
				}

				private int _height;
				public int Height
				{
						get => _width;
						set
						{
								_height = value;
								bottom = top + _height;
						}
				}

				public RECT RECT => new(left, top, right, bottom);


				public Rectangle(int x, int y, int width, int height)
				{
						left = x;
						top = y;
						Width = width;
						Height = height;
				}

				public bool Draw(DeviceContextBase deviceContext, bool throwIfError = false)
				{
						if (PInvoke.Rectangle(deviceContext, left, top, right, bottom) == 0)
								return ThrowWin32(throwIfError, "Failed drawing rectangle.");

						return true;
				}
		}
}
