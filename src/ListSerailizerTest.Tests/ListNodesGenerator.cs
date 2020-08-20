using System;
using System.Linq;
using AutoBogus;
using ListSerializerTest;

namespace ListSerailizerTest.Tests
{
    public static class ListNodesGenerator
    {
        private static readonly Random _rnd = new Random();

        public static ListNode GenerateList(int size)
        {
            var nodes = new ListNode[size];

            for (var i = 0; i < size; i++)
            {
                nodes[i] = new ListNode
                {
                    Data = AutoFaker.Generate<string>()
                };
                var node = nodes[i];

                if (i > 0)
                {
                    var previous = nodes[i - 1];
                    node.Previous = previous;
                    previous.Next = node;
                }
            }

            foreach (var node in nodes)
            {
                var rndIndex = _rnd.Next(size * 2);
                if (rndIndex < size)
                {
                    node.Random = nodes[rndIndex];
                }
            }


            return nodes[0];
        }
    }
}
