using UnityEngine;
using UnityEditor;
namespace Kurisu.VirtualHuman.Editor
{
    [CustomEditor(typeof(VITSController))]
    public class VITSControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var controller = target as VITSController;
            var audioClipProperty = serializedObject.FindProperty("audioClipCache");
            var clip = audioClipProperty.objectReferenceValue as AudioClip;
            EditorGUILayout.BeginVertical();
            if (clip != null)
            {
                GUILayout.Label($"Audio Clip Cached : {clip.name}");
                GUILayout.Label($"Length : {clip.length}");
            }
            else
            {
                GUILayout.Label($"No Audio Clip Cached");
            }
            GUILayout.EndVertical();
            GUI.enabled = clip != null;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Preview", GUILayout.MinHeight(25)))
            {
                AudioUtil.PlayClip(clip);
            }
            if (GUILayout.Button("Stop", GUILayout.MinHeight(25)))
            {
                AudioUtil.StopClip(clip);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            var orgColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(140 / 255f, 160 / 255f, 250 / 255f);
            if (GUILayout.Button("Save Audio", GUILayout.MinHeight(25)))
            {
                Save(audioClipProperty, clip);
            }
            GUI.backgroundColor = new Color(253 / 255f, 163 / 255f, 255 / 255f);
            if (GUILayout.Button("Clear", GUILayout.MinHeight(25)))
            {
                audioClipProperty.objectReferenceValue = null;
                audioClipProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
            GUI.backgroundColor = orgColor;
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }
        private void Save(SerializedProperty audioClipProperty, AudioClip audioClip)
        {
            string path = EditorUtility.OpenFolderPanel("Select save path", Application.dataPath, "");
            if (string.IsNullOrEmpty(path)) return;
            string outPutPath = $"{path}/{audioClip.name}";
            WavUtil.Save(outPutPath, audioClip);
            Debug.Log($"Audio saved succeed! Audio path:{outPutPath}");
        }
    }

}
