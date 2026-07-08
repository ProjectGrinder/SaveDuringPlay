using UnityEngine;
using UnityEngine.Serialization;

namespace SaveDuringPlay
{
    [DisallowMultipleComponent]
    public sealed class SaveDuringPlayMarker : MonoBehaviour
#if UNITY_EDITOR
        , ISerializationCallbackReceiver
#endif
    {
#if UNITY_EDITOR
        [FormerlySerializedAs("m_Guid")] [SerializeField] [HideInInspector] private string mGuid;
        public string Guid => mGuid;
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (string.IsNullOrEmpty(mGuid))
            {
                mGuid = System.Guid.NewGuid().ToString();
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() { }
#endif
    }
}