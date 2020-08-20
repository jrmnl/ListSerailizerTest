using System.IO;
using System.Threading.Tasks;
using ListSerializerTest;
using Xunit;

namespace ListSerailizerTest.Tests
{
    public class ListSerializerTests
    {
        private readonly IListSerializer _serializer = new ListSerializer();

        [Fact]
        public async Task CanDeepCopyNull()
        {
            var copy = await _serializer.DeepCopy(null);

            Assert.Null(copy);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("123qwe")]
        public async Task CanDeepCopySingleElement(string data)
        {
            var head = new ListNode
            {
                Data = data
            };

            var copy = await _serializer.DeepCopy(head);

            CompareList(head, copy);
        }

        [Theory]
        [InlineData(100)]
        public async Task CanDeepCopyList(int nodesCount)
        {
            var head = ListNodesGenerator.GenerateList(nodesCount);

            var copy = await _serializer.DeepCopy(head);

            CompareList(head, copy);
        }

        [Fact]
        public async Task CanSerializeAndDeserializeNull()
        {
            using var stream = new MemoryStream();

            await _serializer.Serialize(null, stream);
            stream.Seek(0, SeekOrigin.Begin);
            var copy = await _serializer.Deserialize(stream);

            Assert.Null(copy);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("123qwe")]
        public async Task CanSerializeAndDeserializeSingleElement(string data)
        {
            var head = new ListNode
            {
                Data = data
            };

            using var stream = new MemoryStream();

            await _serializer.Serialize(head, stream);
            stream.Seek(0, SeekOrigin.Begin);
            var copy = await _serializer.Deserialize(stream);

            CompareList(head, copy);
        }


        [Theory]
        [InlineData(100)]
        public async Task CanSerializeAndDeserializeList(int nodesCount)
        {
            var head = ListNodesGenerator.GenerateList(nodesCount);

            using var stream = new MemoryStream();

            await _serializer.Serialize(head, stream);
            stream.Seek(0, SeekOrigin.Begin);
            var copy = await _serializer.Deserialize(stream);

            CompareList(head, copy);
        }

        private static void CompareList(ListNode head, ListNode copy)
        {
            while (head != null)
            {
                Assert.NotEqual(head, copy);
                Assert.Equal(head.Data, copy.Data);

                if (head.Random != null && copy.Random != null)
                {
                    Assert.NotEqual(head.Random, copy.Random);
                    Assert.Equal(head.Random.Data, copy.Random.Data);
                }

                if (head.Previous != null && copy.Previous != null)
                {
                    Assert.NotEqual(head.Previous, copy.Previous);
                    Assert.Equal(head.Previous.Data, copy.Previous.Data);
                }

                head = head.Next;
                copy = copy.Next;
            }
        }
    }
}
