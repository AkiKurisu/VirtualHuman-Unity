using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Text;
namespace Kurisu.VirtualHuman
{
    public struct GoogleTranslateResponse
    {
        public bool Status { get; internal set; }
        public string SourceText { get; internal set; }
        public string TranslateText { get; internal set; }
    }
    public class GoogleTranslator
    {
        public static async Task<GoogleTranslateResponse> TranslateTextAsync(string sourceLanguage, string targetLanguage, string sourceText)
        {
            StringBuilder stringBuilder = new();
            var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={sourceLanguage}&tl={targetLanguage}&dt=t&q={UnityWebRequest.EscapeURL(sourceText)}";
            var webRequest = UnityWebRequest.Get(url);
            webRequest.SendWebRequest();
            while (!webRequest.isDone)
            {
                await Task.Yield();
            }
            if (webRequest.result is UnityWebRequest.Result.ProtocolError or UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogError(webRequest.error);
                return new GoogleTranslateResponse()
                {
                    Status = false,
                    SourceText = sourceText,
                    TranslateText = string.Empty
                };
            }
            JToken parsedTexts = JToken.Parse(webRequest.downloadHandler.text);
            if (parsedTexts != null && parsedTexts[0] != null)
            {
                var jsonArray = parsedTexts[0].AsJEnumerable();

                if (jsonArray != null)
                {
                    foreach (JToken innerArray in jsonArray)
                    {
                        JToken text = innerArray[0];

                        if (text != null)
                        {
                            stringBuilder.Append(text);
                            stringBuilder.Append(' ');
                        }
                    }
                }
            }
            return new GoogleTranslateResponse()
            {
                Status = true,
                SourceText = sourceText,
                TranslateText = stringBuilder.ToString().Trim()
            };
        }
    }
}
