using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Graphics.Gdi;
using WinHack.Core.Graphics.DeviceContexts;

namespace WinHack.Core.Graphics.Objects.Brushes
{
		public interface IBrush : IDisposable
		{
				public HBRUSH Handle { get; }

				public bool Use(DeviceContextBase deviceContext, bool throwIfError = false);
		}
}
