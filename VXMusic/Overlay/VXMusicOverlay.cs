using System.Drawing;
using System.Drawing.Imaging;
using OVRSharp;
using OVRSharp.Math;
using System.Numerics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Valve.VR;

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
                as ILogger<VXMusicOverlayInstance> ?? throw new ApplicationException("A logger must be created in service provider.");
            
            _logger.LogInformation("Creating VXMusicOverlayInstance.");

            vxMusicOverlay = new VXMusicOverlay(serviceProvider);
            vxMusicOverlay.Show();
        }

        public void SendNotification(string title, string content, int timeout, string image = "")
        {
            vxMusicOverlay.SendNotificationInternal(title, content, timeout, image);
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
                as ILogger<VXMusicOverlay> ?? throw new ApplicationException("A logger must be created in service provider.");
            
            _logger.LogTrace("Initialising VXMusicOverlay.");

            var isDash = IsDashboardOverlay;
            var isHover = OpenVR.Overlay.IsHoverTargetOverlay(Handle);

            //this.StartPolling(); // This starts SteamVR?
            //this.Show();
            //var name = this.Name;
            
            // ANY CALL TO OPENVR OPENS STEAMVR
            
            WidthInMeters = 0.02f;

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
            this.OnMouseUp += HandleMouseMoveEvent;
            this.OnMouseDown += HandleMouseMoveEvent;
            // Maybe try a direct Open VR API call?
            
            var rotationX = Matrix4x4.CreateRotationX(DegreesToRadians(125f)); // Lay the overlay flat by rotating it by 90 degrees
            var rotationY = Matrix4x4.CreateRotationY(DegreesToRadians(90f)); // Lay the overlay flat by rotating it by 90 degrees
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
            Bitmap bitmap = new Bitmap(@"C:\Users\Tam\git\VXMusic\VXMusic\Img\VXLogo\VXLogoSmall.jpg");
            //Bitmap bitmap = new Bitmap(image);
            //Image image = (Image) bitmap;

            IntPtr imageData = GetImageData(bitmap);

            var notificationBitmap = new NotificationBitmap_t();
            notificationBitmap.m_pImageData = imageData;
            notificationBitmap.m_nHeight = 80;
            notificationBitmap.m_nWidth = 80;
            notificationBitmap.m_nBytesPerPixel = 10;

            uint notificationId = 0;
            
            OpenVR.Notifications.CreateNotification(Handle, 0, EVRNotificationType.Transient, title,
                EVRNotificationStyle.Application, ref notificationBitmap, ref notificationId);
            
            Marshal.FreeHGlobal(imageData);
        }

        // Event handler method to handle the mouse move event
        private void HandleMouseMoveEvent(object sender, VREvent_t e)
        {
            // Handle the mouse move event here
            _logger.LogDebug("Mouse move event received!");
            Alpha = 100;
            OpenVR.Overlay.SetOverlayColor(Handle, 0, 0, 254);
            OpenVR.Overlay.TriggerLaserMouseHapticVibration(Handle, 1f, 1f,1f);

            //OpenVR.Overlay.SetOverlayCursor(Handle);
            //OpenVR.IsHmdPresent()

            // You can access event data in the 'e' parameter if needed
            // ...
        }

        private static IntPtr GetImageData(Bitmap bitmap)
        {
            // Lock the bitmap data
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

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