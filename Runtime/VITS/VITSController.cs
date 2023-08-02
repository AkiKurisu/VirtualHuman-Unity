using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System;
namespace Kurisu.VirtualHuman
{
    public struct VITSResponse
    {
        public AudioClip Result { get; internal set; }
        public bool Status { get; internal set; }
    }
    public class VITSController : MonoBehaviour
    {
        private const string CallAPIBase = "http://{0}:{1}/voice/vits?text={2}&id={3}";
        [SerializeField]
        private string address = "127.0.0.1";
        [SerializeField]
        private string port = "23456";
        public string Address { get => address; set => address = value; }
        public string Port { get => port; set => port = value; }
        [SerializeField]
        private int characterID = 0;
        public int CharacterID { get => characterID; set => characterID = value; }
        [SerializeField, HideInInspector]
        private AudioClip audioClipCache;
        private string GetURL(string message)
        {
            return string.Format(CallAPIBase, address, port, message, characterID);
        }
        private void CacheAudioClip(AudioClip audioClip)
        {
            audioClipCache = audioClip;
            audioClipCache.name = $"VITS-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.wav";
        }
        public async Task<VITSResponse> SendVITSRequestAsync(string message)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(GetURL(message), AudioType.WAV))
            {
                www.SendWebRequest();
                while (!www.isDone)
                {
                    await Task.Yield();
                }
                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log(www.error);
                    return new VITSResponse()
                    {
                        Status = false
                    };
                }
                else
                {
                    AudioClip audioClip = null;
                    bool validate;
                    try
                    {
                        audioClip = DownloadHandlerAudioClip.GetContent(www);
                        CacheAudioClip(audioClip);
                        validate = true;
                    }
                    catch
                    {
                        validate = false;
                    }
                    return new VITSResponse()
                    {
                        Result = audioClip,
                        Status = validate
                    };
                }
            }
        }
    }
}
