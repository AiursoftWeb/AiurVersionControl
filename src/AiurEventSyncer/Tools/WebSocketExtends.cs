using AiurStore.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AiurEventSyncer.Tools
{
   public static  class WebSocketExtends
    {
        public static async Task SendMessage(this WebSocket ws, string message)
        {
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            await ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public static async Task<string> GetMessage(this WebSocket ws)
        {
            var buffer = new ArraySegment<byte>(new byte[2048]);
            var wsResult = await ws.ReceiveAsync(buffer, CancellationToken.None);
            if (wsResult.MessageType == WebSocketMessageType.Text)
            {
                var rawJson = Encoding.UTF8.GetString(buffer.Skip(buffer.Offset).Take(buffer.Count).ToArray()).Trim('\0').Trim();
                return rawJson;
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
