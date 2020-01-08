using Microsoft.Kinect;
using PrimitiveBuddy;
using System;
using System.Collections.Generic;

namespace ColorSkeletonStream_KinectMonoGame
{
	class KinectSkeleton
	{
		#region Members

		List<KinectJoint> Joints { get; set; }

		List<KinectBone> Bones { get; set; }

		#endregion //Members

		#region Methods

		public KinectSkeleton(int width, int height)
		{
			Joints = new List<KinectJoint>();
			Bones = new List<KinectBone>();

			//create all the joints
			int numJoints = Enum.GetValues(typeof(JointType)).Length;
			for (int i = 0; i < numJoints; i++)
			{
				Joints.Add(new KinectJoint((JointType)i, width, height));
			}

			//create all the bones
			Bones.Add(new KinectBone(Joints[(int)JointType.Head], Joints[(int)JointType.ShoulderCenter]));
			Bones.Add(new KinectBone(Joints[(int)JointType.ShoulderCenter], Joints[(int)JointType.Spine]));

			Bones.Add(new KinectBone(Joints[(int)JointType.ShoulderCenter], Joints[(int)JointType.ShoulderLeft]));
			Bones.Add(new KinectBone(Joints[(int)JointType.ShoulderLeft], Joints[(int)JointType.ElbowLeft]));
			Bones.Add(new KinectBone(Joints[(int)JointType.ElbowLeft], Joints[(int)JointType.WristLeft]));
			Bones.Add(new KinectBone(Joints[(int)JointType.WristLeft], Joints[(int)JointType.HandLeft]));

			Bones.Add(new KinectBone(Joints[(int)JointType.ShoulderCenter], Joints[(int)JointType.ShoulderRight]));
			Bones.Add(new KinectBone(Joints[(int)JointType.ShoulderRight], Joints[(int)JointType.ElbowRight]));
			Bones.Add(new KinectBone(Joints[(int)JointType.ElbowRight], Joints[(int)JointType.WristRight]));
			Bones.Add(new KinectBone(Joints[(int)JointType.WristRight], Joints[(int)JointType.HandRight]));

			Bones.Add(new KinectBone(Joints[(int)JointType.Spine], Joints[(int)JointType.HipCenter]));
			Bones.Add(new KinectBone(Joints[(int)JointType.HipCenter], Joints[(int)JointType.HipLeft]));
			Bones.Add(new KinectBone(Joints[(int)JointType.HipLeft], Joints[(int)JointType.KneeLeft]));
			Bones.Add(new KinectBone(Joints[(int)JointType.KneeLeft], Joints[(int)JointType.AnkleLeft]));
			Bones.Add(new KinectBone(Joints[(int)JointType.AnkleLeft], Joints[(int)JointType.FootLeft]));

			Bones.Add(new KinectBone(Joints[(int)JointType.HipCenter], Joints[(int)JointType.HipRight]));
			Bones.Add(new KinectBone(Joints[(int)JointType.HipRight], Joints[(int)JointType.KneeRight]));
			Bones.Add(new KinectBone(Joints[(int)JointType.KneeRight], Joints[(int)JointType.AnkleRight]));
			Bones.Add(new KinectBone(Joints[(int)JointType.AnkleRight], Joints[(int)JointType.FootRight]));
		}

		public void Update(Skeleton skel)
		{
			for (int i = 0; i < Joints.Count; i++)
			{
				Joints[i].Update(skel);
			}
		}

		public void UpdateColorPosition(KinectSensor sensor, ColorImageFormat colorFormat)
		{
			for (int i = 0; i < Joints.Count; i++)
			{
				Joints[i].UpdateColorPosition(sensor, colorFormat);
			}
		}

		public void UpdateDepthPosition(KinectSensor sensor, DepthImageFormat depthFormat)
		{
			for (int i = 0; i < Joints.Count; i++)
			{
				Joints[i].UpdateDepthPosition(sensor, depthFormat);
			}
		}

		public void UpdateTexPosition(KinectTexture2D texture)
		{
			for (int i = 0; i < Joints.Count; i++)
			{
				Joints[i].UpdateTexPosition(texture);
			}
		}

		public void Render(IPrimitive prim)
		{
			for (int i = 0; i < Bones.Count; i++)
			{
				Bones[i].Render(prim);
			}

			for (int i = 0; i < Joints.Count; i++)
			{
				Joints[i].Render(prim);
			}
		}

		#endregion //Methods
	}
}
