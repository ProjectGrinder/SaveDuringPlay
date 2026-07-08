using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Takayama.SaveDuringPlay.Editor
{
    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    internal sealed class AttributeInjectorEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
            if (target == null) return;

            var type = target.GetType();
            if (type.GetCustomAttribute<SaveDuringPlayAttribute>() == null) return;
            
            var mb = (MonoBehaviour)target;

            if (mb.gameObject == null || mb.gameObject.GetComponent<SaveDuringPlayMarker>()) return;
            var marker = mb.gameObject.AddComponent<SaveDuringPlayMarker>();
            marker.hideFlags = HideFlags.HideInInspector;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }
}