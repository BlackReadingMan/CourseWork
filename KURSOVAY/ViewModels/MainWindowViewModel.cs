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
		private string path = "5.obj";
		private float spectator_step = 0.025f;

		private WriteableBitmap _render;
		public WriteableBitmap Render
		{
			get => _render;
			set
			{
				_render = value;
				OnPropertyChanged(nameof(Render));
			}
		}
		private string _render_time;
		public string Render_time
		{
			get => _render_time;
			set
			{
				_render_time = value;
				OnPropertyChanged(nameof(Render_time));
			}
		}
		private void GetPicture()
		{
			
			Stopwatch stopwatch = new ();
			stopwatch.Start();
			scene.PictureUpdate();
			Render = scene.WB;
			stopwatch.Stop();
			Render_time = stopwatch.ToString();
			
		}
		private ICommand _window_ContentRenderedCommand;
		public ICommand Window_ContentRenderedCommand => _window_ContentRenderedCommand ??= new RelayCommand(OnWindowRenderedExecute);
		private void OnWindowRenderedExecute(object parameter)
		{
			scene = new Scene(Figure.GetObj(path), new System.Windows.Size(((Grid)parameter).ActualWidth - 1, ((Grid)parameter).ActualHeight - 1));
			GetPicture();
		}

		public void On_WindowKeyUpExecute(KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Up:
					scene.CameraSpherePosition += new System.Numerics.Vector3(0f, 0f, -spectator_step);
					GetPicture();
					break;
				case Key.Down:
					scene.CameraSpherePosition += new System.Numerics.Vector3(0f, 0f, spectator_step);
					GetPicture();
					break;
				case Key.Left:
					scene.CameraSpherePosition += new System.Numerics.Vector3(0f, -spectator_step, 0f);
					GetPicture();
					break;
				case Key.Right:
					scene.CameraSpherePosition += new System.Numerics.Vector3(0f, spectator_step, 0f);
					GetPicture();
					break;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
