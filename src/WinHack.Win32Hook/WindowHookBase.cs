using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.UI.WindowsAndMessaging;
using WinHack.Core.Base;
using WinHack.Core.Utility;
using WinHack.Core.Windowing;
using WinHack.WindowHook.Interop;
using static WinHack.Core.Utility.Thrower;

namespace WinHack.WindowHook
{
		public abstract class WindowHookBase : IWinHackDisposable
		{
				// ======================= Private Variables =======================
				protected bool disposedValue;
				protected readonly List<IDisposable> disposables = [];
				// ======================= End Private Variables =======================


				// ======================= Public Variables =======================
				public HackWindow? Window { get; protected set; }

				public WindowHookData HookData { get; protected set; }
				// ======================= End Public Variables =======================


				protected WindowHookBase(HackWindow? window, WindowHookData hookData)
				{
						Window = window;
						HookData = hookData;
				}


				public abstract void Remove();

				// ======================= Dispose =======================
				public void RetainResources(params IDisposable[] resources)
				{
						DisposableUtility.MapDisposables(disposables, resources);
				}
				public void ReleaseResources(params IDisposable[] resources)
				{
						DisposableUtility.UnmapDisposables(disposables, resources);
				}

				public void Dispose()
				{
						if (disposedValue) return;

						DisposableUtility.DisposeAll(disposables);

						disposedValue = true;
						GC.SuppressFinalize(this);
				}
		}
}
