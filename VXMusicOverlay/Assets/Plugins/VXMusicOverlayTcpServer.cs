using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Plugins.Messaging;
using UnityEngine;

namespace Plugins
{
    public class VXMusicOverlayTcpServer : MonoBehaviour
    {
        private VXMusicOverlay _vxmOverlay;

        private TcpListener _tcpListener;
        private Thread _tcpListenerThread;
        private TcpClient _connectedTcpClient;

        private readonly string _localhost = "127.0.0.1";
        private readonly int _vxMusicDesktopTcpPort = 61820;
        private readonly int _vxMusicOverlayTcpPort = 61821;
        
        public bool IsInRecognitionState = false;
        public bool IsAnimationRunning = false;
        
        public event Action<bool> OnRecognitionStateTriggered;

        void Start()
        {
            GameObject _vxmOverlayGameObject = GameObject.Find("OverlayScriptWrapper");

            if (_vxmOverlayGameObject == null)
                Debug.Log("VXMOverlay GameObject was not found.");
            
            _vxmOverlay = _vxmOverlayGameObject.GetComponent<VXMusicOverlay>();
            
            _tcpListenerThread = new Thread(new ThreadStart(ListenForInboundConnections));
            _tcpListenerThread.IsBackground = true;
            _tcpListenerThread.Start();
        }
        
        private void ListenForInboundConnections()
        {
            _tcpListener = new TcpListener(IPAddress.Any, _vxMusicOverlayTcpPort);
            Debug.Log("Server is listening on port " + _vxMusicOverlayTcpPort);
            _tcpListener.Start();
            
            try
            {
                while (true)
                {
                    TcpClient client = _tcpListener.AcceptTcpClient();
                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleIncomingClientMessage));
                    clientThread.Start(client);
                }
            }
            catch (ThreadAbortException)
            {
                Debug.Log("Thread was aborted");
            }
            finally
            {
                if (_tcpListener != null)
                {
                    _tcpListener.Stop();
                }
            }
        }
        
        private void HandleIncomingClientMessage(object clientObj)
        {
            using (TcpClient client = (TcpClient)clientObj)
            using (NetworkStream stream = client.GetStream())
            {
                try
                {
                    while (true)
                    {
                        if (!ReadMessage(stream, out string message))
                        {
                            Debug.Log("Failed to read message or connection closed by client.");
                            break; // Exit the loop if the message couldn't be read or connection was closed
                        }

                        Debug.Log("Received Message from VXMusicDesktop: " + message);
                        
                        var response = ProcessIncomingVXMusicDesktopEventMessage(_vxmOverlay, message);
                        
                        if(!string.IsNullOrEmpty(response))
                            SendMessageToDesktopClient(response);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Exception in HandleIncomingClientMessage: " + ex.Message);
                }
                finally
                {
                    client.Close();
                }
            }
        }

        public void SendMessageToDesktopClient(string message)
        {
            using (TcpClient client = new TcpClient(_localhost, _vxMusicDesktopTcpPort))
            using (NetworkStream stream = client.GetStream())
            {
                // Convert the message to byte array
                byte[] data = Encoding.ASCII.GetBytes(message);

                // Get the length of the message and convert it to byte array
                byte[] lengthPrefix = BitConverter.GetBytes(data.Length);

                // Send the length of the message first
                stream.Write(lengthPrefix, 0, lengthPrefix.Length);

                // Then send the actual message
                stream.Write(data, 0, data.Length);
            }
        }
        
        private bool ReadMessage(NetworkStream stream, out string message)
        {
            message = null;
            byte[] lengthBytes = new byte[4];
            if (stream.Read(lengthBytes, 0, lengthBytes.Length) != lengthBytes.Length)
            {
                // TODO Handle these errors better
                return false; // Stream closed or error
            }
        
            int length = BitConverter.ToInt32(lengthBytes, 0);
            byte[] buffer = new byte[length];
            int totalRead = 0;
            while (totalRead < length)
            {
                int read = stream.Read(buffer, totalRead, length - totalRead);
                if (read == 0)
                {
                    return false; // Stream closed or error
                }
                totalRead += read;
            }
        
            message = Encoding.ASCII.GetString(buffer);
            return true;
        }
        
        public string ProcessIncomingVXMusicDesktopEventMessage(VXMusicOverlay vxmOverlay, string incomingMessage)
        {
            // Grab running instance of VXMusic Overlay
            
            switch (incomingMessage)
            {
                case VXMMessage.RECOGNITION_ACKNOWLEDGE:
                    Debug.Log("Received Recognition ACK from Desktop Client: " + incomingMessage);
                    IsInRecognitionState = true;
                    OnRecognitionStateTriggered?.Invoke(true);
                    return null;
                case VXMMessage.RECOGNITION_FINISH:
                    Debug.Log("Received Recognition FIN response from .NET: " + incomingMessage);
                    IsInRecognitionState = false;
                    OnRecognitionStateTriggered?.Invoke(false);
                    return null; // TODO Maybe move these out as they don't need responses.
                case VXMMessage.ENABLE_OVERLAY_ANCHOR_LEFTHAND_REQUEST:
                    // Send the connection ack response back to Unity
                    Debug.Log("Setting Overlay Anchor to Left Hand");
                    vxmOverlay.ChangeAnchorToLeftController();
                    return VXMMessage.ENABLE_OVERLAY_ANCHOR_LEFTHAND_ACKNOWLEDGE;
                case VXMMessage.ENABLE_OVERLAY_ANCHOR_RIGHTHAND_REQUEST:
                    Debug.Log("Setting Overlay Anchor to Right Hand");
                    vxmOverlay.ChangeAnchorToRightController();
                    return VXMMessage.ENABLE_OVERLAY_ANCHOR_RIGHTHAND_ACKNOWLEDGE;
                default:
                    Debug.Log($"UNRECOGNISED MESSAGE SENT FROM VXMUSICDESKTOP: {incomingMessage}");
                    return null;
            }
        }

        void OnApplicationQuit()
        {
            if (_tcpListener != null)
            {
                _tcpListener.Stop();
            }

            if (_tcpListenerThread != null)
            {
                _tcpListenerThread.Interrupt();
                _tcpListenerThread.Abort();
            }
        }
    }
}