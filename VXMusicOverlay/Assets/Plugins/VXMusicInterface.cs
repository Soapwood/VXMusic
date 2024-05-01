using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using Plugins;
//using UnityEditor.PackageManager;
using UnityEngine;

namespace Valve.VR
{
    public class VXMusicInterface : MonoBehaviour
    {
        private VXMusicOverlay _vxmOverlay;
        
        private static NamedPipeClientStream ClientStream;
        private static StreamReader ClientReader;
        private static StreamWriter ClientWriter;
        
        private static NamedPipeServerStream ServerStream;
        private static StreamReader ServerReader;
        private static StreamWriter ServerWriter;
        public event Action<string> ServerResponseReceived;

        public bool IsInRecognitionState = false;
        public bool IsAnimationRunning = false;
        public event Action<bool> OnRecognitionStateTriggered;

        public bool IsListening;
        public bool IsVXMusicDesktopConnected;

        private void Start()
        {
            GameObject _vxmOverlayGameObject = GameObject.Find("OverlayScriptWrapper");
            
            if (_vxmOverlayGameObject != null)
            {
                _vxmOverlay = _vxmOverlayGameObject.GetComponent<VXMusicOverlay>();

                // if (_vxmOverlay != null)
                // {
                //     // Subscribe to the event in ScriptA
                //     _vxmOverlay.OnRecognitionStateTriggered += HandlePublicValueChange;
                // }
            }
            
            IsVXMusicDesktopConnected = IsVXMusicDesktopClientRunning();
        }

        public bool IsVXMusicDesktopClientRunning()
        {
            // Create and connect the ClientStream
            ClientStream = new NamedPipeClientStream(".", "VXMusicOverlayEventPipe", PipeDirection.InOut);
            ClientStream.Connect();

            // Initialize ClientReader and ClientWriter
            ClientReader = new StreamReader(ClientStream);
            ClientWriter = new StreamWriter(ClientStream) { AutoFlush = true };

            string messageToSend = "VX_CONNECT_REQ";
            ClientWriter.WriteLine(messageToSend);
            ClientStream.WaitForPipeDrain();

            // Read the response from the .NET server
            string response = ClientReader.ReadLine();

            if (response == "VX_CONNECT_ACK")
            {
                Debug.Log("Received response from .NET: " + response);
            }

            //ClientStream.Close();
            //ClientStream = null;

            //ClientReader.Close();
            //ClientReader = null;
            //ClientWriter.Close();
            //ClientWriter = null;

            return true;
        }

        public async void TickVXMusicOverlayEventListener()
        {
            //while (true)
            //{
                using (ServerStream =
                           new NamedPipeServerStream("VXMusicOverlayEventServerPipe", PipeDirection.InOut))
                {
                    Debug.Log("Listening for event from VXMusicDesktop");
                    //IsListening = true;
                    //ClientStream.WaitForConnection();
                    //_isProcessing = true;
                    IsListening = true;
                    await ServerStream.WaitForConnectionAsync();

                    using (ServerReader = new StreamReader(ServerStream))
                    using (ServerWriter = new StreamWriter(ServerStream) { AutoFlush = true })
                    {
                        //ClientWriter.WriteLine("VX_CONNECT_ACK");
                        
                        string eventData = ServerReader.ReadLine();

                        if (!string.IsNullOrEmpty(eventData))
                        {
                            Debug.Log($"Received event from VXMusic Desktop Client: {eventData}");
                            //ProcessIncomingVXMusicDesktopEventMessage(ClientWriter, eventData);
                            
                        }
                    }
                }
                //IsListening = true;
           // }
        }



        public async Task SendMessageToServer(string request)
        {
            // if (ClientStream != null || ClientStream.IsConnected)
            // {
            //     ClientStream.Close();
            // }

            ClientStream = new NamedPipeClientStream(".", "VXMusicOverlayEventPipe", PipeDirection.InOut);
            ClientStream.Connect();

            //ClientStream.Connect();
            // This is also fucked
            ClientWriter = new StreamWriter(ClientStream) { AutoFlush = true };
            ClientWriter.WriteLine(request);
            //ClientWriter.Flush();

            ClientReader = new StreamReader(ClientStream);

            // Read the response from the .NET server
            string response = await ClientReader.ReadLineAsync();

            if (response == "VX_RECOGNITION_ACK")
            {
                Debug.Log("Received REC ACK response from .NET: " + response);
                // ChangeOverlayOpacity(1.0f);
                IsInRecognitionState = true;
                OnRecognitionStateTriggered?.Invoke(IsInRecognitionState);
            }

            response = await ClientReader.ReadLineAsync();

            if (response == "VX_RECOGNITION_FIN")
            {
                Debug.Log("Received REC FIN response from .NET: " + response);
                //ChangeOverlayOpacity(0.2f);
                IsInRecognitionState = false;
                OnRecognitionStateTriggered?.Invoke(IsInRecognitionState);
            }
        }
    }
}