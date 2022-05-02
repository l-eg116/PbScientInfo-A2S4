using System;

namespace PbScientInfo
{
	class Program
	{
		static void Main(string[] args)
		{
			
		}
		static string InputString(string question = "", string variable_name = "")
		{
			if(question != "") Console.WriteLine(question);
			if(variable_name != "") Console.Write($"{variable_name} ");
			Console.Write("> ");
			return Console.ReadLine();
		}
		static int InputInt(string question = "", string variable_name = "")
		{
			if(question != "") Console.WriteLine(question);
			while(true)
			{
				if(variable_name != "") Console.Write($"{variable_name} ");
				Console.Write("> ");

				try { return int.Parse(Console.ReadLine()); }
				catch(FormatException) { continue; }
			}
		}
		static double InputDouble(string question = "", string variable_name = "")
		{
			if(question != "") Console.WriteLine(question);
			while(true)
			{
				if(variable_name != "") Console.Write($"{variable_name} ");
				Console.Write("> ");

				try { return double.Parse(Console.ReadLine()); }
				catch(FormatException) { continue; }
			}
		}
		static int Menu(string title, string[] options, bool zero_start = false, bool clear = false)
		{
			if(clear) Console.Clear();
			Console.WriteLine(title);
			for(int i = 0; i < Math.Min(10, options.Length); i++)
			{
				int i_ = zero_start ? i : i + 1;
				Console.WriteLine($"[{i_}] - {options[i]}");
			}
			Console.WriteLine("[Esc] to exit");

			int n = 0;
			while(n++ < 50)
				switch(Console.ReadKey().Key)
				{
					case ConsoleKey.NumPad0:
					case ConsoleKey.D0:
						if(zero_start && options.Length > 0) return 0;
						else if(options.Length >= 10) return 10;
						else continue;
					case ConsoleKey.NumPad1:
					case ConsoleKey.D1:
						if(options.Length - (zero_start ? 1 : 0) >= 1) return 1;
						else continue;
					case ConsoleKey.NumPad2:
					case ConsoleKey.D2:
						if(options.Length - (zero_start ? 1 : 0) >= 2) return 2;
						else continue;
					case ConsoleKey.NumPad3:
					case ConsoleKey.D3:
						if(options.Length - (zero_start ? 1 : 0) >= 3) return 3;
						else continue;
					case ConsoleKey.NumPad4:
					case ConsoleKey.D4:
						if(options.Length - (zero_start ? 1 : 0) >= 4) return 4;
						else continue;
					case ConsoleKey.NumPad5:
					case ConsoleKey.D5:
						if(options.Length - (zero_start ? 1 : 0) >= 5) return 5;
						else continue;
					case ConsoleKey.NumPad6:
					case ConsoleKey.D6:
						if(options.Length - (zero_start ? 1 : 0) >= 6) return 6;
						else continue;
					case ConsoleKey.NumPad7:
					case ConsoleKey.D7:
						if(options.Length - (zero_start ? 1 : 0) >= 7) return 7;
						else continue;
					case ConsoleKey.NumPad8:
					case ConsoleKey.D8:
						if(options.Length - (zero_start ? 1 : 0) >= 8) return 8;
						else continue;
					case ConsoleKey.NumPad9:
					case ConsoleKey.D9:
						if(options.Length - (zero_start ? 1 : 0) >= 9) return 9;
						else continue;
					case ConsoleKey.Escape:
						return -1;
					default:
						Console.Write($"{n}\r");
						break;
				}
			return -1;
		}
	}
}
