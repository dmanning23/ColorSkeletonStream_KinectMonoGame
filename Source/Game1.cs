using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ResolutionBuddy;
using System.Collections.Generic;
using System.IO;

namespace ColorSkeletonStream_KinectMonoGame
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Game
	{
		#region Members

		#region MonoGame

		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		private const int ScreenX = 1024;
		private const int ScreenY = 768;

		#endregion //MonoGame

		#region Kinect

		/// <summary>
		/// Active Kinect sensor
		/// </summary>
		private KinectSensor sensor;

		#region Skeleton Tracking

		/// <summary>
		/// The skeleton objects
		/// </summary>
		Skeleton[] skeletons = new Skeleton[0];

		#endregion //Skeleton Tracking

		#region Color Tracking

		/// <summary>
		/// Intermediate storage for the depth data converted to color
		/// </summary>
		private byte[] colorPixels;

		/// <summary>
		/// the texture to write to
		/// </summary>
		Texture2D pixels;

		/// <summary>
		/// temp buffer to hold convert kinect data to color objects
		/// </summary>
		Color[] pixelData_clear;

		#endregion //Color Tracking

		#endregion //Kinect

		#endregion //Members

		#region Methods

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";

			Resolution.Init(ref graphics);
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			Resolution.SetDesiredResolution(ScreenX, ScreenY);
			Resolution.SetScreenResolution(1280, 720, true);

			pixels = new Texture2D(graphics.GraphicsDevice,
							ScreenX,
							ScreenY, false, SurfaceFormat.Color);
			pixelData_clear = new Color[ScreenX * ScreenY];
			for (int i = 0; i < pixelData_clear.Length; ++i)
				pixelData_clear[i] = Color.Black;

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			// Look through all sensors and start the first connected one.
			// This requires that a Kinect is connected at the time of app startup.
			// To make your app robust against plug/unplug, 
			// it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
			foreach (var potentialSensor in KinectSensor.KinectSensors)
			{
				if (potentialSensor.Status == KinectStatus.Connected)
				{
					this.sensor = potentialSensor;
					break;
				}
			}

			if (null != this.sensor)
			{
				// Turn on the color stream to receive color frames
				this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

				// Allocate space to put the color pixels we'll create
				this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

				this.sensor.SkeletonStream.Enable();

				// Add an event handler to be called whenever there is new frame data
				this.sensor.AllFramesReady += this.SensorAllFramesReady;

				// Start the sensor!
				try
				{
					this.sensor.Start();
					//sensor.ColorStream.CameraSettings.BacklightCompensationMode = BacklightCompensationMode.CenterOnly;
				}
				catch (IOException)
				{
					this.sensor = null;
				}
			}

			//if (null == this.sensor)
			//{
			//	this.statusBarText.Text = Properties.Resources.NoKinectReady;
			//}
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			if (null != this.sensor)
			{
				this.sensor.Stop();
			}
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			if ((GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) ||
			Keyboard.GetState().IsKeyDown(Keys.Escape))
			{
				this.Exit();
			}

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			// Calculate Proper Viewport according to Aspect Ratio
			Resolution.ResetViewport();

			pixels.SetData<Color>(pixelData_clear);

			// Calculate Proper Viewport according to Aspect Ratio
			Resolution.ResetViewport();
			spriteBatch.Begin(SpriteSortMode.Immediate,
			BlendState.AlphaBlend,
			null, null, null, null,
			Resolution.TransformationMatrix());

			spriteBatch.Draw(pixels, new Vector2(0, 0), null, Color.White);

			//TODO: draw skeleton

			spriteBatch.End();

			base.Draw(gameTime);
		}

		/// <summary>
		/// Event handler for Kinect sensor's DepthFrameReady event
		/// </summary>
		/// <param name="sender">object sending the event</param>
		/// <param name="e">event arguments</param>
		private void SensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
		{
			//render the color image
			using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
			{
				if (colorFrame != null)
				{
					// Copy the pixel data from the image to a temporary array
					colorFrame.CopyPixelDataTo(this.colorPixels);

					//get the width of the image
					int imageWidth = colorFrame.Width;

					//get the height of the image
					int imageHeight = colorFrame.Height;

					// Convert the depth to RGB
					for (int pixelIndex = 0; pixelIndex < pixelData_clear.Length; pixelIndex++)
					{
						//get the pixel column
						int x = pixelIndex % ScreenX;

						//get the pixel row
						int y = pixelIndex / ScreenX;

						//convert the image x to cell x
						int x2 = (x * imageWidth) / ScreenX;

						//convert the image y to cell y
						int y2 = (y * imageHeight) / ScreenY;

						//get the index of the cell
						int cellIndex = ((y2 * imageWidth) + x2) * 4;

						//Create a new color
						pixelData_clear[pixelIndex] = new Color(colorPixels[cellIndex + 2], colorPixels[cellIndex + 1], colorPixels[cellIndex + 0]);
					}
				}
			}
		}

		#endregion //Methods
	}
}