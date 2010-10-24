namespace DomainDrivenDelivery.Domain.Patterns.Specification
{
    /// <summary>
    /// OR specification, used to create a new specification that is the OR of two other specifications.
    /// </summary>
    /// <typeparam name="T">The type that satisfies the specification.</typeparam>
    public class OrSpecification<T> : AbstractSpecification<T>
    {
        private Specification<T> spec1;
        private Specification<T> spec2;

        /// <summary>
        /// Create a new OR specification based on two other spec.
        /// </summary>
        /// <param name="spec1">Specification one.</param>
        /// <param name="spec2">Specification two.</param>
        public OrSpecification(Specification<T> spec1, Specification<T> spec2)
        {
            this.spec1 = spec1;
            this.spec2 = spec2;
        }

        public override bool isSatisfiedBy(T t)
        {
            return spec1.isSatisfiedBy(t) || spec2.isSatisfiedBy(t);
        }
    }
}