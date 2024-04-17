using KURSOVAY.CustomDataTypes;
using KURSOVAY.Utilities;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace KURSOVAY.ViewModels
{
	internal class MainWindowViewModel : INotifyPropertyChanged
	{
		private readonly Scene scene;
		public MainWindowViewModel()
		{
			scene = new Scene(Figure.GetObj("5.obj"));
		}
		private WriteableBitmap _wb;
		public WriteableBitmap WB
		{
			get => _wb;
			set
			{
				_wb = value;
				OnPropertyChanged(nameof(WB));
			}
		}

		private ICommand _window_ContentRenderedCommand;
		public ICommand Window_ContentRenderedCommand => _window_ContentRenderedCommand ??= new RelayCommand(OnWindowRenderedExecute);
		private void OnWindowRenderedExecute(object parameter)
		{
			Grid grid = (Grid)parameter;
			scene.Size = new(Math.Round(grid.ActualWidth), Math.Round(grid.ActualHeight));
			WB = scene.WB;
		}
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
