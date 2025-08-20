using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

using static WinHack.Core.Utility.Thrower;

namespace WinHack.WindowHook.Internals
{
		public class WindowHookManaged
		{
				public HHOOK HHOOK { get; private set; }
				public WINDOWS_HOOK_ID HookId { get; private set; }
				public bool IsInstalled => !HHOOK.IsNull;

				private Func<int, WPARAM, LPARAM, int?>? callback;


				public WindowHookManaged(WINDOWS_HOOK_ID hookId) 
				{
						HookId = hookId;
				}

				public void Install(Func<int, WPARAM, LPARAM, int?> callback)
				{
						if (IsInstalled)
								throw new InvalidOperationException("Hook already installed.");

						this.callback = callback;

						HHOOK = WHookPI.SetWindowsHookEx(HookId, HookProc, HINSTANCE.Null, 0);
						if (HHOOK.IsNull)
								ThrowWin32(true, "Failed creating hook.");
				}

				//public bool TryInstall(Func<int, WPARAM, LPARAM, int?> callback)
				//{
				//		if (IsInstalled) 
				//				return false;

				//		this.callback = callback;

				//		HHOOK = WHookPI.SetWindowsHookEx(HookId, HookProc, HINSTANCE.Null, 0);
				//		return IsInstalled;
				//}

				public void Remove()
				{
						if (!IsInstalled)
								throw new InvalidOperationException("Hook isn't installed.");

						if (!WHookPI.UnhookWindowsHookEx(HHOOK))
								ThrowWin32(true, "Failed removing hook.");
				}


				private LRESULT HookProc(int nCode, WPARAM wParam, LPARAM lParam)
				{
						if (nCode < 0)
								return WHookPI.CallNextHookEx(HHOOK, nCode, wParam, lParam);

						int? returnCode = callback!(nCode, wParam, lParam);
						if (returnCode == null)
								return WHookPI.CallNextHookEx(HHOOK, nCode, wParam, lParam);
						else
								return (LRESULT)returnCode;
				}
		}
}
