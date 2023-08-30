using UnityEngine;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;
namespace Kurisu.VirtualHuman
{
    public struct OobaboogaResponse : ILLMData
    {
        public bool Status { get; internal set; }
        public string Response { get; internal set; }
    }
    public class OobaboogaTransport : MonoBehaviour, ILLMDriver
    {

        [SerializeField]
        private string address = "127.0.0.1";
        public string Address { get => address; set => address = value; }
        [SerializeField]
        private string port = "5001";
        public string Port { get => port; set => port = value; }
        [SerializeField]
        private CharaPresetConfig charaPreset;
        [SerializeField]
        private bool generatedOnStart = true;
        [TextArea(5, 50)]
        public string generatedMemory;
        [SerializeField, TextArea(5, 50)]
        private string responseCache;
        private OobaboogaClient client;
        [SerializeField, Tooltip("Replace words for received response and replacement will be cached for next send.")]
        private ReplaceWord[] alwaysReplaceWords;
        [SerializeField, Tooltip("Replace words only for received response, used for User Interface.")]
        private ReplaceWord[] localReplaceWords;
        private readonly StringBuilder stringBuilder = new();
        //There are some key words we should handle manually.
        //For example, if meet "END_OF_DIALOGUE", we should cut the response and keep only the first half.
        //However, during the test, we will see that word should only exists in KoboldAI "Novel" mode.
        //SO currently, we don't handle it but skipp reading them instead which may be modified in future.
        private static readonly string[] replaceKeyWords = new string[]
        {
            "<START>","END_OF_DIALOGUE","END_OF_ACTIVE_ANSWER"
        };
        private void Start()
        {
            if (generatedOnStart)
                GenerateMemory();
            InitClient();
        }
        public void InitClient()
        {
            client = new OobaboogaClient($"http://{address}:{port}",
                        generatedMemory,
                        charaPreset,
                        new OobaboogaParams(new List<string>() { $"{charaPreset.user_Name}:", $"\n{charaPreset.user_Name} " })
                    );
        }
        public async Task<ILLMData> ProcessLLM(string message)
        {
            return await SendMessageToKoboldAsync(message);
        }
        public async Task<OobaboogaResponse> SendMessageToKoboldAsync(string message)
        {
            string response = string.Empty;
            bool succeed;
            try
            {
                var result = await client.Generate(message);
                succeed = true;
                responseCache = FormatResponse(result.Results[0].Text);
                client.AppendNewPrompt(responseCache);
                int index = -1;
                for (int i = 0; i < response.Length; i++)
                    if (response[i] != '\n')
                        break;
                    else
                        index++;
                if (index > -1)
                    response = response.Remove(0, index + 1);
                response = responseCache.Replace($"{charaPreset.char_name}:", string.Empty)
                                        .Replace($"{charaPreset.user_Name}:", string.Empty)
                                        .Replace("\n\n", "\n");
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
            return new OobaboogaResponse()
            {
                Status = succeed,
                Response = response
            };
        }
        private string FormatResponse(string response)
        {
            for (int i = 0; i < alwaysReplaceWords.Length; i++)
            {
                response = response.Replace(alwaysReplaceWords[i].original, alwaysReplaceWords[i].replace);
            }
            return response;
        }
        public void GenerateMemory()
        {
            stringBuilder.Clear();
            //This Prompt is the same as KoboldAI generated, useful for generating chat text
            stringBuilder.Append($"[The following is an interesting chat message log between {charaPreset.user_Name} and {charaPreset.char_name}.");
            //This Prompt is added by me and may be modified in future, you can have a test and change it to your version.
            //Note: 
            //Those Prompts are designed to generate a chat text. 
            //If you want other forms like story mode, prompt should be changed. 
            if (!string.IsNullOrEmpty(charaPreset.char_persona))
                stringBuilder.Append($"\n{charaPreset.char_name}'s persona : {charaPreset.char_persona}");
            if (!string.IsNullOrEmpty(charaPreset.world_scenario))
                stringBuilder.Append($"\nWorld's scenario : {charaPreset.world_scenario}");
            //<START> means chat begining
            stringBuilder.Append("]\n<START>");
            //Example dialogue used as first dialogue piece
            if (!string.IsNullOrEmpty(charaPreset.example_dialogue))
            {
                stringBuilder.Append('\n');
                stringBuilder.Append(charaPreset.example_dialogue);
            }
            generatedMemory = stringBuilder.ToString();
        }
        public void InitMemory()
        {
            client.SetMemory(generatedMemory);
        }
    }
}
