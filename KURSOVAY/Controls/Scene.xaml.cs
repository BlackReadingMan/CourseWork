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
		private readonly Vector3 position = new(0f, 0f, 0f);
		private readonly Vector3 forward = new(0f, 0f, -1f);
		private readonly Vector3 up = new(0f, 1f, 0f);

		private readonly Vector3 Radius_PHI_THETA = new(7f, 0.0f, 0.5f);
		private readonly Vector3 cameraTarget = new(0f, 0f, 0f);
		
		private readonly Vector3 cameraUpVector = new(0f, 1f, 0f);
		private readonly float spectator_step = 0.025f;

		private readonly float fieldOfView = 1f;
		private readonly float nearPlaneDistance = 90f;
		private readonly float farPlaneDistance = 100f;

		private readonly float x = 0f;
		private readonly float y = 0f;
		private readonly float minDepth = -56;
		private readonly float maxDepth = 56;

		private Vector3 lightPosition;
		private readonly Vector3 lightColor = new(1f, 1f, 1f);
		private readonly Vector3 objectColor = new(1f, 1f, 1f);

		private readonly Matrix4x4 world;
		private Matrix4x4 view;
		private Matrix4x4 projection;
		private Matrix4x4 viewport;
		private Matrix4x4 final;
		private Queue<Thread> threads = [];
		private Queue<Polygon> polygons = [];
		private Dictionary<Tuple<int, int>, Tuple<double, Color>> zbuffer;
		public Scene()
		{
			InitializeComponent();
			CameraSpherePosition = Radius_PHI_THETA;
			world = Matrix4x4.CreateWorld(position, forward, up);
		}
		private WriteableBitmap writeableBitmap;
		private static readonly DependencyProperty PaintedFiguresProperty = DependencyProperty.Register(nameof(PaintedFigures), typeof(Figure), typeof(Scene),
				new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPaintedFigureChanged)));
		public Figure PaintedFigures
		{
			get { return (Figure)GetValue(PaintedFiguresProperty); }
			set 
			{ 
				SetValue(PaintedFiguresProperty, value); 
			}
		}
		private static void OnPaintedFigureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if(((Scene)d).ActualHeight>0)
				((Scene)d).PictureUpdate();
		}
		private Vector3 CameraPosition;
		private Vector3 _cameraSpherePosition;
		private Vector3 CameraSpherePosition
		{
			get { return _cameraSpherePosition; }
			set
			{
				CameraPosition = position + new Vector3(
				(float)(value.X * Math.Sin(value.Y * 2 * Math.PI) * Math.Sin(value.Z * Math.PI)),
				(float)(value.X * Math.Cos(value.Z * Math.PI)),
				(float)(value.X * Math.Cos(value.Y * 2 * Math.PI) * Math.Sin(value.Z * Math.PI)));
				_cameraSpherePosition = value;
				lightPosition = CameraPosition;
			}
		}
		private void PictureUpdate()
		{
			zbuffer = [];
			writeableBitmap = new WriteableBitmap((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Bgra32, null);
			projection = Matrix4x4.CreatePerspectiveFieldOfView(fieldOfView, (float)(ActualWidth / ActualHeight), nearPlaneDistance, farPlaneDistance);
			viewport = Matrix4x4.CreateViewport(x, y, (float)ActualWidth, (float)ActualHeight, minDepth, maxDepth);
			view = Matrix4x4.CreateLookAt(CameraPosition, cameraTarget, cameraUpVector);
			final = world * view * projection * viewport;
			Stopwatch sw = Stopwatch.StartNew();
			RenderPoints();
			Getting();
			MakeOXYZ();
			MakePolygons();
			sw.Stop();
			RenderTimer.Content = sw.ToString();
			Picture.Source = writeableBitmap;
			BulletCatcher.Focus();
		}
		private void RenderPoints()
		{
			foreach (var triangle in PaintedFigures.F)
			{
				Vector3 PolygonPos = new Vector3(
					(PaintedFigures.V[triangle.Item1.Item1 - 1].X + PaintedFigures.V[triangle.Item2.Item1 - 1].X + PaintedFigures.V[triangle.Item3.Item1 - 1].X) / 3f,
					(PaintedFigures.V[triangle.Item1.Item1 - 1].Y + PaintedFigures.V[triangle.Item2.Item1 - 1].Y + PaintedFigures.V[triangle.Item3.Item1 - 1].Y) / 3f,
					(PaintedFigures.V[triangle.Item1.Item1 - 1].Z + PaintedFigures.V[triangle.Item2.Item1 - 1].Z + PaintedFigures.V[triangle.Item3.Item1 - 1].Z) / 3f);
				polygons.Enqueue(new Polygon(
					Algorithms.VectorMatrixMultiplication(PaintedFigures.V[triangle.Item1.Item1 - 1], final),
					Algorithms.VectorMatrixMultiplication(PaintedFigures.V[triangle.Item2.Item1 - 1], final),
					Algorithms.VectorMatrixMultiplication(PaintedFigures.V[triangle.Item3.Item1 - 1], final),
					Algorithms.GetColor(PaintedFigures.VN[triangle.Item1.Item3 - 1], PolygonPos, lightPosition, lightColor, objectColor)));
				threads.Enqueue(new Thread(polygons.Last().MakeFill));
				threads.Last().Start();
			}
		}
		private void Getting()
		{
			while (threads.Count != 0)
			{
				if (threads.Count != 0 && threads.First().ThreadState == System.Threading.ThreadState.Stopped)
				{
					foreach (var pixel in polygons.First().buffer)
					{
						Tuple<int, int> key = new(pixel.Key.Item1, pixel.Key.Item2);
						if (zbuffer.TryGetValue(key, out Tuple<double, Color>? value))
						{
							if (value.Item1 <= pixel.Value.Item1)
								zbuffer[key] = pixel.Value;
						}
						if (value == null)
							zbuffer.Add(pixel.Key, pixel.Value);
					}
					threads.Dequeue();
					polygons.Dequeue();
				}
			}
		}
		private void MakePolygons()
		{
			foreach (var pixel in zbuffer.Where(x => x.Key.Item1 < ActualWidth && x.Key.Item1 >= 0 && x.Key.Item2 < ActualHeight && x.Key.Item2 >= 0))
			{
				writeableBitmap.WritePixels(new Int32Rect(pixel.Key.Item1, pixel.Key.Item2, 1, 1),
					new byte[] { pixel.Value.Item2.B, pixel.Value.Item2.G, pixel.Value.Item2.R, 255 }, 4, 0);
			}
		}
		private void MakeOXYZ()
		{
			Vector4 vectorO = Algorithms.VectorMatrixMultiplication(new Vector4(0, 0, 0, 1), final);
			Vector4 vectorX = Algorithms.VectorMatrixMultiplication(new Vector4(5, 0, 0, 1), final);
			Vector4 vectorY = Algorithms.VectorMatrixMultiplication(new Vector4(0, 5, 0, 1), final);
			Vector4 vectorZ = Algorithms.VectorMatrixMultiplication(new Vector4(0, 0, 5, 1), final);
			foreach (var item in Algorithms.CDA(new Point(vectorO.X, vectorO.Y), new Point(vectorX.X, vectorX.Y)))
			{
				var key = new Tuple<int, int>((int)Math.Round(item.X, MidpointRounding.AwayFromZero), (int)Math.Round(item.Y,MidpointRounding.AwayFromZero));

				if (zbuffer.ContainsKey(key))
				{
					zbuffer[key] = new Tuple<double, Color>(0, Color.FromRgb(255, 0, 0));
				}
				else
				{
					zbuffer.Add(key, new Tuple<double, Color>(0, Color.FromRgb(255, 0, 0)));
				}
			}
			foreach (var item in Algorithms.CDA(new Point(vectorO.X, vectorO.Y), new Point(vectorY.X, vectorY.Y)))
			{
				var key = new Tuple<int, int>((int)Math.Round(item.X, MidpointRounding.AwayFromZero), (int)Math.Round(item.Y, MidpointRounding.AwayFromZero));

				if (zbuffer.ContainsKey(key))
				{
					zbuffer[key] = new Tuple<double, Color>(0, Color.FromRgb(0, 255, 0));
				}
				else
				{
					zbuffer.Add(key, new Tuple<double, Color>(0, Color.FromRgb(0, 255, 0)));
				}
			}
			foreach (var item in Algorithms.CDA(new Point(vectorO.X, vectorO.Y), new Point(vectorZ.X, vectorZ.Y)))
			{
				var key = new Tuple<int, int>((int)Math.Round(item.X, MidpointRounding.AwayFromZero), (int)Math.Round(item.Y, MidpointRounding.AwayFromZero));

				if (zbuffer.ContainsKey(key))
				{
					zbuffer[key] = new Tuple<double, Color>(0, Color.FromRgb(0, 0, 255));
				}
				else
				{
					zbuffer.Add(key, new Tuple<double, Color>(0, Color.FromRgb(0, 0, 255)));
				}
			}
		}
		private void Win_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Up:
					CameraSpherePosition += new Vector3(0f, 0f, -spectator_step);
					PictureUpdate();
					break;
				case Key.Down:
					CameraSpherePosition += new Vector3(0f, 0f, spectator_step);
					PictureUpdate();
					break;
				case Key.Left:
					CameraSpherePosition += new Vector3(0f, -spectator_step, 0f);
					PictureUpdate();
					break;
				case Key.Right:
					CameraSpherePosition += new Vector3(0f, spectator_step, 0f);
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
			}
		}
	}
}
