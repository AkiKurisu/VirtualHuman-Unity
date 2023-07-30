using UnityEngine;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace Kurisu.VirtualHuman
{
    [Serializable]
    public class KoboldCharaPreset
    {
        public string char_name;
        [TextArea]
        public string char_persona;
        [TextArea]
        public string world_scenario;
        [TextArea]
        public string example_dialogue;
    }
    public struct KoboldResponse : ILLMData
    {
        public bool Status { get; internal set; }

        public string Response { get; internal set; }
    }
    [Serializable]
    public class ReplaceWord
    {
        public string original;
        public string replace;
    }
    public class KoboldController : MonoBehaviour, ILLMDriver
    {

        [SerializeField]
        private string address = "127.0.0.1";
        public string Address { get => address; set => address = value; }
        [SerializeField]
        private string port = "5001";
        public string Port { get => port; set => port = value; }
        [SerializeField]
        private KoboldCharaPreset charaPreset;
        [TextArea(5, 50)]
        public string generatedMemory;
        [SerializeField, TextArea(5, 50)]
        private string responseCache;
        private KoboldClient client;
        [SerializeField]
        private ReplaceWord[] alwaysReplaceWords;
        [SerializeField]
        private ReplaceWord[] localReplaceWords;
        private static string[] replaceKeyWords = new string[]
        {
            ":","{","}","<START>","\"","\\"," ","END_OF_DIALOGUE","END_OF_ACTIVE_ANSWER"
        };
        private void Start()
        {
            InitClient();
        }
        public void InitClient()
        {
            client = new KoboldClient($"http://{address}:{port}", generatedMemory, charaPreset, new GenParams());
        }
        public async Task<ILLMData> ProcessLLM(string message)
        {
            return await SendMessageToKoboldAsync(message);
        }
        public async Task<KoboldResponse> SendMessageToKoboldAsync(string message)
        {
            bool succeed = true;
            string response = string.Empty;
            try
            {
                var result = await client.Generate(message);
                succeed = true;
                responseCache = FormatResponse(result.Results[0].Text);
                client.AppendNewPrompt(responseCache);
                response = responseCache.Replace($"{charaPreset.char_name}", string.Empty);
                foreach (var keyword in replaceKeyWords)
                {
                    response = response.Replace(keyword, string.Empty);
                }
                for (int i = 0; i < localReplaceWords.Length; i++)
                {
                    response = response.Replace(localReplaceWords[i].original, localReplaceWords[i].replace);
                }
            }
            catch
            {
                succeed = false;
            }
            return new KoboldResponse()
            {
                Status = succeed,
                Response = response
            };
        }
        private string FormatResponse(string response)
        {
            response = response.Replace("{{<BOT>}}", charaPreset.char_name)
                                .Replace("{<BOT>}", charaPreset.char_name)
                                .Replace("<BOT>", charaPreset.char_name);
            response = response.Replace("{{<USER>}}", "darling")
                                .Replace("{<USER>}", "darling")
                                .Replace("<USER>", "darling");
            response = response.Replace("You:", string.Empty);
            for (int i = 0; i < alwaysReplaceWords.Length; i++)
            {
                response = response.Replace(alwaysReplaceWords[i].original, alwaysReplaceWords[i].replace);
            }
            return response;
        }
        public void GenerateMemory()
        {
            generatedMemory = JsonConvert.SerializeObject(charaPreset);
        }
        public void InitMemory()
        {
            client.InitMemory(generatedMemory);
        }
        public async void Check()
        {
            var result = await client.Check();
            Debug.Log(result.Results[0].Text);
        }
        public void Abort()
        {
            client.Abort();
        }
    }
}
