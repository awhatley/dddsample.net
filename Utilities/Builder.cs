namespace DomainDrivenDelivery.Utilities
{
    /// <summary>
    /// The Builder interface is designed to designate a class as a <em>builder</em> 
    /// object in the Builder design pattern.
    /// </summary>
    /// <typeparam name="T">the type of object that the builder will construct or compute.</typeparam>
    public interface Builder<out T>
    {
        /// <summary>
        /// Returns a reference to the object being constructed or result being 
        /// calculated by the builder.
        /// </summary>
        /// <returns>the object constructed or result calculated by the builder.</returns>
        T build();
    }
}