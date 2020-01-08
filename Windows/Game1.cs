using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PrimitiveBuddy;
using ResolutionBuddy;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

		IPrimitive prim;

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

		/// <summary>
		/// Intermediate storage for the depth data received from the camera
		/// </summary>
		private DepthImagePixel[] depthPixels;

		ColorImageFormat colorFormat = ColorImageFormat.RgbResolution640x480Fps30;
		DepthImageFormat depthFormat = DepthImageFormat.Resolution80x60Fps30;

		IResolution _resolution;

		#endregion //Kinect

		#endregion //Members

		#region Methods

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";

			_resolution = new ResolutionComponent(this, graphics, new Point(1280, 720), new Point(1280, 720), false, false);
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
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

			prim = new Primitive(GraphicsDevice, spriteBatch);

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
				mySkel = new KinectSkeleton(80, 60);

				// Turn on the color stream to receive color frames
				//this.sensor.ColorStream.Enable(colorFormat);

				// Allocate space to put the color pixels we'll create
				//this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

				// Turn on the depth stream to receive depth frames
				this.sensor.DepthStream.Enable(depthFormat);

				// Allocate space to put the depth pixels we'll receive
				this.depthPixels = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];

				this.sensor.SkeletonStream.Enable();

				// Add an event handlers to be called whenever there is new frame data
				this.sensor.SkeletonFrameReady += this.SensorSkeletonDepthFrameReady;
				//this.sensor.ColorFrameReady += this.SensorColorFrameReady;
				this.sensor.DepthFrameReady += this.SensorDepthFrameReady;
				//this.sensor.AllFramesReady += this.AllFramesReady;

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

			if (null != mySkel)
			{
				mySkel.UpdateTexPosition(Tex);
			}

			// Calculate Proper Viewport according to Aspect Ratio
			Resolution.ResetViewport();
			spriteBatch.Begin(SpriteSortMode.Immediate,
			BlendState.AlphaBlend,
			null, null, null, null,
			Resolution.TransformationMatrix());

			spriteBatch.Draw(Tex.Texture, new Vector2(0, 0), null, Color.White);

			if (null != mySkel)
			{
				mySkel.Render(prim);
			}

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
				if (null != mySkel)
				{
					mySkel.Update(skeleton);
					mySkel.UpdateColorPosition(sensor, colorFormat);
				}
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

				Task.Factory.StartNew(() =>
				{
					Tex.CopyFromKinectColorStream(imageWidth, imageHeight, colorPixels);
				});
			} while (false);
		}

		/// <summary>
		/// Event handler for Kinect sensor's DepthFrameReady event
		/// </summary>
		/// <param name="sender">object sending the event</param>
		/// <param name="e">event arguments</param>
		private void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
		{
			do
			{
				//get the dimensions of the image
				int imageWidth, imageHeight, minDepth, maxDepth = 0;
				using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
				{
					if (depthFrame == null)
					{
						break;
					}
					imageWidth = depthFrame.Width;
					imageHeight = depthFrame.Height;
					minDepth = depthFrame.MinDepth;
					maxDepth = depthFrame.MaxDepth;
					depthFrame.CopyDepthImagePixelDataTo(this.depthPixels);
				}

				Task.Factory.StartNew(() =>
				{
					Tex.CopyFromKinectDepthStream(minDepth, maxDepth, imageWidth, imageHeight, depthPixels);
				});
			} while (false);
		}

		private void SensorSkeletonColorFrameReady(object sender, SkeletonFrameReadyEventArgs e)
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

				if (null != mySkel)
				{
					//update our custom skeleton object
					mySkel.Update(skeleton);
					mySkel.UpdateColorPosition(sensor, colorFormat);
				}
			} while (false);
		}

		private void SensorSkeletonDepthFrameReady(object sender, SkeletonFrameReadyEventArgs e)
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

				if (null != mySkel)
				{
					//update our custom skeleton object
					Task.Factory.StartNew(() =>
					{
						mySkel.Update(skeleton);
						mySkel.UpdateDepthPosition(sensor, depthFormat);
					});
				}
			} while (false);
		}

		#endregion //Event Handlers

		#endregion //Methods
	}
}
