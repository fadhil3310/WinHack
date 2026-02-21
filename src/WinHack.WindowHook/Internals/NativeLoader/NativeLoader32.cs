using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Win32.UI.WindowsAndMessaging;

namespace WinHack.WindowHook.Internals.NativeLoader
{
		public class NativeLoader32 : INativeLoader
		{
				private bool disposedValue;

				private enum SurrogateRequestType : byte
				{
						CreateHook = 0,
						RemoveHook = 1
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
						var surrogatePipeName = WindowHookOptions.Surrogate32PipeName;
						var surrogatePath = WindowHookOptions.Surrogate32Path;
						var surrogateLibraryPath = WindowHookOptions.Surrogate32LibraryPath;

						if (string.IsNullOrEmpty(surrogatePipeName) || string.IsNullOrEmpty(hookPipeName))
								throw new ArgumentException("Surrogate Pipe Name can't be empty.");

						_surrogateProcess = new Process
						{
								StartInfo = new ProcessStartInfo
								{
										FileName = surrogatePath,
										Arguments = $"{hookPipeName} {surrogatePipeName} {surrogateLibraryPath}",
										CreateNoWindow = false,
								}
						};
						_surrogateProcess.Exited += SurrogateExited;
						_surrogateProcess.Start();

						CreatePipeClient();
				}

				public int CreateHook(WINDOWS_HOOK_ID hookType, uint threadId)
				{
						if (!IsInitialized)
								throw new InvalidOperationException("Loader hasn't been initialized.");

						// Tell the surrogate that we want to create a local hook.
						SurrogateSendRequest(SurrogateRequestType.CreateHook, (int)hookType, threadId);

						// Read the answer from the surrogate.
						BinaryReader binaryReader = new(pipeClient);
						int hookId = binaryReader.ReadInt32();
						if (hookId == 0)
								throw new InvalidOperationException("Failed creating hook.");

						return hookId;
				}

				public void RemoveHook(int hookId)
				{
						if (!IsInitialized)
								throw new InvalidOperationException("Loader hasn't been initialized.");

						// Tell the surrogate that we want to remove a hook.
						SurrogateSendRequest(SurrogateRequestType.RemoveHook, hookId, 0);

						// Read the answer from the surrogate.
						BinaryReader binaryReader = new(pipeClient);
						bool isSuccess = binaryReader.ReadInt32() == 1;
						if (hookId == 0)
								throw new InvalidOperationException("Failed removing hook.");
				}

				// ========================== End Public Functions ==========================


				// ============================ Private Functions ============================

				/// <summary>
				/// Connect to the surrogate's pipe server.
				/// </summary>
				/// <param name="surrogatePipeName"></param>
				private void CreatePipeClient()
				{
						var surrogatePipeName = WindowHookOptions.Surrogate32PipeName;

						pipeClient = new NamedPipeClientStream(
								".",
								surrogatePipeName,
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
