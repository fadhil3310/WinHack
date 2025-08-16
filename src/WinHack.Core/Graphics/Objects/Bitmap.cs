using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.Storage.FileSystem;
using WinHack.Core.Graphics.DeviceContexts;
using static WinHack.Core.Utility.Thrower;

namespace WinHack.Core.Graphics.Objects
{
		public enum BITMAP_COMPRESSION : uint
		{
				BI_RGB = 0x0000,
				BI_RLE8 = 0x0001,
				BI_RLE4 = 0x0002,
				BI_BITFIELDS = 0x0003,
				BI_JPEG = 0x0004,
				BI_PNG = 0x0005,
				BI_CMYK = 0x000B,
				BI_CMYKRLE8 = 0x000C,
				BI_CMYKRLE4 = 0x000D
		}

		public record BitmapBufferData(
				byte[] Buffer, 
				int Width,
				int Height,
				BITMAPINFOHEADER RawInfo
		);

		public class Bitmap : IDisposable
		{
				public HBITMAP Handle { get; private set; }
				public DeviceContextBase DeviceContext { get; private set; }


				public Bitmap(HBITMAP hBitmap, DeviceContextBase deviceContext)
				{
						Handle = hBitmap;
						DeviceContext = deviceContext;
				}

				public unsafe BITMAP? GetRawBitmap(bool throwIfError = false)
				{
						BITMAP bitmap = new();
						if (PInvoke.GetObject(Handle, sizeof(BITMAP), &bitmap) == 0)
								if (!ThrowWin32(throwIfError, "Failed getting bitmap from HBITMAP."))
										return null;
						return bitmap;
				}

				public unsafe BitmapBufferData? GetData(bool throwIfError = false)
				{
						BITMAP? _bitmap = GetRawBitmap(throwIfError);
						if (_bitmap == null)
								return null;
						BITMAP bitmap = _bitmap.Value;

						BITMAPINFOHEADER info;
						info.biSize = (uint)sizeof(BITMAPINFOHEADER);
						info.biWidth = bitmap.bmWidth;
						info.biHeight = -bitmap.bmHeight;
						info.biPlanes = 1;
						info.biBitCount = 32;
						info.biCompression = (uint)BITMAP_COMPRESSION.BI_RGB;
						info.biSizeImage = 0;
						info.biXPelsPerMeter = 0;
						info.biYPelsPerMeter = 0;
						info.biClrUsed = 0;
						info.biClrImportant = 0;

						// Always allocate buffer on the heap, not on stack as max memory in stack is very limited,
						// you will get StackOverflowException just for allocating a single bitmap buffer (L for .NET)
						int bitmapSize = bitmap.bmWidth * bitmap.bmHeight * 4;
						byte[] buffer = new byte[bitmapSize];

						fixed (byte* bufferPtr = buffer)
						{
								if (PInvoke.GetDIBits(DeviceContext, Handle, 0, (uint)bitmap.bmHeight, lpvBits: bufferPtr, (BITMAPINFO*)&info, DIB_USAGE.DIB_RGB_COLORS) == 0)
										if (!ThrowWin32(throwIfError, "Failed copying bitmap data to buffer."))
												return null;
						}

						return new BitmapBufferData(buffer, bitmap.bmWidth, bitmap.bmHeight, info);
				}

				//public unsafe bool WriteDataToFile(bool throwIfError = false)
				//{
				//		BITMAP? _bitmap = GetRawBitmap(throwIfError);
				//		if (_bitmap == null)
				//				return false;
				//		BITMAP bitmap = _bitmap.Value;

				//		BITMAPFILEHEADER fileHeader;
				//		BITMAPINFOHEADER info;
				//		info.biSize = (uint)sizeof(BITMAPINFOHEADER);
				//		info.biWidth = bitmap.bmWidth;
				//		info.biHeight = bitmap.bmHeight;
				//		info.biPlanes = 1;
				//		info.biBitCount = 32;
				//		info.biCompression = (uint)BITMAP_COMPRESSION.BI_RGB;
				//		info.biSizeImage = 0;
				//		info.biXPelsPerMeter = 0;
				//		info.biYPelsPerMeter = 0;
				//		info.biClrUsed = 0;
				//		info.biClrImportant = 0;

				//		// Always allocate buffer on the heap, not on stack as max memory in stack is very limited,
				//		// you will get StackOverflowException just for allocating a single bitmap buffer (L for .NET)
				//		//int bitmapSize = ((bitmap.bmWidth * bitmap.bmHeight + 31) / 32) * 4 * bitmap.bmHeight;
				//		int bitmapSize = bitmap.bmWidth * bitmap.bmHeight * 4;
				//		byte[] buffer = new byte[bitmapSize];

				//		fixed (byte* bufferPtr = buffer)
				//		{
				//				if (PInvoke.GetDIBits(DeviceContext, Handle, 0, (uint)bitmap.bmHeight, lpvBits: bufferPtr, (BITMAPINFO*)&info, DIB_USAGE.DIB_RGB_COLORS) == 0)
				//						if (!ThrowWin32(throwIfError, "Failed copying bitmap data to buffer."))
				//								return false;

				//				fixed (char* fileNamePtr = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"/capture_aa.bmp")
				//				{
				//						PCWSTR fileName = new(fileNamePtr);
				//						var fileHandle = PInvoke.CreateFile(
				//								fileName,
				//								0x40000000,
				//								FILE_SHARE_MODE.FILE_SHARE_NONE,
				//								dwCreationDisposition: FILE_CREATION_DISPOSITION.CREATE_ALWAYS,
				//								dwFlagsAndAttributes: FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
				//								hTemplateFile: HANDLE.Null
				//						);

				//						uint fileHeaderSize = (uint)(bitmapSize + sizeof(BITMAPFILEHEADER) + sizeof(BITMAPINFOHEADER));
				//						fileHeader.bfOffBits = (uint)(sizeof(BITMAPFILEHEADER) + sizeof(BITMAPINFOHEADER));
				//						fileHeader.bfSize = fileHeaderSize;
				//						fileHeader.bfType = 0x4D42;

				//						uint bytesWritten;
				//						PInvoke.WriteFile(fileHandle, (byte*)&fileHeader, (uint)sizeof(BITMAPFILEHEADER), lpNumberOfBytesWritten: &bytesWritten);
				//						PInvoke.WriteFile(fileHandle, (byte*)&info, (uint)sizeof(BITMAPINFOHEADER), lpNumberOfBytesWritten: &bytesWritten);
				//						PInvoke.WriteFile(fileHandle, bufferPtr, (uint)bitmapSize, lpNumberOfBytesWritten: &bytesWritten);

				//						PInvoke.CloseHandle(fileHandle);
				//				}
				//		}

				//		return true;
				//}

				public void Dispose()
				{
						if (!Handle.IsNull)
								PInvoke.DeleteObject(Handle);
						GC.SuppressFinalize(this);
				}
		}
}
