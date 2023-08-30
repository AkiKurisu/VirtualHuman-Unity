using UnityEditor;
using UnityEngine;
namespace Kurisu.VirtualHuman.Editor
{
    [CustomEditor(typeof(GPTTransport))]
    public class GPTTransportEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Default URL", GUILayout.MinHeight(25)))
            {
                (target as GPTTransport).API_URL = GPTTransport.OpenAI_API_URL;
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
