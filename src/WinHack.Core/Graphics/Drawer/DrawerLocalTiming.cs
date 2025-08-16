using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.WebUI;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using WinHack.Core.Displays;
using WinHack.Core.Graphics.DeviceContexts;
using WinHack.Core.Graphics.Drawer.Animation;
using WinHack.Core.Graphics.Objects.Brushes;
using WinHack.Core.Graphics.Objects.Drawables;
using WinHack.Core.Windowing;

namespace WinHack.Core.Graphics.Drawer
{
		public class DrawerLocalTiming : DrawerBase
		{
				private bool disposedValue;

				private bool loopStarted;
				private int loopWaitTime;
				private uint _loopFPS;
				public uint LoopFPS
				{
						get => _loopFPS; 
						set
						{
								loopWaitTime = (int)(1000 / value);
								_loopFPS = value;
						}
				}

				private List<AnimationData> animations = [];
				private List<AnimationData> animationsInLoop = [];

				public DrawerLocalTiming(DeviceContextBase deviceContext, uint? fps = null) : base(deviceContext)
				{
						if (fps == null)
						{
								if (deviceContext is WindowDeviceContext)
								{
										var windowDC = (WindowDeviceContext)deviceContext;
										var monitor = HackMonitorEnumerator.GetDisplayFromWindow(windowDC.Window);
										uint monitorRefreshRate = monitor.GetRefreshRate();
										if (monitorRefreshRate == 0)
												LoopFPS = 60;
										else
												LoopFPS = monitorRefreshRate;
								}
								else
								{
										var monitors = HackMonitorEnumerator.GetAllDisplays();
										if (monitors.Count > 0)
										{
												uint monitorRefreshRate = monitors[0].GetRefreshRate();
												if (monitorRefreshRate == 0)
														LoopFPS = 60;
												else
														LoopFPS = monitorRefreshRate;
										}
										else
										{
												LoopFPS = 60;
										}
								}
						}
						else
						{
								LoopFPS = (uint)fps;
						}
				}


				// ========================= PUBLIC FUNCTIONS =========================
				public override bool Draw(IDrawableObject drawableObject, bool throwIfError = false)
				{
						//Debug.WriteLine($"Draw thread ID: {Environment.CurrentManagedThreadId}");
						lock (_lock)
						{
								if (!base.Draw(drawableObject, throwIfError))
										return false;
								if (!loopStarted)
										StartLoop();
								return true;
						}
				}

				public override DrawerAnimation CreateAnimation(float from, float to, int duration, uint? fps = null, Func<float, float>? easing = null)
				{
						lock (_lock)
						{
								DrawerAnimation? animation = null;

								if (fps == null)
								{
										animation = new DrawerAnimation(
												this,
												from,
												to,
												duration,
												out Action<float, float, int> ticker,
												LoopFPS,
												easing
										);
										var animationData = new AnimationData(animation, ticker);
										animation.Started += (_, _) =>
										{
												lock (_lock)
												{
														if (!loopStarted)
																StartLoop();
												}
										};
										animation.Finished += (_, _) =>
										{
												lock (_lock)
												{
														animationsInLoop.Remove(animationData);
												}
										};
										animationsInLoop.Add(animationData);
								}
								else
								{
										throw new NotImplementedException();
										//animation.Started += (_, _) => StartAnimateDifferentThread(new AnimationData(animation, ticker));
								}

								return animation!;
						}
				}

				public void StopLoop()
				{
						lock (_lock)
						{
								loopStarted = false;
						}
				}
				// ========================= END PUBLIC FUNCTIONS =========================


				// ========================= PRIVATE FUNCTIONS =========================
				private void StartLoop()
				{
						loopStarted = true;

						Thread thread = new(new ThreadStart(() =>
						{
								Stopwatch stopwatch = new();
								int waitTime = loopWaitTime;
								int waitTimeTax = 0;

								while (loopStarted)
								{
										stopwatch.Start();

										lock (_lock)
										{
												//Debug.WriteLine($"Loop thread ID: {Environment.CurrentManagedThreadId}");
												for (int i = 0; i < animationsInLoop.Count; i++)
												{
														AnimationData data = animationsInLoop[i];
														DrawerAnimation animation = data.Animation;

														if (animation.IsRunning)
														{
																int entireWaitTime = waitTime + waitTimeTax;
																float progress =
																		Math.Min(
																				1,
																				animation.Progress + (float)entireWaitTime / (animation.Duration == 0 ? 1 : animation.Duration)
																		);
																float animValue = 0;
																if (animation.Easing != null)
																		animValue = animation.From + ((animation.To - animation.From) * animation.Easing(progress));
																else
																		animValue = animation.From + ((animation.To - animation.From) * progress);

																		data.Ticker(progress, animValue, entireWaitTime);
																if (animation.ElapsedTime >= animation.Duration)
																		animation.Stop();
														}
												}
										}

										var size = DeviceContext.GetSize();
										BlitTo(MainDeviceContext, 0, 0, 0, 0, size.Width, size.Height);

										stopwatch.Stop();
										waitTimeTax = Math.Max(waitTime - (int)stopwatch.ElapsedMilliseconds, 0);
										if (waitTimeTax > 10)
												Debug.WriteLine($"waitTimeTax is very high! {waitTimeTax}");
										Thread.Sleep(waitTime - waitTimeTax);
								}

								lock (_lock)
								{
										for (int i = animationsInLoop.Count - 1; i >= 0; i--)
										{
												animationsInLoop[i].Animation.Stop();
										}
								}

								Debug.WriteLine("Loop thread finished");
						}));
						thread.Start();
				}

				//private void StartAnimateDifferentThread(AnimationData data)
				//{
				//		Task.Run(() =>
				//		{
				//				uint waitTime = 1000 / data.animation.FPS;
				//				int duration = (int)data.animation.Duration.TotalSeconds;
				//				Func<float, float>? easing = data.animation.Easing;

				//				Stopwatch stopwatch = new();
				//				float progress = 0;
				//				bool isRunning = true;
				//				data.animation.Finished += (_, _) => isRunning = false;

				//				while (isRunning)
				//				{
				//						stopwatch.Start();

				//						if (easing != null)
				//								data.ticker.Invoke(easing(progress));
				//						else
				//								data.ticker.Invoke(progress);

				//						if (duration > 0)
				//						{
				//								progress += (waitTime / duration);
				//								if (progress > 1.0f)
				//								{
				//										data.animation.Stop();
				//								}
				//						}

				//						stopwatch.Stop();
				//						Thread.Sleep((int)Math.Max(waitTime - stopwatch.ElapsedMilliseconds, 0));
				//				}
				//		});
				//}

				// ========================= END PRIVATE FUNCTIONS =========================


				// ========================= DISPOSE =========================
				protected void Dispose(bool disposing)
				{
						if (!disposedValue)
						{
								if (disposing)
								{
										StopLoop();
								}

								DeviceContext?.Dispose();

								disposedValue = true;
						}
				}
				//~DrawerLocalTiming()
				//{
				//		Dispose(disposing: false);
				//}
				public override void Dispose()
				{
						Dispose(disposing: true);
						GC.SuppressFinalize(this);
				}
				// ========================= END DISPOSE =========================

				private record AnimationData(DrawerAnimation Animation, Action<float, float, int> Ticker);
		}
}
