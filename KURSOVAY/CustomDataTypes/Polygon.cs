using System.Numerics;
using System.Windows;

namespace CourseWork.CustomDataTypes
{
	internal class Polygon(in Vector3 point1, in Vector3 point2, in Vector3 point3, in Vector3 normal)
	{
		private readonly Vector3 _point1 = point1;
		private readonly Vector3 _point2 = point2;
		private readonly Vector3 _point3 = point3;
		public Vector3 Normal { get; set; } = normal;

		public void MakeFill()
		{
			Dictionary<int, Tuple<Point, Point>> cheats = [];
			Algorithms.Algorithms.Cda(_point1, _point2, ref cheats);
			Algorithms.Algorithms.Cda(_point2, _point3, ref cheats);
			Algorithms.Algorithms.Cda(_point3, _point1, ref cheats);
			foreach (var lines in cheats)
			{
				foreach (var item in Algorithms.Algorithms.Cda(lines.Value.Item1, lines.Value.Item2))
				{
					Tuple<int, int> key = new((int)Math.Round(item.X, MidpointRounding.ToZero), lines.Key);
					if (Buffer.TryGetValue(key, out var value))
					{
						if (value <= item.Y)
							Buffer[key] = item.Y;
					}
					else
						Buffer.Add(key, item.Y);
				}
			}
		}

		public Dictionary<Tuple<int, int>, double> Buffer { get; set; } = [];
	}
}
