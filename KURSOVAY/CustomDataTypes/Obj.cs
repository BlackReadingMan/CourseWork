using System.IO;
using System.Numerics;
using System.Reflection;
using System.Windows;

namespace CourseWork.CustomDataTypes;

public class Obj
{
	public List<Vector3> V = [];
	public List<Vector3> Vn = [];
	public List<Tuple<Tuple<int, int, int>, Tuple<int, int, int>, Tuple<int, int, int>>> F = [];

	public static async Task<Obj?> GetObjAsync(string path)
	{
		Obj? result;
		var objFileLines = await ReadObjLinesAsync(path);
		if (objFileLines is null)
			result = null;
		else
		{
			result = MakeObj(objFileLines);
			if (result != null && !result.ObjIsOk())
				result = null;
		}

		return result;
	}

	private static Obj? MakeObj(List<string> objFileLines)
	{
		Obj? obj = new();
		try
		{
			foreach (var line in objFileLines.Where(line => line.Length >= 2))
			{
				switch (line.ToLower()[..2])
				{
					case "v ":
						var v = line.Replace("  ", " ").Replace("  ", " ").Split(' ')
							.Skip(1)
							.Select(x => Convert.ToDouble(x.Replace('.', ',')))
							.ToArray();
						obj.V.Add(new Vector3((float)v[0], (float)v[1], (float)v[2]));
						break;
					case "vn":
						var vn = line.Replace("  ", " ").Replace("  ", " ").Split(' ')
							.Skip(1)
							.Select(x => Convert.ToDouble(x.Replace('.', ',')))
							.ToArray();
						obj.Vn.Add(new Vector3((float)vn[0], (float)vn[1], (float)vn[2]));
						break;
					case "vt":
						continue;
					case "f ":
						var vx = line.Replace("  ", " ").Replace("  ", " ").Split(' ')
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
		}
		catch
		{
			MessageBox.Show("Кривой файл obj, где-то пропущены данные.");
			obj = null;
		}

		return obj;
	}

	private static async Task<List<string>?> ReadObjLinesAsync(string filePath)
	{
		List<string>? lines;
		try
		{
			lines = new List<string>( await File.ReadAllLinesAsync(filePath));
		}
		catch
		{
			MessageBox.Show("Отсутсвует файл с входными данными");
			lines = null;
		}

		return lines;
	}

	private bool ObjIsOk()
	{
		var ok = true;
		var i = 0;
		Vector3 vector;
		try
		{
			for (; i < F.Count; i++)
			{
				vector = V[F[i].Item1.Item1 - 1];
				vector = V[F[i].Item2.Item1 - 1];
				vector = V[F[i].Item3.Item1 - 1];
				vector = Vn[F[i].Item1.Item3 - 1];
				vector = Vn[F[i].Item2.Item3 - 1];
				vector = Vn[F[i].Item3.Item3 - 1];
			}
		}
		catch
		{
			MessageBox.Show($"Ошибка в f линии №{i}");
			ok = false;
		}

		return ok;
	}
}