using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using static WinHack.Core.Utility.Thrower;

namespace WinHack.Core.Systems.Process
{
		public static class ProcessUtility
		{
				public static unsafe bool Is64Bit(HANDLE processHandle, bool throwIfError = false)
				{
						// TODO: I'm not sure why do i have to invert is64bit if the process is running in WoW64.
						// If the process isnt running in WoW64, does it mean that the process is 64 bit?
						// (assumming that WinHack can only be run on a 64 bit machine).
						BOOL is64Bit = false;
						if (!PInvoke.IsWow64Process(processHandle, &is64Bit))
								return ThrowWin32(throwIfError, "Failed getting if process is running in WoW64 environment or not.");
						else
								is64Bit = !is64Bit;
						return is64Bit;
				}
		}
}
