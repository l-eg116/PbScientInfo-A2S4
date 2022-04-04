using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

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
        public void Resize(uint new_height, uint new_width)
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
        public void Scale(double factor)
        {
            this.Resize((uint)(this.Height * factor), (uint)(this.Width * factor));
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

        public BitMap24Image Histrogram(bool resize = false)
        {
            int[] r_count = new int[256];
            int[] g_count = new int[256];
            int[] b_count = new int[256];

            foreach(BGRPixel pixel in this.pixels)
            {
                r_count[pixel.R]++;
                g_count[pixel.G]++;
                b_count[pixel.B]++;
            }

            int max_count = 0;
            for(int i = 0; i < 256; i++)
                max_count = Math.Max(Math.Max(max_count, r_count[i]), Math.Max(g_count[i], b_count[i]));

            BitMap24Image histogram = new BitMap24Image();
            histogram.pixels = new BGRPixel[resize ? 256 : max_count, 256];

            for(int y = 0; y < 256; y++)
                for(int x = 0; x < histogram.Height; x++)
                    histogram.pixels[x, y] =
                        new BGRPixel((byte)(x * (double)max_count / histogram.Height < b_count[y] ? 0xFF : 0),
                                    (byte)(x * (double)max_count / histogram.Height < g_count[y] ? 0xFF : 0),
                                    (byte)(x * (double)max_count / histogram.Height < r_count[y] ? 0xFF : 0));

            return histogram;
        }

        private static byte[] ToEndian(int value, uint size = 0)
        {
            if(size < 1) for(size = 1; value >= (int)Math.Pow(256, size); size++) ;
            byte[] bytes = new byte[size];

            for(int i = (int)size - 1; i >= 0; i--)
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

        public static BitMap24Image NewMandelbrot(uint image_width, double hw_ratio = 1, double centerX = 0, double centerY = 0, double reach = 1, uint depth = 250)
        {
            BitMap24Image mandelbrot = new BitMap24Image();

            int image_height = (int)(image_width / hw_ratio);
            mandelbrot.pixels = new BGRPixel[image_height, image_width];

            for(int i = 0; i < image_height; i++)
                for(int j = 0; j < image_width; j++)
                {
                    double x = 2 * j * reach / (double)image_width - reach + centerX;
                    double y = 2 * i * (reach / hw_ratio) / (double)image_height - reach / hw_ratio + centerY;

                    double a = 0, b = 0;

                    int n;
                    for(n = 0; n < depth && Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2)) < 2; n++)
                        (a, b) = (Math.Pow(a, 2) - Math.Pow(b, 2) + x, 2 * a * b + y);

                    mandelbrot.pixels[i, j] = n == depth ? new BGRPixel(0, 0, 0) : BGRPixel.NewHue((int)(360 * n / depth));
                }

            return mandelbrot;
        }
        public static BitMap24Image NewJuliaSet(uint image_width, double hw_ratio = 1, double pointX = 0, double pointY = 0, double centerX = 0, double centerY = 0, double reach = 1.25, uint depth = 250)
        {
            BitMap24Image julia = new BitMap24Image();

            int image_height = (int)(image_width / hw_ratio);
            julia.pixels = new BGRPixel[image_height, image_width];

            for(int i = 0; i < image_height; i++)
                for(int j = 0; j < image_width; j++)
                {
                    double x = 2 * j * reach / (double)image_width - reach + centerX;
                    double y = 2 * i * (reach / hw_ratio) / (double)image_height - reach / hw_ratio + centerY;

                    double a = x, b = y;

                    int n;
                    for(n = 0; n < depth && Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2)) < 2; n++)
                        (a, b) = (Math.Pow(a, 2) - Math.Pow(b, 2) + pointX, 2 * a * b + pointY);

                    julia.pixels[i, j] = n == depth ? new BGRPixel(0, 0, 0) : BGRPixel.NewHue((int)(360 * n / depth));
                }

            return julia;
        }
        public static BitMap24Image NewSierpinskiCarpet(uint levels, BGRPixel foreground = null, BGRPixel background = null)
        {
            if(foreground == null) foreground = new BGRPixel(255, 255, 255);
            if(background == null) background = new BGRPixel(0, 0, 0);

            BitMap24Image carpet = new BitMap24Image();
            carpet.pixels = new BGRPixel[1, 1] { { foreground } };

            for(int n = 0; n < levels; n++)
            {
                carpet.Scale(3);
                for(int i = 1; i < carpet.Height; i += 3)
                    for(int j = 1; j < carpet.Width; j += 3)
                        carpet.pixels[i, j] = background;
            }

            return carpet;
        }

        private static uint QR_FindVersion(string encoding, char correction, int data_length)
        {
            // qr_capacity_table[version, correction, mode]
            int[,,] qr_capacity_table = new int[40, 4, 4];
            int encoding_ = "nabk".IndexOf(encoding[0]);
            int correction_ = "LMQH".IndexOf(correction);

            qr_capacity_table = new int[40, 4, 4]
            { { { 41, 25, 17, 10 }, { 34, 20, 14, 8 }, { 27, 16, 11, 7 }, { 17, 10, 7, 4 } },
            { { 77, 47, 32, 20 }, { 63, 38, 26, 16 }, { 48, 29, 20, 12 }, { 34, 20, 14, 8 } },
            { { 127, 77, 53, 32 }, { 101, 61, 42, 26 }, { 77, 47, 32, 20 }, { 58, 35, 24, 15 } },
            { { 187, 114, 78, 48 }, { 149, 90, 62, 38 }, { 111, 67, 46, 28 }, { 82, 50, 34, 21 } },
            { { 255, 154, 106, 65 }, { 202, 122, 84, 52 }, { 144, 87, 60, 37 }, { 106, 64, 44, 27 } },
            { { 322, 195, 134, 82 }, { 255, 154, 106, 65 }, { 178, 108, 74, 45 }, { 139, 84, 58, 36 } },
            { { 370, 224, 154, 95 }, { 293, 178, 122, 75 }, { 207, 125, 86, 53 }, { 154, 93, 64, 39 } },
            { { 461, 279, 192, 118 }, { 365, 221, 152, 93 }, { 259, 157, 108, 66 }, { 202, 122, 84, 52 } },
            { { 552, 335, 230, 141 }, { 432, 262, 180, 111 }, { 312, 189, 130, 80 }, { 235, 143, 98, 60 } },
            { { 652, 395, 271, 167 }, { 513, 311, 213, 131 }, { 364, 221, 151, 93 }, { 288, 174, 119, 74 } },
            { { 772, 468, 321, 198 }, { 604, 366, 251, 155 }, { 427, 259, 177, 109 }, { 331, 200, 137, 85 } },
            { { 883, 535, 367, 226 }, { 691, 419, 287, 177 }, { 489, 296, 203, 125 }, { 374, 227, 155, 96 } },
            { { 1022, 619, 425, 262 }, { 796, 483, 331, 204 }, { 580, 352, 241, 149 }, { 427, 259, 177, 109 } },
            { { 1101, 667, 458, 282 }, { 871, 528, 362, 223 }, { 621, 376, 258, 159 }, { 468, 283, 194, 120 } },
            { { 1250, 758, 520, 320 }, { 991, 600, 412, 254 }, { 703, 426, 292, 180 }, { 530, 321, 220, 136 } },
            { { 1408, 854, 586, 361 }, { 1082, 656, 450, 277 }, { 775, 470, 322, 198 }, { 602, 365, 250, 154 } },
            { { 1548, 938, 644, 397 }, { 1212, 734, 504, 310 }, { 876, 531, 364, 224 }, { 674, 408, 280, 173 } },
            { { 1725, 1046, 718, 442 }, { 1346, 816, 560, 345 }, { 948, 574, 394, 243 }, { 746, 452, 310, 191 } },
            { { 1903, 1153, 792, 488 }, { 1500, 909, 624, 384 }, { 1063, 644, 442, 272 }, { 813, 493, 338, 208 } },
            { { 2061, 1249, 858, 528 }, { 1600, 970, 666, 410 }, { 1159, 702, 482, 297 }, { 919, 557, 382, 235 } },
            { { 2232, 1352, 929, 572 }, { 1708, 1035, 711, 438 }, { 1224, 742, 509, 314 }, { 969, 587, 403, 248 } },
            { { 2409, 1460, 1003, 618 }, { 1872, 1134, 779, 480 }, { 1358, 823, 565, 348 }, { 1056, 640, 439, 270 } },
            { { 2620, 1588, 1091, 672 }, { 2059, 1248, 857, 528 }, { 1468, 890, 611, 376 }, { 1108, 672, 461, 284 } },
            { { 2812, 1704, 1171, 721 }, { 2188, 1326, 911, 561 }, { 1588, 963, 661, 407 }, { 1228, 744, 511, 315 } },
            { { 3057, 1853, 1273, 784 }, { 2395, 1451, 997, 614 }, { 1718, 1041, 715, 440 }, { 1286, 779, 535, 330 } },
            { { 3283, 1990, 1367, 842 }, { 2544, 1542, 1059, 652 }, { 1804, 1094, 751, 462 }, { 1425, 864, 593, 365 } },
            { { 3517, 2132, 1465, 902 }, { 2701, 1637, 1125, 692 }, { 1933, 1172, 805, 496 }, { 1501, 910, 625, 385 } },
            { { 3669, 2223, 1528, 940 }, { 2857, 1732, 1190, 732 }, { 2085, 1263, 868, 534 }, { 1581, 958, 658, 405 } },
            { { 3909, 2369, 1628, 1002 }, { 3035, 1839, 1264, 778 }, { 2181, 1322, 908, 559 }, { 1677, 1016, 698, 430 } },
            { { 4158, 2520, 1732, 1066 }, { 3289, 1994, 1370, 843 }, { 2358, 1429, 982, 604 }, { 1782, 1080, 742, 457 } },
            { { 4417, 2677, 1840, 1132 }, { 3486, 2113, 1452, 894 }, { 2473, 1499, 1030, 634 }, { 1897, 1150, 790, 486 } },
            { { 4686, 2840, 1952, 1201 }, { 3693, 2238, 1538, 947 }, { 2670, 1618, 1112, 684 }, { 2022, 1226, 842, 518 } },
            { { 4965, 3009, 2068, 1273 }, { 3909, 2369, 1628, 1002 }, { 2805, 1700, 1168, 719 }, { 2157, 1307, 898, 553 } },
            { { 5253, 3183, 2188, 1347 }, { 4134, 2506, 1722, 1060 }, { 2949, 1787, 1228, 756 }, { 2301, 1394, 958, 590 } },
            { { 5529, 3351, 2303, 1417 }, { 4343, 2632, 1809, 1113 }, { 3081, 1867, 1283, 790 }, { 2361, 1431, 983, 605 } },
            { { 5836, 3537, 2431, 1496 }, { 4588, 2780, 1911, 1176 }, { 3244, 1966, 1351, 832 }, { 2524, 1530, 1051, 647 } },
            { { 6153, 3729, 2563, 1577 }, { 4775, 2894, 1989, 1224 }, { 3417, 2071, 1423, 876 }, { 2625, 1591, 1093, 673 } },
            { { 6479, 3927, 2699, 1661 }, { 5039, 3054, 2099, 1292 }, { 3599, 2181, 1499, 923 }, { 2735, 1658, 1139, 701 } },
            { { 6743, 4087, 2809, 1729 }, { 5313, 3220, 2213, 1362 }, { 3791, 2298, 1579, 972 }, { 2927, 1774, 1219, 750 } },
            { { 7089, 4296, 2953, 1817 }, { 5596, 3391, 2331, 1435 }, { 3993, 2420, 1663, 1024 }, { 3057, 1852, 1273, 784 } } };

            uint version = 0;
            while(qr_capacity_table[version, correction_, encoding_] < data_length && version < 40) version++;

            return version + 1;
        }
        private static void QR_IntToBits(bool[] bits, int integer)
        {
            for(int i = bits.Length - 1; i >= 0; i--)
                if(integer >= Math.Pow(2, i))
                    (integer, bits[bits.Length - i - 1]) = (integer - (int)Math.Pow(2, i), true);
                else
                    bits[bits.Length - i - 1] = false;
        }
        private static int QR_CharToAlphanum(char character)
        {
            char[] alphanum_table = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G',
                'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', ' ', '$', '%', '*', '+', '-', '.', '/', ':' };

            int value = -1;
            for(int i = 0; i < 44 && value == -1; i++) if(alphanum_table[i] == character) value = i;

            return value;
        }
        private static List<bool> QR_Encode(string content, string encoding)
        {
            List<bool> bit_string = new List<bool>();

            switch(encoding)
            {
                case "numeric":
                    for(int i = 0; i < content.Length; i += 3)
                    {
                        string substring;
                        if(i + 2 < content.Length) substring = content.Substring(i, 3);
                        else if(i + 1 < content.Length) substring = content.Substring(i, 2);
                        else substring = content.Substring(i, 1);

                        int num = Convert.ToInt32(substring);
                        bool[] bits = new bool[num.ToString().Length == 1 ? 4 : num.ToString().Length == 2 ? 7 : 10];
                        QR_IntToBits(bits, num);
                        bit_string.AddRange(bits);
                    }
                    break;
                case "alphanumeric":
                    for(int i = 0; i < content.Length; i += 2)
                    {
                        string substring;
                        if(i + 1 < content.Length) substring = content.Substring(i, 2);
                        else substring = content.Substring(i, 1);

                        bool[] bits = new bool[substring.Length == 1 ? 6 : 11];
                        if(i + 1 < content.Length) QR_IntToBits(bits, QR_CharToAlphanum(substring[0]) * 45 + QR_CharToAlphanum(substring[1]));
                        else QR_IntToBits(bits, QR_CharToAlphanum(substring[0]));
                        bit_string.AddRange(bits);
                    }
                    break;
                case "byte":
                    byte[] content_bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(content);
                    foreach(byte char_byte in content_bytes)
                    {
                        bool[] bits = new bool[8];
                        QR_IntToBits(bits, char_byte);
                        bit_string.AddRange(bits);
                    }
                    break;
                case "kanji":
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    Encoding shift_jis = Encoding.GetEncoding("shift_jis");
                    foreach(char character in content)
                    {
                        byte[] bytes = shift_jis.GetBytes(character.ToString());
                        int char_val = bytes[0] * 256 +  bytes[1];

                        if(0x8140 <= char_val && char_val <= 0x9FFC) char_val -= 0x8140;
                        else if(0xE040 <= char_val && char_val <= 0xEBBF) char_val -= 0xC140;

                        byte ms_byte = (byte)(char_val / 256);
                        byte ls_byte = (byte)(char_val - ms_byte * 256);

                        bool[] bits = new bool[13];
                        QR_IntToBits(bits, ms_byte * 0xC0 + ls_byte);
                        bit_string.AddRange(bits);
                    }
                    break;
            }

            return bit_string;
        }
    }
}
