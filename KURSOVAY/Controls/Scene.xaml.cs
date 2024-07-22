using CourseWork.CustomDataTypes;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CourseWork.Algorithms;

namespace CourseWork.Controls;

/// <summary>
/// Логика взаимодействия для Scene.xaml
/// </summary>
public partial class Scene
{
	private readonly Render _render = new();
	private bool _isRunning;
	private bool _dataIsOk;
	public Scene()
	{
		InitializeComponent();
	}
	public void LoadData(Obj? objData, Settings? currentSettings)
	{
		if (objData is null || currentSettings is null)
		{
			_dataIsOk = false;
			return;
		}

		_dataIsOk = true;
		_render.PaintedObj = objData;
		_render.Settings = currentSettings;
		Dispatcher.Invoke(() =>
			Background = new SolidColorBrush(Algorithms.Algorithms.VectorToColor(currentSettings.BackGroundColor)));
		var fontColor = Brushes.White.Color - Algorithms.Algorithms.VectorToColor(currentSettings.BackGroundColor);
		fontColor.A = 255;
		Dispatcher.Invoke(() =>
			Tips.Foreground = new SolidColorBrush(fontColor));
	}
	public async void CameraTransform(CameraTransformType value)
	{
		if (_isRunning || !_dataIsOk)
			return;

		_isRunning = true;

		var turnVector3 = Vector3.Zero;
		switch (value)
		{
			case CameraTransformType.Up:
				turnVector3[2]--;
				break;
			case CameraTransformType.Down:
				turnVector3[2]++;
				break;
			case CameraTransformType.Left:
				turnVector3[1]--;
				break;
			case CameraTransformType.Right:
				turnVector3[1]++;
				break;
			case CameraTransformType.BringCloser:
				turnVector3[0]--;
				break;
			case CameraTransformType.MoveAway:
				turnVector3[0]++;
				break;
			case CameraTransformType.Start:
				turnVector3 = new Vector3(-1f, -1f, -1f);
				break;
		}

		await SceneUpdate(turnVector3);
		_isRunning = false;
	}
	private async Task SceneUpdate(Vector3 turnVector3)
	{
		await Task.Run(() => _render.RenderPicture(RenderSize, turnVector3));
		Picture.Source = DrawPicture();
		RenderTimer.Content = _render.RenderTime;
	}

	private WriteableBitmap DrawPicture()
	{
		var image = new WriteableBitmap((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Bgra32, null);
		foreach (var pixel in _render._zBuffer.Where(x =>
					 x.Key.Item1 < (int)ActualWidth && x.Key.Item1 >= 0 && x.Key.Item2 < (int)ActualHeight &&
					 x.Key.Item2 >= 0 && x.Value.Item2 != Algorithms.Algorithms.VectorToColor(_render.Settings.BackGroundColor)))
		{
			image.WritePixels(new Int32Rect(pixel.Key.Item1, pixel.Key.Item2, 1, 1),
				new byte[] { pixel.Value.Item2.B, pixel.Value.Item2.G, pixel.Value.Item2.R, 255 }, 4, 0);
		}
		return image;
	}


	private void Button_Click(object sender, RoutedEventArgs e)
	{
		CameraTransform(CameraTransformType.MoveAway);
	}

	private void Button_Click_1(object sender, RoutedEventArgs e)
	{
		CameraTransform(CameraTransformType.Up);
	}

	private void Button_Click_2(object sender, RoutedEventArgs e)
	{
		CameraTransform(CameraTransformType.BringCloser);
	}

	private void Button_Click_3(object sender, RoutedEventArgs e)
	{
		CameraTransform(CameraTransformType.Left);
	}

	private void Button_Click_4(object sender, RoutedEventArgs e)
	{
		CameraTransform(CameraTransformType.Down);
	}

	private void Button_Click_5(object sender, RoutedEventArgs e)
	{
		CameraTransform(CameraTransformType.Right);
	}

	private void Button_Click_6(object sender, RoutedEventArgs e)
	{
		CameraTransform(CameraTransformType.Start);
	}
}