using CourseWork.CustomDataTypes;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CourseWork.ViewModels;

internal class MainWindowViewModel : INotifyPropertyChanged
{
	private const string ObjPath = "Input.obj";
	private const string SettingsPath = "Settings.json";

	public MainWindowViewModel()
	{
		GetData();
	}

	private Obj? _currentObj;
	public Obj? CurrentObj
	{
		get => _currentObj;
		set
		{
			_currentObj = value;
			OnPropertyChanged();
		}
	}
	private Settings? _currentSettings;
	public Settings? CurrentSettings
	{
		get => _currentSettings;
		set
		{
			_currentSettings = value;
			OnPropertyChanged();
		}
	}

	private async void GetData()
	{
		CurrentObj = await Obj.GetObjAsync(ObjPath);
		CurrentSettings = await Settings.GetSettingsAsync(SettingsPath);
	}

	public event PropertyChangedEventHandler? PropertyChanged;
	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}