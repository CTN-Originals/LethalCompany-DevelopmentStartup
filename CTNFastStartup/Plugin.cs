using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FastStartup
{
	[BepInPlugin(PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	public class Plugin : BaseUnityPlugin
	{
		public enum GameMode {
			Online,
			LAN,
		}
		public static GameMode gameMode = GameMode.LAN;
		public static ManualLogSource CLog;

		private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);

		void Awake() {
			CLog = BepInEx.Logging.Logger.CreateLogSource(PluginInfo.PLUGIN_NAME);
			CLog.LogInfo($"Plugin {PluginInfo.PLUGIN_NAME} is loaded! Version: {PluginInfo.PLUGIN_VERSION}");
			
			this.ConfigFile();
			harmony.PatchAll();
		}

		private void ConfigFile() {
			gameMode = Config.Bind("General", "GameMode", GameMode.LAN, "The game mode to start in (Online or LAN)").Value;
			Console.Log($"GameMode: {gameMode}");
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
				switch (Plugin.gameMode) {
					case Plugin.GameMode.Online:
						SceneManager.LoadScene("InitScene");
						break;
					case Plugin.GameMode.LAN:
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
			if (Plugin.gameMode == Plugin.GameMode.LAN) {
				__instance.LAN_HostSetLocal();
			}
			__instance.lobbyNameInputField.text = "CTN Fast Startup Lobby";
			__instance.ConfirmHostButton();

			firstTimeLoad = false;
		}
	}
}