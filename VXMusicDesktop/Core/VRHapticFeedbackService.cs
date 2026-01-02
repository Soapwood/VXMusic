using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Valve.VR;

namespace VXMusicDesktop.Core
{
    public interface IVRHapticFeedbackService : IDisposable
    {
        void TriggerHapticFeedback(ushort duration = 1000, uint deviceIndex = OpenVR.k_unTrackedDeviceIndexInvalid);
        bool IsVRSystemAvailable();
        bool HasConnectedControllers();
        void TestHapticFeedback(); // Simple test method
    }

    public class VRHapticFeedbackService : IVRHapticFeedbackService
    {
        private readonly ILogger<VRHapticFeedbackService> _logger;
        private bool _vrInitializationAttempted = false;
        private bool _vrSystemAvailable = false;

        public VRHapticFeedbackService(ILogger<VRHapticFeedbackService> logger)
        {
            _logger = logger;
        }

        public bool IsVRSystemAvailable()
        {
            // First check if SteamVR process is running
            if (!IsSteamVrProcessRunning())
            {
                _logger.LogTrace("SteamVR process (vrserver) is not running");
                return false;
            }

            // If we haven't attempted VR initialization yet, try it now
            if (!_vrInitializationAttempted)
            {
                InitializeVRSystem();
            }

            return _vrSystemAvailable;
        }

        private bool IsSteamVrProcessRunning()
        {
            try
            {
                return Process.GetProcesses()
                    .Any(p => string.Equals(p.ProcessName, "vrserver", StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to check for SteamVR process");
                return false;
            }
        }

        private void InitializeVRSystem()
        {
            _vrInitializationAttempted = true;
            
            try
            {
                _logger.LogInformation("Attempting to initialize OpenVR system for haptic feedback");
                
                // Check if OpenVR is already initialized
                if (OpenVR.System != null)
                {
                    _vrSystemAvailable = true;
                    _logger.LogInformation("OpenVR system is already initialized");
                    return;
                }

                // Try to initialize OpenVR - use Overlay type for desktop apps that don't need full VR
                EVRInitError initError = EVRInitError.None;
                _logger.LogInformation("Initializing OpenVR with Overlay application type");
                OpenVR.Init(ref initError, EVRApplicationType.VRApplication_Overlay);

                if (initError == EVRInitError.None)
                {
                    if (OpenVR.System != null)
                    {
                        _vrSystemAvailable = true;
                        _logger.LogInformation("Successfully initialized OpenVR for haptic feedback");
                        
                        // Test if we can access the system
                        try
                        {
                            var hmdClass = OpenVR.System.GetTrackedDeviceClass(0);
                            _logger.LogInformation($"HMD device class: {hmdClass}");
                        }
                        catch (Exception testEx)
                        {
                            _logger.LogWarning(testEx, "Failed to test OpenVR system access");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("OpenVR.Init succeeded but OpenVR.System is null");
                        _vrSystemAvailable = false;
                    }
                }
                else
                {
                    _logger.LogWarning($"Failed to initialize OpenVR: {initError}");
                    _vrSystemAvailable = false;
                    
                    // Try with a different application type as fallback
                    try
                    {
                        _logger.LogInformation("Trying fallback initialization with Background application type");
                        OpenVR.Init(ref initError, EVRApplicationType.VRApplication_Background);
                        if (initError == EVRInitError.None && OpenVR.System != null)
                        {
                            _vrSystemAvailable = true;
                            _logger.LogInformation("Successfully initialized OpenVR with Background type");
                        }
                        else
                        {
                            _logger.LogWarning($"Fallback initialization also failed: {initError}");
                        }
                    }
                    catch (Exception fallbackEx)
                    {
                        _logger.LogWarning(fallbackEx, "Exception during fallback OpenVR initialization");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during OpenVR initialization");
                _vrSystemAvailable = false;
            }
        }

        public bool HasConnectedControllers()
        {
            if (!IsVRSystemAvailable())
                return false;

            try
            {
                // Double-check that OpenVR.System is available
                if (OpenVR.System == null)
                {
                    _logger.LogTrace("OpenVR.System is null when checking for controllers");
                    return false;
                }

                // Check for connected controllers
                for (uint deviceIndex = 0; deviceIndex < OpenVR.k_unMaxTrackedDeviceCount; deviceIndex++)
                {
                    if (OpenVR.System.IsTrackedDeviceConnected(deviceIndex))
                    {
                        var deviceClass = OpenVR.System.GetTrackedDeviceClass(deviceIndex);
                        if (deviceClass == ETrackedDeviceClass.Controller)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to check for connected controllers");
                return false;
            }
        }

        public void TriggerHapticFeedback(ushort duration = 1000, uint deviceIndex = OpenVR.k_unTrackedDeviceIndexInvalid)
        {
            _logger.LogInformation($"TriggerHapticFeedback called with duration {duration}, deviceIndex {deviceIndex}");
            
            if (!IsVRSystemAvailable())
            {
                _logger.LogWarning("VR system not available, skipping haptic feedback");
                return;
            }

            if (!HasConnectedControllers())
            {
                _logger.LogWarning("No VR controllers connected, skipping haptic feedback");
                return;
            }
            
            _logger.LogInformation("VR system available and controllers connected, proceeding with haptic feedback");

            try
            {
                // Double-check that OpenVR.System is available
                if (OpenVR.System == null)
                {
                    _logger.LogTrace("OpenVR.System is null when trying to trigger haptic feedback");
                    return;
                }

                // If no specific device index is provided, trigger haptic on all controllers
                if (deviceIndex == OpenVR.k_unTrackedDeviceIndexInvalid)
                {
                    TriggerHapticOnAllControllers(duration);
                }
                else
                {
                    TriggerHapticOnDevice(deviceIndex, duration);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to trigger haptic feedback");
            }
        }


        private void TriggerHapticOnDevice(uint deviceIndex, ushort duration)
        {
            try
            {
                // Convert to microseconds and ensure it's in a good range
                // OpenVR TriggerHapticPulse expects duration in microseconds
                // Typical values: 500-4000 microseconds work well
                ushort microseconds;
                if (duration < 500)
                {
                    microseconds = 1500; // Default to 1.5ms for short durations
                }
                else if (duration > 4000)
                {
                    microseconds = 4000; // Cap at 4ms
                }
                else
                {
                    microseconds = duration;
                }
                
                _logger.LogDebug($"Triggering haptic on device {deviceIndex}, duration {microseconds} microseconds");
                
                // Try axis 0 first (most common haptic axis for controllers)
                bool success = false;
                try
                {
                    OpenVR.System.TriggerHapticPulse(deviceIndex, 0, microseconds);
                    success = true;
                    _logger.LogDebug($"Successfully triggered haptic on device {deviceIndex} axis 0");
                }
                catch (Exception axisEx)
                {
                    _logger.LogWarning(axisEx, $"Failed to trigger haptic on device {deviceIndex} axis 0");
                }
                
                // If axis 0 failed, try axis 1 as fallback
                if (!success)
                {
                    try
                    {
                        OpenVR.System.TriggerHapticPulse(deviceIndex, 1, microseconds);
                        _logger.LogDebug($"Successfully triggered haptic on device {deviceIndex} axis 1 (fallback)");
                    }
                    catch (Exception axisEx)
                    {
                        _logger.LogWarning(axisEx, $"Failed to trigger haptic on device {deviceIndex} axis 1 (fallback)");
                        
                        // Try a simple pulse with different parameters
                        try
                        {
                            OpenVR.System.TriggerHapticPulse(deviceIndex, 0, 2000);
                            _logger.LogDebug($"Triggered simple haptic pulse on device {deviceIndex}");
                        }
                        catch (Exception simpleEx)
                        {
                            _logger.LogError(simpleEx, $"All haptic attempts failed for device {deviceIndex}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to trigger haptic feedback on device {deviceIndex}");
            }
        }

        private void TriggerHapticOnAllControllers(ushort duration)
        {
            try
            {
                _logger.LogInformation($"TriggerHapticOnAllControllers called with duration {duration}");
                
                if (OpenVR.System == null)
                {
                    _logger.LogWarning("OpenVR.System is null when trying to trigger haptic on all controllers");
                    return;
                }
                    
                int controllerCount = 0;
                // Trigger haptic feedback on all connected controllers
                for (uint deviceIndex = 0; deviceIndex < OpenVR.k_unMaxTrackedDeviceCount; deviceIndex++)
                {
                    if (OpenVR.System.IsTrackedDeviceConnected(deviceIndex))
                    {
                        var deviceClass = OpenVR.System.GetTrackedDeviceClass(deviceIndex);
                        _logger.LogDebug($"Device {deviceIndex} is connected, class: {deviceClass}");
                        
                        if (deviceClass == ETrackedDeviceClass.Controller)
                        {
                            controllerCount++;
                            _logger.LogInformation($"Triggering haptic on controller {deviceIndex}");
                            TriggerHapticOnDevice(deviceIndex, duration);
                        }
                    }
                }
                
                _logger.LogInformation($"Found and triggered haptic on {controllerCount} controllers");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to trigger haptic on all controllers");
            }
        }

        public void TestHapticFeedback()
        {
            _logger.LogInformation("=== TESTING HAPTIC FEEDBACK ===");
            _logger.LogInformation($"VR system available: {IsVRSystemAvailable()}");
            _logger.LogInformation($"Has connected controllers: {HasConnectedControllers()}");
            
            if (!IsVRSystemAvailable())
            {
                _logger.LogWarning("Cannot test haptic feedback - VR system not available");
                return;
            }
            
            // Try different haptic feedback approaches
            _logger.LogInformation("Testing direct haptic pulse on all detected controllers...");
            
            try
            {
                for (uint deviceIndex = 0; deviceIndex < OpenVR.k_unMaxTrackedDeviceCount; deviceIndex++)
                {
                    if (OpenVR.System.IsTrackedDeviceConnected(deviceIndex))
                    {
                        var deviceClass = OpenVR.System.GetTrackedDeviceClass(deviceIndex);
                        _logger.LogInformation($"Testing device {deviceIndex}, class: {deviceClass}");
                        
                        if (deviceClass == ETrackedDeviceClass.Controller)
                        {
                            _logger.LogInformation($"Sending test haptic pulse to controller {deviceIndex}");
                            
                            // Try multiple pulse durations to see what works
                            ushort[] testDurations = { 500, 1000, 2000, 3000 };
                            foreach (var duration in testDurations)
                            {
                                try
                                {
                                    _logger.LogInformation($"  Trying pulse duration {duration} microseconds");
                                    OpenVR.System.TriggerHapticPulse(deviceIndex, 0, duration);
                                    System.Threading.Thread.Sleep(100); // Brief pause between pulses
                                }
                                catch (Exception pulseEx)
                                {
                                    _logger.LogWarning(pulseEx, $"  Failed pulse duration {duration}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during haptic test");
            }
            
            _logger.LogInformation("=== HAPTIC TEST COMPLETE ===");
        }

        public void Dispose()
        {
            try
            {
                if (_vrSystemAvailable && _vrInitializationAttempted)
                {
                    _logger.LogDebug("Shutting down OpenVR for haptic feedback service");
                    OpenVR.Shutdown();
                    _vrSystemAvailable = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Exception during OpenVR shutdown");
            }
        }
    }
}
