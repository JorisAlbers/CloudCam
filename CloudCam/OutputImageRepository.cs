using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Documents;
using OpenCvSharp;

namespace CloudCam
{
    public class OutputImageRepository
    {
        private readonly string _folder;

        public OutputImageRepository(string folder)
        {
            _folder = folder;
        }

        public void Save(Bitmap image)
        {
            DateTime now = DateTime.Now;
            string fileName = $"CloudCam_{now:yyyy-M-dd--H-mm-ss}.jpg";
            Console.Out.WriteLine("Saving image to disc at {0}", fileName);


            Directory.CreateDirectory(_folder);


            image.Save(Path.Combine(_folder, fileName));
        }

        public string[] GetNames()
        {
            return Directory.GetFiles(_folder, "*.jpg");
        }

        public Mat Load(string name)
        {
            return Cv2.ImRead(name,ImreadModes.Unchanged);
        }
    }
}