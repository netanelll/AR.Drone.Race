
//#define RECORD


using AR.Drone.Data.Navigation;
using AR.Drone.Video;
using DCMAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

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
        private float _previousYaw; // saving the first yaw response to get the quad direction
        //private const int NUMBER_OF_TAGS = 10;
        private const double TO_X_PICXELS = 640f / 1000f, TO_Y_PICXELS = 320f / 1000f; // number of picxels in axis divided by max x and y value (1000)
        private static readonly double PICXELS_TO_METERS_FACTOR = (2 * Math.Tan(Math.PI / 4)) / 734.30239;
        //private static readonly float[,] _tagLocations = new float[NUMBER_OF_TAGS, 2] { { 1, 0 } , {1.5f,0 } , {5,0 }, {0,0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } }; // x,y coordinates of tags
        private int _currentTag = 0;
        bool _oneTagInSight = false;
        private MapConfiguration _mapConf;
        private bool _isSupposeToTurn = false;
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

        public bool IsSupposeToTurn
        {
            get
            {
                return _isSupposeToTurn;
            }

            set
            {
                _isSupposeToTurn = value;
            }
        }
        #endregion

        public RaceController(MapConfiguration _mapConf)
        {
            this._mapConf = _mapConf;

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
            Debug.WriteLine("race started","raceDebug");
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
                _z_cord = 1.5f; // delete TODO
                time_diff = (DateTime.Now.Ticks - _prev_tick) * TICKS_TO_SEC;
                _prev_tick = DateTime.Now.Ticks;

                uint numberOfDetectedTags = data.vision_detect.nb_detected;

                if (data.vision_detect.nb_detected > 0)
                {
                    for (int i = 0; i < numberOfDetectedTags; i++)
                    {
                        TagData tagData = new TagData();

                        double xPix, yPix;

                        fixed (uint* tmp = data.vision_detect.xc)
                        {
                            yPix = tmp[i] * TO_X_PICXELS;
                        }
                        fixed (uint* tmp = data.vision_detect.yc)
                        {
                            xPix = tmp[i] * TO_Y_PICXELS;
                        }
                        fixed (float* tmp = data.vision_detect.orientation_angle)
                        {
                            tagData.Yaw = tmp[i] * (float)(Math.PI / 180.0f);
                        }

                        tagData.X = (float)((xPix - 180f) * _z_cord * PICXELS_TO_METERS_FACTOR);
                        tagData.Y = (float)((320f - yPix) * _z_cord * PICXELS_TO_METERS_FACTOR);

                        PointF tagInMeters_reltiveTo_map = Rotate2DAroundPoint(new PointF(tagData.X, tagData.Y), new PointF(X_cord, Y_cord), _yaw);

                        CalculateLocationByTags(tagInMeters_reltiveTo_map, new PointF(tagData.X, tagData.Y));

                        //Vector_3 tagInMeters_reltiveTo_map = Rotate2DAroundPoint(tagInMeters, new Vector_3(_tagLocations[_currentTag, 0], _tagLocations[_currentTag, 1], 0), _yaw);
                        // _x_cord = _tagLocations[_currentTag, 0] + (float)tagInMeters_reltiveTo_map.x;
                        // _y_cord = _tagLocations[_currentTag, 1] + (float)tagInMeters_reltiveTo_map.y;
                        
                    }



                }
                else
                {
                    _oneTagInSight = false;
                    _roll = data.Roll;
                    _pitch = data.Pitch;
                    // removes unknow jums in the yaw param
                    if (_previousYaw - data.Time < 0.2 || _isSupposeToTurn)
                    {
                        _yaw += _previousYaw - data.Yaw;
                    }
                    _previousYaw = data.Yaw;
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
                _previousYaw = data.Yaw;
            }

        }

        private void CalculateLocationByTags(PointF tagInMeters_reltiveTo_map, PointF tagInMeters)
        {
            foreach (PointF tag in _mapConf.TagLocations)
            {
                float dist = DistanceBetween2Pionts(tagInMeters_reltiveTo_map.X, tagInMeters_reltiveTo_map.Y, tag.X, tag.Y);
                if (dist < 0.2)
                {
                    PointF realTagInMeters_reltiveTo_map = Rotate2DAroundPoint(tagInMeters, new PointF(tag.X, tag.Y), _yaw);
                    _x_cord = tag.X + (float)realTagInMeters_reltiveTo_map.X;
                    _y_cord = tag.Y + (float)realTagInMeters_reltiveTo_map.Y;
                    Debug.WriteLine("x,y: {0}/{1}/{2}/{3}/{4}", X_cord, Y_cord, tag.X, tag.Y, dist); 
                    return;
                }
            }

            //foreach (PointF tag in _mapConf.TagLocations)
            //{
            //    float dist = DistanceBetween2Pionts(tagInMeters_reltiveTo_map.X, tagInMeters_reltiveTo_map.Y, tag.X, tag.Y);
            //    if (dist < 0.4)
            //    {
            //        PointF realTagInMeters_reltiveTo_map = Rotate2DAroundPoint(new PointF(X_cord, Y_cord), new PointF(tag.X, tag.Y), _yaw);
            //        _x_cord = tag.X + (float)realTagInMeters_reltiveTo_map.X;
            //        _y_cord = tag.Y + (float)realTagInMeters_reltiveTo_map.Y;
            //        Debug.WriteLine("x,y: {0}/{1}/{2}/{3}/{4}", X_cord, Y_cord, tag.X, tag.Y, dist);
            //        return;
            //    }
            //}
        }

        public void OnVideoPacketDecoded(VideoFrame frame)
        {

        }

        public double GetYawInDegrees()
        {
            return _yaw * (180 / Math.PI);
        }

        private PointF Rotate2DAroundPoint(PointF p, PointF cp, float angleOfRotation)
        {
            return new PointF((float)(Math.Cos(angleOfRotation) *(p.X - cp.X) - Math.Sin(angleOfRotation) * (p.Y - cp.Y) + cp.X),
                (float)(Math.Sin(angleOfRotation) * (p.X - cp.X) + Math.Cos(angleOfRotation) * (p.Y - cp.Y) + cp.Y));
        }

        private float DistanceBetween2Pionts(float x1, float y1, float x2, float y2)
        {
            return ((float)Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2)));
        }
    }

    public class TagData
    {
        float _x; // in Meters
        float _y; // in Meters
        float _yaw;

        public float X
        {
            get
            {
                return _x;
            }

            set
            {
                _x = value;
            }
        }

        public float Y
        {
            get
            {
                return _y;
            }

            set
            {
                _y = value;
            }
        }

        public float Yaw
        {
            get
            {
                return _yaw;
            }

            set
            {
                _yaw = value;
            }
        }
    }
}
