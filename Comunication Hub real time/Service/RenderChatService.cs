using System.Net.WebSockets;
using System.Text;

namespace Comunication_Hub_real_time.Service
{
    public class RenderChatService
    {
        private ClientWebSocket? _ws;
        private readonly Uri _serverUri = new Uri("ws://tiny-render-server.onrender.com");
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isConnected = false;

        public event Action<string>? OnMessageReceived;

        public async Task ConnectAsync()
        {
            if (_isConnected && _ws?.State == WebSocketState.Open) 
                return;

            try
            {
                _ws = new ClientWebSocket();
                _cancellationTokenSource = new CancellationTokenSource();
                
                await _ws.ConnectAsync(_serverUri, _cancellationTokenSource.Token);
                _isConnected = true;
                
                _ = ReceiveMessages(); // Start listening in background
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection error: {ex.Message}");
                _isConnected = false;
                throw;
            }
        }

        private async Task ReceiveMessages()
        {
            if (_ws == null || _cancellationTokenSource == null) return;

            var buffer = new byte[1024 * 4];
            
            try
            {
                while (_ws.State == WebSocketState.Open && !_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    var result = await _ws.ReceiveAsync(
                        new ArraySegment<byte>(buffer), 
                        _cancellationTokenSource.Token
                    );

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _ws.CloseAsync(
                            WebSocketCloseStatus.NormalClosure, 
                            "Closing", 
                            CancellationToken.None
                        );
                        _isConnected = false;
                    }
                    else
                    {
                        var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        OnMessageReceived?.Invoke(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Receive error: {ex.Message}");
                _isConnected = false;
            }
        }

        public async Task SendMessage(string message)
        {
            if (_ws == null || _ws.State != WebSocketState.Open)
            {
                Console.WriteLine("WebSocket is not connected");
                return;
            }

            try
            {
                var userName = Environment.UserName;
                var payload = new
                {
                    type = userName,   // aici punem numele de utilizator în loc de "update"
                    message = message
                };

                var json = System.Text.Json.JsonSerializer.Serialize(payload);
                var bytes = Encoding.UTF8.GetBytes(json);

                await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Send error: {ex.Message}");
                _isConnected = false;
            }
        }

        public async Task DisconnectAsync()
        {
            if (_ws?.State == WebSocketState.Open)
            {
                _cancellationTokenSource?.Cancel();
                await _ws.CloseAsync(
                    WebSocketCloseStatus.NormalClosure, 
                    "Disconnecting", 
                    CancellationToken.None
                );
                _isConnected = false;
            }
            
            _ws?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
    }
}
