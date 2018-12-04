using System.IO;

namespace MyAdventOfCodeSolution.Christmas2018
{
    abstract class Day : IDay
    {
        protected string[] GetRawInput()
        {
            var inputFile = $@".\Christmas2018\inputs\{GetType().Name}.txt";
            return File.ReadAllLines(inputFile);
        }

        public virtual dynamic Answer1() => "Not Implemented";

        public virtual dynamic Answer2() => "Not Implemented";
    }
}