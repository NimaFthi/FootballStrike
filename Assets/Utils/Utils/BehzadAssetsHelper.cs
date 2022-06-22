#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace _Scripts.Utils
{
    public static class BehzadAssetsHelper
    {
        public static List<T> GetPrefabsOfType<T>()
        {
            var results = new List<T>();
            var guids = AssetDatabase.FindAssets("t:prefab", new string[] {"Assets"});
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var item = go.GetComponent<T>();
                if (item != null)
                    results.Add(item);
            }
            return results;
        }
    }
}
#endif