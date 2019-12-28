using System;
using UnityEngine;

namespace RuntimeScriptField
{
	[Serializable]
	public class ComponentReference : ScriptReference<Component>
	{
		public void AddTo(GameObject gameObject)
		{
			gameObject.AddComponent(script);
		}

		public void AddTo(Component component)
		{
			component.gameObject.AddComponent(script);
		}
	}
}