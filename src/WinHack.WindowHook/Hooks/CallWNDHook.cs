using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
						hookInstance.Install(window, (nCode, wParam, message) =>
						{
								unsafe
								{
										fixed (byte* ptr = message)
										{
												CWPSTRUCT marshalledStruct = Marshal.PtrToStructure<CWPSTRUCT>((nint)ptr);

												return onMessageReceived(nCode, marshalledStruct);
										}
								}

								//Span<CWPSTRUCT> marshalledStruct = MemoryMarshal.Cast<byte, CWPSTRUCT>(message);
								//return onMessageReceived(nCode, marshalledStruct[0]);
						}, onEnded);

						CallWNDHook callWNDHook = new(window, hookInstance);
						return callWNDHook;
				}

				public override void Remove()
				{
						hookInstance.Remove();
				}
		}
}
