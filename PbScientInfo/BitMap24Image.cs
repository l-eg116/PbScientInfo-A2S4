using System;
using System.IO;

namespace PbScientInfo
{
    class BitMap24Image
    {
        private string type;
        private int body_offset;
        private int info_size;
        private int width;
        private int heigth;
        private BGRPixel[,] pixels;

        public string Type
        {
            get { return this.type; }
        }
        public int Size
        {
            get { return 14 + this.info_size + this.heigth * this.width * 3 + this.heigth * ((this.width * 3) % 4 == 0 ? 0 : 4 - (this.width * 3) % 4); }
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
        public BGRPixel[,] Pixels
        {
            get { return this.pixels; }
        }

        public byte[] Bytes
        {
            get
            {
                byte[] bytes = new byte[this.Size];

                /* [HEADER] */
                bytes[0] = Convert.ToByte(this.type[0]);
                bytes[1] = Convert.ToByte(this.type[1]);

                for(int i = 0; i < 4; i++)
                    bytes[2 + i] = ToEndian(this.Size, 4)[i];

                for(int i = 0; i < 4; i++)
                    bytes[6 + i] = Convert.ToByte(new char[] { 'E', 'G', 'A', 'T' }[i]);

                for(int i = 0; i < 4; i++)
                    bytes[10 + i] = ToEndian(this.body_offset, 4)[i];

                /* [HEADER INFO] */
                for(int i = 0; i < 4; i++)
                    bytes[14 + i] = ToEndian(this.info_size, 4)[i];

                for(int i = 0; i < 4; i++)
                    bytes[18 + i] = ToEndian(this.width, 4)[i];

                for(int i = 0; i < 4; i++)
                    bytes[22 + i] = ToEndian(this.heigth, 4)[i];

                for(int i = 0x1A; i < 0x36; i++)
                    bytes[i] = new byte[] { 0x01, 0x00, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC4, 0x0E, 0x00, 0x00, 0xC4, 0x0E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }[i - 0x1A];

                /* [BODY] */
                int n = this.body_offset;
                for(int x = 0; x < this.heigth; x++)
                {
                    for(int y = 0; y < this.width; y++)
                    {
                        (bytes[n + 0], bytes[n + 1], bytes[n + 2]) = this.pixels[x, y].BGR;
                        n += 3;
                    }
                    n += ((n - this.body_offset) % 4 == 0 ? 0 : 4 - (n - this.body_offset) % 4);
                }

                return bytes;
            }
        }
        public BitMap24Image Copy
        {
            get
            {
                BitMap24Image copy = new BitMap24Image();

                copy.type = this.type;
                copy.body_offset = this.body_offset;
                copy.info_size = this.info_size;
                copy.width = this.width;
                copy.heigth = this.heigth;
                copy.pixels = this.pixels;

                return copy;
            }
        }

        private BitMap24Image()
        {

        }
        public BitMap24Image(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);

            /* [HEADER] */
            this.type = ""; // 0x00 to 0x01
            this.type += Convert.ToChar(bytes[0]);
            this.type += Convert.ToChar(bytes[1]);

            //bytes 0x02 to 0x05 for size, ignoried

            //bytes 0x06 to 0x09 reserved for editing app

            this.body_offset = 0; // 0x0A to 0x0D
            for(int i = 0; i < 4; i++)
                this.body_offset += bytes[10 + i] * (int)Math.Pow(256, i);

            /* [HEADER INFO] */
            this.info_size = 0; // 0x0E to 0x11
            for(int i = 0; i < 4; i++)
                this.info_size += bytes[14 + i] * (int)Math.Pow(256, i);

            this.width = 0; // 0x12 to 0x15
            for(int i = 0; i < 4; i++)
                this.width += bytes[18 + i] * (int)Math.Pow(256, i);

            this.heigth = 0; // 0x16 to 0x19
            for(int i = 0; i < 4; i++)
                this.heigth += bytes[22 + i] * (int)Math.Pow(256, i);

            /* [BODY] */
            this.pixels = new BGRPixel[this.heigth, this.width];
            int n = this.body_offset;
            for(int x = 0; x < this.heigth; x++)
            {
                for(int y = 0; y < this.width; y++)
                {
                    this.pixels[x, y] = new BGRPixel(bytes[n], bytes[n + 1], bytes[n + 2]);
                    n += 3;
                }
                n += ((n - this.body_offset) % 4 == 0 ? 0 : 4 - (n - this.body_offset) % 4);
            }
        }
        public void SaveTo(string path)
        {
            File.WriteAllBytes(path, this.Bytes);
        }

        public override string ToString()
        {
            return $"<BitMap24Image>\n"
                + $"type = {this.type}, size = {this.Size} bytes\n"
                + $"header info size = 0x{this.info_size:X}, body offset = 0x{this.body_offset:X}\n"
                + $"heigth = {this.heigth}, width = {this.width}\n"
                + $"pixels = \n{this.PixelsToString()}";
        }
        public string PixelsToString()
        {
            string output = "";

            int n = 1;
            foreach(BGRPixel pixel in this.pixels)
                output += (pixel != null ? pixel.ToString() : " [NULL] ") + (n++ % this.width == 0 ? "\n" : "|");

            return output;
        }
        public string BytesToString()
        {
            string output = "";
            byte[] bytes = this.Bytes;

            output += "[HEADER]\n";
            for(int i = 0x00; i < 0x0E; i++)
                output += $"{bytes[i]:X2} ";
            output += "\n[ INFO ]\n";
            for(int i = 0x0E; i < 0x36; i++)
                output += $"{bytes[i]:X2} ";
            output += "\n[ BODY ]\n";
            for(int i = 0x36; i < bytes.Length; i++)
                output += $"{bytes[i]:X2}{(/*(i - 0x36) / 3 + 1 % this.width == 0 ? "\n" : */(i - 0x36 + 1) % 3 == 0 ? "|" : " ")}";

            return output;
        }

        public void RotateCW()
        {
            BGRPixel[,] copy = this.pixels;
            (this.heigth, this.width) = (this.width, this.heigth);
            this.pixels = new BGRPixel[this.heigth, this.width];
            for(int x = 0; x < this.heigth; x++)
                for(int y = 0; y < this.width; y++)
                    this.pixels[x, y] = copy[this.width - y - 1, x];
        }
        public void RotateCCW()
        {
            BGRPixel[,] copy = this.pixels;
            (this.heigth, this.width) = (this.width, this.heigth);
            this.pixels = new BGRPixel[this.heigth, this.width];
            for(int x = 0; x < this.heigth; x++)
                for(int y = 0; y < this.width; y++)
                    this.pixels[x, y] = copy[y, this.heigth - x - 1];
        }
        public void FlipVertical()
        {
            for(int x = 0; x < this.heigth / 2; x++)
                for(int y = 0; y < this.width; y++)
                    (this.pixels[x, y], this.pixels[this.heigth - x - 1, y]) = (this.pixels[this.heigth - x - 1, y], this.pixels[x, y]);
        }
        public void FlipHorizontal()
        {
            for(int x = 0; x < this.heigth; x++)
                for(int y = 0; y < this.width / 2; y++)
                    (this.pixels[x, y], this.pixels[x, this.width - y - 1]) = (this.pixels[x, this.width - y - 1], this.pixels[x, y]);
        }
        public void Grayify()
        {
            for(int x = 0; x < this.heigth; x++)
                for(int y = 0; y < this.width; y++)
                    this.pixels[x, y].Grayify();
        }
        public void Invert()
        {
            for(int x = 0; x < this.heigth; x++)
                for(int y = 0; y < this.width; y++)
                    this.pixels[x, y].Invert();
        }

        private static byte[] ToEndian(int value, int size = 0)
        {
            if(size < 1) for(size = 1; value >= (int)Math.Pow(256, size); size++) ;
            byte[] bytes = new byte[size];

            for(int i = size - 1; i >= 0; i--)
            {
                value %= (int)Math.Pow(256, i + 1);
                bytes[i] = (byte)(value / (int)Math.Pow(256, i));
            }

            return bytes;
        }
        private static int FromEndian(byte[] bytes)
        {
            int output = 0;
            for(int i = 0; i < bytes.Length; i++)
                output += bytes[i] * (int)Math.Pow(256, i);
            return output;
        }
        public static string BytesToString(string path)
        {
            string output = "";
            byte[] bytes = File.ReadAllBytes(path);

            output += "[HEADER]\n";
            for(int i = 0x00; i < 0x0E; i++)
                output += $"{bytes[i]:X2} ";
            output += "\n[ INFO ]\n";
            for(int i = 0x0E; i < 0x36; i++)
                output += $"{bytes[i]:X2} ";
            output += "\n[ BODY ]\n";
            for(int i = 0x36; i < bytes.Length; i++)
                output += $"{bytes[i]:X2}{((i - 0x36 + 1) % 3 == 0 ? "|" : " ")}";

            return output;
        }
    }
}
