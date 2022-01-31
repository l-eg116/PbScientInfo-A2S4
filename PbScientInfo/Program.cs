using System;

namespace PbScientInfo
{
    class Program
    {
        static void Main(string[] args)
        {
            BitMap24Image image = new BitMap24Image(@"test_items\RGBsquares.bmp");
            Console.WriteLine(image.ToString());
        }
    }
}
