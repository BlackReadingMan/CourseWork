using KURSOVAY.Algorithm;
using KURSOVAY.Utilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KURSOVAY.ViewModels
{
	class MainWindowViewModel
	{
		private List<Figure> _paintedFigeres;
		private List<Figure> PaintedFigeres
		{
			get => _paintedFigeres;
			set
			{
				_paintedFigeres = value;
				MakePolygons();
			}
		}
		private WriteableBitmap _wb;
		private WriteableBitmap WB
		{
			get => _wb;
			set
			{
				_wb = value;
				MakeFigures();
			}
		}
		private void MakeFigures()
		{
			PaintedFigeres = Algorithms.GetObj("5.obj");
		}
		private void MakePolygons()
		{
			foreach (var points in PaintedFigeres)
			{
				//foreach (var pixel in Algorithms.Guro(points))
				//{
				//	WB.WritePixels(new Int32Rect(pixel.Item1, pixel.Item2, 1, 1), new byte[] { pixel.Item3.B, pixel.Item3.G, pixel.Item3.R, 255 }, 4, 0);
				//}
			}
		}
		private ICommand _window_ContentRenderedCommand;
		public ICommand Window_ContentRenderedCommand => _window_ContentRenderedCommand ??= new RelayCommand(OnWindowRenderedExecute);
		private void OnWindowRenderedExecute(object parameter)
		{
			Grid grid = (Grid)parameter;
			Image ima = (Image)grid.Children[0];
			WB = new WriteableBitmap((int)grid.ActualWidth, (int)grid.ActualHeight, 96, 96, PixelFormats.Bgra32, null);
			ima.Source = WB;
		}
	}
}
