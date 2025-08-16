using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using WinHack.Core.Base;
using WinHack.Core.Graphics;
using WinHack.Core.Graphics.DeviceContexts;
using static WinHack.Core.Utility.Thrower;

using BitmapBufferData = WinHack.Core.Graphics.Objects.BitmapBufferData;

namespace WinHack.Core.Windowing
{
		public class HackWindow : IWinHackDisposable
		{
				private bool disposedValue;

				public HWND Handle { get; set; }
				public string? Title { get => HWNDUtility.GetTitle(Handle); }
				public string? ClassName { get => HWNDUtility.GetClassName(Handle); }

				public ObservableCollection<HackWindow> Children { get; private set; } = [];


				private readonly List<IDisposable> retainedResources = [];


				public HackWindow(HWND hwnd)
				{
						Handle = hwnd;
				}

				// ================= PUBLIC FUNCTIONS =================
				public async void EnumerateChildren(bool recursive = false)
				{
						Children.Clear();
						foreach (var child in await HackWindowEnumerator.GetWindowChildren(Handle, recursive))
						{
								Children.Add(child);
						}
				}

				public RECT GetDimensions(bool throwIfError = false)
				{
						return HWNDUtility.GetDimensions(Handle, throwIfError);
				}

				public bool SetParent(HWND parent, bool throwIfError = false)
				{
						if (PInvoke.SetParent(Handle, parent).IsNull)
								return ThrowWin32(throwIfError, "Failed setting window parent.");
						return true;
				}

				public void Minimize()
				{
						PInvoke.ShowWindow(Handle, SHOW_WINDOW_CMD.SW_MINIMIZE);
				}

				public void Maximize()
				{
						PInvoke.ShowWindow(Handle, SHOW_WINDOW_CMD.SW_MAXIMIZE);
				}

				public void Restore()
				{
						PInvoke.ShowWindow(Handle, SHOW_WINDOW_CMD.SW_RESTORE);
				}

				public void Hide()
				{
						PInvoke.ShowWindow(Handle, SHOW_WINDOW_CMD.SW_HIDE);
				}

				public void Show()
				{
						PInvoke.ShowWindow(Handle, SHOW_WINDOW_CMD.SW_SHOW);
				}

				public unsafe ThreadProcessId GetThreadProcessID()
				{
						uint processId;
						uint threadId = PInvoke.GetWindowThreadProcessId(Handle, &processId);
						if (threadId == 0)
								ThrowWin32(
										true,
										"Failed getting window thread process ID, it may be because of the window handle is invalid."
								);
						return new ThreadProcessId(threadId, processId);
				}

				// ---------------- Position ----------------

				/// <summary>
				/// Set the window position
				/// </summary>
				/// <param name="x">The X position</param>
				/// <param name="y">The Y Position</param>
				/// <returns></returns>
				public bool SetPosition(int x, int y, SET_WINDOW_POS_FLAGS? additionalFlags = null, bool throwIfError = false)
				{
						var flags = SET_WINDOW_POS_FLAGS.SWP_NOSIZE;
						if (additionalFlags != null)
								flags |= (SET_WINDOW_POS_FLAGS)additionalFlags;
						if (PInvoke.SetWindowPos(Handle, HWND.Null, x, y, 0, 0, flags) == 0)
								return ThrowWin32(throwIfError, "Failed setting position");
						return true;
				}

				/// <summary>
				/// Sets the window position with all of the parameters from Win32 SetWindowPos.
				/// <br/><br/>
				/// <strong>
				/// Use only when you need direct access to SetWindowPos without abstraction.
				/// </strong>
				/// <br/>
				/// For most use cases, prefer the higher-level methods like 
				/// <see cref="SetPosition(int, int)"/> or <see cref="SetZOrderTop"/> instead.
				/// </summary>
				/// <param name="x">The X position.</param>
				/// <param name="y">The Y position.</param>
				/// <param name="insertAfter">The HWND (Constants like HWND_TOPMOST are available inside the HWND struct).</param>
				/// <param name="width">The width</param>
				/// <param name="height">The height</param>
				/// <param name="flag">The flag</param>
				/// <returns></returns>
				public bool SetPositionRaw(int x, int y, HWND insertAfter, int width, int height, SET_WINDOW_POS_FLAGS flag, bool throwIfError = false)
				{
						if (PInvoke.SetWindowPos(Handle, insertAfter, x, y, width, height, flag) == 0)
								return ThrowWin32(throwIfError, "Failed setting raw position.");
						return true;
				}

				/// <summary>
				/// If enabled, set the window Z order to be always on top of all non-topmost windows.
				/// </summary>
				/// <param name="enable">Enable?</param>
				/// <returns></returns>
				public bool SetTopmost(bool enable = true, SET_WINDOW_POS_FLAGS? additionalFlags = null, bool throwIfError = false)
				{
						var flags = SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOMOVE;
						if (additionalFlags != null)
								flags |= (SET_WINDOW_POS_FLAGS)additionalFlags;
						if (PInvoke.SetWindowPos(Handle, enable ? HWND.HWND_TOPMOST : HWND.HWND_NOTOPMOST, 0, 0, 0, 0, flags) == 0)
								return ThrowWin32(throwIfError, "Failed setting window topmost position.");
						return true;
				}
				
				/// <summary>
				/// Place the window on top of all non-topmost windows (but below of topmost windows).
				/// </summary>
				/// <returns></returns>
				public bool SetZOrderTop(SET_WINDOW_POS_FLAGS? additionalFlags, bool throwIfError = false)
				{
						var flags = SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOMOVE;
						if (additionalFlags != null)
								flags |= (SET_WINDOW_POS_FLAGS)additionalFlags;
						if (PInvoke.SetWindowPos(Handle, HWND.HWND_TOP, 0, 0, 0, 0, flags) == 0)
								return ThrowWin32(throwIfError, "Failed placing window above all non-topmost windows.");
						return true;
				}

				/// <summary>
				/// Place the window at the bottom of all non-topmost windows.
				/// </summary>
				/// <returns></returns>
				public bool SetZOrderBottom(SET_WINDOW_POS_FLAGS? additionalFlags, bool throwIfError = false)
				{
						var flags = SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOMOVE;
						if (additionalFlags != null)
								flags |= (SET_WINDOW_POS_FLAGS)additionalFlags;
						if (PInvoke.SetWindowPos(Handle, HWND.HWND_BOTTOM, 0, 0, 0, 0, flags) == 0)
								return ThrowWin32(throwIfError, "Failed placing window below all non-topmost windows.");
						return true;
				}

				/// <summary>
				/// Place the window on top of another window.
				/// </summary>
				/// <param name="window">The window reference</param>
				/// <returns></returns>
				public bool SetZOrderAbove(HWND window, SET_WINDOW_POS_FLAGS? additionalFlags, bool throwIfError = false)
				{
						var flags = SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOMOVE;
						if (additionalFlags != null)
								flags |= (SET_WINDOW_POS_FLAGS)additionalFlags;
						if (PInvoke.SetWindowPos(Handle, window, 0, 0, 0, 0, flags) == 0)
								return ThrowWin32(throwIfError, "Failed placing window on top of another window.");
						return true;
				}

				/// <summary>
				/// Place the window below another window.
				/// </summary>
				/// <param name="window">The window</param>
				/// <returns></returns>
				public bool SetZOrderBelow(HWND window, SET_WINDOW_POS_FLAGS? additionalFlags = null, bool throwIfError = false)
				{
						var flags = SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOMOVE;
						if (additionalFlags != null)
								flags |= (SET_WINDOW_POS_FLAGS)additionalFlags;
						if (PInvoke.SetWindowPos(Handle, window, 0, 0, 0, 0, flags) == 0)
								return ThrowWin32(throwIfError, "Failed placing window below of another window.");
						return true;
				}

				// ---------------- End Position ----------------


				public BitmapBufferData? GetImage(bool throwIfError = false)
				{
						using var windowDC = new WindowDeviceContext(this);
						using var memoryDC = MemoryDeviceContext.CreateFromDeviceContext(windowDC);
						using Graphic drawer = new(windowDC);

						var size = GetDimensions();
						drawer.BlitTo(memoryDC, 0, 0, 0, 0, size.Width, size.Height, throwIfError: throwIfError);

						var buffer = memoryDC.Bitmap!.GetData(throwIfError);
						return buffer;
				}

				// ================= END PUBLIC FUNCTIONS =================


				// ================= PRIVATE FUNCTIONS =================

				public void RetainResources(params IDisposable[] resources)
				{
						foreach (var item in resources)
						{
								retainedResources.Add(item);
						}
				}

				public void ReleaseResources(params IDisposable[] resources)
				{
						foreach (var item in resources)
						{
								retainedResources.Remove(item);
						}
				}

				public void Dispose()
				{
						if (disposedValue) return;

						foreach (var resource in retainedResources)
						{
								resource.Dispose();
						}

						disposedValue = true;
						GC.SuppressFinalize(this);
				}

				public static readonly HackWindow PRIMARY_MONITOR = new(HWND.Null);
				public static implicit operator HWND(HackWindow window) => window.Handle;
		}

		public record ThreadProcessId(uint ThreadId, uint ProcessId);
}
