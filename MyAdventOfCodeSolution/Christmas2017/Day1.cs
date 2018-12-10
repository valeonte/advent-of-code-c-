using System.Collections.Generic;
using System.Linq;

namespace MyAdventOfCodeSolution.Christmas2017
{
    class Day1 : Day
    {
        string _input;
        string Input => _input ?? (_input = GetRawInput().First());

        static int GetSpecialSum(string inp, int stepsForward = -1)
        {
            var ret = 0;
            var input = inp.ToArray();

            if (stepsForward == -1) stepsForward = input.Length / 2;

            for (var i = 0; i < input.Length; i++)
            {
                var c = inp[i];
                var comp = GetElementFromStepsForward(input, i, stepsForward);

                if (c != comp) continue;

                ret += (int) char.GetNumericValue(c);
            }

            return ret;
        }

        static T GetElementFromStepsForward<T>(IReadOnlyList<T> input, int idx, int stepsForward)
        {
            while (idx + stepsForward >= input.Count)
            {
                stepsForward -= input.Count - idx;
                idx = 0;
            }

            return input[idx + stepsForward];
        }

        public override dynamic Answer1()
        {
            return GetSpecialSum(Input, 1);
        }

        public override dynamic Answer2()
        {
            //return $"{GetSpecialSum("1212")} {GetSpecialSum("1221")} {GetSpecialSum("123425")} {GetSpecialSum("123123")} {GetSpecialSum("12131415")}";
            return GetSpecialSum(Input);
        }
    }
}