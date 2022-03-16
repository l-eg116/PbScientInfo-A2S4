using System;
using System.Collections.Generic;

namespace PbScientInfo
{
    class BGRPixel
    {
        private byte r;
        private byte g;
        private byte b;

        public byte R
        {
            get { return this.r; }
            set { this.r = value; }
        }
        public byte G
        {
            get { return this.g; }
            set { this.g = value; }
        }
        public byte B
        {
            get { return this.b; }
            set { this.b = value; }
        }
        public (byte, byte, byte) BGR
        {
            get { return (this.b, this.g, this.r); }
        }
        public BGRPixel Copy
        {
            get
            {
                return new BGRPixel(this.BGR);
            }
        }

        public BGRPixel()
        {
            (this.b, this.g, this.r) = (0, 0, 0);
        }
        public BGRPixel((byte, byte, byte) bgr)
        {
            (this.b, this.g, this.r) = bgr;
        }
        public BGRPixel(byte b, byte g, byte r)
        {
            this.b = b;
            this.g = g;
            this.r = r;
        }

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

        public override string ToString()
        {
            return this.ToHexString();
        }
        public string ToHexString()
        {
            return $"{this.b:X2} {this.g:X2} {this.r:X2}";
        }
        public string ToDecString()
        {
            return $"{this.b:D3} {this.g:D3} {this.r:D3}";
        }

        public void Grayify()
        {
            int med = (this.r + this.g + this.b) / 3;
            (this.r, this.g, this.b) = ((byte)med, (byte)med, (byte)med);
        }
        public void Invert()
        {
            this.r = (byte)(255 - this.r);
            this.g = (byte)(255 - this.g);
            this.b = (byte)(255 - this.b);
        }
        public void Merge(BGRPixel pixel)
        {
            if(pixel == null) return;

            this.r = (byte)((this.r + pixel.r) / 2);
            this.g = (byte)((this.g + pixel.g) / 2);
            this.b = (byte)((this.b + pixel.b) / 2);
        }
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

            return new BGRPixel((byte)(b / pixels.Count), (byte)(g / pixels.Count), (byte)(r / pixels.Count));
        }
        public static BGRPixel FuseWeighted(List<(BGRPixel, double)> weighted_pixels)
        {
            double r = 0;
            double g = 0;
            double b = 0;
            double total_weight = 0;

            foreach((BGRPixel, double) pair in weighted_pixels)
            {
                BGRPixel pixel = pair.Item1;
                double weight = pair.Item2;

                if(weight < 0)
                {
                    pixel.Invert();
                    weight *= -1;
                }

                r += pixel.r * weight;
                g += pixel.g * weight;
                b += pixel.b * weight;
                total_weight += weight;
            }

            return new BGRPixel((byte)(b / total_weight), (byte)(g / total_weight), (byte)(r / total_weight));
        }
    }
}
