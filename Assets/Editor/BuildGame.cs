using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildGame
{
    [MenuItem("Atlas/Build Playable Scene")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)); camera.tag="MainCamera";
        camera.GetComponent<Camera>().orthographic=true; camera.transform.position=new Vector3(0,0,-10);
        new GameObject("Game", typeof(ExpeditionGame));
        const string path="Assets/Scenes/Expedition.unity";
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene,path);
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(path,true)};
        PlayerSettings.productName="Atlas of the Seventh Sun";
        PlayerSettings.companyName="M2Dev Studio";
        PlayerSettings.defaultScreenWidth=1440;PlayerSettings.defaultScreenHeight=900;
        PlayerSettings.runInBackground=true;
        AssetDatabase.SaveAssets();
        Debug.Log("ATLAS_BUILD_SCENE_OK");
    }

    public static void BuildWeb()
    {
        Build();
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.stripEngineCode = true;
        PlayerSettings.SetManagedStrippingLevel(UnityEditor.Build.NamedBuildTarget.WebGL, ManagedStrippingLevel.High);
        AssetDatabase.SaveAssets();
        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Expedition.unity" },
            locationPathName = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "AtlasOfTheSeventhSun", "WebGL"),
            target = BuildTarget.WebGL,
            options = BuildOptions.CleanBuildCache
        };
        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
            throw new System.Exception("WebGL build failed: " + report.summary.result);
        Debug.Log($"ATLAS_WEBGL_OK size={report.summary.totalSize}");
    }
}
