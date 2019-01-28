using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using Newtonsoft.Json;

namespace MyMixerChat
{
    public class MixerChat
    {
        private static int global_id = 0;
        private const int bufferSize = 65536;
        private ClientWebSocket wss;
        private readonly Uri chatUri;
        ArraySegment<byte> clientBuffer;

        public static int CreateId()
        {
            return global_id++;
        } 
 
        public  MixerChat()
        {
            clientBuffer = WebSocket.CreateClientBuffer(bufferSize, bufferSize);
            chatUri = new Uri("wss://chat.mixer.com/?version=1.0");
            wss = new ClientWebSocket();

        }

        public async Task Open()
        {
            await wss.ConnectAsync(chatUri, CancellationToken.None);
        }

        private async Task<string> Receive()
        {
            if (wss.State != WebSocketState.Open)
            {
                throw new Exception("WebSocket is not open.");
            }
            StringBuilder sb = new StringBuilder(1024);
            WebSocketReceiveResult result;
            do
            {
                result = await wss.ReceiveAsync(clientBuffer, CancellationToken.None);
                if (result.CloseStatus != null)
                    throw new Exception("WebSocket closed by server");

                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    throw new Exception("Invalid message type(Binary)");
                }
                else
                {
                    sb.Append(Encoding.UTF8.GetString(clientBuffer.Array, clientBuffer.Offset, result.Count));
                }
            } while (!result.EndOfMessage);

            return sb.ToString();
        }

        private async Task Send(string message)
        {
            if (wss.State != WebSocketState.Open)
            {
                throw new Exception("WebSocket is not open.");
            }
            var msgBuf = Encoding.UTF8.GetBytes(message);
            await wss.SendAsync(new ArraySegment<byte>(msgBuf, 0, msgBuf.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        internal async Task<MixerChatMessage_FromServer> GetMessage()
        {
            var json = await Receive();
            return JsonConvert.DeserializeObject<MixerChatMessage_FromServer>(json);
        }

        internal async Task SendMessage<T>(MixerChatMessage_Method<T> message)
        {
            var json = JsonConvert.SerializeObject(message);
            await Send(json);
        }

        internal async Task<int> callMethod<T>(MixerChatMessage_Method<T> msg)
        {
            await SendMessage(msg);
            return msg.id;
        }

        public async Task<int> optOutEvents()
        {
            return await callMethod(new MixerChatMessage_Method_optOutEvents(new[] { "UserJoin", "UserLeave" }));
        }

        public async Task<int> auth(int channel)
        {
            return await callMethod(new MixerChatMessage_Method_auth(new int?[] { channel, null, null, null }));
        }

        public async Task<int> history(int pageSize = 50)
        {
            return await callMethod(new MixerChatMessage_Method_history(new [] { pageSize }));
        }

        public async Task<int> ping()
        {
            return await callMethod(new MixerChatMessage_Method_ping());
        }
    }

    internal enum MixerChatMessageType
    {
        @null = 0,
        @event = 1,
        method = 2,
        reply = 3,
    }

    internal class MixerChatMessage
    {
        public string type { get; set; }

        [JsonIgnore]
        public MixerChatMessageType Type
        {
            get
            {
                if (!Enum.TryParse(type, out MixerChatMessageType thisType))
                    return MixerChatMessageType.@null;
                return thisType;
            }
            set
            {
                if (value == MixerChatMessageType.@null)
                    return;
                type = Enum.GetName(typeof(MixerChatMessageType), value);
            }
        }
    }


    [Serializable]
    internal class MixerChatMessage_FromServer : MixerChatMessage
    {
        public string @event { get; set; }
        public dynamic error { get; set; }
        public dynamic data { get; set; }
        public int id { get; set; }
    }


    [Serializable]
    internal class MixerChatMessage_Method<argumentType> : MixerChatMessage
    {
        public MixerChatMessage_Method(argumentType[] args)
        {
            id = MixerChat.CreateId();
            Type = MixerChatMessageType.method;
            arguments = new List<argumentType>(args);
        }

        public int id { get; set; }
        public string method { get; set; }
        public List<argumentType> arguments { get; set; }
    }


    [Serializable]
    internal class MixerChatMessage_Method_optOutEvents : MixerChatMessage_Method<string>
    {
        public MixerChatMessage_Method_optOutEvents(string[] args) : base(args)
        {
            method = "optOutEvents";
        }
    }


    [Serializable]
    internal class MixerChatMessage_Method_auth : MixerChatMessage_Method<int?>
    {
        public MixerChatMessage_Method_auth(int?[] args) : base(args)
        {
            method = "auth";
        }
    }

    [Serializable]
    internal class MixerChatMessage_Method_history : MixerChatMessage_Method<int>
    {
        public MixerChatMessage_Method_history(int[] args) : base(args)
        {
            method = "history";
        }
    }

    [Serializable]
    internal class MixerChatMessage_Method_ping : MixerChatMessage_Method<object>
    {
        public MixerChatMessage_Method_ping() : base(new object[]{})
        {
            method = "ping";
        }
    }
}
