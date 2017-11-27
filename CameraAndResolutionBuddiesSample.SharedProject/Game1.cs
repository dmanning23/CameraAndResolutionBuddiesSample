using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using CameraBuddy;
using GameTimer;
using CollisionBuddy;
using HadoukInput;
using PrimitiveBuddy;
using ResolutionBuddy;
using MatrixExtensions;

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

		/// <summary>
		/// flag to tell if the circles are colliding
		/// </summary>
		bool _colliding = false;

		GameClock _clock;

		InputState _inputState;
		ControllerWrapper _controller;
		InputWrapper _inputWrapper;

		/// <summary>
		/// The camera we are going to use!
		/// </summary>
		Camera _camera;

		Texture2D _texture;
		Primitive primitive;

		Rectangle desired = new Rectangle(0, 0, 1280, 720);

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
			_controller = new ControllerWrapper(PlayerIndex.One, true);
			_inputWrapper = new InputWrapper(_controller, _clock.GetCurrentTime);
			_inputWrapper.Controller.UseKeyboard = true;

#if DESKTOP
			var resolution = new ResolutionComponent(this, graphics, new Point(1280, 720), new Point(1280, 720), false, true);
#else
			var resolution = new ResolutionComponent(this, graphics, new Point(1280, 720), new Point(1280, 720), false, true);
#endif

			//set up the camera
			_camera = new Camera();
			_camera.WorldBoundary = desired;
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			base.Initialize();

			//init the blue circle so it will be on the left of the screen
			_circle1.Initialize(new Vector2(graphics.GraphicsDevice.Viewport.TitleSafeArea.Center.X - 300,
			                                graphics.GraphicsDevice.Viewport.TitleSafeArea.Center.Y), 60.0f);

			//put the red circle on the right of the screen
			_circle2.Initialize(graphics.GraphicsDevice.Viewport.TitleSafeArea.Center, 60.0f);

			//Initiailze the camera to start with everything on screen
			AddCircleToCamera(_circle1);
			AddCircleToCamera(_circle2);
			_camera.BeginScene(true);

			_clock.Start();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);
			primitive = new Primitive(graphics.GraphicsDevice, spriteBatch);
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
#if !__IOS__
				this.Exit();
#endif
			}

			//update the timer
			_clock.Update(gameTime);

			//update the input
			_inputState.Update();
			_inputWrapper.Update(_inputState, false);

			//move the circle
			float movespeed = 600.0f;

			//check veritcal movement
			if (_inputWrapper.Controller.CheckKeystrokeHeld(EKeystroke.Up))
			{
				_circle1.Translate(0.0f, -movespeed * _clock.TimeDelta);
			}
			else if (_inputWrapper.Controller.CheckKeystrokeHeld(EKeystroke.Down))
			{
				_circle1.Translate(0.0f, movespeed * _clock.TimeDelta);
			}

			//check horizontal movement
			if (_inputWrapper.Controller.CheckKeystrokeHeld(EKeystroke.Forward))
			{
				_circle1.Translate(movespeed * _clock.TimeDelta, 0.0f);
			}
			else if (_inputWrapper.Controller.CheckKeystrokeHeld(EKeystroke.Back))
			{
				_circle1.Translate(-movespeed * _clock.TimeDelta, 0.0f);
			}

			//add camera shake when the two circles crash into each other
			if (CollisionCheck.CircleCircleCollision(_circle1, _circle2))
			{
				if (!_colliding)
				{
					_camera.AddCameraShake(0.25f);
				}
				_colliding = true;
			}
			else
			{
				_colliding = false;
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

#if WINDOWS
			// Calculate Proper Viewport according to Aspect Ratio
			Resolution.ResetViewport();
#endif

			//Add all our points to the camera
			AddCircleToCamera(_circle1);
			AddCircleToCamera(_circle2);

			//update all the matrices of the camera before we start drawing
			_camera.BeginScene(false);

			spriteBatch.Begin(
				SpriteSortMode.Deferred,
				BlendState.NonPremultiplied,
				null, null, null, null,
				_camera.TranslationMatrix * Resolution.TransformationMatrix());

			spriteBatch.Draw(_texture, Vector2.Zero, Color.White);

			//draw the players circle in green
			primitive.Circle(_circle1.Pos, _circle1.Radius, Color.Green);

			//draw the stationary circle in red
			primitive.Circle(_circle2.Pos, _circle2.Radius, Color.Red);

			spriteBatch.End();

			//Draw our gui!
			spriteBatch.Begin(
				SpriteSortMode.Deferred,
				BlendState.NonPremultiplied,
				null, null, null, null,
				Resolution.TransformationMatrix());

			primitive.Rectangle(Resolution.TitleSafeArea, Color.Red);

			//Draw the center of the circles as white dots
			DrawCircleCenters();

			spriteBatch.End();

			base.Draw(gameTime);
		}

		public void DrawCircleCenters()
		{
			var centerPosition1 = MatrixExt.Multiply(_camera.TranslationMatrix, _circle1.Pos);
			primitive.Rectangle(Resolution.TitleSafeArea, Color.Red);
			primitive.Circle(centerPosition1, 64, Color.White);
		}

		private void AddCircleToCamera(Circle myCircle)
		{
			float pad = (myCircle.Radius * 1.5f); //add a bit of padding so they aren't touching the edge of the screen

			//Add the upperleft and lowercorners.  That will fit the whole circle in camera
			_camera.AddPoint(myCircle.Pos);
			_camera.AddPoint(new Vector2((myCircle.Pos.X - pad), (myCircle.Pos.Y - pad)));
			_camera.AddPoint(new Vector2((myCircle.Pos.X + pad), (myCircle.Pos.Y + pad)));
		}

		#endregion //Methods
	}
}
