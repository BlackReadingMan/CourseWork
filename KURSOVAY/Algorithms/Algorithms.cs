using KURSOVAY.CustomDataTypes;
using System.Numerics;
using System.Windows;
using System.Windows.Media;


namespace KURSOVAY.Algorithm
{
	internal static class Algorithms
	{
		private static void CDA(Vector4 point1, Vector4 point2, ref Dictionary<double, Tuple<Point, Point>> lines)
		{
			double dx = point2.X - point1.X;
			double dy = point2.Y - point1.Y;
			double dz = point2.Z - point1.Z;
			double step;

			step = Math.Max(Math.Max(Math.Abs(dx), Math.Abs(dy)), Math.Abs(dz));

			double x_incr = dx / step;
			double y_incr = dy / step;
			double z_incr = dz / step;

			double x = point1.X;
			double y = point1.Y;
			double z = point1.Z;

			for (int i = 0; i < step; i++)
			{
				if (lines.TryGetValue(Math.Round(y), out Tuple<Point, Point>? check))
					if (x < check.Item1.X)
						lines[Math.Round(y)] = new Tuple<Point, Point>(new Point(x,z), check.Item2);
					else
						if (x > check.Item2.X)
						lines[Math.Round(y)] = new Tuple<Point, Point>(check.Item1,new Point(x,z));
				if(check==null)
					lines.Add(Math.Round(y), new Tuple<Point, Point>(new Point(x, z), new Point(x,z)));
				x += x_incr;
				y += y_incr;
				z += z_incr;
			}
		}
		private static List<Point> CDA(Point point1, Point point2)
		{
			List<Point> points = [];
			double dx = point2.X - point1.X;
			double dz = point2.Y - point1.Y;

			double step = Math.Max(Math.Abs(dx), Math.Abs(dz));


			double x_incr = dx / step;
			double z_incr = dz / step;

			double x = point1.X;
			double z = point1.Y;

			for (int i = 0; i < step; i++)
			{
				points.Add(new Point(x, z));
				x += x_incr;
				z += z_incr;
			}
			return points;
		}
		private static void Triangle(Vector4 point1, Vector4 point2, Vector4 point3, Color color, ref Dictionary<Tuple<int, int>, Tuple<float, Color>> zbuffer)
		{
			Dictionary<double, Tuple<Point, Point>> cheats = [];
			CDA(point1, point2, ref cheats);
			CDA(point2, point3, ref cheats);
			CDA(point3, point1, ref cheats);
			foreach (var lines in cheats)
			{
                foreach (var item in CDA(lines.Value.Item1, lines.Value.Item2))
                {
					Tuple<int, int> key = new((int)Math.Round(item.X), (int)Math.Round(lines.Key));
					if (zbuffer.TryGetValue(key, out Tuple<float, Color>? value))
					{
						if (value.Item1 <= item.Y)
							zbuffer[key] = new Tuple<float, Color>((float)item.Y, color);
					}
					if (value == null)
						zbuffer.Add(key, new Tuple<float, Color>((float)item.Y, color));
				}
            }
		}
		public static List<Tuple<int, int, Color>> GetViewPoints(Figure figures, Matrix4x4 final, Vector3 lightPosition, Vector3 lightColor, Vector3 objectColor)
		{
			Vector4 point1;
			Vector4 point2;
			Vector4 point3;
			Vector3 FragPos;
			List<Tuple<int, int, Color>> res = [];
			Dictionary<Tuple<int, int>, Tuple<float, Color>> zbuffer = [];
			foreach (var triangle in figures.F)
			{
				point1 = MakeWorldPoint(figures.V[triangle.Item1.Item1 - 1], final);
				point2 = MakeWorldPoint(figures.V[triangle.Item2.Item1 - 1], final);
				point3 = MakeWorldPoint(figures.V[triangle.Item3.Item1 - 1], final);
				FragPos = new Vector3(
					((figures.V[triangle.Item1.Item1 - 1].X + figures.V[triangle.Item2.Item1 - 1].X + figures.V[triangle.Item3.Item1 - 1].X) / 3f),
					((figures.V[triangle.Item1.Item1 - 1].Y + figures.V[triangle.Item2.Item1 - 1].Y + figures.V[triangle.Item3.Item1 - 1].Y) / 3f),
					((figures.V[triangle.Item1.Item1 - 1].Z + figures.V[triangle.Item2.Item1 - 1].Z + figures.V[triangle.Item3.Item1 - 1].Z) / 3f));
				Triangle(point1, point2, point3, GetColor(figures.VN[triangle.Item1.Item3 - 1], new(FragPos.X, FragPos.Y, FragPos.Z), new(lightPosition.X, lightPosition.Y, lightPosition.Z), lightColor, objectColor), ref zbuffer);
			}
			foreach (var item in zbuffer)
			{
				res.Add(new Tuple<int, int, Color>(item.Key.Item1, item.Key.Item2, item.Value.Item2));
			}
			return res;
		}
		private static Color GetColor(Vector3 Normal, Vector3 FragPos, Vector3 lightPosition, Vector3 lightColor, Vector3 objectColor)
		{
			float ambientStrength = 1f;
			Vector3 ambient = ambientStrength * lightColor;

			Vector3 lightDir = Vector3.Normalize(lightPosition - FragPos);
			float diff = Math.Max(Vector3.Dot(Normal, lightDir), 0.0f);
			Vector3 diffuse = diff * lightColor;

			Vector3 result = (ambient + diffuse) * objectColor;
			return Color.FromRgb((byte)(result.X * 255f), (byte)(result.Y * 255f), (byte)(result.Z * 255f));
		}
		private static Vector4 VectorMatrixMultiplication(Matrix4x4 inputMatrix, Vector4 vector)
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
			return result;
		}
		private static Vector4 MakeWorldPoint(Vector4 point, Matrix4x4 final)
		{
			point = VectorMatrixMultiplication(final, point);
			point /= point.W;
			return point;
		}
		private static Vector4 RoundPoint(Vector4 point)
		{
			point.X = (float)Math.Round(point.X);
			point.Y = (float)Math.Round(point.Y);
			point.Z = (float)Math.Round(point.Z);
			return point;
		}
	}
}
