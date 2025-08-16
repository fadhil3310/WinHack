using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Threading;

using static WinHack.Core.Utility.Thrower;

namespace WinHack.Core.Systems.Process
{
		public class HackProcess
		{
				public HANDLE Handle { get; private set; }

				// Get Process ID on demand when Process class is created
				// using only the process' Handle.
				private uint? _processId = null;
				public uint ProcessId
				{
						get
						{
								if (_processId == null)
								{
										uint processId = PInvoke.GetProcessId(Handle);
										_processId = processId;
										return processId;
								}
								return (uint)_processId;
						}
				}


				public HackProcess(uint processId, PROCESS_ACCESS_RIGHTS accessRight) 
				{
						var handle = PInvoke.OpenProcess(accessRight, false, processId);
						if (handle.IsNull)
								ThrowWin32(true, "Failed opening process");

						Handle = handle;
						_processId = processId;
				}
				public HackProcess(HANDLE handle)
				{
						Handle = handle;
				}

				public bool Is64Bit()
				{
						return ProcessUtility.Is64Bit(Handle);
				}


				public static implicit operator HANDLE(HackProcess process) => process.Handle;
		}
}
