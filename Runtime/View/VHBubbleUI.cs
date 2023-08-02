using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
namespace Kurisu.VirtualHuman
{
    public class BubblePool
    {
        private readonly ChatBubble bubblePrefab;
        private readonly int maxCount;
        private Queue<ChatBubble> bubbleQueue = new();
        public BubblePool(ChatBubble bubblePrefab, int maxCount)
        {
            this.bubblePrefab = bubblePrefab;
            this.maxCount = maxCount;
        }
        public ChatBubble GetBubble()
        {
            ChatBubble bubble;
            if (bubbleQueue.Count < maxCount)
            {
                bubble = GameObject.Instantiate(bubblePrefab);
                return bubble;
            }
            else
            {
                bubble = bubbleQueue.Dequeue();
            }
            bubbleQueue.Enqueue(bubble);
            bubble.gameObject.SetActive(true);
            return bubble;
        }
    }
    public class VHBubbleUI : MonoBehaviour
    {
        [SerializeField]
        private RectTransform bubbleContent;
        [SerializeField]
        private TMP_InputField inputField;
        [SerializeField]
        private Button sendButton;
        [SerializeField]
        private ChatBubble userBubblePrefab;
        [SerializeField]
        private ChatBubble aiBubblePrefab;
        [SerializeField]
        private string userName = "You";
        [SerializeField]
        private string aiName = "Bot";
        [SerializeField]
        private AudioSource audioSource;
        private VHController controller;
        private BubblePool aiBubblePool;
        private BubblePool userBubblePool;
        private void Start()
        {
            aiBubblePool = new BubblePool(aiBubblePrefab, 10);
            userBubblePool = new BubblePool(userBubblePrefab, 10);
            controller = VHController.Instance;
            inputField.onValueChanged.AddListener(x => controller.TextToSend = x);
            sendButton.onClick.AddListener(StartWaitTalk);
            controller.OnResponse += ResponseHandler;
            controller.OnFail += FailHandler;
        }
        private void OnDestroy()
        {
            inputField.onValueChanged.RemoveAllListeners();
            sendButton.onClick.RemoveAllListeners();
            controller.OnResponse -= ResponseHandler;
            controller.OnFail -= FailHandler;
        }
        private void AddBubble(bool isUser, string text)
        {
            var bubble = (isUser ? userBubblePool : aiBubblePool).GetBubble();
            bubble.SetUp(isUser ? userName : aiName, text);
            bubble.transform.SetParent(bubbleContent);
            bubble.transform.SetAsLastSibling();
            LayoutRebuilder.ForceRebuildLayoutImmediate(bubbleContent);
        }
        private void StartWaitTalk()
        {
            inputField.interactable = false;
            sendButton.interactable = false;
            AddBubble(true, controller.TextToSend);
            controller.SendAsync();
        }
        private void FailHandler(string failMessage)
        {
            inputField.interactable = true;
            sendButton.interactable = true;
        }
        private void ResponseHandler(AudioClip clip, string response)
        {
            inputField.interactable = true;
            sendButton.interactable = true;
            AddBubble(false, response);
            if (clip == null) return;
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
