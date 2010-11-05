using System;

using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Model.Shared;
using DomainDrivenDelivery.Domain.Model.Travel;
using DomainDrivenDelivery.Domain.Patterns.ValueObject;
using DomainDrivenDelivery.Utilities;

namespace DomainDrivenDelivery.Domain.Model.Freight
{
    /// <summary>
    /// Everything about the delivery of the cargo, i.e. where the cargo is
    /// right now, whether or not it's routed, misdirected and so on.
    /// </summary>
    public class Delivery : ValueObjectSupport<Delivery>
    {
        internal HandlingActivity MostRecentHandlingActivity { get; private set; }
        internal HandlingActivity MostRecentPhysicalHandlingActivity { get; private set; }
        internal DateTime LastUpdatedOn { get; private set; }

        /// <summary>
        /// Initial delivery, before any handling has taken place.
        /// </summary>
        /// <returns>Initial delivery, before any handling has taken place.</returns>
        public static Delivery BeforeHandling()
        {
            return new Delivery(null, null);
        }

        /// <summary>
        /// Derives a new delivery when a cargo has been handled.
        /// </summary>
        /// <param name="newHandlingActivity">handling activity</param>
        /// <returns>An up to date delivery</returns>
        internal Delivery OnHandling(HandlingActivity newHandlingActivity)
        {
            Validate.notNull(newHandlingActivity, "Handling activity is required");

            return newHandlingActivity.Type.isPhysical()
                ? new Delivery(newHandlingActivity, newHandlingActivity)
                : new Delivery(newHandlingActivity, MostRecentPhysicalHandlingActivity);
        }

        /// <summary>
        /// An up to date delivery
        /// </summary>
        /// <returns>An up to date delivery</returns>
        internal Delivery OnRouting()
        {
            return new Delivery(MostRecentHandlingActivity, MostRecentPhysicalHandlingActivity);
        }

        private Delivery(HandlingActivity mostRecentHandlingActivity,
                         HandlingActivity mostRecentPhysicalHandlingActivity)
        {
            MostRecentHandlingActivity = mostRecentHandlingActivity;
            MostRecentPhysicalHandlingActivity = mostRecentPhysicalHandlingActivity;
            LastUpdatedOn = DateTime.Now;
        }

        /// <summary>
        /// Transport status
        /// </summary>
        /// <value>Transport status</value>
        internal TransportStatus TransportStatus
        {
            get { return TransportStatusExtensions.derivedFrom(MostRecentHandlingActivity); }
        }

        /// <summary>
        /// Last known location of the cargo, or Location.UNKNOWN if the delivery history is empty.
        /// </summary>
        /// <value>Last known location of the cargo, or Location.UNKNOWN if the delivery history is empty.</value>
        internal Location LastKnownLocation
        {
            get { return HasBeenHandled ? MostRecentHandlingActivity.Location : Location.None; }
        }

        /// <summary>
        /// Current voyage.
        /// </summary>
        /// <value>Current voyage.</value>
        internal Voyage CurrentVoyage
        {
            get
            {
                return HasBeenHandled && TransportStatus == TransportStatus.ONBOARD_CARRIER
                    ? MostRecentHandlingActivity.Voyage
                    : Voyage.None;
            }
        }

        /// <summary>
        /// True if the cargo has been handled at least once
        /// </summary>
        /// <value>True if the cargo has been handled at least once</value>
        internal bool HasBeenHandled
        {
            get { return MostRecentHandlingActivity != null; }
        }

        /// <summary>
        /// Check if cargo is misdirected. 
        /// </summary>
        /// <remarks>
        /// <list>
        /// <item>A cargo is misdirected if it is in a location that's not in the itinerary.</item>
        /// <item>A cargo with no itinerary can not be misdirected.</item>
        /// <item>A cargo that has received no handling events can not be misdirected.</item>
        /// </list>
        /// </remarks>
        /// <param name="itinerary">itinerary</param>
        /// <returns><code>true</code> if the cargo has been misdirected.</returns>
        internal bool IsMisdirected(Itinerary itinerary)
        {
            return HasBeenHandled && !itinerary.IsExpectedActivity(MostRecentPhysicalHandlingActivity);
        }

        /// <summary>
        /// True if the cargo is routed and not misdirected
        /// </summary>
        /// <param name="itinerary">itinerary</param>
        /// <param name="routeSpecification">route specification</param>
        /// <returns>True if the cargo is routed and not misdirected</returns>
        internal bool IsOnRoute(Itinerary itinerary, RouteSpecification routeSpecification)
        {
            return routeSpecification.StatusOf(itinerary) == RoutingStatus.ROUTED && !IsMisdirected(itinerary);
        }

        internal bool IsUnloadedIn(Location location)
        {
            return HasBeenHandled &&
              MostRecentHandlingActivity.Location.sameAs(location) &&
              MostRecentHandlingActivity.Type == HandlingActivityType.UNLOAD;
        }

        protected internal Delivery()
        {
        }
    }
}