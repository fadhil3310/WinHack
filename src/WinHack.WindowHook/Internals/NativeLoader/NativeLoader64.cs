using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using WinHack.Core.Systems.Library;
using WinHack.Core.Utility;
using WinHack.Core.Windowing;
using static WinHack.Core.Utility.Thrower;

namespace WinHack.WindowHook.Internals.NativeLoader
{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate bool InitializeDelegate([MarshalAs(UnmanagedType.LPWStr)] string mainPipeName);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int CreateHookDelegate(int hookType, uint threadId);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate bool RemoveHookDelegate(int hookId);


		public class NativeLoader64 : INativeLoader
		{
				bool disposedValue;

				/// <summary>
				/// Is initialized?.
				/// </summary>
				public bool IsInitialized => _library.IsAllocated;

				// Not sure if Library need to be stored in a GCHandle as it's a managed class,
				// just worried if it will be deallocated prematurely by the GC
				public HackLibrary? Library { get => (HackLibrary?)_library.Target; }
				private GCHandle _library;

				/// <summary>
				/// The path of the dll. 
				/// <br />
				/// <strong>Can only be changed before initialized.</strong>
				/// </summary>
				/// <param name="path"></param>
				/// <exception cref="InvalidOperationException"></exception>
				public string LibraryPath { 
						get => _libraryPath;
						set
						{
								if (IsInitialized)
										throw new InvalidOperationException("DLL has been loaded.");
								if (string.IsNullOrEmpty(value))
										throw new ArgumentException("Value can't be empty.");

								_libraryPath = value;
						}
				}
				private string _libraryPath = "WinHack.WindowHook.Native64.dll";


				// DLL procedures.
				private InitializeDelegate DLLInitialize => (InitializeDelegate)_dllInitialize.Target!;
				private GCHandle _dllInitialize;
				private CreateHookDelegate createHook => (CreateHookDelegate)_createHook.Target!;
				private GCHandle _createHook;
				private RemoveHookDelegate removeHook => (RemoveHookDelegate)_removeHook.Target!;
				private GCHandle _removeHook;


				/// <summary>
				/// Load the dll and all of its required procedures.
				/// </summary>
				public void Initialize(string hookPipeName)
				{
						// Load DLL.
						var library = new HackLibrary(LibraryPath);
						_library = GCHandle.Alloc(library, GCHandleType.Normal);

						// ---- Get address of all of the required procedures ----
						// Initialize.
						var dgInitialize = library.GetProcAddressDelegate<InitializeDelegate>("Initialize");
						_dllInitialize = GCHandle.Alloc(dgInitialize, GCHandleType.Normal);

						// CreateLocalHook.
						var dgCreateHook = library.GetProcAddressDelegate<CreateHookDelegate>("CreateHook");
						_createHook = GCHandle.Alloc(dgCreateHook, GCHandleType.Normal);

						// RemoveHook.
						var dgRemoveHook = library.GetProcAddressDelegate<RemoveHookDelegate>("RemoveHook");
						_removeHook = GCHandle.Alloc(dgRemoveHook, GCHandleType.Normal);
						// -------------------------------------------------------

						// Initialize dll.
						DLLInitialize(hookPipeName);
				}

				/// <summary>
				/// Create CallWnd hook.
				/// </summary>
				/// <param name="window"></param>
				/// <returns></returns>
				public int CreateHook(WINDOWS_HOOK_ID hookType, uint threadId)
				{
						if (!IsInitialized)
								throw new InvalidOperationException("Cannot call this method before initialization.");

						Debug.WriteLine("Attempt to create a hook.");
						int hookId = createHook((int)hookType, threadId);
						if (hookId == 0)
								ThrowWin32(true, "Failed creating hook.");

						return hookId;
				}

				/// <summary>
				/// Remove hook.
				/// </summary>
				/// <param name="hook"></param>
				public void RemoveHook(int hookId)
				{
						if (!IsInitialized)
								throw new InvalidOperationException("Cannot call this method before initialization.");

						if (!removeHook(hookId))
								throw new InvalidOperationException("Failed removing hook.");
				}

				public void Dispose()
				{
						if (disposedValue) return;

						Library?.Dispose();
						_library.Free();

						_dllInitialize.Free();
						_createHook.Free();
						_removeHook.Free();

						disposedValue = true;
						GC.SuppressFinalize(this);
				}
		}
}
