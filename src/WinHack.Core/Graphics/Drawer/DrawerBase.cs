using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using WinHack.Core.Base;
using WinHack.Core.Graphics;
using WinHack.Core.Graphics.DeviceContexts;
using WinHack.Core.Graphics.Drawer.Animation;
using WinHack.Core.Graphics.Objects.Brushes;
using WinHack.Core.Graphics.Objects.Drawables;
using WinHack.Core.Windowing;

namespace WinHack.Core.Graphics.Drawer
{
		public abstract class DrawerBase : Graphic
		{
				//public DeviceContextBase MainDeviceContext { get; protected set; }
				public DeviceContextBase MainDeviceContext { get; }

				//public Graphic Graphic { get; protected set; }

				protected DrawerBase(DeviceContextBase deviceContext)
				{
						//MainDeviceContext = mainDC;
						MainDeviceContext = deviceContext;
						DeviceContext = MemoryDeviceContext.CreateFromDeviceContext(deviceContext);
						//Graphic = new Graphic(SecondaryDeviceContext);
				}

				//public bool Draw(IDrawableObject drawableObject, bool throwIfError = false);

				public abstract DrawerAnimation CreateAnimation(float from, float to, int duration, uint? fps = null, Func<float, float>? easing = null);
		}
}
