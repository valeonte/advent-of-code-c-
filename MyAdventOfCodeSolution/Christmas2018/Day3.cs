using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace MyAdventOfCodeSolution.Christmas2018
{
    class Day3 : Day
    {
        class Claim
        {
            static readonly Regex ClaimParse = new Regex(@"#(\d+) @ (\d+),(\d+): (\d+)x(\d+)");

            public int Id { get; }

            public int Left { get; }
            public int Top { get; }

            public int Width { get; }
            public int Height { get; }

            public Claim(string claimString)
            {
                var match = ClaimParse.Match(claimString);
                if (!match.Success)
                    throw new InvalidOperationException("Invalid claims string");

                Id = int.Parse(match.Groups[1].Value);

                Left = int.Parse(match.Groups[2].Value);
                Top = int.Parse(match.Groups[3].Value);

                Width = int.Parse(match.Groups[4].Value);
                Height = int.Parse(match.Groups[5].Value);
            }

            public override string ToString() => $"#{Id} @ {Left},{Top}: {Width}x{Height}";
        }

        readonly Claim[] _claims;
        readonly int _fabricWidth, _fabricHeight;

        public Day3()
        {
            _claims = GetRawInput().Select(c => new Claim(c)).ToArray();
            _fabricWidth = _claims.Select(c => c.Left + c.Width).Max();
            _fabricHeight = _claims.Select(c => c.Top + c.Height).Max();
        }

        int[,] _fabricSquares;
        int[,] FabricSquares => _fabricSquares ?? (_fabricSquares = CalculateFabricSquares());

        int[,] CalculateFabricSquares()
        {
            var ret = new int[_fabricWidth, _fabricHeight];

            foreach (var claim in _claims)
            {
                for (var x = claim.Left; x < claim.Left + claim.Width; x++)
                for (var y = claim.Top; y < claim.Top + claim.Height; y++)
                    ret[x, y]++;
            }

            return ret;
        }

        public override dynamic Answer1()
        {
            var cnt = FabricSquares.Cast<int>().Count(s => s > 1);

            return cnt;
        }

        public override dynamic Answer2()
        {
            foreach (var claim in _claims)
            {
                var failed = false;
                for (var x = claim.Left; x < claim.Left + claim.Width; x++)
                {
                    for (var y = claim.Top; y < claim.Top + claim.Height; y++)
                    {
                        failed = FabricSquares[x, y] > 1;
                        if (failed) break;
                    }

                    if (failed) break;
                }

                if (!failed) return claim.Id;
            }

            throw new InvalidOperationException("No appropriate claim found!");
        }
    }
}