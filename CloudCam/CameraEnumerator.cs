using System.Collections.Generic;
using System.Linq;
using DirectShowLib;

namespace CloudCamDotNet4
{
    public static class CameraDevicesEnumerator
    {
        public static List<CameraDevice> GetAllConnectedCameras()
        {
            var videoInputDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            int openCvId = 0;
            return videoInputDevices.Select(v => new CameraDevice(v.DevicePath, v.Name, openCvId++)).ToList();
        }
    }

    public class CameraDevice
    {
        public string DeviceId { get; }
        public string Name { get; }
        public int OpenCdId { get; }

        public CameraDevice(string deviceId, string name, int openCdId)
        {
            DeviceId = deviceId;
            Name = name;
            OpenCdId = openCdId;
        }
    }
}
