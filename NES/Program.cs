using Raylib_cs;
using System.Numerics;
using System.Runtime.InteropServices;

namespace NES
{
	/*static class Program
	{
		public static void Main(string[] args)
		{
			Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
			Raylib.InitWindow(800, 480, "Noxmore Entertainment System");

			Raylib.InitAudioDevice();              // Initialize audio device

			Raylib.SetTargetFPS(120);


			// Create games and tmp directories if there is none.
			Directory.CreateDirectory("./games");
			Directory.CreateDirectory("./tmp");


			// Handle arguments.
			if (args.Length > 0)
			{
				if (File.Exists(args[0]) && args[0].EndsWith(".dll"))
				{
					Util.LoadGame(args[0]);
				}

				if (args.Length > 1) for (int i = 1; i < args.Length; i++)
				{
					switch (args[i])
					{
							case "-console":
								Util.console = true;
								break;
					}
				}
			}

			if (Util.Game == null) for (int x = 0; x < Renderer.image.width; x++) for (int y = 0; y < Renderer.image.height; y++) 
			{
					byte[] colorValues = new byte[3];
					Util.random.NextBytes(colorValues);
				
					Renderer.image.SetPixel(x, y, new(colorValues[0], colorValues[1], colorValues[2], (byte)255));
			}

			while (!Raylib.WindowShouldClose())
			{
				if (Util.Game != null)
				{
					Util.Game.Loop();
				}
				Renderer.Render();
			}

			// Unload current game.
			Util.UnloadGame();

			Raylib.CloseAudioDevice();
			Raylib.CloseWindow();
		}
	}*/
}