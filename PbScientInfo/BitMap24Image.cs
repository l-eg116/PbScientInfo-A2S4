using System;
using System.Collections.Generic;
using System.IO;

namespace PbScientInfo
{
    class BitMap24Image
    {
        private string type;
        private int body_offset;
        private int info_size;
        private BGRPixel[,] pixels;

        public string Type
        {
            get { return this.type; }
        }
        public int Size
        {
            get { return 14 + this.info_size + this.Height * this.Width * 3 + this.Height * ((this.Width * 3) % 4 == 0 ? 0 : 4 - (this.Width * 3) % 4); }
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
            get { return this.pixels.GetLength(1); }
        }
        public int Height
        {
            get { return this.pixels.GetLength(0); }
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
                    bytes[18 + i] = ToEndian(this.Width, 4)[i];

                for(int i = 0; i < 4; i++)
                    bytes[22 + i] = ToEndian(this.Height, 4)[i];

                for(int i = 0x1A; i < 0x36; i++)
                    bytes[i] = new byte[] { 0x01, 0x00, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC4, 0x0E, 0x00, 0x00, 0xC4, 0x0E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }[i - 0x1A];

                /* [BODY] */
                int n = this.body_offset;
                for(int x = 0; x < this.Height; x++)
                {
                    for(int y = 0; y < this.Width; y++)
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
                copy.pixels = this.pixels;

                return copy;
            }
        }

        public BitMap24Image()
        {
            this.type = "BM";
            this.body_offset = 0x36;
            this.info_size = 0x28;
            this.pixels = new BGRPixel[1, 1] { { new BGRPixel() } };
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

            int width = 0; // 0x12 to 0x15
            for(int i = 0; i < 4; i++)
                width += bytes[18 + i] * (int)Math.Pow(256, i);

            int height = 0; // 0x16 to 0x19
            for(int i = 0; i < 4; i++)
                height += bytes[22 + i] * (int)Math.Pow(256, i);

            /* [BODY] */
            this.pixels = new BGRPixel[height, width];
            int n = this.body_offset;
            for(int x = 0; x < this.Height; x++)
            {
                for(int y = 0; y < this.Width; y++)
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
                + $"height = {this.Height}, width = {this.Width}\n"
                + $"pixels = \n{this.PixelsToString()}";
        }
        public string PixelsToString()
        {
            string output = "";

            int n = 1;
            foreach(BGRPixel pixel in this.pixels)
                output += (pixel != null ? pixel.ToString() : " [NULL] ") + (n++ % this.Width == 0 ? "\n" : "|");

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
                output += $"{bytes[i]:X2}{(/*(i - 0x36) / 3 + 1 % this.Width == 0 ? "\n" : */(i - 0x36 + 1) % 3 == 0 ? "|" : " ")}";

            return output;
        }

        public void RotateCW()
        {
            BGRPixel[,] copy = this.pixels;
            this.pixels = new BGRPixel[this.Width, this.Height];
            for(int x = 0; x < this.Height; x++)
                for(int y = 0; y < this.Width; y++)
                    this.pixels[x, y] = copy[this.Width - y - 1, x];
        }
        public void RotateCCW()
        {
            BGRPixel[,] copy = this.pixels;
            this.pixels = new BGRPixel[this.Width, this.Height];
            for(int x = 0; x < this.Height; x++)
                for(int y = 0; y < this.Width; y++)
                    this.pixels[x, y] = copy[y, this.Height - x - 1];
        }
        public void FlipVertical()
        {
            for(int x = 0; x < this.Height / 2; x++)
                for(int y = 0; y < this.Width; y++)
                    (this.pixels[x, y], this.pixels[this.Height - x - 1, y]) = (this.pixels[this.Height - x - 1, y], this.pixels[x, y]);
        }
        public void FlipHorizontal()
        {
            for(int x = 0; x < this.Height; x++)
                for(int y = 0; y < this.Width / 2; y++)
                    (this.pixels[x, y], this.pixels[x, this.Width - y - 1]) = (this.pixels[x, this.Width - y - 1], this.pixels[x, y]);
        }
        public void Grayify()
        {
            for(int x = 0; x < this.Height; x++)
                for(int y = 0; y < this.Width; y++)
                    this.pixels[x, y].Grayify();
        }
        public void Invert()
        {
            for(int x = 0; x < this.Height; x++)
                for(int y = 0; y < this.Width; y++)
                    this.pixels[x, y].Invert();
        }
        public void Resize(int new_height, int new_width)
        {
            double height_ratio = this.Height / (double)new_height;
            double width_ratio = this.Width / (double)new_width;

            BGRPixel[,] resized = new BGRPixel[new_height, this.Width];
            for(int y = 0; y < this.Width; y++)
                for(int x = 0; x < new_height; x++)
                {
                    List<(BGRPixel, double)> weighted_pixels = new List<(BGRPixel, double)>();
                    for(int i = (int)(x * height_ratio); i < Math.Ceiling((x + 1) * height_ratio); i++)
                    {
                        double weight = 0;
                        if(i >= x * height_ratio && i + 1 < (x + 1) * height_ratio) weight = 1 / height_ratio;
                        else if(i < x * height_ratio && i + 1 >= (x + 1) * height_ratio) weight = 1;
                        else if(i < x * height_ratio) weight = (i + 1) - x * height_ratio;
                        else if(i + 1 >= (x + 1) * height_ratio) weight = (x + 1) * height_ratio - i;

                        weighted_pixels.Add((this.pixels[i, y], weight));
                    }

                    resized[x, y] = BGRPixel.FuseWeighted(weighted_pixels);
                }
            this.pixels = resized;

            resized = new BGRPixel[new_height, new_width];
            for(int x = 0; x < new_height; x++)
                for(int y = 0; y < new_width; y++)
                {
                    List<(BGRPixel, double)> weighted_pixels = new List<(BGRPixel, double)>();
                    for(int i = (int)(y * width_ratio); i < Math.Ceiling((y + 1) * width_ratio); i++)
                    {
                        double weight = 0;
                        if(i >= y * width_ratio && i + 1 < (y + 1) * width_ratio) weight = 1 / width_ratio;
                        else if(i < y * width_ratio && i + 1 >= (y + 1) * width_ratio) weight = 1;
                        else if(i < y * width_ratio) weight = (i + 1) - y * width_ratio;
                        else if(i + 1 >= (y + 1) * width_ratio) weight = (y + 1) * width_ratio - i;

                        weighted_pixels.Add((this.pixels[x, i], weight));
                    }

                    resized[x, y] = BGRPixel.FuseWeighted(weighted_pixels);
                }

            this.pixels = resized;
        }
        public void Rotate(double angle, BGRPixel background_pixel = null)
        {
            angle *= -Math.PI / 180;
            if(background_pixel == null) background_pixel = new BGRPixel();

            int new_height = (int)Math.Ceiling(Math.Abs(Math.Sin(angle)) * this.Width + Math.Abs(Math.Cos(angle)) * this.Height);
            int new_width = (int)Math.Ceiling(Math.Abs(Math.Sin(angle)) * this.Height + Math.Abs(Math.Cos(angle)) * this.Width);
            BGRPixel[,] rotated = new BGRPixel[new_height, new_width];

            int centerO_x = this.Height / 2;
            int centerO_y = this.Width / 2;
            int centerR_x = new_height / 2;
            int centerR_y = new_width / 2;

            for(int x = 0; x < this.Height; x++)
                for(int y = 0; y < this.Width; y++)
                    rotated[(int)Math.Round((x - centerO_x) * Math.Cos(angle) - (y - centerO_y) * Math.Sin(angle) + centerR_x),
                        (int)Math.Round((x - centerO_x) * Math.Sin(angle) + (y - centerO_y) * Math.Cos(angle) + centerR_y)]
                        = this.pixels[x, y];

            for(int x = 0; x < new_height; x++)
                for(int y = 0; y < new_width; y++)
                    if(rotated[x, y] == null && (x == 0 || y == 0 || x == new_height - 1 || y == new_width - 1))
                        rotated[x, y] = background_pixel;
                    else if(rotated[x, y] == null && rotated[x + 1, y] != null && rotated[x - 1, y] != null &&
                            rotated[x, y + 1] != null && rotated[x, y - 1] != null)
                        rotated[x, y] = BGRPixel.Fuse(new List<BGRPixel>
                            { rotated[x + 1, y], rotated[x - 1, y], rotated[x, y + 1], rotated[x, y - 1] });

            for(int x = 0; x < new_height; x++)
                for(int y = 0; y < new_width; y++)
                    if(rotated[x, y] == null)
                        rotated[x, y] = background_pixel;

            this.pixels = rotated;
        }
        public void ApplyConvolution(double[,] matrix, string edge_mode = "kernel_crop")
        {
            if(matrix == null || matrix.GetLength(0) != matrix.GetLength(1) || matrix.GetLength(0) % 2 == 0)
                return;

            BGRPixel[,] copy = (BGRPixel[,])this.pixels.Clone();
            int matrix_radius = matrix.GetLength(0) / 2;

            for(int x = 0; x < this.Height; x++)
                for(int y = 0; y < this.Width; y++)
                {
                    List<(BGRPixel, double)> weighted_pixels = new List<(BGRPixel, double)>();

                    for(int i = 0; i < matrix_radius * 2 + 1; i++)
                        for(int j = 0; j < matrix_radius * 2 + 1; j++)
                            switch(edge_mode)
                            {
                                case "kernel_crop":
                                default:
                                    if(x + i - matrix_radius >= 0 && x + i - matrix_radius < this.Height &&
                                        y + j - matrix_radius >= 0 && y + j - matrix_radius < this.Width)
                                        weighted_pixels.Add((copy[x + i - matrix_radius, y + j - matrix_radius].Copy, matrix[i, j]));
                                    break;
                            }
                    this.pixels[x, y] = BGRPixel.FuseWeighted(weighted_pixels);
                }
        }
        public void EdgeDetection()
        {
            this.ApplyConvolution(new double[3, 3] { { -1, -1, -1 }, { -1, 8, -1 }, { -1, -1, -1 } });
        }
        public void Sharpen()
        {
            this.ApplyConvolution(new double[3, 3] { { -1, -1, -1 }, { -1, 9, -1 }, { -1, -1, -1 } });
        }
        public void BoxBlur(uint reach = 1)
        {
            double[,] matrix = new double[reach * 2 + 1, reach * 2 + 1];
            
            for(int i = 0; i < reach * 2 + 1; i++)
                for(int j = 0; j < reach * 2 + 1; j++)
                    matrix[i, j] = 1;

            this.ApplyConvolution(matrix);
        }
        public void GaussianBlur(uint reach = 2, double deviation = 1)
        {
            if(deviation == 0) return;

            double[,] matrix = new double[reach * 2 + 1, reach * 2 + 1];

            for(int i = 0; i < reach * 2 + 1; i++)
                for(int j = 0; j < reach * 2 + 1; j++)
                    matrix[i, j] = Math.Exp(-(Math.Pow(i - reach, 2) + Math.Pow(j - reach, 2)) / (2 * Math.Pow(deviation, 2))) / (2 * Math.PI * Math.Pow(deviation, 2));

            this.ApplyConvolution(matrix);
        }
        public void Emboss(uint reach = 1)
        {
            double[,] matrix = new double[reach * 2 + 1, reach * 2 + 1];

            for(int i = 0; i < reach * 2 + 1; i++)
                if(i - reach < 0) matrix[i, i] = -1;
                else if(i - reach > 0) matrix[i, i] = 1;

            this.ApplyConvolution(matrix);
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
