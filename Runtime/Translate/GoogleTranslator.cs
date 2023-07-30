using UnityEngine;
using SimpleJSON;
using UnityEngine.Networking;
using System.Threading.Tasks;
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
            var parsedTexts = JSONNode.Parse(webRequest.downloadHandler.text);
            var translatedText = string.Empty;
            if (parsedTexts != null && parsedTexts[0] != null)
            {
                var jsonArray = parsedTexts[0].AsArray;

                if (jsonArray != null)
                {
                    foreach (JSONNode innerArray in jsonArray)
                    {
                        string text = innerArray[0];

                        if (!string.IsNullOrEmpty(text))
                        {
                            translatedText += text + " ";
                        }
                    }
                }
            }
            return new GoogleTranslateResponse()
            {
                Status = true,
                SourceText = sourceText,
                TranslateText = translatedText.Trim()
            };
        }
    }
}
