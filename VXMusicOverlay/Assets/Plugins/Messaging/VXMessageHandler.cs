using UnityEngine;
using Valve.VR;

namespace Plugins.Messaging
{
    // public class VXMessageHandler
    // {
    //     public static string ProcessIncomingVXMusicDesktopEventMessage(VXMusicOverlay vxmOverlay, string incomingMessage)
    //     {
    //         // Grab running instance of VXMusic Overlay
    //         
    //         switch (incomingMessage)
    //         {
    //             case VXMMessage.RECOGNITION_ACKNOWLEDGE:
    //                 Debug.Log("Received REC ACK response from .NET: " + response);
    //                 // ChangeOverlayOpacity(1.0f);
    //                 IsInRecognitionState = true;
    //                 OnRecognitionStateTriggered?.Invoke(IsInRecognitionState);
    //                 return "";
    //             case VXMMessage.RECOGNITION_FINISH:
    //                 return "";
    //             case VXMMessage.ENABLE_OVERLAY_ANCHOR_LEFTHAND_REQUEST:
    //                 // Send the connection ack response back to Unity
    //                 Debug.Log("Setting Overlay Anchor to Left Hand");
    //                 vxmOverlay.ChangeAnchorToLeftController();
    //                 return VXMMessage.ENABLE_OVERLAY_ANCHOR_LEFTHAND_ACKNOWLEDGE;
    //             case VXMMessage.ENABLE_OVERLAY_ANCHOR_RIGHTHAND_REQUEST:
    //                 Debug.Log("Setting Overlay Anchor to Right Hand");
    //                 vxmOverlay.ChangeAnchorToRightController();
    //                 return VXMMessage.ENABLE_OVERLAY_ANCHOR_RIGHTHAND_ACKNOWLEDGE;
    //             default:
    //                 Debug.Log($"UNRECOGNISED MESSAGE SENT FROM VXMUSICDESKTOP: {incomingMessage}");
    //                 return null;
    //         }
    //     }
    // }
}