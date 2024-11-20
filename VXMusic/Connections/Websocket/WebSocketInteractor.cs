using System.Net.WebSockets;
using System.Text;

namespace VXMusic.Connections.Websocket;

public class WebSocketInteractor
{
    private readonly ClientWebSocket _webSocket = new ClientWebSocket();

    public async Task ConnectAsync(Uri serverUri)
    {
        try
        {
            Console.WriteLine("Connecting to server...");
            await _webSocket.ConnectAsync(serverUri, CancellationToken.None);
            Console.WriteLine("Connected to server.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to server: {ex.Message}");
        }
    }

    public async Task SendAsync(string message)
    {
        if (_webSocket.State == WebSocketState.Open)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            var buffer = new ArraySegment<byte>(bytes);
            await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine("Message sent: " + message);
        }
        else
        {
            Console.WriteLine("WebSocket is not open.");
        }
    }

    public async Task ReceiveAsync()
    {
        var buffer = new byte[1024];
        var segment = new ArraySegment<byte>(buffer);

        while (_webSocket.State == WebSocketState.Open)
        {
            try
            {
                var result = await _webSocket.ReceiveAsync(segment, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine("Server requested close. Closing connection...");
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }
                else
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine("Message received: " + message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving data: {ex.Message}");
            }
        }
    }

    public async Task CloseAsync()
    {
        if (_webSocket.State == WebSocketState.Open)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None);
            Console.WriteLine("WebSocket connection closed.");
        }
    }
}
