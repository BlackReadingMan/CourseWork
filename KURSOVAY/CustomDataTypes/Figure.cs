using System.IO;
using System.Numerics;
using System.Reflection;

namespace CourseWork.CustomDataTypes
{
	public struct Figure()
	{
		public List<Vector3> V = [];
		public List<Vector3> Vn = [];
		public List<Tuple<Tuple<int, int, int>, Tuple<int, int, int>, Tuple<int, int, int>>> F = [];
		public static Figure GetObj(string path)
		{
			var lines = ReadObj(path);
			Figure figures = new();
			foreach (var line in lines.Where(line => line.Length >= 2))
			{
				switch (line.ToLower()[..2])
				{
					case "v ":
						var v = line.Split(' ')
							.Skip(1)
							.Select(x => Convert.ToDouble(x.Replace('.', ',')))
							.ToArray();
						figures.V.Add(new Vector3((float)v[0], (float)v[1], (float)v[2]));
						break;
					case "vn":
						var vn = line.Split(' ')
							.Skip(1)
							.Select(x => Convert.ToDouble(x.Replace('.', ',')))
							.ToArray();
						figures.Vn.Add(new Vector3((float)vn[0], (float)vn[1], (float)vn[2]));
						break;
					case "vt":
						continue;
					case "f ":
						var vx = line.Split(' ')
							.Skip(1)
							.Select(x => x.Split('/'))
							.Select(x => x.Select(i => Convert.ToInt32(i)).ToArray())
							.ToArray();
						figures.F.Add(new Tuple<Tuple<int, int, int>, Tuple<int, int, int>, Tuple<int, int, int>>(
							new Tuple<int, int, int>(vx[0][0], vx[0][1], vx[0][2]),
							new Tuple<int, int, int>(vx[1][0], vx[1][1], vx[1][2]),
							new Tuple<int, int, int>(vx[2][0], vx[2][1], vx[2][2])
						));
						break;
				}
			}
			return figures;
		}
		private static List<string> ReadObj(string filePath)
		{
			List<string> lines = [];
			using var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(Assembly.GetExecutingAssembly().GetName().Name + ".Objects." + filePath)!);
			while (reader.ReadLine() is { } line)
			{
				lines.Add(line);
			}

			return lines;
		}
	}
}
