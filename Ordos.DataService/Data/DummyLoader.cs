using System.Collections.Generic;
using Ordos.Core.Models;
using System.Linq;

namespace Ordos.DataService.Data
{
    public static class DummyLoader
    {
        public static Device CloneDeviceShallow(SystemContext context, Device device)
        {
            return (Device)context
                .Entry(device)
                .CurrentValues.ToObject();
        }

        public static Device CloneDeviceDeep(SystemContext context, Device device)
        {
            var cloneDevice = (Device)context.Entry(device).CurrentValues.ToObject();

            if (device.DisturbanceRecordings != null && device.DisturbanceRecordings.Count > 0)
                cloneDevice.DisturbanceRecordings = CloneAllDisturbanceRecordings(context, device);

            return cloneDevice;
        }

        public static DisturbanceRecording CloneDisturbanceRecording(SystemContext context, DisturbanceRecording disturbanceRecording)
        {
            return (DisturbanceRecording)context
                .Entry(disturbanceRecording)
                .CurrentValues.ToObject();
        }

        public static List<DisturbanceRecording> CloneAllDisturbanceRecordings(SystemContext context,
            Device device)
        {
            return device.DisturbanceRecordings
                .Select(dr => CloneDisturbanceRecording(context, dr))
                .ToList();
        }
    }
}
