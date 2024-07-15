global using LLV = BepInEx.Logging.LogLevel;
using BepInEx;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;

namespace Ragdoll
{
	[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
	public class P : BaseUnityPlugin
	{
		public static P Instance;
		public static AssetBundle assets;
		private void Awake()
		{
			Instance = this;
			assets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ragdoll"));

			NetcodePatcher();
			RagdollPatch.Patch();

			Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} [{MyPluginInfo.PLUGIN_VERSION}] loaded");
		}

		private void NetcodePatcher()
		{
			var types = Assembly.GetExecutingAssembly().GetTypes();
			foreach (var type in types)
			{
				var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
				foreach (var method in methods)
				{
					var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
					if (attributes.Length > 0)
					{
						method.Invoke(null, null);
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void Log(object data, BepInEx.Logging.LogLevel lvl = BepInEx.Logging.LogLevel.Message)
		{
			Instance.Logger.Log(lvl, data);
		}
	}
}
