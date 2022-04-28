using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Raylib_cs;
using System.IO;
using System.Media;
//using System.Windows.Extensions;

using Color = System.Drawing.Color;
using Image = System.Drawing.Image;
using Rectangle = System.Drawing.Rectangle;
using Font = Raylib_cs.Font;
using System.Runtime.InteropServices;
using System.Numerics;
//using NES.Properties;
using System.Diagnostics;
using NES.Properties;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;

namespace NES
{

	/// <summary>
	/// The class that contains most of the functionallity of this library.
	/// </summary>
	public static partial class Nes
	{
		static Nes()
		{
			//if (!File.Exists("./defaultFont.ttf")) File.WriteAllBytes("./defaultFont.ttf", Resources.micro_mages);
			//defaultFont = Raylib.LoadFontEx("./defaultFont.ttf", 8, new int[]{0}, 250);
			//defaultFont = Raylib.LoadFont("./defaultFont.ttf");
			//defaultFont = Raylib.GetFontDefault();

			SetFontBindings();
		}


		// PRIVATE ===============================================================================================================

		static NesGame? game = null;
		//static Font defaultFont;


		// Caches

		internal static Dictionary<string, Sound> soundCache = new();
		internal static Dictionary<string, Sprite> spriteCache = new();


		// GENERAL  ===============================================================================================================

		public static NesGame? CurrentGame => game;

		//public static Font DefaultFont => defaultFont;
		


		// private flag to make sure that Init() isn't called twice, might cause problems
		internal static bool inited = false;

		/// <summary>
		/// The main function that starts NES, only call this once, and do it before you do anything else with Nes.
		/// </summary>
		public static void Init(NesGame game)
		{
			if (inited) throw new Exception("Tried to Init Nes more then once!");
			inited = true;

			Nes.game = game;

			windowTitle = game.Name;

			Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
			Raylib.InitWindow(800, 480, windowTitle);

			// Get the default font
			//if (!File.Exists("./defaultFont.png")) File.WriteAllBytes("./defaultFont.png", Resources.font);
			//defaultFont = Raylib.LoadFont("./defaultFont.png");

			Raylib.InitAudioDevice();
			Raylib.SetTargetFPS(120);
			Raylib.SetExitKey(Raylib_cs.KeyboardKey.KEY_VOLUME_UP);

			// commands

			Console.RegisterCommandsInType(typeof(DefaultCommands)); // Default commands
			Console.RegisterCommandsInType(game.GetType()); // Game-specific commands

			game.Start();

			while (!Raylib.WindowShouldClose() && game != null && inited)
			{
				if (Console.openKey != null && IsKeyPressed(Console.openKey.Value))
					Console.open = !Console.open;

				if (Console.open) Console.ConsoleLoop();

				if (!Console.open || (Console.open && !Console.stopsExecution)) // i feel like this is a bad way of doing this but i can't think of another way.
				{
					foreach (TimeSince time in TimeSince.instances) time.Value += DeltaTime; // do timesince stuff

					game.Loop();
				}

				Renderer.Render();
			}

			game?.Stop();

			//Raylib.UnloadFont(defaultFont);
			Raylib.CloseAudioDevice();
			Raylib.CloseWindow();
		}

		static string windowTitle = "";
		public static string WindowTitle { get => windowTitle; set { Raylib.SetWindowTitle(value); windowTitle = value; } }


		/// <summary>
		/// Stops and exits out of the current game.
		/// </summary>
		public static void QuitGame()
		{
			if (game != null)
			{
				game.Stop();
				game = null;
				inited = false;

				ClearCaches();
			}
		}


		// Some cache-related functions

		/// <summary>
		/// Clears the sound cache. (See wiki page on caches)
		/// </summary>
		public static void ClearSoundCache() => soundCache.Clear();

		/// <summary>
		/// Clears the sprite cache. (See wiki page on caches)
		/// </summary>
		public static void ClearSpriteCache() => spriteCache.Clear();


		/// <summary>
		/// Clears all build-in caches
		/// </summary>
		public static void ClearCaches()
		{
			ClearSoundCache();
			ClearSpriteCache();
		}



		/// <summary>
		/// Returns the pixel in the NES that x and y point to.
		/// </summary>
		public static Color GetPixel(int x, int y)
		{
			Raylib_cs.Color color = Renderer.image.GetPixel(x, y);
			return Color.FromArgb(color.r, color.g, color.b, color.a);
		}


		/// <summary>
		/// Returns the max width of the NES screen in NES pixels.
		/// </summary>
		public static int ScreenWidth => Util.IMAGE_RES_X;

		/// <summary>
		/// Returns the max height of the NES screen in NES pixels.
		/// </summary>
		public static int ScreenHeight => Util.IMAGE_RES_Y;


		/// <summary>
		/// Returns the width of the window in pixels.
		/// </summary>
		public static int WindowWidth => Raylib.GetScreenWidth();

		/// <summary>
		/// Returns the height of the window in pixels.
		/// </summary>
		public static int WindowHeight => Raylib.GetScreenHeight();



		/// <summary>
		/// Returns the time elapsed since the last frame.
		/// </summary>
		public static float DeltaTime => Raylib.GetFrameTime();

		/// <summary>
		/// Returns the current frames per second.
		/// </summary>
		public static float FPS => Raylib.GetFPS();

		/// <summary>
		/// Fills the NES screen with the color specified.
		/// </summary>
		/// <param name="color">The color to fill the screen with</param>
		public static void ClearScreen(Color color) { Raylib.ImageClearBackground(ref Renderer.image, color.ToRaylibColor()); }

		/// <returns>A random float between 0.0 and 1.0.</returns>
		public static float RandomFloat() { return Util.random.NextSingle(); }

		/// <returns>A random float between min and max.</returns>
		public static float RandomFloat(float min, float max) { return Util.random.NextSingle() * (max - min) + min; }


		/// <returns>A random double between 0.0 and 1.0.</returns>
		public static double RandomDouble() { return Util.random.NextDouble(); }

		/// <returns>A random double between min and max.</returns>
		public static double RandomDouble(double min, double max) { return Util.random.NextDouble() * (max - min) + min; }


		/// <returns>A random int between min and max.</returns>
		public static int RandomInt(int min, int max) { return Util.random.Next(min, max); }

		/// <returns>A random boolean.</returns>
		public static bool RandomBool() => Util.random.Next(2) == 0;

		/// <returns>true if a random number is less then chance, otherwise false. The chance perameter at 0.0f is a 0% chance to return true, and 1.0f is 100%</returns>
		public static bool RandomChance(float chance) => Util.random.NextSingle() < chance;


		// AUDIO    ===============================================================================================================


		/// <summary>
		/// Tries to retrieve the sound specifed from the built-in sound cache, if there is none, load it from the path specified.
		/// <para>path can be a full path to the file, or start with a dot "." to specify that the root path used, is the directory that the exe being run is in.</para>
		/// </summary>
		/// <returns>The sound from the path specified</returns>
		public static Sound GetSound(string path) => Sound.Get(path);


		/// <returns>The number of sounds playing.</returns>
		public static int GetSoundsPlayingCount() { return Raylib.GetSoundsPlaying(); }



		// DRAWING  ===============================================================================================================

		/// <summary>
		/// Tries to retrieve the sprite specifed from the built-in sprite cache, if there is none, load it from the path specified.
		/// <para>path can be a full path to the file, or start with a dot "." to specify that the root path used, is the directory that the exe being run is in.</para>
		/// </summary>
		/// <returns>The sprite from the path specified</returns>
		public static Sprite GetSprite(string path) => Sprite.Get(path);


		/// <summary>
		/// Sets the pixel in the NES that x and y point to, to the color specified
		/// </summary>
		public static void DrawPixel(Vector2 position, Color color) => DrawPixel((int)position.X, (int)position.Y, color);
		/// <summary>
		/// Sets the pixel in the NES that x and y point to, to the color specified
		/// </summary>
		public static void DrawPixel(int x, int y, Color color) 
		{ 
			if (color.A == 255) Renderer.image.SetPixel(x, y, color.ToRaylibColor());
			else Renderer.image.SetPixel(x, y, TintColor(Renderer.image.GetPixel(x, y).ToNesColor(), color).ToRaylibColor());
		}

		/// <summary>
		/// Draws a rectangle to the NES screen.
		/// </summary>
		public static void DrawRectangle(int x, int y, int width, int height, Color color) { Raylib.ImageDrawRectangle(ref Renderer.image, x, y, width, height, color.ToRaylibColor()); }

		/// <summary>
		/// Draws a rectangle outline to the NES screen.
		/// </summary>
		public static void DrawRectangleOutline(int x, int y, int width, int height, Color color, int thickness = 1) { Raylib.ImageDrawRectangleLines(ref Renderer.image, new(x, y, width, height), thickness, color.ToRaylibColor()); }


		/// <summary>
		/// Draws a string of text to the NES screen.
		/// </summary>
		public static void DrawText(string text, Vector2 position, Color color, Color? dropShadow = null, int spacing = 0, bool small = false) => DrawText(text, (int)position.X, (int)position.Y, color, dropShadow, spacing, small);

		/// <summary>
		/// Draws a string of text to the NES screen.
		/// </summary>
		//public static void DrawText(string text, Vector2 vector, Color color, int scaleMultiplier = 1, int spacing = 1, Font? font = null) => Raylib.ImageDrawTextEx(ref Renderer.image, font != null ? font.Value : defaultFont, font == null ? text.ToUpper() : text, vector, 1 * scaleMultiplier, spacing, color.ToRaylibColor());
		public static void DrawText(string text, int x, int y, Color color, Color? dropShadow = null, int spacing = 0, bool small = false)
		{
			int charX = 0;

			text = text.ToLower();

			for (int i = 0; i < text.Length; i++)
			{
				Bitmap chr = GetFontFileSprite(text[i] < fontBindings.Length ? text[i] : (char)0, small);

				if (dropShadow != null) DrawBitmap(x + charX + 1, y + 1, chr, tint: dropShadow == Color.White ? null : dropShadow);
				DrawBitmap(x + charX, y, chr, tint: color == Color.White ? null : color);
				//Log((byte)text[i]);
				charX += (small ? 6 : 8) + spacing;
			}
		}


		/// <summary>
		/// Draws a line to the NES screen.
		/// </summary>
		public static void DrawLine(int startX, int startY, int endX, int endY, Color color) { Raylib.ImageDrawLine(ref Renderer.image, startX, startY, endX, endY, color.ToRaylibColor()); }


		/// <summary>Draws a Sprite to the NES screen.</summary>
		public static void DrawSprite(Vector2 vector, Sprite sprite, SpriteTransform? transform = null, Color? tint = null) => DrawSprite((int)vector.X, (int)vector.Y, sprite, transform, tint);

		/// <summary>Draws a Sprite to the NES screen.</summary>
		public static void DrawSprite(int x, int y, Sprite sprite, SpriteTransform? transform = null, Color? tint = null) => DrawBitmap(x, y, sprite.image, transform, tint);

		/// <summary>Draws a Bitmap image to the NES screen.</summary>
		public static void DrawBitmap(int x, int y, Bitmap image, SpriteTransform? transform = null, Color? tint = null, PixelEffect? pixelEffect = null)
		{
			bool flipHorFlag = transform != null && transform.Value.horizontalFlip; // see if any axises need to be flipped
			bool flipVerFlag = transform != null && transform.Value.verticalFlip;

			for (int x1 = 0; x1 < image.Width; x1++) for (int y1 = 0; y1 < image.Height; y1++)
					//if (!(x + x1 < 0) && !(x + x1 > ScreenWidth) && !(y + y1 < 0) && !(y + y1 > ScreenHeight))
					{
						// account for flipping horisontally and vertically
						int x2 = flipHorFlag ? image.Width - 1 - x1 : x1;
						int y2 = flipVerFlag ? image.Height - 1 - y1 : y1;

						Color color = image.GetPixel(x2, y2);
						if (color.A > 0)
						{
							if (tint != null) color = TintColor(color, tint.Value); // tint if tint color specified

							DrawPixel(x + x1, y + y1, color);
							if (pixelEffect != null) pixelEffect.Invoke(x, y, color);
						}
					}
		}


		// there might be a better way to do this, but i don't know of it.
		public static Color TintColor(Color color, Color tint) => Color.FromArgb(((int)color.A + (int)tint.A).Clamp(0, 255), (color.R + tint.R - 255).Clamp(0, 255), (color.G + tint.G - 255).Clamp(0, 255), (color.B + tint.B - 255).Clamp(0, 255));



		// INPUT    ===============================================================================================================


		// Gamepad

		/// <param name="gamepad">The index of the gamepad to test. (just use 0 if you don't know what to do)</param>
		/// <param name="button">The button to test.</param>
		/// <returns>If the specified button in the specified gamepad is down</returns>
		public static bool IsButtonDown(int gamepad, GamepadButton button) { return Raylib.IsGamepadButtonDown(gamepad, (Raylib_cs.GamepadButton)button); }

		/// <param name="gamepad">The index of the gamepad to test. (just use 0 if you don't know what to do)</param>
		/// <param name="button">The button to test.</param>
		/// <returns>If the specified button in the specified gamepad is up</returns>
		public static bool IsButtonUp(int gamepad, GamepadButton button) { return Raylib.IsGamepadButtonUp(gamepad, (Raylib_cs.GamepadButton)button); }

		/// <param name="gamepad">The index of the gamepad to test. (just use 0 if you don't know what to do)</param>
		/// <param name="button">The button to test.</param>
		/// <returns>If the specified button in the specified gamepad has been releaced in the current frame</returns>
		public static bool IsButtonReleased(int gamepad, GamepadButton button) { return Raylib.IsGamepadButtonReleased(gamepad, (Raylib_cs.GamepadButton)button); }

		/// <param name="gamepad">The index of the gamepad to test. (just use 0 if you don't know what to do)</param>
		/// <param name="button">The button to test.</param>
		/// <returns>If the specified button in the specified gamepad has been pressed in the current frame</returns>
		public static bool IsButtonPressed(int gamepad, GamepadButton button) { return Raylib.IsGamepadButtonPressed(gamepad, (Raylib_cs.GamepadButton)button); }

		/// <param name="gamepad">The index of the gamepad to test.</param>
		/// <returns>If the specified gamepad is connected and avalible</returns>
		public static bool IsGamepadConnected(int gamepad) { return Raylib.IsGamepadAvailable(gamepad); }

		/// <param name="gamepad">The index of the gamepad to test. (just use 0 if you don't know what to do)</param>
		/// <returns>The name of the specified gamepad.</returns>
		//public static unsafe string GetGamepadName(int gamepad) { return Raylib.GetGamepadName(gamepad); }


		// Keyboard


		/// <returns>true if the specifed key is down, otherwise false.</returns>
		public static bool IsKeyDown(KeyboardKey key) => Raylib.IsKeyDown((Raylib_cs.KeyboardKey)key);

		/// <returns>true if the specifed key is up, otherwise false.</returns>
		public static bool IsKeyUp(KeyboardKey key) => Raylib.IsKeyUp((Raylib_cs.KeyboardKey)key);

		/// <returns>true if the specifed key has just been pressed, otherwise false.</returns>
		public static bool IsKeyPressed(KeyboardKey key) => Raylib.IsKeyPressed((Raylib_cs.KeyboardKey)key);

		/// <returns>true if the specifed key has just been released, otherwise false.</returns>
		public static bool IsKeyReleased(KeyboardKey key) => Raylib.IsKeyReleased((Raylib_cs.KeyboardKey)key);


		// Mouse


		/// <returns>true if the specifed mouse button is down, otherwise false.</returns>
		public static bool IsMouseButtonDown(MouseButton button) => Raylib.IsMouseButtonDown((Raylib_cs.MouseButton)button);

		/// <returns>true if the specifed mouse button is up, otherwise false.</returns>
		public static bool IsMouseButtonUp(MouseButton button) => Raylib.IsMouseButtonUp((Raylib_cs.MouseButton)button);

		/// <returns>true if the specifed mouse button has just been pressed, otherwise false.</returns>
		public static bool IsMouseButtonPressed(MouseButton button) => Raylib.IsMouseButtonPressed((Raylib_cs.MouseButton)button);

		/// <returns>true if the specifed mouse button has just been released, otherwise false.</returns>
		public static bool IsMouseButtonReleased(MouseButton button) => Raylib.IsMouseButtonReleased((Raylib_cs.MouseButton)button);


		/// <returns>How much the mouse wheel has moved in this frame.</returns>
		public static float GetMouseWheelDelta() => Raylib.GetMouseWheelMove();
	}



	// =======================================================---------------------------------------------



	/// <summary>
	/// Used to define a game.
	/// </summary>
	public abstract class NesGame
	{
		/// <summary>
		/// The name of the game.
		/// </summary>
		public abstract string Name { get; }


		/// <summary>
		/// Code that gets run whenever the game gets started up.
		/// </summary>
		public abstract void Start();

		/// <summary>
		/// Code that gets run whenever the game exits.
		/// </summary>
		public abstract void Stop();

		/// <summary>
		/// Code that gets run every frame at 120 times per second, mainly used for rendering, and game logic.
		/// </summary>
		public abstract void Loop();
	}


	


	/// <summary>
	/// Represents a sound that can be played.
	/// </summary>
	public class Sound // Used to wrap a Raylib Sound
	{
		Sound(Raylib_cs.Sound sound) => this.sound = sound;


		//public static Sound Load(string fileName) { return Nes.LoadSound(fileName); }
		//public static Sound Load(UnmanagedMemoryStream stream) { return Nes.LoadSound(stream); }

		float volume = 1;
		/// <summary>
		/// The volume of the sound, 1.0 is max. (1.0 is the default value)
		/// </summary>
		public float Volume { get => volume; set { Raylib.SetSoundVolume(sound, value); volume = value; } }


		float pitch = 1;
		/// <summary>
		/// The pitch of the sound, 1.0 is the default value.
		/// </summary>
		public float Pitch { get => pitch; set { Raylib.SetSoundPitch(sound, value); pitch = value; } }


		/// <summary>
		/// Tries to retrieve the sound specifed from the built-in sound cache, if there is none, load it from the path specified.
		/// <para>path can be a full path to the file, or start with a dot "." to specify that the root path used, is the directory that the exe being run is in.</para>
		/// </summary>
		/// <returns>The sound from the path specified</returns>
		public static Sound Get(string path)
		{
			path = Path.GetFullPath(path);
			try { return Nes.soundCache[path]; }
			catch (KeyNotFoundException)
			{
				Sound sound = new(Raylib.LoadSound(path));
				Nes.soundCache.Add(path, sound);
				return sound;
			}
		}

		/// <summary>
		/// Gets the sound specifed from the path, and plays it, with optional volume and pitch perameters.
		/// </summary>
		public static void Play(string path, float volume = 1, float pitch = 1) 
		{
			Sound sound = Get(path);
			if (volume != 1) sound.Volume = volume;
			if (pitch != 1) sound.Pitch = pitch;
			sound.Play();
		}


		Raylib_cs.Sound sound;

		/// <summary>
		/// Plays the sound specified
		/// </summary>
		public void Play() => Raylib.PlaySound(sound);

		/// <summary>
		/// Pauses the sound specified
		/// </summary>
		public void Pause() => Raylib.PauseSound(sound);

		/// <summary>
		/// Resumes the sound specified
		/// </summary>
		public void Resume() => Raylib.ResumeSound(sound);
	}


	/// <summary>
	/// This is a product of this engine being made for me, this works for my knock-off SNES controller, but i have no idea if it works on other ones.
	/// </summary>
	public enum GamepadButton : int
	{
		UNKNOWN = 0,
		UP = 1,
		RIGHT = 2,
		DOWN = 3,
		LEFT = 4,
		X = 5,
		A = 6,
		B = 7,
		Y = 8,
		LB = 9,
		RB = 10,
		SELECT = 13,
		START = 15
	}


	// KeyboardKey would be here but since it's so big i moved it to another file.


	public enum MouseButton
	{
		MOUSE_BUTTON_LEFT = 0,       // Mouse button left
		MOUSE_BUTTON_RIGHT = 1,       // Mouse button right
		MOUSE_BUTTON_MIDDLE = 2,       // Mouse button middle (pressed wheel)
		MOUSE_BUTTON_SIDE = 3,       // Mouse button side (advanced mouse device)
		MOUSE_BUTTON_EXTRA = 4,       // Mouse button extra (advanced mouse device)
		MOUSE_BUTTON_FORWARD = 5,       // Mouse button fordward (advanced mouse device)
		MOUSE_BUTTON_BACK = 6,       // Mouse button back (advanced mouse device)
	}
}
