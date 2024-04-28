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
		private readonly Vector3 _position = new(0f, 0f, 0f);
		private readonly Vector3 _forward = new(0f, 0f, -1f);
		private readonly Vector3 _up = new(0f, 1f, 0f);

		private readonly Vector3 _radiusPhiTheta = new(8f, -0.1f, 0.5f);
		private readonly Vector3 _cameraTarget = new(0f, 0f, 0f);

		private readonly Vector3 _cameraUpVector = new(0f, 1f, 0f);
		private const float SpectatorStep = 0.025f;

		private const float FieldOfView = 1f;
		private const float NearPlaneDistance = 0.9f;
		private const float FarPlaneDistance = 1f;

		private const float X0 = 0f;
		private const float Y0 = 0f;
		private const float MinDepth = -100;
		private const float MaxDepth = 100;

		private readonly Vector3 _lightPosition = new(750f + -500f, 600f + 0f, 750 + 2000f);
		private readonly Vector3 _lightColor = new(1f, 1f, 1f);
		private readonly Vector3 _objectColor = new(1.0f, 0.5f, 0.31f);

		private Matrix4x4 _world;
		private Matrix4x4 _view;
		private Matrix4x4 _projection;
		private Matrix4x4 _viewport;
		private Matrix4x4 _final;
		private readonly Queue<Thread> _threads = [];
		private readonly Queue<Polygon> _polygons = [];
		private Dictionary<Tuple<int, int>, Tuple<double, Color>> _zBuffer = [];
		private WriteableBitmap? _image;
		private readonly Stopwatch _stopwatch = new();

		public Scene()
		{
			InitializeComponent();
		}

		private Vector3 _cameraPosition;
		private Vector3 _cameraSpherePosition;

		private Vector3 CameraSpherePosition
		{
			get => _cameraSpherePosition;
			set
			{
				_cameraPosition = _cameraTarget + new Vector3(
					(float)(value.X * Math.Sin(value.Y * 2 * Math.PI) * Math.Sin(value.Z * Math.PI)),
					(float)(value.X * Math.Cos(value.Z * Math.PI)),
					(float)(value.X * Math.Cos(value.Y * 2 * Math.PI) * Math.Sin(value.Z * Math.PI))
				);
				_cameraSpherePosition = value;
			}
		}

		private void DataUpdate()
		{
			_zBuffer = [];
			_image = new WriteableBitmap((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Bgra32, null);
			_world = Matrix4X4Extension.CreateWorld(_position, _forward, _up);
			_view = Matrix4X4Extension.CreateLookAt(_cameraPosition, _cameraTarget, _cameraUpVector);
			_projection = Matrix4X4Extension.CreatePerspectiveFieldOfView(FieldOfView,
				(float)(ActualWidth / ActualHeight), NearPlaneDistance, FarPlaneDistance);
			_viewport = Matrix4X4Extension.CreateViewport(X0, Y0, (float)ActualWidth, (float)ActualHeight, MinDepth,
				MaxDepth);
			_final = _world * _view * _projection * _viewport;
			_stopwatch.Reset();
		}

		private void RenderPoints()
		{
			foreach (var triangle in PaintedFigures.F)
			{
				_polygons.Enqueue(new Polygon(
					Matrix4X4Extension.VectorMatrixMultiplication(
						PaintedFigures.V[triangle.Item1.Item1 - 1],
						_final),
					Matrix4X4Extension.VectorMatrixMultiplication(
						PaintedFigures.V[triangle.Item2.Item1 - 1],
						_final),
					Matrix4X4Extension.VectorMatrixMultiplication(
						PaintedFigures.V[triangle.Item3.Item1 - 1],
						_final),
					Vector3.Normalize(
						Matrix4X4Extension.VectorMatrixMultiplication(
							PaintedFigures.Vn[triangle.Item1.Item3 - 1],
							Matrix4X4Extension.CreateWorld(new Vector3(0, 0, 0),
								_forward, _up)))));
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
					if (_zBuffer.TryGetValue(key, out var value))
					{
						if (value.Item1 <= pixel.Value)
							_zBuffer[key] = new Tuple<double, Color>(pixel.Value,
								Algorithms.Algorithms.GetColor(_polygons.First().Normal,
									new Vector3(pixel.Key.Item1, pixel.Key.Item2, (float)pixel.Value), _lightPosition,
									_lightColor, _objectColor));
					}

					if (value == null)
						_zBuffer.Add(pixel.Key, new Tuple<double, Color>(pixel.Value,
							Algorithms.Algorithms.GetColor(_polygons.First().Normal,
								new Vector3(pixel.Key.Item1, pixel.Key.Item2, (float)pixel.Value), _lightPosition,
								_lightColor, _objectColor)));
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
				if (_zBuffer.ContainsKey(key))
				{
					_zBuffer[key] = new Tuple<double, Color>(0, Color.FromRgb(255, 0, 0));
				}
				else
				{
					_zBuffer.Add(key, new Tuple<double, Color>(0, Color.FromRgb(255, 0, 0)));
				}
			}

			foreach (var key in Algorithms.Algorithms
				         .Cda(new Point(vectorO.X, vectorO.Y), new Point(vectorY.X, vectorY.Y)).Select(item =>
					         new Tuple<int, int>((int)Math.Round(item.X, MidpointRounding.AwayFromZero),
						         (int)Math.Round(item.Y, MidpointRounding.AwayFromZero))))
			{
				if (_zBuffer.ContainsKey(key))
				{
					_zBuffer[key] = new Tuple<double, Color>(0, Color.FromRgb(0, 255, 0));
				}
				else
				{
					_zBuffer.Add(key, new Tuple<double, Color>(0, Color.FromRgb(0, 255, 0)));
				}
			}

			foreach (var key in Algorithms.Algorithms
				         .Cda(new Point(vectorO.X, vectorO.Y), new Point(vectorZ.X, vectorZ.Y)).Select(item =>
					         new Tuple<int, int>((int)Math.Round(item.X, MidpointRounding.AwayFromZero),
						         (int)Math.Round(item.Y, MidpointRounding.AwayFromZero))))
			{
				if (_zBuffer.ContainsKey(key))
				{
					_zBuffer[key] = new Tuple<double, Color>(0, Color.FromRgb(0, 0, 255));
				}
				else
				{
					_zBuffer.Add(key, new Tuple<double, Color>(0, Color.FromRgb(0, 0, 255)));
				}
			}
		}

		private void MakeImage()
		{
			foreach (var pixel in _zBuffer.Where(x =>
				         x.Key.Item1 < ActualWidth && x.Key.Item1 >= 0 && x.Key.Item2 < ActualHeight &&
				         x.Key.Item2 >= 0))
			{
				_image?.WritePixels(new Int32Rect(pixel.Key.Item1, pixel.Key.Item2, 1, 1),
					new byte[] { pixel.Value.Item2.B, pixel.Value.Item2.G, pixel.Value.Item2.R, 255 }, 4, 0);
			}
		}

		private void DataToUi()
		{
			RenderTimer.Content = _stopwatch.ToString();
			Picture.Source = _image;
		}

		private void PictureUpdateAsync()
		{
			DataUpdate();
			_stopwatch.Start();
			RenderPoints();
			MakeZBuffer();
			BuildCoordinateAxes();
			MakeImage();
			_stopwatch.Stop();
			DataToUi();
		}

		private static readonly DependencyProperty PaintedFiguresProperty =
			DependencyProperty.Register(nameof(PaintedFigures), typeof(Figure), typeof(Scene));

		public Figure PaintedFigures
		{
			get => (Figure)GetValue(PaintedFiguresProperty);
			set => SetValue(PaintedFiguresProperty, value);
		}

		private void Win_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e)
			{
				case { Key: Key.Up }:
					CameraSpherePosition += new Vector3(0f, 0f, -SpectatorStep);
					PictureUpdateAsync();
					break;
				case { Key: Key.Down }:
					CameraSpherePosition += new Vector3(0f, 0f, SpectatorStep);
					PictureUpdateAsync();
					break;
				case { Key: Key.Left }:
					CameraSpherePosition += new Vector3(0f, -SpectatorStep, 0f);
					PictureUpdateAsync();
					break;
				case { Key: Key.Right }:
					CameraSpherePosition += new Vector3(0f, SpectatorStep, 0f);
					PictureUpdateAsync();
					break;
				case { Key: Key.Add }:
					CameraSpherePosition += new Vector3(-1f, 0f, 0f);
					PictureUpdateAsync();
					break;
				case { Key: Key.Subtract }:
					CameraSpherePosition += new Vector3(1f, 0f, 0f);
					PictureUpdateAsync();
					break;
				case { Key: Key.OemPlus }:
					CameraSpherePosition += new Vector3(-1f, 0f, 0f);
					PictureUpdateAsync();
					break;
				case { Key: Key.OemMinus }:
					CameraSpherePosition += new Vector3(1f, 0f, 0f);
					PictureUpdateAsync();
					break;
				case { Key: Key.Enter }:
					CameraSpherePosition = _radiusPhiTheta;
					PictureUpdateAsync();
					break;
			}
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			Keyboard.Focus(Picture);
			Background = Brushes.Black;
		}
	}
}
