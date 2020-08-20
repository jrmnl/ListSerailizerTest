using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;

namespace ListSerializerTest
{
    public class ListSerializer : IListSerializer
    {
        public Task<ListNode> DeepCopy(ListNode head)
        {
            if (head is null) return Task.FromResult<ListNode>(null);

            var originToCopy = new Dictionary<ListNode, ListNode>();
            var currentNode = head;
            while (currentNode != null)
            {
                var copy = new ListNode
                {
                    Data = currentNode.Data
                };
                if (currentNode.Previous != null)
                {
                    var previousNode = originToCopy[currentNode.Previous];
                    copy.Previous = previousNode;
                    previousNode.Next = copy;
                }

                originToCopy.Add(currentNode, copy);

                currentNode = currentNode.Next;
            }

            foreach (var kvp in originToCopy)
            {
                var copy = kvp.Value;
                copy.Random =
                    kvp.Key.Random is null
                    ? null
                    : originToCopy[kvp.Key.Random];
            }

            var copyHead = originToCopy[head];
            return Task.FromResult(copyHead);
        }

        public async Task Serialize(ListNode head, Stream s)
        {
            if (s is null) throw new ArgumentNullException(nameof(s));

            var map = new Dictionary<ListNode, int>();
            var currentNode = head;
            var index = 0;
            while (currentNode != null)
            {
                map.Add(currentNode, index);

                index++;
                currentNode = currentNode.Next;
            }

            currentNode = head;
            while (currentNode != null)
            {
                var node = new NodeData
                {
                    Data = currentNode.Data,
                    IndexToRandom = currentNode.Random is null
                        ? (int?)null
                        : map[currentNode.Random]
                };

                await MessagePackSerializer.SerializeAsync(s, node);

                currentNode = currentNode.Next;
            }
        }

        public async Task<ListNode> Deserialize(Stream s)
        {
            if (s is null) throw new ArgumentNullException(nameof(s));

            var indexToNodes = new Dictionary<int, (ListNode Node, int? IndexToRandom)>();

            using var streamReader = new MessagePackStreamReader(s);
            ListNode previousNode = null;
            var index = 0;
            while (await streamReader.ReadAsync(CancellationToken.None) is ReadOnlySequence<byte> msgpack)
            {
                var nodeData = MessagePackSerializer.Deserialize<NodeData>(msgpack);
                var node = new ListNode
                {
                    Data = nodeData.Data,
                    Previous = previousNode
                };

                if (previousNode != null)
                {
                    previousNode.Next = node;
                }

                indexToNodes.Add(index, (node, nodeData.IndexToRandom));

                index++;
                previousNode = node;
            }

            foreach (var (node, indexToRandom) in indexToNodes.Values)
            {
                if (indexToRandom != null)
                {
                    node.Random = indexToNodes[indexToRandom.Value].Node;
                }
            }

            return indexToNodes.Any()
                ? indexToNodes[0].Node
                : null;
        }

        [MessagePackObject]
        public struct NodeData
        {
            [Key(0)]
            public string Data { get; set; }

            [Key(1)]
            public int? IndexToRandom { get; set; }
        }
    }
}
