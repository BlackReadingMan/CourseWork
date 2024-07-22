using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using CourseWork.CustomDataTypes;

namespace CourseWork.Algorithms;

internal class Render
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
	private readonly Stopwatch _stopwatch = new();
	public Dictionary<Tuple<int, int>, Tuple<double, Color>> _zBuffer { get; private set; } = [];
	public Obj? PaintedObj { get; set; }
	public Settings? Settings { get; set; }
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
					value.Z += Settings.SpectatorStep.Z;
					break;
				case >= 1:
					value.Z -= Settings.SpectatorStep.Z;
					break;
			}

			_cameraSpherePosition = value;
			CreateCameraWorldPosition();
			CheckLightPosition();

		}
	}
	public async Task RenderPicture(Size size, Vector3 cameraTurn)
	{
		if (Settings is null || PaintedObj is null)
			return;
		DataUpdate(size, cameraTurn);
		_stopwatch.Start();
		await RenderPointsAsync();
		await BuildCoordinateAxesAsync();
		_stopwatch.Stop();
		RenderTime = _stopwatch.ToString();
	}
	private void CheckLightPosition()
	{
		_lightPosition = Settings.IsLightFollowsCamera ? _cameraPosition : Settings.LightPosition;
	}

	private void CreateCameraWorldPosition()
	{
		_cameraPosition = Settings.CameraTarget + new Vector3(
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
			CameraSpherePosition = Settings.RadiusPhiTheta;
		else
			CameraSpherePosition +=
				cameraTurn * Settings.SpectatorStep;
	}

	private void DataUpdate(in Size size, in Vector3 cameraTurn)
	{
		_zBuffer = [];
		CameraTurn(cameraTurn);
		_world = Matrix4x4Calc.CreateWorld(Settings.Position, Settings.Forward,
			Settings.Up);
		_scale = Matrix4x4Calc.CreateScale(Settings.Scale);
		_view = Matrix4x4Calc.CreateLookAt(_cameraPosition, Settings.CameraTarget,
			Settings.CameraUpVector);
		_projection = Matrix4x4Calc.CreatePerspectiveFieldOfView(Settings.FieldOfView,
			(float)(size.Width / size.Height), Settings.NearPlaneDistance, Settings.FarPlaneDistance);

		_viewport = Matrix4x4Calc.CreateViewport(Settings.X0, Settings.Y0, (float)size.Width,
			(float)size.Height, Settings.MinDepth,
			Settings.MaxDepth);
		_final = _world * _scale * _view * _projection * _viewport;
		_stopwatch.Reset();
	}

	private async Task RenderPointsAsync()
	{
		var renderTasks = new List<Task>();
		var nullWorld = Matrix4x4Calc.CreateWorld(new Vector3(0, 0, 0),
			Settings.Forward, Settings.Up);
		foreach (var polygon in from triangle in PaintedObj.F
								let newNormal = Vector3.Normalize(
									Matrix4x4Calc.VectorMatrixMultiplication(
										PaintedObj.Vn[triangle.Item1.Item3 - 1],
										nullWorld))
								select new Polygon(
									Matrix4x4Calc.VectorMatrixMultiplication(
										PaintedObj.V[triangle.Item1.Item1 - 1],
										_final),
									Matrix4x4Calc.VectorMatrixMultiplication(
										PaintedObj.V[triangle.Item2.Item1 - 1],
										_final),
									Matrix4x4Calc.VectorMatrixMultiplication(
										PaintedObj.V[triangle.Item3.Item1 - 1],
										_final),
									Algorithms.GetColor(newNormal, PaintedObj.V[triangle.Item1.Item1 - 1],
										_lightPosition,
										Settings.LightColor, Settings.ObjectColor),
									Algorithms.GetColor(newNormal, PaintedObj.V[triangle.Item2.Item1 - 1]
										,
										_lightPosition,
										Settings.LightColor, Settings.ObjectColor),
									Algorithms.GetColor(newNormal, PaintedObj.V[triangle.Item3.Item1 - 1]
										,
										_lightPosition,
										Settings.LightColor, Settings.ObjectColor), _zBuffer
								))
		{
			renderTasks.Add(new Task(polygon.MakeFill));
			renderTasks.Last().Start();
		}

		await Task.WhenAll(renderTasks);
	}

	private async Task BuildCoordinateAxesAsync()
	{
		var newFinal = Matrix4x4Calc.CreateWorld(new Vector3(0, 0, 0),
						   new Vector3(0, 0, -1), new Vector3(0, 1, 0)) * _view * _projection *
					   _viewport;
		var vectorO = Matrix4x4Calc.VectorMatrixMultiplication(new Vector3(0, 0, 0), newFinal);
		var vectorX = Matrix4x4Calc.VectorMatrixMultiplication(new Vector3(5, 0, 0), newFinal);
		var vectorY = Matrix4x4Calc.VectorMatrixMultiplication(new Vector3(0, 5, 0), newFinal);
		var vectorZ = Matrix4x4Calc.VectorMatrixMultiplication(new Vector3(0, 0, 5), newFinal);
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
}