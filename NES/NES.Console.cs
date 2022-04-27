using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NES
{
	public static partial class Nes
	{
		/// <summary>
		/// For storing information about the Nes Console.
		/// </summary>
		public static class Console
		{
			/// <summary>If the console is enabled in this game.</summary>
			public static bool enabled = true;

			/// <summary>If the console being open stops normal game execution, default is true.</summary>
			public static bool stopsExecution = true;


			public static Color backgroundColor = Color.FromArgb(255, 20, 20, 20);


			/// <summary>If a smaller form of the console is visable on the ui, does not remove user input.</summary>
			public static bool uiVisable = true;

			/// <summary>The lines of text in the console.</summary>
			public static List<(string, Color)> Lines { get; } = new();

			/// <summary>The text in the input field in the console.</summary>
			public static string input = "";


			/// <summary>The location of the input text cursor.</summary>
			public static int cursor = 0;

			public static float inputScroll = 0;

			public static float linesYScroll = 0;

			public static float linesXScroll = 0;


			public const int MAX_LINES_ON_SCREEN = 23;


			/// <summary>The previous commands typed into the console.</summary>
			public static List<string> History { get; } = new();

			internal static bool browsingHistory = false; // if the command history is being browsed
			internal static int historyPosition = 0; // which position is the history being browsed at.

			internal static void UpdateHistory(int num) // for updating the history and sending it to the input field, to avoid copy-paste code.
			{
				browsingHistory = true;
				historyPosition = (historyPosition + num).Clamp(0, History.Count);
				input = historyPosition == 0 ? "" : History[historyPosition - 1];
				cursor = input.Length; // set the cursor to the end
			}


			public static MemoryStream OutStream { get; } = new();
			public static StreamWriter Out { get; } = new(OutStream);

			public static bool open = false;

			/// <summary>The lines of text in the console.</summary>
			public static KeyboardKey? openKey = KeyboardKey.GRAVE;



			public static List<MethodInfo> Commands { get; } = new();


			public static List<MethodInfo> GetDefaultCommands() => GetCommandsInType(typeof(DefaultCommands));

			public static void RegisterCommandsInType(Type type) => Commands.AddRange(GetCommandsInType(type));

			static List<MethodInfo> GetCommandsInType(Type type) //: should i make this public?
			{
				List<MethodInfo> commands = new();

				foreach (MethodInfo method in type.GetMethods()) if (method.IsStatic) foreach (Attribute attribute in method.GetCustomAttributes())
						if (attribute is ConsoleCommandAttribute)
							commands.Add(method);

				return commands;
			}


			/// <summary>
			/// Executes a command from a string.
			/// </summary>
			/// <param name="commandString">The command + args to execute</param>
			public static object? Execute(string commandString)
			{
				commandString = commandString.ToUpper().Trim();

				// for dealing with overload command checking.
				bool commandExistsFlag = false;

				foreach (MethodInfo command in Commands)
				{
					if (commandString.StartsWith(command.Name.ToUpper()))
					{
						commandExistsFlag = true;

						string[] tokens = TokenizeCommandString(commandString);
						ParameterInfo[] parameters = command.GetParameters();

						// make sure the amount of parameters is right
						if (parameters.Length != tokens.Length - 1) continue;

						object[] objectParams = new object[parameters.Length];

						// again, for dealing with overloads.
						bool validParametersFlag = true;

						for (int i = 1; i < tokens.Length; i++)
						{
							string token = tokens[i];
							ParameterInfo param = parameters[i - 1];

							// supported types: int, float string bool enum?

							// Parse supported types
							if (param.ParameterType == typeof(int)) try { objectParams[i - 1] = int.Parse(token); } catch (Exception) { validParametersFlag = false; } // throw new ArgumentException(commandString + " | argument " + i + " is not valid!");
							else if (param.ParameterType == typeof(float)) try { objectParams[i - 1] = float.Parse(token); } catch (Exception) { validParametersFlag = false; }
							else if (param.ParameterType == typeof(string)) 
							try 
							{ 
								objectParams[i - 1] = token;
								foreach (Attribute attribute in param.GetCustomAttributes()) if (attribute is MultipleChoiceArgAttribute)
										{
											MultipleChoiceArgAttribute choiceAttribute = (MultipleChoiceArgAttribute)attribute;
											if (!choiceAttribute.Choices.Contains(token) && choiceAttribute.ThrowsError)
												throw new ArgumentException(commandString + " | argument " + i + " has the value \"" + token + "\" which is not allowed for this argument!");
											break;
										}
							} 
							catch (Exception) { validParametersFlag = false; }
							else if (param.ParameterType == typeof(bool)) try { objectParams[i - 1] = bool.Parse(token); } catch (Exception) { validParametersFlag = false; }
							else throw new ArgumentException("Argument " + param.Name + " does not have a supported type! Please report this to the developer!");
						}

						if (!validParametersFlag) continue;

						return command.Invoke(null, objectParams);
					}
				}

				string commandName = commandString.Split(' ')[0];

				if (commandExistsFlag) throw new ArgumentException("No overload of " + commandName + " exists with the specifed parameters.\nType \"help " + commandName + "\" to recieve\nhelp information about this command.");
				throw new ArgumentException("Command not found: " + commandName);
			}


			/// <summary>
			/// Simple Lexer used internally for turing a command string into a list of tokens.
			/// </summary>
			public static string[] TokenizeCommandString(string command)
			{
				List<string> tokens = new();

				bool inString = false;

				int lastToken = 0;

				for (int i = 0; i < command.Length; i++)
				{
					char chr = command[i];

					if (chr == '\"' || chr == '\'') inString = !inString;


					if (chr == ' ' && !inString)
					{
						tokens.Add(command.Substring(lastToken, i - lastToken).TrimStart().Replace("'", "").Replace("\"", "").Replace("\\N", "\n"));

						lastToken = i;
					}

					if (i == command.Length - 1) tokens.Add(command.Substring(lastToken).TrimStart().Replace("'", "").Replace("\"", "").Replace("\\N", "\n"));
				}

				return tokens.ToArray();
			}


			// ====================================================---------------------------------------------------
			//																INTERNAL STUFF
			// ====================================================---------------------------------------------------

			/// <summary>To help with timing the cursor flashing effect.</summary>
			internal static float cursorTimingCounter = 0;

			internal static bool shiftModifier = false;
			internal static bool controlModifier = false;


			/// <summary>Finds the index right before the last word in the console input, used for text navigation.</summary>
			internal static int FindLastWordIndex()
			{
				int index = cursor - 1;

				while (index > 0 && input[index - 1] != ' ') index--;

				return index;
			}

			/// <summary>Finds the index right before the next word in the console input, used for text navigation.</summary>
			internal static int FindNextWordIndex() // copy-paste function because i don't know how to combine these.
			{
				int index = cursor + 1;

				while (index < input.Length && input[index] != ' ') index++;

				return index;
			}


			// ====================================================---------------------------------------------------
			//																Main Console Loop
			// ====================================================---------------------------------------------------

			internal static void ConsoleLoop()
			{
				ClearScreen(backgroundColor);

				cursorTimingCounter += DeltaTime;
				cursorTimingCounter %= 1;

				


				// input

				int inNum = Raylib_cs.Raylib.GetKeyPressed();

				//if (inNum >= 65 && inNum <= 90) Console.input += ((KeyboardKey)inNum).ToString(); // letters

				//else if (inNum >= 48 && inNum <= 57) Console.input += inNum - 48; // numbers

				//else if (inNum == (int)KeyboardKey.SPACE) Console.input += " ";


				if (IsKeyDown(KeyboardKey.LEFT_SHIFT) || IsKeyDown(KeyboardKey.RIGHT_SHIFT)) // for shifting charecters
				{
					shiftModifier = true;
				}
				else shiftModifier = false;

				if (IsKeyDown(KeyboardKey.LEFT_CONTROL) || IsKeyDown(KeyboardKey.RIGHT_CONTROL)) // for shifting charecters
				{
					controlModifier = true;
				}
				else controlModifier = false;


				// SCROLL WHEEL INPUT // ===============================---------------------------------

				{
					float wheel = GetMouseWheelDelta() * 2;

					if (controlModifier) wheel *= 4;

					if (shiftModifier) linesXScroll -= wheel;
					else linesYScroll -= wheel;

					linesYScroll = linesYScroll.Clamp(0, Lines.Count - 1);
					if (linesXScroll < 0) linesXScroll = 0;
				}


				// don't detect these inputs.
				if (inNum == (int)KeyboardKey.LEFT_SHIFT || inNum == (int)KeyboardKey.RIGHT_SHIFT ||
				inNum == (int)KeyboardKey.LEFT_CONTROL || inNum == (int)KeyboardKey.RIGHT_CONTROL ||
				inNum == (int)KeyboardKey.LEFT_ALT || inNum == (int)KeyboardKey.LEFT_ALT) { }

				else if (inNum == (int)KeyboardKey.ESCAPE) open = false;

				else if (openKey != null && inNum == (int)openKey) { } // this is to stop the openKey appearing as the first letter in the console.


				else if (inNum == (int)KeyboardKey.BACKSPACE)
				{
					if (input.Length > 0 && cursor > 0)
					{  // add support for backspacing whole words at a time.
						if (IsKeyDown(KeyboardKey.LEFT_CONTROL))
						{
							int startIndex = FindLastWordIndex();

							input = input.Remove(startIndex, cursor - startIndex);

							cursor = startIndex;
						}
						else
						{
							cursor--;
							input = input.Remove(cursor, 1);
						}

						browsingHistory = false;  // reset history
						historyPosition = 0;
					}

					cursorTimingCounter = 0;
				}

				// COPY-PASTING // ===============================---------------------------------

				else if (controlModifier)
				{
					if (inNum == (int)KeyboardKey.C) Clipboard.SetText(input);
					else if (inNum == (int)KeyboardKey.V) input = input.Insert(cursor, Clipboard.GetText());
				}


				// CURSOR MOVING // ===============================---------------------------------
				else if (inNum == (int)KeyboardKey.LEFT)
				{
					if (controlModifier) cursor = FindLastWordIndex();
					else cursor--;

					cursorTimingCounter = 0;
				}
				else if (inNum == (int)KeyboardKey.RIGHT)
				{
					if (controlModifier) cursor = FindNextWordIndex();
					else cursor++;

					cursorTimingCounter = 0;
				}


				// HISTORY // ===============================---------------------------------

				else if (inNum == (int)KeyboardKey.UP)
					{ if (browsingHistory || input == "") UpdateHistory(1); }
				else if (inNum == (int)KeyboardKey.DOWN)
					{ if (browsingHistory || input == "") UpdateHistory(-1); }



				else if (inNum == (int)KeyboardKey.ENTER) // COMMAND ENTERED // ===============================---------------------------------
				{
					Log("> " + input, Color.Gray);
					// Reset console scroll position
					linesXScroll = 0;
					linesYScroll = (Lines.Count - MAX_LINES_ON_SCREEN).Clamp(0, Lines.Count - 1);

					if (input != "")
					{
						History.Insert(0, input);
						//foreach (string token in TokenizeCommandString(input)) Lines.Add((token, Color.White));
						try
						{
							object? output = Execute(input);

							if (output != null) Log("> " + output);
						}
						catch (Exception e) { Log(/*e.GetType().Name + ": " + */e.Message, Color.Red); }
						input = "";
					}
				}

				else if (inNum != 0)
				{
					char chr = Convert.ToChar(inNum); // for some reason this works, i guess keycodes are just unicode charecters?
					if (shiftModifier && shiftedChars[chr] != 0) chr = shiftedChars[chr];
					input = input.Insert(cursor, chr.ToString());
					cursor++;

					cursorTimingCounter = 0;

					browsingHistory = false; // reset history
					historyPosition = 0;
				}

				int inputTextY = ScreenHeight - 15;


				// Some drawing

				DrawRectangle(0, ScreenHeight - 16, ScreenWidth, 16, Color.FromArgb(255, 40, 40, 40));
				//DrawRectangle(3, 3, ScreenWidth - 6, ScreenHeight - 30, Color.LightGray);

				// Draw console lines
				int scroll = (int)linesYScroll; // apparently i have to do this because the mouse-wheel movement returns a float and not an int.

				for (int i = scroll; i < linesYScroll + MAX_LINES_ON_SCREEN; i++)
				{
					if (i >= Lines.Count) break;

					(string, Color) line = Lines[i];

					DrawText(line.Item1, 1 - (int)(6 * linesXScroll), 9 * (i - scroll), line.Item2, small: true);
				}

				DrawText(input, 1, inputTextY, Color.White, small: true); // draw input

				cursor = cursor.Clamp(0, input.Length); // make sure the cursor does not go outside of the text
				if (cursorTimingCounter < 0.5f) for (int i = 0; i < 6; i++) DrawPixel(6 * cursor, inputTextY + i, Color.White); // draw cursor
			}
		}


		/// <summary>
		/// For storing some debug settings.
		/// </summary>
		public static class Debug
		{
			public static bool actorMonitoring = true;

			public static bool drawColliders = false;
			public static bool drawFps = false;
		}






		// LOGGING    ===============================================================================================================

		/// <summary>Writes an empty line out to the console</summary>
		public static void Log() => Log("");

		/// <summary>
		/// Write an object out to the console.
		/// </summary>
		public static void Log(object? obj, Color? color = null) // I feel like this could be done better, but i have no idea how to do it better
		{
			string? str;
			if (obj == null) str = null;
			else str = obj.ToString();
			Log(str ?? "null", color);
		}

		/// <summary>
		/// Write a string out to the console.
		/// </summary>
		public static void Log(string text, Color? color = null)
		{
			string[] lines = text.Split('\n');

			// update console y position if needed.
			if (Console.linesYScroll == Console.Lines.Count - Console.MAX_LINES_ON_SCREEN) Console.linesYScroll += lines.Length;

			foreach (string line in lines)
			{
				Console.Lines.Add((line, color ?? Color.White));
				Trace.WriteLine(line);
			}
		}



		// Commands    ===============================================================================================================
	}


	/// <summary>
	/// Put on a method in a subclass of NesGame to add a console command.<para/>Console command naming convensions are snake_case.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class ConsoleCommandAttribute : Attribute
	{
		public string? Summary { get; }

		public ConsoleCommandAttribute(string? summary = null)
		{
			Summary = summary;
		}
	}



	/// <summary>
	/// Apply on a string. For creating multiple choice arguments, you can either have a simple list of strings for the choices, or a pre-set dynamic command choice.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public class MultipleChoiceArgAttribute : Attribute
	{
		readonly string[]? choices;
		readonly DynamicCommandChoices? dynamicChoice;

		/// <summary>If the argument not matching any of the choices throws an error.</summary>
		public bool ThrowsError { get; }


		public string[] Choices
		{
			get
			{
				string[] strings = choices ?? dynamicChoice.Value.GetChoices();
				for (int i = 0; i < strings.Length; i++) strings[i] = strings[i].ToUpper();
				return strings;
			}
		}


		public MultipleChoiceArgAttribute(params string[] choices)
		{
			this.choices = choices;
			ThrowsError = true;
		}

		public MultipleChoiceArgAttribute(string[] choices, bool throwsError = true)
		{
			this.choices = choices;
			ThrowsError = throwsError;
		}

		public MultipleChoiceArgAttribute(DynamicCommandChoices dynamicChoice, bool throwsError = true)
		{
			this.dynamicChoice = dynamicChoice;
			ThrowsError = throwsError;
		}

		/// <summary>Don't use.</summary>
		[Obsolete]
		public MultipleChoiceArgAttribute() => throw new Exception("No arguments specified!"); // why does c# allow this constructer by default
	}


	// because c# attributes are dumb, i have to put this stuff into an enum.
	public enum DynamicCommandChoices
	{
		COMMANDS,
		COLORS
	}


	public static class DynamicCommandChoicesExtensionMethods
	{
		public static string[] GetChoices(this DynamicCommandChoices choice)
		{
			if (choice == DynamicCommandChoices.COMMANDS)
			{
				List<string> list = new();
				foreach (MethodInfo command in Nes.Console.Commands) if (!list.Contains(command.Name)) list.Add(command.Name);

				return list.ToArray();
			}
			else if (choice == DynamicCommandChoices.COLORS)
			{
				List<string> list = new();
				foreach (PropertyInfo property in typeof(Color).GetProperties()) list.Add(property.Name);

				return list.ToArray();
			}

			return Array.Empty<string>();
		}
	}
}
