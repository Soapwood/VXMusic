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
    public class SteamVRNotificationClient : Application, INotificationClient
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<VXMusicOverlayInstance> _logger;

        private readonly SteamVRNotificationOverlay _steamVrNotificationClient;

        public SteamVRNotificationClient(IServiceProvider serviceProvider)
            : base(ApplicationType.Overlay)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetService(typeof(ILogger<VXMusicOverlayInstance>))
                          as ILogger<VXMusicOverlayInstance> ??
                      throw new ApplicationException("A logger must be created in service provider.");

            _logger.LogInformation("Creating VXMusicOverlayInstance.");

            _steamVrNotificationClient = new SteamVRNotificationOverlay(serviceProvider);
            _steamVrNotificationClient.Show();
        }

        public void SendNotification(string title, string content, int timeout, string image = "")
        {
            _steamVrNotificationClient.SendNotificationInternal(title, content, timeout, image);
        }
    }

    class SteamVRNotificationOverlay : OVRSharp.Overlay
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<VXMusicOverlay> _logger;

        public SteamVRNotificationOverlay(IServiceProvider serviceProvider)
            : base("vxmusicsteamvrnotifhook", "VXMusic")
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetService(typeof(ILogger<VXMusicOverlay>))
                          as ILogger<VXMusicOverlay> ??
                      throw new ApplicationException("A logger must be created in service provider.");
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


        // Event handler method to handle the mouse move even
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