using System.Numerics;
using System.Windows;
using System.Windows.Media;

namespace CourseWork.CustomDataTypes
{
	internal class Polygon(in Vector4 point1, in Vector4 point2, in Vector4 point3, in Color color)
	{
		private readonly Vector4 _point1 = point1;
		private readonly Vector4 _point2 = point2;
		private readonly Vector4 _point3 = point3;
		private readonly Color _color = color;

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
						if (value.Item1 <= item.Y)
							Buffer[key] = new Tuple<double, Color>(item.Y, _color);
					}
					if (value == null)
						Buffer.Add(key, new Tuple<double, Color>(item.Y, _color));
				}
			}
		}
		public Dictionary<Tuple<int, int>, Tuple<double, Color>> Buffer { get; set; } = [];
	}
}
