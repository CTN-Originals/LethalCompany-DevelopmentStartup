using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

using DevelopmentStartup.Utilities;

namespace DevelopmentStartup
{
	[BepInPlugin(PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	public class Plugin : BaseUnityPlugin
	{
		public enum LaunchMode {
			Online,
			LAN,
		}
		public static LaunchMode launchMode = LaunchMode.LAN;
		public static ManualLogSource CLog;

		private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);

		void Awake() {
			CLog = BepInEx.Logging.Logger.CreateLogSource(PluginInfo.PLUGIN_NAME);
			CLog.LogInfo($"Plugin {PluginInfo.PLUGIN_NAME} is loaded! Version: {PluginInfo.PLUGIN_VERSION}");
			
			this.ConfigFile();
			harmony.PatchAll();
		}

		private void ConfigFile() {
			//TODO Edit the config of the FastStartup dependency to allow for this to be set ... or give up on this setting
			// launchMode = Config.Bind("General", "LaunchMode", LaunchMode.LAN, "The launch mode to start in (Online or LAN)").Value;
			// Console.Log($"LaunchMode: {launchMode}");
		}
	}

	//? Prevent the game from loading default settings before loading player settings (for example, full screen mode on startup)
	[HarmonyPatch(typeof(IngamePlayerSettings))]
	internal class IngamePlayerSettingsPatch {
		[HarmonyPostfix, HarmonyPatch("Awake")]
		private static void AwakePatch(IngamePlayerSettings __instance) {
			Console.LogMessage("Loading settings from prefs before the game loads default settings");
			__instance.LoadSettingsFromPrefs();
			__instance.UpdateGameToMatchSettings();
		}
	}


	[HarmonyPatch(typeof(MenuManager))]
	internal class MenuManagerPatch {
		private static bool firstTimeLoad = true;

		[HarmonyPostfix]
		[HarmonyPatch("OnEnable")]
		static public void OnEnablePatch(MenuManager __instance) {
			if (!firstTimeLoad) return;
			Console.LogMessage("To change the launch mode (Online | Lan), edit the config file for the \"FastStartup\" plugin");
			
			if (__instance.menuButtons != null && __instance.menuButtons.name == "MainButtons") {
				Console.LogInfo("MenuManager.OnEnablePatch() called - MainButtons");
				JumpInGame(__instance);
			}
			else {
				Console.LogInfo("MenuManager.OnEnablePatch() called - not MainButtons");
			}
		}

		static private void JumpInGame(MenuManager __instance) {
			Console.LogDebug("Entering lobby name");
			__instance.lobbyNameInputField.text = "DevelopmentStartup Lobby";
			Console.LogDebug("Confirm Host Button");
			__instance.ConfirmHostButton(); //? This is the same as clicking the "Host > Confirm" buttons

			firstTimeLoad = false;
		}
	}
}