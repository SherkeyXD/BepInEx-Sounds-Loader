using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace MiSideSoundsLoader;

public class AudioFactory
{
    private static AudioFactory _instance;
    public static AudioFactory Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new AudioFactory();
            }
            return _instance;
        }
    }

    private Dictionary<string, AudioClip> replacedClips = [];

    public void LoadAllClips(string folderPath)
    {
        string[] files = Directory.GetFiles(folderPath);
        foreach (string file in files)
        {
            AddClip(file);
        }
    }

    public void AddClip(string filepath)
    {
        string name = Path.GetFileNameWithoutExtension(filepath);
        string format = Path.GetExtension(filepath).Substring(1);
        Plugin.Log.LogInfo($"Loading clip: {name}.{format}");
        AudioClip clip = LoadClip(filepath, format.ToLower());
        if (clip != null)
        {
            Plugin.Log.LogInfo("Successfully loaded.");
            replacedClips[name] = clip;
        }
    }

    public AudioClip LoadClip(string path, string format)
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
                Plugin.Log.LogWarning($"Failed to load clip: {www.error}");
            }
        }
        catch (Exception err)
        {
            Plugin.Log.LogError(
                $"Caught error while loading clip: {err.Message}, {err.StackTrace}"
            );
        }
        finally
        {
            www.Dispose();
        }

        return null;
    }

    public AudioClip GetClip(string name)
    {
        if (replacedClips.TryGetValue(name, out AudioClip clip))
        {
            if (clip != null)
            {
                return clip;
            }
            else
            {
                Plugin.Log.LogWarning($"Clip \"{name}\" is null.");
            }
        }
        return null;
    }

    public void CheckClips()
    {
        foreach (var pair in replacedClips)
        {
            if (pair.Value == null)
            {
                Plugin.Log.LogWarning($"Clip \"{pair.Key}\" is null.");
            }
        }
    }

    public List<string> GetAllClipNames()
    {
        return new List<string>(replacedClips.Keys);
    }
}
