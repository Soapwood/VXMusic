using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using Valve.VR;
//using NLog;
//using Logger = NLog.Logger;

namespace Plugins
{
    public class VXMusicOverlay : MonoBehaviour
    {
        //private static Logger _logger = LogManager.GetCurrentClassLogger();

        #region Script Fields
        [Header("Debug")]
        public bool EnableOverlayDebugView = false;
        
        [FormerlySerializedAs("error")] public bool OverlayInitialisationError = false;

        //Display logs related to events
        [FormerlySerializedAs("eventLog")] public bool EventLog = false;
        
        [FormerlySerializedAs("SpherePrefab")] [FormerlySerializedAs("spherePrefab")] [SerializeField] private GameObject DebugSpherePrefab;
        private GameObject LeftDebugSphere;
        private GameObject RightDebugSphere;

        [FormerlySerializedAs("renderTexture")] [Header("Overlay Render Texture")]
        //Render Texture to get from
        public RenderTexture RenderTexture;

        [FormerlySerializedAs("Position")] [Header("Transform")]
        //Unity compliant position and rotation
        public Vector3 OverlayPosition = new Vector3(0, -0.5f, 3);

        [FormerlySerializedAs("Rotation")] public Vector3 OverlayRotation = new Vector3(0, 0, 0);

        public Vector3 Scale = new Vector3(1, 1, 1);

        //Enable mirror image reversal
        public bool MirrorX = false;
        public bool MirrorY = false;

        [FormerlySerializedAs("width")]
        [Header("Overlay Canvas Settings")]
        [Range(0, 100)] public float Width = 0.2f;
        [FormerlySerializedAs("alpha")] [Range(0, 1)] public float OverlayAlpha = 0.2f;
        [FormerlySerializedAs("_overlayInactiveOpacity")] [Range(0, 1)] public float InactiveOverlayOpacity = 0.2f;
        [FormerlySerializedAs("_overlayActiveOpacity")] [Range(0, 1)] public float ActiveOverlayOpacity = 1.0f;
        [FormerlySerializedAs("show")] public bool ShowOverlay = true;
        public bool SideBySide = false;
        public int ApplicationTargetFramerate = 90;


        [Header("OpenVR Overlay Settings")]
        public string OverlayFriendlyName = "VXMusicOverlay";
        public string OverlayKeyName = "VXMusicOverlay";

        [Header("Device Tracking")]
        public bool DeviceTracking = true;
        [FormerlySerializedAs("OverlayAnchorDevice")] [FormerlySerializedAs("DeviceIndex")] public TrackingDeviceSelect CurrentTrackedDevice = TrackingDeviceSelect.HMD;
        private int DeviceIndexOld = (int)TrackingDeviceSelect.None;

        [Header("Absolute Space")]
        public bool Seated = false;
        public bool ResetSeatedCamera = false;
        
        [FormerlySerializedAs("putLogDevicesInfo")] [Header("Device Info")]
        // Output the list of currently connected devices to the log (automatically returns to false)
        public bool OutputDeviceInfoLogs = false;
        public int ConnectedDevices = 0;
        public int SelectedDeviceIndex = 0;
        public string DeviceSerialNumber = null;
        public string DeviceRenderModelName = null;

        [Header("VXMusic State")] 
        public bool IsInRecognitionState = false;

        public ETrackedControllerRole OverlayTrackedDevice;
        public EVRButtonId TrackedDeviceInputLock = EVRButtonId.k_EButton_SteamVR_Trigger;
        [FormerlySerializedAs("_heartbeatInterval")] public float HeartbeatInterval = 2.0f;
        
        [Header("GUI Tap")]
        [FormerlySerializedAs("LaycastRootObject")] public GameObject RaycastRootObject = null;

        public float TapOnDistance = 0.04f;
        public float TapOffDistance = 0.043f;
        
        public float FingerOffsetX = 0.02f;
        public float FingerOffsetY = -0.04f;
        public float FingerOffsetZ = 0.01f;
        
        public float FingerRotationOffsetX = 80;
        public float FingerRotationOffsetY = 25;
        public float FingerRotationOffsetZ = 0;

        [FormerlySerializedAs("_timeSinceLastHeartbeat")] [Header("Tracked Device Debug")]
        public float TimeSinceLastHeartbeat = 0f;
        [FormerlySerializedAs("tappedLeft")] public bool ClickedLeft = false;
        [FormerlySerializedAs("tappedRight")] public bool ClickedRight = false;
        public float LeftHandU = -1f;
        public float LeftHandV = -1f;
        public float LeftHandDistance = -1f;
        public float RightHandU = -1f;
        public float RightHandV = -1f;
        public float RightHandDistance = -1f;
        
        #endregion
        
        #region Process Events
        public event Action OnRecognitionStateBegin;
        public event Action OnRecognitionStateEnded;
        #endregion
        
        #region Low Level Process Fields
        private ulong _LowLevelOverlayHandle = INVALID_HANDLE;
        // Open VR system instance
        private CVRSystem _openVrSystemApiInstance = null;
        // Overlay instance
        private CVROverlay _openVROverlayInstance = null;
        // Overlay instance
        private CVRNotifications _notifications = null;
        // Native texture to pass to overlay
        private Texture_t _overlayTexture;
        // HMD viewpoint position conversion matrix
        private HmdMatrix34_t p;
        // invalid handle
        private const ulong INVALID_HANDLE = 0;
        #endregion

        #region OpenVR Enums
        // RIGHT HAND OR LEFT HAND // TODO Deprecate this
        enum LeftOrRight
        {
            Left = 0,
            Right = 1
        }

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
        #endregion

        #region Interprocess Instances
        private GameObject _VXMusicInterfaceObject;
        private VXMusicInterface _VXMusicInterface;
        #endregion


        // // // // // // // // // // // // // // // // // // // // // // // // 
        // // // // // // // // // // // // // // // // // // // // // // // // 
        
        #region Events 
        
        private void HandleRecognitionStateBegin()
        {
            Debug.Log("Begin Overlay Recognition State");
            IsInRecognitionState = true;
            
            SetOverlayOpacity(ActiveOverlayOpacity);
        }
        
        private void HandleRecognitionStateEnd()
        {
            Debug.Log("End Overlay Recognition State");
            IsInRecognitionState = false;
            
            SetOverlayOpacity(InactiveOverlayOpacity);
        }

        #endregion

        #region Overlay Initialisation
         private void Start()
        {
#pragma warning disable 0219 // TODO Deprecate this wank - use an actual logger please.
            string Tag = "[" + this.GetType().Name + ":" +
                         System.Reflection.MethodBase.GetCurrentMethod(); // Automatically obtain class name and method name
#pragma warning restore 0219
            
            Debug.Log(Tag + "VXMusic Overlay is Booting");
            OverlayInitialisationError = false;
            
            // Subscribe to VX Event triggers
            Debug.Log("Subscribing to inter-process Events");
            OnRecognitionStateBegin += HandleRecognitionStateBegin;
            OnRecognitionStateEnded += HandleRecognitionStateEnd;
            
            // Instanciate MainThreadDispatcher
            Debug.Log("Instanciating MainThreadDispatcher");
            MainThreadDispatcher.Initialize();
            
            // Create Debug Overlay Markers
            // These will only appear when the EnableOverlayDebugView variable is true
            LeftDebugSphere = Instantiate(DebugSpherePrefab, Vector3.zero, Quaternion.identity);
            LeftDebugSphere.GetComponent<Renderer>().material.color = Color.green;
            RightDebugSphere = Instantiate(DebugSpherePrefab, Vector3.zero, Quaternion.identity);
            RightDebugSphere.GetComponent<Renderer>().material.color = Color.green;

            Debug.Log(Tag + $"Setting Overlay Target Framerate to {ApplicationTargetFramerate}hz.");
            Application.targetFrameRate = ApplicationTargetFramerate;

            InitialiseOpenVrSubsystemInstances();
            InitialiseOverlayTextureBounds();
            
            Debug.Log(Tag + "Initialising overlay");
            
            _VXMusicInterfaceObject = GameObject.Find("VXMusicInterfacePipe");
            _VXMusicInterface = _VXMusicInterfaceObject.GetComponent<VXMusicInterface>();
        }

        private void InitialiseOpenVrSubsystemInstances()
        {
            var openVrErrorContainer = EVRInitError.None;
            var openVrOverlayErrorContainer = EVROverlayError.None;
            
            // Open VR initialization
            _openVrSystemApiInstance = OpenVR.Init(ref openVrErrorContainer, EVRApplicationType.VRApplication_Overlay);
            
            if (openVrErrorContainer != EVRInitError.None)
            {
                Debug.LogError("Open VR initialization failed." + openVrErrorContainer.ToString());
                ProcessError();
                throw new ApplicationException("Open VR initialization failed." + openVrErrorContainer.ToString());
            }

            // Initializing the overlay function
            _openVROverlayInstance = OpenVR.Overlay;
            openVrOverlayErrorContainer = _openVROverlayInstance.CreateOverlay(OverlayKeyName, OverlayFriendlyName, ref _LowLevelOverlayHandle);
            if (openVrOverlayErrorContainer != EVROverlayError.None)
            {
                Debug.LogError("Overlay initialization failed. " + openVrOverlayErrorContainer.ToString());
                ProcessError();
                throw new ApplicationException("Overlay initialization failed." + openVrOverlayErrorContainer.ToString());
            }
        }

        private void InitialiseOverlayTextureBounds()
        {
            // Setting the texture type to be passed to the overlay
            var overlayTextureBounds = new VRTextureBounds_t();
            var isOpenGL = SystemInfo.graphicsDeviceVersion.Contains("OpenGL");
            if (isOpenGL)
            {
                //pGLuintTexture
                _overlayTexture.eType = ETextureType.OpenGL;
                // Not flipped upside down
                overlayTextureBounds.uMin = 0;
                overlayTextureBounds.vMin = 0;
                overlayTextureBounds.uMax = 1;
                overlayTextureBounds.vMax = 1;
                _openVROverlayInstance.SetOverlayTextureBounds(_LowLevelOverlayHandle, ref overlayTextureBounds);
            }
            else
            {
                //pTexture
                _overlayTexture.eType = ETextureType.DirectX;
                // flip upside down
                overlayTextureBounds.uMin = 0;
                overlayTextureBounds.vMin = 1;
                overlayTextureBounds.uMax = 1;
                overlayTextureBounds.vMax = 0;
                _openVROverlayInstance.SetOverlayTextureBounds(_LowLevelOverlayHandle, ref overlayTextureBounds);
            }
        }
        #endregion

        #region Main Overlay Tick Logic
        private void Update()
        {
#pragma warning disable 0219
            string Tag = "[" + this.GetType().Name + ":" +
                         System.Reflection.MethodBase.GetCurrentMethod(); // Automatically obtain class name and method name
#pragma warning restore 0219

            // Do not execute if an error occurs or the handle is invalid
            if (IsError())
                return;

            TimeSinceLastHeartbeat += Time.deltaTime;

            if (TimeSinceLastHeartbeat > HeartbeatInterval)
            {
                _VXMusicInterface.SendHeartbeatMessageToDesktopClient(VXMMessage.CONNECTION_HEARTBEAT_REQUEST);
                TimeSinceLastHeartbeat = 0f;
            }

            if (ShowOverlay)
            {
                // Show overlay
                _openVROverlayInstance.ShowOverlay(_LowLevelOverlayHandle);
            }
            else
            {
                // Hide overlay
                _openVROverlayInstance.HideOverlay(_LowLevelOverlayHandle);
            }

            // Handle termination event
            if (IsQuitEventPresentInOpenVrSubsystem())
            {
                Debug.Log(Tag + "VR system has been terminated");
                ApplicationQuit();
            }

            // When the overlay is displayed
            if (_openVROverlayInstance.IsOverlayVisible(_LowLevelOverlayHandle))
            {
                // Update location information and various settings
                UpdateOverlayPosition();
                // Update display information
                UpdateOverlayTexture();

                // If Canvas is set
                if (RaycastRootObject != null)
                {
                    // Handling overlay touch functions
                    VrInputTick();
                }
            }

            if (OutputDeviceInfoLogs)
            {
                LogDeviceInfo();
                OutputDeviceInfoLogs = false;
            }
        
        }
        
                // Update location information
        private void UpdateOverlayPosition()
        {
#pragma warning disable 0219
            string Tag = "[" + this.GetType().Name + ":" +
                         System.Reflection.MethodBase.GetCurrentMethod(); // Automatically obtain class name and method name
#pragma warning restore 0219

            // Check if Render Texture is generated
            if (!RenderTexture.IsCreated())
            {
                Debug.Log(Tag + "Render Texture has not been generated yet");
                return;
            }

            // GENERATE ROTATION
            Quaternion quaternion = Quaternion.Euler(OverlayRotation.x, OverlayRotation.y, OverlayRotation.z);
            // Change the coordinate system (swap between right-handed and left-handed systems)
            Vector3 position = OverlayPosition;
            position.z = -OverlayPosition.z;
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

                switch (CurrentTrackedDevice)
                {
                    case TrackingDeviceSelect.LeftController:
                        idx = _openVrSystemApiInstance.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
                        break;
                    case TrackingDeviceSelect.RightController:
                        idx = _openVrSystemApiInstance.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
                        break;
                    default:
                        idx = (uint)CurrentTrackedDevice;
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
                _openVROverlayInstance.SetOverlayTransformTrackedDeviceRelative(_LowLevelOverlayHandle, idx, ref p);
            }
            else
            {
                // Display an overlay at an absolute position in space
                if (!Seated)
                {
                    _openVROverlayInstance.SetOverlayTransformAbsolute(_LowLevelOverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding,
                        ref p);
                }
                else
                {
                    _openVROverlayInstance.SetOverlayTransformAbsolute(_LowLevelOverlayHandle, ETrackingUniverseOrigin.TrackingUniverseSeated,
                        ref p);
                }
            }

            if (ResetSeatedCamera)
            {
                //OpenVR.System.ResetSeatedZeroPose(); // bingy
                ResetSeatedCamera = false;
            }

            // Overlay size settings (width only.Height is automatically calculated from the texture ratio)
            _openVROverlayInstance.SetOverlayWidthInMeters(_LowLevelOverlayHandle, Width);

            // Set overlay transparency
            _openVROverlayInstance.SetOverlayAlpha(_LowLevelOverlayHandle, OverlayAlpha);

            // Set the mouse cursor scale (this also determines the size of the display area)
            try
            {
                HmdVector2_t vecMouseScale = new HmdVector2_t
                {
                    v0 = RenderTexture.width,
                    v1 = RenderTexture.height
                };
                _openVROverlayInstance.SetOverlayMouseScale(_LowLevelOverlayHandle, ref vecMouseScale);
            }
            catch (UnassignedReferenceException e)
            {
                Debug.LogError(Tag + "RenderTexture is not set " + e.ToString());
                ProcessError();
                return;
            }
        }

        // Update display information
        private void UpdateOverlayTexture()
        {
#pragma warning disable 0219
            string Tag = "[" + this.GetType().Name + ":" +
                         System.Reflection.MethodBase.GetCurrentMethod();
#pragma warning restore 0219

            _openVROverlayInstance.SetOverlayFlag(_LowLevelOverlayHandle, VROverlayFlags.SideBySide_Parallel, SideBySide);

            // Check if RenderTexture is generated
            if (!RenderTexture.IsCreated())
            {
                Debug.Log(Tag + "RenderTexture has not been generated yet.");
                return;
            }

            // Get native texture handle from RenderTexture
            try
            {
                _overlayTexture.handle = RenderTexture.GetNativeTexturePtr();
            }
            catch (UnassignedReferenceException e)
            {
                Debug.LogError(Tag + "RenderTexture is not set " + e.ToString());
                ProcessError();
                return;
            }

            // Set texture on overlay
            var overlayError = EVROverlayError.None;
            overlayError = _openVROverlayInstance.SetOverlayTexture(_LowLevelOverlayHandle, ref _overlayTexture);
            if (overlayError != EVROverlayError.None)
            {
                Debug.LogError(Tag + "Overlay could not set texture." + overlayError.ToString());
                // Do not treat it as a fatal error
                return;
            }
        }
        
        #endregion

        #region High Level Overlay State

        public void ChangeAnchorToLeftController()
        {
            CurrentTrackedDevice = TrackingDeviceSelect.LeftController;
            OverlayTrackedDevice = ETrackedControllerRole.LeftHand;
            TrackedDeviceInputLock = EVRButtonId.k_EButton_SteamVR_Trigger;
            
            
            OverlayPosition.x = 0.02f;
            OverlayPosition.y = -0.01f;
            OverlayPosition.z = -0.112f;
            OverlayRotation.x = 180;
            OverlayRotation.y = 270;
            OverlayRotation.z = 16;

            FingerOffsetX = 0.02f;
            FingerOffsetY = -0.04f;
            FingerOffsetZ = 0.01f;

            FingerRotationOffsetX = 80;
            FingerRotationOffsetY = 25;
            FingerRotationOffsetZ = 0;
        }

        // Switching device from outside
        public void ChangeAnchorToRightController()
        {
            CurrentTrackedDevice = TrackingDeviceSelect.RightController;
            OverlayTrackedDevice = ETrackedControllerRole.RightHand;
            TrackedDeviceInputLock = EVRButtonId.k_EButton_SteamVR_Trigger;
            
            OverlayPosition.x = -0.02f;
            OverlayPosition.y = -0.01f;
            OverlayPosition.z = -0.112f;
            OverlayRotation.x = 180;
            OverlayRotation.y = -270;
            OverlayRotation.z = -27;
            
            FingerOffsetX = -0.02f;
            FingerOffsetY = -0.04f;
            FingerOffsetZ = 0.01f;

            FingerRotationOffsetX = 80;
            FingerRotationOffsetY = 25;
            FingerRotationOffsetZ = 0;
        }
        
        public void SetOverlayOpacity(float a)
        {
            OverlayAlpha = a;
        }
        
        public void TriggerOnRecognitionStateEnded()
        {
            OnRecognitionStateEnded?.Invoke();
        }

        #endregion
        
        #region Logging

                // Output all device information to log
        private void LogDeviceInfo()
        {
#pragma warning disable 0219
            string Tag = "[" + this.GetType().Name + ":" +
                         System.Reflection.MethodBase.GetCurrentMethod(); //クラス名とメソッド名を自動取得
#pragma warning restore 0219

            // Get connection status of all devices
            TrackedDevicePose_t[] allDevicePose = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            _openVrSystemApiInstance.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0f, allDevicePose);

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

        #endregion

        #region VR Input
        
        private bool IsInputLockButtonPressed()
        {
            VRControllerState_t controllerState = new VRControllerState_t();
            var sizeOfControllerState = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VRControllerState_t));

            uint currentInputDeviceId = _openVrSystemApiInstance.GetTrackedDeviceIndexForControllerRole(OverlayTrackedDevice);

            if (_openVrSystemApiInstance.GetControllerState(currentInputDeviceId, ref controllerState, sizeOfControllerState))
            {
                // Check if specified lock button is pressed.
                return (controllerState.ulButtonPressed & (1UL << (int)TrackedDeviceInputLock)) != 0;
            }
            
            Debug.Log("Failed to get controller state.");
            return false; 
        }
        
        private void VrInputTick()
        {
#pragma warning disable 0219
            string Tag = "[" + this.GetType().Name + ":" +
                         System.Reflection.MethodBase.GetCurrentMethod(); // Automatically obtain class name and method name
#pragma warning restore 0219
            
            // Get information for all VR connected devices
            TrackedDevicePose_t[] allDevicePose = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            _openVrSystemApiInstance.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0f, allDevicePose);

            // Variable to store Overlay ray scan results
            VROverlayIntersectionResults_t results = new VROverlayIntersectionResults_t();
            
            // Get left hand controller information
            uint Leftidx = _openVrSystemApiInstance.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);

            // Calulate origin of Ray for simulated finger
            if (Leftidx != OpenVR.k_unTrackedDeviceIndexInvalid)
            {
                SteamVR_Utils.RigidTransform leftControllerTransform = new SteamVR_Utils.RigidTransform(allDevicePose[Leftidx].mDeviceToAbsoluteTracking);

                if(EnableOverlayDebugView)
                    LeftDebugSphere.transform.position = leftControllerTransform.pos;

                // Calculate finger offset from controller
                Vector3 fingerOffset = new Vector3(FingerOffsetX, FingerOffsetY, FingerOffsetZ); // Tweak these offsets!
                Quaternion fingerRotationOffset = Quaternion.Euler(FingerRotationOffsetX, FingerRotationOffsetY, FingerRotationOffsetZ); // Assuming the finger points slightly differently
                Vector3 fingerPosition = leftControllerTransform.TransformPoint(fingerOffset);
                Quaternion fingerRotation = leftControllerTransform.rot * fingerRotationOffset;
                
                if(EnableOverlayDebugView)
                    Debug.DrawLine(fingerPosition, fingerPosition + (fingerRotation * Vector3.forward) * 2.0f, Color.blue);
                
                // Check for intersection
                if (IsRaycastIntersectingWithOverlayCanvas(Leftidx, fingerPosition, fingerRotation * Vector3.forward, ref results))
                {
                    // If there is an overlay on the line, continue processing
                    IsRaycastInClickingRange(results, LeftOrRight.Left, ref ClickedLeft);

                    // Updated for cursor display
                    LeftHandU = results.vUVs.v0 * RenderTexture.width;
                    LeftHandV = RenderTexture.height - results.vUVs.v1 * RenderTexture.height;
                    LeftHandDistance = results.fDistance;
                }
                else
                {
                    LeftHandU = -1f;
                    LeftHandV = -1f;
                    LeftHandDistance = -1f;
                }
            }

            // Get right hand controller information
            uint Rightidx = _openVrSystemApiInstance.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
            if (Rightidx != OpenVR.k_unTrackedDeviceIndexInvalid)
            {
                SteamVR_Utils.RigidTransform rightControllerTransform = new SteamVR_Utils.RigidTransform(allDevicePose[Rightidx].mDeviceToAbsoluteTracking);
                
                if(EnableOverlayDebugView)
                    RightDebugSphere.transform.position = rightControllerTransform.pos;
                
                // Calculate finger offset from controller
                Vector3 fingerOffset = new Vector3(FingerOffsetX, FingerOffsetY, FingerOffsetZ); // Tweak these offsets!
                Quaternion fingerRotationOffset = Quaternion.Euler(FingerRotationOffsetX, FingerRotationOffsetY, FingerRotationOffsetZ); // Assuming the finger points slightly differently
                Vector3 fingerPosition = rightControllerTransform.TransformPoint(fingerOffset);
                Quaternion fingerRotation = rightControllerTransform.rot * fingerRotationOffset;
                
                if(EnableOverlayDebugView)
                    Debug.DrawLine(fingerPosition, fingerPosition + (fingerRotation * Vector3.forward) * 2.0f, Color.blue);
                
                if (IsRaycastIntersectingWithOverlayCanvas(Rightidx, fingerPosition, fingerRotation * Vector3.forward, ref results))
                {
                    // If there is an overlay on the line, continue processing
                    IsRaycastInClickingRange(results, LeftOrRight.Right, ref ClickedRight);

                    // Updated for cursor display
                    RightHandU = results.vUVs.v0 * RenderTexture.width;
                    RightHandV = RenderTexture.height - results.vUVs.v1 * RenderTexture.height;
                    RightHandDistance = results.fDistance;
                }
                else
                {
                    RightHandU = -1f;
                    RightHandV = -1f;
                    RightHandDistance = -1f;
                }
            }
        }
        
        private bool IsRaycastIntersectingWithOverlayCanvas(uint idx, Vector3 pos, Vector3 dir, ref VROverlayIntersectionResults_t results)
        {
            if (idx != OpenVR.k_unTrackedDeviceIndexInvalid)
            {
                TrackedDevicePose_t[] allDevicePose = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
                _openVrSystemApiInstance.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0f, allDevicePose);

                if (allDevicePose[idx].bDeviceIsConnected && allDevicePose[idx].bPoseIsValid)
                {
                    // Convert the direction and position from Unity's coordinate system to OpenVR's coordinate system
                    HmdVector3_t rayOrigin = new HmdVector3_t { v0 = pos.x, v1 = pos.y, v2 = -pos.z }; // OpenVR uses a left-handed coordinate system
                    HmdVector3_t rayDirection = new HmdVector3_t { v0 = dir.x, v1 = dir.y, v2 = -dir.z };

                    // Raycasting parameters setup
                    VROverlayIntersectionParams_t param = new VROverlayIntersectionParams_t();
                    param.vSource = rayOrigin;
                    param.vDirection = rayDirection;
                    param.eOrigin = ETrackingUniverseOrigin.TrackingUniverseStanding;

                    // Check for intersection with the overlay
                    return _openVROverlayInstance.ComputeOverlayIntersection(_LowLevelOverlayHandle, ref param, ref results);
                }
            }

            return false; // Return false if device index is invalid, device is not connected, or pose is invalid
        }

        /*
         * Checks if the specified device is valid and then checks if it has an intersection with the overlay
         * This is used for the default generalised controller raycast origin
         * Use the other method overload for specifiying augmented raycast origin positioning.
         */
        private bool IsRaycastIntersectingWithOverlayCanvas(uint idx, TrackedDevicePose_t[] allDevicePose, ref VROverlayIntersectionResults_t results)
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
            return _openVROverlayInstance.ComputeOverlayIntersection(_LowLevelOverlayHandle, ref param, ref results);
        }
        
        private void IsRaycastInClickingRange(VROverlayIntersectionResults_t results, LeftOrRight lr, ref bool tapped)
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
                ProcessOverlayClick(results);
            }

            // If the distance between the controller and the overlay is above a certain level
            if (results.fDistance > TapOffDistance && tapped)
            {
                // Distant
                tapped = false;
                PerformHapticFeedback(lr);

                if (IsInputLockButtonPressed() && !IsInRecognitionState) // Make sure Recognition isn't already running.
                {
                    // This is the definitive point where Recognition has been triggered by the user using the Overlay.
                    OnRecognitionStateBegin?.Invoke();
                }
            }
        }
        
                // Identify and click an element on Canvas
        private void ProcessOverlayClick(VROverlayIntersectionResults_t results)
        {
#pragma warning disable 0219
            string Tag = "[" + this.GetType().Name + ":" +
                         System.Reflection.MethodBase.GetCurrentMethod(); // Automatically obtain class name and method name
#pragma warning restore 0219

            // Calculate uv coordinates for click
            float u = results.vUVs.v0 * RenderTexture.width;
            float v = RenderTexture.height - results.vUVs.v1 * RenderTexture.height;

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
                    if (res.gameObject.transform.root.name == RaycastRootObject.name)
                    {
                        ExecuteEvents.Execute(res.gameObject, pointer, ExecuteEvents.pointerClickHandler);
                        break;
                    }
                }
            }
        }
        
        #endregion
        
        #region OpenVR Low Level Api Wrappers

        // Get device information
        private string GetProperty(uint idx, ETrackedDeviceProperty prop)
        {
#pragma warning disable 0219
            string Tag = "[" + this.GetType().Name + ":" +
                         System.Reflection.MethodBase.GetCurrentMethod(); // Automatically obtain class name and method name
#pragma warning restore 0219

            ETrackedPropertyError error = new ETrackedPropertyError();
            // Get the number of characters required to get device information
            uint size = _openVrSystemApiInstance.GetStringTrackedDeviceProperty(idx, prop, null, 0, ref error);
            if (error != ETrackedPropertyError.TrackedProp_BufferTooSmall)
            {
                return null;
            }

            StringBuilder s = new StringBuilder();
            s.Length = (int)size; // Ensure character length
            // Get device information
            _openVrSystemApiInstance.GetStringTrackedDeviceProperty(idx, prop, s, size, ref error);
            if (error != ETrackedPropertyError.TrackedProp_Success)
            {
                return null;
            }

            return s.ToString();
        }
        
        // Reflect the information of the specified device in the Inspector
        private void UpdateDeviceInfo(uint idx)
        {
            // Get connection status of all devices
            TrackedDevicePose_t[] allDevicePose = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            _openVrSystemApiInstance.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0f, allDevicePose);

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
        
        // Perform vibration feedback
        private void PerformHapticFeedback(LeftOrRight lr)
        {
#pragma warning disable 0219
            string Tag = "[" + this.GetType().Name + ":" +
                         System.Reflection.MethodBase.GetCurrentMethod(); // Automatically obtain class name and method name
#pragma warning restore 0219

            // Check if left hand controller is enabled
            uint Leftidx = _openVrSystemApiInstance.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
            //OverlayTrackedDevice
            if (Leftidx != OpenVR.k_unTrackedDeviceIndexInvalid && lr == LeftOrRight.Left)
            {
                _openVrSystemApiInstance.TriggerHapticPulse(Leftidx, 0, 3000);
            }

            // Check if right hand controller is enabled
            uint Rightidx = _openVrSystemApiInstance.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
            if (Rightidx != OpenVR.k_unTrackedDeviceIndexInvalid && lr == LeftOrRight.Right)
            {
                _openVrSystemApiInstance.TriggerHapticPulse(Rightidx, 0, 3000);
            }
        }

        #endregion
        
        #region OpenVr Low Level State Checks
        
        public bool IsError()
        {
            return OverlayInitialisationError || _LowLevelOverlayHandle == INVALID_HANDLE || _openVROverlayInstance == null || _openVrSystemApiInstance == null;
        }
        
        // Return when the end event is caught
        private bool IsQuitEventPresentInOpenVrSubsystem()
        {
#pragma warning disable 0219
            string Tag = "[" + this.GetType().Name + ":" +
                         System.Reflection.MethodBase.GetCurrentMethod();
#pragma warning restore 0219

            // Get the size of the event structure
            uint uncbVREvent = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VREvent_t));

            // Event information storage structure
            VREvent_t Event = new VREvent_t();
            // Retrieve event
            while (_openVROverlayInstance.PollNextOverlayEvent(_LowLevelOverlayHandle, ref Event, uncbVREvent))
            {
                // View event log
                if (EventLog)
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
        
        public bool IsVisible()
        {
            return _openVROverlayInstance.IsOverlayVisible(_LowLevelOverlayHandle) && !IsError();
        }
        

        // Error processing (release processing)
        private void ProcessError()
        {
#pragma warning disable 0219
            string Tag = "[" + this.GetType().Name + ":" +
                         System.Reflection.MethodBase.GetCurrentMethod(); // Automatically obtain class name and method name
#pragma warning restore 0219
            Debug.Log(Tag + "Begin");

            // release the handle
            if (_LowLevelOverlayHandle != INVALID_HANDLE && _openVROverlayInstance != null)
            {
                _openVROverlayInstance.DestroyOverlay(_LowLevelOverlayHandle);
            }

            _LowLevelOverlayHandle = INVALID_HANDLE;
            _openVROverlayInstance = null;
            _openVrSystemApiInstance = null;
            OverlayInitialisationError = true;
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
        
        #endregion
    }
}