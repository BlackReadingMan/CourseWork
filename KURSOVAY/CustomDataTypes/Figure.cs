using System.IO;
using System.Numerics;
using System.Reflection;

namespace KURSOVAY.CustomDataTypes
{
	public struct Figure
	{
		public Figure()
		{
			V = [];
			VN = [];
			F = [];
		}
		public List<Vector4> V;
		public List<Vector3> VN;
		public List<Tuple<Tuple<int, int, int>, Tuple<int, int, int>, Tuple<int, int, int>>> F;
		public static Figure GetObj(string path)
		{
			List<string> lines = ReadObj(path);
			Figure figures = new();
			foreach (string line in lines)
			{
				if (line.Length >= 2)
					switch (line.ToLower()[..2])
					{
						case "v ":
							var V = line.Split(' ')
							.Skip(1)
							.Select(v => Convert.ToDouble(v.Replace('.', ',')))
							.ToArray();
							figures.V.Add(new Vector4((float)V[0], (float)V[1], (float)V[2], 1));
							break;
						case "vn":
							var VN = line.Split(' ')
							.Skip(1)
							.Select(v => Convert.ToDouble(v.Replace('.', ',')))
							.ToArray();
							figures.VN.Add(new Vector3((float)VN[0], (float)VN[1], (float)VN[2]));
							break;
						case "vt":
							continue;
						case "f ":
							var vx = line.Split(' ')
							.Skip(1)
							.Select(v => v.Split('/'))
							.Select(v => v.Select(i => Convert.ToInt32(i)).ToArray())
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
			using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(Assembly.GetExecutingAssembly().GetName().Name + ".Resources." + filePath)))
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
