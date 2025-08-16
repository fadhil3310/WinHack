using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.UI.WindowsAndMessaging;
using WinHack.Core.Windowing;
using WinHack.WindowHook.Interop;

namespace WinHack.WindowHook.Local
{
		public class CallWNDHook : WindowHookBase
		{
				private CallWNDHook(HackWindow window, WindowHookData hookData) : base(window, hookData)
				{
				}

				public static CallWNDHook CreateLocal(HackWindow window, Func<int, CWPSTRUCT, int> onMessageReceived, Action? onEnded = null)
				{
						WindowHookData hookData = WindowHookLowLevel.Instance.CreateLocalHook(WINDOWS_HOOK_ID.WH_CALLWNDPROC, window, onMessageReceived, onEnded);
						CallWNDHook callWNDHook = new(window, hookData);
						return callWNDHook;
				}

				public override void Remove()
				{
						throw new NotImplementedException();
				}
		}
}
