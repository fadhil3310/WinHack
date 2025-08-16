using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.Input;
using DependencyPropertyGenerator;
using WinHack.Core.Graphics;
using WinHack.Core.Graphics.Drawer;

using HackWindow = WinHack.Core.Windowing.HackWindow;
using Color = System.Drawing.Color;
using System.Diagnostics;
using WinHack.Core.Graphics.Drawer.Animation;
using WinHack.Core.Graphics.DeviceContexts;
using WinHack.Core.Graphics.Objects.Drawables;
using WinHack.Core.Graphics.Objects.Brushes;
using Rectangle = WinHack.Core.Graphics.Objects.Drawables.Rectangle;
using WinHack.Core.Displays;
using WinHack.Core.Graphics.Objects;


namespace WinHack.Test.Sheets.WindowProperties.Tabs
{
		/// <summary>
		/// Interaction logic for GraphicTab.xaml
		/// </summary>
		[DependencyProperty<HackWindow>("SelectedWindow")]
		public partial class GraphicTab : UserControl
		{
				WindowDeviceContext windowDC;
				
				Dictionary<string, AnimationInstance> animationInstances = [];

				public GraphicTab()
				{
						InitializeComponent();

						Loaded += (_, _) =>
						{
								windowDC = new WindowDeviceContext(SelectedWindow!);
						};
						Unloaded += (_, _) =>
						{
								windowDC?.Dispose();
								foreach (var item in animationInstances)
								{
										item.Value.Dispose();
								}
						};
				}


				[RelayCommand]
				private void CopyScreenToWindow()
				{
						var displayList = HackMonitorEnumerator.GetAllDisplays();
						if (displayList.Count == 0)
						{
								MessageBox.Show("Failed to get displays.");
								return;
						}
						var displayDC = new DisplayDeviceContext(displayList[0]);

						var size = windowDC.GetSize();
						using (var graphic = new Graphic(windowDC))
						{
								graphic.BlitFrom(displayDC, 0, 0, 0, 0, size.Width, size.Height);
						}
				}

				[RelayCommand]
				public void GetImage()
				{
						BitmapBufferData buffer = null;
						try
						{
								buffer = SelectedWindow!.GetImage(true)!;
						}
						catch (Exception ex)
						{
								MessageBox.Show(ex.Message, "Failed to capture content");
								return;
						}

						var writeableBitmap = new WriteableBitmap(
										buffer!.Width,
										buffer!.Height,
										96,
										96,
										PixelFormats.Bgr32,
										null
						);
						writeableBitmap.WritePixels(new Int32Rect(0, 0, buffer.Width, buffer.Height), buffer.Buffer, writeableBitmap.BackBufferStride, 0);

						var window = new Window()
						{
								Title = $"Captured content - Width: {buffer.Width} - Height: {buffer.Height}"
						};
						var image = new Image
						{
								Source = writeableBitmap
						};
						window.Content = image;
						window.Show();

						//writeableBitmap.Lock();
						//Debug.WriteLine($"Width {writeableBitmap.PixelWidth}");
						//Debug.WriteLine($"Height {writeableBitmap.PixelHeight}");
						//unsafe
						//{
						//		//nint backBufferPtr = writeableBitmap.BackBuffer;
						//		//Marshal.Copy(buffer.Buffer, 0, backBufferPtr, buffer.Buffer.Length);

						//		nint backBufferPtr = writeableBitmap.BackBuffer;


						//		for (int i = 0; i < buffer.Buffer.Length - 3; i++)
						//		{
						//				//int color = 255 << 24;
						//				//color |= buffer.Buffer[i] << 16;
						//				//color |= buffer.Buffer[i + 1] << 8;
						//				//color |= buffer.Buffer[i + 2] << 0;
						//				//Debug.WriteLine(color);

						//				byte red = buffer.Buffer[i];   // GDI RGB: Red is at i + 2
						//				byte green = buffer.Buffer[i + 1]; // GDI RGB: Green is at i + 1
						//				byte blue = buffer.Buffer[i + 2];       // GDI RGB: Blue is at i
						//				//byte alpha = buffer.Buffer[i + 3];

						//				// Create BGRA color
						//				int color = (blue << 16) | (green << 8) | red; // Alpha is set to 255
						//				//Debug.WriteLine($"At i: {i} {red} {green} {blue} {color:X}");

						//				*((int*)backBufferPtr) = color;
						//				//Marshal.WriteInt32(backBufferPtr, color);
						//				backBufferPtr++;
						//		}
						//}

						//writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, buffer.Width, buffer.Height));
						//writeableBitmap.Unlock();
				}

				[RelayCommand]
				private void AnimRectangle()
				{
						if (animationInstances.TryGetValue("rectangle", out var instance))
						{
								instance.Dispose();
								animationInstances.Remove("rectangle");
						}
						else
						{
								//Debug.WriteLine($"Window thread ID: {Environment.CurrentManagedThreadId}");

								//var display = DisplayEnumerator.GetAllDisplays()[0];
								//var displayDC = new DisplayDeviceContext(display);

								var size = windowDC.GetSize();
								//var size = display.GetResolution();
								var drawer = new DrawerLocalTiming(windowDC);
								//var drawer = new DrawerLocalTiming(displayDC);
								var whiteBrush = new SolidBrush(Color.White);
								var redBrush = new SolidBrush(Color.Red);

								var animation = drawer.CreateAnimation(10, 100, 5000, easing: DrawerAnimation.EaseOutCubic);
								var background = new Rectangle(0, 0, size.Width, size.Height);
								var rectangle = new Rectangle(0, 0, 50, 50);
								animation.Tick += (sender, _drawer) =>
								{
										//Debug.WriteLine($"Tick thread ID: {Environment.CurrentManagedThreadId}");

										DrawerAnimation _animation = (DrawerAnimation)sender!;
										rectangle.X = (int)_animation.Value;
										_drawer.UseBrush(whiteBrush, true);
										_drawer.Draw(background);
										_drawer.UseBrush(redBrush, true);
										_drawer.Draw(rectangle);
								};
								animation.Finished += (_, _) =>
								{
										//whiteBrush.Dispose();
										//redBrush.Dispose();
										//GC.KeepAlive(whiteBrush);
										//GC.KeepAlive(redBrush);
								};

								List<IDisposable> disposables = [whiteBrush, redBrush];
								animationInstances.Add("rectangle", new AnimationInstance(drawer, animation, disposables));
								animation.Start();
						}
				}


				private class AnimationInstance(DrawerBase drawer, DrawerAnimation animation, List<IDisposable> disposables) : IDisposable
				{
						public DrawerBase Drawer { get; private set; } = drawer;
						public DrawerAnimation Animation { get; private set; } = animation;
						public List<IDisposable> Disposables { get; private set; } = disposables;

						public void Dispose()
						{
								Drawer.Dispose();
								foreach (IDisposable disposable in Disposables)
								{ 
										disposable.Dispose(); 
								}
						}
				}
		}
}
