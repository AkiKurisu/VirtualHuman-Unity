using System.Text.RegularExpressions;
using UnityEngine;
using System;
namespace Kurisu.VirtualHuman
{
    public enum LLMAgentType
    {
        None, GPT, Kobold
    }
    public class VHController : MonoBehaviour
    {
        private static VHController instance;
        public static VHController Instance => instance;
        [SerializeField, TextArea(5, 50)]
        private string textToSend = "";
        [SerializeField, TextArea(5, 50)]
        private string responseCache;
        public string TextToSend { get => textToSend; set => textToSend = value; }
        [SerializeField]
        private LLMAgentType llmAgentType;
        public LLMAgentType LLMType
        {
            get => llmAgentType;
            set
            {
                if (llmAgentType == value) return;
                llmAgentType = value;
                if (llmAgentType == LLMAgentType.GPT)
                {
                    LLMDriver = GPT;
                    if (LLMDriver == null)
                    {
                        Debug.LogError("GPT controller not found !");
                    }
                }
                else if (llmAgentType == LLMAgentType.Kobold)
                {
                    LLMDriver = Kobold;
                    if (LLMDriver == null)
                    {
                        Debug.LogError("GPKoboldT controller not found !");
                    }
                }
            }
        }
        [SerializeField]
        private bool translateUI2LLM;
        [SerializeField]
        private bool translateLLM2VITS;
        [SerializeField]
        private bool translateLLM2UI;
        public GPTController GPT { get; private set; }
        public KoboldController Kobold { get; private set; }
        public ILLMDriver LLMDriver { get; private set; }
        public VITSController VITS { get; private set; }
        [SerializeField]
        private string llmLanguage = "en";
        [SerializeField]
        private string vitsLanguage = "ja";
        [SerializeField]
        private string userLanguage = "zh";
        [SerializeField, Tooltip("Used to skip speech motion detail, especially used for KoboldAI.")]
        private bool smartReading;
        private const string Pattern = @"\*(.+)\*";
        public event Action<AudioClip, string> OnResponse;
        public event Action<string> OnFail;
        private VITSHandle handleCache;
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            VITS = GetComponentInChildren<VITSController>();
            GPT = GetComponentInChildren<GPTController>();
            Kobold = GetComponentInChildren<KoboldController>();
            if (llmAgentType == LLMAgentType.GPT)
            {
                LLMDriver = GPT;
                if (LLMDriver == null)
                {
                    Debug.LogError("GPT controller not found !");
                }
            }
            else if (llmAgentType == LLMAgentType.Kobold)
            {
                LLMDriver = Kobold;
                if (LLMDriver == null)
                {
                    Debug.LogError("GPKoboldT controller not found !");
                }
            }
        }
        private void OnDestroy()
        {
            handleCache.Release();
            if (instance != null && instance == this) instance = null;
        }
        public void SendAsync()
        {
            SendAsync(textToSend);
        }
        public async void SendAsync(string message)
        {
            string sendToVITS = message;
            //Process LLM
            if (llmAgentType != LLMAgentType.None)
            {
                //Translation Process for UI-LLM
                if (translateUI2LLM)
                {
                    var translateSendResponse = await GoogleTranslator.TranslateTextAsync(userLanguage, llmLanguage, sendToVITS);
                    if (!translateSendResponse.Status)
                    {
                        Debug.LogWarning("Google translate request failed, translation is skipped !");
                    }
                    else
                    {
                        sendToVITS = translateSendResponse.TranslateText;
                    }
                }
                var llmResponse = await LLMDriver.ProcessLLM(sendToVITS);
                if (!llmResponse.Status)
                {
                    OnFail?.Invoke("LLM request failed !");
                    return;
                }

                //Translation Process for LLM-VITS
                sendToVITS = llmResponse.Response;
                if (translateLLM2VITS)
                {
                    var translateSendResponse = await GoogleTranslator.TranslateTextAsync(llmLanguage, vitsLanguage, sendToVITS);
                    if (!translateSendResponse.Status)
                    {
                        Debug.LogWarning("Google translate request failed, translation is skipped !");
                    }
                    else
                    {
                        sendToVITS = translateSendResponse.TranslateText;
                    }
                }

                //Translation Process for LLM-User Interface
                responseCache = sendToVITS;
                if (translateLLM2UI)
                {
                    var translateReceiveResponse = await GoogleTranslator.TranslateTextAsync(llmLanguage, userLanguage, llmResponse.Response);
                    if (!translateReceiveResponse.Status)
                    {
                        Debug.LogWarning("Google translate request failed, translation is skipped !");
                    }
                    else
                    {
                        responseCache = translateReceiveResponse.TranslateText;
                    }
                }
            }
            else
            {
                //Translation Process for UI-VITS
                if (translateUI2LLM && translateLLM2VITS)
                {
                    var translateSendResponse = await GoogleTranslator.TranslateTextAsync(userLanguage, vitsLanguage, sendToVITS);
                    if (!translateSendResponse.Status)
                    {
                        Debug.LogWarning("Google translate request failed, translation is skipped !");
                    }
                    else
                    {
                        sendToVITS = translateSendResponse.TranslateText;
                    }
                }
                responseCache = message;
            }
            string vitsReadMessage = sendToVITS;
            if (smartReading)
            {
                vitsReadMessage = Regex.Replace(vitsReadMessage, Pattern, string.Empty);
            }
            //Since VITS run on local which is faster than LLM, process it at last
            SendVITSAsync(vitsReadMessage);
        }
        public async void SendVITSAsync(string message)
        {
            string sendToVITS = message;
            var vitsResponse = await VITS.SendVITSRequestAsync(sendToVITS);
            handleCache.Release();
            handleCache = vitsResponse;
            if (!vitsResponse.Status)
            {
                OnFail?.Invoke("VITS request failed !");
                return;
            }
            OnResponse?.Invoke(vitsResponse.Result, responseCache);
        }
    }
}
