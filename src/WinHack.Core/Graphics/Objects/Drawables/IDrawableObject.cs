using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinHack.Core.Graphics.DeviceContexts;

namespace WinHack.Core.Graphics.Objects.Drawables
{
		public interface IDrawableObject
		{
				public bool Draw(DeviceContextBase deviceContext, bool throwIfError = false);
		}
}
