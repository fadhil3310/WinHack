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
		public class MouseLocalHook : WindowHookNativeBase
		{
				private MouseLocalHook(HackWindow window, WindowHookNative hookInstance) : base(window, hookInstance)
				{
				}

				public static MouseLocalHook Create(HackWindow window, Func<int, MOUSEHOOKSTRUCT, int> onMessageReceived, Action? onEnded = null)
				{
						WindowHookNative hookInstance = new(WINDOWS_HOOK_ID.WH_MOUSE);
						hookInstance.InstallLocal(window, onMessageReceived, onEnded);

						MouseLocalHook callWNDHook = new(window, hookInstance);
						return callWNDHook;
				}

				public override void Remove()
				{
						hookInstance.Remove();
				}
		}
}
