using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace CloudCam
{
    public class ImageRepository
    {
        private readonly string _folderPath;
        private string[] _images;

        public int Count => _images.Length;

        public ImageRepository(string folderPath)
        {
            _folderPath = folderPath;
        }

        public void Load()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(_folderPath);
            _images = directoryInfo.EnumerateFiles("*.png").Select(x => x.Name).ToArray();
        }

        public Mat this[int index] => Cv2.ImRead(Path.Combine(_folderPath, _images[index]), ImreadModes.Unchanged);
    }
}
