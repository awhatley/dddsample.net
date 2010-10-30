using DomainDrivenDelivery.Domain.Patterns.Specification;

using NUnit.Framework;

namespace DomainDrivenDelivery.Domain.Tests.Patterns.Specification
{
    [TestFixture]
    public class AndSpecificationTest
    {
        [Test]
        public void testAndIsSatisifedBy()
        {
            AlwaysTrueSpec trueSpec = new AlwaysTrueSpec();
            AlwaysFalseSpec falseSpec = new AlwaysFalseSpec();

            AndSpecification<object> andSpecification = new AndSpecification<object>(trueSpec, trueSpec);
            Assert.True(andSpecification.isSatisfiedBy(new object()));

            andSpecification = new AndSpecification<object>(falseSpec, trueSpec);
            Assert.False(andSpecification.isSatisfiedBy(new object()));

            andSpecification = new AndSpecification<object>(trueSpec, falseSpec);
            Assert.False(andSpecification.isSatisfiedBy(new object()));

            andSpecification = new AndSpecification<object>(falseSpec, falseSpec);
            Assert.False(andSpecification.isSatisfiedBy(new object()));
        }
    }
}