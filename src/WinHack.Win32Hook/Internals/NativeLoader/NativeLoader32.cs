using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.UI.WindowsAndMessaging;

namespace WinHack.WindowHook.Internals.NativeLoader
{
		public class NativeLoader32 : INativeLoader
		{
				private bool disposedValue;

				private enum SurrogateRequestType : byte
				{
						CreateLocalHook = 0,
						CreateGlobalHook = 1,
						RemoveHook = 2
				};

				/// <summary>
				/// Is initialized?.
				/// </summary>
				public bool IsInitialized => _surrogateProcess != null;

				/// <summary>
				/// The process to bridge the communication between
				/// the .NET code and the 32-bit hook DLL.
				/// </summary>
				public Process? SurrogateProcess => _surrogateProcess;
				private Process? _surrogateProcess;

				/// <summary>
				/// The path of the dll. 
				/// <br />
				/// <strong>Can only be changed before initialized.</strong>
				/// </summary>
				/// <param name="path"></param>
				/// <exception cref="InvalidOperationException"></exception>
				public string SurrogatePath
				{
						get => _surrogatePath;
						set
						{
								if (IsInitialized)
										throw new InvalidOperationException("Surrogate process has been launched.");
								if (string.IsNullOrEmpty(value))
										throw new ArgumentException("Value can't be empty.");

								_surrogatePath = value;
						}
				}
				private string _surrogatePath = "WinHack.WindowHook.NativeSurrogate.exe";

				/// <summary>
				/// The surrogate's pipe server name. 
				/// <br />
				/// <strong>Can only be changed before initialized.</strong>
				/// </summary>
				/// <param name="path"></param>
				/// <exception cref="InvalidOperationException"></exception>
				public string SurrogatePipeName
				{
						get => _surrogatePipeName;
						set
						{
								if (IsInitialized)
										throw new InvalidOperationException("Surrogate process has been launched.");
								if (string.IsNullOrEmpty(value))
										throw new ArgumentException("Value can't be empty.");

								_surrogatePipeName = value;
						}
				}
				private string _surrogatePipeName = "";

				/// <summary>
				/// The path of the dll. 
				/// <br />
				/// <strong>Can only be changed before initialized.</strong>
				/// </summary>
				/// <param name="path"></param>
				/// <exception cref="InvalidOperationException"></exception>
				public string LibraryPath
				{
						get => _libraryPath;
						set
						{
								if (IsInitialized)
										throw new InvalidOperationException("Surrogate process has been launched.");
								if (string.IsNullOrEmpty(value))
										throw new ArgumentException("Value can't be empty.");

								_libraryPath = value;
						}
				}
				private string _libraryPath = "WinHack.WindowHook.Native32.dll";

				/// <summary>
				/// The pipe client to connect to the pipe server.
				/// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
				private NamedPipeClientStream pipeClient;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.


				// ============================ Public Functions ============================

				/// <summary>
				/// Start.
				/// </summary>
				/// <param name="mainPipeName"></param>
				public void Initialize(string hookPipeName)
				{
						if (string.IsNullOrEmpty(_surrogatePipeName) || string.IsNullOrEmpty(hookPipeName))
								throw new ArgumentException("Surrogate Pipe Name can't be empty.");

						_surrogateProcess = new Process
						{
								StartInfo = new ProcessStartInfo
								{
										FileName = _surrogatePath,
										Arguments = $"{hookPipeName} {_surrogatePipeName} {_libraryPath}",
										CreateNoWindow = false,
								}
						};
						_surrogateProcess.Exited += SurrogateExited;
						_surrogateProcess.Start();

						CreatePipeClient();
				}

				public HHOOK CreateLocalHook(WINDOWS_HOOK_ID hookId, uint threadId)
				{
						if (!IsInitialized)
								throw new InvalidOperationException("Cannot call this method before initialization.");

						// Tell the surrogate that we want to create a local hook.
						SurrogateSendRequest(SurrogateRequestType.CreateLocalHook, (int)hookId, threadId);

						// Read the answer from the surrogate.
						unsafe
						{
								// Read the size of the HHOOK.
								byte[] _hHookSize = new byte[sizeof(uint)];
								if (pipeClient.Read(_hHookSize, 0, _hHookSize.Length) == 0)
										throw new IOException("Failed reading the HHOOK size sent by the surrogate.");
								uint hHookSize = BitConverter.ToUInt32(_hHookSize, 0);

								// If the hook failed to be created, hHookSize value will be 0.
								if (hHookSize == 0)
										throw new InvalidOperationException("Failed creating local hook.");

								// Read the HHOOK.
								byte[] hHookRaw = new byte[hHookSize];
								if (pipeClient.Read(hHookRaw, 0, hHookRaw.Length) == 0)
										throw new IOException("Failed reading the HHOOK sent by the surrogate.");

								// Marshal the HHOOK byte buffer to .NET HHOOK.
								fixed (byte* ptr = hHookRaw)
								{
										HHOOK hHook = Marshal.PtrToStructure<HHOOK>((nint)ptr)!;
										return hHook;
								}
						}
				}

				//public void RemoveHook(WindowHookNativeResult hook)
				//{
				//		throw new NotImplementedException();
				//}

				// ========================== End Public Functions ==========================


				// ============================ Private Functions ============================

				/// <summary>
				/// Connect to the surrogate's pipe server.
				/// </summary>
				/// <param name="surrogatePipeName"></param>
				private void CreatePipeClient()
				{
						pipeClient = new NamedPipeClientStream(
								".",
								_surrogatePipeName,
								PipeDirection.InOut,
								PipeOptions.None,
								TokenImpersonationLevel.None);

						Debug.WriteLine("Connecting to the surrogate's pipe server...");
						pipeClient.Connect();
				}

				private void SurrogateSendRequest(SurrogateRequestType requestType, int firstMessage, uint secondMessage)
				{
						pipeClient.WriteByte((byte)requestType);
						pipeClient.Write(BitConverter.GetBytes(firstMessage));
						pipeClient.Write(BitConverter.GetBytes(secondMessage));
				}

				/// <summary>
				/// When the surrogate process exited.
				/// </summary>
				private void SurrogateExited(object? sender, EventArgs e)
				{
						switch (_surrogateProcess!.ExitCode)
						{
								case 1:
										// Impossible to happen as MainPipeName and LibraryPath can't be empty.
										// So there's must be something wrong when passing the required arguments
										// when launching the surrogate process.
										throw new ArgumentException("MainPipeName or LibraryPath is empty.");
								case 2:
										throw new InvalidOperationException("Failed loading DLL. May be caused by inputting the wrong library path.");
								case 3:
										throw new InvalidOperationException("Failed creating pipe server for the surrogate process.");
								case 4:
										throw new InvalidOperationException("Something went wrong when the surrogate process is waiting for a client to connect.");
								case 5:
										throw new InvalidOperationException("Something went wrong when getting/answering a message from the client.");
						}

						_surrogateProcess?.Dispose();
						_surrogateProcess = null;
				}

				// ============================ End Private Functions ============================


				// ============================ Dispose ============================

				protected virtual void Dispose(bool disposing)
				{
						if (!disposedValue)
						{
								if (disposing)
								{
										// TODO: dispose managed state (managed objects)
								}

								// TODO: free unmanaged resources (unmanaged objects) and override finalizer
								// TODO: set large fields to null
								disposedValue = true;
						}
				}

				public void Dispose()
				{
						// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
						Dispose(disposing: true);
						GC.SuppressFinalize(this);
				}
		}
}
