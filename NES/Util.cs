using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Drawing;

namespace NES
{
	internal static class Util
	{
		public const int IMAGE_RES_X = 256, IMAGE_RES_Y = 224;

		public static Random random = new();

		internal static Process proccess = Process.GetCurrentProcess();

		static Assembly? gameAssembly = null;
		static NesGame? game = null;

		public static NesGame? Game { get { return game; } }
		public static Assembly? GameAssembly { get { return gameAssembly; } }

		[Obsolete]
		private static bool LoadGame(string path)
		{
			// Unload game if there is one loaded.
			UnloadGame();

			// Load the game assembly from the file specified.
			Assembly assembly = Assembly.LoadFile(Path.GetFullPath(path));

			// Find the game type in the executeing assembly, and load it into game.
			foreach (Type type in assembly.GetTypes())
			{
				if (type.IsSubclassOf(typeof(NesGame)))
				{	
					// game can be loaded :D
					game = (NesGame?)Activator.CreateInstance(type);
					for (int i = 0; i < assembly.GetFiles().Length; i++)
					{
						assembly.GetFiles()[i].CopyTo(File.Create("./" + i + ".wav"));
						//foreach (string name in assembly.GetManifestResourceNames()) Console.WriteLine(name);
						
					}

					gameAssembly = assembly;
					game.Start();
					return true;
				}
			}
			return false;
		}

		[Obsolete]
		private static void UnloadGame()
		{
			if (game != null)
			{
				game.Stop();
				game = null;
				Nes.soundCache.Clear();
			}
		}
	}


	/// <summary>
	/// For keeping track of time since the the start of the object.
	/// It's started by default.
	/// </summary>
	public sealed class TimeSince
	{
		internal static readonly List<TimeSince> instances = new();


		public TimeSince(float value = 0, bool started = true)
		{
			Value = value;
			if (started) Start();
		}


		/// <summary>The value stores in this TimeSince</summary>
		public float Value { get; set; }

		bool started;


		/// <summary>Starts keeping track of the time on this object</summary>
		public void Start()
		{
			if (!started)
			{
				instances.Add(this);
				started = true;
			}
		}
		/// <summary>Stops keeping track of the time on this object</summary>
		public void Stop()
		{
			if (started)
			{
				instances.Remove(this);
				started = false;
			}
		}


		public static implicit operator float(TimeSince t) => t.Value;
		public static implicit operator TimeSince(float f) => new(f);
	}


	public delegate void PixelEffect(int x, int y, Color color);
}
