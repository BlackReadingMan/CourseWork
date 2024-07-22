using CourseWork.CustomDataTypes;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CourseWork.Algorithms;

namespace CourseWork.Controls;

/// <summary>
/// Логика взаимодействия для Scene.xaml
/// </summary>
public partial class Scene
{
	private readonly Renderer _renderer;
	private Vector3 _turnVector3 = Vector3.Zero;
	private Key? _pressedKey;
	private bool _running;
	private ImageSource? _imageSource;

	public Scene()
	{
		InitializeComponent();
		_renderer = new Renderer();
		Focusable = true;
	}

	private void MakeTurnVector()
	{
		_turnVector3 = Vector3.Zero;
		switch (_pressedKey)
		{
			case  Key.Up :
				_turnVector3[2]--;
				break;
			case Key.Down :
				_turnVector3[2]++;
				break;
			case Key.Left:
				_turnVector3[1]--;
				break;
			case Key.Right:
				_turnVector3[1]++;
				break;
			case Key.Add:
				_turnVector3[0]--;
				break;
			case Key.Subtract:
				_turnVector3[0]++;
				break;
			case Key.OemPlus:
				_turnVector3[0]--;
				break;
			case Key.OemMinus:
				_turnVector3[0]++;
				break;
			case Key.Enter:
				_turnVector3 = new Vector3(-1f, -1f, -1f);
				break;
		}
	}

	private async void SceneUpdateAsync()
	{
		_running = true;
		MakeTurnVector();
		if (_turnVector3 == Vector3.Zero)
		{
			_running = false;
			return;
		}

		_imageSource = await _renderer.GetPictureAsync(RenderSize, _turnVector3);
		DataToUi();
		_running = false;
	}

	private void DataToUi()
	{
		RenderTimer.Content = _renderer.RenderTime;
		Picture.Source = _imageSource;
	}

	private static readonly DependencyProperty PaintedObjProperty =
		DependencyProperty.Register(nameof(PaintedObj), typeof(Obj), typeof(Scene),
			new FrameworkPropertyMetadata(OnPaintedObjChanged));

	public Obj? PaintedObj
	{
		get => (Obj)GetValue(PaintedObjProperty);
		set => SetValue(PaintedObjProperty, value);
	}

	private static void OnPaintedObjChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Scene)d)._renderer.PaintedObj = (Obj)e.NewValue;
	}

	private static readonly DependencyProperty SettingsProperty =
		DependencyProperty.Register(nameof(Settings), typeof(Settings), typeof(Scene),
			new FrameworkPropertyMetadata(OnSettingsChanged));

	public Settings? Settings
	{
		get => (Settings)GetValue(SettingsProperty);
		set => SetValue(SettingsProperty, value);
	}

	private static void OnSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Scene)d)._renderer.RenderSettings = (Settings)e.NewValue;
		((Scene)d).Background = new SolidColorBrush(Algorithms.Algorithms.VectorToColor(((Settings)e.NewValue).BackGroundColor));
		var fontColor = Brushes.White.Color - Algorithms.Algorithms.VectorToColor(((Settings)e.NewValue).BackGroundColor);
		fontColor.A = 255;
		((Scene)d).Tips.Foreground = new SolidColorBrush(fontColor);
	}

	private void UC_KeyDown(object sender, KeyEventArgs e)
	{
		if (_running || PaintedObj is null || Settings is null) return;
		_pressedKey = e.Key;
		SceneUpdateAsync();
	}

	private void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
		Keyboard.Focus(this);
	}

	private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
	{
		Keyboard.Focus(this);
	}

	private void Button_Click(object sender, RoutedEventArgs e)
	{
		if (_running || PaintedObj is null || Settings is null) return;
		_pressedKey = Key.OemMinus;
		SceneUpdateAsync();
	}

	private void Button_Click_1(object sender, RoutedEventArgs e)
	{
		if (_running || PaintedObj is null || Settings is null) return;
		_pressedKey = Key.Up;
		SceneUpdateAsync();
	}

	private void Button_Click_2(object sender, RoutedEventArgs e)
	{
		if (_running || PaintedObj is null || Settings is null) return;
		_pressedKey = Key.OemPlus;
		SceneUpdateAsync();
	}

	private void Button_Click_3(object sender, RoutedEventArgs e)
	{
		if (_running || PaintedObj is null || Settings is null) return;
		_pressedKey = Key.Left;
		SceneUpdateAsync();
	}

	private void Button_Click_4(object sender, RoutedEventArgs e)
	{
		if (_running || PaintedObj is null || Settings is null) return;
		_pressedKey = Key.Down;
		SceneUpdateAsync();
	}

	private void Button_Click_5(object sender, RoutedEventArgs e)
	{
		if (_running || PaintedObj is null || Settings is null) return;
		_pressedKey = Key.Right;
		SceneUpdateAsync();
	}

	private void Button_Click_6(object sender, RoutedEventArgs e)
	{
		if (_running || PaintedObj is null || Settings is null) return;
		_pressedKey = Key.Enter;
		SceneUpdateAsync();
	}
}