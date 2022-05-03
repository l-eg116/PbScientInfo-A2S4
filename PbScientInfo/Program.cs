using System;
using System.Diagnostics;
using System.Threading;

namespace PbScientInfo
{
	/// <summary>
	/// Main programm
	/// </summary>
	class Program
	{
		/// <summary>
		/// Main loop of the program, runs the UI
		/// </summary>
		/// <param name="args">empty</param>
		static void Main(string[] args)
		{
			Action<string> Print = Console.WriteLine;
			Action<string> PrintNoNL = Console.Write;
			bool quit = false;
			BitMap24Image image = null;
			string loaded = "";
			while(!quit)
			{
				try
				{

					Console.Clear();
					Print("Problème Scientifique Informatique - S4 2022");
					Print("-> Solution de Émile Gatignon <-\n");

					if(loaded != "" && image != null) Print($"`{loaded}` ({image.Height}x{image.Width}) chargé\n");
					switch(Menu("=== Menu Principal ===", new string[]
					{
						"Ouvrir une image",
						"Enregistrer l'image",
						"Modifier l'image...",
						"Appliquer une matrice de convolution...",
						"Générer une image (fractale, QR Code)..."
					}))
					{
						case 1:
							switch(Menu("Quelle image voulez vous ouvrir ?",
								new string[] { "Importer...", "coco", "lac", "hackerman", "doge",
									"B&WSquares", "BGRSquares", "Test1 (bonhomme)", "Test2 (art moderne)", "Test3 (arbre)" }, true, true))
							{
								case 0:
									try
									{
										string path = InputString("\nEntrez le chemin d'accès de l'image");
										image = new BitMap24Image(path);
										loaded = path.Split('\\')[path.Split('\\').Length - 1];
									}
									catch
									{
										Print("\nIl y a eut un problème, vérifiez votre chemin et réessayez");
										for(int i = 5; i > 0; i--)
										{
											Console.Write($"Retour au menu dans {i}... \r");
											Thread.Sleep(1000);
										}
									}
									break;
								case 1:
									image = new BitMap24Image(@"test_items\coco.bmp");
									loaded = "coco";
									break;
								case 2:
									image = new BitMap24Image(@"test_items\lac.bmp");
									loaded = "lac";
									break;
								case 3:
									image = new BitMap24Image(@"test_items\hackerman.bmp");
									loaded = "hackerman";
									break;
								case 4:
									image = new BitMap24Image(@"test_items\doge.bmp");
									loaded = "doge";
									break;
								case 5:
									image = new BitMap24Image(@"test_items\B&Wsquares.bmp");
									loaded = "B&Wsquares";
									break;
								case 6:
									image = new BitMap24Image(@"test_items\RGBsquares.bmp");
									loaded = "RGBsquares";
									break;
								case 7:
									image = new BitMap24Image(@"test_items\Test1.bmp");
									loaded = "Test1";
									break;
								case 8:
									image = new BitMap24Image(@"test_items\Test2.bmp");
									loaded = "Test2";
									break;
								case 9:
									image = new BitMap24Image(@"test_items\Test3.bmp");
									loaded = "Test3";
									break;
								case -1:
								default:
									Print("Retour au menu...");
									break;
							}
							break;
						case 2:
							if(image == null) break;
							try
							{
								string path_save = InputString("\nEntrez le chemin d'enregistrement de votre image");
								image.SaveTo(path_save);
								if(Menu("Ouvrir le résultat ?", new string[] { "Non", "Oui" }, true) == 1)
									Process.Start("explorer.exe", path_save);
							}
							catch
							{
								Print("Une erreur est survenue, vérifiez votre chemin de sauvegarde");
								for(int i = 5; i > 0; i--)
								{
									Console.Write($"Retour au menu dans {i}... \r");
									Thread.Sleep(1000);
								}
							}
							break;
						case 3:
							if(image == null) break;
							switch(Menu(" == Modification d'image == ", new string[]
							{
								"RotateCW > Rotation horaire de 90°",
								"RotateCCW > Rotation antihoraire de 90°",
								"Rotate > Tourne l'image d'un angle quelconque",
								"FlipVertical > Inverse le haut et le bas de l'image",
								"FlipVertical > Inverse la droite et la gauche de l'image",
								"Grayify > Rends l'image en noir et blanc",
								"Invert > Inverse les couleurs de l'image",
								"Resize > Change la taille de l'image",
								"Scale > Applique un facteur à la taille de l'image",
							}, clear: true))
							{
								case 1:
									image.RotateCW();
									break;
								case 2:
									image.RotateCCW();
									break;
								case 3:
									image.Rotate(InputDouble("\nEntrez un angle en degrés pour une rotation dans le sens trigonométrique", "angle (double)"));
									break;
								case 4:
									image.FlipVertical();
									break;
								case 5:
									image.FlipVertical();
									break;
								case 6:
									image.Grayify();
									break;
								case 7:
									image.Invert();
									break;
								case 8:
									image.Resize((uint)InputInt("\nEntrez une nouvelle hauteur et largeur (0 pour inchangée)", "hauteur"),
										(uint)InputInt("", "largeur"));
									break;
								case 9:
									image.Scale(InputDouble("\nEntrez un facteur (<1 pour rétrécir)"));
									break;
								case -1:
								default:
									Print("bye");
									break;
							}
							break;
						case 4:
							if(image == null) break;
							switch(Menu(" == Matrices de convolution == ", new string[]
							{
								"EdgeDetection > Détection de bords",
								"Sharpen > Renforcage",
								"BoxBlur > Flou boite",
								"GaussianBlur > Flou de Gauss",
								"Emboss > Effet 3D"
							}, clear: true))
							{
								case 1:
									image.EdgeDetection();
									break;
								case 2:
									image.Sharpen();
									break;
								case 3:
									image.BoxBlur((uint)InputInt("\nEntrez la taille du flou en pixels"));
									break;
								case 4:
									image.GaussianBlur((uint)InputInt("\nEntrez l'entendue du flou en pixels"),
										InputDouble("Entrez la déviation du flou (1 par défaut)"));
									break;
								case 5:
									image.Emboss();
									break;
								case -1:
								default:
									Print("bye");
									break;
							}
							break;
						case 5:
							switch(Menu(" == Génération d'image == ", new string[]
							{
								"Histogramme de l'image chargée",
								"Ensemble de Mandelbrot",
								"Ensemble de Julia",
								"Tapis de Sierpinski",
								"QR Code (auto)",
								"QR Code (manuel)",
							}, clear: true))
							{
								case 1:
									if(image == null) break;
									image = image.Histrogram();
									loaded = "Histrogramme de " + loaded;
									break;
								case 2:
									image = BitMap24Image.NewMandelbrot(
										(uint)InputInt("\nEntrez la taille de l'image en pixels"), 1,
										InputDouble("Donnez le point central du graphique", "x"), InputDouble("", "y"),
										InputDouble("Donnez la largeur en unités sur le graphique"),
										(uint)InputInt("Entrez la profondeur d'itération"));
									loaded = "MandelbrotSet";
									break;
								case 3:
									image = BitMap24Image.NewJuliaSet(
										(uint)InputInt("\nEntrez la taille de l'image en pixels"), 1,
										InputDouble("Donnez le point source de l'ensemble", "x"), InputDouble("", "y"),
										InputDouble("Donnez le point central du graphique", "x"), InputDouble("", "y"),
										InputDouble("Donnez la largeur en unités sur le graphique"),
										(uint)InputInt("Entrez la profondeur d'itération"));
									loaded = "JuliaSet";
									break;
								case 4:
									image = BitMap24Image.NewSierpinskiCarpet((uint)InputInt("\nEntrez la profondeur d'itération"));
									loaded = "SierpinskiCarpet";
									break;
								case 5:
									image = BitMap24Image.NewQRCode(InputString("\nEntrez le contenu de votre QR Code"));
									loaded = "QRCode";
									break;
								case 6:
									image = BitMap24Image.NewQRCode(
										InputString("\nEntrez le contenu de votre QR Code"),
										(InputString("Choissisez votre correction (L, M, Q ou H)") + " ")[0],
										(uint)InputInt("Choissisez vorte version (entre 1 et 40)"),
										InputString("Choissisez votre encodage (numeric, alphanumeric ou byte)"),
										InputInt("Choissisez votre masque (de 0 à 7)"));
									loaded = "QRCode";
									break;
								case -1:
								default:
									Print("bye");
									break;
							}
							break;
						case -1:
								quit = true;
								break;
							}

					}
				catch(Exception ex)
				{
					Console.WriteLine($"{ex}\n> Exception caught.\n");
					for(int i = 10; i > 0; i--)
					{
						Console.Write($"Resuming in {i}... \r");
						Thread.Sleep(1000);
					}
					Console.Clear();
				}
			}
		}
		/// <summary>
		/// Returns an String queried from the user
		/// </summary>
		/// <param name="question">Phrase above the query line</param>
		/// <param name="variable_name">Variable query name</param>
		/// <returns>A String querried to the user</returns>
		static string InputString(string question = "", string variable_name = "")
		{
			if(question != "") Console.WriteLine(question);
			if(variable_name != "") Console.Write($"{variable_name} ");
			Console.Write("> ");
			return Console.ReadLine();
		}
		/// <summary>
		/// Returns an Int32 queried from the user
		/// </summary>
		/// <param name="question">Phrase above the query line</param>
		/// <param name="variable_name">Variable query name</param>
		/// <returns>An Int32 querried to the user</returns>
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
		/// <summary>
		/// Returns an Double queried from the user
		/// </summary>
		/// <param name="question">Phrase above the query line</param>
		/// <param name="variable_name">Variable query name</param>
		/// <returns>A Double querried to the user</returns>
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
		/// <summary>
		/// Opens up a menu with up to 10 options
		/// </summary>
		/// <param name="title">Title of the menu</param>
		/// <param name="options">String array representing the options, should not exceed 10 in length</param>
		/// <param name="zero_start">Should the menu start at 0 instead of 1</param>
		/// <param name="clear">Should the console be cleared before menu display</param>
		/// <returns>The menu option number querried to the user, -1 if [Esc]</returns>
		static int Menu(string title, string[] options, bool zero_start = false, bool clear = false)
		{
			if(clear) Console.Clear();
			Console.WriteLine(title);
			for(int i = 0; i < Math.Min(10, options.Length); i++)
			{
				int i_ = zero_start ? i : i + 1;
				Console.WriteLine($"[{i_}] - {options[i]}");
			}
			Console.WriteLine("[Esc] pour quitter");

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
