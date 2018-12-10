using System;
using System.Collections.Generic;
using System.Linq;

namespace MyAdventOfCodeSolution.Christmas2017
{
    class Day6 : Day
    {
        static int[] RedistributeMax(int[] banks)
        {
            var ret = new int[banks.Length];
            Array.Copy(banks, ret, banks.Length);

            var maxBlocks = int.MinValue;
            var maxIdx = -1;

            for (var i = 0; i < ret.Length; i++)
            {
                var blocks = ret[i];
                if (blocks <= maxBlocks) continue;
                maxBlocks = blocks;
                maxIdx = i;
            }

            ret[maxIdx] = 0;

            int GetNextIdx(int idx) => idx == ret.Length - 1 ? 0 : idx + 1;

            var curIdx = maxIdx;

            for (var blocks = maxBlocks; blocks > 0; blocks--)
            {
                curIdx = GetNextIdx(curIdx);
                ret[curIdx]++;
            }

            return ret;
        }

        public override dynamic Answer1()
        {
            //var initialBanks = new[] {0, 2, 7, 0};
            var initialBanks = GetRawInput().First().Split("\t").Select(int.Parse).ToArray();
            var seenBanks = new List<int[]>
            {
                initialBanks
            };

            var steps = 0;
            var newBanks = initialBanks;
            int existingIdx;
            do
            {
                steps++;
                newBanks = RedistributeMax(newBanks);
                var banks = newBanks;
                existingIdx = seenBanks.FindIndex(b => b.SequenceEqual(banks));

                seenBanks.Add(newBanks);
            } while (existingIdx == -1);
            
            return steps;
        }

        public override dynamic Answer2()
        {
            //var initialBanks = new[] {0, 2, 7, 0};
            var initialBanks = GetRawInput().First().Split("\t").Select(int.Parse).ToArray();
            var seenBanks = new List<int[]>
            {
                initialBanks
            };

            var steps = 0;
            var newBanks = initialBanks;
            int existingIdx;
            do
            {
                steps++;
                newBanks = RedistributeMax(newBanks);
                var banks = newBanks;
                existingIdx = seenBanks.FindIndex(b => b.SequenceEqual(banks));

                seenBanks.Add(newBanks);
            } while (existingIdx == -1);

            return steps - existingIdx;
        }
    }
}