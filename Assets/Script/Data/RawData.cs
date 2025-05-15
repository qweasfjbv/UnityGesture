using System.Collections.Generic;
using UnityEngine;

namespace Gesture.Data
{
	[System.Serializable]
	public class Vector2ListWrapper
	{
		public List<Vector2> points;
	}

	[CreateAssetMenu(fileName = "RawData", menuName = "Data/RawData")]
	public class RawData : ScriptableObject
	{
		public List<Vector2ListWrapper> dataGrid = new List<Vector2ListWrapper>(96);

		private void OnEnable()
		{
			if (dataGrid.Count != 16 * 6)
			{
				dataGrid.Clear();
				for (int i = 0; i < 16 * 6; i++)
					dataGrid.Add(new Vector2ListWrapper { points = new List<Vector2>() });
			}
		}

		public List<Vector2> GetData(int x, int y)
		{
			return dataGrid[x * 6 + y].points;
		}

		public void SetData(int x, int y, List<Vector2> data)
		{
			dataGrid[x * 6 + y].points = data;
		}

		public List<List<Vector2>> ToTemplateList_RowY0Only()
		{
			List<List<Vector2>> templates = new();

			for (int x = 0; x < 16; x++)
			{
				templates.Add(new List<Vector2>(GetData(x, 0)));
			}

			return templates;
		}
	}
}
