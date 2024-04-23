using KURSOVAY.CustomDataTypes;
using KURSOVAY.Utilities;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace KURSOVAY.ViewModels
{
	internal class MainWindowViewModel : INotifyPropertyChanged
	{
		private readonly string path = "Input.obj";

		private Figure _current_Figure;
		public Figure Current_Figure
		{
			get { return _current_Figure; }
			set
			{
				_current_Figure = value;
				OnPropertyChanged(nameof(Current_Figure));
			}
		}
		private ICommand _window_ContentRenderedCommand;
		public ICommand Window_ContentRenderedCommand => _window_ContentRenderedCommand ??= new RelayCommand(f =>
		{
			Current_Figure = Figure.GetObj(path);
		});

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
