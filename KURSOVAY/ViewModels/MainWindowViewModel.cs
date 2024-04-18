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
		private Scene scene;
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
		private async void Getting()
		{
			if (scene.ready)
			{
				WB = scene.WB;
			}
			else
			{
				await Task.Delay(100);
				Getting();
			}
		}
		private ICommand _window_ContentRenderedCommand;
		public ICommand Window_ContentRenderedCommand => _window_ContentRenderedCommand ??= new RelayCommand(OnWindowRenderedExecute);
		private void OnWindowRenderedExecute(object parameter)
		{
			Grid grid = (Grid)parameter;
			scene = new Scene(Figure.GetObj("5.obj"), new System.Windows.Size(grid.ActualWidth, grid.ActualHeight));
			Getting();
		}
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
