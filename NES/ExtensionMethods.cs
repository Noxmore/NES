using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Raylib_cs;

using Image = Raylib_cs.Image;
using Color = Raylib_cs.Color;

namespace NES
{
	public static class ExtensionMethods
	{
		public static T ShallowClone<T>(this T o)
		{
			return (T)o?.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(o, null);
		}


		public static float Clamp(this float f, float min, float max)
		{
			if (f > max) f = max;
			if (f < min) f = min;
			return f;
		}
		public static float ClampMin(this float f, float min)
		{
			if (f < min) f = min;
			return f;
		}
		public static float ClampMax(this float f, float max)
		{
			if (f > max) f = max;
			return f;
		}


		public static int Clamp(this int f, int min, int max)
		{
			if (f > max) f = max;
			if (f < min) f = min;
			return f;
		}
		public static int ClampMin(this int f, int min)
		{
			if (f < min) f = min;
			return f;
		}
		public static int ClampMax(this int f, int max)
		{
			if (f > max) f = max;
			return f;
		}



		public static float Lerp(this float first, float second, float by)
		{
			//return first * (1 - by) + second * by;
			return first + (second - first) * by;
		}


		public static System.Drawing.Color Lerp(this System.Drawing.Color first, System.Drawing.Color second, float by)
		{
			float r = Lerp(first.R, second.R, by);
			float g = Lerp(first.G, second.G, by);
			float b = Lerp(first.B, second.B, by);
			float a = Lerp(first.A, second.A, by);
			return System.Drawing.Color.FromArgb((int)a, (int)r, (int)g, (int)b);
		}


		public static Color ToRaylibColor(this System.Drawing.Color color)
		{
			return new(color.R, color.G, color.B, color.A);
		}

		public static System.Drawing.Color ToNesColor(this Color color)
		{
			return System.Drawing.Color.FromArgb(color.a, color.r, color.g, color.b);
		}


		public static Vector2 ToVector2(this Point point) => new(point.X, point.Y);


		public static void SetPixel(this Image image, int x, int y, Color color)
		{
			Raylib.ImageDrawPixel(ref image, x, y, color);
		}

		public static Color GetPixel(this Image image, int x, int y) // dealing with pointers because raylib was written in c
		{
			unsafe
			{
				Color* pixels = (Color*)image.data.ToPointer();

				if (x > image.width || x < 0 || y > image.height || y < 0) throw new IndexOutOfRangeException();

				return pixels[y * image.width + x];
			}
		}




		
	}
}
