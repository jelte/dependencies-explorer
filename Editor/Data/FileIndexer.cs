using System.IO;
using UnityEditor;
using UnityEngine;

namespace DependenciesExplorer.Editor.Data
{
    public class FileIndexer
    {
        [MenuItem("Tools/FileIndexer")]
        public static void Index()
        {
            var paths = AssetDatabase.GetAllAssetPaths();
            foreach (var path in paths)
                Index(path);
        }

        private static void Index(string path)
        {
            var type = AssetDatabase.GetMainAssetTypeAtPath(path);
            var dependencies =  AssetDatabase.GetDependencies(path);
            
            Debug.Log($"{path} - {type} - {string.Join(",", dependencies)}");

        }
    }
}