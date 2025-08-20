using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.UI.WindowsAndMessaging;
using WinHack.Core.Windowing;
using WinHack.WindowHook.Internals;

namespace WinHack.WindowHook.Hooks
{
		public class CallWNDHook : WindowHookNativeBase
		{

				private CallWNDHook(HackWindow window, WindowHookNative hookInstance) : base(window, hookInstance)
				{
				}

				public static CallWNDHook CreateLocal(HackWindow window, Func<int, CWPSTRUCT, int> onMessageReceived, Action? onEnded = null)
				{
						WindowHookNative hookInstance = new(WINDOWS_HOOK_ID.WH_CALLWNDPROC);
						hookInstance.InstallLocal(window, onMessageReceived, onEnded);

						CallWNDHook callWNDHook = new(window, hookInstance);
						return callWNDHook;
				}

				public override void Remove()
				{
						hookInstance.Remove();
				}
		}
}
