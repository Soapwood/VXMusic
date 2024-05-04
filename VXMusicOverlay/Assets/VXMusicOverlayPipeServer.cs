// using System;
// using System.Collections;
// using System.IO;
// using System.IO.Pipes;
// using System.Threading;
// using Plugins.Messaging;
// using UnityEngine;
//
// namespace Plugins
// {
//     public class VXMusicOverlayPipeServer : MonoBehaviour
//     {
//         private VXMusicOverlay _vxmOverlay;
//         private Thread OverlayServerThread;
//         
//         void Start()
//         {
//             GameObject _vxmOverlayGameObject = GameObject.Find("OverlayScriptWrapper");
//
//             if (_vxmOverlayGameObject == null)
//                 Debug.Log("VXMOverlay GameObject was not found.");
//             
//             _vxmOverlay = _vxmOverlayGameObject.GetComponent<VXMusicOverlay>();
//             
//             OverlayServerThread = new Thread(ServerThread);
//             OverlayServerThread.Start();
//         }
//
//         void ServerThread()
//         {
//             while (true)
//             {
//                 var serverStream = new NamedPipeServerStream(VXMMessage.VxMusicOverlaySettingsPipe, PipeDirection.InOut);
//                 Debug.Log("Waiting for connection...");
//                 serverStream.WaitForConnection();
//                 Debug.Log("Connected.");
//                     
//                 StreamReader reader = new StreamReader(serverStream);
//                 StreamWriter writer = new StreamWriter(serverStream) { AutoFlush = true };
//
//                 try
//                 {
//                     // Handle reading and writing in a loop or per protocol
//                     while (serverStream.IsConnected)
//                     {
//                         if (reader.Peek() >= 0)
//                         {
//                             string message = reader.ReadLine();
//                             Debug.Log("Received Message from VXMusic Desktop: " + message);
//                                 
//                             string responseMessage = VXMessageHandler.ProcessIncomingVXMusicDesktopEventMessage(_vxmOverlay, message);
//                                 
//                             Debug.Log("Sending Message to VXMusic Desktop: " + responseMessage);
//                             writer.WriteLine(responseMessage);
//                             //Thread.Sleep(200);
//                             break;
//                         }
//                     }
//                 }
//                 catch (IOException e)
//                 {
//                     Debug.LogError("Pipe error: " + e.Message);
//                 }
//                 finally
//                 {
//                     if (serverStream.IsConnected)
//                         serverStream.Disconnect();
//                 }
//             }
//         }
//         
//         void OnDestroy()
//         {
//             OverlayServerThread?.Abort();
//         }
//     }
// }
//
//
