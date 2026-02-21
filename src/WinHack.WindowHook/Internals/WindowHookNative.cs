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
				/// <summary>
				/// The loader for the 32-bit surrogate process as the host for the 32-bit dll.
				/// </summary>
				public static NativeLoader32 Loader32 { get; private set; } = new();
				/// <summary>
				/// The loader for the 64-bit dll.
				/// </summary>
				public static NativeLoader64 Loader64 { get; private set; } = new();

				// ========================== End Static Properties/Fields ==========================


				// ========================== Public Local Properties/Fields ==========================

				public int HookId { get; private set; }
				public WINDOWS_HOOK_ID HookType { get; private set; }

				public bool IsInstalled => HookId != 0;

				// ========================== End Public Local Properties/Fields ==========================

				// ========================== Private Local Properties/Fields ==========================

				private enum WindowHookNativeState
				{
						Uninitialized,
						Installed,
						Removed
				}
				private WindowHookNativeState state = WindowHookNativeState.Uninitialized;

				private INativeLoader? loader;
				private Thread? pipeServerThread;

				// ========================== End Private Local Properties/Fields ==========================

				// ========================== Public Functions ==========================

				public WindowHookNative(WINDOWS_HOOK_ID hookType) 
				{
						HookType = hookType;
				}

				/// <summary>
				/// Create local hook.
				/// </summary>
				/// <typeparam name="T"></typeparam>
				/// <param name="window"></param>
				/// <param name="onMessageReceived"></param>
				/// <returns></returns>
				public void Install(HackWindow window, Func<int, WPARAM, byte[]?, int> onMessageReceived, Action? onEnded)
				{
						if (state != WindowHookNativeState.Uninitialized)
								throw new InvalidOperationException("Hook has been initialized.");

						var hookPipeName = WindowHookOptions.HookPipeName;
						var threadProcessId = window.GetThreadProcessID();
						var process = new HackProcess(threadProcessId.ProcessId, PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_INFORMATION);

						if (!process.Is64Bit())
						{
								Debug.WriteLine("Process is 32 bit");
								// Initialize the loader first if hasn't been initialized.
								if (!Loader32!.IsInitialized)
										Loader32.Initialize(hookPipeName);

								loader = Loader32;
								HookId = Loader32.CreateHook(HookType, threadProcessId.ThreadId);
						}
						else
						{
								Debug.WriteLine("Process is 64 bit");
								// Initialize the loader first if hasn't been initialized.
								if (!Loader64!.IsInitialized)
										Loader64.Initialize(hookPipeName);

								loader = Loader64;
								HookId = Loader64.CreateHook(HookType, threadProcessId.ThreadId);
						}

						Thread pipeServerThread = CreatePipeServer((uint)HookType, threadProcessId.ThreadId, onMessageReceived, onEnded);
						state = WindowHookNativeState.Installed;
				}

				public void Remove()
				{
						if (!IsInstalled)
								throw new InvalidOperationException("Hook isn't installed.");

						loader!.RemoveHook(HookId);
						HookId = 0;

						// TODO: Stop and dispose pipe.

						state = WindowHookNativeState.Removed;
				}

				// ========================== End Public Functions ==========================


				// ========================== Private Functions ==========================

				private Thread CreatePipeServer(uint hookType, uint threadId, Func<int, WPARAM, byte[]?, int> onMessageReceived, Action? onEnded)
				{
						var hookPipeName = WindowHookOptions.HookPipeName;

						if (string.IsNullOrEmpty(hookPipeName))
								throw new ArgumentException("Main Pipe Name can't be empty.");

						Thread thread = new(new ThreadStart(() =>
						{
								// Create pipe server.
								string pipeName = hookPipeName + hookType + "\\" + threadId;
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
