using System.Drawing;
using System.Drawing.Imaging;
using OVRSharp;
using OVRSharp.Math;
using System.Numerics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Valve.VR;
using static OVRSharp.Overlay;

namespace VXMusic.Overlay
{
    public class VXMusicOverlayInstance : Application, INotificationClient
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<VXMusicOverlayInstance> _logger;

        private readonly VXMusicOverlay vxMusicOverlay;
        
        

        public VXMusicOverlayInstance(IServiceProvider serviceProvider)
            : base(ApplicationType.Overlay)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetService(typeof(ILogger<VXMusicOverlayInstance>))
                          as ILogger<VXMusicOverlayInstance> ??
                      throw new ApplicationException("A logger must be created in service provider.");

            _logger.LogInformation("Creating VXMusicOverlayInstance.");

            vxMusicOverlay = new VXMusicOverlay(serviceProvider);
            vxMusicOverlay.Show();
        }

        public void SendNotification(string title, string content, int timeout, string image = "")
        {
            vxMusicOverlay.SendNotificationInternal(title, content, timeout, image);
        }
        public void SetLeftOverlayTrackedDevice()
        {
            _logger.LogInformation("Setting Overlay tracked device to: Left Hand");
            vxMusicOverlay.SetOverlayTrackedDeviceInternal(TrackedDeviceRole.LeftHand);
        }

        public void SetRightHandOverlayTrackedDevice()
        {
            _logger.LogInformation("Setting Overlay tracked device to: Right Hand");
            vxMusicOverlay.SetOverlayTrackedDeviceInternal(TrackedDeviceRole.RightHand);
        }

        public TrackedDeviceRole GetOverlayTrackedDevice()
        {
            return vxMusicOverlay.TrackedDevice;
        }
    }

    class VXMusicOverlay : OVRSharp.Overlay
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<VXMusicOverlay> _logger;

        public VXMusicOverlay(IServiceProvider serviceProvider)
            : base("vxmusic", "VXMusic")
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetService(typeof(ILogger<VXMusicOverlay>))
                          as ILogger<VXMusicOverlay> ??
                      throw new ApplicationException("A logger must be created in service provider.");

            _logger.LogTrace("Initialising VXMusicOverlay.");

            var isDash = IsDashboardOverlay;
            var isHover = OpenVR.Overlay.IsHoverTargetOverlay(Handle);

            //this.StartPolling(); // This starts SteamVR?
            //this.Show();
            //var name = this.Name;

            // ANY CALL TO OPENVR OPENS STEAMVR

            WidthInMeters = 0.1f;

            // TODO This collides with over overlays???
            try
            {
                TrackedDevice = TrackedDeviceRole.RightHand;
            }
            catch (Exception e)
            {
                _logger.LogWarning("VR Device was not found, are your controllers turned on?");
                return;
            }

            //VROverlayTransformType.VROverlayTransform_TrackedDeviceRelative;
            Alpha = 100;

            InputMethod = VROverlayInputMethod.Mouse;

            // Subscribe to input events
            this.OnMouseMove += HandleMouseMoveEvent;
            //this.OnMouseUp += HandleMouseMoveEvent;
            //this.OnMouseDown += HandleMouseMoveEvent;
            // Maybe try a direct Open VR API call?

            var rotationX =
                Matrix4x4.CreateRotationX(DegreesToRadians(125f)); // Lay the overlay flat by rotating it by 90 degrees
            var rotationY =
                Matrix4x4.CreateRotationY(DegreesToRadians(90f)); // Lay the overlay flat by rotating it by 90 degrees
            var rotation = Matrix4x4.Multiply(rotationX, rotationY);
            var translation = Matrix4x4.CreateTranslation(0, 0f, 0.1f); // Raise the overlay one meter above the ground
            var transform = Matrix4x4.Multiply(rotation, translation);

            Transform = transform.ToHmdMatrix34_t();

            //.CreateRotationX(DegreesToRadians(90f)) // DegreesToRadians(45f))
            // .ToHmdMatrix34_t();

            var overlayImagePath = Utils.PathToResource("VXLogo.png");

            SetTextureFromFile(overlayImagePath);

            PollingRate = 5;
            StartPolling();

            //
            // OpenVR.Notifications.CreateNotification(Handle, 0, EVRNotificationType.Transient, "Do these stack",
            //     EVRNotificationStyle.Application, ref notificationBitmap, ref notificationId);
            //
            //OpenVR.Overlay.ShowMessageOverlay("Text", "Caption", "Button0Text", "Button1Text",
            //  "button2text", "button3text");
            //OpenVR.Overlay.ClearOverlayCursorPositionOverride();

            //StopPolling();
        }

        public void SendNotificationInternal(string title, string content, int timeout, string image = "")
        {
            _logger.LogTrace("Sending notification request to SteamVR.");

            // Need to read in file as Base64, or convert a file from Base64
            // TODO Make this more flexible
            var logoBase64 = File.ReadAllText(@"C:\Users\Tam\git\VXMusic\VXMusic\Img\VXLogo\VXLogoBase64Jpg.txt");
            var imageBytes = Convert.FromBase64String(logoBase64);

            Bitmap bitmap = new Bitmap(new MemoryStream(imageBytes));

            NotificationBitmap_t finalBitmap = new NotificationBitmap_t();
            finalBitmap = NotificationBitmapFromBitmap(bitmap, true);
            bitmap.Dispose();

            uint notificationId = 0;

            OpenVR.Notifications.CreateNotification(Handle, 0, EVRNotificationType.Transient, title,
                EVRNotificationStyle.Application, ref finalBitmap, ref notificationId);

            GC.KeepAlive(finalBitmap);
            //Marshal.FreeHGlobal(finalBitmap); //TODO NEED THIS
        }

        public void SetOverlayTrackedDeviceInternal(TrackedDeviceRole trackedDeviceRole)
        {
            try
            {
                TrackedDevice = trackedDeviceRole;
            }
            catch (Exception e)
            {
                _logger.LogWarning("VR Device was not found, are any controllers turned on?");
                return;
            }
        }

        public static NotificationBitmap_t NotificationBitmapFromBitmap(Bitmap bmp, bool flipRnB = true)
        {
            return NotificationBitmapFromBitmapData(BitmapDataFromBitmap(bmp, flipRnB));
        }

        public static BitmapData BitmapDataFromBitmap(Bitmap bmpIn, bool flipRnB = false)
        {
            Bitmap bmp = (Bitmap)bmpIn.Clone();
            if (flipRnB) RGBtoBGR(bmp);
            BitmapData texData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb
            );
            return texData;
        }

        private static void RGBtoBGR(Bitmap bmp)
        {
            // based on https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap.unlockbits?view=netframework-4.8

            int bytesPerPixel = Bitmap.GetPixelFormatSize(bmp.PixelFormat) / 8;
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite,
                bmp.PixelFormat);
            int bytes = Math.Abs(data.Stride) * bmp.Height;

            IntPtr ptr = data.Scan0;
            var rgbValues = new byte[bytes];
            Marshal.Copy(data.Scan0, rgbValues, 0, bytes);
            for (int i = 0; i < bytes; i += bytesPerPixel)
            {
                byte dummy = rgbValues[i];
                rgbValues[i] = rgbValues[i + 2];
                rgbValues[i + 2] = dummy;
            }

            Marshal.Copy(rgbValues, 0, ptr, bytes);
            bmp.UnlockBits(data);
        }

        public static NotificationBitmap_t NotificationBitmapFromBitmapData(BitmapData TextureData)
        {
            NotificationBitmap_t notification_icon = new NotificationBitmap_t();
            notification_icon.m_pImageData = TextureData.Scan0;
            notification_icon.m_nWidth = TextureData.Width;
            notification_icon.m_nHeight = TextureData.Height;
            notification_icon.m_nBytesPerPixel = 4;
            return notification_icon;
        }


        // Event handler method to handle the mouse move event
        private void HandleMouseMoveEvent(object sender, VREvent_t e)
        {
            // Handle the mouse move event here
            _logger.LogDebug("Mouse move event received!");
            Alpha = 100;
            OpenVR.Overlay.SetOverlayColor(Handle, 0, 0, 254);
            OpenVR.Overlay.TriggerLaserMouseHapticVibration(Handle, 1f, 1f, 1f);
            //e.trackedDeviceIndex
            //e.data.controller.button
            if (e.data.touchPadMove.bFingerDown)
            {
                // check SetFlag in OVR Overlay.cs

                //e.data.touchPadMove.flSecondsFingerDown
                _logger.LogDebug("Mouse button: " + e.data.mouse.button);
                _logger.LogDebug("Keyboard user value button: " + e.data.keyboard.uUserValue);
                _logger.LogDebug("Keyboard user value button: " + e.data.keyboard.cNewInput);
                _logger.LogDebug("Seconds finger down: " + e.data.touchPadMove.flSecondsFingerDown);
                _logger.LogDebug("Input binding path message " + e.data.inputBinding.pathMessage);
                _logger.LogDebug("Input binding path url " + e.data.inputBinding.pathUrl);
                _logger.LogDebug("Input binding path controller type " + e.data.inputBinding.pathControllerType);
                _logger.LogDebug("Input binding ulAppContainer " + e.data.inputBinding.ulAppContainer);
                _logger.LogDebug("*****************************");
                
                //_logger.LogDebug(e.data.mouse.button.ToString());
            }

            


            //e.eventType;
            //e.data.overlay.overlayHandle;
            //e.data.messageOverlay; //.unVRMessageOverlayResponse
            //e.data.notification //notificationId

            //OpenVR.System.IsInputAvailable();
            //OpenVR.System.GetControllerState(e.trackedDeviceIndex, ref e.data.controller, );
            //OpenVR.System.GetButtonIdNameFromEnum();

            //OpenVR.Overlay.SetOverlayCursor(Handle);
            //OpenVR.IsHmdPresent()

            // You can access event data in the 'e' parameter if needed
            // ...
        }

        private static IntPtr GetImageData(Bitmap bitmap)
        {
            // Lock the bitmap data
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, bitmap.PixelFormat);

            // Calculate the number of bytes in the image
            int bytes = Math.Abs(bmpData.Stride) * bitmap.Height;

            // Create a buffer to hold the image data
            byte[] imageData = new byte[bytes];

            // Copy the image data to the buffer
            Marshal.Copy(bmpData.Scan0, imageData, 0, bytes);

            // Unlock the bitmap data
            bitmap.UnlockBits(bmpData);

            // Allocate unmanaged memory for the image data
            IntPtr imageDataPtr = Marshal.AllocHGlobal(bytes);

            // Copy the image data from the buffer to the unmanaged memory
            Marshal.Copy(imageData, 0, imageDataPtr, bytes);

            return imageDataPtr;
        }

        private static float DegreesToRadians(float degrees)
        {
            return (float)(degrees * (Math.PI / 180f));
        }
    }
}