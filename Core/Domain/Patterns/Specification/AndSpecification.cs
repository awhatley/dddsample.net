namespace DomainDrivenDelivery.Domain.Patterns.Specification
{
    /// <summary>
    /// AND specification, used to create a new specification that is the AND of two other specifications.
    /// </summary>
    /// <typeparam name="T">The type that satisfies the specification.</typeparam>
    public class AndSpecification<T> : AbstractSpecification<T>
    {
        private Specification<T> spec1;
        private Specification<T> spec2;

        /// <summary>
        /// Create a new AND specification based on two other spec.
        /// </summary>
        /// <param name="spec1">Specification one.</param>
        /// <param name="spec2">Specification two.</param>
        public AndSpecification(Specification<T> spec1, Specification<T> spec2)
        {
            this.spec1 = spec1;
            this.spec2 = spec2;
        }

        public override bool isSatisfiedBy(T t)
        {
            return spec1.isSatisfiedBy(t) && spec2.isSatisfiedBy(t);
        }
        
    }
}