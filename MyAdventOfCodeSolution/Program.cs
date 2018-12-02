using System;

namespace MyAdventOfCodeSolution
{
    class Program
    {
        static void Main()
        {
            var d1 = new Day1();
            Console.WriteLine($"Day 1, Answer 1: {d1.Answer1()}");
            Console.WriteLine($"Day 1, Answer 2: {d1.Answer2(500)}");

            var d2 = new Day2();
            Console.WriteLine($"Day 2, Answer 1: {d2.Answer1()}");
            Console.WriteLine($"Day 2, Answer 2: {d2.Answer2()}");


            Console.ReadKey();
        }
    }
}
