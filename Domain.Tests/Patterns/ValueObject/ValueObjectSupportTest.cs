using DomainDrivenDelivery.Domain.Patterns.ValueObject;

using NUnit.Framework;

namespace DomainDrivenDelivery.Domain.Tests.Patterns.ValueObject
{
    [TestFixture]
    public class ValueObjectSupportTest
    {
        [Test]
        public void testEquals()
        {
            AValueObject vo1 = new AValueObject("A");
            AValueObject vo2 = new AValueObject("A");
            BValueObject vo3 = new BValueObject("A", 1);

            Assert.AreEqual(vo1, vo2);
            Assert.AreEqual(vo2, vo1);
            Assert.False(vo2.Equals(vo3));
            Assert.False(vo3.Equals(vo2));

            Assert.True(vo1.sameValueAs(vo2));
            Assert.False(vo2.sameValueAs(vo3));
        }

        private class AValueObject : ValueObjectSupport<AValueObject>
        {
            private string s;

            internal AValueObject(string s)
            {
                this.s = s;
            }

            internal AValueObject()
            {
            }
        }

        private class BValueObject : AValueObject
        {
            private int x;

            internal BValueObject(string s, int x) : base(s)
            {
                this.x = x;
            }
        }
    }
}