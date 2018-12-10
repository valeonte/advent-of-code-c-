using System;
using System.Collections.Generic;
using System.Linq;

namespace MyAdventOfCodeSolution.Christmas2017
{
    class Day3 : Day
    {
        public override dynamic Answer1()
        {
            return GetManhattanDistance(368078);
        }

        static int GetManhattanDistance(int input)
        {
            var sideSize = (int) Math.Ceiling(Math.Sqrt(input));
            if (sideSize % 2 == 0) sideSize++;
            var lowerRightCorner = sideSize * sideSize;

            var squareRadius = sideSize / 2; // sidesize will be odd always so this will be right
            var middles = Enumerable.Range(0, 4).Select(i => lowerRightCorner - squareRadius - i * (sideSize - 1)).ToArray();

            var distanceFromMiddle = middles.Select(m => Math.Abs(input - m)).Min();

            var manhattan = distanceFromMiddle + squareRadius;
            return manhattan;
        }

        class Point : IEquatable<Point>
        {
            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }

            public int X { get; }
            public int Y { get; }

            public Point GetNext()
            {
                if (X == 0 && Y == 0 || Y < 0 && X >= Y && X < -Y + 1) return new Point(X + 1, Y);
                if (X < 0 && Y >= X && Y <= -X) return new Point(X, Y - 1);
                if (Y > 0 && X <= Y && X > -Y) return new Point(X - 1, Y);
                return new Point(X, Y + 1);
            }

            // ReSharper disable once UnusedMember.Local
            public int ManhattanDistance => Math.Abs(X) + Math.Abs(Y);

            public override bool Equals(object obj)
            {
                if (obj is null) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == GetType() && Equals((Point) obj);
            }

            public bool Equals(Point other)
            {
                return X == other.X && Y == other.Y;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (X * 397) ^ Y;
                }
            }

            public override string ToString()
            {
                return $"{X}, {Y}";
            }
        }

        public override dynamic Answer2()
        {
            var cur = new Point(0, 0);
            var points = new Dictionary<Point, int>
            {
                {cur, 1}
            };

            int GetSumForPoint(Point point)
            {
                var ret = 0;
                for (var x = point.X-1; x <= point.X+1; x++)
                for (var y = point.Y - 1; y <= point.Y + 1; y++)
                {
                    if (points.TryGetValue(new Point(x, y), out var existing)) ret += existing;
                }

                return ret;
            }

            var curValue = 1;
            while (curValue < 368078)
            {
                cur = cur.GetNext();
                curValue = GetSumForPoint(cur);
                points.Add(cur, curValue);
            }
            
            return curValue;
        }
    }
}