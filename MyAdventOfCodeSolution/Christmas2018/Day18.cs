using System;
using System.Collections.Generic;

// ReSharper disable UnusedMember.Local

namespace MyAdventOfCodeSolution.Christmas2018
{
    class Day18 : Day
    {
        enum LandType
        {
            Land, Trees, Lumberyard
        }

        int _dimX, _dimY;

        LandType?[,] GetLandTiles(IReadOnlyList<string> input)
        {
            _dimX = input[0].Length;
            _dimY = input.Count;
            var ret = new LandType?[_dimX, _dimY];
            for (var y = 0; y < _dimY; y++)
            {
                var line = input[y];
                for (var x = 0; x < _dimX; x++)
                {
                    if (line[x] == '|')
                        ret[x, y] = LandType.Trees;
                    else if (line[x] == '#')
                        ret[x, y] = LandType.Lumberyard;
                }
            }

            return ret;
        }

        struct RunningCount
        {
            public RunningCount(int trees, int lumberyards)
            {
                Trees = trees;
                Lumberyards = lumberyards;
            }

            public int Trees { get; private set; }
            public int Lumberyards { get; private set; }

            public void AddTile(LandType tile)
            {
                if (tile == LandType.Lumberyard)
                    Lumberyards++;
                else if (tile == LandType.Trees)
                    Trees++;
            }

            public void RemoveTile(LandType tile)
            {
                if (tile == LandType.Lumberyard)
                    Lumberyards--;
                else if (tile == LandType.Trees)
                    Trees--;
            }

            public int ResourceValue() => Trees * Lumberyards;

            public override string ToString()
            {
                return $"Trees: {Trees}, Lumberyards: {Lumberyards}";
            }
        }

        LandType?[,] GetNextMinute(LandType?[,] land, int adjacentRadius)
        {
            void ProcessCol(int x, int y, Action<LandType> proc)
            {
                for (var yr = y-adjacentRadius; yr <= y+adjacentRadius; yr++)
                {
                    if (yr >= 0 && yr < _dimY && x >= 0 && x < _dimX) proc(land[x, yr] ?? LandType.Land);
                }
            }

            var ret = new LandType?[_dimX, _dimY];

            for (var y = 0; y < _dimY; y++)
            {
                var count = new RunningCount();

                // add first col
                ProcessCol(0, y, tile => count.AddTile(tile));

                for (var x = 0; x < _dimX; x++)
                {
                    // remove left col
                    ProcessCol(x - 2, y, tile => count.RemoveTile(tile));
                    // add right col
                    ProcessCol(x + 1, y, tile => count.AddTile(tile));

                    switch (land[x,y])
                    {
                        case null:
                            if (count.Trees > 2) ret[x, y] = LandType.Trees;
                            break;
                        case LandType.Trees:
                            ret[x, y] = count.Lumberyards > 2 ? LandType.Lumberyard : LandType.Trees;
                            break;
                        case LandType.Lumberyard:
                            if (count.Lumberyards > 1 && count.Trees > 0) ret[x, y] = LandType.Lumberyard;
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
            }

            return ret;
        }

        void Print(LandType?[,] land)
        {
            Console.WriteLine(new string('-', 100));

            for (var y = 0; y < _dimY; y++)
            {
                var line = new char[_dimX];
                for (var x = 0; x < _dimX; x++)
                {
                    switch (land[x, y])
                    {
                        case null:
                            line[x] = '.';
                            break;
                        case LandType.Lumberyard:
                            line[x] = '#';
                            break;
                        case LandType.Trees:
                            line[x] = '|';
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
                Console.WriteLine(new string(line));
            }
        }

        RunningCount GetLandCount(LandType?[,] land)
        {
            var ret = new RunningCount();
            for (var y = 0; y < _dimY; y++)
            for (var x = 0; x < _dimX; x++)
            {
                ret.AddTile(land[x, y] ?? LandType.Land);
            }

            return ret;
        }

        const string TestInput = @".#.#...|#.
.....#|##|
.|..|...#.
..|#.....#
#.#|||#|#|
...#.||...
.|....|...
||...#|.#|
|.||||..|.
...#.|..|.";

        public override dynamic Answer1()
        {
            //var input = TestInput.Split(Environment.NewLine);
            var input = GetRawInput();
            var land = GetLandTiles(input);
            //Print(land);

            var minute = 0;
            do
            {
                land = GetNextMinute(land, 1);
                minute++;
                //Print(land);
                //Console.ReadKey();
            } while (minute < 10);

            //Print(land);

            var count = GetLandCount(land);
            return count.ResourceValue();
        }

        public override dynamic Answer2()
        {
            // following relevant observation, the pattern on minute 419 is repeated every 28 iterations, ie. 447, 475
            // so after we hit minute 419, we only need to run (1_000_000_000 - 419) % 28 more iterations.

            var input = GetRawInput();
            var land = GetLandTiles(input);
            //Print(land);

            const int totalIterations = 419 + (1000000000 - 419) % 28;

            var minute = 0;
            do
            {
                land = GetNextMinute(land, 1);
                minute++;
                //if (minute > 419 && (minute-419)%28==0)
                //{
                //    var runningCount = GetLandCount(land);
                //    Print(land);
                //    Console.WriteLine($"{runningCount}, Minute: {minute}");
                    //Console.ReadKey();
                //}
            } while (minute < totalIterations);

            //Print(land);
            //var runningCount = GetLandCount(land);
            //Console.WriteLine($"{runningCount}, Minute: {minute}");

            var count = GetLandCount(land);
            return count.ResourceValue();
        }

        void ObservePatterns()
        {
            var input = GetRawInput();
            var land = GetLandTiles(input);
            Print(land);

            var minute = 0;
            do
            {
                land = GetNextMinute(land, 1);
                minute++;
                //if (minute % 1000 == 0)
                {
                    Print(land);
                    var runningCount = GetLandCount(land);
                    Console.WriteLine($"{runningCount}, Minute: {minute}");
                    if (runningCount.Trees == 593 && runningCount.Lumberyards == 334)
                    {
                        Console.ReadKey();
                    }
                }
            } while (minute < 1_000_000_000);

            Print(land);
        }
    }
}