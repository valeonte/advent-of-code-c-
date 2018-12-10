using System;
using System.Linq;

namespace MyAdventOfCodeSolution.Christmas2017
{
    class Day2 : Day
    {
        string[] _input;
        string[] Input => _input ?? (_input = GetRawInput().ToArray());

        static int GetRowMinMaxDiff(string row)
        {
            var min = int.MaxValue;
            var max = int.MinValue;
            foreach (var num in row.Split("\t").Select(int.Parse))
            {
                if (min > num) min = num;
                if (max < num) max = num;
            }

            return max - min;
        }

        static int GetRowEvenDivisionResult(string row)
        {
            var nums = row.Split("\t").Select(int.Parse).ToArray();

            for (var i = 0; i < nums.Length-1; i++)
            for (var j = i + 1; j < nums.Length; j++)
            {
                var evenRes = GetEvenDivisionResultOrMinus1(nums[i], nums[j]);
                if (evenRes > -1) return evenRes;
            }

            throw new InvalidOperationException("Failed to find evenly dividable numbers!");
        }

        static int GetEvenDivisionResultOrMinus1(int num1, int num2)
        {
            var result = num1 > num2
                ? Math.DivRem(num1, num2, out var remainder)
                : Math.DivRem(num2, num1, out remainder);

            return remainder == 0 ? result : -1;
        }

        public override dynamic Answer1()
        {
            return Input.Sum(GetRowMinMaxDiff);
        }

        public override dynamic Answer2()
        {
            return Input.Sum(GetRowEvenDivisionResult);
        }
    }
}