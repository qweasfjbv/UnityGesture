using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gesture.Recognizer
{
	public class DollarPRecognizer : RecognizerBase
	{
		public override float[] Recognize(List<List<Vector2>> templates, List<Vector2> points)
		{
			List<float> rawScores = new List<float>();

			foreach (var template in templates)
			{
				//Debug.Log("COUNT CHECK : " + points.Count + ", " + template.Count);
				float d = GreedyCloudMatch(points, template, 64);
				rawScores.Add(d);
			}

			float max = rawScores.Max();
			float min = rawScores.Min();

			float[] normalizedScores = rawScores
				.Select(score => max > 0 ? ((max - score + min) / max) : 0f)
				.ToArray();

			return normalizedScores;
		}

		private float GreedyCloudMatch(List<Vector2> points, List<Vector2> template, int n)
		{
			float epsilon = 0.5f;
			int step = Mathf.FloorToInt(n * (1f - epsilon));
			float min = float.MaxValue;

			for (int i = 0; i < n; i += step)
			{
				float d1 = CloudDistance(points, template, n, i);
				float d2 = CloudDistance(template, points, n, i);
				min = Mathf.Min(min, Mathf.Min(d1, d2));
			}

			return min;
		}

		private float CloudDistance(List<Vector2> points, List<Vector2> tmpl, int n, int start)
		{
			bool[] matched = new bool[n];
			float sum = 0f;
			int i = start;

			do
			{
				float min = float.MaxValue;
				int index = -1;

				for (int j = 0; j < n; j++)
				{
					if (!matched[j])
					{
						//Debug.Log(i + ", " + j + ", "+  points.Count + ", " + tmpl.Count);
						float d = Vector2.Distance(points[i], tmpl[j]);
						if (d < min)
						{
							min = d;
							index = j;
						}
					}
				}

				if (index < 0) break;
				matched[index] = true;
				float weight = 1f - (((i - start + n) % n) / (float)n);
				sum += weight * min;
				i = (i + 1) % n;

			} while (i != start);

			return sum;
		}

	}
}