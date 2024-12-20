using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;

namespace MiSideSoundsLoader;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log;

    public static readonly Dictionary<string, AudioClip> replacedClips = [];

    public override void Load()
    {
        Log = base.Log;
        Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

        string SoundsPath = GetPluginSoundsPath();
        Log.LogInfo($"Sounds path: \"{SoundsPath}\"");
        if (!Directory.Exists(SoundsPath))
        {
            Log.LogInfo($"Path does not exist, creating...");
            Directory.CreateDirectory(SoundsPath);
        }

        foreach (string file in Directory.GetFiles(SoundsPath, "*", SearchOption.AllDirectories))
        {
            string format = Path.GetExtension(file).Substring(1);
            string name = Path.GetFileNameWithoutExtension(file);
            string fullName = $"{name}.{format}";
            Log.LogInfo($"Loading sound file \"{fullName}\"...");

            AudioClip clip = LoadClip(Path.Combine(SoundsPath, file), format.ToLower());
            if (clip != null)
            {
                Log.LogInfo($"Successfully loaded file \"{fullName}\"!");
                clip.name = name;
                replacedClips.Add(name, clip);
            }
            else
            {
                Log.LogWarning($"Failed to load file \"{fullName}\"!");
            }
        }

        string keys = string.Join(", ", replacedClips.Keys.Select(key => $"\"{key}\""));
        if (replacedClips.Count > 0)
        {
            Log.LogInfo($"Found {replacedClips.Count} sound replacements: {keys}");
        }
        else
        {
            Log.LogInfo("No sound replacements found!");
        }

        foreach (var kvp in replacedClips)
        {
            if (kvp.Value == null)
            {
                Log.LogError($"Replaced clip \"{kvp.Key}\" is null!");
            }
            else
            {
                Log.LogInfo($"Replaced clip \"{kvp.Key}\" with \"{kvp.Value.name}\"");
            }
        }

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
        //[HarmonyPatch(nameof(AudioSource.PlayOneShotHelper), [typeof(AudioSource), typeof(AudioClip), typeof(float)])]
        //[HarmonyPrefix]
        //public static void PlayOneShotHelper_Patch(AudioSource source, ref AudioClip clip, float volumeScale) => clip = ReplaceClip(clip, source);
        public static void ReplaceClip(AudioSource source)
        {
            if (source == null || source.clip == null)
            {
                return;
            }

            bool replaced = replacedClips.ContainsKey(source.clip.name);
            Log.LogDebug($"Audio \"{source.clip.name}\", source \"{source.name}\", replaced: {replaced}");

            if (replaced)
            {
                foreach (var kvp in replacedClips)
                {
                    if (kvp.Value == null)
                    {
                        Log.LogError($"Replaced clip \"{kvp.Key}\" is null!");
                    }
                    else
                    {
                        Log.LogInfo($"Replaced clip \"{kvp.Key}\" with \"{kvp.Value.name}\"");
                    }
                }

                AudioClip newAudio = replacedClips[source.clip.name];
                if (newAudio == null)
                {
                    Log.LogError($"Replaced clip \"{source.clip.name}\" is null!");
                    return;
                }
                source.clip = newAudio;
                newAudio.name = source.clip.name;
                try
                {
                    Log.LogWarning($"Replacing clip \"{source.clip.name}\" with \"{newAudio.name}\"");
                    Log.LogDebug($"Clip length: {newAudio.length}, channels: {newAudio.channels}, frequency: {newAudio.frequency}, Load State: {newAudio.loadState}");
                }
                catch (Exception err)
                {
                    Log.LogError($"Caught error while replacing clip: \n{err.Message}, {err.StackTrace}");
                }
            }
        }
    }

    public static AudioClip LoadClip(string path, string format)
    {
        AudioType type = AudioType.UNKNOWN;
        switch (format)
        {
            case "ogg":
                type = AudioType.OGGVORBIS;
                break;
            case "wav":
                type = AudioType.WAV;
                break;
            case "aif":
            case "aiff":
                type = AudioType.AIFF;
                break;
            case "acc":
                type = AudioType.ACC;
                break;
            case "mp2":
            case "mp3":
                type = AudioType.MPEG;
                break;
        }

        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, type);
        try
        {
            www.SendWebRequest();

            while (!www.isDone) { } // Wait

            if (www.result == UnityWebRequest.Result.Success)
            {
                return DownloadHandlerAudioClip.GetContent(www);
            }
            else
            {
                Log.LogWarning($"Failed to load clip: {www.error}");
            }
        }
        catch (Exception err)
        {
            Log.LogError($"Caught error while loading clip: {err.Message}, {err.StackTrace}");
        }
        finally
        {
            www.Dispose();
        }

        return null;
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
