using System.Numerics;
using System.Windows;
using System.Windows.Media;

namespace CourseWork.Algorithms;

internal static class Algorithms
{
	public static void Cda(in Vector3 point1, in Vector3 point2, Dictionary<int, Tuple<Point, Point>> lines)
	{
		double dx = point2.X - point1.X;
		double dy = point2.Y - point1.Y;
		double dz = point2.Z - point1.Z;
		var step = Math.Max(Math.Max(Math.Abs(dx), Math.Abs(dy)), Math.Abs(dz));

		var xIncr = dx / step;
		var yIncr = dy / step;
		var zIncr = dz / step;

		double x = point1.X;
		double y = point1.Y;
		double z = point1.Z;

		for (var i = 0; i < step; i++)
		{
			var key = (int)Math.Round(y, MidpointRounding.ToZero);
			if (lines.TryGetValue(key, out var check))
				if (x < check.Item1.X)
					lines[key] = new Tuple<Point, Point>(new Point(x, z), check.Item2);
				else if (x > check.Item2.X)
					lines[key] = new Tuple<Point, Point>(check.Item1, new Point(x, z));
			if (check == null)
				lines.Add(key, new Tuple<Point, Point>(new Point(x, z), new Point(x, z)));
			x += xIncr;
			y += yIncr;
			z += zIncr;
		}
	}

	public static List<Point> Cda(in Point point1, in Point point2)
	{
		List<Point> points = [];
		var dx = point2.X - point1.X;
		var dy = point2.Y - point1.Y;

		var step = Math.Max(Math.Abs(dx), Math.Abs(dy));

		var xIncr = dx / step;
		var yIncr = dy / step;

		var x = point1.X;
		var y = point1.Y;

		points.Add(new Point(x, y));
		for (var i = 0; i < step; i++)
		{
			points.Add(new Point(x, y));
			x += xIncr;
			y += yIncr;
		}

		return points;
	}

	public static Color GetColor(in Vector3 normal, in Vector3 fragmentPosition, in Vector3 lightPosition,
		in Vector3 lightColor, in Vector3 objectColor)
	{
		const float ambientStrength = 0f;
		var ambient = ambientStrength * lightColor;

		var lightDir = Vector3.Normalize(lightPosition - fragmentPosition);
		var diff = Math.Max(Vector3.Dot(normal, lightDir), 0.0f);
		var diffuse = diff * lightColor;

		const float specularStrength = 0f;
		var viewDir = Vector3.Normalize(lightPosition - fragmentPosition);
		var reflectDir = Vector3.Reflect(-lightDir, normal);
		var spec = (float)Math.Pow(Math.Max(Vector3.Dot(viewDir, reflectDir), 0.0f), 32f);
		var specular = specularStrength * spec * lightColor;

		var finalLight = ambient + diffuse + specular;
		for (var i = 0; i < 3; i++)
			if (finalLight[i] > 1f)
				finalLight[i] = 1f;
		var result = finalLight * objectColor;
		return Color.FromRgb((byte)(result.X * 255f), (byte)(result.Y * 255f), (byte)(result.Z * 255f));
	}
}