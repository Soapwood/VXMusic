using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using UnityEngine;

namespace Valve.VR
{
    public class VXMusicInterface : MonoBehaviour
    {
        private NamedPipeClientStream ClientStream;
        private StreamReader ClientReader;
        private StreamWriter ClientWriter;
        public event Action<string> ServerResponseReceived;

        public bool IsInRecognitionState = false;
        public bool IsAnimationRunning = false;
        public event Action<bool> OnRecognitionStateTriggered;

        public bool IsListening = false;

        private void Start()
        {
            IsVXMusicServerRunning();
        }

        public void IsVXMusicServerRunning()
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

            ClientStream.Close();
            ClientStream = null;

            ClientReader.Close();
            ClientReader = null;
            ClientWriter.Close();
            ClientWriter = null;
        }

        public async void ListenForVXMusicDesktopEvent()
        {
            // Create and connect the ClientStream
            //ClientStream = new NamedPipeClientStream(".", "VXMusicOverlayEventPipeHandUpdate", PipeDirection.InOut);
            //ClientStream.Connect();

            // Initialize ClientReader and ClientWriter
            //ClientReader = new StreamReader(ClientStream);
            //ClientWriter = new StreamWriter(ClientStream) { AutoFlush = true };

            using (NamedPipeServerStream serverStream =
                   new NamedPipeServerStream("VXMusicOverlayEventPipeHandUpdate", PipeDirection.InOut))
            {
                Debug.Log("Waiting for Overlay request from VXMusicDesktop");
                IsListening = true;
                serverStream.WaitForConnection();
                //_isProcessing = true;

                using (StreamReader reader = new StreamReader(serverStream))
                using (StreamWriter writer = new StreamWriter(serverStream) { AutoFlush = true })
                {
                    writer.WriteLine("VX_CONNECT_ACK");
                    
                    string eventData = await reader.ReadLineAsync();
                    Debug.Log($"Received event from VXMusic Desktop Client: {eventData}");
                    //await ProcessIncomingUnityEventMessage(writer, eventData);
                }
            }

            //string messageToSend = "VX_CONNECT_REQ";
            //ClientWriter.WriteLine(messageToSend);
            // ClientStream.WaitForPipeDrain();

            // Read the response from the .NET server
            // string response = ClientReader.ReadLine();
            //
            // if (response == "VX_OVERLAY_ANCHOR_LEFTHAND_REQ")
            // {
            //     Debug.Log("Received response from .NET: " + response);
            // }
            //
            // if (response == "VX_OVERLAY_ANCHOR_RIGHTHAND_REQ")
            // {
            //     Debug.Log("Received response from .NET: " + response);
            // }

            ClientStream.Close();
            ClientStream = null;

            ClientReader.Close();
            ClientReader = null;
            ClientWriter.Close();
            ClientWriter = null;

            IsListening = false;
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