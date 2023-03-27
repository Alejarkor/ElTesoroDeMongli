using UnityEditor;
using UnityEngine;

public class BuildTool
{
    private const string ConfigPath = "Assets/Resources/config.json";

    [MenuItem("Build/Build as Server")]
    public static void BuildAsServer()
    {
        SetIsServer(true);
        BuildPlayerOptions buildPlayerOptions = GetDefaultBuildOptions();
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }

    [MenuItem("Build/Build as Client")]
    public static void BuildAsClient()
    {
        SetIsServer(false);
        BuildPlayerOptions buildPlayerOptions = GetDefaultBuildOptions();
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }

    private static void SetIsServer(bool isServer)
    {
        string configJson = System.IO.File.ReadAllText(ConfigPath);
        MirrorConfigLoader.NetworkConfig config = JsonUtility.FromJson<MirrorConfigLoader.NetworkConfig>(configJson);
        config.isServer = isServer;
        string newConfigJson = JsonUtility.ToJson(config, true);
        System.IO.File.WriteAllText(ConfigPath, newConfigJson);
    }

    private static BuildPlayerOptions GetDefaultBuildOptions()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/YourScene.unity" }; // Reemplaza "YourScene" con el nombre de tu escena
        buildPlayerOptions.locationPathName = "Build";
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.None;
        return buildPlayerOptions;
    }
}
