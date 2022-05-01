using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using PbScientInfo;

namespace PbScientInfo_Tests
{
	[TestClass]
	[DeploymentItem("test_items")]
	[DeploymentItem("expected_results")]
	public class UnitTest3_ModifyImage
	{
		public bool BytesEqual(byte[] bytes1, byte[] bytes2)
		{
			bool equal = bytes1.Length == bytes2.Length;
			for(int i = 0; i < bytes1.Length && equal; i++)
				equal &= bytes1[i] == bytes2[i];
			return equal;
		}

		[TestMethod]
		public void Test_Invert()
		{
			string file_path = @"test_items\coco.bmp";
			string output_path = @"output.bmp";
			string expected_path = @"expected_results\Unit3_Test1_Invert.bmp";

			BitMap24Image image = new BitMap24Image(file_path);

			image.Invert();

			image.SaveTo(output_path);

			byte[] expected = File.ReadAllBytes(expected_path);
			byte[] output = File.ReadAllBytes(output_path);
			Assert.IsTrue(BytesEqual(expected, output));
		}
		[TestMethod]
		public void Test_BoxBlur()
		{
			string file_path = @"test_items\coco.bmp";
			string output_path = @"output.bmp";
			string expected_path = @"expected_results\Unit3_Test2_BoxBlur3.bmp";

			BitMap24Image image = new BitMap24Image(file_path);

			image.BoxBlur(3);

			image.SaveTo(output_path);

			byte[] expected = File.ReadAllBytes(expected_path);
			byte[] output = File.ReadAllBytes(output_path);
			Assert.IsTrue(BytesEqual(expected, output));
		}
		[TestMethod]
		public void Test_Scale()
		{
			string file_path = @"test_items\coco.bmp";
			string output_path = @"output.bmp";
			string expected_path = @"expected_results\Unit3_Test3_Scale075.bmp";

			BitMap24Image image = new BitMap24Image(file_path);

			image.Scale(0.75);

			image.SaveTo(output_path);

			byte[] expected = File.ReadAllBytes(expected_path);
			byte[] output = File.ReadAllBytes(output_path);
			Assert.IsTrue(BytesEqual(expected, output));
		}
		[TestMethod]
		public void Test_Rotate()
		{
			string file_path = @"test_items\coco.bmp";
			string output_path = @"output.bmp";
			string expected_path = @"expected_results\Unit3_Test4_Rotate37.bmp";

			BitMap24Image image = new BitMap24Image(file_path);

			image.Rotate(37);

			image.SaveTo(output_path);

			byte[] expected = File.ReadAllBytes(expected_path);
			byte[] output = File.ReadAllBytes(output_path);
			Assert.IsTrue(BytesEqual(expected, output));
		}
		[TestMethod]
		public void Test_EdgeDetection()
		{
			string file_path = @"test_items\coco.bmp";
			string output_path = @"output.bmp";
			string expected_path = @"expected_results\Unit3_Test5_EdgeDetection.bmp";

			BitMap24Image image = new BitMap24Image(file_path);

			image.EdgeDetection();

			image.SaveTo(output_path);

			byte[] expected = File.ReadAllBytes(expected_path);
			byte[] output = File.ReadAllBytes(output_path);
			Assert.IsTrue(BytesEqual(expected, output));
		}
	}
}