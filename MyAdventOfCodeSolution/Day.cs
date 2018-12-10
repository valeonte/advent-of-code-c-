using System.IO;

namespace MyAdventOfCodeSolution
{
    abstract class Day : IDay
    {
        protected string[] GetRawInput()
        {
            var t = GetType();
            var year = t.Namespace.Substring(t.Namespace.Length - 4);
            var inputFile = $@".\Christmas{year}\inputs\{t.Name}.txt";
            return File.ReadAllLines(inputFile);
        }

        public virtual dynamic Answer1() => "Not Implemented";

        public virtual dynamic Answer2() => "Not Implemented";
    }
}