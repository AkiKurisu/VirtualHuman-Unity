using UnityEngine;
using UnityEditor;
namespace Kurisu.VirtualHuman.Editor
{
    [CustomEditor(typeof(KoboldCPPTransport))]
    public class KoboldTransportEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var controller = target as KoboldCPPTransport;
            if (GUILayout.Button("Generate Memory"))
            {
                controller.GenerateMemory();
            }
            GUI.enabled = Application.isPlaying;
            if (GUILayout.Button("Init Memory"))
            {
                controller.InitMemory();
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Check"))
            {
                controller.Check();
            }
            if (GUILayout.Button("Abort"))
            {
                controller.Abort();
            }
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;
        }
    }
}
