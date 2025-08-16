using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace WinHack.Core.Windowing
{
		static public class HackWindowEnumerator
		{
				/// <summary>
				/// Get all top-level windows
				/// </summary>
				/// <param name="queryChild">If true, also query all of the windows children and its descendant</param>
				/// <returns>The list of all the queried windows</returns>
				static public async Task<Collection<HackWindow>> GetTopLevelWindows(bool queryChild = false)
				{
						Collection<HackWindow> windows = [];

						await Task.Run(() =>
						{
								PInvoke.EnumWindows((HWND hwnd, LPARAM param) =>
								{
										HackWindow window = new(hwnd);

										if (queryChild)
										{
												window.EnumerateChildren();
										}

										windows.Add(window);
										return true;
								}, 0);
						});

						return windows;
				}

				/// <summary>
				/// Get children of the provided window
				/// </summary>
				/// <param name="parentWindow">The window</param>
				/// <param name="recursive">If true, also query all of the child's descendant</param>
				/// <returns>The window's children</returns>
				public static async Task<Collection<HackWindow>> GetWindowChildren(HackWindow parentWindow, bool recursive = false)
				{
						HWND parentHwnd = parentWindow.Handle;
						Collection<HackWindow> windows = await GetWindowChildren(parentHwnd, recursive);

						return windows;
				}

				/// <summary>
				/// Get children of the provided window (using HWND)
				/// </summary>
				/// <param name="parentWindow">The window</param>
				/// <param name="recursive">If true, also query all of the child's descendant</param>
				/// <returns>The window's children</returns>
				public static async Task<Collection<HackWindow>> GetWindowChildren(HWND parentHwnd, bool recursive = false)
				{
						Collection<HackWindow> windows = [];

						await Task.Run(() =>
						{
								HWND hwnd = PInvoke.GetWindow(parentHwnd, GET_WINDOW_CMD.GW_CHILD);

								while (!hwnd.IsNull && PInvoke.IsWindow(hwnd))
								{
										Collection<HackWindow> children = [];

										HackWindow window = new HackWindow(hwnd);
										if (recursive)
												window.EnumerateChildren(true);
										windows.Add(window);

										hwnd = PInvoke.GetWindow(hwnd, GET_WINDOW_CMD.GW_HWNDNEXT);
								}
						});

						return windows;
				}


				//static public async Task<List<WindowData>> GetChildWindows(HWND hwndParent, bool nullIfError = true)
				//{
				//		List<WindowData> windows = [];

				//		await Task.Run(() =>
				//		{
				//				PInvoke.EnumChildWindows(hwndParent, (HWND hwnd, LPARAM param) =>
				//				{
				//						string? title = null;
				//						string? className = null;

				//						unsafe
				//						{
				//								int titleRawLength = PInvoke.GetWindowTextLength(hwnd) + 1;
				//								fixed (char* titleRaw = new char[titleRawLength])
				//								{
				//										if (PInvoke.GetWindowText(hwnd, titleRaw, titleRawLength) == 0)
				//										{
				//												if (!nullIfError)
				//														//throw new InvalidOperationException("Failed when getting window title");
				//														return true;
				//										}
				//										else
				//										{
				//												title = new string(titleRaw);
				//										}
				//								}
				//								fixed (char* classNameRaw = new char[255])
				//								{
				//										if (PInvoke.GetClassName(hwnd, classNameRaw, 255) == 0)
				//										{
				//												if (!nullIfError)
				//														//throw new InvalidOperationException("Failed when getting window class name");
				//														return true;
				//										}
				//										else
				//										{
				//												className = new string(classNameRaw);
				//										}
				//								}
				//						}

				//						WindowData window = new()
				//						{
				//								HWND = hwnd,
				//								Title = title,
				//								ClassName = className
				//						};
				//						windows.Add(window);
				//						return true;
				//				}, 0);
				//		});

				//		return windows;
				//}
		}
}
