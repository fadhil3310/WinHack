using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinHack.Core.Utility
{
		public static class HandleUtility
		{
				public static bool IsInvalid(long handle)
				{
						return handle == -1L || handle == 0L;
				}
				public static bool IsInvalid(IntPtr handle)
				{
						return IsInvalid((long)handle);
				}
		}
}
