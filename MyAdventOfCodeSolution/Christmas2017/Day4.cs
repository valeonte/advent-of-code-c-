using System.Collections.Generic;
using System.Linq;

namespace MyAdventOfCodeSolution.Christmas2017
{
    class Day4 : Day
    {
        public override dynamic Answer1()
        {
            var valid = 0;
            foreach (var passphrase in GetRawInput())
            {
                var words = passphrase.Split(' ');
                var workSet = new HashSet<string>(words.Length);
                var isValid = true;
                foreach (var word in words)
                {
                    isValid = workSet.Add(word);
                    if (!isValid) break;
                }

                if (isValid) valid++;
            }

            return valid;
        }

        static string OrderChars(string word) => new string(word.OrderBy(c => c).ToArray());

        public override dynamic Answer2()
        {
            var valid = 0;
            foreach (var passphrase in GetRawInput())
            {
                var words = passphrase.Split(' ');
                var workSet = new HashSet<string>(words.Length);
                var isValid = true;
                foreach (var word in words.Select(OrderChars))
                {
                    isValid = workSet.Add(word);
                    if (!isValid) break;
                }

                if (isValid) valid++;
            }

            return valid;
        }
    }
}