using System;

namespace DCMAPI
	{
	public class DCM
		{
		static float	RadianToDegree	= (float)(180.0 / Math.PI);

		// DCM is a rotation matrix that rotates vectors
		// in the Body (moving) frame of reference 
		// to the Earth (static) frame of reference.
		//-----------------------------------------------
		// Axis of Body frame in the Earth frame:
		//		Xb/e Yb/e Zb/e
		//----------------------
		float	Rxx, Rxy, Rxz;	// Xe/b - X axis of Earth frame seen from the Body frame
		float	Ryx, Ryy, Ryz;	// Ye/b - Y axis of Earth frame seen from the Body frame
		float	Rzx, Rzy, Rzz;	// Ze/b - Z axis of Earth frame seen from the Body frame

		#region Constructor(s)
		//---------------------------------------------------------
		private DCM()
			{}
		//---------------------------------------------------------
		public DCM(Vector DR)
			{
			// DR (Delta Rotation) vector represents small
			// rotations: around X (roll), around Y (pitch),
			// and around Z (yaw)
			//--------------------------------------------
			// Infinitesimal Rotation Approximation...
			Rxx	=  1.0F;	Rxy = -DR.Z;	Rxz =  DR.Y;
			Ryx =  DR.Z;	Ryy =  1.0F;	Ryz = -DR.X;
			Rzx = -DR.Y;	Rzy =  DR.X;	Rzz =  1.0F;
			//--------------------------------------------
			//		1		-sin(Yaw)		 sin(Pitch)
			//	 sin(Yaw)		1			-sin(Roll)
			//	-sin(Pitch)	 sin(Roll)			1
			//--------------------------------------------
			}
		//---------------------------------------------------------
		public DCM(DCM Base)
			{
			Rxx	= Base.Rxx;		Rxy = Base.Rxy;		Rxz = Base.Rxz;
			Ryx = Base.Ryx;		Ryy = Base.Ryy;		Ryz = Base.Ryz;
			Rzx = Base.Rzx;		Rzy = Base.Rzy;		Rzz = Base.Rzz;
			}
		//---------------------------------------------------------
		#endregion

		#region DCM Transform(DCM T)
		public	DCM Transform(DCM T)
			{
			// Allocate new uninitialized Rotation Matrix
			DCM P = new DCM();
			// Perform multiplication:
			// First row...
			P.Rxx = Rxx*T.Rxx + Rxy*T.Ryx + Rxz*T.Rzx;
			P.Rxy = Rxx*T.Rxy + Rxy*T.Ryy + Rxz*T.Rzy;
			P.Rxz = Rxx*T.Rxz + Rxy*T.Ryz + Rxz*T.Rzz;
			// Second row...
			P.Ryx = Ryx*T.Rxx + Ryy*T.Ryx + Ryz*T.Rzx;
			P.Ryy = Ryx*T.Rxy + Ryy*T.Ryy + Ryz*T.Rzy;
			P.Ryz = Ryx*T.Rxz + Ryy*T.Ryz + Ryz*T.Rzz;
			// Third row...
			P.Rzx = Rzx*T.Rxx + Rzy*T.Ryx + Rzz*T.Rzx;
			P.Rzy = Rzx*T.Rxy + Rzy*T.Ryy + Rzz*T.Rzy;
			P.Rzz = Rzx*T.Rxz + Rzy*T.Ryz + Rzz*T.Rzz;
			//--------------------------------------------
			return P;
			}
		#endregion

		#region DCM Normalize()
		public	DCM Normalize()
			{
			DCM N = new DCM();
			//--------------------------------------
			float	Error	= 0;
			//======================================
			#region Adjust orthogonality of the rows
			// Calculate Error based upon the orthogonality of the
			// first two rows:
			Error = Rxx*Ryx + Rxy*Ryy + Rxz*Ryz;
			// Apply correction to first two row:
			// Xb = Xb - (Error/2)*Yb;
			// Yb = Yb - (Error/2)*Xb;
			//----------------------------------------
			Error = Error/2;	// Preparation...
			//----------------------------------------
			N.Rxx = Rxx - Error*Ryx;
			N.Rxy = Rxy - Error*Ryy;
			N.Rxz = Rxz - Error*Ryz;
			// Second row...
			N.Ryx = Ryx - Error*Rxx;
			N.Ryy = Ryy - Error*Rxy;
			N.Ryz = Ryz - Error*Rxz;
			//----------------------------------------
			// Third row - a cross-product of the
			// first two: (A=Rx, B=Ry)
			//----------------------------------------
			N.Rzx = Rxy*Ryz - Rxz*Ryy;
			N.Rzy = Rxz*Ryx - Rxx*Ryz;
			N.Rzz = Rxx*Ryy - Rxy*Ryx;
			//-----------------------------------------
			#endregion
			//======================================
			#region Normalize the length of axis vectors
			float Scale;
			//-----------------------------------------
			// First row...
			Scale = (3 - (N.Rxx*N.Rxx + N.Rxy*N.Rxy + N.Rxz*N.Rxz))/2;
			N.Rxx = Scale*N.Rxx;
			N.Rxy = Scale*N.Rxy;
			N.Rxz = Scale*N.Rxz;
			// Second row...
			Scale = (3 - (N.Ryx*N.Ryx + N.Ryy*N.Ryy + N.Ryz*N.Ryz))/2;
			N.Ryx = Scale*N.Ryx;
			N.Ryy = Scale*N.Ryy;
			N.Ryz = Scale*N.Ryz;
			//----------------------------------------
			// Third row...
			//----------------------------------------
			Scale = (3 - (N.Rzx*N.Rzx + N.Rzy*N.Rzy + N.Rzz*N.Rzz))/2;
			N.Rzx = Scale*N.Rzx;
			N.Rzy = Scale*N.Rzy;
			N.Rzz = Scale*N.Rzz;
			#endregion
			//======================================
			return N;
			}
		#endregion

		#region	DCM Transpose()
		public	DCM Transpose()
			{
			DCM T = new DCM();
			//--------------------------------------------
			T.Rxx = Rxx;	T.Rxy = Ryx;	T.Rxz = Rzx;
			T.Ryx = Rxy;	T.Ryy = Ryy;	T.Ryz = Rzy;
			T.Rzx = Rxz;	T.Rzy = Ryz;	T.Rzz = Rzz;
			//--------------------------------------------
			return T;
			}
		#endregion



		#region DCM: Rotate vector to Earth frame
		public Vector ToEarth(Vector InBody)
			{
			return new Vector
					(
					XEarth * InBody,
					YEarth * InBody,
					ZEarth * InBody
					);
			}
		#endregion

		#region DCM: Rotate vector to Body frame
		public Vector ToBody(Vector InEarth)
			{
			return new Vector
					(
					InEarth * XBody,
					InEarth * YBody,
					InEarth * ZBody
					);
			}
		#endregion

		#region Yaw, Pitch, Roll properties
		//---------------------------------------------------------
		// Rzz is negative if the IMU is "upside-down"
		//---------------------------------------------------------
		public	float Z
			{
			get { return Rzz; } 
			}
		//---------------------------------------------------------
		public float Pitch
			{ 
			get { return -(float)Math.Asin((double)Rzx); } 
			}
		//---------------------------------------------------------
		public float PitchDeg
			{ 
			get { return RadianToDegree * Pitch; } 
			}
		//---------------------------------------------------------
		public float Roll
			{ 
			//get { return (float)Math.Asin((double)Rzy); } 
			get { 
				if (0 == Rzy && 0 == Rzz )
					return 0;	// Special case
				//--------------------------------------------------
				return (float)Math.Atan2((double)Rzy, (double)Rzz); 
				} 
			}
		//---------------------------------------------------------
		public float RollDeg
			{ 
			get { return RadianToDegree * Roll; } 
			}
		//---------------------------------------------------------
		public float Yaw
			{
			get {return (float)Math.Atan2(Ryx, Rxx);} 
			}
		//---------------------------------------------------------
		public float YawDeg
			{ 
			get { return RadianToDegree * Yaw; } 
			}
		//---------------------------------------------------------
		#endregion

		#region Base Vectors
		//================================================
		// Body axis in the Earth frame of reference are
		// the columns of the Rotation Matrix
		//================================================
		public Vector XBody
			{ get { return new Vector(Rxx, Ryx, Rzx); } }
		//------------------------------------------------
		public Vector YBody
			{ get { return new Vector(Rxy, Ryy, Rzy); } }
		//------------------------------------------------
		public Vector ZBody
			{ get { return new Vector(Rxz, Ryz, Rzz); } }
		//================================================
		// Earth axis in the Body frame of reference are
		// the rows of the Rotation Matrix
		//================================================
		public Vector XEarth
			{ get { return new Vector(Rxx, Rxy, Rxz); } }
		//------------------------------------------------
		public Vector YEarth
			{ get { return new Vector(Ryx, Ryy, Ryz); } }
		//------------------------------------------------
		public Vector ZEarth
			{ get { return new Vector(Rzx, Rzy, Rzz); } }
		//================================================
		#endregion

		public String[] Print(String Name)
			{
			String[] Result = new String[4];
			//-----------------------------------------------------------------
			Result[0] = String.Format("\r\n\tRotation Matrix <{0}>", Name);
			Result[1] = String.Format("\t{0,8:F5}\t{1,8:F5}\t{2,8:F5}", Rxx, Rxy, Rxz);
			Result[2] = String.Format("\t{0,8:F5}\t{1,8:F5}\t{2,8:F5}", Ryx, Ryy, Ryz);
			Result[3] = String.Format("\t{0,8:F5}\t{1,8:F5}\t{2,8:F5}", Rzx, Rzy, Rzz);
			//-----------------------------------------------------------------
			return	Result;
			}

		}
	}
