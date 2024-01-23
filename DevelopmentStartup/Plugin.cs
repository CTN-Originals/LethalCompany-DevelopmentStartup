using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

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
			launchMode = Config.Bind("General", "LaunchMode", LaunchMode.LAN, "The launch mode to start in (Online or LAN)").Value;
			Console.Log($"LaunchMode: {launchMode}");
		}
	}

	public class Console {
		public static void Log(string message) {
			Plugin.CLog.LogInfo(message);
		}
	}

	// Source: https://thunderstore.io/c/lethal-company/p/Owen3H/IntroTweaks/source/
	[HarmonyPatch(typeof(PreInitSceneScript))]
	internal class PreSceneInitPatch
	{
		[HarmonyPrefix]
		[HarmonyPatch("SkipToFinalSetting")]
		private static bool OverrideSkipToFinal()
		{
			return false;
		}

		[HarmonyPostfix]
		[HarmonyPatch("Start")]
		private static void SkipToOnline(PreInitSceneScript __instance, ref bool ___choseLaunchOption)
		{
			CollectionExtensions.Do<GameObject>(__instance.LaunchSettingsPanels, delegate(GameObject p) {
				p.gameObject.SetActive(false);
			});
			__instance.currentLaunchSettingPanel = 0;
			__instance.headerText.text = "";
			__instance.blackTransition.gameObject.SetActive(false);
			__instance.continueButton.gameObject.SetActive(false);
			___choseLaunchOption = true;
			__instance.mainAudio.PlayOneShot(__instance.selectSFX);
			IngamePlayerSettings.Instance.SetPlayerFinishedLaunchOptions();
			IngamePlayerSettings.Instance.SaveChangedSettings();
			if (!IngamePlayerSettings.Instance.encounteredErrorDuringSave)
			{
				switch (Plugin.launchMode) {
					case Plugin.LaunchMode.Online:
						SceneManager.LoadScene("InitScene");
						break;
					case Plugin.LaunchMode.LAN:
					default:
						SceneManager.LoadScene("InitSceneLANMode");
						break;
				}
				// SceneManager.LoadScene("InitScene");
			}
		}
	}


	[HarmonyPatch(typeof(MenuManager))]
	internal class MenuManagerPatch {
		private static bool firstTimeLoad = true;
		
		[HarmonyPostfix]
		[HarmonyPatch("Start")]
		static public void StartPatch() {
			// Console.Log("MenuManager.Start() called");
		}
		[HarmonyPostfix]
		[HarmonyPatch("OnEnable")]
		static public void OnEnablePatch(MenuManager __instance) {
			// Console.Log("MenuManager.OnEnablePatch() called");
			if (!firstTimeLoad) {
				return;
			}
			__instance.ClickHostButton();
			if (Plugin.launchMode == Plugin.LaunchMode.LAN) {
				__instance.LAN_HostSetLocal();
			}
			__instance.lobbyNameInputField.text = "DevelopmentStartup Lobby";
			__instance.ConfirmHostButton();

			firstTimeLoad = false;
		}
	}
}