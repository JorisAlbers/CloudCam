using System.Collections.Generic;
using System.Linq;
using DirectShowLib;

namespace CloudCam
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
        public int OpenCdId { get; private set; }

        public CameraDevice(string deviceId, string name, int openCdId)
        {
            DeviceId = deviceId;
            Name = name;
            OpenCdId = openCdId;
        }

        public bool TryGetOpenCvId(out int openCvId)
        {
            openCvId = CameraDevicesEnumerator.GetAllConnectedCameras().FindIndex(y => y.Name == Name);

            if (openCvId < 0)
            {
                return false;
            }

            OpenCdId = openCvId;
            return true;
        }
    }
}
