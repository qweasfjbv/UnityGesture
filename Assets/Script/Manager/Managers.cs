using Garage.Manager;
using UnityEngine;

namespace Gesutre.Manager
{
	public class Managers : MonoBehaviour
	{
		private static Managers instance;
		public static Managers Instance { get => instance; }

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

		private static ResourceManager _resource = new ResourceManager();
		
		public static ResourceManager Resource { get => _resource; }
	}
}