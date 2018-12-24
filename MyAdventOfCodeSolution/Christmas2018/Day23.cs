using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Local

namespace MyAdventOfCodeSolution.Christmas2018
{
    class Day23 : Day
    {
        public struct Coords : IEquatable<Coords>
        {
            public Coords(int x, int y, int z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public int X { get; }
            public int Y { get; }
            public int Z { get; }

            static readonly int[] AllSigns = {-1, 1};
            public IEnumerable<Coords> GetCoordsInDistance(int distance)
            {
                for (var dx = 0; dx <= distance; dx++)
                for (var dy = 0; dy <= distance - dx; dy++)
                {
                    var dz = distance - dx - dy;
                    foreach (var signX in AllSigns.Where(s => dx != 0 || s == 1))
                    foreach (var signY in AllSigns.Where(s => dy != 0 || s == 1))
                    foreach (var signZ in AllSigns.Where(s => dz != 0 || s == 1))
                    {
                        yield return new Coords(X + signX * dx, Y + signY * dy, Z + signZ * dz);
                    }
                }
            }

            public Coords MoveTowards(Coords target, int distance)
            {
                var dx = target.X - X;
                var dy = target.Y - Y;
                var dz = target.Z - Z;
                var dsum = Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz);

                var moveX = (int)(1.0 * dx / dsum * distance);
                var moveY = (int) (1.0 * dy / dsum * distance);
                var moveZ = Math.Sign(dz) * (distance - Math.Abs(moveX) - Math.Abs(moveY));

                return new Coords(X + moveX, Y + moveY, Z + moveZ);
            }

            public int DistanceFrom(Coords other) => Math.Abs(other.X - X) + Math.Abs(other.Y - Y) + Math.Abs(other.Z - Z);
            public int DistanceFrom(int x, int y, int z) => Math.Abs(x - X) + Math.Abs(y - Y) + Math.Abs(z - Z);

            public override string ToString() => $"({X}, {Y}, {Z})";

            #region Equality

            public override bool Equals(object obj)
            {
                if (obj is null) return false;
                return obj is Coords coords && Equals(coords);
            }

            public bool Equals(Coords other)
            {
                return X == other.X && Y == other.Y && Z == other.Z;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = X;
                    hashCode = (hashCode * 397) ^ Y;
                    hashCode = (hashCode * 397) ^ Z;
                    return hashCode;
                }
            }

            #endregion
        }

        public class Nanobot : IEquatable<Nanobot>
        {
            public Nanobot(int x, int y, int z, int radius)
            {
                Coords = new Coords(x, y, z);
                Radius = radius;
            }

            public Coords Coords { get; }

            public int Radius { get; }

            public override string ToString() => $"{Coords}, R: {Radius}";

            public bool IsInRange(Nanobot other) => Coords.DistanceFrom(other.Coords) <= Radius;

            public bool IsInRange(Coords other) => Coords.DistanceFrom(other) <= Radius;
            public bool IsInRange(int x, int y, int z) => Coords.DistanceFrom(x, y, z) <= Radius;

            static readonly Regex LineRegex = new Regex(@"pos=<([\d-]+),([\d-]+),([\d-]+)>, r=(\d+)");
            public static Nanobot FromLine(string line)
            {
                var match = LineRegex.Match(line);
                if (!match.Success) throw new InvalidOperationException();

                return new Nanobot(
                    int.Parse(match.Groups[1].Value),
                    int.Parse(match.Groups[2].Value),
                    int.Parse(match.Groups[3].Value),
                    int.Parse(match.Groups[4].Value));
            }

            #region Equality

            public override bool Equals(object obj)
            {
                if (obj is null) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == GetType() && Equals((Nanobot) obj);
            }

            public bool Equals(Nanobot other)
            {
                if (other is null) return false;
                return ReferenceEquals(this, other) || Coords.Equals(other.Coords);
            }

            public override int GetHashCode()
            {
                return Coords.GetHashCode();
            }

            #endregion
        }

        const string TestInput = @"pos=<0,0,0>, r=4
pos=<1,0,0>, r=1
pos=<4,0,0>, r=3
pos=<0,2,0>, r=1
pos=<0,5,0>, r=3
pos=<0,0,3>, r=1
pos=<1,1,1>, r=1
pos=<1,1,2>, r=1
pos=<1,3,1>, r=1";

        const string TestInput2 = @"pos=<10,12,12>, r=2
pos=<12,14,12>, r=2
pos=<16,12,12>, r=4
pos=<14,14,14>, r=6
pos=<50,50,50>, r=200
pos=<10,10,10>, r=5";

        public override dynamic Answer1()
        {
            return "";
            //var input = TestInput.Split(Environment.NewLine);
            var input = GetRawInput();

            var bots = input.Select(Nanobot.FromLine).ToArray();
            var mostPowerful = bots.OrderByDescending(b => b.Radius).First();

            var inRange = bots.Count(bot => mostPowerful.IsInRange(bot));

            return inRange;
        }

        public IEnumerable<Nanobot> Downscale(IEnumerable<Nanobot> bots, int factor)
        {
            foreach (var bot in bots)
            {
                if (bot.Coords.X % factor != 0 || bot.Coords.Y % factor != 0 || bot.Coords.Z % factor != 0 || bot.Radius % factor != 0)
                {
                    //throw new InvalidOperationException();
                }

                yield return new Nanobot(bot.Coords.X / factor, bot.Coords.Y / factor, bot.Coords.Z / factor, bot.Radius / factor);
            }
        }

        IEnumerable<int> Divisors(int num)
        {
            for (var i = 1; i < (int)Math.Sqrt(num); i++)
            {
                if (num % i == 0) yield return i;
            }
        }

        void FindCommonDivisors(ICollection<Nanobot> bots)
        {
            int[] divisors = null;
            foreach (var bot in bots)
            {
                var newDivisors = Divisors(bot.Coords.Y);
                divisors = (divisors?.Intersect(newDivisors) ?? newDivisors).ToArray();
            }
        }

        void FindMinDistance(ICollection<Nanobot> bots)
        {
            var minDistance = int.MaxValue;
            Nanobot bot1, bot2;

            foreach (var bot in bots)
            foreach (var other in bots.Where(b=>!b.Equals(bot)))
            {
                var dist = bot.Coords.DistanceFrom(other.Coords);
                if (dist >= minDistance) continue;

                minDistance = dist;
                bot1 = bot;
                bot2 = other;
            }

        }

        static Coords ScanAverage(ICollection<Nanobot> bots)
        {
            var xAvg = (int)bots.Average(b => b.Coords.X);
            var yAvg = (int)bots.Average(b => b.Coords.Y);
            var zAvg = (int)bots.Average(b => b.Coords.Z);
            var coords = new Coords(xAvg, yAvg, zAvg);

            List<Nanobot> outOfRange, inRange = null;
            var tried = new HashSet<Nanobot>();
            int lastInRange;
            do
            {
                lastInRange = inRange?.Count ?? 0;
                outOfRange = OutOfRange(bots, coords, out inRange);

                Nanobot nearest = null;
                var minRadiusDiff = int.MaxValue;
                foreach (var bot in outOfRange.Where(b=>!tried.Contains(b)))
                {
                    var diff = bot.Coords.DistanceFrom(coords) - bot.Radius;
                    if (diff >= minRadiusDiff) continue;

                    minRadiusDiff = diff;
                    nearest = bot;
                }

                if (nearest == null) throw new InvalidOperationException();

                tried.Add(nearest);

                coords = coords.MoveTowards(nearest.Coords, minRadiusDiff);

            } while (true);

            return new Coords(0, 0, 0);
        }

        static List<Nanobot> OutOfRange(ICollection<Nanobot> bots, Coords coords, out List<Nanobot> inRange)
        {
            inRange = new List<Nanobot>();
            var outOfRange = new List<Nanobot>();
            foreach (var bot in bots)
            {
                if (bot.IsInRange(coords.X, coords.Y, coords.Z)) inRange.Add(bot);
                else outOfRange.Add(bot);
            }

            return outOfRange;
        }

        static Coords ScanFromBest(Nanobot bestBot, int inRange, ICollection<Nanobot> bots)
        {
            var maxInRange = inRange;
            var bestCoords = bestBot.Coords;
            var bestCoordsDistance = bestBot.Coords.DistanceFrom(0, 0, 0);

            for (var distance = 1; distance < bestBot.Radius; distance++)
            {
                var localMax = -1;
                foreach (var coords in bestBot.Coords.GetCoordsInDistance(distance))
                {
                    var botsInRange = bots.Count(b => b.IsInRange(coords));
                    if (botsInRange > localMax) localMax = botsInRange;
                    if (botsInRange < maxInRange) continue;
                    if (botsInRange == maxInRange)
                    {
                        var coordsDistance = coords.DistanceFrom(0,0,0);
                        if (coordsDistance < bestCoordsDistance)
                        {
                            bestCoords = coords;
                            bestCoordsDistance = coordsDistance;
                        }
                        continue;
                    }

                    if (botsInRange <= maxInRange) continue;

                    maxInRange = botsInRange;
                    bestCoords = coords;
                    bestCoordsDistance = coords.DistanceFrom(0, 0, 0);
                }

                if (localMax > maxInRange)
                {
                    Console.WriteLine("Should we break?");
                }
            }

            return bestCoords;
        }

        public override dynamic Answer2()
        {
            return "";
            var coords = new Coords(5,5,5);
            var dist = coords.GetCoordsInDistance(5).ToArray();
            var dd = dist.Distinct().ToArray();
            foreach (var d in dist)
            {
                if (coords.DistanceFrom(d) != 5) throw new InvalidOperationException();
            }

            //var input = TestInput2.Split(Environment.NewLine);
            var input = GetRawInput();
            var bots = input.Select(Nanobot.FromLine).ToArray();

            //FindMinDistance(bots);
            //FindCommonDivisors(bots);

            var a = ScanAverage(bots);

            //var dsBots = Downscale(bots, 22923).ToArray();
            //bots = dsBots;

            int minX = int.MaxValue, minY = int.MaxValue, minZ = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue, maxZ = int.MinValue;
            foreach (var bot in bots)
            {
                if (bot.Coords.X < minX) minX = bot.Coords.X;
                if (bot.Coords.Y < minY) minY = bot.Coords.Y;
                if (bot.Coords.Z < minZ) minZ = bot.Coords.Z;

                if (bot.Coords.X > maxX) maxX = bot.Coords.X;
                if (bot.Coords.Y > maxY) maxY = bot.Coords.Y;
                if (bot.Coords.Z > maxZ) maxZ = bot.Coords.Z;
            }

            Nanobot bestBot = null;
            var maxInRange = -1;

            foreach (var bot in bots)
            {
                var inRange = bots.Count(b => b.IsInRange(bot));
                if (inRange <= maxInRange) continue;

                maxInRange = inRange;

                bestBot = bot;
            }
            if (bestBot == null) throw new InvalidOperationException();

            ScanFromBest(bestBot, maxInRange, bots);
            return "";

            var bestX = bestBot.Coords.X;
            var bestY = bestBot.Coords.Y;
            var bestZ = bestBot.Coords.Z;

            var maxMissing = 1000 - maxInRange;

            var minSet = new HashSet<(int X, int Y, int Z)>();

            for (var x = minX; x <= maxX; x++)
            for (var y = minY; y <= maxY; y++)
            for (var z = minZ; z <= maxZ; z++)
            {
                var missing = 0;
                var inRange = 0;
                foreach (var bot in bots)
                {
                    if (bot.IsInRange(x, y, z)) inRange++;
                    else missing++;

                    if (missing > maxMissing) break;
                }

                if (inRange < maxInRange) continue;

                if (inRange > maxInRange)
                {
                    minSet.Clear();

                    maxInRange = inRange;
                    maxMissing = 1000 - maxInRange;

                    bestX = x;
                    bestY = y;
                    bestZ = z;
                }

                minSet.Add((x, y, z));
            }

            return (bestX, bestY, bestZ);
        }
    }
}