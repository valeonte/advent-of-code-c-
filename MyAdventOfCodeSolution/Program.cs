using System;
using System.Diagnostics;
using System.Linq;

namespace MyAdventOfCodeSolution
{
    class Program
    {
        static void Main(string[] args)
        {
            int year;
            if (args.Length == 0)
            {
                year = DateTime.UtcNow.Month == 12 ? DateTime.UtcNow.Year : DateTime.UtcNow.Year - 1;
            }
            else
            {
                year = int.Parse(args[0]);
            }

            var dayTypes =
                from asm in AppDomain.CurrentDomain.GetAssemblies()
                from type in asm.GetTypes()
                where !type.IsInterface && !type.IsAbstract
                where typeof(IDay).IsAssignableFrom(type)
                where type.Namespace.Equals($"{nameof(MyAdventOfCodeSolution)}.Christmas{year}")
                select type;

            var sw = new Stopwatch();
            foreach (var dayType in dayTypes.OrderByDescending(t => int.Parse(t.Name.Replace("Day", ""))))
            {
                var day = (IDay) Activator.CreateInstance(dayType);

                var dayName = day.GetType().Name;

                sw.Restart();
                Console.WriteLine($"{dayName}, Answer 1: {day.Answer1()}");
                Console.WriteLine($"{dayName}, Answer 2: {day.Answer2()}");
                Console.WriteLine($"{dayName}, Timing: {sw.ElapsedMilliseconds}ms");
                Console.WriteLine();
            }

            Console.ReadKey();
        }
    }
}
