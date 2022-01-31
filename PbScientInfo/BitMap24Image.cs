using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PbScientInfo
{
    class BitMap24Image
    {
        private byte[] bytes;
        private string type;
        private int size;
        private int body_offset;
        private int info_size;
        private int width;
        private int height;
        private RGBPixel[,] pixels;
        
        public string Type
        {
            get { return this.type; }
        }
        public int Size
        {
            get { return this.size; }
        }
        public int BodyOffset
        {
            get { return this.body_offset; }
        }
        public int InfoSize
        {
            get { return this.info_size; }
        }
        public int Width
        {
            get { return this.width; }
        }
        public int Heigth
        {
            get { return this.Heigth; }
        }
        public RGBPixel[,] Pixels
        {
            get { return this.pixels; }
        }

        public BitMap24Image(string path)
        {
            this.bytes = File.ReadAllBytes(path);

            /* [HEADER] */
            this.type = ""; // 0x00 to 0x01
            this.type += Convert.ToChar(this.bytes[0]);
            this.type += Convert.ToChar(this.bytes[1]);

            this.size = 0; // 0x02 to 0x05
            for(int i = 0; i < 4; i++)
                this.size += this.bytes[2 + i] * (i != 0 ? 256 ^ i : 1);

            //bytes 0x06 to 0x09 reserved for editing app

            this.body_offset = 0; // 0x0A to 0x0D
            for(int i = 0; i < 4; i++)
                this.body_offset += this.bytes[10 + i] * (i != 0 ? 256 ^ i : 1);

            /* [HEADER INFO] */
            this.info_size = 0; // 0x0E to 0x11
            for(int i = 0; i < 4; i++)
                this.info_size += this.bytes[14 + i] * (i != 0 ? 256 ^ i : 1);
            
            this.width = 0; // 0x12 to 0x15
            for(int i = 0; i < 4; i++)
                this.width += this.bytes[18 + i] * (i != 0 ? 256 ^ i : 1);
            
            this.height = 0; // 0x16 to 0x19
            for(int i = 0; i < 4; i++)
                this.height += this.bytes[22 + i] * (i != 0 ? 256 ^ i : 1);

            this.pixels = new RGBPixel[this.height, this.width];
            int n = this.body_offset;
            for(int x = 0; x < this.height; x++)
                for(int y = 0; y < this.width; y++)
                {
                    this.pixels[x,y] = new RGBPixel(this.bytes[n], this.bytes[n+1], this.bytes[n+2]);
                    n += 3;
                }
        }

        public override string ToString()
        {
            return $"<BitMap24Image>\n"
                + $"type = {this.type}, size = {this.size} bytes\n"
                + $"header info size = 0x{this.info_size:X}, body offset = 0x{this.body_offset:X}\n"
                + $"heigth = {this.height}, width = {this.width}\n"
                + $"pixels = \n{this.PixelsToString()}";
        }
        public string PixelsToString()
        {
            string output = "";

            int n = 1;
            foreach(RGBPixel pixel in this.pixels)
                output += pixel.ToString() + (n++ % this.width == 0 ? "\n" : "|");

            return output;
        }
    }
}
