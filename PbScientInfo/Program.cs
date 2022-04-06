using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PbScientInfo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(BitMap24Image.QR_DataCodeBlocks(1, 'Q'));
        }
    }
}
