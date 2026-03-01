using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace ObenRadio
{
    internal static class AudioLoader
    {
        static readonly FieldInfo soundsField = AccessTools.Field(typeof(Radio), "sounds");
        static readonly FieldInfo statesField = AccessTools.Field(typeof(Radio), "states");
        internal static AudioClip[] clips;
        static string loadedPath;

        public static void Load(Radio radio)
        {
            Plugin.Instance.Config.Reload();
            var dir = new DirectoryInfo(Plugin.AudioFolder.Value);

            if (dir.FullName != loadedPath)
                UnloadClips(radio);

            if (clips == null)
                Plugin.Instance.StartCoroutine(LoadFiles(radio, dir));
            else
                SetClips(radio);
        }

        static void SetClips(Radio radio)
        {
            var sounds = soundsField.GetValue(radio) as RandomAudio;

            if (clips != null && clips.Length > 0 && sounds.audioClips != clips)
            {
                sounds.audioClips = clips;
                radio.CurrentState = 0;
                statesField.SetValue(radio, clips.Length + 1);
            }

            radio.Use();
        }

        static void UnloadClips(Radio radio)
        {
            if (AudioLoader.clips == null)
                return;

            radio.SetState(0);

            foreach (var clip in AudioLoader.clips)
                clip.UnloadAudioData();

            AudioLoader.clips = null;
        }

        static IEnumerator LoadFiles(Radio radio, DirectoryInfo dir)
        {
            IEnumerable<FileInfo> files = dir.Exists ? dir.EnumerateFiles("*.*", SearchOption.AllDirectories) : null;

            if (files == null || loadedPath == dir.FullName)
            {
                radio.Use();
                yield break;
            }

            loadedPath = dir.FullName;

            List<AudioClip> clips = [];

            if (radio.CurrentState != 0)
                radio.SetState(0);

            foreach (FileInfo file in files.OrderBy(x => x.Name))
            {
                AudioType audioType = file.Extension switch
                {
                    ".acc" => AudioType.ACC,
                    ".mp3" => AudioType.MPEG,
                    ".ogg" => AudioType.OGGVORBIS,
                    ".wav" => AudioType.WAV,
                    _ => AudioType.UNKNOWN
                };

                if (audioType == AudioType.UNKNOWN)
                    continue;

                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(file.FullName, audioType))
                {
                    var downloadHandler = new DownloadHandlerAudioClip(file.FullName, AudioType.MPEG);
                    downloadHandler.streamAudio = true;
                    www.downloadHandler = downloadHandler;

                    yield return www.SendWebRequest();

                    if (!www.isNetworkError && !www.isHttpError)
                    {
                        AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                        clip.name = file.Name;
                        clips.Add(clip);
                    }
                }
            }

            AudioLoader.clips = clips.ToArray();
            yield return new WaitForSeconds(0.5f);
            SetClips(radio);
        }
    }
}
