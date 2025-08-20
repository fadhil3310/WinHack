using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using WinHack.Core.Utility;
using Windows.Win32.System.Threading;
using WinHack.Core.Systems.Process;
using WinHack.WindowHook.Internals.NativeLoader;
using WinHack.Core.Windowing;
using System.IO.Pipes;
using System.Diagnostics;

using static WinHack.Core.Utility.Thrower;

namespace WinHack.WindowHook.Internals
{
		public sealed class WindowHookNative
		{
				//static bool IsDisposed;

				// ==================== Singleton ====================
				//private static readonly Lazy<WindowHookNative> lazy =
				//		new Lazy<WindowHookNative>(() => new WindowHookNative());
				//public static WindowHookNative Instance { get { return lazy.Value; } }
				//private unsafe WindowHookNative()
				//{
				//		Loader32 = new NativeLoader32();
				//		Loader64 = new NativeLoader64();
				//}
				// ================== End Singleton ==================


				// ========================== Static Properties/Fields ==========================

				public static string HookPipeName
				{
						get => _hookPipeName;
						set
						{
								if (Loader32.IsInitialized || Loader64.IsInitialized)
										throw new InvalidOperationException("Can't change hook pipe name if one of the loader has been initialized.");
								if (value.Length > 247)
										throw new ArgumentException("Hook pipe name can't be longer than 247 characters.");

								_hookPipeName = value;
						}
				}
				private static string _hookPipeName = "";

				/// <summary>
				/// The loader for the 32-bit surrogate process as the host for the 32-bit dll.
				/// </summary>
				public static NativeLoader32 Loader32 { get; private set; } = new();
				/// <summary>
				/// The loader for the 64-bit dll.
				/// </summary>
				public static NativeLoader64 Loader64 { get; private set; } = new();

				// ========================== End Static Properties/Fields ==========================


				// ========================== Local Properties/Fields ==========================

				public HHOOK HHOOK { get; private set; }
				public WINDOWS_HOOK_ID HookId { get; private set; }
				public Thread? PipeServerThread { get; private set; }

				public bool IsInstalled => !HHOOK.IsNull;

				// ========================== End Local Properties/Fields ==========================


				// ========================== Public Functions ==========================

				public WindowHookNative(WINDOWS_HOOK_ID hookId)
				{
						HookId = hookId;
				}

				/// <summary>
				/// Create local hook.
				/// </summary>
				/// <typeparam name="T"></typeparam>
				/// <param name="window"></param>
				/// <param name="onMessageReceived"></param>
				/// <returns></returns>
				public void InstallLocal(HackWindow Window, Func<int, WPARAM, byte[]?, int> onMessageReceived, Action? onEnded)
				{
						var threadProcessId = Window.GetThreadProcessID();
						var process = new HackProcess(threadProcessId.ProcessId, PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_INFORMATION);

						HHOOK? hHook;
						if (!process.Is64Bit())
						{
								Debug.WriteLine("Process is 32 bit");
								// Initialize the loader first if hasn't been initialized.
								if (!Loader32!.IsInitialized)
										Loader32.Initialize(_hookPipeName);

								hHook = Loader32.CreateLocalHook(HookId, threadProcessId.ThreadId);
						}
						else
						{
								Debug.WriteLine("Process is 64 bit");
								// Initialize the loader first if hasn't been initialized.
								if (!Loader64!.IsInitialized)
										Loader64.Initialize(_hookPipeName);

								hHook = Loader64.CreateLocalHook(HookId, threadProcessId.ThreadId);
						}

						PipeServerThread = CreatePipeServer((uint)HookId, threadProcessId.ThreadId, onMessageReceived, onEnded);
				}

				public void Remove()
				{
						if (!IsInstalled)
								throw new InvalidOperationException("Hook isn't installed.");

						if (!WHookPI.UnhookWindowsHookEx(HHOOK))
								ThrowWin32(true, "Failed removing hook.");
				}

				// ========================== End Public Functions ==========================


				// ========================== Private Functions ==========================

				private Thread CreatePipeServer(uint hookType, uint threadId, Func<int, WPARAM, byte[]?, int> onMessageReceived, Action? onEnded)
				{
						if (string.IsNullOrEmpty(_hookPipeName))
								throw new ArgumentException("Main Pipe Name can't be empty.");

						Thread thread = new(new ThreadStart(() =>
						{
								// Create pipe server.
								string pipeName = _hookPipeName + hookType + "\\" + threadId;
								Debug.WriteLine($"Pipe name: {pipeName}");

								NamedPipeServerStream pipeServer = new(
										pipeName, 
										PipeDirection.InOut, 
										1, 
										PipeTransmissionMode.Byte);

								try
								{
										// Wait for the client to connect.
										Debug.WriteLine("Waiting for connection from client");
										pipeServer.WaitForConnection();
										Debug.WriteLine("Client connected!");

										PipeStreamProcessor processor = new(pipeServer);

										while (true)
										{
												byte[]? clientMessage = processor.WaitMessage(out int nCode, out WPARAM wParam);
												Debug.WriteLine($"Got message from client: {clientMessage}");

												int sendMessage = onMessageReceived(nCode, wParam, clientMessage);
												processor.SendMessage(sendMessage);
												Debug.WriteLine($"Message sent to client");
										}
								}
								catch (Exception e)
								{
								}

								onEnded?.Invoke();
						}));
						thread.Start();
						return thread;
				}

				// ========================== End Private Functions ==========================

				//public static void Dispose()
				//{
				//		if (IsDisposed) return;

				//		Loader64.Dispose();

				//		IsDisposed = true;
				//}

				private class PipeStreamProcessor
				{
						private Stream pipeStream;

						public PipeStreamProcessor(Stream pipeStream)
						{
								this.pipeStream = pipeStream;
						}

						public byte[]? WaitMessage(out int nCode, out WPARAM wParam)
						{
								int _nCode = -1;
								uint _wParam = 0;

								try
								{
										BinaryReader reader = new(pipeStream);
										_nCode = reader.ReadInt32();
										_wParam = reader.ReadUInt32();

										int lParamSize = (int)reader.ReadUInt32();

										// The lParam bytes buffer.
										byte[] lParamBuffer = new byte[lParamSize];
										int lParamReadSize = pipeStream.Read(lParamBuffer, 0, lParamSize);
										if (lParamReadSize == 0 || lParamReadSize < lParamSize)
										{
												throw new IOException("Failed reading lParam.");
										}

										nCode = _nCode;
										wParam = _wParam;
										return lParamBuffer;
								}
								catch (Exception e)
								{
										Debug.WriteLine($"Failed reading message, reason: {e.Message} {e.StackTrace}");

										nCode = _nCode;
										wParam = _wParam;
										return null;
								}
						}

						public void SendMessage(int message)
						{
								pipeStream.WriteByte((byte)message);
						}
				}
		}
}
