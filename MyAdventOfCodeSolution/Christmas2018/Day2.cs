using System;
using System.Collections.Generic;
using System.Linq;

namespace MyAdventOfCodeSolution.Christmas2018
{
    class Day2 : Day
    {
        string[] _boxIds;
        string[] BoxIds => _boxIds ?? (_boxIds = GetRawInput());

        public override dynamic Answer1()
        {
            var cnt2 = 0;
            var cnt3 = 0;
            foreach (var boxId in BoxIds)
            {
                var charCounts = new HashSet<int>(boxId.GroupBy(x => x).Select(x => x.Count()));
                if (charCounts.Contains(2)) cnt2++;
                if (charCounts.Contains(3)) cnt3++;
            }

            return cnt3 * cnt2;
        }

        public override dynamic Answer2()
        {
            for (var i = 0; i < BoxIds.Length; i++)
            for (var j = i+1; j < BoxIds.Length; j++)
            {
                var boxId1 = BoxIds[i];
                var boxId2 = BoxIds[j];

                for (var k = 0; k < boxId1.Length; k++)
                {
                    var failed = false;
                    for (var l = 0; l < boxId1.Length; l++)
                    {
                        if (k == l) continue;
                        failed = boxId1[l] != boxId2[l];
                        if (failed) break;
                    }
                    if (!failed) return boxId1.Remove(k,1);
                }
            }

            throw new InvalidOperationException();
        }
    }
}