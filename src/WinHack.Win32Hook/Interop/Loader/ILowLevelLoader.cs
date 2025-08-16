using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using WinHack.Core.Windowing;

namespace WinHack.WindowHook.Interop.Loader
{
		internal interface ILowLevelLoader : IDisposable
		{
				bool IsInitialized { get; }

				void Initialize(string hookPipeName);

				HHOOK CreateLocalHook(WINDOWS_HOOK_ID hookId, uint threadId);

				void RemoveHook(WindowHookData hook);
		}
}
