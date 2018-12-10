using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MyAdventOfCodeSolution.Christmas2017
{
    class Day7 : Day
    {
        // ReSharper disable once UnusedMember.Local
        const string TestInput = @"pbga (66)
xhth (57)
ebii (61)
havc (66)
ktlj (57)
fwft (72) -> ktlj, cntj, xhth
qoyq (66)
padx (45) -> pbga, havc, qoyq
tknk (41) -> ugml, padx, fwft
jptl (61)
ugml (68) -> gyxo, ebii, jptl
gyxo (61)
cntj (57)";

        static readonly Regex NameWeight = new Regex(@"^(\w+) \((\d+)\)");
        static readonly Regex AbovePrograms = new Regex(@"-> ([\w, ]+)$");

        class ProgramNode : IEquatable<ProgramNode>
        {
            public ProgramNode(string name)
            {
                Name = name;
            }

            public ISet<ProgramNode> AbovePrograms { get; } = new HashSet<ProgramNode>();
            public string Name { get; }
            public int Weight { get; set; }

            public int TotalWeight => Weight + AbovePrograms.Sum(p => p.TotalWeight);

            public ProgramNode FindOddAboveProgram(out int weightDiff)
            {
                weightDiff = 0;
                if (AbovePrograms.Count < 3) return null;

                int? totalWeight1 = null, totalWeight2 = null;
                int totalWeight1Count = 0;

                ProgramNode oddProgram1 = null, oddProgram2 = null;

                foreach (var aboveProgram in AbovePrograms)
                {
                    var programWeight = aboveProgram.TotalWeight;

                    if (!totalWeight1.HasValue)
                    {
                        totalWeight1 = programWeight;
                        oddProgram1 = aboveProgram;
                        totalWeight1Count++;
                        continue;
                    }

                    if (programWeight == totalWeight1.Value)
                    {
                        if (totalWeight2.HasValue)
                        {
                            //2 is odd we got two of 1
                            weightDiff = totalWeight1.Value - totalWeight2.Value;
                            return oddProgram2;
                        }

                        totalWeight1Count++;
                        continue;
                    }

                    if (totalWeight1Count > 1)
                    {
                        // this one (2) is the odd1
                        weightDiff = totalWeight1.Value - programWeight;
                        return aboveProgram;
                    }

                    if (!totalWeight2.HasValue)
                    {
                        totalWeight2 = programWeight;
                        oddProgram2 = aboveProgram;
                        continue;
                    }

                    // 1 is odd
                    weightDiff = totalWeight2.Value - totalWeight1.Value;
                    return oddProgram1;
                }

                return null;
            }

            public override string ToString()
            {
                return $"{Name} ({Weight})" + (AbovePrograms.Count > 0 ? $" -> {string.Join(", ", AbovePrograms.Select(p => p.Name))}" : "");
            }

            public override bool Equals(object obj)
            {
                if (obj is null) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == GetType() && Equals((ProgramNode) obj);
            }

            public bool Equals(ProgramNode other)
            {
                return string.Equals(Name, other.Name);
            }

            public override int GetHashCode()
            {
                return Name != null ? Name.GetHashCode() : 0;
            }
        }

        public override dynamic Answer1()
        {
            var programs = BuildProgramsDictionary();

            var withAbovePrograms = programs.Values.Where(p => p.AbovePrograms.Count > 0).ToArray();
            ProgramNode root = null;
            foreach (var potentialRoot in withAbovePrograms)
            {
                var isAbove = false;
                foreach (var otherProgram in withAbovePrograms.Where(p=>!p.Equals(potentialRoot)))
                {
                    isAbove = otherProgram.AbovePrograms.Contains(potentialRoot);
                    if (isAbove) break;
                }

                if (isAbove) continue;

                root = potentialRoot;
                break;
            }

            return root?.Name;
        }

        public override dynamic Answer2()
        {
            var programs = BuildProgramsDictionary();

            ProgramNode unbalanced = null;
            var weightDiff = 0;
            // looking for the first unbalanced
            foreach (var program in programs)
            {
                unbalanced = program.Value.FindOddAboveProgram(out weightDiff);
                if (unbalanced != null) break;
            }

            if (unbalanced == null) throw new InvalidOperationException();

            // then drilling down the tree from there
            var lastUnbalanced = unbalanced;
            var lastWeightDiff = weightDiff;
            while (unbalanced != null)
            {
                lastUnbalanced = unbalanced;
                lastWeightDiff = weightDiff;
                unbalanced = unbalanced.FindOddAboveProgram(out weightDiff);
            }

            return $"Node weight should be {lastUnbalanced.Weight + lastWeightDiff}, node is {lastUnbalanced}";
        }

        IDictionary<string, ProgramNode> BuildProgramsDictionary()
        {
            //var input = TestInput.Split(Environment.NewLine);
            var input = GetRawInput();

            IDictionary<string, ProgramNode> programs = new Dictionary<string, ProgramNode>();
            foreach (var row in input)
            {
                var nameWeightMatch = NameWeight.Match(row);
                if (!nameWeightMatch.Success) throw new InvalidOperationException();

                var name = nameWeightMatch.Groups[1].Value;
                if (!programs.TryGetValue(name, out var program))
                {
                    program = new ProgramNode(name);
                    programs.Add(name, program);
                }

                program.Weight = int.Parse(nameWeightMatch.Groups[2].Value);


                var aboveMatch = AbovePrograms.Match(row);
                if (!aboveMatch.Success) continue;

                var aboveNames = aboveMatch.Groups[1].Value.Split(", ");
                foreach (var aboveName in aboveNames)
                {
                    if (!programs.TryGetValue(aboveName, out var aboveProgram))
                    {
                        aboveProgram = new ProgramNode(aboveName);
                        programs.Add(aboveName, aboveProgram);
                    }

                    program.AbovePrograms.Add(aboveProgram);
                }
            }

            return programs;
        }
    }
}