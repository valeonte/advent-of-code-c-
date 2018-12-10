using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MyAdventOfCodeSolution.Christmas2018
{
    class Day10 : Day
    {
        const string TestInput = @"position=< 9,  1> velocity=< 0,  2>
position=< 7,  0> velocity=<-1,  0>
position=< 3, -2> velocity=<-1,  1>
position=< 6, 10> velocity=<-2, -1>
position=< 2, -4> velocity=< 2,  2>
position=<-6, 10> velocity=< 2, -2>
position=< 1,  8> velocity=< 1, -1>
position=< 1,  7> velocity=< 1,  0>
position=<-3, 11> velocity=< 1, -2>
position=< 7,  6> velocity=<-1, -1>
position=<-2,  3> velocity=< 1,  0>
position=<-4,  3> velocity=< 2,  0>
position=<10, -3> velocity=<-1,  1>
position=< 5, 11> velocity=< 1, -2>
position=< 4,  7> velocity=< 0, -1>
position=< 8, -2> velocity=< 0,  1>
position=<15,  0> velocity=<-2,  0>
position=< 1,  6> velocity=< 1,  0>
position=< 8,  9> velocity=< 0, -1>
position=< 3,  3> velocity=<-1,  1>
position=< 0,  5> velocity=< 0, -1>
position=<-2,  2> velocity=< 2,  0>
position=< 5, -2> velocity=< 1,  2>
position=< 1,  4> velocity=< 2,  1>
position=<-2,  7> velocity=< 2, -2>
position=< 3,  6> velocity=<-1, -1>
position=< 5,  0> velocity=< 1,  0>
position=<-6,  0> velocity=< 2,  0>
position=< 5,  9> velocity=< 1, -2>
position=<14,  7> velocity=<-2,  0>
position=<-3,  6> velocity=< 2, -1>";

        class MovingPoint
        {
            static readonly Regex PointLineParser = new Regex(@"^position=<\s*([\d-]+),\s*([\d-]+)> velocity=<\s*([\d-]+),\s*([\d-]+)>$");

            public MovingPoint(string pointLine)
            {
                var match = PointLineParser.Match(pointLine);
                if (!match.Success) throw new InvalidOperationException($"Invalid point line {pointLine}!");

                PointX = int.Parse(match.Groups[1].Value);
                PointY = int.Parse(match.Groups[2].Value);
                VelX = int.Parse(match.Groups[3].Value);
                VelY = int.Parse(match.Groups[4].Value);
            }

            public int PointX { get; private set; }
            public int PointY { get; private set; }

            int VelX { get; }
            int VelY { get; }

            public void Move(int seconds)
            {
                PointX += seconds * VelX;
                PointY += seconds * VelY;
            }

            public override string ToString()
            {
                return $"Point <{PointX}, {PointY}>, velocity=<{VelX}, {VelY}>";
            }
        }

        class DisplayBoard
        {
            readonly List<MovingPoint> _points = new List<MovingPoint>();

            public void AddPoint(MovingPoint point)
            {
                _points.Add(point);
            }

            void UpdateMaxMin()
            {
                _minX = int.MaxValue;
                _minY = int.MaxValue;
                _maxX = int.MinValue;
                _maxY = int.MinValue;

                foreach (var point in _points)
                {
                    if (point.PointX < _minX) _minX = point.PointX;
                    if (point.PointY < _minY) _minY = point.PointY;

                    if (point.PointX > _maxX) _maxX = point.PointX;
                    if (point.PointY > _maxY) _maxY = point.PointY;
                }
            }

            public void Move(int seconds)
            {
                foreach (var point in _points)
                {
                    point.Move(seconds);
                }
            }

            int _minX = int.MaxValue, _minY = int.MaxValue, _maxX = int.MinValue, _maxY = int.MinValue;

            public IEnumerable<string> Print(int scalingFactor)
            {
                var boardX = (_maxX - _minX) / scalingFactor + 1;
                var boardY = (_maxY - _minY) / scalingFactor + 1;
                var board = new bool[boardX, boardY];

                yield return $"Max: {_maxX}x{_maxY}, Scaling Factor: {scalingFactor}, Array: {boardX}x{boardY}";

                foreach (var point in _points)
                {
                    var x = (point.PointX - _minX)/scalingFactor;
                    var y = (point.PointY - _minY)/scalingFactor;
                    board[x, y] = true;
                }

                for (var y = 0; y < boardY; y++)
                {
                    var ret = new char[boardX];
                    for (var x = 0; x < boardX; x++)
                    {
                        ret[x] = board[x, y] ? '#' : '.';
                    }

                    yield return new string(ret);
                }
            }

            public int CalculateScalingFactor(int maxDisplayableWidth)
            {
                UpdateMaxMin();

                var scalingFactor = 1;
                if (_maxX - _minX > maxDisplayableWidth || _maxY - _minY > maxDisplayableWidth)
                    scalingFactor = Math.Max((_maxX - _minX) / maxDisplayableWidth, (_maxY - _minY) / maxDisplayableWidth);

                return scalingFactor;
            }

            public int CalculateSumOfSides()
            {
                UpdateMaxMin();
                return _maxX - _minX + _maxY - _minY;
            }
        }

        void Experiment(DisplayBoard board)
        {
            const int maxDisplayableWidth = 70;
            while (true)
            {
                Console.SetCursorPosition(0, 0);
                for (var i = 0; i <= maxDisplayableWidth + 10; i++)
                {
                    Console.WriteLine(new string(' ', maxDisplayableWidth + 5));
                }

                var scalingFactor = board.CalculateScalingFactor(maxDisplayableWidth);

                Console.SetCursorPosition(0, 0);
                foreach (var boardLine in board.Print(scalingFactor)) Console.WriteLine(boardLine);

                Console.ReadKey();
                board.Move(Math.Max(scalingFactor / 10, 1));
            }
        }

        public override dynamic Answer1()
        {
            var board = GetBoardFromInput();

            FastForwardToMessage(board, out var _);

            var scalingFactor = board.CalculateScalingFactor(70);
            foreach (var line in board.Print(scalingFactor))
            {
                Console.WriteLine(line);
            }

            return "See message above";
        }

        public override dynamic Answer2()
        {
            var board = GetBoardFromInput();

            FastForwardToMessage(board, out var totalSeconds);

            return totalSeconds;
        }

        DisplayBoard GetBoardFromInput()
        {
            //var input = TestInput.Split(Environment.NewLine);
            var input = GetRawInput();

            var board = new DisplayBoard();
            foreach (var pointLine in input)
            {
                board.AddPoint(new MovingPoint(pointLine));
            }

            return board;
        }

        static void FastForwardToMessage(DisplayBoard board, out int totalSeconds)
        {
            int lastSumOfSides;
            totalSeconds = 0;
            var sumOfSides = int.MaxValue;
            do
            {
                lastSumOfSides = sumOfSides;
                board.Move(1);

                sumOfSides = board.CalculateSumOfSides();
                totalSeconds++;
            } while (sumOfSides < lastSumOfSides);

            // revert last step
            board.Move(-1);
            totalSeconds--;
        }
    }
}