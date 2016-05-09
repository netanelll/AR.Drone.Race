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

        /// <summary>
        /// C'tor
        /// </summary>
        /// <param name="paintType">To choose diffrent types of tracks</param>
        /// <param name="startingPointX">X coordinate for the location of the painting</param>
        /// <param name="startingPointY">Y coordinate for the location of the painting</param>
        /// <param name="graphics">The screen grahics</param>
        public PaintingHelper(int paintType, int startingPointX, int startingPointY, Graphics graphics)
        {
            _graphics = graphics;
            _startingPointX = startingPointX;
            _startingPointY = startingPointY;

            switch (paintType)
            {
                case 1:
                    pointsLeft = new List<Point>
                    {
                     new Point(_startingPointX + 20, _startingPointY + 280),
                     new Point(_startingPointX + 20, _startingPointY + 20),
                     new Point(_startingPointX + 280, _startingPointY + 20),
                     new Point(_startingPointX + 280, _startingPointY + 280),
                     new Point(_startingPointX + 20, _startingPointY + 280)
                    };

                    pointsRight = new List<Point>
                    {
                     new Point(_startingPointX + 40, _startingPointY + 260),
                     new Point(_startingPointX + 40, _startingPointY + 40),
                     new Point(_startingPointX + 260, _startingPointY + 40),
                     new Point(_startingPointX + 260, _startingPointY + 260),
                     new Point(_startingPointX + 40, _startingPointY + 260)
                    };
                    break;
                default:
                    pointsLeft = new List<Point>();
                    pointsRight = new List<Point>();
                    break;
            }
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
            _graphics.DrawEllipse(_snakePen, _startingPointX + _snakeShifting + x * 10,
                _startingPointY + _snakeShifting + y * 10,
                _snakeSize, _snakeSize);
        }
    }
}
