using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace VXAutoUpdaterDesktop.Core;

public class Utils
{
    public static void StopRunningVXMusicDesktopProcesses()
    {
        List<string> processNames = new List<string>()
        {
            "VXMusicDesktop",
            "VXMOverlay"
        };

        try
        {
            foreach (string processName in processNames)
            {
                // Get the list of all processes with the specified name
                Process[] processes = Process.GetProcessesByName(processName);

                // Loop through each process and terminate it
                foreach (Process process in processes)
                {
                    process.Kill();
                    process.WaitForExit(); // Optional: Wait for the process to exit
                    Console.WriteLine($"Terminated process {process.ProcessName} with ID {process.Id}");
                }

                if (processes.Length == 0)
                {
                    Console.WriteLine($"No processes found with the name {processName}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}