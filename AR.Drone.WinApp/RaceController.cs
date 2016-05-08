
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
        private long start_ticks, end_ticks, prev_tick;
        private bool isRacing;
        float x_cord, y_cord, z_cord, roll, pitch, yaw;
        const float TICKS_TO_SEC = 0.0000001f; // cov from 100 nano sec to sec

        #region properties

        public float X_cord
        {
            get
            {
                return x_cord;
            }

            set
            {
                x_cord = value;
            }
        }

        public float Y_cord
        {
            get
            {
                return y_cord;
            }

            set
            {
                y_cord = value;
            }
        }

        public float Z_cord
        {
            get
            {
                return z_cord;
            }

            set
            {
                z_cord = value;
            }
        }
#endregion

        public RaceController()
        {
            x_cord = 0;
            y_cord = 0;
            z_cord = 0;
            roll = 0;
            pitch = 0;
            yaw = 0;
            start_ticks = 0;
            end_ticks = 0;
            isRacing = false;
        }

        public void startRace()
        {
            x_cord = 0;
            y_cord = 0;
            z_cord = 0;
            roll = 0;
            pitch = 0;
            yaw = 0;
            start_ticks = 0;
            end_ticks = 0;
            isRacing = true;
            start_ticks = DateTime.Now.Ticks;
#if RECORD
            navDataOverTime = new List<NavigationData>();
            timeOverTime = new List<float>();
            cordOverTime = new List<Vector_3>();
#endif
        }

        public void endRace()
        {
            if (isRacing)
            {


                isRacing = false;
                end_ticks = DateTime.Now.Ticks;
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
            if (isRacing)
            {
                time_diff = (DateTime.Now.Ticks - prev_tick)* TICKS_TO_SEC;
                prev_tick = DateTime.Now.Ticks;
                roll = data.Roll;
                pitch = data.Pitch;
                yaw = data.Yaw;

                DCM dcm = new DCM(roll, pitch, yaw);
                Vector_3 velociy = new Vector_3(data.Velocity.X, data.Velocity.Y, data.Velocity.Z);
                Vector_3 velociy_reltiveTo_earth = dcm.ToEarth(velociy);
                x_cord = x_cord + ((float)velociy_reltiveTo_earth.x * time_diff);
                y_cord = y_cord + ((float)velociy_reltiveTo_earth.y * time_diff);
                z_cord = data.Altitude;
#if RECORD
                navDataOverTime.Add(data);
                timeOverTime.Add(time_diff);
                cordOverTime.Add(new Vector_3(x_cord, y_cord, z_cord));
#endif

            }
            else
            {
                prev_tick = DateTime.Now.Ticks;
            }

        }

        public void OnVideoPacketDecoded(VideoFrame frame)
        {

        }
    }
}
