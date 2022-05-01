using System;
using System.Collections.Generic;

namespace PbScientInfo
{
	class BGRPixel
	{
		/// <summary>
		/// Represents the Red value of the pixel
		/// </summary>
		private byte r;
		/// <summary>
		/// Represents the Green value of the pixel
		/// </summary>
		private byte g;
		/// <summary>
		/// Represents the Blue value of the pixel
		/// </summary>
		private byte b;

		/// <summary>
		/// Gets the Red value of the pixel
		/// </summary>
		public byte R
		{
			get { return this.r; }
			set { this.r = value; }
		}
		/// <summary>
		/// Gets the Green value of the pixel
		/// </summary>
		public byte G
		{
			get { return this.g; }
			set { this.g = value; }
		}
		/// <summary>
		/// Gets the Blue value of the pixel
		/// </summary>
		public byte B
		{
			get { return this.b; }
			set { this.b = value; }
		}
		/// <summary>
		/// Gets the Blue, Green and Red value of the pixel
		/// </summary>
		public (byte, byte, byte) BGR
		{
			get { return (this.b, this.g, this.r); }
		}
		/// <summary>
		/// Gets a shallow copy of the pixel
		/// </summary>
		public BGRPixel Copy
		{
			get
			{
				return new BGRPixel(this.BGR);
			}
		}

		/// <summary>
		/// Create a new black pixel
		/// </summary>
		public BGRPixel()
		{
			(this.b, this.g, this.r) = (0, 0, 0);
		}
		/// <summary>
		/// Create a new pixel
		/// </summary>
		/// <param name="bgr">Blue, green and red values of the pixel</param>
		public BGRPixel((byte, byte, byte) bgr)
		{
			(this.b, this.g, this.r) = bgr;
		}
		/// <summary>
		/// Create a new pixel
		/// </summary>
		/// <param name="b">Blue value of the pixel</param>
		/// <param name="g">Green value of the pixel</param>
		/// <param name="r">Red value of the pixel</param>
		public BGRPixel(byte b, byte g, byte r)
		{
			this.b = b;
			this.g = g;
			this.r = r;
		}

		/// <summary>
		/// Verifies if pixels are the same color
		/// </summary>
		/// <param name="pixel1">First pixel</param>
		/// <param name="pixel2">Second pixel</param>
		/// <returns>If pixels have the same color</returns>
		public static bool operator ==(BGRPixel pixel1, BGRPixel pixel2)
			=> (pixel1 is null && pixel2 is null) ||
			(!(pixel1 is null) && !(pixel2 is null) && pixel1.r == pixel2.r && pixel1.g == pixel2.g && pixel1.b == pixel2.b);
		/// <summary>
		/// Verifies if pixels have different colors
		/// </summary>
		/// <param name="pixel1">First pixel</param>
		/// <param name="pixel2">Second pixel</param>
		/// <returns>If pixels have different colors</returns>
		public static bool operator !=(BGRPixel pixel1, BGRPixel pixel2)
			=> !(pixel1 == pixel2);
		/// <summary>
		/// Create a shallow copy of the pixel
		/// </summary>
		/// <param name="pixel">Pixel to create a copy of</param>
		/// <returns>A shallow copy of the pixel</returns>
		public static BGRPixel operator +(BGRPixel pixel)
			=> new BGRPixel(pixel.BGR);
		/// <summary>
		/// Create a shallow copy of the inverted pixel
		/// </summary>
		/// <param name="pixel">Pixel to create a copy of the inverse</param>
		/// <returns>An inverted copy of the pixel</returns>
		public static BGRPixel operator -(BGRPixel pixel)
			=> new BGRPixel((byte)(255 - pixel.b), (byte)(255 - pixel.g), (byte)(255 - pixel.r));
		/// <summary>
		/// Create a shallow copy of the inverted pixel
		/// </summary>
		/// <param name="pixel">Pixel to create a copy of the inverse</param>
		/// <returns>An inverted copy of the pixel</returns>
		public static BGRPixel operator !(BGRPixel pixel)
			=> -pixel;

		/// <summary>
		/// Create a new RGB pixel from a hue, saturation and value (HSV standart)
		/// </summary>
		/// <param name="hue">Hue</param>
		/// <param name="saturation">Saturation</param>
		/// <param name="value">Value</param>
		/// <returns>A new RGB pixel from HSV</returns>
		public static BGRPixel NewHue(int hue, double saturation = 1, double value = 1)
		{
			hue %= 360;
			saturation = Math.Max(0, Math.Min(1, saturation));
			value = Math.Max(0, Math.Min(1, value));

			double c = value * saturation;
			double x = c * (1 - Math.Abs((hue / 60.0) % 2 - 1));
			double m = value - c;

			double r_, g_, b_;
			if(hue < 60) (r_, g_, b_) = (c, x, 0);
			else if(hue < 120) (r_, g_, b_) = (x, c, 0);
			else if(hue < 180) (r_, g_, b_) = (0, c, x);
			else if(hue < 240) (r_, g_, b_) = (0, x, c);
			else if(hue < 300) (r_, g_, b_) = (x, 0, c);
			else if(hue < 360) (r_, g_, b_) = (c, 0, x);
			else (r_, g_, b_) = (-m, -m, -m);

			return new BGRPixel((byte)((b_ + m) * 255), (byte)((g_ + m) * 255), (byte)((r_ + m) * 255));
		}

		/// <summary>
		/// Returns a string tha represents the pixel
		/// </summary>
		/// <returns>"BB GG RR"</returns>
		public override string ToString()
		{
			return this.ToHexString();
		}
		/// <summary>
		/// Returns a string tha represents the pixel in hexadecimal
		/// </summary>
		/// <returns>"BB GG RR"</returns>
		public string ToHexString()
		{
			return $"{this.b:X2} {this.g:X2} {this.r:X2}";
		}
		/// <summary>
		/// Returns a string tha represents the pixel in decimal
		/// </summary>
		/// <returns>"BBB GGG RRR"</returns>
		public string ToDecString()
		{
			return $"{this.b:D3} {this.g:D3} {this.r:D3}";
		}

		/// <summary>
		/// Evens out the Blue, Green and Red values of the pixel, making it a shade of gray
		/// </summary>
		public void Grayify()
		{
			int med = (int)Math.Round((this.r + this.g + this.b) / 3.0);
			(this.r, this.g, this.b) = ((byte)med, (byte)med, (byte)med);
		}
		/// <summary>
		/// Inverts the Blue, Green and Red value of the pixel
		/// </summary>
		public void Invert()
		{
			this.r = (byte)(255 - this.r);
			this.g = (byte)(255 - this.g);
			this.b = (byte)(255 - this.b);
		}
		/// <summary>
		/// Merge a pixel to this instance by evening their Blue, Green and Red values two by two
		/// </summary>
		/// <param name="pixel">Pixel to merge</param>
		public void Merge(BGRPixel pixel)
		{
			if(pixel == null) return;

			this.r = (byte)((this.r + pixel.r) / 2);
			this.g = (byte)((this.g + pixel.g) / 2);
			this.b = (byte)((this.b + pixel.b) / 2);
		}
		/// <summary>
		/// Returns a new pixel whose Blue, Green and Red values are the mean of those of the pixels in the list
		/// </summary>
		/// <param name="pixels">List of pixels to fuse</param>
		/// <returns>A pixel whose Blue, Green and Red values are the mean of those of the pixels in the list</returns>
		public static BGRPixel Fuse(List<BGRPixel> pixels)
		{
			int r = 0;
			int g = 0;
			int b = 0;

			foreach(BGRPixel pixel in pixels)
			{
				r += pixel.r;
				g += pixel.g;
				b += pixel.b;
			}

			return new BGRPixel((byte)Math.Round(b / (double)pixels.Count), (byte)Math.Round(g / (double)pixels.Count), (byte)Math.Round(r / (double)pixels.Count));
		}
		/// <summary>
		/// Returns a new pixel whose Blue, Green and Red values are the mean
		/// of those of the pixels in the list according to their weight
		/// </summary>
		/// <param name="pixels">List of pixels to fuse paired with their weight</param>
		/// <param name="ignore_total">If the end pixel's BGR values should NOT be divided by total weight</param>
		/// <returns>A pixel whose Blue, Green and Red values are the mean of those of the weighted pixels in the list</returns>
		public static BGRPixel FuseWeighted(List<(BGRPixel, double)> weighted_pixels, bool ignore_total = false)
		{
			double r = 0;
			double g = 0;
			double b = 0;
			double total_weight = 0;

			foreach((BGRPixel, double) pair in weighted_pixels)
			{
				BGRPixel pixel = pair.Item1;
				double weight = pair.Item2;

				if(weight < 0 && !ignore_total)
				{
					pixel.Invert();
					weight *= -1;
				}

				r += pixel.r * weight;
				g += pixel.g * weight;
				b += pixel.b * weight;
				total_weight += weight;
			}

			if(ignore_total)
				return new BGRPixel((byte)Math.Round(Math.Max(0, Math.Min(255, b))),
									(byte)Math.Round(Math.Max(0, Math.Min(255, g))),
									(byte)Math.Round(Math.Max(0, Math.Min(255, r))));
			else
				return new BGRPixel((byte)Math.Round(b / total_weight),
									(byte)Math.Round(g / total_weight),
									(byte)Math.Round(r / total_weight));
		}
	}
}
