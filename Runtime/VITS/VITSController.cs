using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
namespace Kurisu.VirtualHuman
{
    public struct VITSHandle
    {
        public AudioClip Result { get; internal set; }
        public bool Status { get; internal set; }
        public void Release()
        {
            if (Result != null) UnityEngine.Object.Destroy(Result);
            Result = null;
        }
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
        private string GetURL(string message)
        {
            return string.Format(CallAPIBase, address, port, message, characterID);
        }
        public async Task<VITSHandle> SendVITSRequestAsync(string message)
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
                    return new VITSHandle()
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
                        validate = true;
                    }
                    catch
                    {
                        validate = false;
                    }
                    return new VITSHandle()
                    {
                        Result = audioClip,
                        Status = validate
                    };
                }
            }
        }
    }
}
