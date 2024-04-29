using CourseWork.Algorithms;
using CourseWork.CustomDataTypes;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CourseWork.Controls
{
	/// <summary>
	/// Логика взаимодействия для Scene.xaml
	/// </summary>
	public partial class Scene
	{
		private class Renderer
		{
			private class Polygon(in Vector3 point1, in Vector3 point2, in Vector3 point3, in Vector3 normal)
			{
				private readonly Vector3 _point1 = point1;
				private readonly Vector3 _point2 = point2;
				private readonly Vector3 _point3 = point3;
				public Vector3 Normal { get; set; } = normal;

				public void MakeFill()
				{
					Dictionary<int, Tuple<Point, Point>> cheats = [];
					Algorithms.Algorithms.Cda(_point1, _point2, ref cheats);
					Algorithms.Algorithms.Cda(_point2, _point3, ref cheats);
					Algorithms.Algorithms.Cda(_point3, _point1, ref cheats);
					foreach (var lines in cheats)
					{
						foreach (var item in Algorithms.Algorithms.Cda(lines.Value.Item1, lines.Value.Item2))
						{
							Tuple<int, int> key = new((int)Math.Round(item.X, MidpointRounding.ToZero), lines.Key);
							if (Buffer.TryGetValue(key, out var value))
							{
								if (value <= item.Y)
									Buffer[key] = item.Y;
							}
							else
								Buffer.Add(key, item.Y);
						}
					}
				}
				public Dictionary<Tuple<int, int>, double> Buffer { get; set; } = [];
			}

			private Matrix4x4 _world;
			private Matrix4x4 _view;
			private Matrix4x4 _projection;
			private Matrix4x4 _viewport;
			private Matrix4x4 _final;
			private readonly Queue<Thread> _threads = [];
			private readonly Queue<Polygon> _polygons = [];
			private readonly Stopwatch _stopwatch = new();
			private Vector3 _cameraPosition;
			private Vector3 _cameraSpherePosition;
			public Obj PaintedObj { private get; set; }
			public Settings Settings { private get; set; } = new();
			public Dictionary<Tuple<int, int>, Tuple<double, Color>> ZBuffer { get; private set; } = [];
			public string RenderTime { get; private set; } = "";
			private Size Size { get; set; }
			private Vector3 CameraSpherePosition
			{
				get => _cameraSpherePosition;
				set
				{
					_cameraPosition = Settings._cameraTarget + new Vector3(
						(float)(value.X * Math.Sin(value.Y * 2 * Math.PI) * Math.Sin(value.Z * Math.PI)),
						(float)(value.X * Math.Cos(value.Z * Math.PI)),
						(float)(value.X * Math.Cos(value.Y * 2 * Math.PI) * Math.Sin(value.Z * Math.PI))
					);
					_cameraSpherePosition = value;
				}
			}

			private void CameraTurn(in Vector3 cameraTurn)
			{
				if (cameraTurn == new Vector3(-1f, -1f, -1f))
					CameraSpherePosition = Settings._radiusPhiTheta;
				else
					CameraSpherePosition +=
						cameraTurn * new Vector3(1f, Settings.SpectatorStep, Settings.SpectatorStep);
			}

			private void DataUpdate(in Size size, in Vector3 cameraTurn)
			{
				Size = size;
				ZBuffer = [];
				CameraTurn(cameraTurn);
				_world = Matrix4X4Extension.CreateWorld(Settings._position, Settings._forward, Settings._up);
				_view = Matrix4X4Extension.CreateLookAt(_cameraPosition, Settings._cameraTarget, Settings._cameraUpVector);
				_projection = Matrix4X4Extension.CreatePerspectiveFieldOfView(Settings.FieldOfView,
					(float)(Size.Width / Size.Height), Settings.NearPlaneDistance, Settings.FarPlaneDistance);
				_viewport = Matrix4X4Extension.CreateViewport(Settings.X0, Settings.Y0, (float)Size.Width, (float)Size.Height, Settings.MinDepth,
					Settings.MaxDepth);
				_final = _world * _view * _projection * _viewport;
				_stopwatch.Reset();
			}

			private void RenderPoints()
			{
				foreach (var triangle in PaintedObj.F)
				{
					_polygons.Enqueue(new Polygon(
						Matrix4X4Extension.VectorMatrixMultiplication(
							PaintedObj.V[triangle.Item1.Item1 - 1],
							_final),
						Matrix4X4Extension.VectorMatrixMultiplication(
							PaintedObj.V[triangle.Item2.Item1 - 1],
							_final),
						Matrix4X4Extension.VectorMatrixMultiplication(
							PaintedObj.V[triangle.Item3.Item1 - 1],
							_final),
						Vector3.Normalize(
							Matrix4X4Extension.VectorMatrixMultiplication(
								PaintedObj.Vn[triangle.Item1.Item3 - 1],
								Matrix4X4Extension.CreateWorld(new Vector3(0, 0, 0),
									Settings._forward, Settings._up)))));
					_threads.Enqueue(new Thread(_polygons.Last().MakeFill));
					_threads.Last().Start();
				}
			}

			private void MakeZBuffer()
			{
				while (_threads.Count != 0)
				{
					if (_threads.First().ThreadState != System.Threading.ThreadState.Stopped) continue;
					foreach (var pixel in _polygons.First().Buffer)
					{
						Tuple<int, int> key = new(pixel.Key.Item1, pixel.Key.Item2);
						if (ZBuffer.TryGetValue(key, out var value))
						{
							if (value.Item1 <= pixel.Value)
								ZBuffer[key] = new Tuple<double, Color>(pixel.Value,
									Algorithms.Algorithms.GetColor(_polygons.First().Normal,
										new Vector3(pixel.Key.Item1, pixel.Key.Item2, (float)pixel.Value), Settings._lightPosition,
										Settings._lightColor, Settings._objectColor));
						}

						if (value == null)
							ZBuffer.Add(pixel.Key, new Tuple<double, Color>(pixel.Value,
								Algorithms.Algorithms.GetColor(_polygons.First().Normal,
									new Vector3(pixel.Key.Item1, pixel.Key.Item2, (float)pixel.Value), Settings._lightPosition,
									Settings._lightColor, Settings._objectColor)));
					}

					_threads.Dequeue();
					_polygons.Dequeue();
				}
			}

			private void BuildCoordinateAxes()
			{
				var newFinal = Matrix4X4Extension.CreateWorld(new Vector3(0, 0, 0),
								   new Vector3(0, 0, -1), new Vector3(0, 1, 0)) * _view * _projection *
							   _viewport;
				var vectorO = Matrix4X4Extension.VectorMatrixMultiplication(new Vector3(0, 0, 0), newFinal);
				var vectorX = Matrix4X4Extension.VectorMatrixMultiplication(new Vector3(5, 0, 0), newFinal);
				var vectorY = Matrix4X4Extension.VectorMatrixMultiplication(new Vector3(0, 5, 0), newFinal);
				var vectorZ = Matrix4X4Extension.VectorMatrixMultiplication(new Vector3(0, 0, 5), newFinal);
				foreach (var key in Algorithms.Algorithms
							 .Cda(new Point(vectorO.X, vectorO.Y), new Point(vectorX.X, vectorX.Y)).Select(item =>
								 new Tuple<int, int>((int)Math.Round(item.X, MidpointRounding.AwayFromZero),
									 (int)Math.Round(item.Y, MidpointRounding.AwayFromZero))))
				{
					if (ZBuffer.ContainsKey(key))
					{
						ZBuffer[key] = new Tuple<double, Color>(0, Color.FromRgb(255, 0, 0));
					}
					else
					{
						ZBuffer.Add(key, new Tuple<double, Color>(0, Color.FromRgb(255, 0, 0)));
					}
				}

				foreach (var key in Algorithms.Algorithms
							 .Cda(new Point(vectorO.X, vectorO.Y), new Point(vectorY.X, vectorY.Y)).Select(item =>
								 new Tuple<int, int>((int)Math.Round(item.X, MidpointRounding.AwayFromZero),
									 (int)Math.Round(item.Y, MidpointRounding.AwayFromZero))))
				{
					if (ZBuffer.ContainsKey(key))
					{
						ZBuffer[key] = new Tuple<double, Color>(0, Color.FromRgb(0, 255, 0));
					}
					else
					{
						ZBuffer.Add(key, new Tuple<double, Color>(0, Color.FromRgb(0, 255, 0)));
					}
				}

				foreach (var key in Algorithms.Algorithms
							 .Cda(new Point(vectorO.X, vectorO.Y), new Point(vectorZ.X, vectorZ.Y)).Select(item =>
								 new Tuple<int, int>((int)Math.Round(item.X, MidpointRounding.AwayFromZero),
									 (int)Math.Round(item.Y, MidpointRounding.AwayFromZero))))
				{
					if (ZBuffer.ContainsKey(key))
					{
						ZBuffer[key] = new Tuple<double, Color>(0, Color.FromRgb(0, 0, 255));
					}
					else
					{
						ZBuffer.Add(key, new Tuple<double, Color>(0, Color.FromRgb(0, 0, 255)));
					}
				}
			}

			public void PictureUpdateAsync(Size size, Vector3 cameraTurn)
			{
				DataUpdate(size, cameraTurn);
				_stopwatch.Start();
				RenderPoints();
				MakeZBuffer();
				BuildCoordinateAxes();
				_stopwatch.Stop();
				RenderTime = _stopwatch.ToString();
			}
		}




		private readonly Renderer _renderer;
		private Vector3 _turnVector3 = Vector3.Zero;
		private KeyEventArgs? _pressedKey;
		private bool _running = false;
		public Scene()
		{
			InitializeComponent();
			_renderer = new Renderer();
		}

		private void MakeTurnVector()
		{
			_turnVector3 = Vector3.Zero;
			switch (_pressedKey)
			{
				case { Key: Key.Up }:
					_turnVector3[2]--;
					break;
				case { Key: Key.Down }:
					_turnVector3[2]++;
					break;
				case { Key: Key.Left }:
					_turnVector3[1]--;
					break;
				case { Key: Key.Right }:
					_turnVector3[1]++;
					break;
				case { Key: Key.Add }:
					_turnVector3[0]--;
					break;
				case { Key: Key.Subtract }:
					_turnVector3[0]++;
					break;
				case { Key: Key.OemPlus }:
					_turnVector3[0]--;
					break;
				case { Key: Key.OemMinus }:
					_turnVector3[0]++;
					break;
				case { Key: Key.Enter }:
					_turnVector3 = new Vector3(-1f, -1f, -1f);
					break;
			}
		}
		private async void SceneUpdate()
		{
			_running = true;
			MakeTurnVector();
			if (_turnVector3 == Vector3.Zero) return;
			await Task.Run(() => _renderer.PictureUpdateAsync(RenderSize, _turnVector3));
			DataToUi();
			_running = false;
		}
		private void DataToUi()
		{
			RenderTimer.Content = _renderer.RenderTime;
			var image = new WriteableBitmap((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Bgra32, null);
			foreach (var pixel in _renderer.ZBuffer.Where(x =>
							 x.Key.Item1 < ActualWidth && x.Key.Item1 >= 0 && x.Key.Item2 < ActualHeight &&
							 x.Key.Item2 >= 0))
			{
				image.WritePixels(new Int32Rect(pixel.Key.Item1, pixel.Key.Item2, 1, 1),
					new byte[] { pixel.Value.Item2.B, pixel.Value.Item2.G, pixel.Value.Item2.R, 255 }, 4, 0);
			}
			Picture.Source = image;
		}

		private static readonly DependencyProperty PaintedObjProperty =
			DependencyProperty.Register(nameof(PaintedObj), typeof(Obj), typeof(Scene),
				new FrameworkPropertyMetadata(OnPaintedObjChanged));

		public Obj PaintedObj
		{
			set => SetValue(PaintedObjProperty, value);
		}
		private static void OnPaintedObjChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((Scene)d)._renderer.PaintedObj = (Obj)e.NewValue;
		}
		private static readonly DependencyProperty SettingsProperty =
			DependencyProperty.Register(nameof(Settings), typeof(Settings), typeof(Scene),
				new FrameworkPropertyMetadata(OnSettingsChanged));

		public Settings Settings
		{
			set => SetValue(SettingsProperty, value);
		}
		private static void OnSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((Scene)d)._renderer.Settings = (Settings)e.NewValue;
			((Scene)d).Background = new SolidColorBrush(Color.FromRgb((byte)(((Settings)e.NewValue).BackGroundColor[0] * 255), (byte)(((Settings)e.NewValue).BackGroundColor[1] * 255), (byte)(((Settings)e.NewValue).BackGroundColor[2] * 255)));
		}
		private void Win_KeyDown(object sender, KeyEventArgs e)
		{
			if (_running) return;
			_pressedKey = e;
			SceneUpdate();
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			Keyboard.Focus(Picture);
		}
	}
}
