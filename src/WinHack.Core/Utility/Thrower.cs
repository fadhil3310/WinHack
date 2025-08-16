using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using WinHack.Core.Global;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Diagnostics;

namespace WinHack.Core.Utility
{
		public class WinHackCoreException : Win32Exception
		{
				private const int E_FAIL = unchecked((int)0x80004005);

				public string? AdditionalMessage { get; private set; }
				public WinHackCoreException(int errorCode, string message, string? additionalMessage = null)
						: base(errorCode, message)
				{
						AdditionalMessage = additionalMessage;
				}

				/// <summary>
				/// Returns a string that contains the <see cref="NativeErrorCode"/>, or <see cref="Exception.HResult"/>, or both.
				/// <br/>
				/// Copied and modified from .NET's Win32Exception code, MIT Licensed
				/// </summary>
				/// <returns>A string that represents the <see cref="NativeErrorCode"/>, or <see cref="Exception.HResult"/>, or both.</returns>
				public override string ToString()
				{
						//if (NativeErrorCode == 0 || NativeErrorCode == HResult)
						//{
						//		return base.ToString();
						//}

						string message = Message;
						string? additionalMessage = AdditionalMessage;
						Debug.WriteLine($"Is empty? {additionalMessage}");
						string className = GetType().ToString();
						StringBuilder s = new StringBuilder(className);
						string nativeErrorString = NativeErrorCode < 0
										? $"0x{NativeErrorCode:X8}"
										: NativeErrorCode.ToString(CultureInfo.InvariantCulture);
						if (HResult == E_FAIL)
						{
								s.Append($" ({nativeErrorString})");
						}
						else
						{
								s.Append($" ({HResult:X8}, {nativeErrorString})");
						}

						if (!(string.IsNullOrEmpty(message)))
						{
								s.Append(": ");
								s.Append(message);
						}

						if (!(string.IsNullOrEmpty(additionalMessage)))
						{
								s.Append(" [Additional message: ");
								s.Append(additionalMessage);
								s.Append("]");
						}

						Exception? innerException = InnerException;
						if (innerException != null)
						{
								s.Append(" ---> ");
								s.Append(innerException.ToString());
						}

						string? stackTrace = StackTrace;
						if (stackTrace != null)
						{
								s.AppendLine();
								s.Append(stackTrace);
						}

						return s.ToString();
				}
		}

		public static class Thrower
		{
				public static bool ThrowInvalidOperation(bool throwIfError, string message)
				{
						//bool throwIfError = WinHackSettings.Get().ThrowIfError;
						if (throwIfError)
								throw new InvalidOperationException(message);
						else
								return false;
				}

				public static bool ThrowWin32(
						bool throwIfError, string message, string? additionalMessage = null, int? errorCode = null)
				{
						//bool throwIfError = WinHackSettings.Get().ThrowIfError;
						if (throwIfError)
								throw new WinHackCoreException(
										errorCode ?? Marshal.GetLastWin32Error(),
										message, 
										additionalMessage
								);
						else
								return false;
				}
		}
}
