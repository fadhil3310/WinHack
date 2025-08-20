using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using WinHack.Core.Windowing;
using WinHack.WindowHook.Internals;

namespace WinHack.WindowHook.Hooks
{
		public class MouseGlobalHook : WindowHookManagedBase
		{
				private Func<int, WPARAM, MSLLHOOKSTRUCT, int?> callback;


				private MouseGlobalHook(HackWindow window, WindowHookManaged hookInstance, Func<int, WPARAM, MSLLHOOKSTRUCT, int?> callback) : base(window, hookInstance)
				{
						this.callback = callback;

						hookInstance.Install(ProcessRawHookCallback);
				}

				public static MouseGlobalHook Create(HackWindow window, Func<int, WPARAM, MSLLHOOKSTRUCT, int?> onMessageReceived, Action? onEnded = null)
				{
						WindowHookManaged hookInstance = new(WINDOWS_HOOK_ID.WH_MOUSE_LL);
						MouseGlobalHook callWNDHook = new(window, hookInstance, onMessageReceived);
						return callWNDHook;
				}

				private unsafe int? ProcessRawHookCallback(int nCode, WPARAM wParam, LPARAM lParam)
				{
						MSLLHOOKSTRUCT param = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
						return callback(nCode, wParam, param);
				}

				public override void Remove()
				{
						hookInstance.Remove();
				}
		}
}
