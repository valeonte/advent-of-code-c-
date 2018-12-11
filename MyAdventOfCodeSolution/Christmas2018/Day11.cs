namespace MyAdventOfCodeSolution.Christmas2018
{
    class Day11 : Day
    {
        class Grid
        {
            readonly int _dimension;
            readonly int _serial;
            readonly int[,] _grid;

            public Grid(int dimension, int serial)
            {
                _dimension = dimension;
                _serial = serial;
                _grid = new int[dimension, dimension];
            }

            int GetCellPowerLevel(int x, int y)
            {
                var rackId = x + 10;
                var powerLevel = rackId * y + _serial;
                powerLevel *= rackId;
                powerLevel = (powerLevel % 1000) / 100;
                return powerLevel - 5;
            }

            public void PopulatePowerLevels()
            {
                for (var i = 0; i < _dimension; i++)
                for (var j = 0; j < _dimension; j++)
                {
                    _grid[i, j] = GetCellPowerLevel(i + 1, j + 1);
                }
            }

            int SumRectangle(int x, int y, int dx, int dy)
            {
                var sum = 0;
                for (var i = 0; i < dx; i++)
                for (var j = 0; j < dy; j++)
                {
                    sum += _grid[x + i, y + j];
                }

                return sum;
            }

            public (int X, int Y) GetMaxTotalPowerSquare(int squareDimension, out int maxSum)
            {
                var maxSquareDim = _dimension - squareDimension;

                maxSum = int.MinValue;
                var ret = (-1, -1);

                for (var x = 0; x < maxSquareDim; x++)
                {
                    var runningSum = SumRectangle(x, 0, squareDimension, squareDimension - 1);
                    for (var y = 0; y < maxSquareDim; y++)
                    {
                        runningSum += SumRectangle(x, y + squareDimension - 1, squareDimension, 1);

                        if (runningSum > maxSum)
                        {
                            maxSum = runningSum;
                            ret = (x + 1, y + 1);
                        }

                        runningSum -= SumRectangle(x, y, squareDimension, 1);
                    }
                }

                return ret;
            }
        }

        public override dynamic Answer1()
        {
            //var grid = new Grid(300, 18);
            //grid.PopulatePowerLevels();
            //var m = grid.GetMaxTotalPowerSquare(3, out var maxSum);

            //var a = new Grid(8).GetCellPowerLevel(3, 5);
            //var b = new Grid(57).GetCellPowerLevel(122, 79);
            //var c = new Grid(39).GetCellPowerLevel(217, 196);
            //var d = new Grid(71).GetCellPowerLevel(101, 153);

            var grid = new Grid(300, 6548);
            grid.PopulatePowerLevels();
            var max = grid.GetMaxTotalPowerSquare(3, out var _);

            return max;
        }

        public override dynamic Answer2()
        {
            //var grid = new Grid(300, 18);
            //grid.PopulatePowerLevels();
            //var m = grid.GetMaxTotalPowerSquare(3, out var maxSum);

            //var a = new Grid(8).GetCellPowerLevel(3, 5);
            //var b = new Grid(57).GetCellPowerLevel(122, 79);
            //var c = new Grid(39).GetCellPowerLevel(217, 196);
            //var d = new Grid(71).GetCellPowerLevel(101, 153);

            var grid = new Grid(300, 6548);
            grid.PopulatePowerLevels();

            var bestSize = -1;
            var bestSum = int.MinValue;
            var bestSquare = (-1, -1);

            for (var i = 0; i < 300; i++)
            {
                var square = grid.GetMaxTotalPowerSquare(i + 1, out var maxSum);
                if (maxSum <= bestSum) continue;

                bestSize = i + 1;
                bestSum = maxSum;
                bestSquare = square;
            }

            return $"{bestSquare}, {bestSize}";
        }
    }
}