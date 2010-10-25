namespace DomainDrivenDelivery.Domain.Patterns.Entity
{
    /// <summary>
    /// An entity, as explained in the DDD book.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TIdentity">The identity type.</typeparam>
    public interface Entity<in TEntity, out TIdentity>
    {
        /// <summary>
        /// Entities have an identity.
        /// </summary>
        /// <returns>The identity of this entity.</returns>
        TIdentity identity();

        /// <summary>
        /// Entities compare by identity, not by attributes.
        /// </summary>
        /// <param name="other">The other entity.</param>
        /// <returns>true if the identities are the same, regardles of other attributes.</returns>
        bool sameAs(TEntity other);
    }
}