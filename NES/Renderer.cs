using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Raylib_cs;

using Color = Raylib_cs.Color;
using Image = Raylib_cs.Image;

namespace NES
{
	internal static class Renderer
	{
		//public static Bitmap image = new(Util.IMAGE_RES_X, Util.IMAGE_RES_Y);
		public static Image image = Raylib.GenImageColor(Util.IMAGE_RES_X, Util.IMAGE_RES_Y, Color.BLACK);
		public static Texture2D screenTexture = Raylib.LoadTextureFromImage(image);
		public static Camera2D camera = new();

		public static Texture2D pixelTexture = Raylib.LoadTextureFromImage(Raylib.GenImageColor(1, 1, Color.WHITE));
		

		public static unsafe void Render()
		{
			Raylib.BeginDrawing();
			Raylib.ClearBackground(Color.BLACK);
			
			//Raylib.DrawText("Hello, world!", 12, 12, 20, Color.WHITE);

			// draw pixels
			camera.zoom = (float)Raylib.GetScreenHeight() / (float)Util.IMAGE_RES_Y;
			camera.offset = new((Raylib.GetScreenWidth() / 2) - ((float)Util.IMAGE_RES_X / 2) * camera.zoom, 0);

			Raylib.BeginMode2D(camera);

			/*for (int x = 0; x < image.Width; x++) 
				for (int y = 0; y < image.Height; y++)
				{
					Raylib.ImageDrawPixel(ref screenImage, x, y, image.GetPixel(x, y).ToRaylibColor());
					//Raylib.DrawTexture(pixelTexture, x, y, image.GetPixel(x, y).ToRaylibColor());
					
				}*/

			

			Raylib.UpdateTexture(screenTexture, image.data);

			Raylib.DrawTexture(screenTexture, 0, 0, Color.WHITE);

			Raylib.EndMode2D();

			// -----------


			// =======================----------------------
			//								DEBUG

			if (Nes.Debug.actorMonitoring)
			{

			}

			if (Nes.Debug.drawFps) Raylib.DrawFPS(5, 5);

			// =======================----------------------


			Raylib.EndDrawing();
		}
	}
}
