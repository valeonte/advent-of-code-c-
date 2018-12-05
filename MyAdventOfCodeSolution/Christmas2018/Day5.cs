using System;
using System.Collections.Generic;
using System.Linq;

namespace MyAdventOfCodeSolution.Christmas2018
{
    class Day5 : Day
    {
        string _polymer;
        string Polymer => _polymer ?? (_polymer = GetRawInput().Single());
        
        public override dynamic Answer1()
        {
            return GetUnitCountAfterReaction();
        }

        public override dynamic Answer2()
        {
            var minCount = int.MaxValue;
            for (var unit = 'a'; unit <= 'z'; unit++)
            {
                var unitCount = GetUnitCountAfterReaction(unit);
                if (unitCount >= minCount) continue;

                minCount = unitCount;
            }

            return minCount;
        }

        int GetUnitCountAfterReaction(char? excludeUnit = null)
        {
            var ll = new LinkedList<char>(Polymer);
            var curNode = GetNodeRemovingExcluded(ll, ll.First, excludeUnit);
            if (curNode == null) return 0;

            var nextNode = GetNodeRemovingExcluded(ll, curNode.Next, excludeUnit);
            if (nextNode == null) return 1;

            do
            {
                if (UnitsTrigger(curNode.Value, nextNode.Value))
                {
                    var newCurNode = curNode.Previous ?? GetNodeRemovingExcluded(ll, nextNode.Next, excludeUnit);
                    ll.Remove(curNode);
                    ll.Remove(nextNode);
                    curNode = newCurNode;
                }
                else
                {
                    curNode = nextNode;
                }

                nextNode = GetNodeRemovingExcluded(ll, curNode.Next, excludeUnit);
            } while (nextNode != null);

            return ll.Count;
        }

        static LinkedListNode<char> GetNodeRemovingExcluded(LinkedList<char> ll, LinkedListNode<char> node, char? excludeUnit)
        {
            if (!excludeUnit.HasValue) return node;

            var ret = node;
            while (ret != null && char.ToLower(ret.Value) == excludeUnit.Value)
            {
                var newRet = ret.Next;
                ll.Remove(ret);
                ret = newRet;
            }

            return ret;
        }

        static bool UnitsTrigger(char unit1, char unit2) => (char.IsLower(unit1) && char.IsUpper(unit2) || char.IsUpper(unit1) && char.IsLower(unit2))
                                                            && char.ToUpper(unit1) == char.ToUpper(unit2);
    }
}