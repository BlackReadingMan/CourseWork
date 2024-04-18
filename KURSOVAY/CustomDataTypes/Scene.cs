using KURSOVAY.Algorithm;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KURSOVAY.CustomDataTypes
{
	internal class Scene
	{
		private readonly Vector3 position = new(0f, 0f, 0f);
		private readonly Vector3 forward = new(0f, 0f, -1f);
		private readonly Vector3 up = new(0f, 1f, 0f);

		private Vector3 _cameraPosition = new(-3f, 3f, 6f);
		private readonly Vector3 cameraTarget = new(0f, 0f, 0f);
		private readonly Vector3 cameraUpVector = new(0f, 1f, 0f);

		private readonly float fieldOfView = 1f;
		private readonly float nearPlaneDistance = 90f;
		private readonly float farPlaneDistance = 100f;

		private readonly float x = 0f;
		private readonly float y = 0f;
		private readonly float minDepth = 0;
		private readonly float maxDepth = 512;

		private Vector3 lightPosition;
		private readonly Vector3 lightColor = new(1f, 1f, 1f);
		private readonly Vector3 objectColor = new(1f, 1f, 1f);

		private readonly Matrix4x4 world;
		private Matrix4x4 view;
		private readonly Matrix4x4 projection;
		private readonly Matrix4x4 viewport;
		private Matrix4x4 final;
		private Queue<Thread> threads = [];
		private Queue<Triangle> triangles = [];
		private Dictionary<Tuple<int, int>, Tuple<float, Color>> zbuffer = [];
		public Scene(Figure figure, Size _size)
		{
			Size = _size;
			paintedFigures = figure;
			lightPosition = CameraPosition;
			world = Matrix4x4.CreateWorld(position, forward, up);
			projection = Matrix4x4.CreatePerspectiveFieldOfView(fieldOfView, (float)(Size.Width / Size.Height), nearPlaneDistance, farPlaneDistance);
			viewport = Matrix4x4.CreateViewport(x, y, (float)Size.Width, (float)Size.Height, minDepth, maxDepth);
		}
		private WriteableBitmap _wb;
		public WriteableBitmap WB
		{
			get
			{
				return _wb;
			}
			private set
			{
				_wb = value;
			}
		}
		private Figure paintedFigures;
		private Size _size;
		private Size Size
		{
			get
			{
				return _size;
			}
			set
			{
				_size = value;
			}
		}
		public Vector3 CameraPosition
		{
			get { return _cameraPosition; }
			set
			{
				_cameraPosition = value;
				lightPosition = value;
			}
		}
		public void PictureUpdate()
		{
			WB = new WriteableBitmap((int)Size.Width, (int)Size.Height, 96, 96, PixelFormats.Bgra32, null);
			view = Matrix4x4.CreateLookAt(CameraPosition, cameraTarget, cameraUpVector);
			final = world * view * projection * viewport;
			RenderPoints();
		}
		private void RenderPoints()
		{
			foreach (var triangle in paintedFigures.F)
			{
				Vector3 FragPos = new Vector3(
					((paintedFigures.V[triangle.Item1.Item1 - 1].X + paintedFigures.V[triangle.Item2.Item1 - 1].X + paintedFigures.V[triangle.Item3.Item1 - 1].X) / 3f),
					((paintedFigures.V[triangle.Item1.Item1 - 1].Y + paintedFigures.V[triangle.Item2.Item1 - 1].Y + paintedFigures.V[triangle.Item3.Item1 - 1].Y) / 3f),
					((paintedFigures.V[triangle.Item1.Item1 - 1].Z + paintedFigures.V[triangle.Item2.Item1 - 1].Z + paintedFigures.V[triangle.Item3.Item1 - 1].Z) / 3f));
				triangles.Enqueue(new Triangle(
					Algorithms.MakeWorldPoint(paintedFigures.V[triangle.Item1.Item1 - 1], final),
					Algorithms.MakeWorldPoint(paintedFigures.V[triangle.Item2.Item1 - 1], final),
					Algorithms.MakeWorldPoint(paintedFigures.V[triangle.Item3.Item1 - 1], final),
					Algorithms.GetColor(paintedFigures.VN[triangle.Item1.Item3 - 1], FragPos, lightPosition, lightColor, objectColor)));
				threads.Enqueue(new Thread(triangles.Last().MakeFill));
				threads.Last().Start();
			}
			Getting();
		}
		private void Getting()
		{
			while (threads.Count != 0)
			{
				if (threads.Count != 0 && threads.First().ThreadState == ThreadState.Stopped)
				{
					foreach (var pixel in triangles.First().buffer)
					{
						Tuple<int, int> key = new(pixel.Key.Item1, pixel.Key.Item2);
						if (zbuffer.TryGetValue(key, out Tuple<float, Color>? value))
						{
							if (value.Item1 <= pixel.Value.Item1)
								zbuffer[key] = pixel.Value;
						}
						if (value == null)
							zbuffer.Add(pixel.Key, pixel.Value);
					}
					threads.Dequeue();
					triangles.Dequeue();
				}
			}
			MakePolygons();
		}
		private void MakePolygons()
		{
			foreach (var pixel in zbuffer.Where(x => x.Key.Item1 < Size.Width && x.Key.Item1 >= 0 && x.Key.Item2 < Size.Height && x.Key.Item2 >= 0))
			{
				WB.WritePixels(new Int32Rect(pixel.Key.Item1, pixel.Key.Item2, 1, 1), new byte[] { pixel.Value.Item2.B, pixel.Value.Item2.G, pixel.Value.Item2.R, 255 }, 4, 0);
			}
		}
	}
}
