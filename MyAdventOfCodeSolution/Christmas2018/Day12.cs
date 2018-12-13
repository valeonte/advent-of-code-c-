using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
// ReSharper disable AssignNullToNotNullAttribute
// ReSharper disable PossibleNullReferenceException

namespace MyAdventOfCodeSolution.Christmas2018
{
    class Day12 : Day
    {
        // ReSharper disable once UnusedMember.Local
        const string TestInput = @"initial state: #..#.#..##......###...###

...## => #
..#.. => #
.#... => #
.#.#. => #
.#.## => #
.##.. => #
.#### => #
#.#.# => #
#.### => #
##.#. => #
##.## => #
###.. => #
###.# => #
####. => #";

        static readonly Regex InitialStateRegex = new Regex(@"^initial state: ([#\.]+)$");
        static readonly Regex RuleRegex = new Regex(@"^([#\.]{5}) => ([#\.])$");

        static IEnumerable<bool> StateStringToBoolEnum(string state)
        {
            foreach (var ch in state)
            {
                yield return ch == '#';
            }
        }

        public override dynamic Answer1()
        {
            var potRow = GetPotRowFromInput();

            for (var i = 0; i < 20; i++)
            {
                //Console.WriteLine($"{i}: {potRow} - {potRow.SumPots()}");

                potRow.ForwardGeneration();
            }
            //Console.WriteLine($"--: {potRow} - {potRow.SumPots()}");

            return potRow.SumPots();
        }

        public override dynamic Answer2()
        {
            var potRow = GetPotRowFromInput();

            // We noticed that after iteration 168 the sum was increasing by 75 on every step
            // so we do first 200 iterations, and calculate it from there
            var lastSumPot = 0;
            for (var i = 0; i < 200; i++)
            {
                var sumPot = potRow.SumPots();
                //Console.WriteLine($"{i:D5}: {potRow} - {sumPot}, {sumPot - lastSumPot}");
                lastSumPot = sumPot;
                potRow.ForwardGeneration();

                //if (i % 200 == 0) Console.ReadKey();
            }

            var finalSum = (50_000_000_000 - 200) * 75 + potRow.SumPots();

            return finalSum;
        }

        PotRow GetPotRowFromInput()
        {
            //var input = TestInput.Split(Environment.NewLine);
            var input = GetRawInput();

            var potRow = new PotRow();
            foreach (var line in input)
            {
                var initialStateMatch = InitialStateRegex.Match(line);
                if (initialStateMatch.Success) potRow.PopulateRow(StateStringToBoolEnum(initialStateMatch.Groups[1].Value));

                var ruleRowMatch = RuleRegex.Match(line);
                if (ruleRowMatch.Success)
                    potRow.AddRule(new PotRule(StateStringToBoolEnum(ruleRowMatch.Groups[1].Value), ruleRowMatch.Groups[2].Value[0] == '#'));
            }

            return potRow;
        }

        struct Pot
        {
            public Pot(int number, bool hasPlant = false)
            {
                Number = number;
                HasPlant = hasPlant;
            }

            public int Number { get; }
            public bool HasPlant { get; }

            public override string ToString()
            {
                return $"{Number}: {(HasPlant ? '#' : '.')}";
            }
        }

        class PotRule
        {
            readonly bool[] _currentStatus;
            readonly bool _nextPlant;

            public PotRule(IEnumerable<bool> currentStatus, bool nextPlant)
            {
                _currentStatus = currentStatus.ToArray();
                _nextPlant = nextPlant;
            }

            public bool IsMatch(IEnumerable<Pot> pots, out bool nextPlant)
            {
                nextPlant = _nextPlant;
                return _currentStatus.SequenceEqual(pots.Select(p => p.HasPlant));
            }

            public override string ToString()
            {
                return $"{new string(_currentStatus.Select(s => s ? '#' : '.').ToArray())} => {(_nextPlant ? '#' : '.')}";
            }
        }

        class PotRow
        {
            LinkedList<Pot> _potRow = new LinkedList<Pot>();
            readonly IList<PotRule> _rules = new List<PotRule>();
            int _minNumber, _maxNumber;
            
            public void PopulateRow(IEnumerable<bool> initialState)
            {
                _minNumber = -1;
                _potRow.AddFirst(new Pot(-1));
                var lastNode = _potRow.First;
                
                var cnt = 0;
                foreach (var potPlant in initialState)
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    _potRow.AddAfter(lastNode, new Pot(cnt, potPlant));
                    lastNode = lastNode.Next;

                    cnt++;
                }

                _maxNumber = cnt - 1;

                EnsureEnoughSpaceAtStart();
                EnsureEnoughSpaceAtEnd();
            }

            public int SumPots() => _potRow.Where(l => l.HasPlant).Sum(p => p.Number);

            public void AddRule(PotRule rule)
            {
                _rules.Add(rule);
            }

            static IEnumerable<LinkedListNode<Pot>> EnumerateForward(LinkedListNode<Pot> startNode, int numberOfPots)
            {
                var nextNode = startNode;
                for (var i = 0; i < numberOfPots; i++)
                {
                    yield return nextNode;

                    nextNode = nextNode.Next;
                    if (nextNode == null) yield break;
                }
            }

            public void ForwardGeneration()
            {
                var curNode = _potRow.First;

                var newRow = new LinkedList<Pot>(EnumerateForward(curNode, 2).Select(n => n.Value));
                var newRowLastNode = newRow.Last;

                do
                {
                    var nodeWindow = EnumerateForward(curNode, 5).Select(n => n.Value).ToArray();
                    if (nodeWindow.Length < 5)
                    {
                        // reached the end, appending the remaining pots and breaking
                        for (var i = 2; i < nodeWindow.Length; i++)
                        {
                            newRow.AddAfter(newRowLastNode, nodeWindow[i]);
                            newRowLastNode = newRowLastNode.Next;
                        }
                        break;
                    }

                    var gotMatch = false;
                    foreach (var rule in _rules)
                    {
                        gotMatch = rule.IsMatch(nodeWindow, out var plant);
                        if (!gotMatch) continue;

                        newRow.AddAfter(newRowLastNode, new Pot(nodeWindow[2].Number, plant));
                        break;
                    }

                    if (!gotMatch) newRow.AddAfter(newRowLastNode, new Pot(nodeWindow[2].Number));

                    curNode = curNode.Next;
                    
                    newRowLastNode = newRowLastNode.Next;

                } while (true);

                _potRow = newRow;

                EnsureEnoughSpaceAtStart();
                EnsureEnoughSpaceAtEnd();
            }

            void EnsureEnoughSpaceAtEnd()
            {
                var lastPlant = _potRow.Last;
                while (!lastPlant.Value.HasPlant)
                {
                    lastPlant = lastPlant.Previous;
                }

                if (_maxNumber - lastPlant.Value.Number == 4) return;
                if (_maxNumber - lastPlant.Value.Number < 4)
                {
                    var lastNode = lastPlant;
                    for (var i = 0; i < 4; i++)
                    {
                        if (lastNode.Next == null)
                        {
                            _maxNumber = lastNode.Value.Number + 1;
                            _potRow.AddAfter(lastNode, new Pot(_maxNumber));
                        }

                        lastNode = lastNode.Next;
                    }
                    return;
                }

                _maxNumber = lastPlant.Value.Number + 4;
                var nextNode = lastPlant;
                while (nextNode.Next != null)
                {
                    if (nextNode.Value.Number < _maxNumber) nextNode = nextNode.Next;
                    else _potRow.Remove(nextNode.Next);
                }
            }

            void EnsureEnoughSpaceAtStart()
            {
                var firstPlant = _potRow.First;
                while (!firstPlant.Value.HasPlant)
                {
                    firstPlant = firstPlant.Next;
                }

                if (firstPlant.Value.Number - _minNumber == 4) return;
                if (firstPlant.Value.Number - _minNumber < 4)
                {
                    var lastNode = firstPlant;
                    for (var i = 0; i < 4; i++)
                    {
                        if (lastNode.Previous == null)
                        {
                            _minNumber = lastNode.Value.Number - 1;
                            _potRow.AddBefore(lastNode, new Pot(_minNumber));
                        }

                        lastNode = lastNode.Previous;
                    }
                    return;
                }

                _minNumber = firstPlant.Value.Number - 4;
                var prevNode = firstPlant;
                while (prevNode.Previous != null)
                {
                    if (prevNode.Value.Number > _minNumber) prevNode = prevNode.Previous;
                    else _potRow.Remove(prevNode.Previous);
                }
            }

            public override string ToString()
            {
                return new string(_potRow.Select(p => p.HasPlant ? '#' : '.').ToArray());
            }
        }
    }
}