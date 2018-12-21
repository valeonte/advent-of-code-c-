using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

// ReSharper disable UnusedMember.Local

namespace MyAdventOfCodeSolution.Christmas2018
{
    class Day21 : Day
    {
        public override dynamic Answer1()
        {
            var ret = Day19.RunProgram(GetRawInput(), new long[6], false, day21Answer1: true);
            return ret[1];
        }

        public override dynamic Answer2()
        {
            var ret = Day19.RunProgram(GetRawInput(), new long[6], false, day21Answer1: false, day21Answer2: true);
            return ret[1];
        }
    }
}