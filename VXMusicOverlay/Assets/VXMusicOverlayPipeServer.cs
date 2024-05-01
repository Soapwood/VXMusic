using System;
using System.Collections;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using UnityEngine;

namespace Plugins
{
    public class VXMusicOverlayPipeServer : MonoBehaviour
    {
        private VXMusicOverlay _vxmOverlay;
        private Thread OverlayServerThread;
        
        void Start()
        {
            // Grab running instance of VXMusic Overlay
            GameObject _vxmOverlayGameObject = GameObject.Find("OverlayScriptWrapper");

            if (_vxmOverlayGameObject != null)
                _vxmOverlay = _vxmOverlayGameObject.GetComponent<VXMusicOverlay>();

            OverlayServerThread = new Thread(ServerThread);
            OverlayServerThread.Start();
        }

        void ServerThread()
        {
            while (true)
            {
                using (var serverStream = new NamedPipeServerStream("VXMusicOverlayEventServerPipe", PipeDirection.InOut))
                {
                    Debug.Log("Waiting for connection...");
                    serverStream.WaitForConnection();
                    Debug.Log("Connected.");
                    
                    StreamReader reader = new StreamReader(serverStream);
                    StreamWriter writer = new StreamWriter(serverStream) { AutoFlush = true };

                    try
                    {
                        // Handle reading and writing in a loop or per protocol
                        while (serverStream.IsConnected)
                        {
                            if (reader.Peek() >= 0)
                            {
                                string message = reader.ReadLine();
                                Debug.Log("Received Message from VXMusic Desktop: " + message);
                                
                                string responseMessage = ProcessIncomingVXMusicDesktopEventMessage(message);
                                
                                Debug.Log("Sending Message to VXMusic Desktop: " + responseMessage);
                                writer.WriteLine(responseMessage);
                                //Thread.Sleep(1000);
                                break;
                            }
                        }
                    }
                    catch (IOException e)
                    {
                        Debug.LogError("Pipe error: " + e.Message);
                    }
                    finally
                    {
                        if (serverStream.IsConnected)
                           serverStream.Disconnect();
                    }
                }
            }
        }
        
        void OnDestroy()
        {
            OverlayServerThread?.Abort();
        }

        private string ProcessIncomingVXMusicDesktopEventMessage(string incomingMessage)
        {
            switch (incomingMessage)
            {
                case VXMMessage.ENABLE_OVERLAY_ANCHOR_LEFTHAND_REQUEST:
                    // Send the connection ack response back to Unity
                    Debug.Log("Setting Overlay Anchor to Left Hand");
                    _vxmOverlay.ChangeAnchorToLeftController();
                    return VXMMessage.ENABLE_OVERLAY_ANCHOR_LEFTHAND_ACKNOWLEDGE;
                case VXMMessage.ENABLE_OVERLAY_ANCHOR_RIGHTHAND_REQUEST:
                    Debug.Log("Setting Overlay Anchor to Right Hand");
                    _vxmOverlay.ChangeAnchorToRightController();
                    return VXMMessage.ENABLE_OVERLAY_ANCHOR_RIGHTHAND_ACKNOWLEDGE;
                default:
                    Debug.Log($"UNRECOGNISED MESSAGE SENT FROM VXMUSICDESKTOP: {incomingMessage}");
                    return null;
            }

        }
    }
    
    ////////////////////////////////
    ///
    ///
    // IEnumerator WaitForConnectionCoroutine()
    //     {
    //         // This flag will keep the coroutine yielding until connection is established
    //         bool connected = false;
    //
    //         // Run the blocking call on a new thread to avoid freezing Unity's main thread
    //         Thread connectionThread = new Thread(() =>
    //         {
    //             ServerStream.WaitForConnection();
    //             connected = true;
    //         });
    //         connectionThread.Start();
    //
    //         // Keep yielding return until the pipe is connected
    //         while (!connected)
    //         {
    //             yield return null; // This will wait for the next frame before continuing
    //         }
    //
    //         // Continue with your code after the connection is established
    //         OnConnected();
    //     }
    //     
    //     void OnConnected()
    //     {
    //         Debug.Log("Pipe connected.");
    //         
    //         ServerReader = new StreamReader(ServerStream);
    //         ServerWriter = new StreamWriter(ServerStream) { AutoFlush = true };
    //         
    //         // Start the reading coroutine
    //         StartCoroutine(ReadEventMessageFromVxMusicDesktop());
    //     }
    //     
    //     IEnumerator ReadEventMessageFromVxMusicDesktop()
    //     {
    //         // This loop runs as long as the pipe is connected
    //         while (ServerStream.IsConnected)
    //         {
    //             if (ServerReader.Peek() >= 0) // Check if there is data to read
    //             {
    //                 string message = ServerReader.ReadLine();
    //                 if (message != null)
    //                 {
    //                     Debug.Log("Received: " + message);
    //                     // Process received message
    //                     ProcessIncomingVXMusicDesktopEventMessage(message);
    //                 }
    //             }
    //             yield return null; // Wait for the next frame
    //         }
    //     }
    //     
    //     public void WriteEventMessageToVxMusicDesktop(string message)
    //     {
    //         if (ServerStream.IsConnected && ServerWriter != null)
    //         {
    //             ServerWriter.WriteLine(message);
    //             Debug.Log("Sent: " + message);
    //         }
    //         else
    //         {
    //             Debug.LogError("Pipe not connected or writer not initialized.");
    //         }
    //     }
    
}


