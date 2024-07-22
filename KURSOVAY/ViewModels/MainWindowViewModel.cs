using CourseWork.Controls;
using CourseWork.CustomDataTypes;
using CourseWork.Utilities;
using System.Windows.Input;

namespace CourseWork.ViewModels;

internal class MainWindowViewModel
{
	private const string ObjPath = "InputData/Input.obj";
	private const string SettingsPath = "InputData/Settings.json";

	private Scene? _scene;

	private ICommand? _loadedCommand;
	public ICommand LoadedCommand => _loadedCommand ??= new RelayCommand(async f =>
	{
		if (f is Scene scene)
		{
			_scene = scene;
			await GetData();
			_scene?.CameraTransform(CameraTransformType.Start);
		}
	});


	private ICommand? _hotkeyCommand;
	public ICommand HotkeyCommand => _hotkeyCommand ??= new RelayCommand(async a =>
	{
		if (a is string key)
		{
			switch (key)
			{
				case "Up":
					_scene?.CameraTransform(CameraTransformType.Up);
					break;
				case "Down":
					_scene?.CameraTransform(CameraTransformType.Down);
					break;
				case "Left":
					_scene?.CameraTransform(CameraTransformType.Left);
					break;
				case "Right":
					_scene?.CameraTransform(CameraTransformType.Right);
					break;
				case "OemPlus":
					_scene?.CameraTransform(CameraTransformType.BringCloser);
					break;
				case "OemMinus":
					_scene?.CameraTransform(CameraTransformType.MoveAway);
					break;
				case "Enter":
					_scene?.CameraTransform(CameraTransformType.Start);
					break;
			}
		}
	});

	private async Task GetData()
	{
		var currentObj = await Obj.GetObjAsync(ObjPath).ConfigureAwait(false);
		var currentSettings = await Settings.GetSettingsAsync(SettingsPath).ConfigureAwait(false);
		_scene?.LoadData(currentObj, currentSettings);
	}
}