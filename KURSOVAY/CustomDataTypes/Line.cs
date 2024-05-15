using System.Numerics;
using System.Windows;
using System.Windows.Media;

namespace CourseWork.CustomDataTypes
{
	internal class Line
	{
		private readonly Vector3 _point1;
		private readonly Vector3 _point2;
		private readonly Color _color1;
		private readonly Color _color2;
		private readonly Vector3 _sideVec1;
		private readonly float _dot11;
		private readonly Dictionary<Tuple<int, int>, Tuple<double, Color>> _zBuffer;

		public Line(in Vector3 point1, in Vector3 point2, in Color color1, in Color color2, Dictionary<Tuple<int, int>, Tuple<double, Color>> zBuffer)
		{
			_point1 = point1;
			_point2 = point2;
			_color1 = color1;
			_color2 = color2;
			_zBuffer = zBuffer;
			_sideVec1 = _point2 - _point1;
			_dot11 = Vector3.Dot(_sideVec1, _sideVec1);
		}

		public void MakeFill(in bool ignoreZ = false)
		{
			Dictionary<int, Tuple<Point, Point>> frame = [];
			Algorithms.Algorithms.Cda(_point1, _point2, frame);
			if (ignoreZ)
				foreach (var lines in frame)
				{
					foreach (var item in Algorithms.Algorithms.Cda(lines.Value.Item1, lines.Value.Item2))
					{
						Tuple<int, int> key = new((int)Math.Round(item.X, MidpointRounding.ToZero), lines.Key);
						lock (_zBuffer)
						{
							if (_zBuffer.TryGetValue(key, out var value))
							{
								_zBuffer[key] = new Tuple<double, Color>(item.Y,
									Barycentric(new Vector3(key.Item1, key.Item2, (float)item.Y)));
							}
							else
								_zBuffer.Add(key,
									new Tuple<double, Color>(item.Y,
										Barycentric(new Vector3(key.Item1, key.Item2, (float)item.Y))));

						}
					}
				}
			else

				foreach (var lines in frame)
				{
					foreach (var item in Algorithms.Algorithms.Cda(lines.Value.Item1, lines.Value.Item2))
					{
						Tuple<int, int> key = new((int)Math.Round(item.X, MidpointRounding.ToZero), lines.Key);
						lock (_zBuffer)
						{
							if (_zBuffer.TryGetValue(key, out var value))
							{
								if (value.Item1 <= item.Y)
									_zBuffer[key] = new Tuple<double, Color>(item.Y,
										Barycentric(new Vector3(key.Item1, key.Item2, (float)item.Y)));
							}
							else
								_zBuffer.Add(key,
									new Tuple<double, Color>(item.Y,
										Barycentric(new Vector3(key.Item1, key.Item2, (float)item.Y))));

						}
					}
				}
		}

		private Color Barycentric(in Vector3 pointPos)
		{
			var sideVec3 = pointPos - _point1;
			var dot31 = Vector3.Dot(sideVec3, _sideVec1);
			var c1 = (_dot11 - dot31) / _dot11;
			var c2 = 1.0f - c1;
			return Color.Multiply(_color1, c1) + Color.Multiply(_color2, c2);
		}
	}
}
