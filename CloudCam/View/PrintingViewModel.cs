using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ReactiveUI;

namespace CloudCam.View
{
    public class PrintingViewModel : ReactiveObject
    {
        private static Random _random = new Random();


        public string Message { get; }
        public BitmapSource Image1 { get; }
        public BitmapSource Image2 { get; }
        public BitmapSource Image3 { get; }

        public PrintingViewModel(BitmapSource image1)
        {
            Message = photoPrintingMessages[_random.Next(0, photoPrintingMessages.Length)];
            Image1 = image1;
            Image2 = image1;
            Image3 = image1;
        }

        static private string[] photoPrintingMessages = {
            "Printing in the name of love!",
            "I will always print you!",
            "Take a print on me!",
            "We are the champions of print!",
            "I just printed to say I love you!",
            "Don't stop printing!",
            "Every print you take, every shot you make, I'll be watching you!",
            "Print me tender!",
            "Say a little print for you!",
            "I want to hold your print!",
            "Sweet print o' mine!",
            "Print the way you are!",
            "You've got a print!",
            "Print it like it's hot!",
            "Print me maybe!",
            "Print the rhythm of the night!",
            "Print the way you make me feel!",
            "Print me, baby, one more time!",
            "Print the world!",
            "Print it up!",
            "Print me like you do!",
            "Print, sweet print!",
            "Print, I'm yours!",
            "Print on!",
            "Print it loud!",
            "Print the beat!",
            "Print your story!",
            "Print and shine!",
            "Print the magic!",
            "Print and groove!",
            "Print like a star!",
            "Print and soar!",
            "Print and conquer!",
            "Print the symphony!",
            "Print and ignite!",
            "Print the melody!",
            "Print and celebrate!",
            "Print and sparkle!",
            "Print the anthem!",
            "Print and cherish!",
            "Print the groove!",
            "Print and conquer!",
            "Print and dazzle!",
            "Print and capture!",
            "Print it up, baby!",
            "Print and savor!",
            "Print the moment!",
            "Print and inspire!",
            "Print and cherish forever!",
            "Print and dance!",
            "Print it out, shout it out!",
        };

    }


}
