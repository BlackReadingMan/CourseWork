﻿using CourseWork.CustomDataTypes;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CourseWork.Controls
{
	/// <summary>
	/// Логика взаимодействия для Scene.xaml
	/// </summary>
	public partial class Scene
	{
		private readonly Renderer _renderer;
		private Vector3 _turnVector3 = Vector3.Zero;
		private KeyEventArgs? _pressedKey;
		private bool _running;
		public Scene()
		{
			InitializeComponent();
			_renderer = new Renderer();
		}

		private void MakeTurnVector()
		{
			_turnVector3 = Vector3.Zero;
			switch (_pressedKey)
			{
				case { Key: Key.Up }:
					_turnVector3[2]--;
					break;
				case { Key: Key.Down }:
					_turnVector3[2]++;
					break;
				case { Key: Key.Left }:
					_turnVector3[1]--;
					break;
				case { Key: Key.Right }:
					_turnVector3[1]++;
					break;
				case { Key: Key.Add }:
					_turnVector3[0]--;
					break;
				case { Key: Key.Subtract }:
					_turnVector3[0]++;
					break;
				case { Key: Key.OemPlus }:
					_turnVector3[0]--;
					break;
				case { Key: Key.OemMinus }:
					_turnVector3[0]++;
					break;
				case { Key: Key.Enter }:
					_turnVector3 = new Vector3(-1f, -1f, -1f);
					break;
			}
		}
		private async void SceneUpdate()
		{
			_running = true;
			MakeTurnVector();
			if (_turnVector3 == Vector3.Zero) return;
			await Task.Run(() => _renderer.PictureUpdate(RenderSize, _turnVector3));
			DataToUi();
			_running = false;
		}
		private void DataToUi()
		{
			RenderTimer.Content = _renderer.RenderTime;
			var image = new WriteableBitmap((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Bgra32, null);
			foreach (var pixel in _renderer.ZBuffer.Where(x =>
							 x.Key.Item1 < ActualWidth && x.Key.Item1 >= 0 && x.Key.Item2 < ActualHeight &&
							 x.Key.Item2 >= 0))
			{
				image.WritePixels(new Int32Rect(pixel.Key.Item1,pixel.Key.Item2, 1, 1),
					new byte[] { pixel.Value.Item2.B, pixel.Value.Item2.G, pixel.Value.Item2.R, 255 }, 4, 0);
			}
			Picture.Source = image;
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
			((Scene)d)._renderer.Settings = (Settings)e.NewValue;
			((Scene)d).Background = new SolidColorBrush(Color.FromRgb((byte)(((Settings)e.NewValue).BackGroundColor[0] * 255), (byte)(((Settings)e.NewValue).BackGroundColor[1] * 255), (byte)(((Settings)e.NewValue).BackGroundColor[2] * 255)));
		}
		private void UC_KeyDown(object sender, KeyEventArgs e)
		{
			if (_running || PaintedObj is null || Settings is null) return;
			_pressedKey = e;
			SceneUpdate();
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			Keyboard.Focus(this);
		}
	}
}
