using System.Collections.Generic;

using DomainDrivenDelivery.Domain.Model.Travel;

namespace DomainDrivenDelivery.Domain.Model.Frieght
{
    /// <summary>
    /// Cargo repository.
    /// </summary>
    public interface CargoRepository
    {
        /// <summary>
        /// Finds a cargo using given id.
        /// </summary>
        /// <param name="trackingId">Id</param>
        /// <returns>Cargo if found, else <code>null</code></returns>
        Cargo find(TrackingId trackingId);

        /// <summary>
        /// Finds all cargo on the specified voyage.
        /// </summary>
        /// <param name="voyage">voyage</param>
        /// <returns>A list of cargo for the specified voyage.</returns>
        IEnumerable<Cargo> findCargosOnVoyage(Voyage voyage);

        /// <summary>
        /// Finds all cargo.
        /// </summary>
        /// <returns>All cargo.</returns>
        IEnumerable<Cargo> findAll();

        /// <summary>
        /// Saves given cargo.
        /// </summary>
        /// <param name="cargo">cargo to save</param>
        void store(Cargo cargo);
    }
}