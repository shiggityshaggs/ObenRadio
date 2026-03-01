using BepInEx;
using HarmonyLib;
using System;
using System.IO;

namespace ObenRadio;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static Plugin Instance;
    internal static BepInEx.Configuration.ConfigEntry<string> AudioFolder;

    private void Awake()
    {
        Instance = this;
        Console.WriteLine($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(Patches));

        string path = Path.Combine(BepInEx.Paths.PluginPath, MyPluginInfo.PLUGIN_GUID, "Audio");
        AudioFolder = Plugin.Instance.Config.Bind(section: "Config", key: "AudioFolder", defaultValue: path, description: string.Empty);
    }
}
