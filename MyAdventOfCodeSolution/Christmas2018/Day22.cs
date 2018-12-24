using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Local

namespace MyAdventOfCodeSolution.Christmas2018
{
    class Day22 : Day
    {
        enum Tool
        {
            None,
            ClimbingGear,
            Torch
        }

        class Region : IEquatable<Region>
        {
            public Region(Area area, int x, int y)
            {
                Area = area;
                X = x;
                Y = y;
            }

            public int X { get; }
            public int Y { get; }
            public Area Area { get; }

            public Region North => Area[X, Y - 1];
            public Region South => Area[X, Y + 1];
            public Region East => Area[X + 1, Y];
            public Region West => Area[X - 1, Y];

            int _geologicalIndex = -1;
            public int GeologicalIndex => _geologicalIndex == -1 ? (_geologicalIndex = CalculateGeologicalIndex()) : _geologicalIndex;

            int CalculateGeologicalIndex()
            {
                if (X == 0 && Y == 0) return 0;
                if (X == Area.TargetX && Y == Area.TargetY) return 0;
                if (Y == 0) return X * 16807;
                if (X == 0) return Y * 48271;

                return Area[X - 1, Y].ErosionLevel * Area[X, Y - 1].ErosionLevel;
            }

            public int ErosionLevel => (GeologicalIndex + Area.Depth) % 20183;

            int _type = -1;
            public int Type => _type == -1 ? (_type =  ErosionLevel % 3) : _type;

            public bool IsCompatible(Tool tool)
            {
                switch (Type)
                {
                    case 0:
                        return tool == Tool.ClimbingGear || tool == Tool.Torch;
                    case 1:
                        return tool == Tool.ClimbingGear || tool == Tool.None;
                    case 2:
                        return tool == Tool.None || tool == Tool.Torch;
                    default:
                        throw new InvalidOperationException();
                }
            }

            public string TypeBreakdown => $"Geological Index: {GeologicalIndex}, Erosiol Level: {ErosionLevel}, Type: {Type}";

            IEnumerable<Region> PotentialNext()
            {
                if (X > 0) yield return West;
                yield return East;
                if (Y > 0) yield return North;
                yield return South;
            }

            public int GetCostToAdjacentRegion(Region region, Tool currentTool, out Tool newTool)
            {
                newTool = currentTool;
                while (!IsCompatible(newTool) || !region.IsCompatible(newTool))
                {
                    newTool = (Tool) (((int)newTool + 1) % 3);
                }

                return 1 + (newTool == currentTool ? 0 : 7);
            }

            int _minDistanceToTarget = -1;
            int MinDistanceToTarget => _minDistanceToTarget == -1
                ? (_minDistanceToTarget = Math.Abs(X - Area.TargetX) + Math.Abs(Y - Area.TargetY))
                : _minDistanceToTarget;

            static Stopwatch _stopwatch;
            static long _iterations;
            public int GetShortestPathToTarget(Tool currentTool, int pathCostSoFar, int bestPath, ISet<Region> visited)
            {
                if (_stopwatch == null) _stopwatch = Stopwatch.StartNew();
                Interlocked.Increment(ref _iterations);
                if (_iterations % 1_000_000 == 0)
                    Console.WriteLine($"Path cost so far: {pathCostSoFar}, Best path: {bestPath}, Iterations: {_iterations/1_000_000:N1}m, Speed: {1.0 * _iterations / _stopwatch.ElapsedMilliseconds:F} per ms");

                if (X == Area.TargetX && Y == Area.TargetY)
                {
                    // got there! I am the target
                    //Area.Print(visited);
                    var newCost = pathCostSoFar + (currentTool == Tool.Torch ? 0 : 7);
                    Console.WriteLine($"New path cost: {newCost}, Best path: {bestPath}, Iterations: {_iterations}, Speed: {1.0 * _iterations / _stopwatch.ElapsedMilliseconds} per ms");
                    //if (newCost < bestPath) Console.ReadKey();
                    return newCost;
                }

                //Console.WriteLine($"Tool: {currentTool}, Cost: {pathCostSoFar}, Best path: {bestPath}, Visited: {string.Join(", ", visited.Select(v=>v.ToString()))}");
                //Console.ReadKey();

                //var newVisited = new HashSet<Region>(visited.Concat(new[] {this}));
                visited.Add(this);
                foreach (var nextRegion in PotentialNext().Where(r => !visited.Contains(r)))
                {
                    var costToAdjacent = GetCostToAdjacentRegion(nextRegion, currentTool, out var newTool);

                    var newPathCostSoFar = pathCostSoFar + costToAdjacent;
                    
                    // stop iterating if bestSoFar exceeded
                    // comparison cost is the minimum distance of the next region, plus seven if we don't hold Torch
                    var comparisonCost = newPathCostSoFar + nextRegion.MinDistanceToTarget + (newTool == Tool.Torch ? 0 : 7);
                    if (comparisonCost >= bestPath) continue;

                    var newBest = nextRegion.GetShortestPathToTarget(newTool, newPathCostSoFar, bestPath, visited);
                    if (newBest < bestPath) bestPath = newBest;
                }

                visited.Remove(this);
                return bestPath;
            }

            #region Equality

            public override bool Equals(object obj)
            {
                if (obj is null) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == GetType() && Equals((Region)obj);
            }

            public bool Equals(Region other)
            {
                if (other is null) return false;
                return ReferenceEquals(this, other) || X.Equals(other.X) && Y.Equals(other.Y);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (X * 397) ^ Y;
                }
            }

            #endregion

            public override string ToString() => $"({X}, {Y})";
        }

        class Area
        {
            readonly Region[,] _regions;

            public Area(int maxX, int maxY, int depth, int targetX, int targetY)
            {
                MaxX = maxX;
                MaxY = maxY;
                Depth = depth;
                TargetX = targetX;
                TargetY = targetY;

                _regions = new Region[maxX + 1, maxY + 1];
            }

            public Region this[int x, int y] => _regions[x, y];
            public int MaxX { get; }
            public int MaxY { get; }
            public int Depth { get; }
            public int TargetX { get; }
            public int TargetY { get; }

            public void Initialise()
            {
                for (var x = 0; x <= MaxX; x++)
                for (var y = 0; y <= MaxY; y++)
                {
                    _regions[x, y] = new Region(this, x, y);
                }
            }

            public int CalculateRiskLevel()
            {
                var riskLevel = 0;
                for (var x = 0; x <= TargetX; x++)
                for (var y = 0; y <= TargetY; y++)
                    riskLevel += _regions[x, y].Type;

                return riskLevel;
            }

            readonly Random _rnd = new Random();
            public int GetRandomPathToTargetCost(Tool tool)
            {
                var targetSum = TargetX + TargetY;

                var curRegion = _regions[0, 0];
                var totalCost = 0;
                var curTool = tool;

                while (curRegion.X != TargetX || curRegion.Y != TargetY)
                {
                    //Print(curRegion.X, curRegion.Y);
                    //Console.WriteLine($"Tool: {curTool}, Cost: {totalCost}");
                    //Console.ReadKey();

                    Region nextRegion;
                    if (curRegion.X < TargetX && curRegion.Y < TargetY)
                    {
                        nextRegion = _rnd.Next(targetSum) < TargetX ? curRegion.East : curRegion.South;
                    } else if (curRegion.X == TargetX)
                    {
                        nextRegion = curRegion.South;
                    }
                    else
                    {
                        nextRegion = curRegion.East;
                    }

                    totalCost += curRegion.GetCostToAdjacentRegion(nextRegion, curTool, out curTool);
                    curRegion = nextRegion;
                }

                if (curTool != Tool.Torch) totalCost += 7;

                return totalCost;
            }

            public void Print(ICollection<Region> path = null)
            {
                Console.WriteLine(new string('-', 100));
                for (var y = 0; y <= MaxY; y++)
                {
                    var line = new char[MaxX + 1];
                    for (var x = 0; x <= MaxX; x++)
                    {
                        if (x == 0 && y == 0) line[x] = 'M';
                        else if (x == TargetX && y == TargetY) line[x] = 'T';
                        else if (path != null && path.Any(r => x == r.X && y == r.Y)) line[x] = 'X';
                        else
                            switch (_regions[x, y].Type)
                            {
                                case 0:
                                    line[x] = '.';
                                    break;
                                case 1:
                                    line[x] = '=';
                                    break;
                                case 2:
                                    line[x] = '|';
                                    break;
                                default:
                                    throw new InvalidOperationException();
                            }

                    }
                    Console.WriteLine(new string(line));
                }
            }

            public override string ToString() => $"{MaxX} x {MaxY} area";
        }

        public override dynamic Answer1()
        {
            //var area = new Area(10, 10, 510);
            var area = new Area(7, 782, 11820, 7, 782);
            area.Initialise();

            //Console.WriteLine(area[0, 0].TypeBreakdown);
            //Console.WriteLine(area[1, 0].TypeBreakdown);
            //Console.WriteLine(area[0, 1].TypeBreakdown);
            //Console.WriteLine(area[1, 1].TypeBreakdown);
            //Console.WriteLine(area[10, 10].TypeBreakdown);

            var riskLevel = area.CalculateRiskLevel();

            return riskLevel;
        }

        enum Direction
        {
            Top, Left, Bottom, Right
        }

        int MinCosPathWay(Area area, int baseLineCost)
        {
            var tc = new (int Cost, Tool Tool, Direction Direction)[area.MaxX + 1, area.MaxY + 1];
            for (var y = 0; y <= area.MaxY; y++)
            for (var x = 0; x <= area.MaxX; x++)
            {
                tc[x,y] = (int.MaxValue, Tool.None, Direction.Top);
            }

            while (true)
            {
                var switches1 = 0;
                var ties1 = 0;
                for (var y = 0; y <= area.MaxY; y++)
                for (var x = 0; x <= area.MaxX; x++)
                {
                    if (x == 0 && y == 0)
                    {
                        tc[0, 0] = (0, Tool.Torch, Direction.Left);
                        continue;
                    }

                    if (y == 0)
                    {
                        var cost = tc[x - 1, y].Cost + area[x - 1, y].GetCostToAdjacentRegion(area[x, y], tc[x - 1, y].Tool, out var newTool);
                        if (cost >= tc[x, y].Cost) continue;

                        tc[x, y] = (cost, newTool, Direction.Left);
                        switches1++;
                    }
                    else if (x == 0)
                    {
                        var cost = tc[x, y - 1].Cost + area[x, y - 1].GetCostToAdjacentRegion(area[x, y], tc[x, y - 1].Tool, out var newTool);
                        if (cost >= tc[x, y].Cost) continue;

                        switches1++;
                        tc[x, y] = (cost, newTool, Direction.Top);
                    }
                    else
                    {
                        var topCost = tc[x, y - 1].Cost + area[x, y - 1].GetCostToAdjacentRegion(area[x, y], tc[x, y - 1].Tool, out var topTool);
                        var leftCost = tc[x - 1, y].Cost + area[x - 1, y].GetCostToAdjacentRegion(area[x, y], tc[x - 1, y].Tool, out var leftTool);

                        if (tc[x, y].Cost < topCost && tc[x, y].Cost < leftCost) continue;

                        switches1++;

                        if (topCost < leftCost)
                        {
                            tc[x, y] = (topCost, topTool, Direction.Top);
                        }
                        else
                        {
                            if (topCost == leftCost && topTool != leftTool) ties1++;
                            tc[x, y] = (leftCost, leftTool, Direction.Left);
                        }
                    }
                }

                var switches2 = 0;
                var ties2 = 0;
                for (var y = area.MaxY; y >= 0; y--)
                for (var x = area.MaxX; x >= 0; x--)
                {
                    if (x == area.MaxX && y == area.MaxY) continue;

                    if (y == area.MaxY)
                    {
                        var cost = tc[x + 1, y].Cost + area[x + 1, y].GetCostToAdjacentRegion(area[x, y], tc[x + 1, y].Tool, out var newTool);
                        if (cost >= tc[x, y].Cost) continue;

                        tc[x, y] = (cost, newTool, Direction.Right);
                        switches2++;
                    }
                    else if (x == area.MaxX)
                    {
                        var cost = tc[x, y + 1].Cost + area[x, y + 1].GetCostToAdjacentRegion(area[x, y], tc[x, y + 1].Tool, out var newTool);
                        if (cost >= tc[x, y].Cost) continue;

                        tc[x, y] = (cost, newTool, Direction.Bottom);
                        switches2++;
                    }
                    else
                    {
                        var bottomCost = tc[x, y + 1].Cost + area[x, y + 1].GetCostToAdjacentRegion(area[x, y], tc[x, y + 1].Tool, out var bottomTool);
                        var rightCost = tc[x + 1, y].Cost + area[x + 1, y].GetCostToAdjacentRegion(area[x, y], tc[x + 1, y].Tool, out var rightTool);

                        if (tc[x, y].Cost <= bottomCost && tc[x, y].Cost <= rightCost) continue;
                        switches2++;

                        if (bottomCost < rightCost)
                        {
                            tc[x, y] = (bottomCost, bottomTool, Direction.Bottom);
                        }
                        else
                        {
                            if (bottomCost == rightCost && bottomTool != rightTool) ties2++;
                            tc[x, y] = (rightCost, rightTool, Direction.Right);
                        }
                    }
                }
            }

            return 2;
        }

        public override dynamic Answer2()
        {
            var area = new Area(50, 50, 510, 10, 10);
            //var area = new Area(1000, 1000, 11820, 7, 782);

            area.Initialise();

            var baseLineCost = 2406;
            for (var i = 0; i < 100000; i++)
            {
                var newCost = area.GetRandomPathToTargetCost(Tool.Torch);
                if (newCost < baseLineCost) baseLineCost = newCost;
            }
            Console.WriteLine($"Baseline cost: {baseLineCost}");

            return MinCosPathWay(area, baseLineCost);

            File.AppendAllText("safe.txt", "start");
            var task1 = Task.Run(() => area[0, 1].GetShortestPathToTarget(Tool.Torch, 1, baseLineCost, new HashSet<Region> {area[0, 0]}));
            var task2 = Task.Run(() => area[1, 0].GetShortestPathToTarget(Tool.Torch, 1, baseLineCost, new HashSet<Region> {area[0, 0]}));
            Task.WaitAll(task1, task2);

            File.AppendAllText("safe.txt", $"task1: {task1.Result}, task2: {task2.Result}");

            var minCost = Math.Min(task1.Result, task2.Result);

            return minCost;
        }
    }
}