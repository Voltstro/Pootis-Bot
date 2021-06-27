using NUnit.Framework;
using Pootis_Bot.Helper;

namespace Pootis_Bot.Tests
{
    public class EmojiUtilTests
    {
        [Test]
        public void OnlyOneEmojiNoneTest()
        {
            const string test = "Bruh!";
            Assert.False(test.DoesContainOnlyOneEmoji());
        }
        
        [Test]
        public void OnlyOneEmojiOneTest()
        {
            const string test = "🥱";
            Assert.True(test.DoesContainOnlyOneEmoji());
        }
        
        [Test]
        public void OnlyOneEmojiTwoTest()
        {
            const string test = "🥱🥱";
            Assert.False(test.DoesContainOnlyOneEmoji());
        }
        
        [Test]
        public void OnlyOneEmojiWithStringTest()
        {
            const string test = "Bruh! 🥱";
            Assert.False(test.DoesContainOnlyOneEmoji());
        }
        
        [Test]
        public void ContainEmojiNoneTest()
        {
            const string test = "Bruh!";
            bool contain = test.DoesContainEmoji(out int count);
            Assert.False(contain);
            Assert.AreEqual(0, count);
        }
        
        [Test]
        public void ContainEmojiOneTest()
        {
            const string test = "🥱";
            bool contain = test.DoesContainEmoji(out int count);
            Assert.True(contain);
            Assert.AreEqual(1, count);
        }
        
        [Test]
        public void ContainEmojiTwoTest()
        {
            const string test = "🥱🥱";
            bool contain = test.DoesContainEmoji(out int count);
            Assert.True(contain);
            Assert.AreEqual(2, count);
        }
        
        [Test]
        public void ContainEmojiWithStringTest()
        {
            const string test = "Bruh! 🥱";
            bool contain = test.DoesContainEmoji(out int count);
            Assert.True(contain);
            Assert.AreEqual(1, count);
        }
    }
}