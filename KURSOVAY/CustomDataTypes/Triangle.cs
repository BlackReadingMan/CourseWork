﻿using KURSOVAY.Algorithm;
using System.Numerics;
using System.Windows;
using System.Windows.Media;

namespace KURSOVAY.CustomDataTypes
{
	internal class Triangle
	{
		private Vector4 point1;
		private Vector4 point2;
		private Vector4 point3;
		private Color color;
		public Triangle(in Vector4 _point1, in Vector4 _point2, in Vector4 _point3, in Color _color)
		{
			point1 = _point1;
			point2 = _point2;
			point3 = _point3;
			color = _color;
		}
		public void MakeFill()
		{
			buffer = new();
			Dictionary<double, Tuple<Point, Point>> cheats = [];
			Algorithms.CDA(point1, point2, ref cheats);
			Algorithms.CDA(point2, point3, ref cheats);
			Algorithms.CDA(point3, point1, ref cheats);
			foreach (var lines in cheats)
			{
				foreach (var item in Algorithms.CDA(lines.Value.Item1, lines.Value.Item2))
				{
					Tuple<int, int> key = new((int)Math.Round(item.X), (int)Math.Round(lines.Key));
					if (buffer.TryGetValue(key, out Tuple<float, Color>? value))
					{
						if (value.Item1 <= item.Y)
							buffer[key] = new Tuple<float, Color>((float)item.Y, color);
					}
					if (value == null)
						buffer.Add(key, new Tuple<float, Color>((float)item.Y, color));
				}
			}
		}
		private Dictionary<Tuple<int, int>, Tuple<float, Color>> _buffer;
		public Dictionary<Tuple<int, int>, Tuple<float, Color>> buffer
		{
			get { return _buffer; }
			set
			{
				_buffer = value;
			}
		}
	}
}