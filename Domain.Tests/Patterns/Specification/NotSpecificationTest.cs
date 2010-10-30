using DomainDrivenDelivery.Domain.Patterns.Specification;

using NUnit.Framework;

namespace DomainDrivenDelivery.Domain.Tests.Patterns.Specification
{
    [TestFixture]
    public class NotSpecificationTest
    {
        [Test]
        public void testAndIsSatisifedBy()
        {
            AlwaysTrueSpec trueSpec = new AlwaysTrueSpec();
            AlwaysFalseSpec falseSpec = new AlwaysFalseSpec();

            NotSpecification<object> notSpecification = new NotSpecification<object>(trueSpec);
            Assert.False(notSpecification.isSatisfiedBy(new object()));

            notSpecification = new NotSpecification<object>(falseSpec);
            Assert.True(notSpecification.isSatisfiedBy(new object()));
        }
    }
}