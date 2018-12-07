using System;
using System.Collections.Generic;
using System.Linq;

namespace MyAdventOfCodeSolution.Christmas2018
{
    class Day6 : Day
    {
        struct Coordinate
        {
            public Coordinate(int x, int y)
            {
                X = x;
                Y = y;
            }

            public int X { get; }
            public int Y { get; }

            public int DistanceFrom(Coordinate coord) => Math.Abs(X - coord.X) + Math.Abs(Y - coord.Y);
        }

        Coordinate[] _coordinates;

        Coordinate[] Coordinates => _coordinates ?? (_coordinates =
                                        GetRawInput()
                                            .Select(s => s.Split(','))
                                            .Select(s => new Coordinate(int.Parse(s[0]), int.Parse(s[1])))
                                            .ToArray()
                                    );
        
        public override dynamic Answer1()
        {
            var minX = int.MaxValue;
            var maxX = int.MinValue;
            var minY = int.MaxValue;
            var maxY = int.MinValue;
            var closestNodeCounts = new Dictionary<Coordinate, int>(Coordinates.Length);
            foreach (var coordinate in Coordinates)
            {
                if (coordinate.X < minX) minX = coordinate.X;
                if (coordinate.X > maxX) maxX = coordinate.X;

                if (coordinate.Y < minY) minY = coordinate.Y;
                if (coordinate.Y > maxY) maxY = coordinate.Y;

                closestNodeCounts.Add(coordinate, 0);
            }

            var borderNodes = new HashSet<Coordinate>();
            for (var x = minX; x <= maxX; x++)
            for (var y = minY; y <= maxY; y++)
            {
                var curNode = new Coordinate(x, y);
                var shortestDistance = int.MaxValue;
                Coordinate? closestNode = null;
                foreach (var node in Coordinates)
                {
                    var distance = curNode.DistanceFrom(node);
                    if (distance > shortestDistance) continue;
                    if (distance == shortestDistance)
                    {
                        // tie
                        closestNode = null;
                        continue;
                    }

                    shortestDistance = distance;
                    closestNode = node;
                }

                // tie
                if (!closestNode.HasValue) continue;

                closestNodeCounts[closestNode.Value]++;

                if (x == minX || y == minY || x == maxX || y == maxY) borderNodes.Add(closestNode.Value);
            }

            var maxCount = closestNodeCounts
                .Where(c => !borderNodes.Contains(c.Key))
                .OrderByDescending(c => c.Value)
                .First();

            return maxCount.Value;
        }

        public override dynamic Answer2()
        {
            var minX = int.MaxValue;
            var maxX = int.MinValue;
            var minY = int.MaxValue;
            var maxY = int.MinValue;
            foreach (var coordinate in Coordinates)
            {
                if (coordinate.X < minX) minX = coordinate.X;
                if (coordinate.X > maxX) maxX = coordinate.X;

                if (coordinate.Y < minY) minY = coordinate.Y;
                if (coordinate.Y > maxY) maxY = coordinate.Y;
            }

            var nodesInSageRegion = 0;
            for (var x = minX; x <= maxX; x++)
            for (var y = minY; y <= maxY; y++)
            {
                var curNode = new Coordinate(x, y);
                var distanceSum = 0;
                var isSafe = true;
                foreach (var node in Coordinates)
                {
                    distanceSum += curNode.DistanceFrom(node);
                    isSafe = distanceSum < 10000;
                    if (!isSafe) break;
                }

                if (isSafe) nodesInSageRegion++;
            }

            return nodesInSageRegion;
        }

    }
}