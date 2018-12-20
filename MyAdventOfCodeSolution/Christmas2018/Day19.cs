using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

// ReSharper disable UnusedMember.Local

namespace MyAdventOfCodeSolution.Christmas2018
{
    class Day19 : Day
    {
        static readonly IDictionary<string, Day16.IOperation> Operations = Day16.Operations.ToDictionary(x => x.Name);

        class Instruction
        {
            public Instruction(string instruction)
            {
                var parts = instruction.Split(' ');
                if (parts[0].Equals("#ip")) return;

                Operation = Operations[parts[0]];
                InputA = long.Parse(parts[1]);
                InputB = long.Parse(parts[2]);
                Output = long.Parse(parts[3]);
            }

            Day16.IOperation Operation { get; }
            long InputA { get; }
            long InputB { get; }
            long Output { get; }

            public long[] Apply(long[] registers) => Operation.ApplyToRegisters(registers, InputA, InputB, Output);

            public override string ToString() => $"{Operation} {InputA} {InputB} {Output}";
        }

        static long[] RunProgram(IReadOnlyList<string> instructions, long[] registers)
        {
            var ret = (long[])registers.Clone();

            // first command is setting the ip register
            var instruction = instructions[0];
            var parts = instruction.Split(' ');
            if (!parts[0].Equals("#ip")) throw new InvalidOperationException();
            var ipRegister = long.Parse(parts[1]);
            var ip = ret[ipRegister];

            long cmdCount = 0;
            var sw = Stopwatch.StartNew();

            var processedInstructions = instructions.Select(i => new Instruction(i)).ToArray();

            while (ip + 1 < instructions.Count)
            {
                cmdCount++;

                // pick the corrent instruction
                var instr = processedInstructions[(int)ip + 1];
                
                // update the ip register with the ip value
                ret[ipRegister] = ip;

                if (ip == 9 && ret[1] == 10_551_311 && ret[5] < ret[1])
                {
                    if (ret[2] < 2 || ret[1] % ret[2] != 0)
                        ret[5] = ret[1];
                    else
                        ret[5] = ret[1] / ret[2];
                    // we noticed that this condition occurs repeatedly, and every time register 5 is increased by 1,
                    // until it reaches the value of register 1 which is 10m. We can speed this up by setting it eagerly
                    // and saving ourselves from tens of millions of operations
                    //ret[5] = ret[1];
                    //ret[2] = ret[1];
                }

                //if (ip == 13 && ret[1] == 10_551_311 && ret[5] > ret[1] && ret[2] > 100 && ret[2] < ret[1] - 100)
                //{
                //    ret[2] = ret[1]-100;
                //}

                //var log1 = $"ip={ip} [{string.Join(", ", ret)}]";

                // execute the command
                //parts = instruction.Split(' ');
                //var op = Operations[parts[0]];
                //ret = op.ApplyToRegisters(ret, long.Parse(parts[1]), long.Parse(parts[2]), long.Parse(parts[3]));
                ret = instr.Apply(ret);

                //Console.WriteLine($"{log1} {instr} [{string.Join(", ", ret)}]");

                // set the instruction point to the value of the instruction register
                ip = ret[ipRegister];

                // increase ip
                ip++;

                if (cmdCount % 100000 == 0)
                {
                    //Console.WriteLine($"{cmdCount} commands, {cmdCount/(sw.ElapsedMilliseconds/1000.0)} commands per second");
                    //Console.WriteLine($"{instr} [{string.Join(", ", ret)}]");
                }
            }

            return ret;
        }
        
        const string TestInput = @"#ip 0
seti 5 0 1
seti 6 0 2
addi 0 1 0
addr 1 2 3
setr 1 0 0
seti 8 0 4
seti 9 0 5";

        public override dynamic Answer1()
        {
            //var input = TestInput.Split(Environment.NewLine);
            var input = GetRawInput();

            var reg = RunProgram(input, new long[6]);

            return reg[0];
        }

        public override dynamic Answer2()
        {
            // After many many many runs and step throughs, we realised that:
            // - Number 10_551_311 was put in register 1
            // - Then the program did enumerate 1 to 10_551_311 in the registers 2 and 5 testing all the products
            //   if they are equal to 10_551_311
            // - if they were, it added register 2 to register 0
            // - hence the answer is the sum of all the integer divisors of 10_551_311
            // in the process we super-optimised the execution peaking at 16m operations per second
            // but apparently this wasn't needed
            return Enumerable.Range(1, 10_551_311).Where(i => 10_551_311 % i == 0).Sum(); ;
        }
    }
}