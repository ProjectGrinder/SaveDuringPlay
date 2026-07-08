using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                case PlayModeStateChange.ExitingEditMode:
                case PlayModeStateChange.EnteredPlayMode:
                default:
                    /* Ignore */
                    break;
            }
        }

        private static void CacheRuntimeStates()
        {
            SerializedDataCache.Clear();

            var targetTypes = GetTypesWithSaveAttributeSafe();
            if (targetTypes.Count == 0) return;

            var targets = targetTypes
                .Select(type => Object.FindObjectsByType(type, FindObjectsInactive.Include))
                .SelectMany(objects => objects)
                .OfType<MonoBehaviour>();

            foreach (var mb in targets)
            {
                if (mb == null || mb.gameObject == null) continue;

                var marker = mb.gameObject.GetComponent<SaveDuringPlayMarker>();
                if (marker == null)
                {
                    marker = mb.gameObject.AddComponent<SaveDuringPlayMarker>();
                    marker.hideFlags = HideFlags.HideInInspector;
                }

                if (string.IsNullOrEmpty(marker.Guid)) continue;

                var key = $"{marker.Guid}_{mb.GetType().FullName}";
                var json = EditorJsonUtility.ToJson(mb);
                SerializedDataCache[key] = json;
            }
        }

        private static void RestoreEditorStates()
        {
            if (SerializedDataCache.Count == 0) return;

            // Edit mode instances will already have the markers attached because they were serialized!
            var markers = Object.FindObjectsByType<SaveDuringPlayMarker>(FindObjectsInactive.Include);

            foreach (var marker in markers)
            {
                if (string.IsNullOrEmpty(marker.Guid)) continue;

                var components = marker.GetComponents<MonoBehaviour>();
                foreach (var comp in components)
                {
                    if (comp == null || comp == marker) continue;

                    var key = $"{marker.Guid}_{comp.GetType().FullName}";

                    if (!SerializedDataCache.TryGetValue(key, out var cachedJson)) continue;
                    Undo.RecordObject(comp, "Restore SaveDuringPlay Changes");
                    EditorJsonUtility.FromJsonOverwrite(cachedJson, comp);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(comp);
                }
            }

            SerializedDataCache.Clear();
        }

        private static List<Type> GetTypesWithSaveAttributeSafe()
        {
            var matchedTypes = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var types = assembly.GetTypes();
                    matchedTypes.AddRange(types.Where(t => t.GetCustomAttribute<SaveDuringPlayAttribute>() != null));
                }
                catch
                {
                    // ignored
                }
            }
            return matchedTypes;
        }
    }
}