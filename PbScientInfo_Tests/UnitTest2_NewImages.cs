using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using PbScientInfo;

namespace PbScientInfo_Tests
{
	[TestClass]
	[DeploymentItem("test_items")]
	[DeploymentItem("expected_results")]
	public class UnitTest2_NewImages
	{
		public bool BytesEqual(byte[] bytes1, byte[] bytes2)
		{
			bool equal = bytes1.Length == bytes2.Length;
			for(int i = 0; i < bytes1.Length && equal; i++)
				equal &= bytes1[i] == bytes2[i];
			return equal;
		}

		[TestMethod]
		public void Test_Mandelbrot()
		{
			string output_path = @"output.bmp";
			string expected_path = @"expected_results\Unit2_Test1.bmp";

			BitMap24Image image = BitMap24Image.NewMandelbrot(1024);

			image.SaveTo(output_path);

			byte[] expected = File.ReadAllBytes(expected_path);
			byte[] output = File.ReadAllBytes(output_path);
			Assert.IsTrue(BytesEqual(expected, output));
		}
		[TestMethod]
		public void Test_Julia()
		{
			string output_path = @"output.bmp";
			string expected_path = @"expected_results\Unit2_Test2_Julia-0.4-0.3.bmp";

			BitMap24Image image = BitMap24Image.NewJuliaSet(1024, 1, 0.4, 0.3);

			image.SaveTo(output_path);

			byte[] expected = File.ReadAllBytes(expected_path);
			byte[] output = File.ReadAllBytes(output_path);
			Assert.IsTrue(BytesEqual(expected, output));
		}
		[TestMethod]
		public void Test_SierpinskiCarpet()
		{
			string output_path = @"output.bmp";
			string expected_path = @"expected_results\Unit2_Test3_SC5.bmp";

			BitMap24Image image = BitMap24Image.NewSierpinskiCarpet(5);

			image.SaveTo(output_path);

			byte[] expected = File.ReadAllBytes(expected_path);
			byte[] output = File.ReadAllBytes(output_path);
			Assert.IsTrue(BytesEqual(expected, output));
		}
		[TestMethod]
		public void Test_QRCode()
		{
			string output_path = @"output.bmp";
			string expected_path = @"expected_results\Unit2_Test4.bmp";

			BitMap24Image image = BitMap24Image.NewQRCode("Hello world, what a lovely day");

			image.SaveTo(output_path);

			byte[] expected = File.ReadAllBytes(expected_path);
			byte[] output = File.ReadAllBytes(output_path);
			Assert.IsTrue(BytesEqual(expected, output));
		}
		[TestMethod]
		public void Test_Histogram()
		{
			string output_path = @"output.bmp";
			string expected_path = @"expected_results\Unit2_Test5_cocoHist.bmp";

			BitMap24Image image = new BitMap24Image(@"test_items\coco.bmp").Histrogram();

			image.SaveTo(output_path);

			byte[] expected = File.ReadAllBytes(expected_path);
			byte[] output = File.ReadAllBytes(output_path);
			Assert.IsTrue(BytesEqual(expected, output));
		}
	}
}