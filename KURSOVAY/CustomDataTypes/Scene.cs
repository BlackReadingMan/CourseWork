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
		private Vector3 cameraPosition = new(-3f, 2f, 6f);
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
		private readonly Matrix4x4 view;
		private Matrix4x4 projection;
		private Matrix4x4 viewport;
		private Matrix4x4 final;
		private Queue<Thread> threads;
		private Queue<Triangle> triangles;
		private Dictionary<Tuple<int, int>, Tuple<float, Color>> zbuffer;
		private bool load;
		public bool ready = false;
		public Scene(Figure figure, Size _size)
		{
			Size = _size;
			paintedFigures = figure;
			world = Matrix4x4.CreateWorld(position, forward, up);
			view = Matrix4x4.CreateLookAt(cameraPosition, cameraTarget, cameraUpVector);
			lightPosition = cameraPosition;
			final = world * view * projection * viewport;
			RenderPoints();
		}
		private WriteableBitmap _wb;
		public WriteableBitmap WB
		{
			get
			{
				return _wb;
			}
			private set { _wb = value; }
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
				projection = Matrix4x4.CreatePerspectiveFieldOfView(fieldOfView, (float)(value.Width / value.Height), nearPlaneDistance, farPlaneDistance);
				viewport = Matrix4x4.CreateViewport(x, y, (float)value.Width, (float)value.Height, minDepth, maxDepth);
				WB = new WriteableBitmap((int)value.Width, (int)value.Height, 96, 96, PixelFormats.Bgra32, null);
				_size = value;
			}
		}
		private void RenderPoints()
		{
			zbuffer = [];
			load = true;
			threads = [];
			triangles = [];
			Getting();
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
					Algorithms.GetColor(paintedFigures.VN[triangle.Item1.Item3 - 1], new(FragPos.X, FragPos.Y, FragPos.Z), new(lightPosition.X, lightPosition.Y, lightPosition.Z), lightColor, objectColor)));
				threads.Enqueue(new Thread(triangles.Last().MakeFill));
				threads.Last().Start();
			}
			load = false;
		}
		private async void Getting()
		{
			if (threads.Count != 0 || load)
			{
				if (threads.Count != 0 && threads.First().ThreadState == ThreadState.Stopped)
				{
					foreach (var pixels in triangles.First().buffer)
					{
						Tuple<int, int> key = new(pixels.Key.Item1, pixels.Key.Item2);
						if (zbuffer.TryGetValue(key, out Tuple<float, Color>? value))
						{
							if (value.Item1 <= pixels.Value.Item1)
								zbuffer[key] = pixels.Value;
						}
						if (value == null)
							zbuffer.Add(pixels.Key, pixels.Value);
					}
					threads.Dequeue();
					triangles.Dequeue();


				}
				await Task.Delay(5);
				Getting();
			}
			else
			{
				MakePolygons();
			}
		}
		private void MakePolygons()
		{
			foreach (var pixel in zbuffer.Where(x => x.Key.Item1 < Size.Width && x.Key.Item1 >= 0 && x.Key.Item2 < Size.Height && x.Key.Item2 >= 0))
			{
				WB.WritePixels(new Int32Rect(pixel.Key.Item1, pixel.Key.Item2, 1, 1), new byte[] { pixel.Value.Item2.B, pixel.Value.Item2.G, pixel.Value.Item2.R, 255 }, 4, 0);
			}
			ready = true;
		}
	}
}
