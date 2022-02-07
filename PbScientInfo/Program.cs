using System;
using System.IO;

namespace PbScientInfo
{
    class Program
    {
        static void Main(string[] args)
        {
            BitMap24Image image = new BitMap24Image(@"test_items\Test2.bmp");
            Console.WriteLine(image.ToString());

            image.RotateCCW();
            Console.WriteLine(image.ToString());

            image.SaveTo(@"C:\Users\legco\Downloads\image.bmp");
        }
    }
}
