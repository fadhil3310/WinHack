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
using WinHack.WindowHook.Interop.Loader;
using WinHack.Core.Windowing;
using System.IO.Pipes;
using System.Diagnostics;

namespace WinHack.WindowHook.Interop
{
		public sealed class WindowHookLowLevel : IDisposable
		{
				bool disposedValue;

				// ==================== Singleton ====================
				private static readonly Lazy<WindowHookLowLevel> lazy =
						new Lazy<WindowHookLowLevel>(() => new WindowHookLowLevel());
				public static WindowHookLowLevel Instance { get { return lazy.Value; } }
				private unsafe WindowHookLowLevel()
				{
						Loader32 = new LowLevelLoader32();
						Loader64 = new LowLevelLoader64();
				}
				// ================== End Singleton ==================


				public string HookPipeName
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
				private string _hookPipeName = "";

				/// <summary>
				/// The loader for the 32-bit surrogate process as the host for the 32-bit dll.
				/// </summary>
				public LowLevelLoader32 Loader32 { get; private set; }
				/// <summary>
				/// The loader for the 64-bit dll.
				/// </summary>
				public LowLevelLoader64 Loader64 { get; private set; }


				// ========================== Public Functions ==========================

				/// <summary>
				/// Create local hook.
				/// </summary>
				/// <typeparam name="T"></typeparam>
				/// <param name="hookType"></param>
				/// <param name="window"></param>
				/// <param name="onMessageReceived"></param>
				/// <returns></returns>
				public WindowHookData CreateLocalHook<T>(WINDOWS_HOOK_ID hookType, HackWindow window, Func<int, T?, int> onMessageReceived, Action? onEnded)
				{
						WindowHookData? hookData = null;
						var threadProcessId = window.GetThreadProcessID();
						var process = new HackProcess(threadProcessId.ProcessId, PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_INFORMATION);

						HHOOK? hHook;
						if (!process.Is64Bit())
						{
								Debug.WriteLine("Process is 32 bit");
								// Initialize the loader first if hasn't been initialized.
								if (!Loader32.IsInitialized)
										Loader32.Initialize(_hookPipeName);

								hHook = Loader32.CreateLocalHook(hookType, threadProcessId.ThreadId);
						}
						else
						{
								Debug.WriteLine("Process is 64 bit");
								// Initialize the loader first if hasn't been initialized.
								if (!Loader64.IsInitialized)
										Loader64.Initialize(_hookPipeName);

								hHook = Loader64.CreateLocalHook(hookType, threadProcessId.ThreadId);
						}

						Thread pipeServerThread = CreatePipeServer((uint)hookType, threadProcessId.ThreadId, onMessageReceived, onEnded);
						hookData = new WindowHookData((HHOOK)hHook, pipeServerThread);

						return hookData!;
				}

				// ========================== End Public Functions ==========================


				// ========================== Private Functions ==========================

				private Thread CreatePipeServer<T>(uint hookType, uint threadId, Func<int, T?, int> onMessageReceived, Action? onEnded)
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

										PipeStreamProcessor<T> processor = new(pipeServer);

										while (true)
										{
												T? clientMessage = processor.WaitMessage(out int nCode);
												Debug.WriteLine($"Got message from client: {clientMessage}");

												int sendMessage = onMessageReceived(nCode, clientMessage);
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

				public void Dispose()
				{
						if (disposedValue) return;

						Loader64.Dispose();

						disposedValue = true;
				}

				private class PipeStreamProcessor<T>
				{
						private Stream pipeStream;

						public PipeStreamProcessor(Stream pipeStream)
						{
								this.pipeStream = pipeStream;
						}

						public T? WaitMessage(out int nCode)
						{
								// Hook code.
								byte[] nCodeBuffer = new byte[sizeof(int)];
								if (pipeStream.Read(nCodeBuffer, 0, sizeof(int)) == 0)
								{
										Debug.WriteLine($"Failed reading nCode");
										nCode = -1;
										return default;
								}
								nCode = BitConverter.ToInt32(nCodeBuffer, 0);
								//Debug.WriteLine($"nCode: {nCode}");

								// The size of the lParam.
								byte[] lParamSizeBuffer = new byte[sizeof(int)];
								if (pipeStream.Read(lParamSizeBuffer, 0, sizeof(int)) == 0)
								{
										Debug.WriteLine($"Failed reading lParamSize");
										return default;
								}
								int lParamSize = BitConverter.ToInt32(lParamSizeBuffer, 0);
								//Debug.WriteLine($"lParamSize: {lParamSize}");

								// The lParam bytes buffer.
								byte[] lParamBuffer = new byte[lParamSize];
								int lParamReadSize = pipeStream.Read(lParamBuffer, 0, lParamSize);
								if (lParamReadSize == 0 || lParamReadSize < lParamSize)
								{
										Debug.WriteLine($"Failed reading lParam, read size: {lParamReadSize}");
										return default;
								}

								// Deserialize lParam.
								unsafe
								{
										if (lParamBuffer.Length < sizeof(T))
												throw new InvalidOperationException("Buffer too small.");

										fixed (byte* ptr = lParamBuffer)
										{
												T lParam = *(T*)ptr;
												return lParam;
										}
								}
						}

						public void SendMessage(int message)
						{
								pipeStream.WriteByte((byte)message);
						}
				}
		}
}
