using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace VXMusic.Utils;
using Valve.VR;

public class BitmapUtils
{
            
        public static NotificationBitmap_t NotificationBitmapFromBitmap(Bitmap bmp, bool flipRnB=true)
        {
            return NotificationBitmapFromBitmapData(BitmapDataFromBitmap(bmp, flipRnB));
        }
        
        public static BitmapData BitmapDataFromBitmap(Bitmap bmpIn, bool flipRnB=false)
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
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
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
        
        public static byte[] ConvertPngToByteArray(string filePath)
        {
            using (Bitmap bitmap = new Bitmap(filePath))
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Png);
                return memoryStream.ToArray();
            }
        }
        
        public static byte[] ConvertBase64ToBitmapByteArray(string base64String)
        {
            // Convert Base64 to byte array
            return Convert.FromBase64String(base64String);
        }

        static byte[] ConvertBitmapToByteArray(Bitmap bitmap, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, format);
                return ms.ToArray();
            }
        }
}