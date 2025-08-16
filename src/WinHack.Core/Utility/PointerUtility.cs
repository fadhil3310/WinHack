using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.Foundation;

namespace WinHack.Core.Utility
{
		public static class PointerUtility
		{
				public static unsafe char* StringToCharP(string value)
				{
						fixed (char* ptr = value)
						{
								return ptr;
						}
				}

				public static unsafe PCWSTR StringToPCWSTR(string value)
				{
						return new PCWSTR(StringToCharP(value));
				}

				public static unsafe PCSTR StringToPCSTR(string value)
				{
						fixed (byte* ptr = Encoding.ASCII.GetBytes(value))
						{
								return new PCSTR(ptr);
						}
				}
		}
}
