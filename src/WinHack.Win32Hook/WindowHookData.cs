using System.Runtime.InteropServices;
using Windows.Win32.UI.WindowsAndMessaging;

namespace WinHack.WindowHook
{
		public record WindowHookData(HHOOK HHOOK, Thread PipeServerThread);
}
