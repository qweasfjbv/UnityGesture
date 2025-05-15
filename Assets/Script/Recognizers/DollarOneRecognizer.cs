using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gesture.Recognizer
{
	public abstract class RecognizerBase
	{
		public abstract float[] Recognize(List<List<Vector2>> templates, List<Vector2> points);
	}

	public class DollarOneRecognizer : RecognizerBase
	{
		private float angleRange = 45f * Mathf.Deg2Rad;
		private float anglePrecision = 2f * Mathf.Deg2Rad;
		private float goldenRatio = 0.5f * (-1f + Mathf.Sqrt(5f));

		public override float[] Recognize(List<List<Vector2>> templates, List<Vector2> points)
		{
			List<float> rawScores = new List<float>();

			foreach (var template in templates)
			{
				float distance = DistanceAtBestAngle(points, template, -angleRange, angleRange, anglePrecision);
				rawScores.Add(distance);
			}

			float max = rawScores.Max();
			float min = rawScores.Min();

			float[] normalizedScores = rawScores
				.Select(score => max > 0 ? ((max - score + min) / max) : 0f)
				.ToArray();

			return normalizedScores;
		}

		private float DistanceAtBestAngle(List<Vector2> points, List<Vector2> template, float thetaA, float thetaB, float deltaTheta)
		{
			float x1 = goldenRatio * thetaA + (1 - goldenRatio) * thetaB;
			float f1 = DistanceAtAngle(points, template, x1);

			float x2 = (1 - goldenRatio) * thetaA + goldenRatio * thetaB;
			float f2 = DistanceAtAngle(points, template, x2);

			while (Mathf.Abs(thetaB - thetaA) > deltaTheta)
			{
				if (f1 < f2)
				{
					thetaB = x2;
					x2 = x1;
					f2 = f1;
					x1 = goldenRatio * thetaA + (1 - goldenRatio) * thetaB;
					f1 = DistanceAtAngle(points, template, x1);
				}
				else
				{
					thetaA = x1;
					x1 = x2;
					f1 = f2;
					x2 = (1 - goldenRatio) * thetaA + goldenRatio * thetaB;
					f2 = DistanceAtAngle(points, template, x2);
				}
			}

			return Mathf.Min(f1, f2);
		}

		private float DistanceAtAngle(List<Vector2> points, List<Vector2> template, float angle)
		{
			var newPoints = Gesture.RotateBy(points, angle);
			return PathDistance(newPoints, template);
		}

		private float PathDistance(List<Vector2> a, List<Vector2> b)
		{
			float d = 0;
			for (int i = 0; i < a.Count; i++)
			{
				d += UnityEngine.Vector2.Distance(new UnityEngine.Vector2(a[i].x, a[i].y), new UnityEngine.Vector2(b[i].x, b[i].y));
			}
			return d / a.Count;
		}

	}
}