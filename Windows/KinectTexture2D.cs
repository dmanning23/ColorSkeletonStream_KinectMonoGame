using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ColorSkeletonStream_KinectMonoGame
{
	/// <summary>
	/// Helper class for mapping a kinect color stream to a texture
	/// </summary>
	class KinectTexture2D
	{
		#region Members

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
		public KinectTexture2D(int width, int height)
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
			for (int i = 0; i < PixelData.Length; ++i)
			{
				PixelData[i] = Color.Black;
			}
		}

		/// <summary>
		/// Called before this dude is rendered to setup the texture2d member
		/// </summary>
		public void PrepareToRender()
		{
			Texture.SetData<Color>(PixelData);
		}

		/// <summary>
		/// Copy the data from a color image frame into this dude's color buffer
		/// </summary>
		/// <param name="colorFrame"></param>
		public void CopyFromKinectColorStream(ColorImageFrame colorFrame, byte[] colorPixels)
		{
			//get the width of the image
			int imageWidth = colorFrame.Width;

			//get the height of the image
			int imageHeight = colorFrame.Height;

			// Convert the depth to RGB
			for (int pixelIndex = 0; pixelIndex < PixelData.Length; pixelIndex++)
			{
				//get the pixel column
				int x = pixelIndex % Width;

				//get the pixel row
				int y = pixelIndex / Height;

				//convert the image x to cell x
				int x2 = (x * imageWidth) / Width;

				//convert the image y to cell y
				int y2 = (y * imageHeight) / Height;

				//get the index of the cell
				int cellIndex = ((y2 * imageWidth) + x2) * 4;

				//Create a new color
				PixelData[pixelIndex] = new Color(colorPixels[cellIndex + 2], colorPixels[cellIndex + 1], colorPixels[cellIndex + 0]);
			}
		}

		public void CopyFromKinectDepthStream(DepthImageFrame depthFrame, DepthImagePixel[] depthPixels)
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

			// Convert the depth to RGB
			for (int pixelIndex = 0; pixelIndex < PixelData.Length; pixelIndex++)
			{
				//get the pixel column
				int x = pixelIndex % Width;

				//get the pixel row
				int y = pixelIndex / Height;

				//convert the image x to cell x
				int x2 = (x * imageWidth) / Width;

				//convert the image y to cell y
				int y2 = (y * imageHeight) / Height;

				//get the index of the cell
				int imageIndex = (y2 * imageWidth) + x2;

				// Get the depth for this pixel
				short depth = depthPixels[imageIndex].Depth;

				//convert to a range that will fit in one byte
				byte intensity = 0;
				if (depth >= minDepth && depth <= maxDepth)
				{
					intensity = (byte)((depth * byte.MaxValue) / depthDelta);
				}

				//set the color
				PixelData[pixelIndex] = new Color(intensity, intensity, intensity);
			}
		}

		#endregion //Methods
	}
}
