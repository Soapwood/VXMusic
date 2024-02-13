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

        private void Start()
        {
            // // Initialize the Named Pipe client here
            // pipeClient = new NamedPipeClientStream(".", "VXMusicOverlayEventPipe", PipeDirection.Out);
            // pipeClient.Connect();
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

            // string eventData;
            // // THis is likely where the connectino is failing
            // while ((eventData = await ClientReader.ReadLineAsync()) != null)
            // {
            //     Console.WriteLine($"Received event from Unity: {eventData}");
            //     
            //     switch (response)
            //     {
            //         case "VX_RECOGNITION_ACK":
            //             IsInRecognitionState = true;
            //             //OnRecognitionStateTriggered?.Invoke(IsInRecognitionState);
            //             continue;
            //         case "VX_RECOGNITION_FIN":
            //             IsInRecognitionState = false;
            //             //OnRecognitionStateTriggered?.Invoke(IsInRecognitionState);
            //             continue;
            //         default:
            //             Console.WriteLine("UNRECOGNISED MESSAGE SENT FROM VXM");
            //             continue;
            //     }
            //         
            //     //await writer.WriteLineAsync(line);
            // }

            //Task.Run(() => ListenForServerResponses());
        }
        
        public async Task SendRequestToServer(string request)
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
            
            //SendRequestToServer("VX_TRIGGER_RECOGNITION");
            
            //ClientStream.
            
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