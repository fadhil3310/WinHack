using Windows.Win32;
using Windows.Win32.Foundation;
using static WinHack.Core.Utility.Thrower;

namespace WinHack.Core.Windowing
{
  public static class HWNDUtility
  {
    /// <summary>
    /// Get window caption
    /// </summary>
    /// <param name="hwnd"></param>
    /// <returns>The window caption if success, null if failed</returns>
    public static string? GetTitle(HWND hwnd)
    {
      unsafe
      {
        int titleRawLength = PInvoke.GetWindowTextLength(hwnd) + 1;
        fixed (char* titleRaw = new char[titleRawLength])
        {
          if (PInvoke.GetWindowText(hwnd, titleRaw, titleRawLength) == 0)
          {
            return null;
          }
          else
          {
            return new string(titleRaw);
          }
        }
      }
    }

    /// <summary>
    /// Get window class name
    /// </summary>
    /// <param name="hwnd"></param>
    /// <returns>The window class name if success, null if failed</returns>
    public static string? GetClassName(HWND hwnd)
    {
      unsafe
      {
        fixed (char* classNameRaw = new char[255])
        {
          if (PInvoke.GetClassName(hwnd, classNameRaw, 255) == 0)
          {
            return null;
          }
          else
          {
            return new string(classNameRaw);
          }
        }
      }
    }

    public static RECT GetDimensions(HWND hwnd, bool throwIfError = false)
    {
      if (PInvoke.GetWindowRect(hwnd, out RECT rect) == 0)
        if (!ThrowWin32(throwIfError, "Failed getting window dimensions."))
          return new RECT();
      return rect;
    }
  }
}
