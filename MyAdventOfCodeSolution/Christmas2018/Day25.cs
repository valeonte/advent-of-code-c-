using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

// ReSharper disable UnusedMember.Local

namespace MyAdventOfCodeSolution.Christmas2018
{
    class Day25 : Day
    {
        class FixedPoint : IEquatable<FixedPoint>
        {
            FixedPoint(int x, int y, int z, int w)
            {
                X = x;
                Y = y;
                Z = z;
                W = w;
            }

            public int X { get; }
            public int Y { get; }
            public int Z { get; }
            public int W { get; }

            public Constellation Constellation { get; set; }

            public override string ToString() => $"({X},{Y},{Z},{W})";

            public int DistanceFrom(FixedPoint other) => Math.Abs(X - other.X) + Math.Abs(Y - other.Y) + Math.Abs(Z - other.Z) + Math.Abs(W - other.W);

            static readonly Regex LineRegex = new Regex(@"^(?<X>[\d-]+),(?<Y>[\d-]+),(?<Z>[\d-]+),(?<W>[\d-]+)$");

            public static FixedPoint FromLine(string line)
            {
                var match = LineRegex.Match(line);
                if (!match.Success) throw new InvalidOperationException();

                return new FixedPoint(int.Parse(match.Groups["X"].Value), int.Parse(match.Groups["Y"].Value), int.Parse(match.Groups["Z"].Value),
                    int.Parse(match.Groups["W"].Value));
            }

            #region Equality

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is FixedPoint && Equals((FixedPoint) obj);
            }

            public bool Equals(FixedPoint other)
            {
                return X == other.X && Y == other.Y && Z == other.Z && W == other.W;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = X;
                    hashCode = (hashCode * 397) ^ Y;
                    hashCode = (hashCode * 397) ^ Z;
                    hashCode = (hashCode * 397) ^ W;
                    return hashCode;
                }
            }

            #endregion
        }

        class Constellation : ICollection<FixedPoint>, IEquatable<Constellation>
        {
            public int Id { get; } = _idGenerator++;

            readonly ISet<FixedPoint> _points = new HashSet<FixedPoint>();
            static int _idGenerator = 1;


            public IEnumerator<FixedPoint> GetEnumerator() => _points.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public void Add(FixedPoint item)
            {
                _points.Add(item);
            }

            public void Clear()
            {
                _points.Clear();
            }

            public bool Contains(FixedPoint item)
            {
                return _points.Contains(item);
            }

            public void MergeConstellation(Constellation other)
            {
                foreach (var otherPoint in other)
                {
                    Add(otherPoint);
                    otherPoint.Constellation = this;
                }
            }

            public void CopyTo(FixedPoint[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public bool Remove(FixedPoint item)
            {
                if (item.Constellation == null || !item.Constellation.Equals(this)) return false;
                item.Constellation = null;

                return _points.Remove(item);
            }

            public int Count => _points.Count;
            public bool IsReadOnly => false;

            public override bool Equals(object obj)
            {
                if (obj is null) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == GetType() && Equals((Constellation) obj);
            }

            public bool Equals(Constellation other)
            {
                if (other is null) return false;
                if (ReferenceEquals(this, other)) return true;
                return Id == other.Id;
            }

            public override int GetHashCode()
            {
                return Id;
            }
        }

        const string TestInput = @"0,0,0,0
 3,0,0,0
 0,3,0,0
 0,0,3,0
 0,0,0,3
 0,0,0,6
 9,0,0,0
12,0,0,0";

        const string TestInput2 = @"-1,2,2,0
0,0,2,-2
0,0,0,-2
-1,2,0,0
-2,-2,-2,2
3,0,2,-1
-1,3,2,2
-1,0,-1,0
0,2,1,-2
3,0,0,0";

        const string TestInput3 = @"1,-1,0,1
2,0,-1,0
3,2,-1,0
0,0,3,1
0,0,-1,-1
2,3,-2,0
-2,2,0,0
2,-2,0,-1
1,-1,0,-1
3,2,0,2";

        const string TestInput4 = @"1,-1,-1,-2
-2,-2,0,1
0,2,1,3
-2,3,-2,1
0,2,3,-2
-1,-1,1,-2
0,-2,-1,0
-2,2,3,-1
1,2,2,0
-1,-2,0,-2";

        public override dynamic Answer1()
        {
            //var input = TestInput.Split(Environment.NewLine);
            //var input = TestInput2.Split(Environment.NewLine);
            //var input = TestInput3.Split(Environment.NewLine);
            //var input = TestInput4.Split(Environment.NewLine);
            var input = GetRawInput();

            var points = input.Select(FixedPoint.FromLine).ToArray();

            var conDist = 3;

            var constellations = new HashSet<Constellation>();
            for (var i = 0; i < points.Length; i++)
            {
                var p1 = points[i];
                if (p1.Constellation == null)
                {
                    p1.Constellation = new Constellation {p1};
                    constellations.Add(p1.Constellation);
                }
                
                for (var j = i + 1; j < points.Length; j++)
                {
                    //if (points.Count(p => p.Constellation != null) != constellations.Sum(c => c.Count))
                    //{
                    //    var a = "breal";
                    //}

                    var p2 = points[j];
                    if (p1.DistanceFrom(p2) > conDist) continue;

                    if (p2.Constellation == null)
                    {
                        p2.Constellation = p1.Constellation;
                        p1.Constellation.Add(p2);
                        continue;
                    }

                    if (p1.Constellation.Equals(p2.Constellation)) continue;

                    var oldConstellation = p2.Constellation;
                    p1.Constellation.MergeConstellation(oldConstellation);
                    constellations.Remove(oldConstellation);
                }
            }

            return constellations.Count;
        }
    }
}