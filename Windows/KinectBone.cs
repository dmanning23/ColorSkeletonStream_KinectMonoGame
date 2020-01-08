using Microsoft.Xna.Framework;
using PrimitiveBuddy;

namespace ColorSkeletonStream_KinectMonoGame
{
	class KinectBone
	{
		#region Members

		public KinectJoint First { get; private set; }

		public KinectJoint Second { get; private set; }

		#endregion Members

		#region Methods

		public KinectBone(KinectJoint first, KinectJoint second)
		{
			First = first;
			Second = second;
		}

		/// <summary>
		/// draw the bone
		/// </summary>
		/// <param name="prim"></param>
		public void Render(IPrimitive prim)
		{
			if (First.IsValidPosition() && Second.IsValidPosition())
			{
				prim.Thickness = 5;
				prim.Line(First.TexPosition, Second.TexPosition, Color.Red);
			}
		}

		#endregion Methods
	}
}
