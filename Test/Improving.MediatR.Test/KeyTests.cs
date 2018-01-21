namespace Improving.MediatR.Tests
{
    using System;
    using MediatR;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class KeyTests
    {
        [TestMethod]
        public void Can_Create_Key()
        {
            var key = Key.For(1, "Red");
            Assert.AreEqual(1, key.Id);
            Assert.AreEqual("Red", key.Name);
        }

        [TestMethod]
        public void Can_Match_Keys()
        {
            var key1 = Key.For(1, "Red");
            var key2 = Key.For(1, "Red");
            var key3 = Key.For(3, "Blue");
            Key<int> key4 = null;

            Assert.AreEqual(key1, key2);
            Assert.AreNotEqual(key1, key3);
            Assert.IsTrue(key1 == key2);
            Assert.IsTrue(key1 != key3);
            Assert.IsTrue(key4 == null);
        }

        [TestMethod]
        public void Can_Match_Key_In_Collection()
        {
            var key1 = Key.For(1, "Red");
            var key2 = Key.For(2, "Green");
            var key3 = Key.For(3, "Blue");

            var keys = new[] {key1, key2, key3};
            Assert.AreSame(key2, keys.Find(2));
        }

        [TestMethod]
        public void Can_Get_Default_Key_If_No_Match()
        {
            var keys = new[] { Key.For(1, "Red") };
            var key = keys.FindOrDefault(3);
            Assert.AreEqual(3, key.Id);
            Assert.IsNull(key.Name);
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void Will_Fail_If_Key_Not_Found()
        {
            var keys = new[] { Key.For(1, "Red") };
            var key = keys.Find(3);
            Assert.AreEqual(3, key.Id);
            Assert.IsNull(key.Name);
        }
    }
}

