using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using WinHack.Core.Utility;
using WinHack.Core.Windowing;
using static WinHack.Core.Utility.Thrower;

namespace WinHack.Core.Accesibility.Magnification
{
		public class HkFullscreenMagnification
		{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
				private static HkFullscreenMagnification _instance;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

				private static readonly object _lock = new object();

				public static HkFullscreenMagnification Get()
				{
						if (_instance == null)
						{
								lock (_lock)
								{
										if (_instance == null)
										{
												_instance = new HkFullscreenMagnification();
										}
								}
						}
						return _instance;
				}


				private HkFullscreenMagnification()
				{
						if (!PInvoke.MagInitialize())
						{
								ThrowWin32(true, "Failed initializing Magnifier");
						}
						// TODO: Uninitialize on App shutdown.
				}


				public void SetMagnificationTransform(float factor, int xOffset, int yOffset)
				{
						PInvoke.MagSetFullscreenTransform(factor, xOffset, yOffset);
				}

				public void GetMagnificationTransform()
				{
						//PInvoke.MagGetFullscreenTransform()
				}

				public unsafe void SetWindowExclusionList(HackWindow mainWindow, Span<HWND> windows)
				{
						fixed (HWND* ptr = windows)
						{
								PInvoke.MagSetWindowFilterList(
										mainWindow,
										Windows.Win32.UI.Magnification.MW_FILTERMODE.MW_FILTERMODE_EXCLUDE,
										windows.Length,
										ptr);
						}
				}
				public unsafe void SetWindowExclusionList(HackWindow mainWindow, List<HackWindow> windows)
				{
						Span<HWND> hwndList = windows.Select(x => x.Handle).ToArray();
						SetWindowExclusionList(mainWindow, hwndList);
				}

				public void SetSystemCursorVisibility(bool visible)
				{
						PInvoke.MagShowSystemCursor(visible);
				}
		}
}
