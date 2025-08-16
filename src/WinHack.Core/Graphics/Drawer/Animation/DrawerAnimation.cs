using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinHack.Core.Graphics.Drawer.Animation
{
		//public record AnimationProgress(float progress, DrawerBase drawer);

		public class DrawerAnimation
		{
				private DrawerBase drawer;

				public bool IsRunning { get; private set; }
				public int Duration { get; private set; }
				public int ElapsedTime { get; private set; }
				public float Progress { get; private set; }
				public float Value { get; private set; }
				public float From { get; private set; }
				public float To { get; private set; }

				public uint FPS { get; private set; }
				public Func<float, float>? Easing { get; private set; }


				public event EventHandler<DrawerBase>? Started;
				public event EventHandler<DrawerBase>? Tick;
				public event EventHandler<DrawerBase>? Finished;

				public DrawerAnimation(
						DrawerBase drawer,
						float from,
						float to,
						int duration,
						out Action<float, float, int> requestTick,
						uint fps, Func<float, float>? easing)
				{
						this.drawer = drawer;
						Duration = duration;
						From = from;
						To = to;
						FPS = fps;
						Easing = easing;

						requestTick = (progress, value, elapsedTimeAdd) =>
						{
								Progress = progress;
								Value = value;
								ElapsedTime += elapsedTimeAdd;
								Tick?.Invoke(this, this.drawer);
						};
				}

				public void Start()
				{
						if (IsRunning) return;
						IsRunning = true;
						Started?.Invoke(this, drawer);
				}

				public void Stop()
				{
						if (!IsRunning) return;
						IsRunning = false;
						Finished?.Invoke(this, drawer);
				}


				public static Func<float, float> EaseOutCubic = x => (float)(1 - Math.Pow(1 - x, 3));
		}
}
