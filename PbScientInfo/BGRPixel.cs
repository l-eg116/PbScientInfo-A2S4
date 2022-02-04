using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
