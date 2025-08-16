using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinHack.Core.Windowing;
using Windows.Win32.UI.WindowsAndMessaging;

using static WinHack.Core.Utility.Thrower;

namespace WinHack.WindowHook
{
		public static class WindowHookExtensions
		{
				//public static bool Hook(
				//		this Window window, 
				//		WINDOWS_HOOK_ID hookId, 
				//		WindowHookCallback callback, 
				//		bool throwIfError = false)
				//{
				//		WindowHookBase hook = new(window);
				//		if (!hook.Start(hookId, callback, throwIfError))
				//		{
				//				hook.Dispose();
				//				return false;
				//		}

				//		window.RetainResources(hook);
				//		return true;
				//}
		}
}
