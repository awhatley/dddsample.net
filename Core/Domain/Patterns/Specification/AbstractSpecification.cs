namespace DomainDrivenDelivery.Domain.Patterns.Specification
{
    /// <summary>
    /// Abstract base implementation of composite <see cref="Specification{T}"/> with default
    /// implementations for <see cref="and"/>, <see cref="or"/>, and <see cref="not"/>.
    /// </summary>
    /// <typeparam name="T">The type that satisfies the specification.</typeparam>
    public abstract class AbstractSpecification<T> : Specification<T>
    {
        public abstract bool isSatisfiedBy(T t);

        public Specification<T> and(Specification<T> specification)
        {
            return new AndSpecification<T>(this, specification);
        }

        public Specification<T> or(Specification<T> specification)
        {
            return new OrSpecification<T>(this, specification);
        }

        public Specification<T> not(Specification<T> specification)
        {
            return new NotSpecification<T>(specification);
        }
    }
}