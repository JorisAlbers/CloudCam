using System.Collections.Generic;
using System.Linq;
using DirectShowLib;

namespace CloudCam
{
    public record CameraDevice(string DeviceId, string Name, int OpenCvId);

    public static class CameraDevicesEnumerator
    {
        public static List<CameraDevice> GetAllConnectedCameras()
        {
            var videoInputDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            int openCvId = 0;
            return videoInputDevices.Select(v => new CameraDevice(v.DevicePath, v.Name, openCvId++)).ToList();
        }
    }
}
