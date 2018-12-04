using System;
using System.Linq;
using MyAdventOfCodeSolution.Christmas2018;

namespace MyAdventOfCodeSolution
{
    class Program
    {
        static void Main()
        {
            var dayTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IDay).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

            foreach (var dayType in dayTypes)
            {
                var day = (IDay) Activator.CreateInstance(dayType);

                var dayName = day.GetType().Name;
                Console.WriteLine($"{dayName}, Answer 1: {day.Answer1()}");
                Console.WriteLine($"{dayName}, Answer 2: {day.Answer2()}");
            }

            Console.ReadKey();
        }
    }
}
