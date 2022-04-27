using NES;
using System.Drawing;
using System.Numerics;
using System.Reflection;

namespace SNB
{
	class SnbGame : NesGame // Game used for testing
	{
		public static SnbGame game = null;


		public override string Name => "Umbra Testing";


		public Player player = new();

		public readonly List<Actor> actors = new();

		public int score = 0;



		// DEBUG

		public static bool showActors = true;


		// stars background

		public Vector2[] Stars { get; } = new Vector2[20];
		public float scrollSpeed = 0.4f;



		public override void Start()
		{
			game = this;

			player.position = new(Nes.ScreenWidth / 2, Nes.ScreenHeight - 70);

			// populate stars
			Random random = new();
			for (int i = 0; i < Stars.Length; i++) Stars[i] = new(random.Next(Nes.ScreenWidth), random.Next(Nes.ScreenHeight));

			

			Nes.Log(Name + " Started!");
		}

		public override void Stop()
		{
			game = null;

			Nes.Log(Name + " Stopped!");
		}


		public override void Loop()
		{
			Nes.ClearScreen(Color.Black);


			if (Nes.IsButtonPressed(0, GamepadButton.A))
			{
				//Sound.Play("./assets/sounds/coin.wav");
				Coin coin = new();
				actors.Add(coin);
				coin.position = new(Nes.RandomInt(0, Nes.ScreenWidth), 5);
			}

			/*if (Nes.IsButtonDown(0, GamepadButton.LB))
			{
				if (player.shootCooldown == Player.SHOOT_COOLDOWN)
				{
					Bullet bullet = new();
					bullet.position = player.position;

					entities.Add(bullet);

					player.shootCooldown = 0;
				}
			}*/



			player.position = Vector2.Clamp(player.position, new(0, 0), new(Nes.ScreenWidth, Nes.ScreenHeight - 24));


			

			//if (player.shootCooldown != Player.SHOOT_COOLDOWN) player.shootCooldown++;

			for (int i = 0; i < Stars.Length; i++)
			{
				Vector2 star = Stars[i];

				Stars[i].Y += scrollSpeed;
				Nes.DrawPixel(star, Color.Gray);
				if (star.Y > Nes.ScreenHeight)
				{
					Stars[i].Y = 0;
					Stars[i].X = Nes.RandomInt(0, Nes.ScreenWidth - 1);
				}
			}

			if (Nes.RandomInt(0, 120) == 0) // add coins
			{
				Coin coin = new();
				actors.Add(coin);
				coin.position = new(Nes.RandomInt(-8, Nes.ScreenWidth), 5);
			}

			for (int i = actors.Count - 1; i >= 0; i--)
			{
				Actor actor = actors[i];

				actor.Loop();

				if (actor.Removed) actors.RemoveAt(i);
			}

			player.Loop();

			// UI

			// debug

			if (showActors) Nes.DrawText("ACTORS " + actors.Count.ToString("000"), 5, 5, Color.White);


			// Bottom panel

			Nes.DrawRectangle(0, 200, Nes.ScreenWidth, 24, Color.Gray);
			Nes.DrawRectangleOutline(1, 201, Nes.ScreenWidth - 2, 22, Color.DarkGray);
			Nes.DrawText("SCORE " + score.ToString("0000000"), 5, Nes.ScreenHeight - 20, Color.Yellow, Color.DarkGoldenrod);
			Nes.DrawText("HEALTH " + player.health.ToString("000"), 140, Nes.ScreenHeight - 20, Color.Yellow, Color.DarkGoldenrod);
		}
	}
}