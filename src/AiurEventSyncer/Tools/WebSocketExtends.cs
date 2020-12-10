using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AiurEventSyncer.Tools
{
   public static  class WebSocketExtends
    {
        public static async Task SendMessage(WebSocket ws, string message)
        {
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            await ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public static async Task<string> GetMessage(WebSocket ws)
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
    }
}
