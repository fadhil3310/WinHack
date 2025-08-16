using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.Foundation;
using System.Drawing;

namespace WinHack.Core.Graphics
{
		public static class GraphicUtility
		{
				public static COLORREF ColorToCOLORREF(Color color) => 
						new COLORREF((uint)ColorTranslator.ToWin32(color));
		}
}
