using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using Plugins;
using UnityEngine;

namespace Valve.VR
{
    public class VXMusicInterface : MonoBehaviour
    {
        private VXMusicOverlay _vxmOverlay;
        
        public bool IsVXMusicDesktopConnected;

        private void Start()
        {
            IsVXMusicDesktopConnected = IsVXMusicDesktopClientRunning();
        }

        public bool IsVXMusicDesktopClientRunning()
        {
            // Create and connect the ClientStream
            using (var clientStream = new NamedPipeClientStream(".", VXMMessage.VxMusicHeartbeatPipeName, PipeDirection.InOut))
            {
                clientStream.Connect(3000);

                // Initialize ClientReader and ClientWriter
                var clientReader = new StreamReader(clientStream);
                var clientWriter = new StreamWriter(clientStream) { AutoFlush = true };

                string messageToSend = VXMMessage.CONNECTION_REQUEST;
                clientWriter.WriteLine(messageToSend);
                clientStream.WaitForPipeDrain();

                // Read the response from the .NET server
                string response = clientReader.ReadLine();

                if (response == VXMMessage.CONNECTION_ACKNOWLEDGE)
                {
                    Debug.Log("Received response from .NET: " + response);
                    return true;
                }

                return false;
            }
        }
        
        public async Task SendHeartbeatMessageToDesktopClient(string request)
        {
            using (var clientSteam = new NamedPipeClientStream(".", VXMMessage.VxMusicHeartbeatPipeName, PipeDirection.InOut))
            {
                clientSteam.Connect(5000);
                
                var clientWriter = new StreamWriter(clientSteam) { AutoFlush = true };
                clientWriter.WriteLine(request);

                var clientReader = new StreamReader(clientSteam);

                // Read the response from the .NET server
                string response = await clientReader.ReadLineAsync();
            }
        }
    }
}