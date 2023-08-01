using UnityEngine;
using TMPro;
using UnityEngine.UI;
namespace Kurisu.VirtualHuman
{
    public class ChatBubble : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text character;
        [SerializeField]
        private TMP_Text context;
        private RectTransform rectTransform;
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }
        public void SetUp(string characterName, string text)
        {
            this.character.text = characterName;
            this.context.text = text;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }
    }
}
