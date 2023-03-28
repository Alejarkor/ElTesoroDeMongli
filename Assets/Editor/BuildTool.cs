using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.Reporting;

public class BuildTool : EditorWindow
{
    private class SceneInfo
    {
        public string sceneName;
        public bool isSelected;

        public SceneInfo(string sceneName, bool isSelected)
        {
            this.sceneName = sceneName;
            this.isSelected = isSelected;
        }
    }

    private List<SceneInfo> _scenes = new List<SceneInfo>();
    private ReorderableList _reorderableList;
    private bool _isServerBuild;

    [MenuItem("Tools/Build Tool")]
    public static void ShowWindow()
    {
        GetWindow<BuildTool>("Build Tool");
    }

    private void OnEnable()
    {
        _reorderableList = new ReorderableList(_scenes, typeof(SceneInfo), true, true, false, false)
        {
            drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Scenes"),
            drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                SceneInfo scene = _scenes[index];
                rect.y += 2;
                rect.height = EditorGUIUtility.singleLineHeight;
                scene.isSelected = EditorGUI.Toggle(new Rect(rect.x, rect.y, 20, rect.height), scene.isSelected);
                scene.sceneName = EditorGUI.TextField(new Rect(rect.x + 20, rect.y, rect.width - 20, rect.height), scene.sceneName);
            }
        };
    }

    private void OnGUI()
    {
        GUILayout.Label("Build Settings", EditorStyles.boldLabel);

        _isServerBuild = EditorGUILayout.Toggle("Server Build", _isServerBuild);

        _reorderableList.DoLayoutList();

        if (GUILayout.Button("Add Scene"))
        {
            _scenes.Add(new SceneInfo("", false));
        }

        if (GUILayout.Button("Build"))
        {
            Build();
        }
    }

    private void Build()
    {
        string[] selectedScenes = _scenes.Where(scene => scene.isSelected).Select(scene => scene.sceneName).ToArray();

        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = selectedScenes,
            target = _isServerBuild ? BuildTarget.StandaloneWindows : BuildTarget.WebGL,
            options = BuildOptions.None
        };

        if (_isServerBuild)
        {
            buildOptions.locationPathName = "Build/Server/MyServer.exe";
        }
        else
        {
            buildOptions.locationPathName = "Build/Client";
        }

        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError("Build failed");
        }
    }
}
