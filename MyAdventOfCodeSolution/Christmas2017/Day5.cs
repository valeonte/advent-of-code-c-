using System.Linq;

namespace MyAdventOfCodeSolution.Christmas2017
{
    class Day5 : Day
    {
        public override dynamic Answer1()
        {
            var input = GetRawInput().Select(int.Parse).ToArray();
            //var input = new[] {0, 3, 0, 1, -3};

            var nextIdx = 0;
            var steps = 0;

            while (nextIdx >= 0 && nextIdx < input.Length)
            {
                steps++;
                var jumpTo = input[nextIdx];
                input[nextIdx] = jumpTo + 1;
                nextIdx += jumpTo;
            }

            return steps;
        }

        public override dynamic Answer2()
        {
            var input = GetRawInput().Select(int.Parse).ToArray();
            //var input = new[] {0, 3, 0, 1, -3};

            var nextIdx = 0;
            var steps = 0;

            while (nextIdx >= 0 && nextIdx < input.Length)
            {
                steps++;
                var jumpTo = input[nextIdx];
                input[nextIdx] = jumpTo >2 ? jumpTo - 1 : jumpTo + 1;
                nextIdx += jumpTo;
            }

            return steps;
        }
    }
}