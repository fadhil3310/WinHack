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
		public delegate HHOOK CreateLocalHookDelegate(int hookId, uint threadId);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate bool RemoveHookDelegate(WindowHookNativeResult hookData);


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
				private CreateLocalHookDelegate createLocalHook => (CreateLocalHookDelegate)_createLocalHook.Target!;
				private GCHandle _createLocalHook;
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
						var dgCreateLocalHook = library.GetProcAddressDelegate<CreateLocalHookDelegate>("CreateLocalHook");
						_createLocalHook = GCHandle.Alloc(dgCreateLocalHook, GCHandleType.Normal);

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
				public HHOOK CreateLocalHook(WINDOWS_HOOK_ID hookId, uint threadId)
				{
						if (!IsInitialized)
								throw new InvalidOperationException("Cannot call this method before initialization.");

						Debug.WriteLine("Attempt to create a local hook.");
						HHOOK hookData = createLocalHook((int)hookId, threadId);
						if (hookData.IsNull)
								ThrowWin32(true, "Failed creating a local hook.");
						return hookData;
				}

				///// <summary>
				///// Remove hook.
				///// </summary>
				///// <param name="hook"></param>
				//public void RemoveHook(WindowHookNativeResult hook)
				//{
				//		if (!IsInitialized)
				//				throw new InvalidOperationException("Cannot call this method before initialization.");

				//		if (!removeHook(hook))
				//				ThrowWin32(true, "Failed removing hook.");
				//}

				public void Dispose()
				{
						if (disposedValue) return;

						Library?.Dispose();
						_library.Free();

						_dllInitialize.Free();
						_createLocalHook.Free();
						_removeHook.Free();

						disposedValue = true;
						GC.SuppressFinalize(this);
				}
		}
}
