using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;


namespace KURSOVAY.Algorithm
{
	internal static class Algorithms
	{
		private static void CDA(Point point1, Point point2, ref Dictionary<int, Tuple<double, double>> lines)
		{
			double dx = point2.X - point1.X;
			double dy = point2.Y - point1.Y;

			double step;

			if (Math.Abs(dx) > Math.Abs(dy))
				step = Math.Abs(dx);
			else
				step = Math.Abs(dy);

			double x_incr = dx / step;
			double y_incr = dy / step;

			double x = point1.X;
			double y = point1.Y;

			for (int i = 0; i < step; i++)
			{
				if (lines.TryGetValue((int)y, out Tuple<double, double>? check))
					if (x < check.Item1)
						lines[(int)y] = new Tuple<double, double>(x, check.Item2);
					else
						if (x > check.Item2)
						lines[(int)y] = new Tuple<double, double>(check.Item1, x);
				if (check == null)
					lines.Add((int)y, new Tuple<double, double>(x, x));
				x += x_incr;
				y += y_incr;
			}
		}
		public static List<Tuple<int, int, Color>> Guro(Tuple<Point, Point, Point> triangle)
		{
			List<Tuple<int, int, Color>> points = [];
			Random rand = new Random();
			Dictionary<int, Tuple<double, double>> cheats = new Dictionary<int, Tuple<double, double>>();
			CDA(triangle.Item1, triangle.Item2, ref cheats);
			CDA(triangle.Item2, triangle.Item3, ref cheats);
			CDA(triangle.Item3, triangle.Item1, ref cheats);
			double t;
			Color col;
			byte c1r = (byte)rand.Next(255), c1g = (byte)rand.Next(255), c1b = (byte)rand.Next(255);
			byte c2r = (byte)rand.Next(255), c2g = (byte)rand.Next(255), c2b = (byte)rand.Next(255);
			foreach (var lines in cheats)
			{
				for (double x = lines.Value.Item1; x <= lines.Value.Item2; x++)
				{
					t = (lines.Value.Item2 - x) / (lines.Value.Item2 - lines.Value.Item1);
					col = Color.FromRgb((byte)(c1r * t), (byte)(c1g * t), (byte)(c1b * t)) + Color.FromRgb((byte)(c2r * (1 - t)), (byte)(c2g * (1 - t)), (byte)(c2b * (1 - t)));
					points.Add(new Tuple<int, int, Color>((int)x, lines.Key, col));
				}
			}
			return points;
		}
		public static List<Figure> GetObj(string path)
		{
			List<string> lines = ReadObj(path);
			List<Figure> figures = new List<Figure>();
			foreach (string line in lines)
			{
				// строки с вершинами
				if (line.ToLower().StartsWith("vt"))
				{
					continue;
				}
				else if (line.ToLower().StartsWith('o'))
				{
					figures.Add(new Figure([], [], []));
				}
				else if (line.ToLower().StartsWith("vn"))
				{
					var vx = line.Split(' ')
						.Skip(1)
						.Select(v => Convert.ToDouble(v.Replace('.', ',')))
						.ToArray();
					figures.Last().VN.Add(new Point3D(vx[0], vx[1], vx[2]));
				}
				else if (line.ToLower().StartsWith('v'))
				{
					var vx = line.Split(' ')
						.Skip(1)
						.Select(v => Convert.ToDouble(v.Replace('.', ',')))
						.ToArray();
					figures.Last().V.Add(new Point3D(vx[0], vx[1], vx[2]));
				}
				else if (line.ToLower().StartsWith('f'))
				{
					var vx = line.Split(' ')
						.Skip(1)
						.Select(v => v.Split('/'))
						.Select(v => v.Select(i=> Convert.ToInt32(i)).ToList())
						.ToList();
					figures.Last().F.Add(new Tuple<Tuple<int, int, int>, Tuple<int, int, int>, Tuple<int, int, int>>(
						new Tuple<int, int, int>( vx[0][0], vx[0][1], vx[0][2]),
						new Tuple<int, int, int>(vx[1][0], vx[1][1], vx[1][2]),
						new Tuple<int, int, int>(vx[2][0], vx[2][1], vx[2][2])
						));
				}
			}
			return figures;
		}
		private static List<string> ReadObj(string filePath) 
		{
			List<string> lines = new List<string>();
			string s = Assembly.GetExecutingAssembly().GetName().Name;
			using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(Assembly.GetExecutingAssembly().GetName().Name + ".Resourses." + filePath)))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					lines.Add(line);
				}
			}
			return lines;
		}
	}
}
