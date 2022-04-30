using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NES
{
	/// <summary>
	/// Class full of default commands
	/// </summary>
	public static class DefaultCommands
	{
		[ConsoleCommand("Gets the keycode of the specifed string.")]
		public static int get_keycode(string name)
		{
			try
			{
				return (int)Enum.Parse(typeof(KeyboardKey), name.ToUpper());
			}
			catch (Exception)
			{
				return 0;
			}
		}


		[ConsoleCommand("Executes all the commands in a .exec file.\n.exec files are stored in: ./game/exec")]
		public static void exec([MultipleChoiceArg(new string[] { "thingy1", "thingy2", "thingy again" }, false)] string file)
		{
			const string execPath = "./game/exec";

			if (!Directory.Exists(execPath))
			{
				Directory.CreateDirectory(execPath);
				Nes.Log("Exec folder not found, creating...", Color.Lime);
				return;
			}

			foreach (string path in Directory.GetFiles(execPath)) if (Path.GetExtension(path) == ".exec" && Path.GetFileNameWithoutExtension(path).ToUpper() == file)
				{
					string[] lines = File.ReadAllLines(path);

					for (int i = 0; i < lines.Length; i++)
					{
						lines[i] = lines[i].Trim();

						if (lines[i] == "" || lines[i].StartsWith("//")) continue;

							try
						{ Nes.Console.Execute(lines[i]); }

						catch (Exception e)
						{
							Nes.Log("At line " + (i + 1) + ": " + e.Message, Color.Red);
							return;
						}
					}

					return;
				}

			Nes.Log("File \"" + file + ".exec\" not found!", Color.Red);
		}


		[ConsoleCommand("Prints a string out to the console.")]
		public static void print(string message) => Nes.Log(message);


		[ConsoleCommand("Prints a string with the specified color out to the console.")]
		public static void print(string message, [MultipleChoiceArg(DynamicCommandChoices.COLORS)] string color) => Nes.Log(message, Color.FromName(color));


		[ConsoleCommand("Prints a blank line out to the console.")]
		public static void print() => Nes.Log();


		[ConsoleCommand("Sets the tooltip at the bottom of the screen.\nNote: \"time\" is measured in seconds.")]
		public static void set_tooltip(string text, float time)
		{
			Nes.Console.tooltip = text;
			Nes.Console.tooltipTimer = time;
		}


		[ConsoleCommand("Prints out the summary and usage information about a command.")]
		public static void help([MultipleChoiceArg(DynamicCommandChoices.COMMANDS, false)] string commandName)
		{
			bool commandFoundFlag = false; // overload handling

			foreach (MethodInfo command in Nes.Console.Commands)
			{
				if (commandName.Trim().StartsWith(command.Name.ToUpper()))
				{
					ConsoleCommandAttribute? attribute = command.GetCustomAttribute(typeof(ConsoleCommandAttribute)) as ConsoleCommandAttribute;

					if (attribute == null) throw new Exception("INTERNAL ERROR: COMMAND DOES NOT HAVE ConsoleCommandAttribute");

					Nes.Log();
					if (!commandFoundFlag) Nes.Log("--== \"" + command.Name + "\" help ==--", Color.Gold);
					else Nes.Log("--== OVERLOAD ==--", Color.Gold);
					Nes.Log(attribute.Summary == null ? "No summary information specifed." : attribute.Summary, Color.Aqua);

					// USAGE
					string usage = "";
					foreach (ParameterInfo param in command.GetParameters())
						usage += " <" + param.Name + " (" + param.ParameterType.Name + ")>";
					Nes.Log("Usage: " + command.Name + usage, Color.Aqua);
					if (command.ReturnType != typeof(void)) Nes.Log("Returns: " + command.ReturnType.Name, Color.Aqua);
					Nes.Log();

					commandFoundFlag = true;
				}
			}

			if (!commandFoundFlag) Nes.Log("Command \"" + commandName + "\" not found!", Color.Red);
		}

		[ConsoleCommand("Prints out a generic help message.")]
		public static void help()
		{
			Nes.Log("Use \"help <command name>\" to get\nSpecific information about that\ncommand.", Color.Yellow);
			Nes.Log();
			Nes.Log("Use \"commands\" to get a list of\nall the registered commands.", Color.Yellow);
		}


		[ConsoleCommand("Prints out every registered command.")]
		public static void commands()
		{
			foreach (MethodInfo command in Nes.Console.Commands)
			{
				string parameterString = "";

				foreach (ParameterInfo param in command.GetParameters())
				{
					parameterString += " <" + param.Name + " (" + param.ParameterType.Name + ")>";
				}

				Nes.Log(command.Name + " " + parameterString + (command.ReturnType != typeof(void) ? ("  Returns: " + command.ReturnType.Name) : ""));
			}
		}



		[ConsoleCommand("Clears all resources caches.")]
		public static void reload_resources()
		{
			Nes.ClearCaches();

			Nes.Log("All Caches cleared and resources reloaded!");
		}


		[ConsoleCommand("Closes the game.")]
		public static void quit()
		{
			Nes.QuitGame();

		}



		[ConsoleCommand("Sets if to draw the colliders in the game.")]
		public static void draw_colliders(bool value) => Nes.Debug.drawColliders = value;

		[ConsoleCommand("Returns if colliders are to be drawn.")]
		public static bool draw_colliders() => Nes.Debug.drawColliders;


		[ConsoleCommand("Sets if to draw the current fps in the game.")]
		public static void draw_fps(bool value) => Nes.Debug.drawFps = value;

		[ConsoleCommand("Returns if fps is to be drawn.")]
		public static bool draw_fps() => Nes.Debug.drawFps;



		[ConsoleCommand("Clears all text from the console.")]
		public static void clear_console()
		{
			Nes.Console.Lines.Clear();
			Nes.Log("Console Cleared!");
		}
	}
}
