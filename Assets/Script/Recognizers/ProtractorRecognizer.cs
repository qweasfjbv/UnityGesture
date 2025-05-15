using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gesture.Recognizer
{
	public class ProtractorRecognizer : RecognizerBase
	{
		public override float[] Recognize(List<List<Vector2>> templates, List<Vector2> points)
		{
			List<float> rawScores = new List<float>();

			foreach (var template in templates)
			{
				float score = OptimalCosineDistanceScore(points, template); 
				rawScores.Add(score);
			}

			float min = rawScores.Min();
			float max = rawScores.Max();
			float range = max - min;

			float[] normalizedScores = rawScores
				.Select(score => range > 0 ? (score - min) / range : 1f)
				.ToArray();

			return normalizedScores;
		}

		private float OptimalCosineDistanceScore(List<Vector2> a, List<Vector2> b)
		{
			float sumA = 0f;
			float sumB = 0f;
			float sumC = 0f;


			for (int i = 0; i < a.Count; i++)
			{
				sumA += a[i].x * b[i].x + a[i].y * b[i].y;
				sumB += a[i].x * a[i].x + a[i].y * a[i].y;
				sumC += b[i].x * b[i].x + b[i].y * b[i].y;
			}

			float numerator = sumA;
			float denominator = Mathf.Sqrt(sumB) * Mathf.Sqrt(sumC);

			float angle = Mathf.Acos(Mathf.Clamp(numerator / denominator, -1f, 1f));

			return 1 / angle;
		}
	}

}