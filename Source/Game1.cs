using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using CameraBuddy;
using GameTimer;
using CollisionBuddy;
using HadoukInput;
using BasicPrimitiveBuddy;
using ResolutionBuddy;

namespace CameraAndResolutionBuddiesSample
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Game
	{
		#region Members

		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		Circle _circle1;
		Circle _circle2;

		GameClock _clock;

		InputState _inputState;
		InputWrapper _inputWrapper;

		/// <summary>
		/// The camera we are going to use!
		/// </summary>
		Camera _camera;

		Texture2D _texture;
		BasicPrimitive titlesafe;

		#endregion //Members

		#region Methods

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			graphics.IsFullScreen = false;

			_circle1 = new Circle();
			_circle2 = new Circle();

			_clock = new GameClock();
			_inputState = new InputState();
			_inputWrapper = new InputWrapper(PlayerIndex.One, _clock.GetCurrentTime);
			_inputWrapper.Controller.UseKeyboard = true;

			//set up the camera
			_camera = new Camera();
			_camera.WorldBoundary = new Rectangle(0, 0, 1280, 720);

			Resolution.Init(ref graphics);
			Resolution.SetDesiredResolution(1280, 720);

			//Resolution.SetScreenResolution(480, 800, false);
			Resolution.SetScreenResolution(1920, 1080, true);
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			//init the blue circle so it will be on the left of the screen
			_circle1.Initialize(new Vector2(graphics.GraphicsDevice.Viewport.TitleSafeArea.Center.X - 300,
			                                graphics.GraphicsDevice.Viewport.TitleSafeArea.Center.Y), 80.0f);

			//put the red circle on the right of the screen
			_circle2.Initialize(graphics.GraphicsDevice.Viewport.TitleSafeArea.Center, 40.0f);

			_clock.Start();

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			titlesafe = new BasicPrimitive(graphics.GraphicsDevice);

			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			//Setup all the rectangles used by the camera
			_camera.SetScreenRects(graphics.GraphicsDevice.Viewport.Bounds, graphics.GraphicsDevice.Viewport.TitleSafeArea);

			_texture = Content.Load<Texture2D>("Braid_screenshot8");
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// Allows the game to exit
			if ((GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) || 
			    Keyboard.GetState().IsKeyDown(Keys.Escape))
			{
				this.Exit();
			}

			//update the timer
			_clock.Update(gameTime);

			//update the input
			_inputState.Update();
			_inputWrapper.Update(_inputState, false);

			//move the circle
			float movespeed = 1600.0f;

			//check veritcal movement
			if (_inputWrapper.Controller.KeystrokeHeld[(int)EKeystroke.Up])
			{
				_circle1.Translate(0.0f, -movespeed * _clock.TimeDelta);
			}
			else if (_inputWrapper.Controller.KeystrokeHeld[(int)EKeystroke.Down])
			{
				_circle1.Translate(0.0f, movespeed * _clock.TimeDelta);
			}

			//check horizontal movement
			if (_inputWrapper.Controller.KeystrokeHeld[(int)EKeystroke.Forward])
			{
				_circle1.Translate(movespeed * _clock.TimeDelta, 0.0f);
			}
			else if (_inputWrapper.Controller.KeystrokeHeld[(int)EKeystroke.Back])
			{
				_circle1.Translate(-movespeed * _clock.TimeDelta, 0.0f);
			}

			//add camera shake?
			if (_inputWrapper.Controller.KeystrokePress[(int)EKeystroke.A])
			{
				_camera.AddCameraShake(0.5f);
			}

			//update the camera
			_camera.Update(_clock);

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			//Add all our points to the camera
			_camera.AddPoint(_circle1.Pos);
			_camera.AddPoint(_circle2.Pos);

			//update all the matrices of the camera before we start drawing
			_camera.BeginScene(false);

			spriteBatch.Begin(
				SpriteSortMode.Deferred,
				BlendState.NonPremultiplied,
				null, null, null, null,
				_camera.TranslationMatrix);

			spriteBatch.Draw(_texture, Vector2.Zero, Color.White);

			//draw the players circle in green
			BasicPrimitive circlePrim = new BasicPrimitive(graphics.GraphicsDevice);
			circlePrim.Circle(_circle1.Pos, _circle1.Radius, Color.Green, spriteBatch);

			//draw the stationary circle in red
			circlePrim = new BasicPrimitive(graphics.GraphicsDevice);
			circlePrim.Circle(_circle2.Pos, _circle2.Radius, Color.Red, spriteBatch);

			titlesafe.Rectangle(Resolution.TitleSafeArea, Color.Red, spriteBatch);

			spriteBatch.End();

			base.Draw(gameTime);
		}

		#endregion //Members
	}
}
