namespace DomainDrivenDelivery.Domain.Patterns.ValueObject
{
    /// <summary>
    /// A value object.
    /// </summary>
    public interface ValueObject<in T>
    {
        /// <summary>
        /// Value objects compare by the values of their attributes, they don't have an identity.
        /// </summary>
        /// <param name="other">The other value object.</param>
        /// <returns><code>true</code> if the given value object's and this value object's attributes are the same.</returns>
        bool sameValueAs(T other);
    }
}