using Aiursoft.AiurStore.Tools;
using System.Net.WebSockets;
using System.Text;

namespace Aiursoft.AiurEventSyncer.Tools
{
    public static class WebSocketExtends
    {
        public static async Task SendMessage(this WebSocket ws, string message)
        {
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            await ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public static async Task Subscribe(this WebSocket ws, Func<string, Task> onNewMessage)
        {
            var ms = new MemoryStream();
            while (ws.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;
                do
                {
                    var messageBuffer = WebSocket.CreateClientBuffer(1024, 16);
                    result = await ws.ReceiveAsync(messageBuffer, CancellationToken.None);
                    ms.Write(messageBuffer.Array ?? Array.Empty<byte>(), messageBuffer.Offset, result.Count);
                }
                while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var msgString = Encoding.UTF8.GetString(ms.ToArray());
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.SetLength(0);
                    await onNewMessage(msgString);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public static async Task SendObject<T>(this WebSocket ws, T model)
        {
            var rawJson = JsonTools.Serialize(model);
            await ws.SendMessage(rawJson);
        }

        public static Task Monitor<T>(this WebSocket ws, Func<T, Task> onNewObject)
        {
            return ws.Subscribe(rawJson => onNewObject(JsonTools.Deserialize<T>(rawJson)));
        }
    }
}
