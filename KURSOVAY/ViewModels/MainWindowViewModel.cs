using CourseWork.CustomDataTypes;
using CourseWork.Utilities;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CourseWork.ViewModels
{
	internal class MainWindowViewModel : INotifyPropertyChanged
	{
		private const string Path = "Input.obj";

		private Figure _currentFigure;
		public Figure CurrentFigure
		{
			get => _currentFigure;
			set
			{
				_currentFigure = value;
				OnPropertyChanged();
			}
		}
		private ICommand _windowContentRenderedCommand;
		public ICommand WindowContentRenderedCommand => _windowContentRenderedCommand = new RelayCommand(f =>
		{
			CurrentFigure = Figure.GetObj(Path);
		});

		public event PropertyChangedEventHandler? PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
