using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
namespace Kurisu.VirtualHuman
{
    public struct GPTResponse : ILLMData
    {
        public bool Status { get; internal set; }
        public string Response { get; internal set; }
    }
    //Modify from https://github.com/Navi-Studio/Virtual-Human-for-Chatting
    public class GPTController : MonoBehaviour, ILLMDriver
    {
        private const string chatAPI = "https://api.openai-proxy.com/v1/chat/completions";
        private const string m_gptModel = "gpt-3.5-turbo";
        private readonly List<SendData> m_DataList = new();
        [SerializeField, TextArea(5, 20)]
        private string alwaysInclude;
        [SerializeField, TextArea(5, 20)]
        private string m_Prompt;
        [SerializeField]
        private string openAIKey;
        [SerializeField, TextArea(5, 50)]
        private string responseCache;
        private void Start()
        {
            m_DataList.Add(new SendData("system", m_Prompt));
        }
        public async Task<ILLMData> ProcessLLM(string message)
        {
            return await SendMessageToGPTAsync(message);
        }
        public async Task<GPTResponse> SendMessageToGPTAsync(string message)
        {
            m_DataList.Add(new SendData("user", message + $"[{alwaysInclude}]"));
            using (UnityWebRequest request = new UnityWebRequest(chatAPI, "POST"))
            {
                PostData _postData = new PostData
                {
                    model = m_gptModel,
                    messages = m_DataList
                };

                string _jsonText = JsonUtility.ToJson(_postData);
                byte[] data = System.Text.Encoding.UTF8.GetBytes(_jsonText);
                request.uploadHandler = (UploadHandler)new UploadHandlerRaw(data);
                request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", string.Format("Bearer {0}", openAIKey));
                request.SendWebRequest();
                while (!request.isDone)
                {
                    await Task.Yield();
                }
                if (request.responseCode == 200)
                {
                    string _msg = request.downloadHandler.text;
                    MessageBack messageBack = JsonUtility.FromJson<MessageBack>(_msg);
                    string _backMsg = string.Empty;
                    if (messageBack != null && messageBack.choices.Count > 0)
                    {

                        _backMsg = messageBack.choices[0].message.content;
                        //添加记录
                        m_DataList.Add(new SendData("assistant", _backMsg));
                    }
                    responseCache = _backMsg;
                    return new GPTResponse()
                    {
                        Response = _backMsg,
                        Status = true
                    };
                }
                Debug.Log($"ChatGPT_responseCode : {request.responseCode}");
                return new GPTResponse()
                {
                    Response = string.Empty,
                    Status = false
                };
            }
        }

        #region 数据包

        [Serializable]
        private class PostData
        {
            public string model;
            public List<SendData> messages;
        }

        [Serializable]
        private class SendData
        {
            public string role;
            public string content;
            public SendData(string _role, string _content)
            {
                role = _role;
                content = _content;
            }

        }
        [Serializable]
        private class MessageBack
        {
            public string id;
            public string created;
            public string model;
            public List<MessageBody> choices;
        }
        [Serializable]
        private class MessageBody
        {
            public Message message;
            public string finish_reason;
            public string index;
        }
        [Serializable]
        private class Message
        {
            public string role;
            public string content;
        }

        #endregion
    }
}

