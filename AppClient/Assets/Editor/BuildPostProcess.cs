
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;


public static class BuildPostProcess
{
    private const string DockerConfig = "dockerconfig";

    //[PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.StandaloneLinux64) return;

        var path = Path.GetFullPath(Path.Combine(Application.dataPath, "../"));
        var dockerPath = Path.GetFullPath(Path.Combine(path, DockerConfig));
        Debug.Log($"Project:{path} Docker:{dockerPath} build:{pathToBuiltProject}");

        var buildPath = Path.GetFullPath(Path.Combine(pathToBuiltProject, "../"));

        CopyAllFile(dockerPath, buildPath);

    }

    private static void CopyAllFile(string from, string to)
    {
        var files = Directory.GetFiles(from);
        foreach (var file in files)
        {
            var dest = Path.GetFullPath(Path.Combine(to, Path.GetFileName(file)));
            FileUtil.CopyFileOrDirectory(file, dest);
            Debug.Log($"From:{from} -> {dest}");
        }
    }

#if UNITY_IOS
    
    [PostProcessBuild(1)]
    public static void OniOSPostprocessBuild(BuildTarget target, string pathToBuildProject)
    {
        if (target != BuildTarget.iOS) return;
        var pbxProject = new UnityEditor.iOS.Xcode.PBXProject();
        var proFile = Path.Combine(pathToBuildProject, "Unity-iPhone.xcodeproj/project.pbxproj");
        pbxProject.ReadFromFile(proFile);
        var targetGuid = pbxProject.GetUnityFrameworkTargetGuid();
        pbxProject.AddFrameworkToProject(targetGuid, "libz.tbd", true);
        pbxProject.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
        pbxProject.WriteToFile(proFile);
    }
#endif
}
