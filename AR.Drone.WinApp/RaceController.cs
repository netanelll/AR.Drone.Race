using AR.Drone.Data.Navigation;
using DCMAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AR.Drone.WinApp
{
    class RaceController
    {
        private List<NavigationData> navDataOverTime;
        private List<long> timeOverTime;
        private long start_ticks, end_ticks, prev_tick;
        private bool isFlying;
        float x_cord, y_cord, z_cord;

        #region properties
        public List<NavigationData> NavDataOverTime
        {
            get
            {
                return navDataOverTime;
            }

            set
            {
                navDataOverTime = value;
            }
        }

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

        public List<long> TimeOverTime
        {
            get
            {
                return timeOverTime;
            }

            set
            {
                timeOverTime = value;
            }
        }
        #endregion

        public RaceController()
        {
            x_cord = 0;
            y_cord = 0;
            z_cord = 0;
            start_ticks = 0;
            end_ticks = 0;
            isFlying = false;
        }

        public void OnNavigationDataAcquired(NavigationData data)
        {
            if (isFlying)
            {
                navDataOverTime.Add(data);
                timeOverTime.Add(DateTime.Now.Ticks - start_ticks);
            }

        }

        public void StartRecording()
        {
            navDataOverTime = new List<NavigationData>();
            timeOverTime = new List<long>();
            isFlying = true;
            start_ticks = DateTime.Now.Ticks;
        }

        public void stopRecoreAndSave()
        {
            if (!isFlying)
            {
                return;
            }
            isFlying = false;
            end_ticks = DateTime.Now.Ticks;
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
                            row.Add((timeOverTime[i] * (0.0000001)).ToString()); // cov from 100 nano sec to sec
                            writer.WriteRow(row);
                            if (i < timeOverTime.Count)
                            {
                                i++;
                            }
                        }
                    }

                    // MessageBox.Show("data saved");
                }


                // Saves all the orders that were sent to the drone doring the flight
                string fileNameOrders = "navData" + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString()
                 + DateTime.Now.Minute.ToString() + "orders" + ".csv";

               /* if (xBoxHelper.allNavOrders != null && xBoxHelper.allNavOrders.Count > 0)
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
            }
            catch (Exception e)
            {

            }
        }
        //public void OnNavigationDataAcquired(NavigationData data)
        //{
        //    long time_diff;
        //    if (isFlying)
        //    {
        //        navDataOverTime.Add(data);
        //        time_diff = DateTime.Now.Ticks - start_ticks;
        //        timeOverTime.Add(time_diff);
        //        // x-> roll ; y -> Pitch ; z -> yaw
        //        DCM E = new DCM(new Vector(0.0F, 0.0F, 0.0F));

        //        x_cord = x_cord + data.Velocity.X * time_diff;
        //        y_cord = y_cord + data.Velocity.X * time_diff;
        //        z_cord = data.Altitude;

        //    }
        //    else
        //    {
        //        prev_tick = DateTime.Now.Ticks;
        //    }

        //}

    }
}
