/**
 * EasyOpenVROverlayForUnity by gpsnmeajp v0.24
 * 2019/07/15
 * 
 * v0.24 Fixed an issue where it did not work correctly in OpenGL environments.
 *       Commented out showDevices due to potential errors.
 * 
 * v0.23 Added support for Side-by-Side 3D.
 *
 * v0.22 Device updates:
 *      - Retrieve device information and log the list.
 *      - Added the ability to obtain detailed information about selected devices.
 *      - Allows overlays to be placed on trackers and base station positions.
 *      - Suppressed warnings related to Tags.
 * 
 * v0.21 Minor fixes.
 * 
 * v0.2 Major updates:
 *      - Changed debugging tags method.
 *      - Added support for uGUI clicks.
 *      - Added the ability to select controllers.
 *      - Added external error checking and display state management functions.
 *      - Modularized various processes.
 *      - Added processes for releasing resources at the end.
 *      - Added processes for releasing resources on errors.
 *      - Added mouse scaling handling.
 *      - Added catching of exit events.
 * 
 * v0.1 Released on 2018/08/25.
 * 
 * This script allows you to overlay 2D textures onto the VR space, independent of the currently running application.
 * 
 * Input functionality is currently not functioning correctly and has been omitted.
 * Dashboard overlay is also excluded.
 * 
 * These codes are licensed under CC0.
 * http://creativecommons.org/publicdomain/zero/1.0/deed.en
 */

/** About uGUI Clicks
 * 
 * Usage:
 * 1. Set the LaycastRootObject to the Canvas you want to interact with (place it directly under the scene).
 * 2. Only supports Button clicks (tap the Overlay with the controller's tip to click).
 * 3. Set the Raycast Target of the Button to ON. Set the Raycast Target of the Text on the Button to OFF.
 * 4. Set the Canvas's Render Mode to "Screen Space - Camera".
 * 5. The Render Camera of the Canvas should be the same as the Camera that has the RenderTexture set.
 * 
 * Note: Setting LaycastRootObject to null (None) will disable GUI functionality.
 */

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;

namespace Plugins
{
    public class VXMusicOverlay : MonoBehaviour
    {
        //ERROR FLAG
        public bool error = true; //Initialization failed

        //Display logs related to events
        public bool eventLog = false;

        private GameObject VXMusicLogo;
        //private Animator vxMusicLogoAnimationController = GetComponent<Animator>();

        [Header("RenderTexture")]
        //Render Texture to get from
        public RenderTexture renderTexture;

        [Header("Transform")]
        //Unity compliant position and rotation
        public Vector3 Position = new Vector3(0, -0.5f, 3);

        public Vector3 Rotation = new Vector3(0, 0, 0);

        public Vector3 Scale = new Vector3(1, 1, 1);

        //Enable mirror image reversal
        public bool MirrorX = false;
        public bool MirrorY = false;

        [Header("Setting")]
        //Set the overlay size (width only.Height is automatically calculated from the texture ratio)
        [Range(0, 100)]
        public float width = 5.0f;

        //Set overlay transparency
        [Range(0, 1)] public float alpha = 1.0f;

        //Display or not
        public bool show = true;

        //side-by-side 3D
        public bool SideBySide = false;


        [Header("Name")]
        //Name of the overlay for the user to see
        public string OverlayFriendlyName = "VXMusicOverlay";

        //Global key (distinguished name between system overlays).
        // Must be unique. Recommend random numbers, UUIDs, etc.
        public string OverlayKeyName = "VXMusicOverlay";

        [Header("DeviceTracking")]
        //Absolute space?
        public bool DeviceTracking = true;

        //Followed device. HMD=0
        //public uint DeviceIndex = OpenVR.k_unTrackedDeviceIndex_Hmd;
        public TrackingDeviceSelect DeviceIndex = TrackingDeviceSelect.HMD;
        private int DeviceIndexOld = (int)TrackingDeviceSelect.None;

        [Header("Absolute space")]
        //(For absolute space) Room scale or seated state?
        public bool Seated = false;

        //Reset the seated camera (it will automatically return to false after resetting)
        public bool ResetSeatedCamera = false;

        //Follow target list. Special handling because the controller changes
        public enum TrackingDeviceSelect
        {
            None = -99,
            RightController = -2,
            LeftController = -1,
            HMD = (int)OpenVR.k_unTrackedDeviceIndex_Hmd,
            Device1 = 1,
            Device2 = 2,
            Device3 = 3,
            Device4 = 4,
            Device5 = 5,
            Device6 = 6,
            Device7 = 7,
            Device8 = 8,
        }

        //--------------------------------------------------------------------------

        [Header("Device Info")]
        // Output the list of currently connected devices to the log (automatically returns to false)
        public bool putLogDevicesInfo = false;

        // Number of currently connected devices (at the time of device selection)
        public int ConnectedDevices = 0;

        // select device number
        public int SelectedDeviceIndex = 0;

        // serial number of selected device
        public string DeviceSerialNumber = null;

        // Model name of selected device
        public string DeviceRenderModelName = null;


        [Header("GUI Tap")]
        // Root Canvas object for identifying raycast target
        public GameObject LaycastRootObject = null;

        // Tap state management
        public bool tappedLeft = false;
        public bool tappedRight = false;

        // TAP DISTANCE
        public float TapOnDistance = 0.04f;
        public float TapOffDistance = 0.043f;

        // Variable for displaying cursor position
        public float LeftHandU = -1f;
        public float LeftHandV = -1f;
        public float LeftHandDistance = -1f;
        public float RightHandU = -1f;
        public float RightHandV = -1f;
        public float RightHandDistance = -1f;

        [Header("VXMusic State")] 
        public bool IsInRecognitionState = false;

        public ETrackedControllerRole OverlayTrackedDevice;
        public EVRButtonId TrackedDeviceInputLock = EVRButtonId.k_EButton_SteamVR_Trigger;

        public event Action<bool> OnRecognitionStateTriggered;

        private GameObject _VXMusicInterfaceObject;
        private VXMusicInterface _VXMusicInterface;

        private GameObject _recognitionAudioSourceObject;
        private RecognitionAudioTrigger _recognitionAudioSource;

        private float _timeSinceLastHeartbeat = 0f;
        private float _heartbeatInterval = 2f;
        
        // RIGHT HAND OR LEFT HAND
        enum LeftOrRight
        {
            Left = 0,
            Right = 1
        }

        //--------------------------------------------------------------------------

        // Overlay handle (integer)
        private ulong overlayHandle = INVALID_HANDLE;

        // Open VR system instance
        private CVRSystem openvr = null;

        // Overlay instance
        private CVROverlay overlay = null;
        
        // Overlay instance
        private CVRNotifications notifications = null;

        // Native texture to pass to overlay
        private Texture_t overlayTexture;

        // HMD viewpoint position conversion matrix
        private HmdMatrix34_t p;

        // invalid handle
        private const ulong INVALID_HANDLE = 0;

        //--------------------------------------------------------------------------

        // Switch transparency settings from outside
        public void setAlpha(float a)
        {
            alpha = a;
        }

        // Switching device from outside
        public void changeToHMD()
        {
            DeviceIndex = TrackingDeviceSelect.HMD;
        }

        // Switching device from outside
        public void changeToLeftController()
        {
            DeviceIndex = TrackingDeviceSelect.LeftController;
        }

        // Switching device from outside
        public void changeToRightController()
        {
            DeviceIndex = TrackingDeviceSelect.RightController;
        }

        //--------------------------------------------------------------------------

        // Check if Overlay is displayed externally
        public bool IsVisible()
        {
            return overlay.IsOverlayVisible(overlayHandle) && !IsError();
        }

        // Check for error status
        public bool IsError()
        {
            return error || overlayHandle == INVALID_HANDLE || overlay == null || openvr == null;
        }

        // Error processing (release processing)
        private void ProcessError()
        {
#pragma warning disable 0219
            string Tag = "[" + this.GetType().Name + ":" +
                         System.Reflection.MethodBase.GetCurrentMethod(); // クラス名とメソッド名を自動取得
#pragma warning restore 0219
            Debug.Log(Tag + "Begin");

            // release the handle
            if (overlayHandle != INVALID_HANDLE && overlay != null)
            {
                overlay.DestroyOverlay(overlayHandle);
            }

            overlayHandle = INVALID_HANDLE;
            overlay = null;
            openvr = null;
            error = true;
        }

        // When destroying an object
        private void OnDestroy()
        {
#pragma warning disable 0219
            string Tag = "[" + this.GetType().Name + ":" +
                         System.Reflection.MethodBase.GetCurrentMethod(); // automatically obtain class name and method name
#pragma warning restore 0219
            Debug.Log(Tag + "Begin");

            // Fully open handles
            ProcessError();
        }

        // When application termination is detected
        private void OnApplicationQuit()
        {
#pragma warning disable 0219
            string Tag = "[" + this.GetType().Name + ":" +
                         System.Reflection.MethodBase.GetCurrentMethod(); // Automatically obtain class name and method name
#pragma warning restore 0219
            Debug.Log(Tag + "Begin");

            // Fully open handles
            ProcessError();
        }

        // Terminate the application
        private void ApplicationQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }

        //--------------------------------------------------------------------------

        // Initialization process
        private void Start()
        {
#pragma warning disable 0219
            string Tag = "[" + this.GetType().Name + ":" +
                         System.Reflection.MethodBase.GetCurrentMethod(); // Automatically obtain class name and method name
#pragma warning restore 0219
            Debug.Log(Tag + "Begin");

            var openVRError = EVRInitError.None;
            var overlayError = EVROverlayError.None;
            error = false;

            // Set the frame rate to 90 fps. (Otherwise it can become infinitely fast)
            Application.targetFrameRate = 90;
            Debug.Log(Tag + "Set Frame Rate 90");

            // Open VR initialization
            openvr = OpenVR.Init(ref openVRError, EVRApplicationType.VRApplication_Overlay);
            if (openVRError != EVRInitError.None)
            {
                Debug.LogError(Tag + "Open VR initialization failed." + openVRError.ToString());
                ProcessError();
                return;
            }

            // Initializing the overlay function
            overlay = OpenVR.Overlay;
            overlayError = overlay.CreateOverlay(OverlayKeyName, OverlayFriendlyName, ref overlayHandle);
            if (overlayError != EVROverlayError.None)
            {
                Debug.LogError(Tag + "Overlay initialization failed. " + overlayError.ToString());
                ProcessError();
                return;
            }

            // Setting the texture type to be passed to the overlay
            var OverlayTextureBounds = new VRTextureBounds_t();
            var isOpenGL = SystemInfo.graphicsDeviceVersion.Contains("OpenGL");
            if (isOpenGL)
            {
                //pGLuintTexture
                overlayTexture.eType = ETextureType.OpenGL;
                // Not flipped upside down
                OverlayTextureBounds.uMin = 0;
                OverlayTextureBounds.vMin = 0;
                OverlayTextureBounds.uMax = 1;
                OverlayTextureBounds.vMax = 1;
                overlay.SetOverlayTextureBounds(overlayHandle, ref OverlayTextureBounds);
            }
            else
            {
                //pTexture
                overlayTexture.eType = ETextureType.DirectX;
                // flip upside down
                OverlayTextureBounds.uMin = 0;
                OverlayTextureBounds.vMin = 1;
                OverlayTextureBounds.uMax = 1;
                OverlayTextureBounds.vMax = 0;
                overlay.SetOverlayTextureBounds(overlayHandle, ref OverlayTextureBounds);
            }

            //--------
            //showDevices();

            Debug.Log(Tag + "Initialising overlay");
            VXMusicLogo = GameObject.Find("VXMusicOverlayLogo");
            
            _VXMusicInterfaceObject = GameObject.Find("VXMusicInterfacePipe");
            _VXMusicInterface = _VXMusicInterfaceObject.GetComponent<VXMusicInterface>();

            _recognitionAudioSourceObject = GameObject.Find("AudioOutput");
            _recognitionAudioSource = _recognitionAudioSourceObject.GetComponent<RecognitionAudioTrigger>();

            // Notifications
            //notifications = OpenVR.Notifications;
            
            //uint notificationId = 0;
            //NotificationBitmap_t finalBitmap = new NotificationBitmap_t();
            //notifications.CreateNotification(overlayHandle, 0, EVRNotificationType.Transient, "Title", EVRNotificationStyle.Application, ref finalBitmap, ref notificationId);

            //IsVXMusicServerRunning();
            //SendRequestToServerAsync("VX_TRIGGER_RECOGNITION");
        }

        private void Update()
        {
#pragma warning disable 0219
            string Tag = "[" + this.GetType().Name + ":" +
                         System.Reflection.MethodBase.GetCurrentMethod(); // Automatically obtain class name and method name
#pragma warning restore 0219

            // Do not execute if an error occurs or the handle is invalid
            if (IsError())
            {
                return;
            }

            _timeSinceLastHeartbeat += Time.deltaTime;

            if (_timeSinceLastHeartbeat > _heartbeatInterval)
            {
                _VXMusicInterface.SendMessageToServer("VX_HEARTBEAT_REQ");
                _timeSinceLastHeartbeat = 0f;
            }

            if (show)
            {
                // Show overlay
                overlay.ShowOverlay(overlayHandle);
            }
            else
            {
                // Hide overlay
                overlay.HideOverlay(overlayHandle);
            }

            // Handle the event (true when terminated)
            if (ProcessEvent())
            {
                Debug.Log(Tag + "VR system has been terminated");
                _VXMusicInterface.SendMessageToServer("VX_CONNECTION_TERM"); 
                // TODO Does the application quit before this message is received? Possible race condition
                ApplicationQuit();
            }

            // When the overlay is displayed
            if (overlay.IsOverlayVisible(overlayHandle))
            {
                // Update location information and various settings
                updatePosition();
                // Update display information
                updateTexture();

                // If Canvas is set
                if (LaycastRootObject != null)
                {
                    // Handling GUI touch functions
                    updateVRTouch();
                }

                if (_VXMusicInterface.IsInRecognitionState)
                {
                    Debug.Log("VXMusic is in a Recognition State.");
                    if (!_VXMusicInterface.IsAnimationRunning)
                    {
                        ChangeOverlayOpacity(1.0f);
                        _VXMusicInterface.IsAnimationRunning = true;
                        OnRecognitionStateTriggered?.Invoke(_VXMusicInterface.IsInRecognitionState);
                    }
                }
                else
                {
                    Debug.Log("VXMusic is not in a Recognition State.");
                    if (_VXMusicInterface.IsAnimationRunning)
                    {
                        ChangeOverlayOpacity(0.2f);
                        _VXMusicInterface.IsAnimationRunning = false;
                        OnRecognitionStateTriggered?.Invoke(_VXMusicInterface.IsInRecognitionState);
                    }
                }
                    
            }

            if (putLogDevicesInfo)
            {
                showDevices();
                putLogDevicesInfo = false;
            }
        
        }

        private bool IsInputLockButtonPressed()
        {
            VRControllerState_t controllerState = new VRControllerState_t();
            var sizeOfControllerState = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VRControllerState_t));

            OverlayTrackedDevice = ETrackedControllerRole.LeftHand;
            
            // If overlay is tracked to left controller, make the right hand the dominant input device, and visa versa.
            var inputDeviceRelativeToOverlayDevice = OverlayTrackedDevice == ETrackedControllerRole.LeftHand
                ? ETrackedControllerRole.RightHand
                : ETrackedControllerRole.LeftHand;
            
            uint currentInputDeviceId = openvr.GetTrackedDeviceIndexForControllerRole(inputDeviceRelativeToOverlayDevice);

            if (openvr.GetControllerState(currentInputDeviceId, ref controllerState, sizeOfControllerState))
            {
                // Check if the system button is pressed
                bool isPressed = (controllerState.ulButtonPressed & (1UL << (int)EVRButtonId.k_EButton_SteamVR_Trigger)) != 0;
                return isPressed;
            }
            
            Debug.Log("Failed to get controller state.");
            return false; // failed to get controller state.
        }
        

        // Update location information
        private void updatePosition()
        {
#pragma warning disable 0219
            string Tag = "[" + this.GetType().Name + ":" +
                         System.Reflection.MethodBase.GetCurrentMethod(); // Automatically obtain class name and method name
#pragma warning restore 0219

            // Check if Render Texture is generated
            if (!renderTexture.IsCreated())
            {
                Debug.Log(Tag + "Render Texture has not been generated yet");
                return;
            }

            // GENERATE ROTATION
            Quaternion quaternion = Quaternion.Euler(Rotation.x, Rotation.y, Rotation.z);
            // Change the coordinate system (swap between right-handed and left-handed systems)
            Vector3 position = Position;
            position.z = -Position.z;
            // Write to HMD viewpoint position conversion matrix.
            Matrix4x4 m = Matrix4x4.TRS(position, quaternion, Scale);

            // MIRROR IMAGE REVERSAL
            Vector3 Mirroring = new Vector3(MirrorX ? -1 : 1, MirrorY ? -1 : 1, 1);

            // Convert a 4 x 4 matrix to a 3 x 4 matrix.
            p.m0 = Mirroring.x * m.m00;
            p.m1 = Mirroring.y * m.m01;
            p.m2 = Mirroring.z * m.m02;
            p.m3 = m.m03;
            p.m4 = Mirroring.x * m.m10;
            p.m5 = Mirroring.y * m.m11;
            p.m6 = Mirroring.z * m.m12;
            p.m7 = m.m13;
            p.m8 = Mirroring.x * m.m20;
            p.m9 = Mirroring.y * m.m21;
            p.m10 = Mirroring.z * m.m22;
            p.m11 = m.m23;

            // Display relative position based on rotation matrix
            if (DeviceTracking)
            {
                // Handle deviceindex (because controllers etc. change from time to time)
                var idx = OpenVR.k_unTrackedDeviceIndex_Hmd;

                switch (DeviceIndex)
                {
                    case TrackingDeviceSelect.LeftController:
                        idx = openvr.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
                        break;
                    case TrackingDeviceSelect.RightController:
                        idx = openvr.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
                        break;
                    default:
                        idx = (uint)DeviceIndex;
                        break;
                }

                // If there is a change in device information, it will be reflected in the Inspector
                if (DeviceIndexOld != (int)idx)
                {
                    Debug.Log(Tag + "Device Updated");
                    UpdateDeviceInfo(idx);
                    DeviceIndexOld = (int)idx;
                }

                // Display the overlay relative to the HMD.
                overlay.SetOverlayTransformTrackedDeviceRelative(overlayHandle, idx, ref p);
            }
            else
            {
                // Display an overlay at an absolute position in space
                if (!Seated)
                {
                    overlay.SetOverlayTransformAbsolute(overlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding,
                        ref p);
                }
                else
                {
                    overlay.SetOverlayTransformAbsolute(overlayHandle, ETrackingUniverseOrigin.TrackingUniverseSeated,
                        ref p);
                }
            }

            if (ResetSeatedCamera)
            {
                //OpenVR.System.ResetSeatedZeroPose(); // bingy
                ResetSeatedCamera = false;
            }

            // Overlay size settings (width only.Height is automatically calculated from the texture ratio)
            overlay.SetOverlayWidthInMeters(overlayHandle, width);

            // Set overlay transparency
            overlay.SetOverlayAlpha(overlayHandle, alpha);

            // Set the mouse cursor scale (this also determines the size of the display area)
            try
            {
                HmdVector2_t vecMouseScale = new HmdVector2_t
                {
                    v0 = renderTexture.width,
                    v1 = renderTexture.height
                };
                overlay.SetOverlayMouseScale(overlayHandle, ref vecMouseScale);
            }
            catch (UnassignedReferenceException e)
            {
                Debug.LogError(Tag + "RenderTextureがセットされていません " + e.ToString());
                ProcessError();
                return;
            }
        }

        // Update display information
        private void updateTexture()
        {
#pragma warning disable 0219
            string Tag = "[" + this.GetType().Name + ":" +
                         System.Reflection.MethodBase.GetCurrentMethod(); //クラス名とメソッド名を自動取得
#pragma warning restore 0219

            overlay.SetOverlayFlag(overlayHandle, VROverlayFlags.SideBySide_Parallel, SideBySide);

            // Check if RenderTexture is generated
            if (!renderTexture.IsCreated())
            {
                Debug.Log(Tag + "RenderTextureがまだ生成されていない");
                return;
            }

            // Get native texture handle from RenderTexture
            try
            {
                overlayTexture.handle = renderTexture.GetNativeTexturePtr();
            }
            catch (UnassignedReferenceException e)
            {
                Debug.LogError(Tag + "RenderTextureがセットされていません " + e.ToString());
                ProcessError();
                return;
            }

            // Set texture on overlay
            var overlayError = EVROverlayError.None;
            overlayError = overlay.SetOverlayTexture(overlayHandle, ref overlayTexture);
            if (overlayError != EVROverlayError.None)
            {
                Debug.LogError(Tag + "Overlayにテクスチャをセットできませんでした. " + overlayError.ToString());
                // Do not treat it as a fatal error
                return;
            }
        }

        // Return when the end event is caught
        private bool ProcessEvent()
        {
#pragma warning disable 0219
            string Tag = "[" + this.GetType().Name + ":" +
                         System.Reflection.MethodBase.GetCurrentMethod(); //クラス名とメソッド名を自動取得
#pragma warning restore 0219

            // Get the size of the event structure
            uint uncbVREvent = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VREvent_t));

            // Event information storage structure
            VREvent_t Event = new VREvent_t();
            // Retrieve event
            while (overlay.PollNextOverlayEvent(overlayHandle, ref Event, uncbVREvent))
            {
                // View event log
                if (eventLog)
                {
                    Debug.Log(Tag + "Event:" + ((EVREventType)Event.eventType).ToString());
                }

                // Branch by event information
                switch ((EVREventType)Event.eventType)
                {
                    case EVREventType.VREvent_Quit:
                        Debug.Log(Tag + "Quit");
                        return true;
                }
            }

            return false;
        }


        //----------Bonus (device details)-------------

        // Output all device information to log
        private void showDevices()
        {
#pragma warning disable 0219
            string Tag = "[" + this.GetType().Name + ":" +
                         System.Reflection.MethodBase.GetCurrentMethod(); //クラス名とメソッド名を自動取得
#pragma warning restore 0219

            // Get connection status of all devices
            TrackedDevicePose_t[] allDevicePose = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            openvr.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0f, allDevicePose);

            // Count the number of connected devices
            uint connectedDeviceNum = 0;
            for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
            {
                if (allDevicePose[i].bDeviceIsConnected)
                {
                    connectedDeviceNum++;
                }
            }

            // Read device details one by one
            uint connectedDeviceCount = 0;
            for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
            {
                // If connected, increase number of read completions by 1
                if (GetPropertyAndPutLog(i, allDevicePose))
                {
                    connectedDeviceCount++;
                }

                // Exit after reading as many as the number connected.
                if (connectedDeviceCount >= connectedDeviceNum)
                {
                    break;
                }
            }
        }

        // Output device information to log (1 item)
        private bool GetPropertyAndPutLog(uint idx, TrackedDevicePose_t[] allDevicePose)
        {
#pragma warning disable 0219
            string Tag = "[" + this.GetType().Name + ":" +
                         System.Reflection.MethodBase.GetCurrentMethod(); // Automatically obtain class name and method name
#pragma warning restore 0219

            // Check if connected
            if (allDevicePose[idx].bDeviceIsConnected)
            {
                // connected device

                // Obtain the device serial number (often used to identify Tracker) and device model name (device type)
                string s1 = GetProperty(idx, ETrackedDeviceProperty.Prop_SerialNumber_String);
                string s2 = GetProperty(idx, ETrackedDeviceProperty.Prop_RenderModelName_String);
                if (s1 != null && s2 != null)
                {
                    // Show in log
                    Debug.Log(Tag + "Device " + idx + ":" + s1 + " : " + s2);
                }
                else
                {
                    // Acquisition failed for some reason
                    Debug.Log(Tag + "Device " + idx + ": Error");
                }

                return true;
            }
            else
            {
                // unconnected device
                Debug.Log(Tag + "Device " + idx + ": Not connected");
                return false;
            }
        }

        // Reflect the information of the specified device in the Inspector
        private void UpdateDeviceInfo(uint idx)
        {
            // Get connection status of all devices
            TrackedDevicePose_t[] allDevicePose = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            openvr.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0f, allDevicePose);

            // Count the number of connected devices
            ConnectedDevices = 0;
            for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
            {
                if (allDevicePose[i].bDeviceIsConnected)
                {
                    ConnectedDevices++;
                }
            }

            // Reflect device information to Inspector
            SelectedDeviceIndex = (int)idx;
            DeviceSerialNumber = GetProperty(idx, ETrackedDeviceProperty.Prop_SerialNumber_String);

            try
            {
                DeviceRenderModelName = GetProperty(idx, ETrackedDeviceProperty.Prop_RenderModelName_String); // bingy
            }
            catch (Exception e)
            {
                Debug.LogWarning("Maybe the device model name couldn't be found?");
                Debug.LogWarning(e);
            }
        }

        // Get device information
        private string GetProperty(uint idx, ETrackedDeviceProperty prop)
        {
#pragma warning disable 0219
            string Tag = "[" + this.GetType().Name + ":" +
                         System.Reflection.MethodBase.GetCurrentMethod(); // Automatically obtain class name and method name
#pragma warning restore 0219

            ETrackedPropertyError error = new ETrackedPropertyError();
            // Get the number of characters required to get device information
            uint size = openvr.GetStringTrackedDeviceProperty(idx, prop, null, 0, ref error);
            if (error != ETrackedPropertyError.TrackedProp_BufferTooSmall)
            {
                return null;
            }

            StringBuilder s = new StringBuilder();
            s.Length = (int)size; // Ensure character length
            // Get device information
            openvr.GetStringTrackedDeviceProperty(idx, prop, s, size, ref error);
            if (error != ETrackedPropertyError.TrackedProp_Success)
            {
                return null;
            }

            return s.ToString();
        }


        //----------Bonus (you can hit Overlay with a controller and click uGUI)-------------

        //Realize uGUI click
        private void updateVRTouch()
        {
#pragma warning disable 0219
            string Tag = "[" + this.GetType().Name + ":" +
                         System.Reflection.MethodBase.GetCurrentMethod(); // Automatically obtain class name and method name
#pragma warning restore 0219

            //controller index

            // Get information for all VR connected devices
            TrackedDevicePose_t[] allDevicePose = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            openvr.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0f, allDevicePose);

            // Variable to store Overlay ray scan results
            VROverlayIntersectionResults_t results = new VROverlayIntersectionResults_t();

            /*
        // Control by line of sight
        uint Hmdidx = OpenVR.k_unTrackedDeviceIndex_Hmd;
        if (checkRay(Hmdidx, allDevicePose, ref results))
        {
            parent.setCursorPosition(results, LeftOrRight.Right, channel);
            Debug.Log(DEBUG_TAG + "HMD u:"+results.vUVs.v0+" v:"+ results.vUVs.v1+" d:"+results.fDistance);
        }
        */
            // Get left hand controller information
            uint Leftidx = openvr.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
            if (checkRay(Leftidx, allDevicePose, ref results))
            {
                // If there is an overlay on the line, continue processing
                CheckTapping(results, LeftOrRight.Left, ref tappedLeft);

                // Updated for cursor display
                LeftHandU = results.vUVs.v0 * renderTexture.width;
                LeftHandV = renderTexture.height - results.vUVs.v1 * renderTexture.height;
                LeftHandDistance = results.fDistance;
            }
            else
            {
                LeftHandU = -1f;
                LeftHandV = -1f;
                LeftHandDistance = -1f;
            }

            // Get right hand controller information
            uint Rightidx = openvr.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
            if (checkRay(Rightidx, allDevicePose, ref results))
            {
                // If there is an overlay on the line, continue processing
                CheckTapping(results, LeftOrRight.Right, ref tappedRight);

                // Updated for cursor display
                RightHandU = results.vUVs.v0 * renderTexture.width;
                RightHandV = renderTexture.height - results.vUVs.v1 * renderTexture.height;
                RightHandDistance = results.fDistance;
            }
            else
            {
                RightHandU = -1f;
                RightHandV = -1f;
                RightHandDistance = -1f;
            }
        }

        // Checks if the specified device is valid and then checks if it has an intersection with the overlay
        private bool checkRay(uint idx, TrackedDevicePose_t[] allDevicePose, ref VROverlayIntersectionResults_t results)
        {
#pragma warning disable 0219
            string Tag = "[" + this.GetType().Name + ":" +
                         System.Reflection.MethodBase.GetCurrentMethod(); // Automatically obtain class name and method name
#pragma warning restore 0219

            //device index enabled
            if (idx != OpenVR.k_unTrackedDeviceIndexInvalid)
            {
                //Connected and posture information valid
                if (allDevicePose[idx].bDeviceIsConnected && allDevicePose[idx].bPoseIsValid)
                {
                    // Have posture information etc. converted
                    TrackedDevicePose_t Pose = allDevicePose[idx];
                    SteamVR_Utils.RigidTransform Trans = new SteamVR_Utils.RigidTransform(Pose.mDeviceToAbsoluteTracking);

                    // Calculate direction vector tilted forward 45 degrees for controller
                    Vector3 vect = (Trans.rot * Quaternion.AngleAxis(45, Vector3.right)) * Vector3.forward;
                    // Bingy this looks like where the top of the controller is set
                    // Trans.Pos is the left hand, vect is the right
                    return ComputeOverlayIntersection(Trans.pos, vect, ref results);
                }
            }

            return false;
        }

        // Check if it has an intersection with the overlay
        private bool ComputeOverlayIntersection(Vector3 pos, Vector3 rotvect, ref VROverlayIntersectionResults_t results)
        {
#pragma warning disable 0219
            string Tag = "[" + this.GetType().Name + ":" +
                         System.Reflection.MethodBase.GetCurrentMethod(); // Automatically obtain class name and method name
#pragma warning restore 0219

            // Ray irradiation information
            VROverlayIntersectionParams_t param = new VROverlayIntersectionParams_t();
            // Ray launcher position
            param.vSource = new HmdVector3_t
            {
                v0 = pos.x, // bingy change this here
                v1 = pos.y,
                v2 = -pos.z // right-handed to left-handed
            };
            // Ray firing unit direction vector
            param.vDirection = new HmdVector3_t
            {
                v0 = rotvect.x,
                v1 = rotvect.y,
                v2 = -rotvect.z // right-handed to left-handed
            };
            // Irradiation in room space coordinate system
            param.eOrigin = ETrackingUniverseOrigin.TrackingUniverseStanding;

            // True if it intersects with Overlay, false if not, detailed information is entered in results
            return overlay.ComputeOverlayIntersection(overlayHandle, ref param, ref results);
        }

        // Check if tapped
        private void CheckTapping(VROverlayIntersectionResults_t results, LeftOrRight lr, ref bool tapped)
        {
#pragma warning disable 0219
            string Tag = "[" + this.GetType().Name + ":" +
                         System.Reflection.MethodBase.GetCurrentMethod(); // Automatically obtain class name and method name
#pragma warning restore 0219

            // If the distance between the controller and the overlay is below a certain level
            if (results.fDistance < TapOnDistance && !tapped)
            {
                // Tapped
                tapped = true;
                PerformHapticFeedback(lr);

                // Click processing
                uGUIclick(results);
            }

            // If the distance between the controller and the overlay is above a certain level
            if (results.fDistance > TapOffDistance && tapped)
            {
                // Distant
                tapped = false;
                PerformHapticFeedback(lr);

                if (IsInputLockButtonPressed() && !_VXMusicInterface.IsInRecognitionState)
                {
                    
                    _recognitionAudioSource.recognitionAudioSource.Play();
                    _VXMusicInterface.SendMessageToServer("VX_RECOGNITION_REQ"); 
                }
            }
        }

        private void ChangeOverlayOpacity(float alpha)
        {
            // bingy
            setAlpha(alpha);
        }


        // Perform vibration feedback
        private void PerformHapticFeedback(LeftOrRight lr)
        {
#pragma warning disable 0219
            string Tag = "[" + this.GetType().Name + ":" +
                         System.Reflection.MethodBase.GetCurrentMethod(); // Automatically obtain class name and method name
#pragma warning restore 0219

            // Check if left hand controller is enabled
            uint Leftidx = openvr.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
            if (Leftidx != OpenVR.k_unTrackedDeviceIndexInvalid && lr == LeftOrRight.Left)
            {
                // blut???
                openvr.TriggerHapticPulse(Leftidx, 0, 3000);
            }

            // Check if right hand controller is enabled
            uint Rightidx = openvr.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
            if (Rightidx != OpenVR.k_unTrackedDeviceIndexInvalid && lr == LeftOrRight.Right)
            {
                // blut???
                openvr.TriggerHapticPulse(Rightidx, 0, 3000);
            }
        }

        // Identify and click an element on Canvas
        private void uGUIclick(VROverlayIntersectionResults_t results)
        {
#pragma warning disable 0219
            string Tag = "[" + this.GetType().Name + ":" +
                         System.Reflection.MethodBase.GetCurrentMethod(); // Automatically obtain class name and method name
#pragma warning restore 0219

            // Calculate uv coordinates for click
            float u = results.vUVs.v0 * renderTexture.width;
            float v = renderTexture.height - results.vUVs.v1 * renderTexture.height;

            // Set coordinates for raycast on Canvas
            Vector2 ScreenPoint = new Vector2(u, v);
            PointerEventData pointer = new PointerEventData(EventSystem.current)
            {
                position = ScreenPoint
            };

            // Secure list for storing raycast results
            List<RaycastResult> result = new List<RaycastResult>();

            // RaycastAll hits the raycaster.
            // Hit all the raycasters attached to the Canvas and Camera (it's a good idea to cut off the ones you don't need)
            EventSystem.current.RaycastAll(pointer, result);

            // Number and coordinates of detected elements

            Debug.Log(Tag + "count:" + result.Count + " u:" + u + " / v:" + v);

            // Perform click processing on the first element found
            for (int i = 0; i < result.Count; i++)
            {
                var res = result[i];

                // Hit the first thing you hooked (uncheck everything other than target)
                if (res.isValid)
                {
                    Debug.Log(Tag + res.gameObject.name + " at " + res.gameObject.transform.root.name);
                    // Check if it is a child of the root object you want to target
                    if (res.gameObject.transform.root.name == LaycastRootObject.name)
                    {
                        ExecuteEvents.Execute(res.gameObject, pointer, ExecuteEvents.pointerClickHandler);
                        break;
                    }
                }
            }
        }

        // public async Task IsVXMusicServerRunning()
        // {
        //     // Create and connect the ClientStream
        //     ClientStream = new NamedPipeClientStream(".", "VXMusicOverlayEventPipe", PipeDirection.InOut);
        //     ClientStream.Connect();
        //
        //     // Initialize ClientReader and ClientWriter
        //     ClientReader = new StreamReader(ClientStream);
        //     ClientWriter = new StreamWriter(ClientStream) { AutoFlush = true };
        //
        //     string messageToSend = "VX_CONNECT_REQ";
        //     ClientWriter.WriteLine(messageToSend);
        //     ClientStream.WaitForPipeDrain();
        //
        //     // Read the response from the .NET server
        //     string response = ClientReader.ReadLine();
        //
        //     if (response == "VX_CONNECT_ACK")
        //     {
        //         Debug.Log("Received response from .NET: " + response);
        //     }
        //     
        //     ClientStream.Close();
        //     ClientStream = null;
        //     
        //     ClientReader.Close();
        //     ClientReader = null;
        //     ClientWriter.Close();
        //     ClientWriter = null;
        //
        //     // string eventData;
        //     // // THis is likely where the connectino is failing
        //     // while ((eventData = await ClientReader.ReadLineAsync()) != null)
        //     // {
        //     //     Console.WriteLine($"Received event from Unity: {eventData}");
        //     //     
        //     //     switch (response)
        //     //     {
        //     //         case "VX_RECOGNITION_ACK":
        //     //             IsInRecognitionState = true;
        //     //             //OnRecognitionStateTriggered?.Invoke(IsInRecognitionState);
        //     //             continue;
        //     //         case "VX_RECOGNITION_FIN":
        //     //             IsInRecognitionState = false;
        //     //             //OnRecognitionStateTriggered?.Invoke(IsInRecognitionState);
        //     //             continue;
        //     //         default:
        //     //             Console.WriteLine("UNRECOGNISED MESSAGE SENT FROM VXM");
        //     //             continue;
        //     //     }
        //     //         
        //     //     //await writer.WriteLineAsync(line);
        //     // }
        //
        //     //Task.Run(() => ListenForServerResponses());
        // }
    
        // private async Task ListenForServerResponses()
        // {
        //     while (ClientStream != null && ClientStream.IsConnected)
        //     {
        //         string response = ClientReader.ReadLine();
        //
        //         switch (response)
        //         {
        //             case "VX_RECOGNITION_ACK":
        //                 IsInRecognitionState = true;
        //                 //OnRecognitionStateTriggered?.Invoke(IsInRecognitionState);
        //                 continue;
        //             case "VX_RECOGNITION_FIN":
        //                 IsInRecognitionState = false;
        //                 //OnRecognitionStateTriggered?.Invoke(IsInRecognitionState);
        //                 continue;
        //             default:
        //                 Console.WriteLine("UNRECOGNISED MESSAGE SENT FROM VXM");
        //                 continue;
        //         }
        //     }
        // }
    
        // public async Task SendRequestToServer(string request)
        // {
        //     // if (ClientStream != null || ClientStream.IsConnected)
        //     // {
        //     //     ClientStream.Close();
        //     // }
        //
        //     ClientStream = new NamedPipeClientStream(".", "VXMusicOverlayEventPipe", PipeDirection.InOut);
        //     ClientStream.Connect();
        //     
        //     //ClientStream.Connect();
        //     // This is also fucked
        //     ClientWriter = new StreamWriter(ClientStream) { AutoFlush = true };
        //     ClientWriter.WriteLine(request);
        //     //ClientWriter.Flush();
        //     
        //     ClientReader = new StreamReader(ClientStream);
        //
        //     // Read the response from the .NET server
        //     Task<string> response = ClientReader.ReadLineAsync();
        //
        //     if (response.Result == "VX_RECOGNITION_ACK")
        //     {
        //         Debug.Log("Received REC ACK response from .NET: " + response);
        //         // ChangeOverlayOpacity(1.0f);
        //         IsInRecognitionState = true;
        //         OnRecognitionStateTriggered?.Invoke(IsInRecognitionState);
        //     }
        //     
        //     //ClientStream.
        //     
        //     response = ClientReader.ReadLineAsync();
        //     
        //     if (response.Result == "VX_RECOGNITION_FIN")
        //     {
        //         Debug.Log("Received REC FIN response from .NET: " + response);
        //         //ChangeOverlayOpacity(0.2f);
        //         IsInRecognitionState = false;
        //         OnRecognitionStateTriggered?.Invoke(IsInRecognitionState);
        //     }
        // }

        // public void SendRecognitionRequestToVXMusic()
        // {
        //     Debug.Log("Sending Recognition Request to VXMusic");
        //
        //
        //     string messageToSend = "VX_TRIGGER_RECOGNITION";
        //     ClientWriter.WriteLine(messageToSend);
        //     ClientWriter.Flush();
        //
        //     // Read the response from the .NET server
        //     //ClientReader.BaseStream.Seek(0, SeekOrigin.Begin);
        //     string response = ClientReader.ReadLine();
        //
        //     if (response == "VX_RECOGNITION_ACK")
        //     {
        //         Debug.Log("Received response from .NET: " + response);
        //
        //     }
        // }
    }
}