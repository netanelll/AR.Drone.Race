
//#define RECORD


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
        private float _startingYaw; // saving the first yaw response to get the quad direction
        private const int NUMBER_OF_TAGS = 3;
        private const double TO_X_PICXELS = 640f / 1000f, TO_Y_PICXELS = 320f / 1000f; // number of picxels in axis divided by max x and y value (1000)
        private static readonly double PICXELS_TO_METERS_FACTOR = (2 * Math.Tan(Math.PI / 4)) / 734.30239;
        private static readonly float[,] _tagLocations = new float[NUMBER_OF_TAGS, 2] { { 1, 0 } , {1.5f,0 } , {5,0 } }; // x,y coordinates of tags
        private int _currentTag = 0;
        bool _oneTagInSight = false;
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
            _currentTag = 0;
            _oneTagInSight = false;
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
        unsafe public void OnNavigationDataAcquired(NavigationData data)
        {
            float time_diff = 0;
            double tag1_x, tag1_y;
            if (_isRacing)
            {
                //  _z_cord = data.Altitude;
                _z_cord = 2;
                time_diff = (DateTime.Now.Ticks - _prev_tick) * TICKS_TO_SEC;
                _prev_tick = DateTime.Now.Ticks;

                if (data.vision_detect.nb_detected == 2)
                {
                    if (_oneTagInSight)
                    {
                        float dictanceToNextTag, dictanceToPreviousTag;
                        int nextTagIndex, previousTagIndex;
                        _oneTagInSight = false;
                        if (_currentTag == 0 )  {
                            nextTagIndex = 1;
                            previousTagIndex = NUMBER_OF_TAGS - 1;
                        }
                        else if (_currentTag == (NUMBER_OF_TAGS - 1))
                        {
                            nextTagIndex = 0;
                            if (NUMBER_OF_TAGS > 1)
                                previousTagIndex = NUMBER_OF_TAGS - 2;
                            else 
                                previousTagIndex = 0;
                        }
                        else
                        {
                            nextTagIndex = _currentTag + 1;
                            previousTagIndex = _currentTag - 1;

                        }
                        dictanceToNextTag = DictenceBtween2Pionts(_x_cord, _y_cord, _tagLocations[nextTagIndex, 0], _tagLocations[nextTagIndex, 1]);
                        dictanceToPreviousTag = DictenceBtween2Pionts(_x_cord, _y_cord, _tagLocations[previousTagIndex, 0], _tagLocations[previousTagIndex, 1]);
                        if (dictanceToNextTag < dictanceToPreviousTag)
                            _currentTag = nextTagIndex;
                        else
                            _currentTag = previousTagIndex;


                    }
                }
                 if (data.vision_detect.nb_detected >= 1)
                {
                    _oneTagInSight = true;
                    fixed (uint* tmp = data.vision_detect.xc) {
                         tag1_y = tmp[0] * TO_X_PICXELS;
                    }
                    fixed (uint* tmp = data.vision_detect.yc)
                    {
                        tag1_x = tmp[0] * TO_Y_PICXELS;
                    }
                    fixed (float* tmp = data.vision_detect.orientation_angle)
                    {
                       _yaw = tmp[0] * (float) (Math.PI / 180.0f);
                    }
                    Vector_3 tagInMeters = new Vector_3(((tag1_x - 180f) * _z_cord * PICXELS_TO_METERS_FACTOR), ((320f - tag1_y) * _z_cord * PICXELS_TO_METERS_FACTOR), 0);
                    Vector_3 tagInMeters_reltiveTo_map = Rotate2DAroundPoint(tagInMeters, new Vector_3(_tagLocations[_currentTag, 0], _tagLocations[_currentTag, 1], 0), _yaw);
                    _x_cord = _tagLocations[_currentTag, 0] + (float)tagInMeters_reltiveTo_map.x;
                    _y_cord = _tagLocations[_currentTag, 1] + (float)tagInMeters_reltiveTo_map.y;


                }
                else
                {
                    _oneTagInSight = false;
                    _roll = data.Roll;
                    _pitch = data.Pitch;
                    _yaw = data.Yaw - _startingYaw;
                    DCM dcm = new DCM(_yaw);
                    Vector_3 velociy = new Vector_3(data.Velocity.X, data.Velocity.Y, data.Velocity.Z);
                    Vector_3 velociy_reltiveTo_map = dcm.ToEarth(velociy);
                    _x_cord = _x_cord + ((float)velociy_reltiveTo_map.x * time_diff);
                    _y_cord = _y_cord + ((float)velociy_reltiveTo_map.y * time_diff);

                }
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

        private Vector_3 Rotate2DAroundPoint(Vector_3 p,Vector_3 cp, float angleOfRotation)
        {
            return new Vector_3(Math.Cos(angleOfRotation) *(p.x - cp.x) - Math.Sin(angleOfRotation) * (p.y - cp.y) + cp.x,
                Math.Sin(angleOfRotation) * (p.x - cp.x) + Math.Cos(angleOfRotation) * (p.y - cp.y) + cp.y, 0);
        }

        private float DictenceBtween2Pionts(float x1, float y1, float x2, float y2)
        {
            return ((float)(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2)));
        }
    }
}
