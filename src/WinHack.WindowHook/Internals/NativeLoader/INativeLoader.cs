using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using WinHack.Core.Windowing;

namespace WinHack.WindowHook.Internals.NativeLoader
{
		internal interface INativeLoader : IDisposable
		{
				bool IsInitialized { get; }

				void Initialize(string hookPipeName);

				int CreateHook(WINDOWS_HOOK_ID hookType, uint threadId);

				void RemoveHook(int hookId);
		}
}
