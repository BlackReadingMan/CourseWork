using CourseWork.Algorithms;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CourseWork.CustomDataTypes
{
	internal class Renderer
	{
		private Matrix4x4 _world;
		private Matrix4x4 _scale;
		private Matrix4x4 _view;
		private Matrix4x4 _projection;
		private Matrix4x4 _viewport;
		private Matrix4x4 _final;
		private readonly Queue<Thread> _threads = [];
		private readonly Queue<Polygon> _polygons = [];
		private readonly Stopwatch _stopwatch = new();
		private Vector3 _cameraPosition;
		private Vector3 _lightPosition;
		private Vector3 _cameraSpherePosition;
		private Dictionary<Tuple<int, int>, Tuple<double, Color>> _zBuffer = [];
		public Obj PaintedObj { private get; set; } = new();
		public Settings RenderSettings { private get; set; } = new();
		public string RenderTime { get; private set; } = "";
		private Size Size { get; set; }

		private Vector3 CameraSpherePosition
		{
			get => _cameraSpherePosition;
			set
			{
				if (value.X <= 0) value.X += 1;
				switch (value.Y)
				{
					case < 0:
						value.Y += 1f;
						break;
					case > 1:
						value.Y -= 1f;
						break;
				}

				switch (value.Z)
				{
					case <= 0:
						value.Z += RenderSettings.SpectatorStep;
						break;
					case >= 1:
						value.Z -= RenderSettings.SpectatorStep;
						break;
				}

				_cameraSpherePosition = value;
				CreateCameraWorldPosition();
				CheckLightPosition();

			}
		}

		private void CheckLightPosition()
		{
			_lightPosition = RenderSettings.LightFollowCamera ? _cameraPosition : RenderSettings._lightPosition;
		}

		private void CreateCameraWorldPosition()
		{
			_cameraPosition = RenderSettings._cameraTarget + new Vector3(
				(float)(_cameraSpherePosition.X * Math.Sin(_cameraSpherePosition.Y * 2 * Math.PI) *
				        Math.Sin(_cameraSpherePosition.Z * Math.PI)),
				(float)(_cameraSpherePosition.X * Math.Cos(_cameraSpherePosition.Z * Math.PI)),
				(float)(_cameraSpherePosition.X * Math.Cos(_cameraSpherePosition.Y * 2 * Math.PI) *
				        Math.Sin(_cameraSpherePosition.Z * Math.PI))
			);
		}

		private void CameraTurn(in Vector3 cameraTurn)
		{
			if (cameraTurn == new Vector3(-1f, -1f, -1f))
				CameraSpherePosition = RenderSettings._radiusPhiTheta;
			else
				CameraSpherePosition +=
					cameraTurn * new Vector3(1f, RenderSettings.SpectatorStep, RenderSettings.SpectatorStep);
		}

		private void DataUpdate(in Size size, in Vector3 cameraTurn)
		{
			Size = size;
			_zBuffer = [];
			CameraTurn(cameraTurn);
			_world = Matrix4X4Extension.CreateWorld(RenderSettings._position, RenderSettings._forward,
				RenderSettings._up);
			_scale = Matrix4X4Extension.CreateScale(RenderSettings.Scale);
			_view = Matrix4X4Extension.CreateLookAt(_cameraPosition, RenderSettings._cameraTarget,
				RenderSettings._cameraUpVector);
			_projection = Matrix4X4Extension.CreatePerspectiveFieldOfView(RenderSettings.FieldOfView,
				(float)(Size.Width / Size.Height), RenderSettings.NearPlaneDistance, RenderSettings.FarPlaneDistance);

			_viewport = Matrix4X4Extension.CreateViewport(RenderSettings.X0, RenderSettings.Y0, (float)Size.Width,
				(float)Size.Height, RenderSettings.MinDepth,
				RenderSettings.MaxDepth);
			_final = _world * _scale * _view * _projection * _viewport;
			_stopwatch.Reset();
		}

		private void RenderPoints()
		{
			var nullWorld = Matrix4X4Extension.CreateWorld(new Vector3(0, 0, 0),
				RenderSettings._forward, RenderSettings._up);

			foreach (var triangle in PaintedObj.F)
			{
				var newNormal = Vector3.Normalize(
					Matrix4X4Extension.VectorMatrixMultiplication(
						PaintedObj.Vn[triangle.Item1.Item3 - 1],
						nullWorld));
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
					Algorithms.Algorithms.GetColor(newNormal, PaintedObj.V[triangle.Item1.Item1 - 1],
						_lightPosition,
						RenderSettings._lightColor, RenderSettings._objectColor),
					Algorithms.Algorithms.GetColor(newNormal, PaintedObj.V[triangle.Item2.Item1 - 1]
						,
						_lightPosition,
						RenderSettings._lightColor, RenderSettings._objectColor),
					Algorithms.Algorithms.GetColor(newNormal, PaintedObj.V[triangle.Item3.Item1 - 1]
						,
						_lightPosition,
						RenderSettings._lightColor, RenderSettings._objectColor)
				));
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

		public async Task<WriteableBitmap> GetPictureAsync(Size size, Vector3 cameraTurn)
		{
			DataUpdate(size, cameraTurn);
			_stopwatch.Start();
			await Task.Run(RenderPoints);
			await Task.Run(MakeZBuffer);
			BuildCoordinateAxes();
			var result = DrawPicture();
			_stopwatch.Stop();
			RenderTime = _stopwatch.ToString();
			return result;
		}

		private WriteableBitmap DrawPicture()
		{
			var image = new WriteableBitmap((int)Size.Width, (int)Size.Height, 96, 96, PixelFormats.Bgra32, null);
			foreach (var pixel in _zBuffer.Where(x =>
				         x.Key.Item1 < Size.Width && x.Key.Item1 >= 0 && x.Key.Item2 < Size.Height &&
				         x.Key.Item2 >= 0 && x.Value.Item2 != RenderSettings._backGroundColor))
			{
				image.WritePixels(new Int32Rect(pixel.Key.Item1, pixel.Key.Item2, 1, 1),
					new byte[] { pixel.Value.Item2.B, pixel.Value.Item2.G, pixel.Value.Item2.R, 255 }, 4, 0);
			}

			return image;
		}
	}
}

