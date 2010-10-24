namespace DomainDrivenDelivery.Domain.Patterns.Entity
{
    /// <summary>
    /// Supporting base class for entities.
    /// </summary>
    /// <remarks>
    /// While the Entity interface makes the pattern properties explicit,
    /// this class is less general and is suited for this particular application.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TIdentity">The identity type.</typeparam>
    public abstract class EntitySupport<TEntity, TIdentity> : Entity<TEntity, TIdentity> where TEntity : class, Entity<TEntity, TIdentity>
    {
        private readonly long _primaryKey;

        public abstract TIdentity identity();

        public bool sameAs(TEntity other)
        {
            return other != null && this.identity().Equals(other.identity());
        }

        public override int GetHashCode()
        {
            return identity().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if(obj == null) return false;
            if(this == obj) return true;
            if(GetType() != obj.GetType()) return false;

            return sameAs((TEntity)obj);
        }
    }
}