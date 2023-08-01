using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace Kurisu.VirtualHuman
{
    //Modify from https://github.com/pboardman/KoboldSharp
    public class KoboldClient
    {
        private readonly HttpClient _client;
        private readonly string _baseUri;
        private string promptAlwaysInclude;
        private readonly GenParams genParams;
        private StringBuilder stringBuilder;
        private readonly KoboldCharaPreset charaPreset;
        public KoboldClient(string baseUri, string memory, KoboldCharaPreset charaPreset, GenParams genParams)
        {
            _client = new HttpClient()
            {
                Timeout = TimeSpan.FromMinutes(5)
            };
            stringBuilder = new StringBuilder();
            _baseUri = baseUri;
            this.genParams = genParams;
            this.charaPreset = charaPreset;
            stringBuilder.Clear();
            stringBuilder.Append(memory);
            stringBuilder.Append('\n');
            //User input first
            stringBuilder.Append(charaPreset.user_Name);
            stringBuilder.Append(':');
            promptAlwaysInclude = stringBuilder.ToString();
        }
        public void AppendNewPrompt(string newPrompt)
        {
            stringBuilder.Clear();
            stringBuilder.Append(promptAlwaysInclude);
            stringBuilder.Append('\n');
            stringBuilder.Append(newPrompt);
            promptAlwaysInclude = stringBuilder.ToString();
        }
        public async Task<ModelOutput> Generate(string message)
        {
            stringBuilder.Clear();
            stringBuilder.Append(promptAlwaysInclude);
            stringBuilder.Append(message);
            //Should be add if no end symbol ('!','.','?') added.
            stringBuilder.Append('\n');
            //In order to let AI know it should complete character's dialogue.
            stringBuilder.Append(charaPreset.char_name);
            stringBuilder.Append(": ");
            genParams.Prompt = stringBuilder.ToString();
            promptAlwaysInclude = genParams.Prompt;
            var payload = new StringContent(genParams.GetJson(), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"{_baseUri}/api/v1/generate", payload);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            content = content.Trim();
            return JsonConvert.DeserializeObject<ModelOutput>(content);
        }
        public void SetMemory(string memory)
        {
            promptAlwaysInclude = memory;
        }

        public async Task<ModelOutput> Check()
        {
            var payload = new StringContent(string.Empty);
            var response = await _client.PostAsync($"{_baseUri}/api/extra/generate/check", payload);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            content = content.Trim();
            return JsonConvert.DeserializeObject<ModelOutput>(content);
        }
        public async void Abort()
        {
            var payload = new StringContent(string.Empty);
            var response = await _client.PostAsync($"{_baseUri}/api/v1/abort", payload);
            await response.Content.ReadAsStringAsync();
        }
    }
}
