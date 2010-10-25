namespace DomainDrivenDelivery.Domain.Patterns.Specification
{
    public class AlwaysFalseSpec : AbstractSpecification<object>
    {
        public override bool isSatisfiedBy(object o)
        {
            return false;
        }
    }
}