using System;
using System.Diagnostics;

namespace MyAppLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            string appPath = @"..\publish\YourMainApp.exe";
            try
            {
                Process.Start(appPath, args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start the application: {ex.Message}");
            }
        }
    }
}