using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gesture
{
	public class Gesture
	{
		public static List<Vector2> PreprocessPoints(List<Vector2> points, bool isAll)
		{
			List<Vector2> newPoints = RemoveNaNPoints(points);
			newPoints = RemoveConsecutiveDuplicates(newPoints);

			newPoints = Resample(newPoints, 64);

			if (isAll)
			{
				newPoints = RotateToZero(newPoints);
			}
			newPoints = ScaleToSquare(newPoints, 250);
			newPoints = TranslateToOrigin(newPoints);

			return newPoints;
		}

		/// <summary>
		/// Remove Float.NaN value from data
		/// </summary>
		private static List<Vector2> RemoveNaNPoints(List<Vector2> points)
		{
			return points.FindAll(p => !float.IsNaN(p.x) && !float.IsNaN(p.y));
		}

		/// <summary>
		/// Remove ConsecutiveDuplicates from data
		///  which can cause Float.NaN value
		/// </summary>
		private static List<Vector2> RemoveConsecutiveDuplicates(List<Vector2> points)
		{
			List<Vector2> result = new List<Vector2>();

			if (points == null || points.Count == 0)
				return result;

			result.Add(points[0]);

			for (int i = 1; i < points.Count; i++)
			{
				if (points[i] != points[i - 1])
				{
					result.Add(points[i]);
				}
			}

			return result;
		}

		private static List<Vector2> Resample(List<Vector2> points, int n)
		{
			float pathLength = PathLength(points);
			float interval = pathLength / (n - 1);
			float D = 0f;

			List<Vector2> newPoints = new List<Vector2> { points[0] };

			for (int i = 1; i < points.Count; i++)
			{
				float d = Vector2.Distance(points[i - 1], points[i]);
				
				if (D + d >= interval)
				{
					float t = (interval - D) / d;
					Vector2 newPoint = Vector2.Lerp(points[i - 1], points[i], t);
					newPoints.Add(newPoint);
					points.Insert(i, newPoint);
					D = 0f;
				}
				else
				{
					D += d;
				}
			}

			if (newPoints.Count < n)
			{
				newPoints.Add(points[points.Count - 1]);
			}

			return newPoints;
		}
		private static float PathLength(List<Vector2> points)
		{
			float length = 0f;
			for (int i = 1; i < points.Count; i++)
			{
				length += Vector2.Distance(points[i - 1], points[i]);
			}
			return length;
		}
		public static List<Vector2> RotateToZero(List<Vector2> points)
		{
			Vector2 c = Centroid(points);
			Vector2 first = points[0];
			float angle = Mathf.Atan2(first.y - c.y, first.x - c.x);
			return RotateBy(points, -angle);
		}
		public static List<Vector2> RotateBy(List<Vector2> points, float radians)
		{
			Vector2 c = Centroid(points);
			List<Vector2> newPoints = new();

			foreach (var p in points)
			{
				float dx = p.x - c.x;
				float dy = p.y - c.y;
				float newX = dx * Mathf.Cos(radians) - dy * Mathf.Sin(radians) + c.x;
				float newY = dx * Mathf.Sin(radians) + dy * Mathf.Cos(radians) + c.y;
				newPoints.Add(new Vector2(newX, newY));
			}

			return newPoints;
		}
		private static List<Vector2> ScaleToSquare(List<Vector2> points, float size)
		{
			float minX = float.MaxValue, maxX = float.MinValue;
			float minY = float.MaxValue, maxY = float.MinValue;

			foreach (var p in points)
			{
				minX = Mathf.Min(minX, p.x);
				maxX = Mathf.Max(maxX, p.x);
				minY = Mathf.Min(minY, p.y);
				maxY = Mathf.Max(maxY, p.y);
			}

			float scaleX = maxX - minX;
			float scaleY = maxY - minY;

			List<Vector2> newPoints = new();
			foreach (var p in points)
			{
				float scaledX = (p.x - minX) / scaleX * size;
				float scaledY = (p.y - minY) / scaleY * size;
				newPoints.Add(new Vector2(scaledX, scaledY));
			}

			return newPoints;
		}
		private static List<Vector2> TranslateToOrigin(List<Vector2> points)
		{
			Vector2 c = Centroid(points);
			List<Vector2> newPoints = new();

			foreach (var p in points)
			{
				newPoints.Add(new Vector2(p.x - c.x, p.y - c.y));
			}

			return newPoints;
		}

		/// <summary>
		/// Find centroid of points
		/// </summary>
		private static Vector2 Centroid(List<Vector2> points)
		{
			float sumX = 0, sumY = 0;
			foreach (var p in points)
			{
				sumX += p.x;
				sumY += p.y;
			}
			return new Vector2(sumX / points.Count, sumY / points.Count);
		}
	}
}