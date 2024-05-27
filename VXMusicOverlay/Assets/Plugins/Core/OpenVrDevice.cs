using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Plugins.Core
{
    public class OpenVrDevice
    {
        public string SerialNumber;
        public string RenderModelName;
        public string ControllerType;
        public string ManufacturerName;
        public string ModelNumber;
        public string DefaultPlaybackDeviceId;
        public string DefaultRecordingDeviceId;
        public string RegisteredDeviceType;

        public override string ToString()
        {
            var deviceDetailsReport = new StringBuilder("Device Details:" + Environment.NewLine);

            void AppendIfNotNullOrEmpty(string label, string value)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    deviceDetailsReport.AppendLine($"{label}: {value}");
                }
            }

            AppendIfNotNullOrEmpty("Manufacturer", ManufacturerName);
            AppendIfNotNullOrEmpty("Model Number", ModelNumber);
            AppendIfNotNullOrEmpty("Serial", SerialNumber);
            AppendIfNotNullOrEmpty("Registered Device Type", RegisteredDeviceType);
            AppendIfNotNullOrEmpty("Controller Type", ControllerType);
            AppendIfNotNullOrEmpty("Render Model Name", RenderModelName);
            AppendIfNotNullOrEmpty("Default Playback Device Id", DefaultPlaybackDeviceId);
            AppendIfNotNullOrEmpty("Default Recording Device Id", DefaultRecordingDeviceId);

            return deviceDetailsReport.ToString().TrimEnd();
        }
    }
}

// return $"Device Details: {Environment.NewLine}" +
//        $"{ManufacturerName}: {ModelNumber}{Environment.NewLine}" +
//        $"Serial: {SerialNumber}{Environment.NewLine}" +
//        $"Registered Device Type: {RegisteredDeviceType}{Environment.NewLine}" +
//        $"Controller Type: {ControllerType}{Environment.NewLine}" +
//        $"Render Model Name: {RenderModelName}{Environment.NewLine}" +
//        $"Default Playback Device Id: {DefaultPlaybackDeviceId}{Environment.NewLine}" +
//        $"Default Recording Device Id: {DefaultRecordingDeviceId}{Environment.NewLine}";