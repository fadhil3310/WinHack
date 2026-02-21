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
		public class MouseLocalHook : WindowHookNativeBase
		{
				// Note: I'm not sure whether to give the WPARAM to the caller
				// as i can't figure out if the information given in WPARAM is neccessary (need testing).

				private MouseLocalHook(HackWindow window, WindowHookNative hookInstance) : base(window, hookInstance)
				{
				}

				public static MouseLocalHook Create(HackWindow window, Func<int, WPARAM, MOUSEHOOKSTRUCT, int> onMessageReceived, Action? onEnded = null)
				{
						WindowHookNative hookInstance = new(WINDOWS_HOOK_ID.WH_MOUSE);
						hookInstance.Install(window, (nCode, wParam, message) =>
						{
								ReadOnlySpan<MOUSEHOOKSTRUCT> marshalledStruct = MemoryMarshal.Cast<byte, MOUSEHOOKSTRUCT>(message);
								return onMessageReceived(nCode, wParam, marshalledStruct[0]);
						}, onEnded);

						MouseLocalHook callWNDHook = new(window, hookInstance);
						return callWNDHook;
				}

				public override void Remove()
				{
						hookInstance.Remove();
				}
		}
}
