using NES.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NES
{
	public static partial class Nes
	{
		/// <summary>
		/// Bindings for the default font, the unicode charecter is the index into the array, and the value is the index into the font image file.,
		/// </summary>
		public static readonly byte[] fontBindings = new byte[200];

		/// <summary>
		/// Bindings for the default font while holding shift, the unicode charecter is the index into the array, and the value is the index into the font image file.,
		/// </summary>
		public static readonly byte[] fontShiftBindings = new byte[200];


		/// <summary>
		/// The bitmaps of the chars associated with fontBindings.
		/// </summary>
		public static readonly Bitmap[] fontBindingImages = new Bitmap[75];

		/// <summary>
		/// The bitmaps of the chars associated with fontBindings but small.
		/// </summary>
		public static readonly Bitmap[] fontBindingSmallImages = new Bitmap[75];


		internal static readonly char[] shiftedChars = new char[300];


		internal static Bitmap GetFontFileSprite(char chr, bool small = false, bool shift = false)
		{
			// repeated code, this is dumb.
			try { return small ? fontBindingSmallImages[shift ? fontShiftBindings[chr] : fontBindings[chr]] : fontBindingImages[shift ? fontShiftBindings[chr] : fontBindings[chr]]; }
			catch (IndexOutOfRangeException) { }

			return small ? fontBindingSmallImages[0] : fontBindingImages[0];
		}


		/// <summary>
		/// Sets font bindings and font binding images.
		/// </summary>
		private static void SetFontBindings()
		{
			fontBindings[32] = 43; // space

			// Main Letters
			for (int i = 97; i <= 122; i++)
				fontBindings[i] = (byte)(i - 96);
			
			// Numbers
			fontBindings[48] = 27;
			fontBindings[49] = 28;
			fontBindings[50] = 29;
			fontBindings[51] = 30;
			fontBindings[52] = 31;
			fontBindings[53] = 32;
			fontBindings[54] = 33;
			fontBindings[55] = 34;
			fontBindings[56] = 35;
			fontBindings[57] = 36;

			// Misc
			fontBindings[46] = 37; // .
			fontBindings[44] = 38; // ,
			fontBindings[33] = 39; // !
			fontBindings[45] = 40; // -
			fontBindings[47] = 41; // /
			fontBindings[58] = 42; // :
			fontBindings['_'] = 44;
			fontBindings['\''] = 45;
			fontBindings['\"'] = 46;
			fontBindings['+'] = 47;
			fontBindings['='] = 48;
			fontBindings['('] = 49;
			fontBindings[')'] = 50;
			fontBindings['{'] = 51;
			fontBindings['}'] = 52;
			fontBindings['['] = 53;
			fontBindings[']'] = 54;
			fontBindings['<'] = 55;
			fontBindings['>'] = 56;
			fontBindings[';'] = 57;
			fontBindings['?'] = 58;
			fontBindings['@'] = 59;
			fontBindings['#'] = 60;
			fontBindings['$'] = 61;
			fontBindings['%'] = 62;
			fontBindings['^'] = 63;
			fontBindings['&'] = 64;
			fontBindings['*'] = 65;
			fontBindings['|'] = 66;
			fontBindings['\\'] = 67;


			{
				// font binding images
				Bitmap font = Resources.font; // probably not nessesary

				for (int i = 0; i < fontBindingImages.Length; i++) try
				{
					fontBindingImages[i] = font.Clone(new(8 * i, 0, 8, 8), font.PixelFormat);
				}
				catch (OutOfMemoryException) { break; }
			}

			{
				// small font binding images
				Bitmap font = Resources.font_small; // probably not nessesary

				for (int i = 0; i < fontBindingSmallImages.Length; i++) try
					{
						fontBindingSmallImages[i] = font.Clone(new(6 * i, 0, 6, 6), font.PixelFormat);
					}
					catch (OutOfMemoryException) { break; }
			}



			// SHIFTED CHARS

			shiftedChars['1'] = '!';
			shiftedChars['2'] = '@';
			shiftedChars['3'] = '#';
			shiftedChars['4'] = '$';
			shiftedChars['5'] = '%';
			shiftedChars['6'] = '^';
			shiftedChars['7'] = '&';
			shiftedChars['8'] = '*';
			shiftedChars['9'] = '(';
			shiftedChars['0'] = ')';

			shiftedChars['-'] = '_';
			shiftedChars['='] = '+';

			shiftedChars['['] = '{';
			shiftedChars[']'] = '}';

			shiftedChars[';'] = ':';
			shiftedChars['\''] = '\"';

			shiftedChars[','] = '<';
			shiftedChars['.'] = '>';
			shiftedChars['/'] = '?';

			shiftedChars['\\'] = '|';
		}
	}
}
