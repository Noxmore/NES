using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NES
{

	/// <summary>
	/// Built-in class for dynamic objects in a game.
	/// 
	/// <para/>.
	/// 
	/// <para/>----===== ACTOR LOGIC =====----
	/// 
	/// <para>
	/// 
	///	Actors support both a component system, similer to Unity, and triditional sub-class based logic, it's recomended that you use a mix of the two.
	///	<para/>
	///	When extending Actor, manditory Components should be added as fields, using the "AddComponent" method.
	///	
	/// </para>
	/// </summary>
	public partial class Actor : ComponentHolder<Actor>
	{
		/// <summary>
		/// The position of the actor in pixels
		/// </summary>
		public Vector2 position = new();

		/// <summary>
		/// The name of the actor.
		/// </summary>
		public string? name = null;

		/// <summary>
		/// A little utility tool to manage string tags in actors.
		/// </summary>
		public List<string> Tags { get; private set; } = new();

		/// <summary>
		/// If the object has been removed, it is recomended to remove referances of this object if this is true;
		/// </summary>
		public bool Removed { get; private set; } = false;




		/// <summary>
		/// Code that gets run every frame (120 times per second), mainly used for rendering, and actor logic.
		/// </summary>
		protected virtual void OnLoop() { }

		/// <summary>
		/// Runs every frame, 
		/// </summary>
		public void Loop() //: should i run component loop logic first, or second?
		{
			if (!Removed)
			{
				ComponentLoop();
				OnLoop();
			}
		}


		/// <summary>
		/// Stops all loop logic from happening and enabled Removed, it's recomended to remove referances to this object after this.
		/// </summary>
		public void Remove() => Removed = true;
	}


	/// <summary>
	/// Used to store components, Actor extends this by default, and you can make your own types avalible for components by extending this type.
	/// <para>
	///	If used on a custom component holder, you must impliment the 
	/// </para>
	/// </summary>
	/// <typeparam name="T">The type to expect components to go to.</typeparam>
	public abstract partial class ComponentHolder<T> where T : ComponentHolder<T>
	{

		// =======================================-----------------------------------------------------
		//												NON-STATIC STUFF
		// =======================================-----------------------------------------------------


		/// <summary>
		/// A list of all the components on the object.
		/// </summary>
		public List<Component<T>> Components { get; private set; } = new();


		/// <summary>
		/// Adds a component to the component list, then returns it.
		/// </summary>
		/// <typeparam name="T">The type of component to add.</typeparam>
		/// <param name="component">The component to add.</param>
		/// <returns>The component added.</returns>
		public C AddComponent<C>(C component) where C : Component<T>
		{
			Components.Add(component);
			return component;
		}

		/// <summary>
		/// Gets the first component from the component list of the specified type, and/or if wanted a subclass of the specified type, and if wanted with the specifed name.
		/// </summary>
		/// <typeparam name="C">The type of component to find.</typeparam>
		/// <param name="acceptSubClasses">If true, components only have to be a subclass of the specifed type, otherwise they have to be the exact type.</param>
		/// <param name="name">If null, the name of the component is not checked, if not null, the name of the component must match this one.</param>
		/// <returns>The first component with the specified specifications, if none exist, null.</returns>
		public C? GetComponent<C>(bool acceptSubClasses = false, string? name = null) where C : Component<T>
		{
			foreach (Component<T> component in Components)
			{
				if ((acceptSubClasses ? component.GetType() == typeof(C) : component.GetType().IsSubclassOf(typeof(C))) && // make sure that it is the specified type or optionally a subclass of it.
					(name == null || name == component.Name)) // check the name is needed.
						return (C)component;
			}

			return null;
		}


		/// <summary>
		/// Call this every frame. Used to handle looping logic with components.
		/// </summary>
		protected void ComponentLoop()
		{
			// Loop through every component and call the loop logic.
			foreach (Component<T> component in Components) component.Loop();
		}
	}


	/// <summary>
	/// Used for adding modular logic to things like actors, although can be re-used for other things.
	/// </summary>
	/// <typeparam name="T">The Type the component if for, Actor recomended.</typeparam>
	public abstract class Component<T> where T : ComponentHolder<T>
	{
		/// <summary>
		/// If this is false, Loop will not be run, default is true.
		/// </summary>
		public bool Enabled { get; set; } = true;

		/// <summary>
		/// The object this is a part of.
		/// </summary>
		public T Parent { get; set; }

		/// <summary>Name for identifying certain component instances.</summary>
		public string? Name { get; set; }

		public Component(T parent)
		{
			Parent = parent;
			//Name = name;
		}

		/// <summary>
		/// Code that gets run 120 times per second.
		/// </summary>
		public abstract void Loop();
	}



	// =======================================-----------------------------------------------------
	//												COMPONENTS
	// =======================================-----------------------------------------------------


	public class SpriteRendererComponent : Component<Actor>
	{
		public SpriteRendererComponent(Actor parent, Sprite sprite, int currentAnimation = 0, int currentFrameNum = 0, SpriteTransform? transform = null, Color? tint = null) : base(parent)
		{
			this.sprite = sprite;
			this.currentAnimation = currentAnimation;
			this.currentFrameNum = currentFrameNum;
			currentFrame = sprite.animation != null ? sprite.animation.Value.animations[currentAnimation].frames[currentFrameNum] : sprite.image;
			this.tint = tint;
			this.transform = transform;
		}

		public Sprite sprite;
		public int currentAnimation;
		public int currentFrameNum;
		public Bitmap currentFrame;
		public SpriteTransform? transform;
		public Color? tint;
		/// <summary>How much time has past, resets when the frame is updated.</summary>
		public float frameTime = 0;

		/// <summary>Finds the animation index with the name specified, then sets currentAnimation to it.</summary>
		/// <param name="resetCurrentFrame">If the animation should reset to the beginning, or stay at the frame it's on before the change.</param>
		public void SetAnimation(string animationName, bool resetCurrentFrame = false)
		{
			if (sprite.animation == null) Nes.Log("WARNING: SpriteRenderer tried to set animation, when sprite does not support it. STACKTRACE: " + Environment.StackTrace);
			else
			{
				Bitmap[] frames = sprite.animation.Value.animations[currentAnimation].frames;

				currentAnimation = sprite.GetAnimationIndex(animationName);
				currentFrameNum = resetCurrentFrame ? 0 : currentFrameNum % frames.Length; // make sure the frames doesn't go out of bounds
			}
		}

		public override void Loop()
		{
			if (sprite.animation == null) Nes.DrawSprite(Parent.position, sprite, transform, tint);
			else
			{
				AnimationProperties properties = sprite.animation.Value;
				float? animationSpeed = properties.animations[currentAnimation].speed;

				// update animation
				frameTime += Nes.DeltaTime;

				if (frameTime > 1 / (animationSpeed == null ? properties.globalSpeed : animationSpeed))
				{
					frameTime = 0;
					currentFrameNum++;
					currentFrameNum %= properties.animations[currentAnimation].frames.Length;
					currentFrame = properties.animations[currentAnimation].frames[currentFrameNum];
				}

				// draw frame
				//Nes.DrawBitmap((int)Parent.position.X, (int)Parent.position.Y, sprite.GetGridCutout(currentAnimation, currentFrame), transform, tint);
				Nes.DrawBitmap((int)Parent.position.X, (int)Parent.position.Y, currentFrame, transform, tint);
			}
		}
	} // SpriteRendererComponent


	public class BoxColliderComponent : Component<Actor>
	{
		public BoxColliderComponent(Actor parent, Rectangle box) : base(parent)
		{
			this.box = box;
		}

		public BoxColliderComponent(Actor parent, int width, int height) : base(parent)
		{
			box = new(0, 0, width, height);
		}

		public Rectangle box;

		public Rectangle GetGlobalSpaceRectangle() => new(box.X + (int)Parent.position.X, box.Y + (int)Parent.position.Y, box.Width, box.Height);

		public bool IsColliding(BoxColliderComponent component) => GetGlobalSpaceRectangle().IntersectsWith(component.GetGlobalSpaceRectangle());

		public override void Loop()
		{
			if (Nes.Debug.drawColliders)
			{
				Rectangle gBox = GetGlobalSpaceRectangle();
				Nes.DrawRectangle(gBox.X, gBox.Y, gBox.Width, gBox.Height, Color.FromArgb(100, Color.Lime));
			}
		}
	}
}
