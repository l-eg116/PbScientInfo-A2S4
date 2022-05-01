using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using PbScientInfo;

namespace PbScientInfo_Tests
{
	[TestClass]
	[DeploymentItem("test_items")]
	[DeploymentItem("expected_results")]
	public class UnitTest1_ReadAndWrite
	{
		public bool BytesEqual(byte[] bytes1, byte[] bytes2)
		{
			bool equal = bytes1.Length == bytes2.Length;
			for(int i = 0; i < bytes1.Length && equal; i++)
				equal &= bytes1[i] == bytes2[i];
			return equal;
		}

		[TestMethod]
		public void Test_coco()
		{
			string file_path = @"test_items\coco.bmp";
			string output_path = @"output.bmp";
			string expected_path = @"expected_results\Unit1_Test1_coco.bmp";

			BitMap24Image image = new BitMap24Image(file_path);

			image.SaveTo(output_path);

			byte[] expected = File.ReadAllBytes(expected_path);
			byte[] output = File.ReadAllBytes(output_path);
			Assert.IsTrue(BytesEqual(expected, output));
		}
		[TestMethod]
		public void Test_lac()
		{
			string file_path = @"test_items\lac.bmp";
			string output_path = @"output.bmp";
			string expected_path = @"expected_results\Unit1_Test2_lac.bmp";

			BitMap24Image image = new BitMap24Image(file_path);

			image.SaveTo(output_path);

			byte[] expected = File.ReadAllBytes(expected_path);
			byte[] output = File.ReadAllBytes(output_path);
			Assert.IsTrue(BytesEqual(expected, output));
		}
		[TestMethod]
		public void Test_test1()
		{
			string file_path = @"test_items\Test1.bmp";
			string output_path = @"output.bmp";
			string expected_path = @"expected_results\Unit1_Test3_Test1.bmp";

			BitMap24Image image = new BitMap24Image(file_path);

			image.SaveTo(output_path);

			byte[] expected = File.ReadAllBytes(expected_path);
			byte[] output = File.ReadAllBytes(output_path);
			Assert.IsTrue(BytesEqual(expected, output));
		}
		[TestMethod]
		public void Test_test2()
		{
			string file_path = @"test_items\Test2.bmp";
			string output_path = @"output.bmp";
			string expected_path = @"expected_results\Unit1_Test4_Test2.bmp";

			BitMap24Image image = new BitMap24Image(file_path);

			image.SaveTo(output_path);

			byte[] expected = File.ReadAllBytes(expected_path);
			byte[] output = File.ReadAllBytes(output_path);
			Assert.IsTrue(BytesEqual(expected, output));
		}
	}
}