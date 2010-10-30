using DomainDrivenDelivery.Domain.Patterns.Specification;

using NUnit.Framework;

namespace DomainDrivenDelivery.Domain.Tests.Patterns.Specification
{
    [TestFixture]
    public class OrSpecificationTest
    {
        [Test]
        public void testAndIsSatisifedBy()
        {
            AlwaysTrueSpec trueSpec = new AlwaysTrueSpec();
            AlwaysFalseSpec falseSpec = new AlwaysFalseSpec();

            OrSpecification<object> orSpecification = new OrSpecification<object>(trueSpec, trueSpec);
            Assert.True(orSpecification.isSatisfiedBy(new object()));

            orSpecification = new OrSpecification<object>(falseSpec, trueSpec);
            Assert.True(orSpecification.isSatisfiedBy(new object()));

            orSpecification = new OrSpecification<object>(trueSpec, falseSpec);
            Assert.True(orSpecification.isSatisfiedBy(new object()));

            orSpecification = new OrSpecification<object>(falseSpec, falseSpec);
            Assert.False(orSpecification.isSatisfiedBy(new object()));
        }
    }
}