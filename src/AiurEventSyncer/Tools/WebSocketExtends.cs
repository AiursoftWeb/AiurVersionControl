using AiurStore.Tools;
using System;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AiurEventSyncer.Tools
{
    public static class WebSocketExtends
    {
        public static async Task SendMessage(this WebSocket ws, string message)
        {
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            await ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public static async Task<string> GetMessage(this WebSocket ws)
        {
            using var ms = new MemoryStream();
            WebSocketReceiveResult result;
            do
            {
                var messageBuffer = WebSocket.CreateClientBuffer(1024, 16);
                result = await ws.ReceiveAsync(messageBuffer, CancellationToken.None);
                ms.Write(messageBuffer.Array, messageBuffer.Offset, result.Count);
            }
            while (!result.EndOfMessage);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                var msgString = Encoding.UTF8.GetString(ms.ToArray());
                ms.Seek(0, SeekOrigin.Begin);
                ms.Position = 0;
                return msgString;
            }
            throw new InvalidOperationException();
        }

        public static async Task SendObject<T>(this WebSocket ws, T model)
        {
            var rawJson = JsonTools.Serialize(model);
            await ws.SendMessage(rawJson);
        }

        public static async Task<T> GetObject<T>(this WebSocket ws)
        {
            var rawJson = await ws.GetMessage();
            return JsonTools.Deserialize<T>(rawJson);
        }
    }
}
