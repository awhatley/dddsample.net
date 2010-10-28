using System;

using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Handling;
using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Model.Shared;
using DomainDrivenDelivery.Domain.Model.Travel;

namespace DomainDrivenDelivery.Application.Handling
{
    /// <summary>
    /// Handling event service
    /// </summary>
    public interface HandlingEventService
    {
        /// <summary>
        /// Registers a handling event in the system, and notifies interested
        /// parties that a cargo has been handled.
        /// </summary>
        /// <param name="completionTime">when the event was completed</param>
        /// <param name="trackingId">cargo tracking id</param>
        /// <param name="voyageNumber">voyage number</param>
        /// <param name="unLocode">UN locode for the location where the event occurred</param>
        /// <param name="type">type of event</param>
        /// <param name="operatorCode">operator code</param>
        /// <exception cref="CannotCreateHandlingEventException">
        /// if a handling event that represents an actual event that's relevant to a cargo we're tracking
        /// can't be created from the parameters
        /// </exception>
        void registerHandlingEvent(DateTime completionTime,
                                   TrackingId trackingId,
                                   VoyageNumber voyageNumber,
                                   UnLocode unLocode,
                                   HandlingActivityType type,
                                   OperatorCode operatorCode);
    }
}