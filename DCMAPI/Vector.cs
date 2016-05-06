using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCMAPI
	{
	public class Vector
		{
		float m_X;
		float m_Y;
		float m_Z;

		#region Constructor(s)
		public Vector()
		//--------------------
			{
			m_X	= 0.0F;
			m_Y	= 0.0F;
			m_Z = 0.0F;
			}
		//--------------------
		public Vector(float X, float Y, float Z)
			{
			m_X	= X;
			m_Y	= Y;
			m_Z = Z;
			}
		//--------------------
		#endregion

		#region Properties
		//----------------------
		public float X
			{
			get { return m_X; }
			set { m_X = value; }
			}
		//----------------------
		public float Y
			{
			get { return m_Y; }
			set { m_Y = value; }
			}
		//----------------------
		public float Z
			{
			get { return m_Z; }
			set { m_Z = value; }
			}
		//----------------------
		public float Size
			{
			get { return (float)Math.Sqrt(m_X*m_X + m_Y*m_Y + m_Z*m_Z); }
			}
		//----------------------
		#endregion

		#region Custom Unary Operator: -
		//--------------------------------------------------
		public static Vector operator -(Vector V1)
			{
			return new Vector	(
								-V1.m_X,
								-V1.m_Y,
								-V1.m_Z
								);
			}
		//--------------------------------------------------
		#endregion

		#region Custom Binary Operators: (Vector * Number) and (Vector / Number)
		//--------------------------------------------------
		public static Vector operator *(Vector V1, float F)
			{
			return new Vector	(
								V1.m_X * F,
								V1.m_Y * F,
								V1.m_Z * F
								);
			}
		//--------------------------------------------------
		public static Vector operator /(Vector V1, float F)
			{
			return new Vector	(
								V1.m_X / F,
								V1.m_Y / F,
								V1.m_Z / F
								);
			}
		//--------------------------------------------------
		#endregion

		#region Custom Binary Operator "&" - SCALE vector by vector
		public static Vector operator & (Vector V1, Vector V2)
			{
			return new Vector	(
								V1.m_X * V2.m_X,
								V1.m_Y * V2.m_Y,
								V1.m_Z * V2.m_Z
								);
			}
		#endregion

		#region Custom Binary Operators: + and -
		//--------------------------------------------------
		public static Vector operator +(Vector V1, Vector V2)
			{
			return new Vector	(
								V1.m_X + V2.m_X,
								V1.m_Y + V2.m_Y,
								V1.m_Z + V2.m_Z
								);
			}
		//--------------------------------------------------
		public static Vector operator -(Vector V1, Vector V2)
			{
			return new Vector	(
								V1.m_X - V2.m_X,
								V1.m_Y - V2.m_Y,
								V1.m_Z - V2.m_Z
								);
			}
		//--------------------------------------------------
		#endregion

		#region Custom Binary Operators: Dot (*) and Cross(^) products
		//--------------------------------------------------
		// Dot product 
		//--------------------------------------------------
		public static float operator *(Vector V1, Vector V2)
			{
			return V1.m_X*V2.m_X + V1.m_Y*V2.m_Y + V1.m_Z*V2.m_Z;
			}
		//--------------------------------------------------
		// Dot product 
		//--------------------------------------------------
		public static Vector operator ^(Vector V1, Vector V2)
			{
			return new Vector	(
								V1.m_Y*V2.m_Z - V1.m_Z*V2.m_Y,
								V1.m_Z*V2.m_X - V1.m_X*V2.m_Z,
								V1.m_X*V2.m_Y - V1.m_Y*V2.m_X
								);
			}
		#endregion

		public String[] Print(String Name)
			{
			String[] Result = new String[2];
			//-----------------------------------------------------------------
			Result[0] = String.Format("\r\n\t<{0}> vector", Name);
			Result[1] = ToString("\t{0,8:F5}\t{1,8:F5}\t{2,8:F5}");
			//-----------------------------------------------------------------
			return	Result;
			}

		public String ToString(String Format)
			{
			return String.Format(Format, m_X, m_Y, m_Z);
			}
		}
	}
