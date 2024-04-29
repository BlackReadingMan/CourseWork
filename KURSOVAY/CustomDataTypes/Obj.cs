using System.IO;
using System.Numerics;
using System.Reflection;

namespace CourseWork.CustomDataTypes
{
	public struct Obj()
	{
		public List<Vector3> V = [];
		public List<Vector3> Vn = [];
		public List<Tuple<Tuple<int, int, int>, Tuple<int, int, int>, Tuple<int, int, int>>> F = [];
		public static async Task<Obj> GetObjAsync(string path)
		{
			var objFileLines = await ReadObjLinesAsync(path);
			return await Task.Run(() => MakeObjAsync(objFileLines));
		}
		private static Obj MakeObjAsync(List<string> objFileLines)
		{
			Obj obj = new();
			foreach (var line in objFileLines.Where(line => line.Length >= 2))
			{
				switch (line.ToLower()[..2])
				{
					case "v ":
						var v = line.Split(' ')
							.Skip(1)
							.Select(x => Convert.ToDouble(x.Replace('.', ',')))
							.ToArray();
						obj.V.Add(new Vector3((float)v[0], (float)v[1], (float)v[2]));
						break;
					case "vn":
						var vn = line.Split(' ')
							.Skip(1)
							.Select(x => Convert.ToDouble(x.Replace('.', ',')))
							.ToArray();
						obj.Vn.Add(new Vector3((float)vn[0], (float)vn[1], (float)vn[2]));
						break;
					case "vt":
						continue;
					case "f ":
						var vx = line.Split(' ')
							.Skip(1)
							.Select(x => x.Split('/'))
							.Select(x => x.Select(i => Convert.ToInt32(i)).ToArray())
							.ToArray();
						obj.F.Add(new Tuple<Tuple<int, int, int>, Tuple<int, int, int>, Tuple<int, int, int>>(
							new Tuple<int, int, int>(vx[0][0], vx[0][1], vx[0][2]),
							new Tuple<int, int, int>(vx[1][0], vx[1][1], vx[1][2]),
							new Tuple<int, int, int>(vx[2][0], vx[2][1], vx[2][2])
						));
						break;
				}
			}
			return obj;
		}
		private static async Task<List<string>> ReadObjLinesAsync(string filePath)
		{
			List<string> lines = [];
			using var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(Assembly.GetExecutingAssembly().GetName().Name + ".InputData." + filePath)!);
			while (await reader.ReadLineAsync() is { } line)
			{
				lines.Add(line);
			}
			return lines;
		}
	}
}
