using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using PrimitiveBuddy;

namespace ColorSkeletonStream_KinectMonoGame
{
	class KinectJoint
	{
		#region Members

		private SkeletonPoint _skelPosition;

		private ColorImagePoint _colorPosition;

		private DepthImagePoint _depthPosition;

		private Vector2 _texPosition;

		private object _lock = new object();

		#endregion //Members

		#region Properties

		/// <summary>
		/// The joints location in skeleton space
		/// </summary>
		public SkeletonPoint SkeletonPosition 
		{
			get
			{
				return _skelPosition;
			}
			set
			{
				_skelPosition = value;
			}
		}

		/// <summary>
		/// the joints location in the color space
		/// </summary>
		public ColorImagePoint ColorPosition
		{
			get
			{
				return _colorPosition;
			}
			set
			{
				_colorPosition = value;
			}
		}

		/// <summary>
		/// the joints location in the depth space
		/// </summary>
		public DepthImagePoint DepthPosition
		{
			get
			{
				return _depthPosition;
			}
			set
			{
				_depthPosition = value;
			}
		}

		/// <summary>
		/// The location in texture space
		/// </summary>
		public Vector2 TexPosition
		{
			get
			{
				return _texPosition;
			}
			set
			{
				_texPosition = value;
			}
		}

		public JointTrackingState TrackingState { get; set; }

		public JointType JointType { get; set; }

		public bool LeafJoint { get; private set; }

		public int ColorWidth { get; set; }
		public int ColorHeight { get; set; }

		#endregion //Properties

		#region Methods

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="jType"></param>
		public KinectJoint(JointType jType, int width, int height)
		{
			JointType = jType;
			LeafJoint = IsLeafJoint();

			//set these to some default values
			ColorWidth = width;
			ColorHeight = height;
		}

		public void Update(Skeleton skel)
		{
			//find this dude's kinect joint
			Joint myJoint = skel.Joints[JointType];

			//update my position and stuff
			UpdateSkeletonPosition(myJoint);
		}

		private void UpdateSkeletonPosition(Joint myJoint)
		{
			lock (_lock)
			{
				//set the position
				_skelPosition = myJoint.Position;

				//set the tracking state
				TrackingState = myJoint.TrackingState;
			}
		}

		public void UpdateColorPosition(KinectSensor sensor, ColorImageFormat colorFormat)
		{
			lock (_lock)
			{
				if (IsValidPosition())
				{
					//update the skel pos to the color stream 
					ColorPosition = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(SkeletonPosition, colorFormat);
				}
			}
		}

		public void UpdateDepthPosition(KinectSensor sensor, DepthImageFormat depthFormat)
		{
			lock (_lock)
			{
				if (IsValidPosition())
				{
					//update the skel pos to the depth stream 
					DepthPosition = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(SkeletonPosition, depthFormat);
				}
			}
		}

		public void UpdateTexPosition(KinectTexture2D texture)
		{
			lock (_lock)
			{
				if (IsValidPosition())
				{
					////take the color pos and translate to texture position
					//_texPosition.X = (ColorPosition.X * texture.Width) / ColorWidth;
					//_texPosition.Y = (ColorPosition.Y * texture.Height) / ColorHeight;

					//take the color pos and translate to texture position
					_texPosition.X = (DepthPosition.X * texture.Width) / ColorWidth;
					_texPosition.Y = (DepthPosition.Y * texture.Height) / ColorHeight;
				}
			}
		}

		public void Render(IPrimitive prim)
		{
			lock (_lock)
			{
				if (IsValidPosition())
				{
					//draw a dot at the correct spot
					prim.Thickness = 5;
					Color myColor = Color.Green;
					if (TrackingState == JointTrackingState.Inferred)
					{
						myColor = Color.Yellow;
					}
					prim.Circle(TexPosition, 10, myColor);
				}
			}
		}

		public bool IsValidPosition()
		{
			return ((TrackingState == JointTrackingState.Tracked) ||
				(TrackingState == JointTrackingState.Inferred));
		}

		/// <summary>
		/// Check if this is a leaf joint
		/// </summary>
		/// <returns></returns>
		private bool IsLeafJoint()
		{
			switch (JointType)
			{
				case JointType.HandRight: return true;
				case JointType.HandLeft: return true;
				case JointType.FootRight: return true;
				case JointType.FootLeft: return true;
				case JointType.Head: return true;
				default: return false;
			}
		}

		#endregion //Methods
	}
}
