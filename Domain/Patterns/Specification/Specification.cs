namespace DomainDrivenDelivery.Domain.Patterns.Specification
{
    /// <summary>
    /// Specificaiton interface.
    /// </summary>
    /// <remarks>
    /// Use <see cref="AbstractSpecification{T}"/> as base for creating specifications, and
    /// only the method <see cref="isSatisfiedBy"/> must be implemented.
    /// </remarks>
    /// <typeparam name="T">The type that satisfies the specification.</typeparam>
    public interface Specification<T>
    {
        /// <summary>
        /// Check if <paramref name="t"/> satisfies the specification.
        /// </summary>
        /// <param name="t">Object to test.</param>
        /// <returns><code>true</code> if <paramref name="t"/> satisfies the specification.</returns>
        bool isSatisfiedBy(T t);

        /// <summary>
        /// Create a new specification that is the AND operation of this specification and another specification.
        /// </summary>
        /// <param name="specification">Specification to AND.</param>
        /// <returns>A new specification.</returns>
        Specification<T> and(Specification<T> specification);

        /// <summary>
        /// Create a new specification that is the OR operation of this specification and another specification.
        /// </summary>
        /// <param name="specification">Specification to OR.</param>
        /// <returns>A new specification.</returns>
        Specification<T> or(Specification<T> specification);

        /// <summary>
        /// Create a new specification that is the NOT operation of another specification.
        /// </summary>
        /// <param name="specification">Specification to NOT.</param>
        /// <returns>A new specification.</returns>
        Specification<T> not(Specification<T> specification);
    }
}