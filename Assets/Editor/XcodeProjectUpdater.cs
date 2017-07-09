using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public static class XcodeProjectUpdater  {
	[PostProcessBuild]
	public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
	{

        if (buildTarget == BuildTarget.iOS)
		{
			string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";

			PBXProject proj = new PBXProject();
			proj.ReadFromString(File.ReadAllText(projPath));

			string target = proj.TargetGuidByName("Unity-iPhone");

			// Add user packages to project. Most other source or resource files and packages 
			// can be added the same way.
			//CopyAndReplaceDirectory("NativeAssets/TestLib.bundle", Path.Combine(path, "Frameworks/TestLib.bundle"));
			//proj.AddFileToBuild(target, proj.AddFile("Frameworks/TestLib.bundle",
			//										 "Frameworks/TestLib.bundle", PBXSourceTree.Source));

			//CopyAndReplaceDirectory("NativeAssets/TestLib.framework", Path.Combine(path, "Frameworks/TestLib.framework"));
			//proj.AddFileToBuild(target, proj.AddFile("Frameworks/TestLib.framework",
			//										 "Frameworks/TestLib.framework", PBXSourceTree.Source));

			//// Add custom system frameworks. Duplicate frameworks are ignored.
			//// needed by our native plugin in Assets/Plugins/iOS
			//proj.AddFrameworkToProject(target, "AssetsLibrary.framework", false /*not weak*/);

			//// Add our framework directory to the framework include path
			//proj.SetBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", "$(inherited)");
			//proj.AddBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", "$(PROJECT_DIR)/Frameworks");

			// Set a custom link flag
			proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-ObjC");

			File.WriteAllText(projPath, proj.WriteToString());
		}
	}
}
