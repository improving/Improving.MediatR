namespace Improving.MediatR.Tests
{
    using System;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DTOTests
    {
        public class Picture : DTO
        {
            public byte[] bytes { get; set; }   
        }

        [TestMethod, Ignore]
        public void Should_Format_Byte_Array_As_Size()
        {
            var picture = new Picture
            {
                bytes = Encoding.UTF8.GetBytes("The is a test")
            };

            var json = picture.ToString();
            Assert.AreEqual(json, @"Picture {
  bytes: (13 bytes)
}");
        }

        [TestMethod, Ignore]
        public void Should_Format_Empty_Byte_Array()
        {
            var picture = new Picture
            {
                bytes = new byte[0]
            };

            var json = picture.ToString();
            Assert.AreEqual(json, @"Picture {
  bytes: (0 bytes)
}");
        }
    }
}
