using KURSOVAY.ViewModels;
using System.Windows;

namespace KURSOVAY
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			((MainWindowViewModel)DataContext).On_WindowKeyUpExecute(e);
        }
    }
}