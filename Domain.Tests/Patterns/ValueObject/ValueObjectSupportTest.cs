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
            AValueObject vo3 = new AValueObject("1");

            Assert.AreEqual(vo1, vo2);
            Assert.AreEqual(vo2, vo1);
            Assert.False(vo2.Equals(vo3));
            Assert.False(vo3.Equals(vo2));

            Assert.True(vo1.sameValueAs(vo2));
            Assert.False(vo2.sameValueAs(vo3));
        }

        private class AValueObject : ValueObjectSupport<AValueObject>
        {
            public string s { get; private set; }

            internal AValueObject(string s)
            {
                this.s = s;
            }

            internal AValueObject()
            {
            }
        }
    }
}