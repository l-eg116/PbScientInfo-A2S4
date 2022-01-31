using System;
using System.Collections.Generic;
using System.Text;

namespace PbScientInfo
{
    class RGBPixel
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
        
        public RGBPixel()
        {
            (this.r, this.g, this.b) = (0, 0, 0);
        }
        public RGBPixel((byte, byte, byte) rgb)
        {
            (this.r, this.g, this.b) = rgb;
        }
        public RGBPixel(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public override string ToString()
        {
            return this.ToHexString();
        }
        public string ToHexString()
        {
            return $"{this.r:X2} {this.g:X2} {this.b:X2}";
        }
        public string ToDecString()
        {
            return $"{this.r:D3} {this.g:D3} {this.b:D3}";
        }
    }
}
