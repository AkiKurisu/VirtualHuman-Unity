using UnityEngine;
using TMPro;
using UnityEngine.UI;
namespace Kurisu.VirtualHuman
{
    public class VHUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text outputText;
        [SerializeField]
        private TMP_InputField inputField;
        [SerializeField]
        private Button sendButton;
        [SerializeField]
        private AudioSource audioSource;
        private VHController controller;
        private void Start()
        {
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
        private void StartWaitTalk()
        {
            inputField.interactable = false;
            sendButton.interactable = false;
            controller.SendAsync();
        }
        private void FailHandler(string failMessage)
        {
            inputField.interactable = true;
            sendButton.interactable = true;
            outputText.text = failMessage;
        }
        private void ResponseHandler(AudioClip clip, string response)
        {
            inputField.interactable = true;
            sendButton.interactable = true;
            outputText.text = response;
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
