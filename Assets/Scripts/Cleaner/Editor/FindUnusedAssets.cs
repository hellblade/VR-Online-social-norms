/**
*asset cleaner
*Copyright (c) 2015 Tatsuhiko Yamamura
*
*This software is released under the MIT License.
*http://opensource.org/licenses/mit-license.php
*/

//============================================
//
//Modified By Bathur Lu
//
//Date:     2019.3.23
//Website:  http://bathur.cn/
//
//============================================

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AssetClean
{
    public class FindUnusedAssets : EditorWindow
    {
        AssetCollector collection = new AssetCollector();
        List<DeleteAsset> deleteAssets = new List<DeleteAsset>();
        Vector2 scroll;
        static bool newData = false;

        [MenuItem("Assets/Delete Unused Assets/Only resource", false, 50)]
        static void InitWithoutCode()
        {
            var window = CreateInstance<FindUnusedAssets>();
            window.collection.useCodeStrip = false;
            window.collection.Collection();
            window.CopyDeleteFileList(window.collection.deleteFileList);

            newData = true;
            window.Show();
        }
        /*
        [MenuItem("Assets/Delete Unused Assets/Unused by editor", false, 51)]
        static void InitWithout()
        {
            var window = CreateInstance<FindUnusedAssets>();
            window.collection.Collection();
            window.CopyDeleteFileList(window.collection.deleteFileList);

            window.Show();
        }

        [MenuItem("Assets/Delete Unused Assets/Unused by game", false, 52)]
        static void Init()
        {
            var window = CreateInstance<FindUnusedAssets>();
            window.collection.saveEditorExtensions = false;
            window.collection.Collection();
            window.CopyDeleteFileList(window.collection.deleteFileList);

            window.Show();
        }*/

        void OnGUI()
        {
            using (var horizonal = new EditorGUILayout.HorizontalScope("box"))
            {
                EditorGUILayout.LabelField("Delete unreference assets from buildsettings and resources");

                if (GUILayout.Button("Delete", GUILayout.Width(120), GUILayout.Height(40)) && deleteAssets.Count != 0)
                {
                    RemoveFiles();
                    Close();
                }
            }

            if (newData)
            {
                var skipFolders = new string[] { "Assets/csc.rsp",
                    "Assets/Imported Assets/Female Duck",
                    "Assets/Imported Assets/Male Duck",
                    "Assets/Lean",
                    "Assets/PhysSound",
                    "Assets/ThirdParty",
                    "Assets/TextMesh Pro",
                    "Assets/Imported Assets/Pet Clutter",
                    "Assets/Data", "Assets/Plugins", "Assets/Scenes", "Assets/SteamVR",
                    "Assets/VRTK", "Assets/Imported Assets/Screen", "Assets/Character Animations",
                    "Assets/Imported Assets/DesktopComputerSet", "Assets/Imported Assets/Office Stuff",
                    "Assets/Imported Assets/Standard Assets", "Assets/Prefabs/computer.fbx", "Assets/StreamingAssets"};
                deleteAssets.RemoveAll(x => skipFolders.Any(skipPath => x.path.StartsWith(skipPath) || x.path.EndsWith(".txt")));

                using (var writer = new StreamWriter("D:\\unusedAssets.csv"))
                {
                    writer.WriteLine("Path, Size");
                    foreach (var asset in deleteAssets)
                    {
                        if (string.IsNullOrEmpty(asset.path))
                        {
                            continue;
                        }

                        var info = new FileInfo(asset.path);

                        writer.WriteLine(asset.path.Replace(',', '-') + "," + (info.Length / 1024));
                    }
                }

                newData = false;
            }

            using (var scrollScope = new EditorGUILayout.ScrollViewScope(scroll))
            {
                scroll = scrollScope.scrollPosition;
                foreach (var asset in deleteAssets)
                {
                    if (string.IsNullOrEmpty(asset.path))
                    {
                        continue;
                    }

                    using (var horizonal = new EditorGUILayout.HorizontalScope())
                    {
                        asset.isDelete = EditorGUILayout.Toggle(asset.isDelete, GUILayout.Width(20));
                        var icon = AssetDatabase.GetCachedIcon(asset.path);
                        GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(20));
                        if (GUILayout.Button(asset.path, EditorStyles.largeLabel))
                        {
                            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(asset.path);
                        }
                    }
                }
            }

        }

        static void CleanDir()
        {
            RemoveEmptyDirectry("Assets");
            AssetDatabase.Refresh();
        }

        void CopyDeleteFileList(IEnumerable<string> deleteFileList)
        {
            foreach (var asset in deleteFileList)
            {
                var filePath = AssetDatabase.GUIDToAssetPath(asset);
                if (string.IsNullOrEmpty(filePath) == false)
                {
                    deleteAssets.Add(new DeleteAsset() { path = filePath });
                }
            }
        }

        void RemoveFiles()
        {
            try
            {
                string exportDirectry = "BackupUnusedAssets";
                Directory.CreateDirectory(exportDirectry);
                var files = deleteAssets.Where(item => item.isDelete == true).Select(item => item.path).ToArray();
                string backupPackageName = exportDirectry + "/package" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".unitypackage";
                EditorUtility.DisplayProgressBar("Export package", backupPackageName, 0);
                AssetDatabase.ExportPackage(files, backupPackageName);

                int i = 0;
                int length = deleteAssets.Count;

                foreach (var assetPath in files)
                {
                    i++;
                    EditorUtility.DisplayProgressBar("Delete unused assets", assetPath, (float)i / length);
                    AssetDatabase.DeleteAsset(assetPath);
                }

                EditorUtility.DisplayProgressBar("Clean directory", "", 1);
                foreach (var dir in Directory.GetDirectories("Assets"))
                {
                    RemoveEmptyDirectry(dir);
                }

                System.Diagnostics.Process.Start(exportDirectry);

                AssetDatabase.Refresh();
            }
            catch (System.Exception e)
            {
                Debug.Log(e.Message);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        static void RemoveEmptyDirectry(string path)
        {
            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
            {
                RemoveEmptyDirectry(dir);
            }

            var files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly).Where(item => Path.GetExtension(item) != ".meta");
            if (files.Count() == 0 && Directory.GetDirectories(path).Count() == 0)
            {
                var metaFile = AssetDatabase.GetTextMetaFilePathFromAssetPath(path);
                FileUtil.DeleteFileOrDirectory(path);
                FileUtil.DeleteFileOrDirectory(metaFile);
            }
        }

        class DeleteAsset
        {
            public bool isDelete = true;
            public string path;
        }
    }
}
