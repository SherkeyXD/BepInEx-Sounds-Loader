using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;

namespace MiSideSoundsLoader;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log;

    public override void Load()
    {
        Log = base.Log;
        Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

        string soundsPath = GetPluginSoundsPath();
        Log.LogInfo($"Sounds path: \"{soundsPath}\"");
        if (!Directory.Exists(soundsPath))
        {
            Log.LogInfo($"Path does not exist, creating...");
            Directory.CreateDirectory(soundsPath);
        }

        AudioFactory.Instance.LoadAllClips(soundsPath);

        Harmony.CreateAndPatchAll(typeof(AudioSourcePatch));
    }

    [HarmonyPatch(typeof(AudioSource))]
    internal class AudioSourcePatch
    {
        [HarmonyPatch(nameof(AudioSource.Play), [])]
        [HarmonyPrefix]
        public static void Play_Patch(AudioSource __instance) => ReplaceClip(__instance);

        [HarmonyPatch(nameof(AudioSource.Play), [typeof(ulong)])]
        [HarmonyPrefix]
        public static void Play_UlongPatch(AudioSource __instance) => ReplaceClip(__instance);

        [HarmonyPatch(nameof(AudioSource.Play), [typeof(double)])]
        [HarmonyPrefix]
        public static void Play_DoublePatch(AudioSource __instance) => ReplaceClip(__instance);

        [HarmonyPatch(nameof(AudioSource.PlayDelayed), [typeof(float)])]
        [HarmonyPrefix]
        public static void PlayDelayed_Patch(AudioSource __instance) => ReplaceClip(__instance);

        public static void ReplaceClip(AudioSource source)
        {
            AudioFactory.Instance.CheckClips();
            if (source == null || source.clip == null)
            {
                return;
            }

            AudioClip newClip = AudioFactory.Instance.GetClip(source.clip.name);
            if (newClip != null)
            {
                Log.LogWarning($"Replacing clip \"{source.clip.name}\" with \"{newClip.name}\"");
                source.clip = newClip;
                return;
            }
        }
    }

    public static string GetPluginPath()
    {
        return Paths.PluginPath;
    }

    public static string GetPluginSoundsPath()
    {
        return Path.Combine(GetPluginPath(), "CustomSounds");
    }
}
