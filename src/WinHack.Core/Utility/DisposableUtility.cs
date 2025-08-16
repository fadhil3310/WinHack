using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinHack.Core.Utility
{
		public static class DisposableUtility
		{
				public static void MapDisposables(List<IDisposable> target, IDisposable[] source)
				{
						foreach (var disposable in source)
						{
								target.Add(disposable);
						}
				}

				public static void UnmapDisposables(List<IDisposable> target, IDisposable[] source)
				{
						foreach (var disposable in source)
						{
								target.Remove(disposable);
						}
				}

				public static void DisposeAll(List<IDisposable> disposables)
				{
						foreach (var disposable in disposables)
						{
								disposable.Dispose();
						}
				}
		}
}
