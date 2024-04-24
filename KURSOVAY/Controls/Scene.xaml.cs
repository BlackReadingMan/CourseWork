using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CourseWork.Algorithm;
using CourseWork.CustomDataTypes;

namespace CourseWork.Controls
{
	/// <summary>
	/// Логика взаимодействия для Scene.xaml
	/// </summary>
	public partial class Scene : UserControl
	{
		private readonly Vector3 _position = new(0f, 0f, 0f);
		private readonly Vector3 _forward = new(0f, 0f, -1f);
		private readonly Vector3 _up = new(0f, 1f, 0f);

		private readonly Vector3 _radiusPhiTheta = new(7f, 0.0f, 0.5f);
		private readonly Vector3 _cameraTarget = new(0f, 0f, 0f);

		private readonly Vector3 _cameraUpVector = new(0f, 1f, 0f);
		private const float SpectatorStep = 0.025f;

		private const float FieldOfView = 1f;
		private const float NearPlaneDistance = 90f;
		private const float FarPlaneDistance = 100f;

		private const float X0 = 0f;
		private const float Y0 = 0f;
		private const float MinDepth = -56;
		private const float MaxDepth = 56;

		private Vector3 _lightPosition;
		private readonly Vector3 _lightColor = new(1f, 1f, 1f);
		private readonly Vector3 _objectColor = new(1f, 1f, 1f);

		private Matrix4x4 _world;
		private Matrix4x4 _view;
		private Matrix4x4 _projection;
		private Matrix4x4 _viewport;
		private Matrix4x4 _final;
		private readonly Queue<Thread> _threads = [];
		private readonly Queue<Polygon> _polygons = [];
		private Dictionary<Tuple<int, int>, Tuple<double, Color>> _zBuffer = [];
		private WriteableBitmap? _image;
		private readonly Stopwatch _stopwatch = new ();
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
				_cameraPosition = _position + new Vector3(
				(float)(value.X * Math.Sin(value.Y * 2 * Math.PI) * Math.Sin(value.Z * Math.PI)),
				(float)(value.X * Math.Cos(value.Z * Math.PI)),
				(float)(value.X * Math.Cos(value.Y * 2 * Math.PI) * Math.Sin(value.Z * Math.PI))
				);
				_cameraSpherePosition = value;
				_lightPosition = _cameraPosition;
			}
		}
		private void DataUpdate()
		{
			_zBuffer = [];
			_image = new WriteableBitmap((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Bgra32, null);
			_world = Matrix4X4Extension.CreateWorld(_position, _forward, _up);
			_view = Matrix4X4Extension.CreateLookAt(_cameraPosition, _cameraTarget, _cameraUpVector);
			_projection = Matrix4X4Extension.CreatePerspectiveFieldOfView(FieldOfView, (float)(ActualWidth / ActualHeight), NearPlaneDistance, FarPlaneDistance);
			_viewport = Matrix4X4Extension.CreateViewport(X0, Y0, (float)ActualWidth, (float)ActualHeight, MinDepth, MaxDepth);
			_final = _world * _view * _projection * _viewport;
			_stopwatch.Reset();
		}
		private void RenderPoints()
		{
			foreach (var triangle in PaintedFigures.F)
			{
				var polygonPos = new Vector3(
					(PaintedFigures.V[triangle.Item1.Item1 - 1].X + PaintedFigures.V[triangle.Item2.Item1 - 1].X + PaintedFigures.V[triangle.Item3.Item1 - 1].X) / 3f,
					(PaintedFigures.V[triangle.Item1.Item1 - 1].Y + PaintedFigures.V[triangle.Item2.Item1 - 1].Y + PaintedFigures.V[triangle.Item3.Item1 - 1].Y) / 3f,
					(PaintedFigures.V[triangle.Item1.Item1 - 1].Z + PaintedFigures.V[triangle.Item2.Item1 - 1].Z + PaintedFigures.V[triangle.Item3.Item1 - 1].Z) / 3f);
				_polygons.Enqueue(new Polygon(
					Algorithms.VectorMatrixMultiplication(PaintedFigures.V[triangle.Item1.Item1 - 1], _final),
					Algorithms.VectorMatrixMultiplication(PaintedFigures.V[triangle.Item2.Item1 - 1], _final),
					Algorithms.VectorMatrixMultiplication(PaintedFigures.V[triangle.Item3.Item1 - 1], _final),
					Algorithms.GetColor(PaintedFigures.Vn[triangle.Item1.Item3 - 1], polygonPos, _lightPosition, _lightColor, _objectColor)));
				_threads.Enqueue(new Thread(_polygons.Last().MakeFill));
				_threads.Last().Start();
			}
		}
		private void MakeZBuffer()
		{
			while (_threads.Count != 0)
			{
				if (_threads.First().ThreadState == System.Threading.ThreadState.Stopped)
				{
					foreach (var pixel in _polygons.First().Buffer)
					{
						Tuple<int, int> key = new(pixel.Key.Item1, pixel.Key.Item2);
						if (_zBuffer.TryGetValue(key, out var value))
						{
							if (value.Item1 <= pixel.Value.Item1)
								_zBuffer[key] = pixel.Value;
						}
						if (value == null)
							_zBuffer.Add(pixel.Key, pixel.Value);
					}
					_threads.Dequeue();
					_polygons.Dequeue();
				}
			}
		}
		private void BuildCoordinateAxes()
		{
			var vectorO = Algorithms.VectorMatrixMultiplication(new Vector4(0, 0, 0, 1), _final);
			var vectorX = Algorithms.VectorMatrixMultiplication(new Vector4(5, 0, 0, 1), _final);
			var vectorY = Algorithms.VectorMatrixMultiplication(new Vector4(0, 5, 0, 1), _final);
			var vectorZ = Algorithms.VectorMatrixMultiplication(new Vector4(0, 0, 5, 1), _final);
			foreach (var key in Algorithms.Cda(new Point(vectorO.X, vectorO.Y), new Point(vectorX.X, vectorX.Y)).Select(item => new Tuple<int, int>((int)Math.Round(item.X, MidpointRounding.AwayFromZero), (int)Math.Round(item.Y, MidpointRounding.AwayFromZero))))
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
			foreach (var key in Algorithms.Cda(new Point(vectorO.X, vectorO.Y), new Point(vectorY.X, vectorY.Y)).Select(item => new Tuple<int, int>((int)Math.Round(item.X, MidpointRounding.AwayFromZero), (int)Math.Round(item.Y, MidpointRounding.AwayFromZero))))
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
			foreach (var key in Algorithms.Cda(new Point(vectorO.X, vectorO.Y), new Point(vectorZ.X, vectorZ.Y)).Select(item => new Tuple<int, int>((int)Math.Round(item.X, MidpointRounding.AwayFromZero), (int)Math.Round(item.Y, MidpointRounding.AwayFromZero))))
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
			foreach (var pixel in _zBuffer.Where(x => x.Key.Item1 < ActualWidth && x.Key.Item1 >= 0 && x.Key.Item2 < ActualHeight && x.Key.Item2 >= 0))
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
		private void PictureUpdate()
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
		private static readonly DependencyProperty PaintedFiguresProperty = DependencyProperty.Register(nameof(PaintedFigures), typeof(Figure), typeof(Scene));
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
					PictureUpdate();
					break;
				case { Key: Key.Down }:
					CameraSpherePosition += new Vector3(0f, 0f, SpectatorStep);
					PictureUpdate();
					break;
				case { Key: Key.Left }:
					CameraSpherePosition += new Vector3(0f, -SpectatorStep, 0f);
					PictureUpdate();
					break;
				case { Key: Key.Right }:
					CameraSpherePosition += new Vector3(0f, SpectatorStep, 0f);
					PictureUpdate();
					break;
				case { Key: Key.Add }:
					CameraSpherePosition += new Vector3(-1f, 0f, 0f);
					PictureUpdate();
					break;
				case { Key: Key.Subtract }:
					CameraSpherePosition += new Vector3(1f, 0f, 0f);
					PictureUpdate();
					break;
				case { Key: Key.OemPlus }:
					CameraSpherePosition += new Vector3(-1f, 0f, 0f);
					PictureUpdate();
					break;
				case { Key: Key.OemMinus }:
					CameraSpherePosition += new Vector3(1f, 0f, 0f);
					PictureUpdate();
					break;
				case { Key: Key.Enter }:
					CameraSpherePosition = _radiusPhiTheta;
					PictureUpdate();
					break;
			}
		}
		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			Keyboard.Focus(Picture);
		}
	}
}
