
#define RECORD


using AR.Drone.Data.Navigation;
using AR.Drone.Video;
using DCMAPI;
using System;
using System.Collections.Generic;

namespace AR.Drone.WinApp
{

    class RaceController
    {
#if RECORD
        private List<NavigationData> navDataOverTime;
        private List<float> timeOverTime;
        private List<Vector_3> cordOverTime;
#endif
        private long _start_ticks, _end_ticks, _prev_tick;
        private bool _isRacing;
        float _x_cord, _y_cord, _z_cord, _roll, _pitch, _yaw;
        const float TICKS_TO_SEC = 0.0000001f; // cov from 100 nano sec to sec
        public double _north,_northDeg;
        private float _startingYaw; // saving the first yaw response to get the quad direction
        private int  count;
        Vector_3 sum = new Vector_3(0,0,0);
        private double[] _magClib = new double[] { 0.004291144772399, -2.632443996201751e-04, -0.159509130315845, -2.632443995302962e-04, 0.004576539823135, 0.198956637219492 };
        #region properties

        public float X_cord
        {
            get
            {
                return _x_cord;
            }

            set
            {
                _x_cord = value;
            }
        }

        public float Y_cord
        {
            get
            {
                return _y_cord;
            }

            set
            {
                _y_cord = value;
            }
        }

        public float Z_cord
        {
            get
            {
                return _z_cord;
            }

            set
            {
                _z_cord = value;
            }
        }

        public bool IsRacing
        {
            get
            {
                return _isRacing;
            }

            set
            {
                _isRacing = value;
            }
        }
        #endregion

        public RaceController()
        {
            _x_cord = 0;
            _y_cord = 0;
            _z_cord = 0;
            _roll = 0;
            _pitch = 0;
            _yaw = 0;
            _start_ticks = 0;
            _end_ticks = 0;
            _isRacing = false;
        }

        public void startRace()
        {
            _x_cord = 0;
            _y_cord = 0;
            _z_cord = 0;
            _roll = 0;
            _pitch = 0;
            _yaw = 0;
            _start_ticks = 0;
            _end_ticks = 0;
            _isRacing = true;
            _start_ticks = DateTime.Now.Ticks;
#if RECORD
            navDataOverTime = new List<NavigationData>();
            timeOverTime = new List<float>();
            cordOverTime = new List<Vector_3>();
#endif
        }

        public void endRace()
        {
            if (_isRacing)
            {
                _isRacing = false;
                _end_ticks = DateTime.Now.Ticks;
#if RECORD
            string fileName = "navData" + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString()
                 + DateTime.Now.Minute.ToString() + ".csv";
            try
            {
                using (CsvFileWriter writer = new CsvFileWriter(fileName))
                {
                    int i = 0;
                    foreach (NavigationData navData in navDataOverTime)
                    {
                        if (navData != null)
                        {
                            CsvRow row = new CsvRow();
                            row.Add(navData.Roll.ToString());
                            row.Add(navData.Pitch.ToString());
                            row.Add(navData.Yaw.ToString());
                            row.Add(navData.Altitude.ToString());
                            row.Add(navData.Velocity.X.ToString());
                            row.Add(navData.Velocity.Y.ToString());
                            row.Add(navData.Velocity.Z.ToString());
                            row.Add(timeOverTime[i].ToString()); 
                            row.Add(cordOverTime[i].x.ToString());
                            row.Add(cordOverTime[i].y.ToString());
                            row.Add(cordOverTime[i].z.ToString());
                            row.Add(navData.Magneto.Rectified.X.ToString());
                            row.Add(navData.Magneto.Rectified.Y.ToString());
                            row.Add(navData.Magneto.Rectified.Z.ToString());
                            row.Add(_northDeg.ToString());
                            writer.WriteRow(row);
                            if (i < timeOverTime.Count)
                            {
                                i++;
                            }
                        }
                    }

                    // MessageBox.Show("data saved");
                }
                #region save orders that were sent to the drone


                /*
                // Saves all the  doring the flight
                string fileNameOrders = "navData" + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString()
                 + DateTime.Now.Minute.ToString() + "orders" + ".csv";

                if (xBoxHelper.allNavOrders != null && xBoxHelper.allNavOrders.Count > 0)
                {
                    using (CsvFileWriter writer = new CsvFileWriter(fileNameOrders))
                    {
                        foreach (navOrder navData in xBoxHelper.allNavOrders)
                        {
                            if (navData != null)
                            {
                                CsvRow row = new CsvRow();
                                row.Add(navData.time);
                                row.Add(navData.orders[0].ToString());
                                row.Add(navData.orders[1].ToString());
                                row.Add(navData.orders[2].ToString());
                                row.Add(navData.orders[3].ToString());
                                writer.WriteRow(row);
                            }
                        }

                        // MessageBox.Show("data saved");
                    }
                }*/
                #endregion
            }
            catch (Exception e)
            {

            }
#endif
            }
        }
        public void OnNavigationDataAcquired(NavigationData data)
        {
            float time_diff = 0;
            if (_isRacing)
            {
                time_diff = (DateTime.Now.Ticks - _prev_tick)* TICKS_TO_SEC;
                _prev_tick = DateTime.Now.Ticks;
                _roll = data.Roll;
                _pitch = data.Pitch;
                _yaw = data.Yaw - _startingYaw;
                //   Console.WriteLine(data.Yaw);
                //  Console.WriteLine(_startingYaw.ToString());
                //  Console.WriteLine(yaw.ToString());
                if (count > 100)
                {
    //                Vector_3 magnetometer = new Vector_3(data.Magneto.Rectified.X, data.Magneto.Rectified.Y, data.Magneto.Rectified.Z);
                   // DCM magnetoDCM = new DCM(_roll, _pitch, 0);
                   DCM magnetoDCM = new DCM(0, 0, 0);
                    // Vector_3 magnetometer_reltiveTo_earth = magnetoDCM.ToEarth(magnetometer);
                    sum.x /= (float)count;
                    sum.y /= (float)count;
                    sum.z /= (float)count;
                   // sum.x += data.Magneto.Offset.X;
                   // sum.y += data.Magneto.Offset.Y;
                    Vector_3 magnetometer_reltiveTo_earth = magnetoDCM.ToEarth(sum);
                    sum = new Vector_3(0, 0, 0);
                    count = 0;
                    //x(1).*Data{i}(:,1)+x(2).*Data{i}(:,2)+x(3)
                    double xAfterClib = _magClib[0] * magnetometer_reltiveTo_earth.x + _magClib[1] * magnetometer_reltiveTo_earth.y + _magClib[2];
                    //x(4).*Data{i}(:,1)+x(5).*Data{i}(:,2)+x(6)
                    double yAfterClib = _magClib[3] * magnetometer_reltiveTo_earth.x + _magClib[4] * magnetometer_reltiveTo_earth.y + _magClib[5];
                    /*   if (yAfterClib > 0)
                       {
                           _north = (Math.PI / 2) - Math.Atan(xAfterClib / yAfterClib);
                       }
                       else if (magnetometer_reltiveTo_earth.y < 0)
                       {
                           _north = ((3 * Math.PI) / 2) - Math.Atan(xAfterClib / yAfterClib);
                       }
                       else if (xAfterClib < 0)
                       {
                           _north = Math.PI;
                       }
                       else
                       {
                           _north = 0;
                       }*/
                    _north = Math.Atan2(xAfterClib, yAfterClib) + Math.PI;
                    _northDeg = (180 / Math.PI) * _north; 
                }
                else
                {
                    sum.x += data.Magneto.Rectified.X;
                    sum.y += data.Magneto.Rectified.Y;
                    sum.z += data.Magneto.Rectified.Z;
                    count++;
                }
                DCM dcm = new DCM(_yaw);
                Vector_3 velociy = new Vector_3(data.Velocity.X, data.Velocity.Y, data.Velocity.Z);
                Vector_3 velociy_reltiveTo_earth = dcm.ToEarth(velociy);
                _x_cord = _x_cord + ((float)velociy_reltiveTo_earth.x * time_diff);
                _y_cord = _y_cord + ((float)velociy_reltiveTo_earth.y * time_diff);
                _z_cord = data.Altitude;
#if RECORD
                navDataOverTime.Add(data);
                timeOverTime.Add(time_diff);
                cordOverTime.Add(new Vector_3(_x_cord, _y_cord, _z_cord));
#endif

            }
            else
            {
                _prev_tick = DateTime.Now.Ticks;
                _startingYaw = data.Yaw;
            }

        }

        public void OnVideoPacketDecoded(VideoFrame frame)
        {

        }

        public double GetYawInDegrees()
        {
            return _yaw * (180 / Math.PI);
        }
    }
}
