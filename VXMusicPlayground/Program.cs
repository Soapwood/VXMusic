// See https://aka.ms/new-console-template for more information

using OVRSharp;
using Valve.VR;

public class MyCoolApp : Application {
    private Overlay overlay;

    public MyCoolApp() : base(ApplicationType.Overlay) {
        // You can also extend the `Overlay` class yourself, but for
        // demonstration we will be instantiating it directly.
        overlay = new Overlay("vxmusic", "VXMusic");
        
        overlay.Show();
    }
}

public static class Program {
    public static void Main() {
        var app = new MyCoolApp();

        Console.ReadLine();
    }
}