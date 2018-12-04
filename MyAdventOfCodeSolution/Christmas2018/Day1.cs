using System;
using System.Collections.Generic;
using System.Linq;

namespace MyAdventOfCodeSolution.Christmas2018
{
    class Day1 : Day
    {
        const int MaxIterations = 500;

        int[] _sequence;
        IEnumerable<int> Sequence => _sequence ?? (_sequence = GetRawInput().Select(int.Parse).ToArray());

        public override dynamic Answer1()
        {
            return Sequence.Sum();
        }

        public override dynamic Answer2()
        {
            var seenFrequencies = new HashSet<int> {0};
            var lastFrequency = 0;
            for (var rep = 0; rep < MaxIterations; rep++)
                foreach (var next in Sequence)
                {
                    lastFrequency += next;
                    if (seenFrequencies.Contains(lastFrequency))
                        return lastFrequency;

                    seenFrequencies.Add(lastFrequency);
                }

            throw new InvalidOperationException();
        }
    }
}