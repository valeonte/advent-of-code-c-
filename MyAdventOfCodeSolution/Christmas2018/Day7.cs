using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MyAdventOfCodeSolution.Christmas2018
{
    class Day7 : Day
    {
        const int Workers = 5;

        class TaskNode : IEquatable<TaskNode>
        {
            const int ExtraDurationSeconds = 60;

            public TaskNode(char id)
            {
                if (!char.IsUpper(id)) throw new InvalidOperationException($"Lower case node id: {id}");

                Id = id;
                Prerequisites = new HashSet<TaskNode>();
            }

            public char Id { get; }

            public ISet<TaskNode> Prerequisites { get; }

            public bool IsPickedUp { get; set; }

            public int Duration => Id - 64 + ExtraDurationSeconds;

            public override bool Equals(object obj)
            {
                if (obj is null) return false;
                return obj is TaskNode node && Equals(node);
            }

            public bool Equals(TaskNode other)
            {
                return Id == other.Id;
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }

        static readonly Regex Parser = new Regex(@"^Step (\w) must be finished before step (\w) can begin\.$");

        IDictionary<char, TaskNode> GetNodes()
        {
            var input = GetRawInput();
            //var input = new[]
            //{
            //    "Step C must be finished before step A can begin.",
            //    "Step C must be finished before step F can begin.",
            //    "Step A must be finished before step B can begin.",
            //    "Step A must be finished before step D can begin.",
            //    "Step B must be finished before step E can begin.",
            //    "Step D must be finished before step E can begin.",
            //    "Step F must be finished before step E can begin."
            //};
            var ret = new Dictionary<char, TaskNode>();
            foreach (var s in input)
            {
                var match = Parser.Match(s);
                if (!match.Success) throw new InvalidOperationException($"Invalid string in input: {s}");


                var prereq = match.Groups[1].Value[0];
                var main = match.Groups[2].Value[0];

                if (!ret.TryGetValue(prereq, out var prereqNode))
                {
                    prereqNode = new TaskNode(prereq);
                    ret.Add(prereq, prereqNode);
                }

                if (!ret.TryGetValue(main, out var mainNode))
                {
                    mainNode = new TaskNode(main);
                    ret.Add(main, mainNode);
                }

                mainNode.Prerequisites.Add(prereqNode);
            }

            return ret;
        }

        public override dynamic Answer1()
        {
            return new string(GetNodesInOrder().Select(n => n.Id).ToArray());
        }

        IEnumerable<TaskNode> GetNodesInOrder()
        {
            var workingSet = GetNodes();

            while (workingSet.Count > 0)
            {
                var nextAvailable = workingSet.Values.Where(n => n.Prerequisites.Count == 0).OrderBy(n => n.Id).FirstOrDefault();
                if (nextAvailable == null) throw new InvalidOperationException("No available node!");

                yield return nextAvailable;
                workingSet.Remove(nextAvailable.Id);

                foreach (var node in workingSet.Values)
                {
                    node.Prerequisites.Remove(nextAvailable);
                }
            }
        }

        class Worker
        {
            public TaskNode TaskNode { get; set; }
            public int SecondsSpent { get; set; }

            public bool IsIdle => TaskNode == null;
            public int? SecondsLeft => TaskNode?.Duration - SecondsSpent;
        }

        public override dynamic Answer2()
        {
            var workingSet = GetNodes();

            var workers = new Worker[Workers];
            for (var i = 0; i < Workers; i++)
            {
                workers[i] = new Worker();
            }

            void CompleteWorkerTask(Worker worker)
            {
                workingSet.Remove(worker.TaskNode.Id);

                foreach (var node in workingSet.Values)
                {
                    node.Prerequisites.Remove(worker.TaskNode);
                }

                worker.TaskNode = null;
                worker.SecondsSpent = 0;
            }

            var secondsTaken = 0;
            while (workingSet.Count > 0)
            {
                secondsTaken++;
                foreach (var worker in workers.Where(w => !w.IsIdle).ToArray())
                {
                    worker.SecondsSpent++;
                    if (worker.SecondsLeft > 0) continue;

                    CompleteWorkerTask(worker);
                }

                var workersAvailable = workers.Where(w => w.IsIdle).ToArray();
                if (workersAvailable.Length == 0) continue;

                var availableTasks = workingSet.Values.Where(n => !n.IsPickedUp && n.Prerequisites.Count == 0).OrderBy(n => n.Id).ToArray();
                if (availableTasks.Length == 0) continue;

                var maxAssignments = Math.Min(workersAvailable.Length, availableTasks.Length);
                for (var i = 0; i < maxAssignments; i++)
                {
                    var worker = workersAvailable[i];
                    var task = availableTasks[i];

                    worker.TaskNode = task;
                    task.IsPickedUp = true;
                }
            }

            return secondsTaken - 1; // -1 because last second is not actually spent
        }

    }
}