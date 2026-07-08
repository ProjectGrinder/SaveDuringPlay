using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Takayama.SaveDuringPlay.Editor
{
    [InitializeOnLoad]
    internal static class PlayModeSaver
    {
        private static readonly Dictionary<string, string> SerializedDataCache = new();
        static PlayModeSaver()
        {
            EditorApplication.playModeStateChanged += OnStateChanged;
        }
        private static void OnStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingPlayMode:
                    CacheRuntimeStates();
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    RestoreEditorStates();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private static void CacheRuntimeStates()
        {
            SerializedDataCache.Clear();

            var markers = Object.FindObjectsByType<SaveDuringPlayMarker>(FindObjectsInactive.Include);

            foreach (var marker in markers)
            {
                if (string.IsNullOrEmpty(marker.Guid)) continue;
                var components = marker.GetComponents<MonoBehaviour>();
                foreach (var comp in components)
                {
                    if (comp == null || comp == marker) continue;

                    // Key pattern: "GUID_ComponentTypeName"
                    string key = $"{marker.Guid}_{comp.GetType().FullName}";
                    string json = EditorJsonUtility.ToJson(comp);
                    SerializedDataCache[key] = json;
                }
            }
        }

        private static void RestoreEditorStates()
        {
            if (SerializedDataCache.Count == 0) return;

            var markers = Object.FindObjectsByType<SaveDuringPlayMarker>(FindObjectsInactive.Include);

            foreach (var marker in markers)
            {
                if (string.IsNullOrEmpty(marker.Guid)) continue;

                var components = marker.GetComponents<MonoBehaviour>();
                foreach (var comp in components)
                {
                    if (comp == null || comp == marker) continue;

                    string key = $"{marker.Guid}_{comp.GetType().FullName}";

                    if (SerializedDataCache.TryGetValue(key, out var cachedJson))
                    {
                        Undo.RecordObject(comp, "Restore SaveDuringPlay Changes");
                        EditorJsonUtility.FromJsonOverwrite(cachedJson, comp);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(comp);
                    }
                }
            }

            SerializedDataCache.Clear();
        }
    }
}
