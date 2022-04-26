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
					bytes[6 + i] = Convert.ToByte(new char[] { 'E', 'G', 'A', 'T' }[i]); // watermark

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
		public void ApplyConvolution(double[,] matrix, bool ignore_total = false, string edge_mode = "kernel_crop")
		{
			if(matrix == null || matrix.GetLength(0) != matrix.GetLength(1) || matrix.GetLength(0) % 2 == 0)
				return;

			BGRPixel[,] copy = (BGRPixel[,])this.pixels.Clone();
			int matrix_radius = matrix.GetLength(0) / 2;
			for(int i = 0; i < matrix_radius; i++)
				for(int j = 0; j < matrix.GetLength(1); j++)
					(matrix[i, j], matrix[matrix_radius * 2 - i, j]) = (matrix[matrix_radius * 2 - i, j], matrix[i, j]);

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
					this.pixels[x, y] = BGRPixel.FuseWeighted(weighted_pixels, ignore_total);
				}
		}
		public void EdgeDetection()
		{
			this.ApplyConvolution(new double[3, 3] { { -1, -1, -1 }, { -1, 8, -1 }, { -1, -1, -1 } }, true);
		}
		public void Sharpen()
		{
			this.ApplyConvolution(new double[3, 3] { { -1, -1, -1 }, { -1, 9, -1 }, { -1, -1, -1 } }, true);
		}
		public void BoxBlur(uint reach = 1)
		{
			double[,] matrix = new double[reach * 2 + 1, reach * 2 + 1];

			for(int i = 0; i < reach * 2 + 1; i++)
				for(int j = 0; j < reach * 2 + 1; j++)
					matrix[i, j] = 1;

			this.ApplyConvolution(matrix, false);
		}
		public void GaussianBlur(uint reach = 2, double deviation = 1)
		{
			if(deviation == 0) return;

			double[,] matrix = new double[reach * 2 + 1, reach * 2 + 1];

			for(int i = 0; i < reach * 2 + 1; i++)
				for(int j = 0; j < reach * 2 + 1; j++)
					matrix[i, j] = Math.Exp(-(Math.Pow(i - reach, 2) + Math.Pow(j - reach, 2)) / (2 * Math.Pow(deviation, 2))) / (2 * Math.PI * Math.Pow(deviation, 2));

			this.ApplyConvolution(matrix, false);
		}
		public void Emboss(uint reach = 1)
		{
			double[,] matrix = new double[reach * 2 + 1, reach * 2 + 1];

			for(int i = 0; i < reach * 2 + 1; i++)
				if(i - reach < 0) matrix[i, i] = -1;
				else if(i - reach > 0) matrix[i, i] = 1;

			this.ApplyConvolution(matrix, false);
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

		public static BitMap24Image NewQRCode(string content, uint version = 0, char correction = 'L', string encoding = "", uint mask = 0)
		{
			// # Auto mode
			// Choosing best encoding
			if(encoding != "numeric" && encoding != "alphanumeric" && encoding != "byte" && encoding != "kanji")
			{
				bool contains_kanji = Regex.IsMatch(content, "^[一-龯]*$");
				bool contains_iso8859 = Regex.IsMatch(content, "^[\x20-\x7E\xA0-\xA3\xA5\xA7\xA9-\xB3\xB5-\xB7\xB9-\xBB\xBF-\xFF\u20AC\u0160\u0161\u017D\u017E\u0152\u0153\u0178]*$");
				bool contains_alphanum = Regex.IsMatch(content, @"^[A-Z0-9\s$%*+\-/.:]*$");
				bool contains_numbers = Regex.IsMatch(content, "^[0-9]*$");

				if(contains_numbers && !contains_alphanum && !contains_iso8859 && !contains_kanji) encoding = "numeric";
				else if(contains_alphanum && !contains_iso8859 && !contains_kanji) encoding = "alphanumeric";
				else if(contains_iso8859 && !contains_kanji) encoding = "byte";
				else if(contains_kanji) encoding = "kanji";
				else encoding = "alphanumeric";
			} // TODO : fix encoding selection choosing wrong encoding (space skipping alphanum ?)

			// Choosing best correction
			correction = correction.ToString().ToUpper()[0];
			if(correction != 'L' && correction != 'M' && correction != 'Q' && correction != 'H')
				correction = 'M';

			// Choosing best version
			if(version == 0 || version > 40)
				version = QR_FindVersion(encoding, correction, content.Length);

			// # Creation of bit string
			// Mode indicator
			bool[] mode_indicator = new bool[] { false, false, false, false };
			switch(encoding)
			{
				case "numeric":
					mode_indicator = new bool[] { false, false, false, true };
					break;
				case "alphanumeric":
					mode_indicator = new bool[] { false, false, true, false };
					break;
				case "byte":
					mode_indicator = new bool[] { false, true, false, false };
					break;
				case "kanji":
					mode_indicator = new bool[] { true, false, false, false };
					break;
			}

			// Character count indicator
			bool[] character_count_indicator = new bool[0];
			if(version <= 9)
				switch(encoding)
				{
					case "numeric":
						character_count_indicator = new bool[10];
						break;
					case "alphanumeric":
						character_count_indicator = new bool[9];
						break;
					case "byte":
						character_count_indicator = new bool[8];
						break;
					case "kanji":
						character_count_indicator = new bool[8];
						break;
				}
			else if(version >= 10 && version <= 26)
				switch(encoding)
				{
					case "numeric":
						character_count_indicator = new bool[12];
						break;
					case "alphanumeric":
						character_count_indicator = new bool[11];
						break;
					case "byte":
						character_count_indicator = new bool[16];
						break;
					case "kanji":
						character_count_indicator = new bool[10];
						break;
				}
			if(version >= 27)
				switch(encoding)
				{
					case "numeric":
						character_count_indicator = new bool[14];
						break;
					case "alphanumeric":
						character_count_indicator = new bool[16];
						break;
					case "byte":
						character_count_indicator = new bool[13];
						break;
					case "kanji":
						character_count_indicator = new bool[12];
						break;
				}
			QR_IntToBits(character_count_indicator, content.Length);

			// Content bit string
			bool[] content_bits = QR_Encode(content, encoding).ToArray();

			// Merging
			bool[] init_bit_string = new bool[mode_indicator.Length + character_count_indicator.Length + content_bits.Length];
			mode_indicator.CopyTo(init_bit_string, 0);
			character_count_indicator.CopyTo(init_bit_string, mode_indicator.Length);
			content_bits.CopyTo(init_bit_string, mode_indicator.Length + character_count_indicator.Length);

			// Adding terminator bits
			int qr_bit_capacity = QR_ErrorCorrectionTable(version, correction)[0] * 8;
			bool[] terminated_bits = new bool[Math.Min(init_bit_string.Length + 4, qr_bit_capacity)];
			init_bit_string.CopyTo(terminated_bits, 0);

			// Making it fit to bytes
			bool[] byted_bits;
			if(terminated_bits.Length % 8 == 0) byted_bits = new bool[terminated_bits.Length];
			else byted_bits = new bool[(terminated_bits.Length / 8 + 1) * 8];
			terminated_bits.CopyTo(byted_bits, 0);

			// Final padding
			bool[] padded_bits = new bool[qr_bit_capacity];
			bool[][] padding = new bool[][] { new bool[] { true, true, true, false, true, true, false, false },
											new bool[] { false, false, false, true, false, false, false, true } };
			for(int i = byted_bits.Length; i < qr_bit_capacity; i += 8)
				padding[i / 8 % 2].CopyTo(padded_bits, i);
			byted_bits.CopyTo(padded_bits, 0);

			// Memory cleanup
			mode_indicator = null;
			character_count_indicator = null;
			content_bits = null;
			init_bit_string = null;
			terminated_bits = null;
			byted_bits = null;

			// Converting to bytes
			byte[] qr_bytes = new byte[padded_bits.Length / 8];
			for(int i = 0; i < padded_bits.Length; i += 8)
				for(int j = 0; j < 8; j++)
					if(padded_bits[i + j])
						qr_bytes[i / 8] += (byte)Math.Pow(2, 7 - j);
			padded_bits = null;

			// # Error correction coding
			// Breaking into blocks
			int[] ect_line = QR_ErrorCorrectionTable(version, correction);
			byte[][,] qr_blocks = new byte[][,] { new byte[ect_line[2], ect_line[3]], new byte[ect_line[4], ect_line[5]] };
			int n = 0;
			for(int grp = 0; grp < 2; grp++)
				for(int blk = 0; blk < qr_blocks[grp].GetLength(0); blk++)
					for(int i = 0; i < qr_blocks[grp].GetLength(0); i++)
						qr_blocks[grp][blk, i] = qr_bytes[n++];

			return new BitMap24Image();
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
		private static bool[] QR_ByteToBitString(byte num)
		{
			bool[] bits = new bool[8];
			for(int i = bits.Length - 1; i >= 0; i--)
				if(num >= Math.Pow(2, i))
					(num, bits[bits.Length - i - 1]) = ((byte)(num - Math.Pow(2, i)), true);
				else
					bits[bits.Length - i - 1] = false;
			return bits;
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
						int char_val = bytes[0] * 256 + bytes[1];

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
		private static int[] QR_ErrorCorrectionTable(uint version, char correction)
		{
			// [v][0] Total Number of Data Codewords for this Version and EC Level
			// [v][1] EC Codewords Per Block
			// [v][2] Number of Blocks in Group 1
			// [v][3] Number of Data Codewords in Each of Group 1's Blocks
			// [v][4] Number of Blocks in Group 2
			// [v][5] Number of Data Codewords in Each of Group 2's Blocks
			int[][] ec_table = new int[][]
				{ new int[] { 19, 7, 1, 19, 0, 0 },
				new int[] { 16, 10, 1, 16, 0, 0 },
				new int[] { 13, 13, 1, 13, 0, 0 },
				new int[] { 9, 17, 1, 9, 0, 0 },
				new int[] { 34, 10, 1, 34, 0, 0 },
				new int[] { 28, 16, 1, 28, 0, 0 },
				new int[] { 22, 22, 1, 22, 0, 0 },
				new int[] { 16, 28, 1, 16, 0, 0 },
				new int[] { 55, 15, 1, 55, 0, 0 },
				new int[] { 44, 26, 1, 44, 0, 0 },
				new int[] { 34, 18, 2, 17, 0, 0 },
				new int[] { 26, 22, 2, 13, 0, 0 },
				new int[] { 80, 20, 1, 80, 0, 0 },
				new int[] { 64, 18, 2, 32, 0, 0 },
				new int[] { 48, 26, 2, 24, 0, 0 },
				new int[] { 36, 16, 4, 9, 0, 0 },
				new int[] { 108, 26, 1, 108, 0, 0 },
				new int[] { 86, 24, 2, 43, 0, 0 },
				new int[] { 62, 18, 2, 15, 2, 16 },
				new int[] { 46, 22, 2, 11, 2, 12 },
				new int[] { 136, 18, 2, 68, 0, 0 },
				new int[] { 108, 16, 4, 27, 0, 0 },
				new int[] { 76, 24, 4, 19, 0, 0 },
				new int[] { 60, 28, 4, 15, 0, 0 },
				new int[] { 156, 20, 2, 78, 0, 0 },
				new int[] { 124, 18, 4, 31, 0, 0 },
				new int[] { 88, 18, 2, 14, 4, 15 },
				new int[] { 66, 26, 4, 13, 1, 14 },
				new int[] { 194, 24, 2, 97, 0, 0 },
				new int[] { 154, 22, 2, 38, 2, 39 },
				new int[] { 110, 22, 4, 18, 2, 19 },
				new int[] { 86, 26, 4, 14, 2, 15 },
				new int[] { 232, 30, 2, 116, 0, 0 },
				new int[] { 182, 22, 3, 36, 2, 37 },
				new int[] { 132, 20, 4, 16, 4, 17 },
				new int[] { 100, 24, 4, 12, 4, 13 },
				new int[] { 274, 18, 2, 68, 2, 69 },
				new int[] { 216, 26, 4, 43, 1, 44 },
				new int[] { 154, 24, 6, 19, 2, 20 },
				new int[] { 122, 28, 6, 15, 2, 16 },
				new int[] { 324, 20, 4, 81, 0, 0,  },
				new int[] { 254, 30, 1, 50, 4, 51 },
				new int[] { 180, 28, 4, 22, 4, 23 },
				new int[] { 140, 24, 3, 12, 8, 13 },
				new int[] { 370, 24, 2, 92, 2, 93 },
				new int[] { 290, 22, 6, 36, 2, 37 },
				new int[] { 206, 26, 4, 20, 6, 21 },
				new int[] { 158, 28, 7, 14, 4, 15 },
				new int[] { 428, 26, 4, 107, 0, 0 },
				new int[] { 334, 22, 8, 37, 1, 38 },
				new int[] { 244, 24, 8, 20, 4, 21 },
				new int[] { 180, 22, 12, 11, 4, 12 },
				new int[] { 461, 30, 3, 115, 1, 116 },
				new int[] { 365, 24, 4, 40, 5, 41 },
				new int[] { 261, 20, 11, 16, 5, 17 },
				new int[] { 197, 24, 11, 12, 5, 13 },
				new int[] { 523, 22, 5, 87, 1, 88 },
				new int[] { 415, 24, 5, 41, 5, 42 },
				new int[] { 295, 30, 5, 24, 7, 25 },
				new int[] { 223, 24, 11, 12, 7, 13 },
				new int[] { 589, 24, 5, 98, 1, 99 },
				new int[] { 453, 28, 7, 45, 3, 46 },
				new int[] { 325, 24, 15, 19, 2, 20 },
				new int[] { 253, 30, 3, 15, 13, 16 },
				new int[] { 647, 28, 1, 107, 5, 108 },
				new int[] { 507, 28, 10, 46, 1, 47 },
				new int[] { 367, 28, 1, 22, 15, 23 },
				new int[] { 283, 28, 2, 14, 17, 15 },
				new int[] { 721, 30, 5, 120, 1, 121 },
				new int[] { 563, 26, 9, 43, 4, 44 },
				new int[] { 397, 28, 17, 22, 1, 23 },
				new int[] { 313, 28, 2, 14, 19, 15 },
				new int[] { 795, 28, 3, 113, 4, 114 },
				new int[] { 627, 26, 3, 44, 11, 45 },
				new int[] { 445, 26, 17, 21, 4, 22 },
				new int[] { 341, 26, 9, 13, 16, 14 },
				new int[] { 861, 28, 3, 107, 5, 108 },
				new int[] { 669, 26, 3, 41, 13, 42 },
				new int[] { 485, 30, 15, 24, 5, 25 },
				new int[] { 385, 28, 15, 15, 10, 16 },
				new int[] { 932, 28, 4, 116, 4, 117 },
				new int[] { 714, 26, 17, 42, 0, 0 },
				new int[] { 512, 28, 17, 22, 6, 23 },
				new int[] { 406, 30, 19, 16, 6, 17 },
				new int[] { 1006, 28, 2, 111, 7, 112 },
				new int[] { 782, 28, 17, 46, 0, 0,  },
				new int[] { 568, 30, 7, 24, 16, 25 },
				new int[] { 442, 24, 34, 13, 0, 0 },
				new int[] { 1094, 30, 4, 121, 5, 122 },
				new int[] { 860, 28, 4, 47, 14, 48 },
				new int[] { 614, 30, 11, 24, 14, 25 },
				new int[] { 464, 30, 16, 15, 14, 16 },
				new int[] { 1174, 30, 6, 117, 4, 118 },
				new int[] { 914, 28, 6, 45, 14, 46 },
				new int[] { 664, 30, 11, 24, 16, 25 },
				new int[] { 514, 30, 30, 16, 2, 17 },
				new int[] { 1276, 26, 8, 106, 4, 107 },
				new int[] { 1000, 28, 8, 47, 13, 48 },
				new int[] { 718, 30, 7, 24, 22, 25 },
				new int[] { 538, 30, 22, 15, 13, 16 },
				new int[] { 1370, 28, 10, 114, 2, 115 },
				new int[] { 1062, 28, 19, 46, 4, 47 },
				new int[] { 754, 28, 28, 22, 6, 23 },
				new int[] { 596, 30, 33, 16, 4, 17 },
				new int[] { 1468, 30, 8, 122, 4, 123 },
				new int[] { 1128, 28, 22, 45, 3, 46 },
				new int[] { 808, 30, 8, 23, 26, 24 },
				new int[] { 628, 30, 12, 15, 28, 16 },
				new int[] { 1531, 30, 3, 117, 10, 118 },
				new int[] { 1193, 28, 3, 45, 23, 46 },
				new int[] { 871, 30, 4, 24, 31, 25 },
				new int[] { 661, 30, 11, 15, 31, 16 },
				new int[] { 1631, 30, 7, 116, 7, 117 },
				new int[] { 1267, 28, 21, 45, 7, 46 },
				new int[] { 911, 30, 1, 23, 37, 24 },
				new int[] { 701, 30, 19, 15, 26, 16 },
				new int[] { 1735, 30, 5, 115, 10, 116 },
				new int[] { 1373, 28, 19, 47, 10, 48 },
				new int[] { 985, 30, 15, 24, 25, 25 },
				new int[] { 745, 30, 23, 15, 25, 16 },
				new int[] { 1843, 30, 13, 115, 3, 116 },
				new int[] { 1455, 28, 2, 46, 29, 47 },
				new int[] { 1033, 30, 42, 24, 1, 25 },
				new int[] { 793, 30, 23, 15, 28, 16 },
				new int[] { 1955, 30, 17, 115, 0, 0 },
				new int[] { 1541, 28, 10, 46, 23, 47 },
				new int[] { 1115, 30, 10, 24, 35, 25 },
				new int[] { 845, 30, 19, 15, 35, 16 },
				new int[] { 2071, 30, 17, 115, 1, 116 },
				new int[] { 1631, 28, 14, 46, 21, 47 },
				new int[] { 1171, 30, 29, 24, 19, 25 },
				new int[] { 901, 30, 11, 15, 46, 16 },
				new int[] { 2191, 30, 13, 115, 6, 116 },
				new int[] { 1725, 28, 14, 46, 23, 47 },
				new int[] { 1231, 30, 44, 24, 7, 25 },
				new int[] { 961, 30, 59, 16, 1, 17 },
				new int[] { 2306, 30, 12, 121, 7, 122 },
				new int[] { 1812, 28, 12, 47, 26, 48 },
				new int[] { 1286, 30, 39, 24, 14, 25 },
				new int[] { 986, 30, 22, 15, 41, 16 },
				new int[] { 2434, 30, 6, 121, 14, 122 },
				new int[] { 1914, 28, 6, 47, 34, 48 },
				new int[] { 1354, 30, 46, 24, 10, 25 },
				new int[] { 1054, 30, 2, 15, 64, 16 },
				new int[] { 2566, 30, 17, 122, 4, 123 },
				new int[] { 1992, 28, 29, 46, 14, 47 },
				new int[] { 1426, 30, 49, 24, 10, 25 },
				new int[] { 1096, 30, 24, 15, 46, 16 },
				new int[] { 2702, 30, 4, 122, 18, 123 },
				new int[] { 2102, 28, 13, 46, 32, 47 },
				new int[] { 1502, 30, 48, 24, 14, 25 },
				new int[] { 1142, 30, 42, 15, 32, 16 },
				new int[] { 2812, 30, 20, 117, 4, 118 },
				new int[] { 2216, 28, 40, 47, 7, 48 },
				new int[] { 1582, 30, 43, 24, 22, 25 },
				new int[] { 1222, 30, 10, 15, 67, 16 },
				new int[] { 2956, 30, 19, 118, 6, 119 },
				new int[] { 2334, 28, 18, 47, 31, 48 },
				new int[] { 1666, 30, 34, 24, 34, 25 },
				new int[] { 1276, 30, 20, 15, 61, 16 } };

			return ec_table[(version - 1) * 4 + "LMQH".IndexOf(correction)];
		}
		private static byte QR_GF256Log(byte integer)
		{
			byte[] table = new byte[] { 0, 1, 25, 2, 50, 26, 198, 3, 223, 51, 238, 27, 104, 199, 75, 4, 100, 224, 14,
				52, 141, 239, 129, 28, 193, 105, 248, 200, 8, 76, 113, 5, 138, 101, 47, 225, 36, 15, 33, 53, 147, 142,
				218, 240, 18, 130, 69, 29, 181, 194, 125, 106, 39, 249, 185, 201, 154, 9, 120, 77, 228, 114, 166, 6,
				191, 139, 98, 102, 221, 48, 253, 226, 152, 37, 179, 16, 145, 34, 136, 54, 208, 148, 206, 143, 150, 219,
				189, 241, 210, 19, 92, 131, 56, 70, 64, 30, 66, 182, 163, 195, 72, 126, 110, 107, 58, 40, 84, 250, 133,
				186, 61, 202, 94, 155, 159, 10, 21, 121, 43, 78, 212, 229, 172, 115, 243, 167, 87, 7, 112, 192, 247, 140,
				128, 99, 13, 103, 74, 222, 237, 49, 197, 254, 24, 227, 165, 153, 119, 38, 184, 180, 124, 17, 68, 146, 217,
				35, 32, 137, 46, 55, 63, 209, 91, 149, 188, 207, 205, 144, 135, 151, 178, 220, 252, 190, 97, 242, 86, 211,
				171, 20, 42, 93, 158, 132, 60, 57, 83, 71, 109, 65, 162, 31, 45, 67, 216, 183, 123, 164, 118, 196, 23, 73,
				236, 127, 12, 111, 246, 108, 161, 59, 82, 41, 157, 85, 170, 251, 96, 134, 177, 187, 204, 62, 90, 203, 89,
				95, 176, 156, 169, 160, 81, 11, 245, 22, 235, 122, 117, 44, 215, 79, 174, 213, 233, 230, 231, 173, 232,
				116, 214, 244, 234, 168, 80, 88, 175 };

			return table[integer - 1];
		}
		private static byte QR_GF256AntiLog(byte exp)
		{
			byte[] table = new byte[] { 1, 2, 4, 8, 16, 32, 64, 128, 29, 58, 116, 232, 205, 135, 19, 38, 76, 152,
				45, 90, 180, 117, 234, 201, 143, 3, 6, 12, 24, 48, 96, 192, 157, 39, 78, 156, 37, 74, 148, 53, 106,
				212, 181, 119, 238, 193, 159, 35, 70, 140, 5, 10, 20, 40, 80, 160, 93, 186, 105, 210, 185, 111, 222,
				161, 95, 190, 97, 194, 153, 47, 94, 188, 101, 202, 137, 15, 30, 60, 120, 240, 253, 231, 211, 187, 107,
				214, 177, 127, 254, 225, 223, 163, 91, 182, 113, 226, 217, 175, 67, 134, 17, 34, 68, 136, 13, 26, 52,
				104, 208, 189, 103, 206, 129, 31, 62, 124, 248, 237, 199, 147, 59, 118, 236, 197, 151, 51, 102, 204,
				133, 23, 46, 92, 184, 109, 218, 169, 79, 158, 33, 66, 132, 21, 42, 84, 168, 77, 154, 41, 82, 164, 85,
				170, 73, 146, 57, 114, 228, 213, 183, 115, 230, 209, 191, 99, 198, 145, 63, 126, 252, 229, 215, 179, 123,
				246, 241, 255, 227, 219, 171, 75, 150, 49, 98, 196, 149, 55, 110, 220, 165, 87, 174, 65, 130, 25, 50,
				100, 200, 141, 7, 14, 28, 56, 112, 224, 221, 167, 83, 166, 81, 162, 89, 178, 121, 242, 249, 239, 195, 155,
				43, 86, 172, 69, 138, 9, 18, 36, 72, 144, 61, 122, 244, 245, 247, 243, 251, 235, 203, 139, 11, 22, 44, 88,
				176, 125, 250, 233, 207, 131, 27, 54, 108, 216, 173, 71, 142, 1 };

			return table[exp];
		}
		private static byte[] QR_MultiplyGF256Polynomials(byte[] p1, byte[] p2)
		{
			// p1 and p2 are in alpha form
			byte[] prod = new byte[p1.Length + p2.Length - 1];
			byte[,] prods = new byte[p1.Length + p2.Length - 1, p1.Length];

			for(int i = 0; i < p1.Length; i++)
				for(int j = 0; j < p2.Length; j++)
					prods[i + j, i] = (byte)((p1[i] + p2[j]) % 255);

			// prod in alpha form
			// prods in alpha form
			for(int i = 0; i < prod.Length; i++)
			{
				int j = Math.Min(i, p1.Length - 1);
				byte sum = QR_GF256AntiLog(prods[i, j]);
				for(j = j - 1; j >= 0 && j > i - p2.Length; j--)
					sum ^= QR_GF256AntiLog(prods[i, j]);
				prod[i] = QR_GF256Log(sum);
			}

			return prod;
		}
		private static byte[] QR_CorrGeneratorPolynomial(int degre)

		{
			byte[] pol = new byte[] { 0, 0 };

			for(int i = 1; i < degre; i++)
				pol = QR_MultiplyGF256Polynomials(pol, new byte[] { (byte)i, 0 });

			return pol;
		}
		private static byte[] QR_CorrectionPolynomial(byte[] message, uint corr_pol_len)
		{
			byte[] correction = new byte[message.Length + corr_pol_len]; // int notation
			for(int i = 0; i < message.Length; i++)
				correction[correction.Length - 1 - i] = message[i];
			byte[] generator = QR_CorrGeneratorPolynomial((int)corr_pol_len); // aplha notation

			for(int i = message.Length + (int)corr_pol_len - 1; i >= corr_pol_len; i--)
			{
				if(correction[i] != 0)
				{
					byte mult_factor = QR_GF256Log(correction[i]); // alpha notation
					for(int j = generator.Length - 1; j >= 0; j--)
					{
						correction[i - (generator.Length - 1) + j] ^= QR_GF256AntiLog((byte)((generator[j] + mult_factor) % 255));
					}
				}
			}

			byte[] trimed = new byte[corr_pol_len];
			for(int i = 0; i < corr_pol_len; i++)
				trimed[i] = correction[i];

			return trimed;
		}
		private static int QR_RemainderBits(uint version)
		{
			int[] table = new int[] { 0, 7, 7, 7, 7, 7, 0, 0, 0, 0, 0, 0, 0, 3, 3, 3, 3, 3, 3, 3,
									4, 4, 4, 4, 4, 4, 4, 3, 3, 3, 3, 3, 3, 3, 0, 0, 0, 0, 0, 0 };
			return table[version - 1];
		}
		private static int[] QR_AlignementLocations(uint version)
		{
			int[][] table = new int[][] {
				new int[] { 6 },
				new int[] { 6, 18 },
				new int[] { 6, 22 },
				new int[] { 6, 26 },
				new int[] { 6, 30},
				new int[] { 6, 34 },
				new int[] { 6, 22, 38 },
				new int[] { 6, 24, 42 },
				new int[] { 6, 26, 46 },
				new int[] { 6, 28, 50 },
				new int[] { 6, 30, 54 },
				new int[] { 6, 32, 58 },
				new int[] { 6, 34, 62  },
				new int[] { 6, 26, 46, 66 },
				new int[] { 6, 26, 48, 70 },
				new int[] { 6, 26, 50, 74 },
				new int[] { 6, 30, 54, 78 },
				new int[] { 6, 30, 56, 82 },
				new int[] { 6, 30, 58, 86 },
				new int[] { 6, 34, 62, 90 },
				new int[] { 6, 28, 50, 72, 94 },
				new int[] { 6, 26, 50, 74, 98 },
				new int[] { 6, 30, 54, 78, 102 },
				new int[] { 6, 28, 54, 80, 106 },
				new int[] { 6, 32, 58, 84, 110 },
				new int[] { 6, 30, 58, 86, 114 },
				new int[] { 6, 34, 62, 90, 118 },
				new int[] { 6, 26, 50, 74, 98, 122 },
				new int[] { 6, 30, 54, 78, 102, 126 },
				new int[] { 6, 26, 52, 78, 104, 130 },
				new int[] { 6, 30, 56, 82, 108, 134 },
				new int[] { 6, 34, 60, 86, 112, 138 },
				new int[] { 6, 30, 58, 86, 114, 142 },
				new int[] { 6, 34, 62, 90, 118, 146 },
				new int[] { 6, 30, 54, 78, 102, 126, 150 },
				new int[] { 6, 24, 50, 76, 102, 128, 154 },
				new int[] { 6, 28, 54, 80, 106, 132, 158 },
				new int[] { 6, 32, 58, 84, 110, 136, 162 },
				new int[] { 6, 26, 54, 82, 110, 138, 166 },
				new int[] { 6, 30, 58, 86, 114, 142, 170 } };

			return table[version - 1];
		}
	}
}
