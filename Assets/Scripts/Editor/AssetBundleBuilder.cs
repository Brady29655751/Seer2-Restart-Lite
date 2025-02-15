﻿using System;
using System.Linq;

namespace U_Editor
{
    using UnityEditor;
    using UnityEngine;
    using System.IO;

    public class AssetBundleBuilder//打包精灵动画专用
    {
        /*
        [MenuItem("Tools/Check Pet Exp System")]
        public static void CheckPetExpSystem() 
        {
            Debug.Log(PetExpSystem.GetTotalExp(0, 1));  
            Debug.Log(PetExpSystem.GetTotalExp(1, 1));  
            Debug.Log(PetExpSystem.GetTotalExp(2, 1));  
        }
        */

        [MenuItem("Tools/Check Active Build Target")]
        public static void CheckActiveBuildTarget() 
        {
            Debug.Log(EditorUserBuildSettings.activeBuildTarget);
        }

        [MenuItem("Tools/Build PetAnim AssetBundles")]
        public static void BuildAllAssetBundles()
        {
            string assetBundleDirectory = "Assets/AssetBundles";
            if (!Directory.Exists(assetBundleDirectory))
            {
                Directory.CreateDirectory(assetBundleDirectory);
            }

            string parentFolder = "Assets/PetAnimation"; // Change this to your parent folder path
            DirectoryInfo dir = new DirectoryInfo(parentFolder);
            DirectoryInfo[] subDirs = dir.GetDirectories();

            foreach (var subDir in subDirs)
            {
                string bundleName = "PFA_" + subDir.Name;
                FileInfo[] files = subDir.GetFiles("*.prefab", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    string assetPath = file.FullName.Substring(file.FullName.IndexOf("Assets", StringComparison.Ordinal)).Replace("\\", "/");
                    // Debug.Log(Application.dataPath);
                    // Debug.Log(assetPath);
                    AssetImporter.GetAtPath(assetPath).assetBundleName = bundleName;
                }
            }

            BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
        }
    }
}