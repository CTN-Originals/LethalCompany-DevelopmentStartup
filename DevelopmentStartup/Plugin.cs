﻿using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

using DevelopmentStartup.Utilities;
using System.Threading;

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
		public static bool autoJoinLan;
		public static bool autoPullLever;

        internal static bool IsHostInstance;

		public static ManualLogSource CLog;
		private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);

        void Awake() {
			#if DEBUG //TODO make a config option for this
				const bool debugMode = true;
			#else
				const bool debugMode = false;
			#endif

            CLog = BepInEx.Logging.Logger.CreateLogSource(PluginInfo.PLUGIN_NAME);
			CLog.LogInfo($"Plugin {PluginInfo.PLUGIN_NAME} is loaded! Version: {PluginInfo.PLUGIN_VERSION} ({(debugMode ? "Debug" : "Release")})");
			
			this.ConfigFile();
			harmony.PatchAll();

			IsHostInstance = !autoJoinLan || !CheckMutex();
		}

		private void ConfigFile() {
			//TODO Edit the config of the FastStartup dependency to allow for this to be set ... or give up on this setting
			// launchMode = Config.Bind("General", "LaunchMode", LaunchMode.LAN, "The launch mode to start in (Online or LAN)").Value;
			// Console.Log($"LaunchMode: {launchMode}");

			autoJoinLan = Config.Bind("General", "AutoJoinLAN", true, "Automatically join LAN lobbies when game is launched more than once.").Value;
			autoPullLever = Config.Bind("General", "Auto Pull Lever", false, "Automatically pull the ship's lever on startup.").Value;
        }

        private static Mutex AppMutex;
        internal static bool CheckMutex() {
            try {
				if (AppMutex == null) AppMutex = new Mutex(true, "LethalCompany-" + PluginInfo.PLUGIN_NAME);
                return AppMutex != null && !AppMutex.WaitOne(System.TimeSpan.Zero, true);
            }
            catch {
                return false;
            }
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

			if (Plugin.IsHostInstance) {
				__instance.ConfirmHostButton(); //? This is the same as clicking the "Host > Confirm" buttons
			} else {
				__instance.StartAClient();
			}

            firstTimeLoad = false;
		}
	}

	[HarmonyPatch(typeof(StartMatchLever))]
	internal class StartMatchLeverPatch {
		private static bool hasPulledLever;

		[HarmonyPatch("Start"), HarmonyPostfix]
		static public void StartPatch(StartMatchLever __instance) {
			if (!Plugin.autoPullLever || hasPulledLever) return;

			Console.LogDebug("Pullling Ship Lever");

			// __instance.LeverAnimation();
			hasPulledLever = true;
			__instance.leverHasBeenPulled = true;
			__instance.leverAnimatorObject.SetBool("pullLever", true);
			__instance.triggerScript.interactable = false;
			__instance.PullLever();
		}
	}
}