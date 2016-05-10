using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace AR.Drone.WinApp
{
    public class PaintingHelper
    {
        Graphics _graphics;

        List<Point> pointsLeft;
        List<Point> pointsRight;

        const int _snakeSize = 2;
        const int _snakeShifting = 200;

        Pen _coursePen = new Pen(Color.Black, 4);
        Pen _squarePen = new Pen(Color.Red, 8);
        Pen _snakePen = new Pen(Color.Green, _snakeSize);

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
            _graphics.DrawEllipse(_snakePen, _startingPointX + _snakeShifting + x * 20,
                _startingPointY + _snakeShifting + y * 20,
                _snakeSize, _snakeSize);
        }

        internal void CleanMap(MainForm mainForm)
        {
            mainForm.Invalidate(new Rectangle(_startingPointX, _startingPointY, 400, 400));
        }
    }
}
