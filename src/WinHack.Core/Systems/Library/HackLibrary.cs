using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using WinHack.Core.Base;
using WinHack.Core.Utility;

using static WinHack.Core.Utility.Thrower;

namespace WinHack.Core.Systems.Library
{
		public class HackLibrary : IWinHackDisposable
		{
				bool disposedValue;
				List<IDisposable> disposables = [];

				public HMODULE Handle => (HMODULE)_handle.Target!;
				GCHandle _handle;

				// Get library path on demand when Library class is created
				// using only the library's Handle
				public string? _libraryPath;
				public string LibraryPath
				{
						get
						{
								if (_libraryPath == null)
								{
										unsafe
										{
												char[] libraryPath = new char[255];
												fixed (char* ptr = libraryPath)
												{
														if (PInvoke.GetModuleFileName(Handle, ptr, 255 * sizeof(char)) == 0)
																ThrowWin32(true, "Failed getting library path.");
												}

												string strLibraryPath = new string(libraryPath);
												_libraryPath = strLibraryPath;
												return strLibraryPath;
										}
								}

								return _libraryPath;
						}
				}

				public HackLibrary(string libraryPath) 
				{
						HMODULE handle = PInvoke.LoadLibrary(PointerUtility.StringToPCWSTR(libraryPath));
						if (handle.IsNull)
								ThrowWin32(true, "Failed loading library", libraryPath);
						_handle = GCHandle.Alloc(handle, GCHandleType.Pinned);
				}
				public HackLibrary(HMODULE libraryHandle) 
				{
						_handle = GCHandle.Alloc(libraryHandle, GCHandleType.Pinned);
				}

				public FARPROC GetProcAddress(string name)
				{
						FARPROC lpProc = PInvoke.GetProcAddress(Handle, PointerUtility.StringToPCSTR(name));
						if (lpProc.IsNull)
								ThrowWin32(true, "Failed getting procedure address", name);
						return lpProc;
				}

				public T GetProcAddressDelegate<T>(string name) where T : Delegate
				{
						FARPROC lpProc = PInvoke.GetProcAddress(Handle, PointerUtility.StringToPCSTR(name));
						if (lpProc.IsNull)
								ThrowWin32(true, "Failed getting procedure address", name);
						T procDelegate = lpProc.CreateDelegate<T>();
						return procDelegate;
				}

				// ======================= DISPOSE =======================
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
						PInvoke.FreeLibrary(Handle);
						_handle.Free();

						disposedValue = true;
						GC.SuppressFinalize(this);
				}

				public static implicit operator HMODULE(HackLibrary library) => library.Handle;
		}
}
