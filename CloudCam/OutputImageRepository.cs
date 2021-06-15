using System;
using System.Drawing;
using System.IO;

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
            image.Save(Path.Combine(_folder, fileName));
        }
    }
}