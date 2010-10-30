using System;

using DomainDrivenDelivery.Domain.Model.Locations;

using NUnit.Framework;

namespace DomainDrivenDelivery.Domain.Tests.Model.Locations
{
    [TestFixture]
    public class UnLocodeTest
    {
        [Test]
        public void testNew()
        {
            assertValid("AA234");
            assertValid("AAA9B");
            assertValid("AAAAA");

            assertInvalid("AAAA");
            assertInvalid("AAAAAA");
            assertInvalid("AAAA");
            assertInvalid("AAAAAA");
            assertInvalid("22AAA");
            assertInvalid("AA111");
            assertInvalid(null);
        }

        [Test]
        public void testStringValue()
        {
            Assert.AreEqual("ABCDE", new UnLocode("AbcDe").stringValue());
        }

        [Test]
        public void testEquals()
        {
            UnLocode allCaps = new UnLocode("ABCDE");
            UnLocode mixedCase = new UnLocode("aBcDe");

            Assert.IsTrue(allCaps.Equals(mixedCase));
            Assert.IsTrue(mixedCase.Equals(allCaps));
            Assert.IsTrue(allCaps.Equals(allCaps));

            Assert.IsFalse(allCaps.Equals(null));
            Assert.IsFalse(allCaps.Equals(new UnLocode("FGHIJ")));
        }

        [Test]
        public void testHashCode()
        {
            UnLocode allCaps = new UnLocode("ABCDE");
            UnLocode mixedCase = new UnLocode("aBcDe");

            Assert.AreEqual(allCaps.GetHashCode(), mixedCase.GetHashCode());
        }

        private void assertValid(string unlocode)
        {
            new UnLocode(unlocode);
        }

        private void assertInvalid(string unlocode)
        {
            try
            {
                new UnLocode(unlocode);
                Assert.Fail("The combination [" + unlocode + "] is not a valid UnLocode");
            }
            catch(ArgumentException)
            {
            }
        }
    }
}