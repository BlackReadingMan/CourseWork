using System.Numerics;
using System.Windows;
using System.Windows.Media;

namespace CourseWork.CustomDataTypes
{
	internal class Polygon(in Vector3 point1, in Vector3 point2, in Vector3 point3, in Color color1, in Color color2, in Color color3, ref Dictionary<Tuple<int, int>, Tuple<double, Color>> _zBuffer)
	{
		private readonly Vector3 _point1 = point1;
		private readonly Vector3 _point2 = point2;
		private readonly Vector3 _point3 = point3;
		private readonly Color _color1 = color1;
		private readonly Color _color2 = color2;
		private readonly Color _color3 = color3;
		private readonly Dictionary<Tuple<int, int>, Tuple<double, Color>> zBuffer = _zBuffer;
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
					lock (zBuffer)
					{
						if (zBuffer.TryGetValue(key, out var value))
						{
							if (value.Item1 <= item.Y)
								zBuffer[key] = new Tuple<double, Color>(item.Y,
									Barycentric(new Vector3(key.Item1, key.Item2, (float)item.Y)));
						}
						else
							zBuffer.Add(key,
								new Tuple<double, Color>(item.Y,
									Barycentric(new Vector3(key.Item1, key.Item2, (float)item.Y))));

					}
				}
			}
		}
		private Color Barycentric(in Vector3 pointPos)
		{
			Vector3 sideVec1 = _point2 - _point1, sideVec2 = _point3 - _point1, sideVec3 = pointPos - _point1;
			var dot11 = Vector3.Dot(sideVec1, sideVec1);
			var dot12 = Vector3.Dot(sideVec1, sideVec2);
			var dot22 = Vector3.Dot(sideVec2, sideVec2);
			var dot31 = Vector3.Dot(sideVec3, sideVec1);
			var dot32 = Vector3.Dot(sideVec3, sideVec2);
			var denom = dot11 * dot22 - dot12 * dot12;
			var c1 = (dot22 * dot31 - dot12 * dot32) / denom;
			var c2 = (dot11 * dot32 - dot12 * dot31) / denom;
			var c3 = 1.0 - c1 - c2;
			return Color.Multiply(_color1, (float)c3) + Color.Multiply(_color2, c1) + Color.Multiply(_color3, c2);
		}
	}
}
