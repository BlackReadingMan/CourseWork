using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media.Media3D;

namespace KURSOVAY
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		
	}
	public struct Figure
	{
		public Figure(List<Point3D> v, List<Point3D> vn, List<Tuple<Tuple<int, int, int>, Tuple<int, int, int>, Tuple<int, int, int>>> f) : this()
		{
			this.V = v;
			this.VN = vn;
			this.F = f;
		}
		public List<Point3D> V = [];
		public List<Point3D> VN = [];
		public List<Tuple<Tuple<int, int, int>, Tuple<int, int, int>, Tuple<int, int, int>>> F = [];
	}
}
