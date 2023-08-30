using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
namespace Kurisu.VirtualHuman
{
    public struct GLMResponse : ILLMData
    {
        public bool Status { get; internal set; }
        public string Response { get; internal set; }
    }
    /// <summary>
    /// Use ChatGLM with OpenAI API format to generate chat text
    /// See https://github.com/THUDM/ChatGLM2-6B/blob/main/openai_api.py
    /// </summary>
    public class GLMTransport : MonoBehaviour, ILLMDriver
    {
        [SerializeField]
        private string address = "127.0.0.1";
        public string Address { get => address; set => address = value; }
        [SerializeField]
        private string port = "5001";
        public string Port { get => port; set => port = value; }
        private const string m_gptModel = "gpt-3.5-turbo";
        private readonly List<SendData> m_DataList = new();
        [SerializeField, TextArea(5, 20)]
        private string m_Prompt;
        private SendData promptData;
        private void Awake()
        {
            promptData = new SendData("system", m_Prompt);
            m_DataList.Add(promptData);
        }
        public void SetPrompt(string prompt)
        {
            promptData.content = prompt;
        }
        public async Task<ILLMData> ProcessLLM(string message)
        {
            return await SendMessageToGPTAsync(message);
        }
        public async Task<GPTResponse> SendMessageToGPTAsync(string message)
        {
            string _baseUri = $"http://{address}:{port}/v1/chat/completions";
            m_DataList.Add(new SendData("user", message));
            using UnityWebRequest request = new(_baseUri, "POST");
            PostData _postData = new()
            {
                model = m_gptModel,
                messages = m_DataList
            };

            string _jsonText = JsonUtility.ToJson(_postData);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(_jsonText);
            request.uploadHandler = new UploadHandlerRaw(data);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
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
                return new GPTResponse()
                {
                    Response = _backMsg,
                    Status = true
                };
            }
            Debug.Log($"ChatGLM_responseCode : {request.responseCode}");
            return new GPTResponse()
            {
                Response = string.Empty,
                Status = false
            };
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

