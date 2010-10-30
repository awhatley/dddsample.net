using DomainDrivenDelivery.Domain.Patterns.Entity;

using NUnit.Framework;

namespace DomainDrivenDelivery.Domain.Tests.Patterns.Entity
{
    [TestFixture]
    public class EntitySupportTest
    {
        [Test]
        public void testOneAnnotationSuccess()
        {
            OneAnnotationEntity entity = new OneAnnotationEntity("id");
            Assert.AreEqual("id", entity.identity());
        }

        [Test]
        public void testSameIdentityEqualsHashcode()
        {
            OneAnnotationEntity entity1 = new OneAnnotationEntity("A");
            OneAnnotationEntity entity2 = new OneAnnotationEntity("A");
            OneAnnotationEntity entity3 = new OneAnnotationEntity("B");

            Assert.True(entity1.sameAs(entity2));
            Assert.False(entity2.sameAs(entity3));

            Assert.True(entity1.Equals(entity2));
            Assert.False(entity2.Equals(entity3));

            Assert.True(entity1.GetHashCode() == entity2.GetHashCode());
            Assert.False(entity2.GetHashCode() == entity3.GetHashCode());
        }

        private class OneAnnotationEntity : EntitySupport<OneAnnotationEntity, string>
        {
            private readonly string id;

            internal OneAnnotationEntity(string id)
            {
                this.id = id;
            }

            public override string identity()
            {
                return id;
            }
        }
    }
}