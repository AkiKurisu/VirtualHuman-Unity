using System.Text.RegularExpressions;
using UnityEngine;
using System;
using System.Threading.Tasks;
namespace Kurisu.VirtualHuman
{
    public enum LLMAgentType
    {
        None, GPT, KoboldCPP, Oobabooga, GLM
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
                OnLLMChanged();
            }
        }
        public GPTTransport GPT { get; private set; }
        public KoboldCPPTransport Kobold { get; private set; }
        public OobaboogaTransport Oobabooga { get; private set; }
        public GLMTransport GLM { get; private set; }
        public ILLMDriver LLMDriver { get; private set; }
        public VITSTransport VITS { get; private set; }
        [SerializeField]
        private string llmLanguage = "en";
        public string LLMLanguage { get => llmLanguage; set => llmLanguage = value; }
        [SerializeField]
        private string vitsLanguage = "ja";
        public string VITSLanguage { get => vitsLanguage; set => vitsLanguage = value; }
        [SerializeField]
        private string userLanguage = "zh";
        public string UserLanguage { get => userLanguage; set => userLanguage = value; }
        [SerializeField]
        private bool useVITS = true;
        public bool UseVITS { get => useVITS; set => useVITS = value; }
        [SerializeField, Tooltip("Used to skip reading motion detail, especially used for KoboldAI.")]
        private bool smartReading;
        private const string Pattern = @"\*(.+)\*";
        public event Action<AudioClip, string> OnResponse;
        public event Action<string> OnFail;
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            VITS = GetComponentInChildren<VITSTransport>();
            GPT = GetComponentInChildren<GPTTransport>();
            Kobold = GetComponentInChildren<KoboldCPPTransport>();
            Oobabooga = GetComponentInChildren<OobaboogaTransport>();
            GLM = GetComponentInChildren<GLMTransport>();
            OnLLMChanged();
        }
        private void OnLLMChanged()
        {
            if (llmAgentType == LLMAgentType.GPT)
            {
                LLMDriver = GPT;
                if (LLMDriver == null)
                {
                    Debug.LogError("GPT controller not found !");
                }
            }
            else if (llmAgentType == LLMAgentType.KoboldCPP)
            {
                LLMDriver = Kobold;
                if (LLMDriver == null)
                {
                    Debug.LogError("Kobold controller not found !");
                }
            }
            else if (llmAgentType == LLMAgentType.Oobabooga)
            {
                LLMDriver = Oobabooga;
                if (LLMDriver == null)
                {
                    Debug.LogError("Oobabooga controller not found !");
                }
            }
            else if (llmAgentType == LLMAgentType.GLM)
            {
                LLMDriver = GLM;
                if (LLMDriver == null)
                {
                    Debug.LogError("GLM controller not found !");
                }
            }
        }
        private void OnDestroy()
        {
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
                string currentLanaguage = userLanguage;

                //Translation Process for UI-LLM
                if (currentLanaguage != llmLanguage)
                {
                    sendToVITS = await ProcessTranslation(userLanguage, llmLanguage, sendToVITS);
                }
                var llmResponse = await LLMDriver.ProcessLLM(sendToVITS);
                if (!llmResponse.Status)
                {
                    OnFail?.Invoke("LLM request failed !");
                    return;
                }
                currentLanaguage = llmLanguage;

                //Translation Process for LLM-VITS
                sendToVITS = llmResponse.Response;
                if (currentLanaguage != vitsLanguage)
                {
                    sendToVITS = await ProcessTranslation(llmLanguage, vitsLanguage, sendToVITS);
                }

                //Translation Process for LLM-User Interface
                responseCache = llmResponse.Response;
                if (currentLanaguage != userLanguage)
                {
                    responseCache = await ProcessTranslation(llmLanguage, userLanguage, llmResponse.Response);
                }
            }
            else
            {
                //Translation Process for UI-VITS
                if (userLanguage != vitsLanguage)
                {
                    sendToVITS = await ProcessTranslation(userLanguage, vitsLanguage, sendToVITS);
                }
                responseCache = message;
            }
            if (!useVITS)
            {
                OnResponse?.Invoke(null, responseCache);
                return;
            }
            string vitsReadMessage = sendToVITS;
            if (smartReading)
            {
                vitsReadMessage = Regex.Replace(vitsReadMessage, Pattern, string.Empty);
            }
            //Since VITS run on local which is faster than LLM, process it at last
            SendVITSAsync(vitsReadMessage);
        }
        private static async Task<string> ProcessTranslation(string souceLanguage, string targetLanaguage, string sourceText)
        {
            var translateSendResponse = await GoogleTranslator.TranslateTextAsync(souceLanguage, targetLanaguage, sourceText);
            if (!translateSendResponse.Status)
            {
                Debug.LogWarning("Google request failed, translation is skipped !");
            }
            else
            {
                sourceText = translateSendResponse.TranslateText;
            }
            return sourceText;
        }
        public async void SendVITSAsync(string message)
        {
            string sendToVITS = message;
            //If only contains motion detail, no need to use VITS
            if (string.IsNullOrWhiteSpace(sendToVITS) || string.IsNullOrEmpty(sendToVITS))
            {
                OnResponse?.Invoke(null, responseCache);
                return;
            }
            var vitsResponse = await VITS.SendVITSRequestAsync(sendToVITS);
            if (!vitsResponse.Status)
            {
                OnFail?.Invoke("VITS request failed !");
                return;
            }
            OnResponse?.Invoke(vitsResponse.Result, responseCache);
        }
    }
}
