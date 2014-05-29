using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ColorSkeletonStream_KinectMonoGame
{
	/// <summary>
	/// Helper class for mapping a kinect color stream to a texture
	/// </summary>
	class KinectDepthTexture
	{
		#region Members

		private object _lock = new object();

		/// <summary>
		/// the texture that will contain the kinect image
		/// </summary>
		public Texture2D Texture { get; private set; }

		/// <summary>
		/// This array will hold the color data for each pixel of the texture
		/// </summary>
		private Color[] PixelData { get; set; }

		/// <summary>
		/// width of the image
		/// </summary>
		public int Width { get; private set; }

		/// <summary>
		/// height of the image
		/// </summary>
		public int Height { get; private set; }

		#endregion //Members

		#region Methods

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public KinectDepthTexture(int width, int height)
		{
			Width = width;
			Height = height;
		}

		/// <summary>
		/// Initialize the internal data structures
		/// </summary>
		/// <param name="graphicsDevice"></param>
		public void Initialize(GraphicsDevice graphicsDevice)
		{
			//create the texture
			Texture = new Texture2D(graphicsDevice, Width, Height, false, SurfaceFormat.Color);

			//create the color data
			PixelData = new Color[Width * Height];
		}

		/// <summary>
		/// Called before this dude is rendered to setup the texture2d member
		/// </summary>
		public void PrepareToRender()
		{
			lock (_lock)
			{
				Texture.SetData<Color>(PixelData);
			}
		}

		public void CopyFromKinectDepthStream(DepthImageFrame depthFrame, DepthImagePixel[] depthPixels, KinectSensor sensor, ColorImageFormat colorFormat)
		{
			// Get the min and max reliable depth for the current frame
			int minDepth = depthFrame.MinDepth;
			int maxDepth = depthFrame.MaxDepth;

			//Get the depth delta
			int depthDelta = maxDepth - minDepth;

			//get the width of the image
			int imageWidth = depthFrame.Width;

			//get the height of the image
			int imageHeight = depthFrame.Height;

			//setup temp variables
			int x, y, x2, y2, imageIndex = 0;
			short depth = 0;
			byte intensity = 0;
			DepthImagePixel depthPixel;
			ColorImagePoint colorPoint;

			// Convert the depth to RGB
			for (int pixelIndex = 0; pixelIndex < PixelData.Length; pixelIndex++)
			{
				//get the pixel column
				x = pixelIndex % Width;

				//get the pixel row
				y = pixelIndex / Height;

				//convert the image x to cell x
				x2 = (x * imageWidth) / Width;

				//convert the image y to cell y
				y2 = (y * imageHeight) / Height;

				//get the index of the cell
				imageIndex = (y2 * imageWidth) + x2;
				depthPixel = depthPixels[imageIndex];
				DepthImagePoint depthPoint = new DepthImagePoint()
				{
					X = x2,
					Y = y2,
					Depth = depthPixel.Depth,
					//PlayerIndex = depthPixel.PlayerIndex
				};

				  //convert to color point

				  //convert back to depth point

				  // Get the depth for this pixel
				  depth = depthPixels[imageIndex].Depth;

				//convert to a range that will fit in one byte
				intensity = 0;
				if (depth >= minDepth && depth <= maxDepth)
				{
					intensity = (byte)(byte.MaxValue - ((depth * byte.MaxValue) / depthDelta));
				}

				//set the color
				PixelData[pixelIndex] = new Color(intensity, intensity, intensity);
			}
		}

		#endregion //Methods
	}
}
