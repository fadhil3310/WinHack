using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.UI.WindowsAndMessaging;
using WinHack.Core.Windowing;
using WinHack.WindowHook.Internals;

namespace WinHack.WindowHook
{
		public abstract class WindowHookManagedBase : WindowHookBase
		{
				public override HHOOK HHOOK => hookInstance.HHOOK;

				protected WindowHookManaged hookInstance;


				protected WindowHookManagedBase(HackWindow? window, WindowHookManaged hookInstance) : base(window)
				{
						this.hookInstance = hookInstance;
				}
		}
}
