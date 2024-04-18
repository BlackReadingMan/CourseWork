using KURSOVAY.CustomDataTypes;
using KURSOVAY.Utilities;
using System.ComponentModel;
using System.Diagnostics;
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
		private string path = "5.obj";
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
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			scene = new Scene(Figure.GetObj(path), new System.Windows.Size(grid.ActualWidth - 1, grid.ActualHeight - 1));
			scene.PictureUpdate();
			WB = scene.WB;
			stopwatch.Stop();
		}
		private ICommand _windowKeyUp;
		public ICommand WindowKeyUp => _windowKeyUp ??= new RelayCommand(On_WindowKeyUpExecute);
		private void On_WindowKeyUpExecute(object parameter)
		{
			Grid grid = (Grid)parameter;
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			scene = new Scene(Figure.GetObj(path), new System.Windows.Size(grid.ActualWidth - 1, grid.ActualHeight - 1));
			scene.PictureUpdate();
			WB = scene.WB;
			stopwatch.Stop();
		}
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
