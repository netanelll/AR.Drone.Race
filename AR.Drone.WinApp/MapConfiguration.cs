using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace AR.Drone.WinApp
{
    public class MapConfiguration
    {
        List<Point> pointsLeft;
        List<Point> pointsRight;

        List<Square> gates;

        List<PointF> _tagLocations;

        List<Square> mapSquares;
        //List<Point> middleLine;

        int _snakeShiftingX = 50;
        int _snakeShiftingY = 250;
        int _snakeMuliplier = 50;

        //float dx;
        //float dy;

        int _startingPointX, _startingPointY;

        public MapConfiguration(int map, int startingPointX, int startingPointY)
        {
            _startingPointX = startingPointX;
            _startingPointY = startingPointY;

            switch (map)
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
                     new Point(_startingPointX + 80, _startingPointY + 220),
                     new Point(_startingPointX + 80, _startingPointY + 80),
                     new Point(_startingPointX + 220, _startingPointY + 80),
                     new Point(_startingPointX + 220, _startingPointY + 220),
                     new Point(_startingPointX + 80, _startingPointY + 220)
                    };

                    //middleLine = new List<Point>
                    //{
                    // new Point(_startingPointX + 30, _startingPointY + 270),
                    // new Point(_startingPointX + 30, _startingPointY + 30),
                    // new Point(_startingPointX + 270, _startingPointY + 30),
                    // new Point(_startingPointX + 270, _startingPointY + 270),
                    // new Point(_startingPointX + 30, _startingPointY + 270)
                    //};

                    //dx = p2.X - p1.X;
                    //dy = p2.Y - p1.Y;

                    //mapSquares = new List<Square>
                    //{
                    // new Square(new Point(_startingPointX + 20, _startingPointY + 20),
                    // new Point(_startingPointX + 280, _startingPointY + 280))
                    //};

                    // the areas in which the quad is allowed to be
                    mapSquares = new List<Square>
                    {
                     new Square(new Point(_startingPointX + 20, _startingPointY + 20),
                                new Point(_startingPointX + 280, _startingPointY + 80)),
                     new Square(new Point(_startingPointX + 20, _startingPointY + 220),
                                new Point(_startingPointX + 280, _startingPointY + 280)),
                     new Square(new Point(_startingPointX + 20, _startingPointY + 80),
                                new Point(_startingPointX + 80, _startingPointY + 220)),
                     new Square(new Point(_startingPointX + 220, _startingPointY + 80),
                                new Point(_startingPointX + 280, _startingPointY + 220))
                    };
                    //
                    // the gates for the quad to pass throw
                    gates = new List<Square>
                    {
                        new Square(new Point(_startingPointX + 150, _startingPointY + 220),
                                new Point(_startingPointX + 150, _startingPointY + 280),
                                0, 0, 1),
                     new Square(new Point(_startingPointX + 220, _startingPointY + 150),
                                new Point(_startingPointX + 280, _startingPointY + 150),
                                270, 270, -1),
                     new Square(new Point(_startingPointX + 150, _startingPointY + 20),
                                new Point(_startingPointX + 150, _startingPointY + 80),
                                180, 180, -1),
                     new Square(new Point(_startingPointX + 20, _startingPointY + 150),
                                new Point(_startingPointX + 80, _startingPointY + 150),
                                90, 90, 1)
                    };

                    // the locations of the tags
                    _tagLocations = new List<PointF>
                    {
                     new PointF(0,0),
                     new PointF(0.4f,0),
                     new PointF(0.8f,0),
                     new PointF(1.2f,0),
                     new PointF(1.6f,0)
                    };

                    break;
                default:
                    pointsRight = new List<Point>();
                    PointsLeft = new List<Point>();
                    break;
            }
        }

        public bool CheckQuadInSquares(float x, float y)
        {
            x = _startingPointX + SnakeShiftingX + x * SnakeMuliplier;
            y = _startingPointY + SnakeShiftingY + y * SnakeMuliplier;

            foreach (Square square in mapSquares)
            {
                if (x > square.FirstCorner.X && x < square.SecondCorner.X &&
                    y > square.FirstCorner.Y && y < square.SecondCorner.Y)
                {
                    return true;
                }
            }
            return false;
        }

        public List<Point> PointsLeft
        {
            get
            {
                return pointsLeft;
            }

            set
            {
                pointsLeft = value;
            }
        }

        public List<Point> PointsRight
        {
            get
            {
                return pointsRight;
            }

            set
            {
                pointsRight = value;
            }
        }

        public int StartingPointX
        {
            get
            {
                return _startingPointX;
            }

            set
            {
                _startingPointX = value;
            }
        }

        public int StartingPointY
        {
            get
            {
                return _startingPointY;
            }

            set
            {
                _startingPointY = value;
            }
        }

        public int SnakeShiftingX
        {
            get
            {
                return _snakeShiftingX;
            }

            set
            {
                _snakeShiftingX = value;
            }
        }

        public int SnakeShiftingY
        {
            get
            {
                return _snakeShiftingY;
            }

            set
            {
                _snakeShiftingY = value;
            }
        }

        public int SnakeMuliplier
        {
            get
            {
                return _snakeMuliplier;
            }

            set
            {
                _snakeMuliplier = value;
            }
        }

        public List<Square> Gates
        {
            get
            {
                return gates;
            }

            set
            {
                gates = value;
            }
        }

        public List<PointF> TagLocations
        {
            get
            {
                return _tagLocations;
            }

            set
            {
                _tagLocations = value;
            }
        }
    }

    public class Square
    {
        Point _firstCorner;
        Point _secondCorner;

        int _turnDegree;
        int _realDegree;

        int _multiplier;

        public Square(Point firstCorner, Point secondCorner)
        {
            _firstCorner = firstCorner;
            _secondCorner = secondCorner;
        }

        public Square(Point firstCorner, Point secondCorner, int turnDegree, int realDegree, int multiplier)
        {
            _firstCorner = firstCorner;
            _secondCorner = secondCorner;
            _turnDegree = turnDegree;
            _realDegree = realDegree;
            _multiplier = multiplier;
        }

        public Point FirstCorner
        {
            get
            {
                return _firstCorner;
            }

            set
            {
                _firstCorner = value;
            }
        }

        public Point SecondCorner
        {
            get
            {
                return _secondCorner;
            }

            set
            {
                _secondCorner = value;
            }
        }

        public int RealDegree
        {
            get
            {
                return _realDegree;
            }

            set
            {
                _realDegree = value;
            }
        }

        public int TurnDegree
        {
            get
            {
                return _turnDegree;
            }

            set
            {
                _turnDegree = value;
            }
        }

        public int Multiplier
        {
            get
            {
                return _multiplier;
            }

            set
            {
                _multiplier = value;
            }
        }
    }

    public class PointsF
    {
        float _x, _y;
        public PointsF(float x, float y)
        {
            _x = x;
            _y = y;
        }

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
    }

}
