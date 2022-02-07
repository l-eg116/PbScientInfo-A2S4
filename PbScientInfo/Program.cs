using System;
using System.IO;

namespace PbScientInfo
{
    class Program
    {
        static void Main(string[] args)
        {
            BitMap24Image image = new BitMap24Image(@"test_items\RGBsquares.bmp");
            Console.WriteLine(image.ToString());

            image.Grayify();
            Console.WriteLine(image.ToString());

            image.SaveTo(@"C:\Users\legco\source\repos\PbScientInfo\PbScientInfo\test_items\OUTPUT.bmp");
        }
    }
}
