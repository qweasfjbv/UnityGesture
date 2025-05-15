using Gesture.Data;
using Gesture.Recognizer;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gesture.Manager
{
	public class ExecuteManager : MonoBehaviour
	{
		#region Singleton

		private static ExecuteManager instance;
		public static ExecuteManager Instance { get => instance; }
		void Awake()
		{
			Init();
		}

		private void Init()
		{
			if (null == instance)
			{
				instance = this;
				DontDestroyOnLoad(this.gameObject);
			}
			else
			{
				Destroy(this.gameObject);
			}

		}
		#endregion

		[SerializeField] private LineRenderer line;
		[SerializeField] private Dropdown gestureDropdown;
		[SerializeField] private Dropdown dataIndexDropdown;

		[SerializeField] public RawData rawData;

		private List<Vector3> points = new List<Vector3>();
		private Camera mainCam;
		private bool isDrawing = false;

		private float[,,] comparedScore = new float[4, 80, 16];
		private float[,] spendTime = new float[4, 80];

		private void Start()
		{
			mainCam = Camera.main;

			List<string> stringOptions = new List<string>
			{
				"Triangle", "X", "Rectangle", "Circle",
				"Check", "Caret", "Question", "Arrow",
				"left Square Bracket", "Right Square Bracket", "V", "Delete",
				"Left Curly Brace", "Right Curly Brace", "Star", "Pigtail"
			};

			List<string> intOptions = new List<string>();
			for (int i = 0; i <= 5; i++)
			{
				intOptions.Add(i.ToString());
			}

			SetDropdownOptions(gestureDropdown, stringOptions);
			SetDropdownOptions(dataIndexDropdown, intOptions);
		}
		private void SetDropdownOptions(Dropdown dropdown, List<string> options)
		{
			dropdown.ClearOptions();
			List<Dropdown.OptionData> optionDataList = new List<Dropdown.OptionData>();

			foreach (string option in options)
			{
				optionDataList.Add(new Dropdown.OptionData(option));
			}

			dropdown.AddOptions(optionDataList);
		}
		private void Update()
		{
			if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
				return;

			if (Input.GetMouseButtonDown(0))
			{
				isDrawing = true;
				points.Clear();
				line.positionCount = 0;
			}

			if (isDrawing && Input.GetMouseButton(0))
			{
				Vector3 mousePos = Input.mousePosition;
				mousePos.z = 20f; 
				Vector3 worldPos = mainCam.ScreenToWorldPoint(mousePos);

				if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], worldPos) > 0.1f)
				{
					points.Add(worldPos);
					line.positionCount = points.Count;
					line.SetPosition(points.Count - 1, worldPos);
				}
			}

			if (Input.GetMouseButtonUp(0))
			{
				isDrawing = false;
			}
		}

		/// <summary>
		/// Save gesture into rawData
		/// </summary>
		public void OnSaveButton()
		{
			int gestureIndex = gestureDropdown.value;
			int dataIndex = dataIndexDropdown.value;

			List<Vector2> converted = new List<Vector2>();

			foreach (var point in points)
				converted.Add(new Vector2(point.x, point.y));

			rawData.SetData(gestureIndex, dataIndex, converted);

#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(rawData);
			UnityEditor.AssetDatabase.SaveAssets();
#endif
		}

		/// <summary>
		/// Load Gesture from rawData
		/// </summary>
		public void OnLoadButton()
		{
			int gestureIndex = gestureDropdown.value;
			int dataIndex = dataIndexDropdown.value;

			List<Vector2> loadedData = rawData.GetData(gestureIndex, dataIndex);

			line.positionCount = loadedData.Count;
			for (int i = 0; i < loadedData.Count; i++)
			{
				line.SetPosition(i, new Vector3(loadedData[i].x, loadedData[i].y, 10f));
			}
		}

		/// <summary>
		/// Function to compare gestures and save results as .csv file
		/// </summary>
		public void OnCompareButton()
		{
			List<List<Vector2>> templates = new();
			List<List<Vector2>> processedTemplates = new();
			List<Vector2> points = new();

			RecognizerBase[] recognizers = new RecognizerBase[4];
			recognizers[0] = new DollarOneRecognizer();
			recognizers[1] = new DollarPRecognizer();
			recognizers[2] = new ProtractorRecognizer();
			recognizers[3] = new DollarPRecognizer();

			Stopwatch sw = new Stopwatch();

			for (int i = 0; i < 4; i++)
			{
				bool isPreprocessAll = i == 0 || i == 3;

				// Preprocess Templates
				templates = rawData.ToTemplateList_RowY0Only();
				processedTemplates.Clear();

				for (int t = 0; t < templates.Count; t++)
				{
					processedTemplates.Add(Gesture.PreprocessPoints(templates[t], isPreprocessAll));
				}

				// Preprocess Compare gesture
				for (int x = 0; x < 16; x++)
				{
					for (int y = 1; y <= 5; y++)
					{
						sw.Restart();

						List<Vector2> gesture = Gesture.PreprocessPoints(rawData.GetData(x, y), isPreprocessAll);
						float[] results = recognizers[i].Recognize(processedTemplates, gesture);

						sw.Stop();
						SaveResultDatas(i, x, y, (float)sw.Elapsed.TotalMilliseconds, results);
					}
				}

				SaveToCSV(Path.Combine(Application.dataPath, "Data/score_data.csv"));
			}

		}
		private void SaveResultDatas(int recognizerIdx, int x, int y, float time, float[] results)
		{
			for (int i = 0; i < results.Count(); i++)
			{
				comparedScore[recognizerIdx, x * 5 + (y-1), i] = results[i];
			}

			spendTime[recognizerIdx, x * 5 + (y-1)] = time;

		}
		private void SaveToCSV(string path)
		{
			StringBuilder sb = new StringBuilder();

			for (int k = 0; k < 16; k++)
				sb.Append($"Score{k},");
			sb.Append("SpendTime\n");

			// µ¥ÀÌÅÍ
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 80; j++)
				{
					for (int k = 0; k < 16; k++)
					{
						sb.Append(comparedScore[i, j, k].ToString("F4"));
						sb.Append(",");
					}

					sb.Append(spendTime[i, j].ToString("F4"));
					sb.Append("\n");
				}
			}

			File.WriteAllText(path, sb.ToString());
		}
	}
}