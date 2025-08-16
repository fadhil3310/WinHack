using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinHack.Core.Global
{
		public class WinHackSettings
		{
				private WinHackSettings() { }
				private static WinHackSettings _instance;
				private static readonly object _lock = new object();

				public static WinHackSettings Get()
				{
						if (_instance == null)
						{
								lock (_lock)
								{
										if (_instance == null)
										{
												_instance = new WinHackSettings();
										}
								}
						}
						return _instance;
				}

				public bool ThrowIfError;
		}
}
