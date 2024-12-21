using System.IO;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;

namespace MiSideSoundsLoader;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log;

    public override void Load()
    {
        Log = base.Log;
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        string soundsPath = GetPluginSoundsPath();
        Log.LogInfo($"Sounds path: \"{soundsPath}\"");
        if (!Directory.Exists(soundsPath))
        {
            Log.LogInfo($"Path does not exist, creating...");
            Directory.CreateDirectory(soundsPath);
        }

        AudioFactory.Instance.InitializeClips(soundsPath);

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
                Log.LogInfo($"Replacing clip \"{source.clip.name}\"");
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
