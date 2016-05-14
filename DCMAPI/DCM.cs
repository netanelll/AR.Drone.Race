using System;

namespace DCMAPI
{
    public class DCM
    {
     //   static float RadianToDegree = (float)(180.0 / Math.PI);

        private Matrix3 rM, pM, yM, dcm, inv_dcm;

        #region Constructor(s)
        //---------------------------------------------------------
        private DCM()
        {
            rM = new Matrix3();
            pM = new Matrix3();
            yM = new Matrix3();
            dcm = new Matrix3();
        }
        //---------------------------------------------------------
        public DCM(double roll, double pitch, double yaw)
        {
            rM = new Matrix3((new double[,] { { 1, 0, 0 }, { 0, Math.Cos(roll), (-1 * Math.Sin(roll)) }, { 0, Math.Sin(roll), Math.Cos(roll) } }));
            pM = new Matrix3((new double[,] { { Math.Cos(pitch), 0, Math.Sin(pitch) }, { 0, 1, 0 }, { (-1 * Math.Sin(pitch)), 0, Math.Cos(pitch) } }));
            yM = new Matrix3((new double[,] { { Math.Cos(yaw), (-1 * Math.Sin(yaw)), 0 }, { Math.Sin(yaw), Math.Cos(yaw), 0 }, { 0, 0, 1 } }));
            dcm = (yM * pM) * rM;
          //  inv_dcm = Matrix3.INV(dcm);
        }
        //---------------------------------------------------------
        public DCM(double yaw)
        {
            dcm =  new Matrix3((new double[,] { { Math.Cos(yaw), (-1 * Math.Sin(yaw)), 0 }, { Math.Sin(yaw), Math.Cos(yaw), 0 }, { 0, 0, 1 } }));
          //  inv_dcm = Matrix3.INV(dcm);
        }
        #endregion

        #region DCM: Rotate vector to Earth frame
        public Vector_3 ToEarth(Vector_3 vec)
        {
            return dcm * vec;
        }
        #endregion









    }
}
