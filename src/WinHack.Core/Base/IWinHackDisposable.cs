using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinHack.Core.Base
{
		public interface IWinHackDisposable : IDisposable
		{
				public void RetainResources(params IDisposable[] resources);
				public void ReleaseResources(params IDisposable[] resources);
		}
}
