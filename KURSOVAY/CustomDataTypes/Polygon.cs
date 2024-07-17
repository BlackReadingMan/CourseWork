using System.Numerics;
using System.Windows;
using System.Windows.Media;

namespace CourseWork.CustomDataTypes;

internal class Polygon
{
	private readonly Vector3 _point1;
	private readonly Vector3 _point2;
	private readonly Vector3 _point3;
	private readonly Color _color1;
	private readonly Color _color2;
	private readonly Color _color3;
	private readonly Vector3 _sideVec1;
	private readonly Vector3 _sideVec2;
	private readonly float _dot11;
	private readonly float _dot12;
	private readonly float _dot22;
	private readonly float _denominator;
	private readonly Dictionary<Tuple<int, int>, Tuple<double, Color>> _zBuffer;

	public Polygon(in Vector3 point1, in Vector3 point2, in Vector3 point3, in Color color1, in Color color2,
		in Color color3, Dictionary<Tuple<int, int>, Tuple<double, Color>> zBuffer)
	{
		_point1 = point1;
		_point2 = point2;
		_point3 = point3;
		_color1 = color1;
		_color2 = color2;
		_color3 = color3;
		_zBuffer = zBuffer;
		_sideVec1 = _point2 - _point1;
		_sideVec2 = _point3 - _point1;
		_dot11 = Vector3.Dot(_sideVec1, _sideVec1);
		_dot12 = Vector3.Dot(_sideVec1, _sideVec2);
		_dot22 = Vector3.Dot(_sideVec2, _sideVec2);
		_denominator = _dot11 * _dot22 - _dot12 * _dot12;
	}

	public void MakeFill()
	{
		Dictionary<int, Tuple<Point, Point>> frame = [];
		Algorithms.Algorithms.Cda(_point1, _point2, frame);
		Algorithms.Algorithms.Cda(_point2, _point3, frame);
		Algorithms.Algorithms.Cda(_point3, _point1, frame);

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
		var dot32 = Vector3.Dot(sideVec3, _sideVec2);
		var c1 = (_dot22 * dot31 - _dot12 * dot32) / _denominator;
		var c2 = (_dot11 * dot32 - _dot12 * dot31) / _denominator;
		var c3 = 1.0f - c1 - c2;
		return Color.Multiply(_color1, c3) + Color.Multiply(_color2, c1) + Color.Multiply(_color3, c2);
	}
}