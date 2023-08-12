using UnityEngine;
using UnityEditor;
namespace Kurisu.VirtualHuman.Editor
{
    [CustomEditor(typeof(OobaboogaController))]
    public class OobaboogaControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var controller = target as OobaboogaController;
            if (GUILayout.Button("Generate Memory"))
            {
                controller.GenerateMemory();
            }
            GUI.enabled = Application.isPlaying;
            if (GUILayout.Button("Init Memory"))
            {
                controller.InitMemory();
            }
            GUI.enabled = true;
        }
    }
}
