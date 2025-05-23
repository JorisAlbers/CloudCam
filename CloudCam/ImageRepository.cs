﻿using System.Collections.Generic;
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

        public int Count => _images?.Length ?? 0;

        public ImageRepository(string folderPath)
        {
            _folderPath = folderPath;
        }

        public void Load()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(_folderPath);
            _images = directoryInfo.EnumerateFiles("*.png").Select(x => x.Name).ToArray();
        }

        public (string name, Mat image) this[int index] => (_images[index], Cv2.ImRead(Path.Combine(_folderPath, _images[index]), ImreadModes.Unchanged));
    }
}
