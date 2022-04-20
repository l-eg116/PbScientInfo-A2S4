namespace PbScientInfo
{
	class Program
	{
		static void Main(string[] args)
		{
			BitMap24Image coco = new BitMap24Image("test_items\\coco.bmp");

			coco.EdgeDetection();

			coco.SaveTo(@"C:\Users\legco\source\repos\PbScientInfo\PbScientInfo\test_items\OUTPUT.bmp");
		}
	}
}
