using CourseWork.Algorithms;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Media;

namespace CourseWork.CustomDataTypes
{
	internal class Renderer
	{
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
		public Obj? PaintedObj { private get; set; }
		public Settings? Settings { private get; set; }
		public Dictionary<Tuple<int, int>, Tuple<double, Color>> ZBuffer { get; private set; } = [];
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
						value.Z += Settings.SpectatorStep;
						break;
					case >= 1:
						value.Z -= Settings.SpectatorStep;
						break;
				}
				_cameraSpherePosition = value;
				CreateCameraWorldPosition();
			}
		}

		private void CreateCameraWorldPosition()
		{
			_cameraPosition = Settings._cameraTarget + new Vector3(
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
			_viewport = Matrix4X4Extension.CreateViewport(Settings.X0, Settings.Y0, (float)Size.Width,
				(float)Size.Height, Settings.MinDepth,
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
									new Vector3(pixel.Key.Item1, pixel.Key.Item2, (float)pixel.Value),
									Settings._lightPosition,
									Settings._lightColor, Settings._objectColor));
					}

					if (value == null)
						ZBuffer.Add(pixel.Key, new Tuple<double, Color>(pixel.Value,
							Algorithms.Algorithms.GetColor(_polygons.First().Normal,
								new Vector3(pixel.Key.Item1, pixel.Key.Item2, (float)pixel.Value),
								Settings._lightPosition,
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
		public void PictureUpdate(Size size, Vector3 cameraTurn)
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
}

