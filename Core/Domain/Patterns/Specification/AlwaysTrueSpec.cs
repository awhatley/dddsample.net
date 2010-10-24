namespace DomainDrivenDelivery.Domain.Patterns.Specification
{
    public class AlwaysTrueSpec : AbstractSpecification<object>
    {
        public override bool isSatisfiedBy(object o)
        {
            return true;
        }
    }
}