using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

// ReSharper disable UnusedMember.Local

namespace MyAdventOfCodeSolution.Christmas2018
{
    class Day16 : Day
    {
        interface IOperation
        {
            string Name { get; }
            int[] ApplyToRegisters(int[] registers, int inputA, int inputB, int output);
            bool OperationMatches(int[] registersBefore, int[] input, int[] registersAfter);
        }

        abstract class Operation : IOperation, IEquatable<Operation>
        {
            public string Name { get; }

            protected readonly Func<int, int, int> Function;

            protected Operation(string name, Func<int, int, int> function)
            {
                Function = function;
                Name = name;
            }

            public abstract int[] ApplyToRegisters(int[] registers, int inputA, int inputB, int output);

            public bool OperationMatches(int[] registersBefore, int[] input, int[] registersAfter)
            {
                var opRegistersAfter = ApplyToRegisters(registersBefore, input[1], input[2], input[3]);
                return opRegistersAfter.SequenceEqual(registersAfter);
            }

            public override bool Equals(object obj)
            {
                if (obj is null) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == GetType() && Equals((Operation) obj);
            }

            public bool Equals(Operation other)
            {
                if (other is null) return false;
                return ReferenceEquals(this, other) || string.Equals(Name, other.Name);
            }

            public override int GetHashCode()
            {
                return Name.GetHashCode();
            }

            public override string ToString() => Name;
        }

        class RegisterRegisterOperation : Operation
        {
            public RegisterRegisterOperation(string name, Func<int, int, int> function) : base(name, function)
            {
            }

            public override int[] ApplyToRegisters(int[] registers, int inputA, int inputB, int output)
            {
                var ret = (int[]) registers.Clone();
                ret[output] = Function(registers[inputA], registers[inputB]);
                return ret;
            }
        }

        class RegisterImmediateOperation : Operation
        {
            public RegisterImmediateOperation(string name, Func<int, int, int> function) : base(name, function)
            {
            }

            public override int[] ApplyToRegisters(int[] registers, int inputA, int inputB, int output)
            {
                var ret = (int[])registers.Clone();
                ret[output] = Function(registers[inputA], inputB);
                return ret;
            }
        }

        class ImmediateRegisterOperation : Operation
        {
            public ImmediateRegisterOperation(string name, Func<int, int, int> function) : base(name, function)
            {
            }

            public override int[] ApplyToRegisters(int[] registers, int inputA, int inputB, int output)
            {
                var ret = (int[])registers.Clone();
                ret[output] = Function(inputA, registers[inputB]);
                return ret;
            }
        }

        static readonly IOperation[] Operations =
        {
            new RegisterRegisterOperation("addr", (a, b) => a + b),
            new RegisterImmediateOperation("addi", (a, b) => a + b),

            new RegisterRegisterOperation("mulr", (a, b) => a * b), 
            new RegisterImmediateOperation("muli", (a, b) => a * b),

            new RegisterRegisterOperation("banr", (a, b) => a & b),
            new RegisterImmediateOperation("bani", (a, b) => a & b),

            new RegisterRegisterOperation("borr", (a, b) => a | b),
            new RegisterImmediateOperation("bori", (a, b) => a | b),

            new RegisterRegisterOperation("setr", (a, b) => a),
            new ImmediateRegisterOperation("seti", (a, b) => a),

            new ImmediateRegisterOperation("gtir", (a, b) => a > b ? 1 : 0),
            new RegisterImmediateOperation("gtri", (a, b) => a > b ? 1 : 0),
            new RegisterRegisterOperation("gtrr", (a, b) => a > b ? 1 : 0),

            new ImmediateRegisterOperation("eqir", (a, b) => a == b ? 1 : 0),
            new RegisterImmediateOperation("eqri", (a, b) => a == b ? 1 : 0),
            new RegisterRegisterOperation("eqrr", (a, b) => a == b ? 1 : 0)
        };

        static IEnumerable<IOperation> BehavesLike(int[] registersBefore, int[] input, int[] registersAfter)
        {
            return Operations.Where(op => op.OperationMatches(registersBefore, input, registersAfter));
        }

        static readonly Regex BeforeRegex = new Regex(@"Before: +\[(\d+), (\d+), (\d+), (\d+)\]");
        static readonly Regex InputRegex = new Regex(@"(\d+) (\d+) (\d+) (\d+)");
        static readonly Regex AfterRegex = new Regex(@"After: +\[(\d+), (\d+), (\d+), (\d+)\]");

        public override dynamic Answer1()
        {
            int[] registersBefore = null;
            int[] input = null;

            var cnt = 0;
            foreach (var line in GetRawInput())
            {
                var match = BeforeRegex.Match(line);
                if (match.Success)
                {
                    registersBefore = match.Groups.Skip(1).Select(g => int.Parse(g.Value)).ToArray();
                    continue;
                }

                match = InputRegex.Match(line);
                if (match.Success)
                {
                    input = match.Groups.Skip(1).Select(g => int.Parse(g.Value)).ToArray();
                    continue;
                }

                match = AfterRegex.Match(line);
                if (!match.Success) continue;

                // we don't have a full set
                if (registersBefore == null || input == null) throw new InvalidOperationException();

                var registersAfter = match.Groups.Skip(1).Select(g => int.Parse(g.Value)).ToArray();
                if (BehavesLike(registersBefore, input, registersAfter).Count() > 2) cnt++;

                registersBefore = input = null;
            }

            return cnt;
        }

        public override dynamic Answer2()
        {
            var opcodes = GetOperationMap();

            var startRunning = false;
            var seqEmptyCount = 0;
            // running the program
            var registers = new int[4];
            foreach (var line in GetRawInput())
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    seqEmptyCount++;
                    continue;
                }
                if (seqEmptyCount >= 3) startRunning = true;
                seqEmptyCount = 0;

                if (!startRunning) continue;

                var match = InputRegex.Match(line);
                if (!match.Success) throw new InvalidOperationException();

                var input = match.Groups.Skip(1).Select(g => int.Parse(g.Value)).ToArray();
                var op = opcodes[input[0]];
                registers = op.ApplyToRegisters(registers, input[1], input[2], input[3]);
            }

            return registers[0];
        }

        IOperation[] GetOperationMap()
        {
            var opcodes = new IOperation[16];
            var foundCodes = new HashSet<int>();
            var foundOps = new HashSet<string>();

            int[] registersBefore = null;
            int[] input = null;

            while (foundOps.Count < 16)
                foreach (var line in GetRawInput())
                {
                    var match = BeforeRegex.Match(line);
                    if (match.Success)
                    {
                        registersBefore = match.Groups.Skip(1).Select(g => int.Parse(g.Value)).ToArray();
                        continue;
                    }

                    match = InputRegex.Match(line);
                    if (match.Success)
                    {
                        input = match.Groups.Skip(1).Select(g => int.Parse(g.Value)).ToArray();
                        continue;
                    }

                    match = AfterRegex.Match(line);
                    if (!match.Success) continue;

                    // we don't have a full set
                    if (registersBefore == null || input == null) throw new InvalidOperationException();

                    var opcode = input[0];
                    if (!foundCodes.Contains(opcode))
                    {
                        var registersAfter = match.Groups.Skip(1).Select(g => int.Parse(g.Value)).ToArray();
                        var all = BehavesLike(registersBefore, input, registersAfter).ToArray();
                        var behavesLike = all.Where(o => !foundOps.Contains(o.Name)).ToArray();
                        if (behavesLike.Length != 1) continue;

                        opcodes[opcode] = behavesLike[0];
                        foundCodes.Add(opcode);
                        foundOps.Add(behavesLike[0].Name);
                    }

                    registersBefore = input = null;
                }

            return opcodes;
        }

        void Test()
        {
            var regBefore = new[] {3, 2, 1, 1};
            var input = new[] {9, 2, 1, 2};
            var regAfter = new[] {3, 2, 2, 1};

            var o = BehavesLike(regBefore, input, regAfter).ToArray();

        }
    }
}