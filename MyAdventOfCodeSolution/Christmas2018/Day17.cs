using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

// ReSharper disable UnusedMember.Local

namespace MyAdventOfCodeSolution.Christmas2018
{
    class Day17 : Day
    {
        class WaterReaction
        {
            public Block Result { get; set; }
            public bool AllowsWaterThrough { get; set; }
            public bool ConsumesWater { get; set; }
        }

        abstract class Block
        {
            public abstract WaterReaction WaterReacts();
        }

        class Clay : Block
        {
            public override string ToString() => "#";

            public override WaterReaction WaterReacts() => new WaterReaction();
        }

        class Sand : Block
        {
            public override string ToString() => ".";

            public override WaterReaction WaterReacts() => new WaterReaction {AllowsWaterThrough = true, ConsumesWater = true, Result = new FlowingWater()};
        }

        class FlowingWater : Block
        {
            public override string ToString() => "|";

            public override WaterReaction WaterReacts() => new WaterReaction {AllowsWaterThrough = true, Result = this};
        }

        class SettledWater : Block
        {
            public override string ToString() => "~";

            public override WaterReaction WaterReacts() => new WaterReaction();
        }

        class Slice
        {
            static readonly Regex Input1 = new Regex(@"^x=(\d+), y=(\d+)..(\d+)");
            static readonly Regex Input2 = new Regex(@"^y=(\d+), x=(\d+)..(\d+)");

            readonly Block[,] _slice;

            int _minX, _minY, _maxX, _maxY, _dimX, _dimY;

            public Slice(IEnumerable<string> input)
            {
                _slice = BuildSlice(input);
            }

            static IEnumerable<(int X, int Y)> ExpandInput(IEnumerable<string> input)
            {
                foreach (var line in input)
                {
                    var match = Input1.Match(line);
                    if (match.Success)
                    {
                        var x = int.Parse(match.Groups[1].Value);
                        for (var y = int.Parse(match.Groups[2].Value); y <= int.Parse(match.Groups[3].Value); y++)
                        {
                            yield return (x, y);
                        }
                        continue;
                    }

                    match = Input2.Match(line);
                    if (match.Success)
                    {
                        var y = int.Parse(match.Groups[1].Value);
                        for (var x = int.Parse(match.Groups[2].Value); x <= int.Parse(match.Groups[3].Value); x++)
                        {
                            yield return (x, y);
                        }
                        continue;
                    }

                    throw new InvalidOperationException();
                }
            }

            Block[,] BuildSlice(IEnumerable<string> input)
            {
                var expInput = ExpandInput(input).ToArray();

                _minX = int.MaxValue;
                _maxX = int.MinValue;
                _minY = int.MaxValue;
                _maxY = int.MinValue;

                foreach (var (x, y) in expInput)
                {
                    if (x < _minX) _minX = x;
                    if (y < _minY) _minY = y;

                    if (x > _maxX) _maxX = x;
                    if (y > _maxY) _maxY = y;
                }

                _dimX = _maxX - _minX + 3;
                _dimY = _maxY - _minY + 1;
                var ret = new Block[_dimX, _dimY];

                foreach (var (x, y) in expInput)
                {
                    ret[x - _minX + 1, y - _minY] = new Clay();
                }

                return ret;
            }

            public bool DripWaterTo(int origX)
            {
                if (origX > _maxX || origX < _minX) throw new InvalidOperationException();

                var x = origX - _minX + 1;
                return InternalDripWaterTo(x, 0);
            }

            bool InternalDripWaterTo(int x, int y, IDictionary<(int X, int Y), int> visited = null)
            {
                if (visited == null) visited = new Dictionary<(int X, int Y), int>();

                if (visited.TryGetValue((x, y), out var visitCount))
                {
                    visitCount++;
                    visited[(x, y)] = visitCount;
                    return false;
                }

                visited.Add((x, y), 1);

                var reaction = (_slice[x, y] ?? DefaultBlock).WaterReacts();
                _slice[x, y] = reaction.Result ?? _slice[x, y];

                if (reaction.ConsumesWater) return true;
                if (!reaction.AllowsWaterThrough) return false;

                // end of slice
                if (y == _dimY - 1) return false;

                if (InternalDripWaterTo(x, y + 1, visited)) return true;
                // when y + 1 return false, if y+1 is flowing water, make it settled between clays
                if (y < _dimY - 2 && _slice[x, y + 1] is FlowingWater && !_slice[x, y + 2].WaterReacts().AllowsWaterThrough)
                {
                    if (!SettleWater(x, y + 1)) return false;
                }

                // if the below block allows water through, don't try expanding to the sides
                if (_slice[x, y + 1].WaterReacts().AllowsWaterThrough) return false;

                return InternalDripWaterTo(x - 1, y, visited) || InternalDripWaterTo(x + 1, y, visited);
            }

            bool SettleWater(int x, int y)
            {
                var settleX = x;
                var settleY = y;

                // first testing
                while (_slice[settleX, settleY] is FlowingWater)
                {
                    settleX--;
                }

                if (_slice[settleX, settleY]?.WaterReacts().AllowsWaterThrough ?? true) return false;

                // both directions
                settleX = x + 1;
                while (_slice[settleX, settleY] is FlowingWater)
                {
                    settleX++;
                }
                if (_slice[settleX, settleY]?.WaterReacts().AllowsWaterThrough ?? true) return false;

                // then applying
                settleX = x;
                while (_slice[settleX, settleY] is FlowingWater)
                {
                    _slice[settleX, settleY] = new SettledWater();
                    settleX--;
                }

                settleX = x + 1;
                while (_slice[settleX, settleY] is FlowingWater)
                {
                    _slice[settleX, settleY] = new SettledWater();
                    settleX++;
                }

                return true;
            }

            public int CountWater(bool settledOnly = false)
            {
                var cnt = 0;
                for (var y = 0; y < _dimY; y++)
                for (var x = 0; x < _dimX; x++)
                {
                    var block = _slice[x, y];

                    switch (block)
                    {
                        case null:
                            continue;
                        case FlowingWater _:
                            if (settledOnly) continue;
                            else cnt++;
                            break;
                        case SettledWater _:
                            cnt++;
                            break;
                    }
                }

                return cnt;
            }

            static readonly Block DefaultBlock = new Sand();
            public void Print()
            {
                Console.WriteLine(new string('-', 100));
                for (var y = 0; y < _dimY; y++)
                {
                    var line = new char[_dimX];
                    for (var x = 0; x < _dimX; x++)
                    {
                        line[x] = (_slice[x, y] ?? DefaultBlock).ToString()[0];
                    }
                    Console.WriteLine(new string(line));
                }
            }
        }


        const string TestInput = @"x=495, y=2..7
y=7, x=495..501
x=501, y=3..7
x=498, y=2..4
x=506, y=1..2
x=498, y=10..13
x=504, y=10..13
y=13, x=498..504";

        Slice _slice;

        public override dynamic Answer1()
        {
            //var input = TestInput.Split(Environment.NewLine);
            var input = GetRawInput();
            _slice = new Slice(input);
            //_slice.Print();
            //Console.ReadKey();

            var cnt = 0;
            while (_slice.DripWaterTo(500))
            {
                cnt++;
                //slice.Print();
                //Console.ReadKey();
            }

            //_slice.Print();
            //Console.ReadKey();

            return _slice.CountWater();
        }

        public override dynamic Answer2()
        {
            return _slice.CountWater(true);
        }
    }
}