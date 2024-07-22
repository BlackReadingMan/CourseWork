using CourseWork.Algorithms;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CourseWork.CustomDataTypes;

internal class Renderer
{
	private Matrix4x4 _world;
	private Matrix4x4 _scale;
	private Matrix4x4 _view;
	private Matrix4x4 _projection;
	private Matrix4x4 _viewport;
	private Matrix4x4 _final;
	private Vector3 _cameraPosition;
	private Vector3 _lightPosition;
	private Vector3 _cameraSpherePosition;
	private Size _size;
	private readonly Stopwatch _stopwatch = new();
	private Dictionary<Tuple<int, int>, Tuple<double, Color>> _zBuffer = [];
	public Obj PaintedObj { private get; set; } = new();
	public Settings RenderSettings { private get; set; } = new();
	public string RenderTime { get; private set; } = "";
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
					value.Z += RenderSettings.SpectatorStep.Z;
					break;
				case >= 1:
					value.Z -= RenderSettings.SpectatorStep.Z;
					break;
			}

			_cameraSpherePosition = value;
			CreateCameraWorldPosition();
			CheckLightPosition();

		}
	}

	private void CheckLightPosition()
	{
		_lightPosition = RenderSettings.IsLightFollowsCamera ? _cameraPosition : RenderSettings.LightPosition;
	}

	private void CreateCameraWorldPosition()
	{
		_cameraPosition = RenderSettings.CameraTarget + new Vector3(
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
			CameraSpherePosition = RenderSettings.RadiusPhiTheta;
		else
			CameraSpherePosition +=
				cameraTurn * RenderSettings.SpectatorStep;
	}

	private void DataUpdate(in Size size, in Vector3 cameraTurn)
	{
		_size = size;
		_zBuffer = [];
		CameraTurn(cameraTurn);
		_world = Matrix4X4Extension.CreateWorld(RenderSettings.Position, RenderSettings.Forward,
			RenderSettings.Up);
		_scale = Matrix4X4Extension.CreateScale(RenderSettings.Scale);
		_view = Matrix4X4Extension.CreateLookAt(_cameraPosition, RenderSettings.CameraTarget,
			RenderSettings.CameraUpVector);
		_projection = Matrix4X4Extension.CreatePerspectiveFieldOfView(RenderSettings.FieldOfView,
			(float)(_size.Width / _size.Height), RenderSettings.NearPlaneDistance, RenderSettings.FarPlaneDistance);

		_viewport = Matrix4X4Extension.CreateViewport(RenderSettings.X0, RenderSettings.Y0, (float)_size.Width,
			(float)_size.Height, RenderSettings.MinDepth,
			RenderSettings.MaxDepth);
		_final = _world * _scale * _view * _projection * _viewport;
		_stopwatch.Reset();
	}

	private async Task RenderPointsAsync()
	{
		var renderTasks = new List<Task>();
		var nullWorld = Matrix4X4Extension.CreateWorld(new Vector3(0, 0, 0),
			RenderSettings.Forward, RenderSettings.Up);
		foreach (var polygon in from triangle in PaintedObj.F
								let newNormal = Vector3.Normalize(
									Matrix4X4Extension.VectorMatrixMultiplication(
										PaintedObj.Vn[triangle.Item1.Item3 - 1],
										nullWorld))
								select new Polygon(
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
										RenderSettings.LightColor, RenderSettings.ObjectColor),
									Algorithms.Algorithms.GetColor(newNormal, PaintedObj.V[triangle.Item2.Item1 - 1]
										,
										_lightPosition,
										RenderSettings.LightColor, RenderSettings.ObjectColor),
									Algorithms.Algorithms.GetColor(newNormal, PaintedObj.V[triangle.Item3.Item1 - 1]
										,
										_lightPosition,
										RenderSettings.LightColor, RenderSettings.ObjectColor), _zBuffer
								))
		{
			renderTasks.Add(new Task(polygon.MakeFill));
			renderTasks.Last().Start();
		}

		await Task.WhenAll(renderTasks);
	}

	private async Task BuildCoordinateAxesAsync()
	{
		var newFinal = Matrix4X4Extension.CreateWorld(new Vector3(0, 0, 0),
						   new Vector3(0, 0, -1), new Vector3(0, 1, 0)) * _view * _projection *
					   _viewport;
		var vectorO = Matrix4X4Extension.VectorMatrixMultiplication(new Vector3(0, 0, 0), newFinal);
		var vectorX = Matrix4X4Extension.VectorMatrixMultiplication(new Vector3(5, 0, 0), newFinal);
		var vectorY = Matrix4X4Extension.VectorMatrixMultiplication(new Vector3(0, 5, 0), newFinal);
		var vectorZ = Matrix4X4Extension.VectorMatrixMultiplication(new Vector3(0, 0, 5), newFinal);
		var linesTasks = new[]
		{
			new Task(() =>
				new Line(vectorO, vectorX, Brushes.Red.Color, Brushes.Red.Color,
					_zBuffer).MakeFill(true)),
			new Task(() =>
				new Line(vectorO, vectorY, Brushes.Lime.Color, Brushes.Lime.Color,
					_zBuffer).MakeFill(true)),
			new Task(() =>
				new Line(vectorO, vectorZ, Brushes.Blue.Color, Brushes.Blue.Color,
					_zBuffer).MakeFill(true))
		};
		foreach (var linesTask in linesTasks)
		{
			linesTask.Start();
		}

		await Task.WhenAll(linesTasks);
	}

	private WriteableBitmap DrawPicture()
	{
		var image = new WriteableBitmap((int)_size.Width, (int)_size.Height, 96, 96, PixelFormats.Bgra32, null);
		foreach (var pixel in _zBuffer.Where(x =>
					 x.Key.Item1 < (int)_size.Width && x.Key.Item1 >= 0 && x.Key.Item2 < (int)_size.Height &&
					 x.Key.Item2 >= 0 && x.Value.Item2 !=Algorithms.Algorithms.VectorToColor( RenderSettings.BackGroundColor)))
		{
			image.WritePixels(new Int32Rect(pixel.Key.Item1, pixel.Key.Item2, 1, 1),
				new byte[] { pixel.Value.Item2.B, pixel.Value.Item2.G, pixel.Value.Item2.R, 255 }, 4, 0);
		}
		return image;
	}

	public async Task<WriteableBitmap> GetPictureAsync(Size size, Vector3 cameraTurn)
	{
		DataUpdate(size, cameraTurn);
		_stopwatch.Start();
		await RenderPointsAsync();
		await BuildCoordinateAxesAsync();
		var result = DrawPicture();
		_stopwatch.Stop();
		RenderTime = _stopwatch.ToString();
		return result;
	}
}