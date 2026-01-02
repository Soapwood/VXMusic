using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VXMusicDesktop;
using VXMusicDesktop.MVVM.ViewModel;

namespace VXMusic.Overlay;

public class VXMusicOverlayInterface
{
    public static ILogger Logger = App.ServiceProvider.GetRequiredService<ILogger<VXMusicOverlayInterface>>();
    
    private static readonly string OverlayLogOutputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "VirtualXtensions", "VXMusic", "Logs", "VXMOverlay");
    
    public static NamedPipeServerStream ServerStream;
    public static StreamReader ServerReader;
    public static StreamWriter ServerWriter;

    private static TcpListener _listener;
    
    private static CancellationTokenSource _cancellationTokenSource;
    
    private static int _vxMusicDesktopTcpPort = 61820;
    private static int _vxMusicOverlayTcpPort = 61821;
    private static string _localhost = "127.0.0.1";

    private static bool _isProcessing;

    private static readonly string VxOverlayHeartBeatPipeName = "VXMusicOverlayEventPipe";

    // Overlay State
    private static DispatcherTimer _heartbeatTimer;
    private static readonly int _heartbeatInterval = 8; // in seconds
    
    private static bool _isOverlayConnected;
    public static bool IsOverlayConnected => _isOverlayConnected;
    
    public static bool HasNewHeartbeatMessage { get; set; }
    public static bool OverlayWasRunning { get; set; }

    public static SharedViewModel SharedViewModel { get; set; }

    public static Process LaunchVXMOverlayRuntime(string runtimePath)
    {
        if(App.VXMusicSession.ToastNotification != null)
            App.VXMusicSession.ToastNotification.SendNotification(NotificationLevel.Info, "", "Launching Overlay", 3);
        
        if (!Directory.Exists(OverlayLogOutputPath))
            Directory.CreateDirectory(OverlayLogOutputPath);
        
        string overlayLogFileName = $"VXMOverlayLog-{DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss")}.log";

        ProcessStartInfo overlayProcessStartInfo = new ProcessStartInfo
        {
            FileName = runtimePath,
            Arguments = $"-logFile {Path.Combine(OverlayLogOutputPath, overlayLogFileName)}",
            UseShellExecute = false, // Set this to false to redirect output if needed
            CreateNoWindow = true, 
            Verb = "runas",
            WindowStyle = ProcessWindowStyle.Minimized
        };

        Process overlayProcess = new Process
        {
            StartInfo = overlayProcessStartInfo
        };

        overlayProcess.EnableRaisingEvents = true;
        overlayProcess.Exited += RemoveExistingOverlayInstance;
        
        Logger.LogInformation("Starting VXMusic Overlay Runtime...");
        Logger.LogDebug($"Running {overlayProcessStartInfo.FileName} with args: [{overlayProcessStartInfo.Arguments}]");
        
        overlayProcess.Start();

        Logger.LogDebug($"VXMusicOverlay process running with PID: {overlayProcess.Id}");

        _cancellationTokenSource = new CancellationTokenSource();
        
        _cancellationTokenSource.Token.Register(() =>
        {
            // Close NamedPipeServerStream and TcpListener if they are still active
            ServerStream?.Close();
            _listener?.Stop();

            _isOverlayConnected = false;
            SharedViewModel.IsOverlayRunning = false;
            OverlayWasRunning = false;
            
            _isProcessing = false;
        });
        
        StartVXMusicDesktopHeartbeatListener(_cancellationTokenSource.Token);
        StartHeartbeatMonitoring();
        StartVXMusicDesktopTcpServer(_cancellationTokenSource.Token);

        _isOverlayConnected = true;
        
        // Need to send payload every time the overlay connects
        return overlayProcess;
    }

    private static void RemoveExistingOverlayInstance(object? sender, EventArgs e)
    {
        Logger.LogWarning("VXMusicOverlay has Exited. Tearing down.");
        App.VXMOverlayProcess = null;
        
        VXMusicSession.NotificationClient.SendNotification(NotificationLevel.Warning,"VXMusic Overlay has Disconnected.","", 5);
        App.VXMusicSession.ToastNotification.SendNotification(NotificationLevel.Warning,"VXMusic Overlay has Disconnected.","", 5);

        _isOverlayConnected = false;
        
        Logger.LogDebug("Triggering Cancellation Token for Heartbeat Listener and TCP Server.");
        _heartbeatTimer.Stop();
        _cancellationTokenSource?.Cancel();
    }
    
    public static void StartHeartbeatMonitoring()
    {
        _heartbeatTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(_heartbeatInterval)
        };
        _heartbeatTimer.Tick += OnHeartbeatTick;
        _heartbeatTimer.Start();
    }
    
    private static void OnHeartbeatTick(object sender, EventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (HasNewHeartbeatMessage)
            {
                SharedViewModel.IsOverlayRunning = true;
                HasNewHeartbeatMessage = false;
            }
            else
            {
                SharedViewModel.IsOverlayRunning = false;
                //Logger.LogWarning("No heartbeat received, overlay might be disconnected.");
            }
        });
    }
    
    public static async Task StartVXMusicDesktopHeartbeatListener(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Waiting for Unity Runtime Client to Connect...");

        try
        {
            Logger.LogTrace("Listening for Heartbeats from VXMusic Overlay");
            while (!cancellationToken.IsCancellationRequested)
            {
                using (ServerStream =
                           new NamedPipeServerStream(VXMMessage.VxMusicHeartbeatPipeName, PipeDirection.InOut))
                {

                    // Wait for connection, and cancel wait if cancellation token is triggered.
                    await ServerStream.WaitForConnectionAsync(cancellationToken);

                    // Ensure cancellation hasn't been triggered.
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        _isProcessing = true;

                        using (ServerReader = new StreamReader(ServerStream))
                        using (ServerWriter = new StreamWriter(ServerStream) { AutoFlush = true })
                        {
                            string eventData = await ServerReader.ReadLineAsync();
                            Logger.LogTrace($"Received Heartbeat from Unity Client: {eventData}");
                            await ProcessIncomingUnityHeartbeat(ServerWriter, eventData);
                        }
                    }
                }
            }
        }
        catch (OperationCanceledException oce)
        {
            Logger.LogDebug("VXMusicDesktopHeartbeatListener Cancellation was Requested.");
            Logger.LogDebug(oce.Message);

            _isOverlayConnected = false;
        }
        
        Logger.LogDebug("VXMusicDesktopHeartbeatListener has stopped listening.");
    }

    public static async Task StartVXMusicDesktopTcpServer(CancellationToken cancellationToken)
    {
        try
        {
            _listener = new TcpListener(IPAddress.Any, _vxMusicDesktopTcpPort);
            _listener.Start();
        }
        catch (Exception ex)
        {
            Logger.LogError($"TCP Server failed to start! Desktop Port: {_vxMusicDesktopTcpPort} Overlay Port: {_vxMusicOverlayTcpPort}");
            Logger.LogError(ex.Message);
        }
        
        Logger.LogTrace($"VXMusic Desktop TCP Server started.");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Logger.LogTrace("Waiting for Overlay Messages...");
                TcpClient client = await _listener.AcceptTcpClientAsync(cancellationToken);
                
                if (!cancellationToken.IsCancellationRequested)
                {
                    Logger.LogDebug("Overlay connected for Message.");
                    ProcessClientAsync(client);
                }
            }
        }
        catch (OperationCanceledException oce)
        {
            Logger.LogDebug("VXMusicDesktopTcpServer Cancellation was Requested.");
            Logger.LogDebug(oce.Message);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Server error: {ex.Message}");
        }
        finally
        {
            if (_listener != null)
            {
                _listener.Stop();
                _listener = null;
            }
        }
        Logger.LogDebug("VXMusicDesktopTcpServer has stopped listening.");
    }
    
    private static async Task ProcessClientAsync(TcpClient client)
    {
        try
        {
            using (client)
            using (NetworkStream stream = client.GetStream())
            {
                await ListenForMessages(stream);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error processing client: {ex.Message}");
        }
    }
    
    private static async Task ListenForMessages(NetworkStream stream)
    {
        try
        {
            while (true)
            {
                ReadMessage(stream, out var message);
                if (message == null)
                {
                    Logger.LogTrace("Client disconnected or stream ended.");
                    break;
                }
                Logger.LogTrace("Received from VXMusic Overlay: " + message);
                ProcessIncomingUnityEventMessage(message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Error in receiving data: " + ex.Message);
            // Handle the error (e.g., reconnect logic)
        }
    }

    private static Task<bool> SendOverlayInitPayload()
    {
        Logger.LogDebug($"Sending Overlay Init Payload to VXMusicOverlay: {App.VXMusicSession.OverlaySettings.ToPayload()}");
        return SendMessageToVxMusicOverlayOverTcp(App.VXMusicSession.OverlaySettings.ToPayload()); //await SendMessageToVxMusicOverlay(App.VXMusicSession.OverlaySettings.ToPayload());
    }
    
    public static Task<bool> SendOverlayAnchorUpdateRequest(string messageContent)
    {
        return SendMessageToVxMusicOverlayOverTcp(messageContent);
    }

    public static async Task<bool> SendMessageToVxMusicOverlayOverTcp(string messageContent)
    {
        using (TcpClient client = new TcpClient(_localhost, _vxMusicOverlayTcpPort))
        using (NetworkStream stream = client.GetStream())
        {
            await SendMessageAsync(stream, messageContent);

            // Wait for and read the acknowledgment from the server
            string response = await ReadMessageAsync(stream);
            Logger.LogTrace("Received from VXMusic Overlay: " + response);
        }
        return true;
    }
    
    private static async Task SendMessageAsync(NetworkStream stream, string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        byte[] lengthPrefix = BitConverter.GetBytes(data.Length);
        await stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
        await stream.WriteAsync(data, 0, data.Length);
    }
    
    private static bool ReadMessage(NetworkStream stream, out string message)
    {
        message = null;
        byte[] lengthBytes = new byte[4];
        int totalBytesRead = 0;

        // Ensure all bytes for length are read
        while (totalBytesRead < 4)
        {
            int bytesRead = stream.Read(lengthBytes, totalBytesRead, 4 - totalBytesRead);
            if (bytesRead == 0)
            {
                return false; // Connection has been closed by the client.
            }
            totalBytesRead += bytesRead;
        }

        int length = BitConverter.ToInt32(lengthBytes, 0);
        byte[] buffer = new byte[length];
        totalBytesRead = 0;

        // Ensure all bytes for the message are read
        while (totalBytesRead < length)
        {
            int bytesRead = stream.Read(buffer, totalBytesRead, length - totalBytesRead);
            if (bytesRead == 0)
            {
                return false; // Connection has been closed by the client.
            }
            totalBytesRead += bytesRead;
        }

        message = Encoding.ASCII.GetString(buffer);
        return true;
    }
    
    private static async Task<string> ReadMessageAsync(NetworkStream stream)
    {
        byte[] lengthBytes = new byte[4];
        int bytesRead = await stream.ReadAsync(lengthBytes, 0, lengthBytes.Length);
        if (bytesRead != lengthBytes.Length)
        {
            throw new Exception("Failed to read the message length from the stream.");
        }

        int length = BitConverter.ToInt32(lengthBytes, 0);
        byte[] buffer = new byte[length];
        int totalRead = 0;
        while (totalRead < length)
        {
            bytesRead = await stream.ReadAsync(buffer, totalRead, length - totalRead);
            if (bytesRead == 0)
            {
                throw new Exception("Connection closed prematurely.");
            }
            totalRead += bytesRead;
        }

        return Encoding.ASCII.GetString(buffer);
    }

    public static async Task<bool> ProcessIncomingUnityHeartbeat(StreamWriter writer, string incomingMessage)
    {
        switch (incomingMessage)
        {
            case VXMMessage.CONNECTION_REQUEST:
                return await HandleOverlayConnectionRequest(writer);
            case VXMMessage.CONNECTION_HEARTBEAT_REQUEST:
                writer.WriteLine(VXMMessage.CONNECTION_HEARTBEAT_ACKNOWLEDGE);
                SharedViewModel.IsOverlayRunning = true;
                HasNewHeartbeatMessage = true; // Flush has new HeartbeatMessage flag
                return true;
            case VXMMessage.CONNECTION_TERMINATION:
                Logger.LogInformation($"Received a Termination message from VXMusicOverlay!");
                SharedViewModel.IsOverlayRunning = false;
                return true;
            default:
                Logger.LogError($"UNRECOGNISED MESSAGE SENT FROM OVERLAY TO VXMUSIC: {incomingMessage}");
                return false;
        }
    }

    public static async Task<bool> ProcessIncomingUnityEventMessage(string incomingMessage)
    {
        switch (incomingMessage)
        {
            case VXMMessage.RECOGNITION_REQUEST:
                return await HandleRecognitionRequest();
            case VXMMessage.ENABLE_OVERLAY_ANCHOR_LEFTHAND_ACKNOWLEDGE:
                VXUserSettings.Overlay.SetOverlayAnchor(VXMusicOverlayAnchor.LeftHand); // TODO Move this logic out somewhere else
                App.VXMusicSession.OverlaySettings.OverlayAnchor = VXMusicOverlayAnchor.LeftHand;
                return true;
            case VXMMessage.ENABLE_OVERLAY_ANCHOR_RIGHTHAND_ACKNOWLEDGE:
                VXUserSettings.Overlay.SetOverlayAnchor(VXMusicOverlayAnchor.RightHand);
                App.VXMusicSession.OverlaySettings.OverlayAnchor = VXMusicOverlayAnchor.RightHand;
                return true;
            default:
                Logger.LogError($"UNRECOGNISED MESSAGE SENT FROM UNITY TO VXMUSIC: {incomingMessage}");
                return false;
        }
    }

    private static bool IsCurrentRecognitionClientConnected(RecognitionApi currentRecognitionApi)
    {
        switch (currentRecognitionApi)
        {
            case RecognitionApi.Shazam:
                return SharedViewModel.IsShazamApiConnected;
            case RecognitionApi.Unknown:
                return false;
            default:
                return false;
        }
    }

    public static async Task<bool> HandleRecognitionRequest()
    {
        RecognitionApi currentRecognitionApi = App.VXMusicSession.RecognitionSettings.CurrentRecognitionApi;
        
        // This shouldn't be checked by the overlay anyway
        // if (!IsCurrentRecognitionClientConnected(currentRecognitionApi))
        // {
        //     Logger.LogError($"Recognition Failed. {currentRecognitionApi} is not connected.");
        //     SharedViewModel.IsRecognitionRunning = false;
        //     //App.ToastNotification.Error($"{currentRecognitionApi} is not connected! Check your Recognition settings.");
        //     VXMusicSession.NotificationClient.SendNotification(NotificationLevel.Error, "Recognition Failed", $"{currentRecognitionApi} is not connected! Check your Recognition settings.", 8);
        //     App.VXMusicSession.ToastNotification.SendNotification(NotificationLevel.Error, "Recognition Failed", $"{currentRecognitionApi} is not connected! Check your Recognition settings.", 8);
        //     // Finish, or effectively cancel recognition if fails.
        //     SendMessageToVxMusicOverlayOverTcp(VXMMessage.RECOGNITION_FINISH);
        //     return false;
        // }
        try
        {
            if (!SharedViewModel.IsRecognitionRunning)
            {
                Logger.LogTrace($"Sending event to VXMusicOverlay: {VXMMessage.RECOGNITION_ACKNOWLEDGE}");
                SendMessageToVxMusicOverlayOverTcp(VXMMessage.RECOGNITION_ACKNOWLEDGE);

                Logger.LogInformation("Running Recognition Flow from VXMusic Overlay.");
                SharedViewModel.IsRecognitionRunning = true;
                bool isFinished = await VXMusicActions.PerformRecognitionFlow();
                SharedViewModel.IsRecognitionRunning = false;

                Logger.LogTrace($"Sending event to VXMusicOverlay: {VXMMessage.RECOGNITION_FINISH}");
                SendMessageToVxMusicOverlayOverTcp(VXMMessage.RECOGNITION_FINISH);

                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Recognition failed: Exception {ex}");
            SharedViewModel.IsRecognitionRunning = false;
            SendMessageToVxMusicOverlayOverTcp(VXMMessage.RECOGNITION_FINISH);
            return false;
        }
        
        return false;
    }

    public static async Task<bool> HandleOverlayConnectionRequest(StreamWriter writer)
    {
        // Send the connection ack response back to Unity
        if (!SharedViewModel.IsOverlayRunning)
        {
            //SendMessageToVxMusicOverlayOverTcp(VXMMessage.CONNECTION_ACKNOWLEDGE);
            writer.WriteLine(VXMMessage.CONNECTION_ACKNOWLEDGE);
            Logger.LogInformation($"Connected to VXMusicOverlay!");
            VXMusicSession.NotificationClient.SendNotification(NotificationLevel.Success,"VXMusic Overlay Connected!", "",4);
            App.VXMusicSession.ToastNotification.SendNotification(NotificationLevel.Success,"VXMusic Overlay Connected!", "",4);
            SharedViewModel.IsOverlayRunning = true;
            OverlayWasRunning = true;

            SendOverlayInitPayload();
            
            return true;
        }

        Logger.LogWarning("Overlay tried to connect but it is already connected.");
        return false;
    }
}
