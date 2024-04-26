using System.Numerics;
using System.Windows;
using System.Windows.Media;

namespace CourseWork.CustomDataTypes
{
	internal class Polygon(in Vector3 point1, in Vector3 point2, in Vector3 point3, in Vector3 normal, in Vector3 lightPosition, in Vector3 lightColor,in Vector3 objectColor)
	{
		private readonly Vector3 _point1 = point1;
		private readonly Vector3 _point2 = point2;
		private readonly Vector3 _point3 = point3;
		private readonly Vector3 _normal = normal;
		private readonly Vector3 _lightPosition = lightPosition;
		private readonly Vector3 _lightColor = lightColor;
		private readonly Vector3 _objectColor = objectColor;

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
							Buffer[key] = new Tuple<double, Color>(item.Y, Algorithms.Algorithms.GetColor(_normal,new Vector3(key.Item1,key.Item2, (float)item.Y),_lightPosition,_lightColor,_objectColor));
					}
					if (value == null)
						Buffer.Add(key, new Tuple<double, Color>(item.Y, Algorithms.Algorithms.GetColor(_normal, new Vector3(key.Item1, key.Item2, (float)item.Y), _lightPosition, _lightColor, _objectColor)));
				}
			}
		}
		public Dictionary<Tuple<int, int>, Tuple<double, Color>> Buffer { get; set; } = [];
	}
}
