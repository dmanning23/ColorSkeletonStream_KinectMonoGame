using Microsoft.Kinect;
using BasicPrimitiveBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ResolutionBuddy;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

		private const int ScreenX = 640;
		private const int ScreenY = 480;

		KinectTexture2D Tex;

		KinectSkeleton mySkel;

		XNABasicPrimitive prim;

		#endregion //MonoGame

		#region Kinect

		/// <summary>
		/// Active Kinect sensor
		/// </summary>
		private KinectSensor sensor;

		/// <summary>
		/// The skeleton object
		/// </summary>
		Skeleton skeleton;

		/// <summary>
		/// Intermediate storage for the depth data converted to color
		/// </summary>
		private byte[] colorPixels;

		ColorImageFormat colorFormat = ColorImageFormat.RgbResolution640x480Fps30;
		DepthImageFormat depthFormat = DepthImageFormat.Resolution640x480Fps30;

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

			Tex = new KinectTexture2D(ScreenX, ScreenY);
			Tex.Initialize(graphics.GraphicsDevice);

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

			prim = new XNABasicPrimitive(GraphicsDevice, spriteBatch);

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
				mySkel = new KinectSkeleton(640, 480);

				// Turn on the color stream to receive color frames
				this.sensor.ColorStream.Enable(colorFormat);

				// Allocate space to put the color pixels we'll create
				this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
				
				this.sensor.SkeletonStream.Enable();

				// Add an event handlers to be called whenever there is new frame data
				//this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;
				//this.sensor.ColorFrameReady += this.SensorColorFrameReady;
				this.sensor.AllFramesReady += this.AllFramesReady;

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

			Tex.PrepareToRender();

			mySkel.UpdateTexPosition(Tex);

			// Calculate Proper Viewport according to Aspect Ratio
			Resolution.ResetViewport();
			spriteBatch.Begin(SpriteSortMode.Immediate,
			BlendState.AlphaBlend,
			null, null, null, null,
			Resolution.TransformationMatrix());

			spriteBatch.Draw(Tex.Texture, new Vector2(0, 0), null, Color.White);

			mySkel.Render(prim);

			spriteBatch.End();

			base.Draw(gameTime);
		}

		#region Event Handlers

		/// <summary>
		/// Event handler for Kinect sensor's DepthFrameReady event
		/// </summary>
		/// <param name="sender">object sending the event</param>
		/// <param name="e">event arguments</param>
		private void AllFramesReady(object sender, AllFramesReadyEventArgs e)
		{
			do
			{
				//get the dimensions of the image
				int imageWidth, imageHeight = 0;
				using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
				{
					if (colorFrame == null)
					{
						break;
					}
					imageWidth = colorFrame.Width;
					imageHeight = colorFrame.Height;
					colorFrame.CopyPixelDataTo(this.colorPixels);
				}

				Tex.CopyFromKinectColorStream(imageWidth, imageHeight, colorPixels);
			} while (false);

			do
			{
				Skeleton[] skeletons = null;
				using (SkeletonFrame frame = e.OpenSkeletonFrame())
				{
					if (frame != null)
					{
						skeletons = new Skeleton[frame.SkeletonArrayLength];
						frame.CopySkeletonDataTo(skeletons);
					}
				}

				if (skeletons == null)
				{
					break;
				}

				skeleton = (from trackSkeleton in skeletons
							where trackSkeleton.TrackingState == SkeletonTrackingState.Tracked
							select trackSkeleton).FirstOrDefault();

				if (skeleton == null)
				{
					break;
				}

				//update our custom skeleton object
				mySkel.Update(skeleton);
				mySkel.UpdateColorPosition(sensor);
			} while (false);
		}

		/// <summary>
		/// Event handler for Kinect sensor's DepthFrameReady event
		/// </summary>
		/// <param name="sender">object sending the event</param>
		/// <param name="e">event arguments</param>
		private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
		{
			do
			{
				//get the dimensions of the image
				int imageWidth, imageHeight = 0;
				using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
				{
					if (colorFrame == null)
					{
						break;
					}
					imageWidth = colorFrame.Width;
					imageHeight = colorFrame.Height;
					colorFrame.CopyPixelDataTo(this.colorPixels);
				}

				Tex.CopyFromKinectColorStream(imageWidth, imageHeight, colorPixels);
			} while (false);
		}

		private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
		{
			do
			{
				Skeleton[] skeletons = null;
				using (SkeletonFrame frame = e.OpenSkeletonFrame())
				{
					if (frame != null)
					{
						skeletons = new Skeleton[frame.SkeletonArrayLength];
						frame.CopySkeletonDataTo(skeletons);
					}
				}

				if (skeletons == null)
				{
					break;
				}

				skeleton = (from trackSkeleton in skeletons
							where trackSkeleton.TrackingState == SkeletonTrackingState.Tracked
							select trackSkeleton).FirstOrDefault();

				if (skeleton == null)
				{
					break;
				}

				//update our custom skeleton object
				mySkel.Update(skeleton);
				mySkel.UpdateColorPosition(sensor);
			} while (false);
		}

		#endregion //Event Handlers

		#endregion //Methods
	}
}
