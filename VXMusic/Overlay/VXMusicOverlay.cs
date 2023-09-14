using OVRSharp;
using OVRSharp.Math;
using System.Numerics;
using Valve.VR;

namespace VXMusic.Overlay
{
    
    public class VXMusicOverlayInstance : Application
    {
        private readonly OVRSharp.Overlay vxMusicOverlay;

        public VXMusicOverlayInstance()
            : base(ApplicationType.Overlay)
        {
            vxMusicOverlay = new VXMusicOverlay();
            vxMusicOverlay.Show();
        }
    }
    
    class VXMusicOverlay : OVRSharp.Overlay
    {
        public VXMusicOverlay()
            : base("vxmusic", "VXMusic")
        {
            WidthInMeters = 0.02f;

            // TODO This collides with over overlays???
            TrackedDevice = OVRSharp.Overlay.TrackedDeviceRole.RightHand;
            //VROverlayTransformType.VROverlayTransform_TrackedDeviceRelative;
            Alpha = 100;
            InputMethod = VROverlayInputMethod.Mouse;
            
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
        }

        private static float DegreesToRadians(float degrees)
        {
            return (float)(degrees * (Math.PI / 180f));
        }
    }
}