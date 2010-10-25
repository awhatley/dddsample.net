using System;
using System.Collections.Generic;

using DomainDrivenDelivery.Domain.Model.Frieght;
using DomainDrivenDelivery.Domain.Model.Locations;

namespace DomainDrivenDelivery.Application.Booking
{
    /// <summary>
    /// Cargo booking service.
    /// </summary>
    public interface BookingService
    {
        /// <summary>
        /// Registers a new cargo in the tracking system, not yet routed.
        /// </summary>
        /// <param name="origin">cargo origin</param>
        /// <param name="destination">cargo destination</param>
        /// <param name="arrivalDeadline">arrival deadline</param>
        /// <returns>Cargo tracking id</returns>
        TrackingId bookNewCargo(UnLocode origin, UnLocode destination, DateTime arrivalDeadline);

        /// <summary>
        /// Requests a list of itineraries describing possible routes for this cargo.
        /// </summary>
        /// <param name="trackingId">cargo tracking id</param>
        /// <returns>A list of possible itineraries for this cargo</returns>
        IEnumerable<Itinerary> requestPossibleRoutesForCargo(TrackingId trackingId);

        /// <summary>
        /// Assigns a cargo to the specified route.
        /// </summary>
        /// <param name="itinerary">itinerary describing the selected route</param>
        /// <param name="trackingId">cargo tracking id</param>
        void assignCargoToRoute(Itinerary itinerary, TrackingId trackingId);

        /// <summary>
        /// Changes the destination of a cargo.
        /// </summary>
        /// <param name="trackingId">cargo tracking id</param>
        /// <param name="unLocode">UN locode of new destination</param>
        void changeDestination(TrackingId trackingId, UnLocode unLocode);

        /// <summary>
        /// Loads a cargo for (re-) routing.
        /// Locks the cargo for exclusive modification.
        /// </summary>
        /// <param name="trackingId">tracking id</param>
        /// <returns>The cargo.</returns>
        Cargo loadCargoForRouting(TrackingId trackingId);
    }
}