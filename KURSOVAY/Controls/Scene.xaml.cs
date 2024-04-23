using KURSOVAY.Algorithm;
using KURSOVAY.CustomDataTypes;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KURSOVAY.Controls
{
	/// <summary>
	/// Логика взаимодействия для Scene.xaml
	/// </summary>
	public partial class Scene : UserControl
	{
		private readonly Vector3 _position = new(0f, 0f, 0f);
		private readonly Vector3 _forward = new(0f, 0f, -1f);
		private readonly Vector3 _up = new(0f, 1f, 0f);

		private readonly Vector3 _radius_PHI_THETA = new(7f, 0.0f, 0.5f);
		private readonly Vector3 _cameraTarget = new(0f, 0f, 0f);

		private readonly Vector3 _cameraUpVector = new(0f, 1f, 0f);
		private readonly float _spectator_step = 0.025f;

		private readonly float _fieldOfView = 1f;
		private readonly float _nearPlaneDistance = 90f;
		private readonly float _farPlaneDistance = 100f;

		private readonly float _x = 0f;
		private readonly float _y = 0f;
		private readonly float _minDepth = -56;
		private readonly float _maxDepth = 56;

		private Vector3 _lightPosition;
		private readonly Vector3 _lightColor = new(1f, 1f, 1f);
		private readonly Vector3 _objectColor = new(1f, 1f, 1f);

		private Matrix4x4 _world;
		private Matrix4x4 _view;
		private Matrix4x4 _projection;
		private Matrix4x4 _viewport;
		private Matrix4x4 _final;
		private Queue<Thread> _threads = [];
		private Queue<Polygon> _polygons = [];
		private Dictionary<Tuple<int, int>, Tuple<double, Color>> _zbuffer;
		private WriteableBitmap _image;
		private Stopwatch sw = new Stopwatch();
		public Scene()
		{
			InitializeComponent();
		}
		private Vector3 _cameraPosition;
		private Vector3 _cameraSpherePosition;
		private Vector3 CameraSpherePosition
		{
			get { return _cameraSpherePosition; }
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
			_zbuffer = [];
			_image = new WriteableBitmap((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Bgra32, null);
			_world = Matrix4x4.CreateWorld(_position, _forward, _up);
			_projection = Matrix4x4.CreatePerspectiveFieldOfView(_fieldOfView, (float)(ActualWidth / ActualHeight), _nearPlaneDistance, _farPlaneDistance);
			_viewport = Matrix4x4.CreateViewport(_x, _y, (float)ActualWidth, (float)ActualHeight, _minDepth, _maxDepth);
			_view = Matrix4x4.CreateLookAt(_cameraPosition, _cameraTarget, _cameraUpVector);
			_final = _world * _view * _projection * _viewport;
			sw.Reset();
		}
		private void RenderPoints()
		{
			foreach (var triangle in PaintedFigures.F)
			{
				Vector3 PolygonPos = new Vector3(
					(PaintedFigures.V[triangle.Item1.Item1 - 1].X + PaintedFigures.V[triangle.Item2.Item1 - 1].X + PaintedFigures.V[triangle.Item3.Item1 - 1].X) / 3f,
					(PaintedFigures.V[triangle.Item1.Item1 - 1].Y + PaintedFigures.V[triangle.Item2.Item1 - 1].Y + PaintedFigures.V[triangle.Item3.Item1 - 1].Y) / 3f,
					(PaintedFigures.V[triangle.Item1.Item1 - 1].Z + PaintedFigures.V[triangle.Item2.Item1 - 1].Z + PaintedFigures.V[triangle.Item3.Item1 - 1].Z) / 3f);
				_polygons.Enqueue(new Polygon(
					Algorithms.VectorMatrixMultiplication(PaintedFigures.V[triangle.Item1.Item1 - 1], _final),
					Algorithms.VectorMatrixMultiplication(PaintedFigures.V[triangle.Item2.Item1 - 1], _final),
					Algorithms.VectorMatrixMultiplication(PaintedFigures.V[triangle.Item3.Item1 - 1], _final),
					Algorithms.GetColor(PaintedFigures.VN[triangle.Item1.Item3 - 1], PolygonPos, _lightPosition, _lightColor, _objectColor)));
				_threads.Enqueue(new Thread(_polygons.Last().MakeFill));
				_threads.Last().Start();
			}
		}
		private void MakeZBuffer()
		{
			while (_threads.Count != 0)
			{
				if (_threads.Count != 0 && _threads.First().ThreadState == System.Threading.ThreadState.Stopped)
				{
					foreach (var pixel in _polygons.First().buffer)
					{
						Tuple<int, int> key = new(pixel.Key.Item1, pixel.Key.Item2);
						if (_zbuffer.TryGetValue(key, out Tuple<double, Color>? value))
						{
							if (value.Item1 <= pixel.Value.Item1)
								_zbuffer[key] = pixel.Value;
						}
						if (value == null)
							_zbuffer.Add(pixel.Key, pixel.Value);
					}
					_threads.Dequeue();
					_polygons.Dequeue();
				}
			}
		}
		private void MakeOXYZ()
		{
			Vector4 vectorO = Algorithms.VectorMatrixMultiplication(new Vector4(0, 0, 0, 1), _final);
			Vector4 vectorX = Algorithms.VectorMatrixMultiplication(new Vector4(5, 0, 0, 1), _final);
			Vector4 vectorY = Algorithms.VectorMatrixMultiplication(new Vector4(0, 5, 0, 1), _final);
			Vector4 vectorZ = Algorithms.VectorMatrixMultiplication(new Vector4(0, 0, 5, 1), _final);
			foreach (var item in Algorithms.CDA(new Point(vectorO.X, vectorO.Y), new Point(vectorX.X, vectorX.Y)))
			{
				var key = new Tuple<int, int>((int)Math.Round(item.X, MidpointRounding.AwayFromZero), (int)Math.Round(item.Y, MidpointRounding.AwayFromZero));

				if (_zbuffer.ContainsKey(key))
				{
					_zbuffer[key] = new Tuple<double, Color>(0, Color.FromRgb(255, 0, 0));
				}
				else
				{
					_zbuffer.Add(key, new Tuple<double, Color>(0, Color.FromRgb(255, 0, 0)));
				}
			}
			foreach (var item in Algorithms.CDA(new Point(vectorO.X, vectorO.Y), new Point(vectorY.X, vectorY.Y)))
			{
				var key = new Tuple<int, int>((int)Math.Round(item.X, MidpointRounding.AwayFromZero), (int)Math.Round(item.Y, MidpointRounding.AwayFromZero));

				if (_zbuffer.ContainsKey(key))
				{
					_zbuffer[key] = new Tuple<double, Color>(0, Color.FromRgb(0, 255, 0));
				}
				else
				{
					_zbuffer.Add(key, new Tuple<double, Color>(0, Color.FromRgb(0, 255, 0)));
				}
			}
			foreach (var item in Algorithms.CDA(new Point(vectorO.X, vectorO.Y), new Point(vectorZ.X, vectorZ.Y)))
			{
				var key = new Tuple<int, int>((int)Math.Round(item.X, MidpointRounding.AwayFromZero), (int)Math.Round(item.Y, MidpointRounding.AwayFromZero));

				if (_zbuffer.ContainsKey(key))
				{
					_zbuffer[key] = new Tuple<double, Color>(0, Color.FromRgb(0, 0, 255));
				}
				else
				{
					_zbuffer.Add(key, new Tuple<double, Color>(0, Color.FromRgb(0, 0, 255)));
				}
			}
		}
		private void MakeImage()
		{
			foreach (var pixel in _zbuffer.Where(x => x.Key.Item1 < ActualWidth && x.Key.Item1 >= 0 && x.Key.Item2 < ActualHeight && x.Key.Item2 >= 0))
			{
				_image.WritePixels(new Int32Rect(pixel.Key.Item1, pixel.Key.Item2, 1, 1),
					new byte[] { pixel.Value.Item2.B, pixel.Value.Item2.G, pixel.Value.Item2.R, 255 }, 4, 0);
			}
		}
		private void DataToUI()
		{
			RenderTimer.Content = sw.ToString();
			Picture.Source = _image;
		}
		private void PictureUpdate()
		{
			DataUpdate();
			sw.Start();
			RenderPoints();
			MakeZBuffer();
			MakeOXYZ();
			MakeImage();
			sw.Stop();
			DataToUI();
		}
		private static readonly DependencyProperty PaintedFiguresProperty = DependencyProperty.Register(nameof(PaintedFigures), typeof(Figure), typeof(Scene));
		public Figure PaintedFigures
		{
			get { return (Figure)GetValue(PaintedFiguresProperty); }
			set
			{
				SetValue(PaintedFiguresProperty, value);
			}
		}
		private void Win_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Up:
					CameraSpherePosition += new Vector3(0f, 0f, -_spectator_step);
					PictureUpdate();
					break;
				case Key.Down:
					CameraSpherePosition += new Vector3(0f, 0f, _spectator_step);
					PictureUpdate();
					break;
				case Key.Left:
					CameraSpherePosition += new Vector3(0f, -_spectator_step, 0f);
					PictureUpdate();
					break;
				case Key.Right:
					CameraSpherePosition += new Vector3(0f, _spectator_step, 0f);
					PictureUpdate();
					break;
				case Key.Add:
					CameraSpherePosition += new Vector3(-1f, 0f, 0f);
					PictureUpdate();
					break;
				case Key.Subtract:
					CameraSpherePosition += new Vector3(1f, 0f, 0f);
					PictureUpdate();
					break;
				case Key.Enter:
					CameraSpherePosition = _radius_PHI_THETA;
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
