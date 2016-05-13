using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace AR.Drone.WinApp
{
    public class PaintingHelper
    {
        bool _isGateSeeable;
        Graphics _graphics;

        float gateFullSize = 360;

        int sumGates = 0;
        int currentGate = 0;

        int gateDistanceToShow = 300;

        List<Point> pointsLeft;
        List<Point> pointsRight;

        const int _snakeSize = 2;

        Pen _coursePen = new Pen(Color.Black, 4);
        Pen _squarePen = new Pen(Color.Red, 8);
        Pen _snakePen = new Pen(Color.Green, _snakeSize);
        Pen _gatePen = new Pen(Color.Gold, 2);

        MapConfiguration _mapConf;

        Rectangle _rect;

        int _startingPointX, _startingPointY;

        public Pen SnakePen
        {
            get
            {
                return _snakePen;
            }

            set
            {
                _snakePen = value;
            }
        }

        public bool IsGateSeeable
        {
            get
            {
                return _isGateSeeable;
            }

            set
            {
                _isGateSeeable = value;
            }
        }

        /// <summary>
        /// C'tor
        /// </summary>
        /// <param name="paintType">To choose diffrent types of tracks</param>
        /// <param name="startingPointX">X coordinate for the location of the painting</param>
        /// <param name="startingPointY">Y coordinate for the location of the painting</param>
        /// <param name="graphics">The screen grahics</param>
        public PaintingHelper(MapConfiguration mapConf, Graphics graphics)
        {
            _graphics = graphics;
            _startingPointX = mapConf.StartingPointX;
            _startingPointY = mapConf.StartingPointY;

            pointsLeft = mapConf.PointsLeft;

            pointsRight = mapConf.PointsRight;

            _mapConf = mapConf;

            _rect = new Rectangle();
            _isGateSeeable = false;

            sumGates = _mapConf.Gates.Count;
        }

        public void DrawRectangle()
        {
            System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle(
               _startingPointX, _startingPointY, 300, 300);

            _graphics.DrawRectangle(_squarePen, rectangle);
        }

        public void DrawTrack()
        {
            _graphics.DrawLines(_coursePen, pointsLeft.ToArray());
            _graphics.DrawLines(_coursePen, pointsRight.ToArray());
        }

        public void DrawPoint(float x, float y)
        {
            _graphics.DrawEllipse(_snakePen, _startingPointX + _mapConf.SnakeShiftingX + x * _mapConf.SnakeMuliplier,
                _startingPointY + _mapConf.SnakeShiftingY + y * _mapConf.SnakeMuliplier,
                _snakeSize, _snakeSize);
        }//

        public void CleanMap(MainForm mainForm)
        {
            mainForm.Invalidate(new Rectangle(_startingPointX, _startingPointY, 400, 400));
        }

        public void DrawRectangleOnVideo(Bitmap _frameBitmap)
        {
            Graphics gr = Graphics.FromImage(_frameBitmap);
            gr.DrawRectangle(_gatePen, _rect);
        }

        /// <summary>
        /// Checks if there is relevant rectangle to paint, and if so decides the size and location of the recatangle
        /// </summary>
        /// <param name="x_cord"></param>
        /// <param name="y_cord"></param>
        public void ChangeVideoRectangleSize(float x_cord, float y_cord)
        {
            if (sumGates > currentGate)
            {
                float distance;
                int size = 0;
                float squareXLocation = 0;
                Square gate = _mapConf.Gates[currentGate];

                _isGateSeeable = true;

                // checks if the gate is vertical or horizontal
                if (gate.FirstCorner.X - gate.SecondCorner.X == 0)
                {
                    distance = gate.FirstCorner.X - (x_cord * _mapConf.SnakeMuliplier) - _startingPointX - _mapConf.SnakeShiftingX;

                    // distance too far to show rectangle
                    if (distance > gateDistanceToShow - 1)
                    {
                        _isGateSeeable = false;
                    }
                    // the quad passed the gate, moving to next gate
                    else if (distance < 0)
                    {
                        currentGate++;
                    }
                    // need to paint the gate, calculating the size and location
                    else
                    {
                        size = (int)(gateFullSize / (gateDistanceToShow / (gateDistanceToShow - Math.Abs(distance))));
                        squareXLocation = (gate.FirstCorner.Y - gate.SecondCorner.Y) / 2 - (y_cord * _mapConf.SnakeMuliplier);
                        _rect = new Rectangle(320 - size / 2 + (int)squareXLocation, 180 - size / 2, size, size);
                    }
                }
                else
                {
                    distance = Math.Abs(gate.FirstCorner.Y - (y_cord * _mapConf.SnakeMuliplier) - _startingPointY - _mapConf.SnakeShiftingY);

                    // distance too far to show rectangle
                    if (distance > gateDistanceToShow - 1)
                    {
                        _isGateSeeable = false;
                    }
                    // the quad passed the gate, moving to next gate
                    else if (distance < 0)
                    {
                        currentGate++;
                    }
                    // need to paint the gate, calculating the size and location
                    else
                    {
                        size = (int)(gateFullSize / (gateDistanceToShow / (gateDistanceToShow - Math.Abs(distance))));
                        squareXLocation = (gate.FirstCorner.X - gate.SecondCorner.X) / 2 - (x_cord * _mapConf.SnakeMuliplier);
                        _rect = new Rectangle(320 - size / 2 + (int)squareXLocation, 180 - size / 2, size, size);
                    }
                } 
            }
            // no gates left to see
            else
            {
                _isGateSeeable = false;
            }
        }
    }
}
