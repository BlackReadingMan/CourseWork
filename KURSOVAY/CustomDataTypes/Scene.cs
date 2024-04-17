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
		private Vector3 cameraPosition = new(-3f, 2f, 5f);
		private readonly Vector3 cameraTarget = new(0f, 0f, 0f);
		private readonly Vector3 cameraUpVector = new(0f, 1f, 0f);
		private readonly float fieldOfView = 1f;
		private readonly float nearPlaneDistance = 90f;
		private readonly float farPlaneDistance = 100f;
		private readonly float x = 0f;
		private readonly float y = 400f;
		private readonly float minDepth = 0;
		private readonly float maxDepth = 1024;
		private Vector3 lightPos;
		private readonly Vector3 lightColor = new(1f, 1f, 1f);
		private readonly Vector3 objectColor = new(1f, 1f, 1f);
		private readonly Matrix4x4 world;
		private readonly Matrix4x4 view;
		private Matrix4x4 projection;
		private Matrix4x4 viewport;
		public Scene(Figure figure)
		{
			PaintedFigures = figure;
			world = Matrix4x4.CreateWorld(position, forward, up);
			view = Matrix4x4.CreateLookAt(cameraPosition, cameraTarget, cameraUpVector);
			lightPos=cameraPosition;
		}
		private WriteableBitmap _wb;
		public WriteableBitmap WB
		{
			get => _wb;
			set
			{
				_wb = value;
			}
		}
		private Figure _paintedFigures;
		public Figure PaintedFigures
		{
			get => _paintedFigures;
			set
			{
				_paintedFigures = value;

			}
		}
		private Point _size;
		public Point Size
		{
			get => _size;
			set
			{
				_size = value;
				projection = Matrix4x4.CreatePerspectiveFieldOfView(fieldOfView, (float)(value.X / value.Y), nearPlaneDistance, farPlaneDistance);
				viewport = Matrix4x4.CreateViewport(x, y, (float)value.X, (float)value.Y, minDepth, maxDepth);
				WB = new WriteableBitmap((int)Size.X, (int)Size.X, 96, 96, PixelFormats.Bgra32, null);
				MakePolygons();
			}
		}
		private void MakePolygons()
		{
			Matrix4x4 final = world * view * projection * viewport;
			foreach (var pixel in Algorithms.GetViewPoints(PaintedFigures, final, lightPos, lightColor, objectColor))
			{
					WB.WritePixels(new Int32Rect(pixel.Item1, pixel.Item2, 1, 1), new byte[] { pixel.Item3.B, pixel.Item3.G, pixel.Item3.R, 255 }, 4, 0);
			}
		}
	}
}
