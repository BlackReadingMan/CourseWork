using System.Numerics;
using System.Windows;
using System.Windows.Media;


namespace KURSOVAY.Algorithm
{
	internal static class Algorithms
	{
		public static void CDA(in Vector4 point1, in Vector4 point2, ref Dictionary<int, Tuple<Point, Point>> lines)
		{
			double dx = point2.X - point1.X;
			double dy = point2.Y - point1.Y;
			double dz = point2.Z - point1.Z;
			double step = Math.Max(Math.Max(Math.Abs(dx), Math.Abs(dy)), Math.Abs(dz));

			double x_incr = dx / step;
			double y_incr = dy / step;
			double z_incr = dz / step;

			double x = point1.X;
			double y = point1.Y;
			double z = point1.Z;

			for (int i = 0; i < step; i++)
			{
				int key = (int)Math.Round(y,MidpointRounding.ToZero);
				if (lines.TryGetValue(key, out Tuple<Point, Point>? check))
					if (x < check.Item1.X)
						lines[key] = new Tuple<Point, Point>(new Point(x, z), check.Item2);
					else
						if (x > check.Item2.X)
						lines[key] = new Tuple<Point, Point>(check.Item1, new Point(x, z));
				if (check == null)
					lines.Add(key, new Tuple<Point, Point>(new Point(x, z), new Point(x, z)));
				x += x_incr;
				y += y_incr;
				z += z_incr;
			}
		}
		public static List<Point> CDA(in Point point1, in Point point2)
		{
			List<Point> points = [];
			double dx = point2.X - point1.X;
			double dy = point2.Y - point1.Y;

			double step = Math.Max(Math.Abs(dx), Math.Abs(dy));


			double x_incr = dx / step;
			double y_incr = dy / step;

			double x = point1.X;
			double y = point1.Y;

			for (int i = 0; i < step; i++)
			{
				points.Add(new Point(x, y));
				x += x_incr;
				y += y_incr;
			}
			return points;
		}
		public static Color GetColor(in Vector3 Normal, in Vector3 FragPos, in Vector3 lightPosition, in Vector3 lightColor, in Vector3 objectColor)
		{
			float ambientStrength = 1f;
			Vector3 ambient = ambientStrength * lightColor;

			Vector3 lightDir = Vector3.Normalize(lightPosition - FragPos);
			float diff = Math.Max(Vector3.Dot(Normal, lightDir), 0.0f);
			Vector3 diffuse = diff * lightColor;

			Vector3 result = (ambient + diffuse) * objectColor;
			return Color.FromRgb((byte)(result.X * 255f), (byte)(result.Y * 255f), (byte)(result.Z * 255f));
		}
		public static Vector4 VectorMatrixMultiplication(in Vector4 vector, in Matrix4x4 inputMatrix)
		{
			Vector4 result = new();
			for (var i = 0; i < 4; i++)
			{
				result[i] = 0;

				for (var k = 0; k < 4; k++)
				{
					result[i] += inputMatrix[k, i] * vector[k];
				}
			}
			result /= result.W;
			return result;
		}
	}
}
