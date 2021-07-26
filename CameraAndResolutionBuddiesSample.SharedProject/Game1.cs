using CameraBuddy;
using CollisionBuddy;
using GameTimer;
using HadoukInput;
using MatrixExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PrimitiveBuddy;
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

		/// <summary>
		/// speed to move the circle
		/// </summary>
		const float circleMovementSpeed = 600.0f;

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

			//Setup the input for this game.
			_inputState = new InputState();
			Mappings.UseKeyboard[0] = true; //Set the first player to use the keyboard. 
			_controller = new ControllerWrapper(0);
			_inputWrapper = new InputWrapper(_controller, _clock.GetCurrentTime);


			var resolution = new ResolutionComponent(this, 
				graphics,
				new Point(1280, 720), //The desired virtual resolution that items in the game will be drawn with
				new Point(1280, 720), //The desired physical resolution to set the screen.
				false, //Flag whether or not to fullscreen the app
				true, //Flag whether or not to letterbox the app to fit the aspect ratio of virtual/screen resolutions.
				false); //This flag can be used to set it to use the entire screen BUT without setting the Device.Fullscreen flag.

			//set up the camera
			_camera = new Camera();

			//The WorldBoundary is a rectangle that the Camera will try to stay inside. When the circle moves out of this rectangle it will appear to go offscreen.
			_camera.WorldBoundary = new Rectangle(0, 0, 1280, 720); 
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
			_circle1.Initialize(new Vector2(Resolution.TitleSafeArea.Center.X - 300,
											Resolution.TitleSafeArea.Center.Y), 60.0f);

			//put the red circle on the right of the screen
			_circle2.Initialize(Resolution.TitleSafeArea.Center, 60.0f);

			//Initialize the camera to start with everything on screen
			AddCircleToCamera(_circle1);
			AddCircleToCamera(_circle2);
			_camera.BeginScene(true); //Pass true to camera.BeginScene to force the camera to instnatly snap to the desired position.

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

			//check veritcal movement
			if (_inputWrapper.Controller.CheckKeystrokeHeld(EKeystroke.Up))
			{
				_circle1.Translate(0.0f, -circleMovementSpeed * _clock.TimeDelta);
			}
			else if (_inputWrapper.Controller.CheckKeystrokeHeld(EKeystroke.Down))
			{
				_circle1.Translate(0.0f, circleMovementSpeed * _clock.TimeDelta);
			}

			//check horizontal movement
			if (_inputWrapper.Controller.CheckKeystrokeHeld(EKeystroke.Forward))
			{
				_circle1.Translate(circleMovementSpeed * _clock.TimeDelta, 0.0f);
			}
			else if (_inputWrapper.Controller.CheckKeystrokeHeld(EKeystroke.Back))
			{
				_circle1.Translate(-circleMovementSpeed * _clock.TimeDelta, 0.0f);
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

			//1. Add all our points to the camera
			AddCircleToCamera(_circle1);
			AddCircleToCamera(_circle2);

			//2. Update all the matrices of the camera before we start drawing
			_camera.BeginScene(false);

			spriteBatch.Begin(
				SpriteSortMode.Deferred,
				BlendState.NonPremultiplied,
				null, null, null, null,
				_camera.TranslationMatrix * Resolution.TransformationMatrix()); //3. MAGIC SAUCE: Multiply the Camera and Resolution matrixes to transform all the SpriteBatch.Draw calls.

			//Draw the background image so that we can see the camera moving easier
			spriteBatch.Draw(_texture, Vector2.Zero, Color.White);

			//draw the players circle in green
			primitive.Circle(_circle1.Pos, _circle1.Radius, Color.Green);

			//draw the stationary circle in red
			primitive.Circle(_circle2.Pos, _circle2.Radius, Color.Red);

			spriteBatch.End();

			//Start a new Spriteatch loop to draw our gui! 
			spriteBatch.Begin(
				SpriteSortMode.Deferred,
				BlendState.NonPremultiplied,
				null, null, null, null,
				Resolution.TransformationMatrix()); //Pass in the plain Resolution matrix so the GUI isn't transformed by the Camera.

			primitive.Rectangle(Resolution.TitleSafeArea, Color.Red);

			//Draw the center of the circles as white dots
			DrawCircleCenters();

			spriteBatch.End();

			base.Draw(gameTime);
		}

		private void AddCircleToCamera(Circle myCircle)
		{
			float pad = (myCircle.Radius * 1.5f); //add a bit of padding so they aren't touching the edge of the screen

			//Add the upperleft and lowerright corners.  That will fit the whole circle in camera
			_camera.AddPoint(myCircle.Pos);
			_camera.AddPoint(new Vector2((myCircle.Pos.X - pad), (myCircle.Pos.Y - pad)));
			_camera.AddPoint(new Vector2((myCircle.Pos.X + pad), (myCircle.Pos.Y + pad)));
		}

		public void DrawCircleCenters()
		{
			//This is a trick to draw bits of GUI over items that have been translated around by the CameraBuddy, for example if you want to draw a name over a character or something.

			var centerPosition1 = MatrixExt.Multiply(_camera.TranslationMatrix, _circle1.Pos); //Use the MatrixExt to get a point that has been transformed by the camera matrix.
			primitive.Circle(centerPosition1, 64, Color.White); //This circle will always be 64px but will be drawn on top of the circle. In theory. This math needs to be cleaned up :/
		}

		#endregion //Methods
	}
}
