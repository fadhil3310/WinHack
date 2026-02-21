using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinHack.WindowHook
{
		public static class WindowHookOptions
		{
				/// <summary>
				/// Hook pipe name.
				/// <br />
				/// <br />
				/// <strong>Name changes will not be honored after pipe has been initialized.</strong>
				/// </summary>
				public static string HookPipeName { get; set; } = "";

				/// <summary>
				/// 32-bit hook surrogate's pipe name.
				/// <br />
				/// <br />
				/// <strong>Name changes will not be honored after surrogate has been initialized.</strong>
				/// </summary>
				public static string Surrogate32PipeName { get; set; } = "";

				/// <summary>
				/// 32-bit hook surrogate executable path. 
				/// <br />
				/// <br />
				/// <strong>Path changes will not be honored after surrogate process has been launched.</strong>
				/// </summary>
				public static string Surrogate32Path = "WinHack.WindowHook.NativeSurrogate.exe";

				/// <summary>
				/// 32-bit hook surrogate DLL path.
				/// <br />
				/// <strong>Path changes will not be honored after surrogate process has been launched.</strong>
				/// </summary>
				public static string Surrogate32LibraryPath { get; set; } = "WinHack.WindowHook.Native32.dll";
		}
}
