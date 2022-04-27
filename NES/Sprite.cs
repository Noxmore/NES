using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NES
{
	/// <summary>
	/// Represents an image used to draw stuff to the screen, with optional sprite animations.
	/// </summary>
	public class Sprite // Used to wrap a Bitmap, and an AnimationProperties
	{
		/// <summary>
		/// Optional animation properties, if this is null, there is no animation on the sprite.
		/// </summary>
		public AnimationProperties? animation;

		// <summary>If this sprite supports animation.</summary>
		//public bool Animated => animation != null;

		Sprite(Bitmap image, AnimationProperties? animation)
		{
			this.image = image;
			this.animation = animation;

			//texture = Raylib.LoadTextureFromImage(image);
		}

		//Image image;
		//Texture2D texture;
		/// <summary>The Bitmap image used to store sprite image data.</summary>
		public Bitmap image;


		/// <summary>
		/// The width of the sprite in pixels
		/// </summary>
		public int Width => image.Width;

		/// <summary>
		/// The height of the sprite in pixels
		/// </summary>
		public int Height => image.Height;


		/// <summary>
		/// Used mainly for animation. This cannot be used if animations is null because of technical reasons. Throws an OutOfMemoryException if index is out of bounds.
		/// <para/>
		/// The x and y parameters are the index of the cutout, not the coords.
		/// </summary>
		/// <returns>The cutout/cropped grid section specifed, see wiki for more details.</returns>
		/// /// <exception cref="InvalidOperationException"></exception>
		/// /// <exception cref="OutOfMemoryException"></exception>
		public Bitmap GetGridCutout(int x, int y)
		{
			if (animation == null) throw new InvalidOperationException("\"animations\" is null, when it cannot be!"); // make sure animations is not null

			int spriteSize = image.Width / animation.Value.animations.Length;

			// Good thing Bitmap already has a cropping function!
			return image.Clone(new(x * spriteSize, y * spriteSize, spriteSize, spriteSize), image.PixelFormat);
		}

		/// <summary>Used mainly for atlases. Throws an OutOfMemoryException if the rectangle is out of bounds.</summary>
		/// <returns>The cutout/cropped section specifed.</returns>
		/// <exception cref="OutOfMemoryException"></exception>
		public Bitmap GetCutout(Rectangle rectangle) => image.Clone(rectangle, image.PixelFormat);


		/// <returns>The index of the animation by the specifed name.</returns>
		/// <exception cref="InvalidOperationException"></exception>
		public int GetAnimationIndex(string name)
		{
			if (animation == null) throw new InvalidOperationException("\"animations\" is null, when it cannot be!"); // make sure animations is not null
			Animation[] animations = animation.Value.animations;

			for (int i = 0; i < animations.Length; i++) if (animations[i].name == name) return i;

			throw new InvalidOperationException(name + " is not a valid animation!");
		}



		/// <returns>The color of the pixel at the specifed location of the sprite.</returns>
		public Color GetPixel(int x, int y) => image.GetPixel(x, y);

		/// <summary>Sets the color of the pixel at the specifed location of the sprite.</summary>
		public unsafe void SetPixel(int x, int y, Color color)
		{
			image.SetPixel(x, y, color);
			//Raylib.UpdateTexture(texture, image.data);
		}



		/// <summary>
		/// Tries to retrieve the sprite specifed from the built-in sprite cache, if there is none, load it from the path specified.
		/// <para>path can be a full path to the file, or start with a dot "." to specify that the root path used, is the directory that the exe being run is in.</para>
		/// </summary>
		/// <returns>The sprite from the path specified</returns>
		public static Sprite Get(string path)
		{
			path = Path.GetFullPath(path);
			try { return Nes.spriteCache[path]; }
			catch (KeyNotFoundException)
			{
				// Load sprite
				Sprite sprite = new((Bitmap)Image.FromFile(path), AnimationProperties.Load(path));
				Nes.spriteCache.Add(path, sprite);
				return sprite;
			}
		}
	} // sprite


	/// <summary>Extra information about how a sprite is drawn</summary>
	public struct SpriteTransform
	{
		public bool horizontalFlip;
		public bool verticalFlip;

		public SpriteTransform(bool horizontalFlip = false, bool verticalFlip = false)
		{
			this.horizontalFlip = horizontalFlip;
			this.verticalFlip = verticalFlip;
		}
	}





	/// <summary>
	/// Used for storing an animation and it's details.
	/// </summary>
	public struct Animation
	{
		/// <summary>The name of the animation</summary>
		public string name;

		/// <summary>The column that the animation is in.</summary>
		public int index;

		/// <summary>The frames of the animation.</summary>
		public Bitmap[] frames;
		/// <summary>How fast the animation plays in frames per second. null if set by global AnimationProperties variable.</summary>
		public float? speed;
		/// <summary>If the animation should repeat, or stop on the last frame. null if set by global AnimationProperties variable.</summary>
		public bool? repeating;


		public Animation(string name, int index, Bitmap[] frames, float? speed = null, bool? repeating = null)
		{
			this.name = name;
			this.index = index;
			this.frames = frames;
			this.speed = speed;
			this.repeating = repeating;
		}
	}

	/// <summary>
	/// Used for storing a sprite's animation properties.
	/// </summary>
	public struct AnimationProperties
	{
		/// <summary>All the animations in the sprite.</summary>
		public Animation[] animations;

		/// <summary>The speed used if an animation does not specify a speed.</summary>
		public float globalSpeed;
		/// <summary>If an animation repeats. Used if an animation does not specify if it repeats.</summary>
		public bool globalRepeating;


		public AnimationProperties(Animation[] animations, bool globalRepeating = true, float globalSpeed = 5)
		{
			this.animations = animations;
			this.globalRepeating = globalRepeating;
			this.globalSpeed = globalSpeed;
		}


		/// <returns>null if the json file does not exist, else the AnimationProperties the json file supplies.</returns>
		public static AnimationProperties? Load(string path)
		{
			// Make sure path is pointing to the json properties file of the sprite.
			if (!path.EndsWith(".json")) path += ".json";

			// Make sure the file exists.
			if (!File.Exists(path)) return null;


			JObject json = JObject.Parse(File.ReadAllText(path));

			// this probably could be done better, but i don't have a good idea atm

			//: handle all these possible null values.
			float globalAnimationSpeed = (float)json["globalAnimationSpeed"];
			bool globalRepeating = (bool)json["globalRepeating"];

			JToken[] animationsJson = json["animations"].ToArray();

			Animation[] animations = new Animation[animationsJson.Length];

			// Need to access the image data for the size and to pre-compute the frames.
			Bitmap image = (Bitmap)Image.FromFile(path.Substring(0, path.Length - 5));
			int spriteSize = image.Width / animations.Length;

			for (int i = 0; i < animationsJson.Length; i++)
			{
				JToken animjson = animationsJson[i];

				string name = (string)animjson["name"];
				int? frameCount = (int?)animjson["frames"];
				float? speed = (float?)animjson["speed"];
				bool? repeating = (bool?)animjson["repeating"];

				// if frames are not defined, calculate it.
				if (frameCount == null) frameCount = (int)MathF.Floor(image.Height / (float)spriteSize); // flooring this in case the canvas is a bit bigger then we expect.

				// compute frames
				Bitmap[] frames = new Bitmap[frameCount.Value];

				for (int o = 0; o < frameCount; o++)
					frames[o] = image.Clone(new(i * spriteSize, o * spriteSize, spriteSize, spriteSize), image.PixelFormat);

				animations[i] = new(name, i, frames, speed, repeating);
			}

			return new(animations, globalRepeating, globalAnimationSpeed);
		}

		// private function for parsing json
		/*static T? ConfirmToken<T>(JObject json, object key, JTokenType expectedType, string fileName)
		{
			JToken? token = json[key];

			if (token == null) if (typeof(T).IsSubclassOf(typeof(Nullable))) return default; else throw new Exception("Sprite property in " + fileName + " \"" + key + "\" is null or not defined, when it has to be.");
			// make sure it has the right type
			if (token.Type != expectedType) throw new Exception("Sprite property in " + fileName + " \"" + key + "\" was expected to be a " + expectedType + " but was a " + token.Type + ".");

			return (T)(object)token;
		}*/ //: unnessisary?
	}
}
