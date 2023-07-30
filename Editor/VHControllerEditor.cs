using UnityEngine;
using UnityEditor;
namespace Kurisu.VirtualHuman.Editor
{
    [CustomEditor(typeof(VHController))]
    public class VHControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var controller = target as VHController;
            GUI.enabled = Application.isPlaying;
            var orgColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(140 / 255f, 160 / 255f, 250 / 255f);
            if (GUILayout.Button("Generate Speech", GUILayout.MinHeight(25)))
            {
                controller.SendAsync();
            }
            GUI.backgroundColor = orgColor;
            GUI.enabled = true;
        }
    }
}
