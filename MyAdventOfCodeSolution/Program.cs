using System;
using MyAdventOfCodeSolution.Christmas2018;

namespace MyAdventOfCodeSolution
{
    class Program
    {
        static readonly IDay[] DaysToChristmas =
        {
            new Day1(), new Day2(), new Day3()
        };

        static void Main()
        {
            foreach (var day in DaysToChristmas)
            {
                var dayName = day.GetType().Name;
                Console.WriteLine($"{dayName}, Answer 1: {day.Answer1()}");
                Console.WriteLine($"{dayName}, Answer 2: {day.Answer2()}");
            }

            Console.ReadKey();
        }
    }
}
