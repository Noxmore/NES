using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using NES;
using System.Drawing;

namespace SNB
{
	public class Player : Actor
	{
		public Player()
		{
			spriteRenderer = AddComponent(new SpriteRendererComponent(this, Sprite.Get("./assets/sprites/player.png")));
			collider = AddComponent(new BoxColliderComponent(this, 16, 16));
		}

		public float speed = 1.5f;
		public int health = 100;

		public readonly SpriteRendererComponent spriteRenderer;
		public readonly BoxColliderComponent collider;

		protected override void OnLoop()
		{
			// UPDATE
			Vector2 movement = new();

			if (Nes.IsButtonDown(0, GamepadButton.RIGHT))
			{
				movement.X += 1;
				spriteRenderer.SetAnimation("right");
			}
			if (Nes.IsButtonDown(0, GamepadButton.LEFT))
			{
				movement.X -= 1;
				spriteRenderer.SetAnimation("left");
			}
			if (Nes.IsButtonDown(0, GamepadButton.UP)) movement.Y -= 1;
			if (Nes.IsButtonDown(0, GamepadButton.DOWN)) movement.Y += 1;

			// reset animation is not holding anything
			if (Nes.IsButtonUp(0, GamepadButton.LEFT) && Nes.IsButtonUp(0, GamepadButton.RIGHT)) spriteRenderer.SetAnimation("idle");


			if (movement != Vector2.Zero) position += Vector2.Normalize(movement);


			//if (Nes.IsKeyDown(KeyboardKey.D)) position.X += speed;
			//if (Nes.IsKeyDown(KeyboardKey.A)) position.X -= speed;
			//if (Nes.IsKeyDown(KeyboardKey.W)) position.Y -= speed;
			//if (Nes.IsKeyDown(KeyboardKey.S)) position.Y += speed;



			position = Vector2.Clamp(position, Vector2.Zero, new(Nes.ScreenWidth - 16, Nes.ScreenHeight - 40));


			// RENDER

		}
	}


	public class Coin : Actor
	{
		public Coin()
		{
			AddComponent(new CollectableComponent(this, Sound.Get("./assets/sounds/coin.wav"), AddComponent(new BoxColliderComponent(this, 8, 8))));
			AddComponent(new SpriteRendererComponent(this, Sprite.Get("./assets/sprites/coin.png")));
		}
	}



	public abstract class Enemy : Actor
	{
		public Enemy()
		{
			collider = AddComponent(new BoxColliderComponent(this, new(0, 0, 16, 16)));
			rb = AddComponent(new RigidBodyComponent(this));

			health = DefaultHealth;
		}

		public readonly BoxColliderComponent collider;
		public readonly RigidBodyComponent rb;

		public abstract int Atk { get; }
		public abstract float Speed { get; }
		public abstract int DefaultHealth { get; }
		//public abstract int Def { get; }

		public int health;


		protected void HitCollidingPlayers()
		{
			Player player = SnbGame.game.player;

			if (collider.IsColliding(player.collider))
				player.health -= Atk;
		}
	}


	public class ChargingEnemy : Enemy
	{
		public ChargingEnemy() : base()
		{
			spriteRenderer = AddComponent(new SpriteRendererComponent(this, Sprite.Get("./assets/sprites/enemy_charging.png")));
		}

		public readonly SpriteRendererComponent spriteRenderer;



		//public override int Def => 10

		public override int Atk => Nes.RandomInt(6, 15);
		public override int DefaultHealth => 3;
		public override float Speed => 1;


		public State state = State.IDLE;
		

		public TimeSince startedCharging = new(started: false);


		protected override void OnLoop()
		{
			if (state == State.IDLE)
			{
				if (Nes.RandomInt(0, 300) == 0)
				{
					state = State.CHARGING;
					startedCharging.Start();
				}


			}

			else if (state == State.CHARGING)
			{
				if (startedCharging > 2)
				{
					state = State.IDLE;
					startedCharging.Stop();
					startedCharging.Value = 0;
				}

				Rectangle thisBox = collider.GetGlobalSpaceRectangle();
				Rectangle PlayerBox = SnbGame.game.player.collider.GetGlobalSpaceRectangle();

				Vector2 center = (thisBox.Location + thisBox.Size / 2).ToVector2();
				Vector2 playerCenter = (PlayerBox.Location + PlayerBox.Size / 2).ToVector2();

				Vector2 dir = playerCenter - center;

				rb.velocity += Vector2.Normalize(dir) / 3;
			}

			HitCollidingPlayers();

			position = Vector2.Clamp(position, Vector2.Zero, new(Nes.ScreenWidth - 16, Nes.ScreenHeight - 40));
		}


		public enum State
		{
			IDLE,
			CHARGING
		}
	}



	// ================================================----------------------------------------
	//															COMPONENTS
	// ================================================----------------------------------------

	public class HittableComponent : Component<Actor>
	{
		public HittableComponent(Actor parent) : base(parent)
		{
		}

		public int hp;
		public bool explodesWhenDead;

		public override void Loop()
		{
			
		}
	}

	// =============================================--------------------------------------------

	public class RigidBodyComponent : Component<Actor>
	{
		public RigidBodyComponent(Actor parent, float drag = 1, float gravity = 0) : base(parent)
		{
			this.drag = drag;
			this.gravity = gravity;
		}

		public Vector2 velocity = new();
		public float drag;
		public float gravity;

		public override void Loop()
		{
			velocity.Y += gravity;
			velocity /= drag;
			Parent.position += velocity;
		}
	}

	// =============================================--------------------------------------------

	public class CollectableComponent : Component<Actor>
	{
		public CollectableComponent(Actor parent, Sound sound, BoxColliderComponent collider, float playerMagnetStrength = 6, int scoreAddition = 50, bool moves = true, Action? onCollect = null) : base(parent)
		{
			this.sound = sound;
			this.collider = collider;
			this.scoreAddition = scoreAddition;
			this.onCollect = onCollect;
			this.moves = moves;
			this.playerMagnetStrength = playerMagnetStrength;
		}

		public Sound sound;
		public BoxColliderComponent collider;
		public int scoreAddition;
		public Action? onCollect;
		public bool moves;
		public float playerMagnetStrength;

		public override void Loop()
		{
			SnbGame game = SnbGame.game;

			if (moves)
			{
				Parent.position.Y += game.scrollSpeed * 1.5f;

				if (Parent.position.Y > Nes.ScreenHeight) Parent.Remove();
			}

			if (playerMagnetStrength != 0)
			{
				Rectangle thisBox = collider.GetGlobalSpaceRectangle();	
				Rectangle PlayerBox = game.player.collider.GetGlobalSpaceRectangle();	

				Vector2 center = (thisBox.Location + thisBox.Size / 2).ToVector2();
				Vector2 playerCenter = (PlayerBox.Location + PlayerBox.Size / 2).ToVector2();

				Vector2 dir = playerCenter - center;

				Parent.position += Vector2.Normalize(dir) / Vector2.Distance(center, playerCenter) * playerMagnetStrength;
			}

			if (collider.IsColliding(game.player.collider))
			{
				Parent.Remove();
				game.score += scoreAddition;
				sound.Play();
				if (onCollect != null) onCollect.Invoke();
			}
		}
	}





	/*public abstract class Entity : IDisposable
	{
		public Entity()
		{
			Spawn();
		}

		/// <summary>
		/// The position of the entity in pixels.
		/// </summary>
		public Vector2 position = new();

		/// <summary>
		/// The name of the entity.
		/// </summary>
		public string? name = null;

		/// <summary>
		/// Code that gets run 120 times per second, mainly used for rendering, and entity logic.
		/// </summary>
		public abstract void Loop();

		/// <summary>
		/// Code that gets run whenever the entity spawns
		/// </summary>
		protected abstract void Spawn();

		/// <summary>
		/// Code that gets run whenever the entitiy gets removed.
		/// </summary>
		protected abstract void Removed();

		/// <summary>
		/// Remove this entity from the game at the end of the frame.
		/// </summary>
		public void Remove()
		{
			SnbGame.game.entitiesToRemove.Add(this);
			Removed();
		}

		/// <summary>
		/// Removes the entity instantly, do not use unless needed.
		/// </summary>
		public void Dispose()
		{
			GC.Collect();
			GC.SuppressFinalize(this);
		}
	}

	internal class Bullet : Entity
	{
		public Color color = Color.Red;

		public float speed = 1;
		public Vector2 size = new (1, 2);
		public float damage = 20;


		public override void Loop()
		{
			position.Y -= speed;

			if (position.Y < 0 - size.Y || position.Y > Nes.ScreenHeight + size.Y) Remove();

			//Nes.DrawPixel((int)position.X, (int)position.Y, color);
			//Nes.DrawPixel((int)position.X, (int)position.Y + 1, color);

			Nes.DrawRectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y, color);
		}

		protected override void Removed()
		{
			
		}

		protected override void Spawn()
		{
			
		}
	}


	internal abstract class LivingEntity : Entity
	{
		float hp = 10;
	}

	internal class Player : LivingEntity
	{
		public const int SHOOT_COOLDOWN = 20;

		public int shootCooldown = 0;

		public override void Loop()
		{
			//Nes.DrawRectangle((int)position.X - 4, (int)position.Y - 4, 8, 8, Color.White);
			//Nes.DrawRectangle((int)position.X - 2, (int)position.Y - 2, 4, 4, Color.Black);
			Nes.DrawSprite(position, Sprite.Get("./assets/sprites/player.png"));
		}

		protected override void Removed()
		{
			
		}

		protected override void Spawn()
		{
			
		}
	}

	internal abstract class Enemy : LivingEntity
	{
	
	}*/
}
