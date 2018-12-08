using System.Collections.Generic;
using System.Linq;

namespace MyAdventOfCodeSolution.Christmas2018
{
    class Day8 : Day
    {
        class Node
        {
            public Node(int childrenCount, int metadataCount)
            {
                Children = new Node[childrenCount];
                Metadata = new int[metadataCount];
            }

            public Node[] Children { get; }
            public int[] Metadata { get; }

            public int MetadataSum() => Metadata.Sum() + Children.Select(n => n.MetadataSum()).Sum();

            public int Value() => Children.Length == 0 ? Metadata.Sum() : Metadata.Where(idx => idx <= Children.Length).Sum(idx => Children[idx - 1].Value());
        }

        public override dynamic Answer1()
        {
            var node = ExtractNode(InputEnumerator());
            return node.MetadataSum();
        }

        public override dynamic Answer2()
        {
            var node = ExtractNode(InputEnumerator());
            return node.Value();
        }

        IEnumerator<int> InputEnumerator()
        {
            //var input = "2 3 0 3 10 11 12 1 1 0 1 99 2 1 1 2";
            var input = GetRawInput().First();

            var curNum = 0;
            foreach (var ch in input)
            {
                if (ch == ' ')
                {
                    yield return curNum;
                    curNum = 0;
                }
                else
                {
                    curNum = 10 * curNum + (int) char.GetNumericValue(ch);
                }
            }

            // last one
            yield return curNum;
        }

        static Node ExtractNode(IEnumerator<int> numberEnum)
        {
            numberEnum.MoveNext();
            var childrenCount = numberEnum.Current;
            numberEnum.MoveNext();
            var metadataCount = numberEnum.Current;

            var node = new Node(childrenCount, metadataCount);

            for (var i = 0; i < childrenCount; i++)
            {
                node.Children[i] = ExtractNode(numberEnum);
            }

            for (var i = 0; i < metadataCount; i++)
            {
                numberEnum.MoveNext();
                node.Metadata[i] = numberEnum.Current;
            }

            return node;
        }
    }
}