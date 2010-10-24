namespace DomainDrivenDelivery.Domain.Patterns.DomainEvent
{
    /// <summary>
    /// A domain event is something that is unique, but does not have a lifecycle.
    /// The identity may be explicit, for example the sequence number of a payment,
    /// or it could be derived from various aspects of the event such as where, when and what
    /// has happened.
    /// </summary>
    /// <typeparam name="T">The domain event type.</typeparam>
    public interface DomainEvent<T>
    {
        // TODO: create support class

        /// <summary>
        /// Domain events compare differently depending on the event.
        /// </summary>
        /// <param name="other">The other domain event.</param>
        /// <returns><code>true</code> if the given domain event and this event are regarded as being the same event.</returns>
        bool sameEventAs(T other);
    }
}