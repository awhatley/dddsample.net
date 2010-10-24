using System;

using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Model.Shared;
using DomainDrivenDelivery.Domain.Model.Travel;
using DomainDrivenDelivery.Domain.Patterns;
using DomainDrivenDelivery.Domain.Patterns.ValueObject;

namespace DomainDrivenDelivery.Domain.Model.Frieght
{
    /// <summary>
    /// Everything about the delivery of the cargo, i.e. where the cargo is
    /// right now, whether or not it's routed, misdirected and so on.
    /// </summary>
    public class Delivery : ValueObjectSupport<Delivery>
    {
        private readonly HandlingActivity _mostRecentHandlingActivity;
        private readonly HandlingActivity _mostRecentPhysicalHandlingActivity;
        private readonly DateTime _lastUpdatedOn;

        /// <summary>
        /// Initial delivery, before any handling has taken place.
        /// </summary>
        /// <returns>Initial delivery, before any handling has taken place.</returns>
        public static Delivery beforeHandling()
        {
            return new Delivery(null, null);
        }

        /// <summary>
        /// Derives a new delivery when a cargo has been handled.
        /// </summary>
        /// <param name="newHandlingActivity">handling activity</param>
        /// <returns>An up to date delivery</returns>
        internal Delivery onHandling(HandlingActivity newHandlingActivity)
        {
            Validate.notNull(newHandlingActivity, "Handling activity is required");

            if(newHandlingActivity.type().isPhysical())
            {
                return new Delivery(newHandlingActivity, newHandlingActivity);
            }
            else
            {
                return new Delivery(newHandlingActivity, _mostRecentPhysicalHandlingActivity);
            }
        }

        /// <summary>
        /// An up to date delivery
        /// </summary>
        /// <returns>An up to date delivery</returns>
        internal Delivery onRouting()
        {
            return new Delivery(_mostRecentHandlingActivity, _mostRecentPhysicalHandlingActivity);
        }

        private Delivery(HandlingActivity mostRecentHandlingActivity,
                         HandlingActivity mostRecentPhysicalHandlingActivity)
        {
            this._mostRecentHandlingActivity = mostRecentHandlingActivity;
            this._mostRecentPhysicalHandlingActivity = mostRecentPhysicalHandlingActivity;
            this._lastUpdatedOn = DateTime.Now;
        }

        internal HandlingActivity mostRecentHandlingActivity()
        {
            return _mostRecentHandlingActivity;
        }

        internal HandlingActivity mostRecentPhysicalHandlingActivity()
        {
            return _mostRecentPhysicalHandlingActivity;
        }

        /// <summary>
        /// Transport status
        /// </summary>
        /// <returns>Transport status</returns>
        internal TransportStatus transportStatus()
        {
            return TransportStatus.derivedFrom(_mostRecentHandlingActivity);
        }

        /// <summary>
        /// Last known location of the cargo, or Location.UNKNOWN if the delivery history is empty.
        /// </summary>
        /// <returns>Last known location of the cargo, or Location.UNKNOWN if the delivery history is empty.</returns>
        internal Location lastKnownLocation()
        {
            if(hasBeenHandled())
            {
                return _mostRecentHandlingActivity.location();
            }
            else
            {
                return Location.NONE;
            }
        }

        /// <summary>
        /// Current voyage.
        /// </summary>
        /// <returns>Current voyage.</returns>
        internal Voyage currentVoyage()
        {
            if(hasBeenHandled() && transportStatus() == TransportStatus.ONBOARD_CARRIER)
            {
                return _mostRecentHandlingActivity.voyage();
            }
            else
            {
                return Voyage.NONE;
            }
        }

        /// <summary>
        /// True if the cargo has been handled at least once
        /// </summary>
        /// <returns>True if the cargo has been handled at least once</returns>
        internal bool hasBeenHandled()
        {
            return _mostRecentHandlingActivity != null;
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
        internal bool isMisdirected(Itinerary itinerary)
        {
            return hasBeenHandled() && !itinerary.isExpectedActivity(_mostRecentPhysicalHandlingActivity);
        }

        /// <summary>
        /// Routing status.
        /// </summary>
        /// <param name="itinerary">itinerary</param>
        /// <param name="routeSpecification">route specification</param>
        /// <returns>Routing status.</returns>
        internal RoutingStatus routingStatus(Itinerary itinerary, RouteSpecification routeSpecification)
        {
            if(itinerary == null)
            {
                return RoutingStatus.NOT_ROUTED;
            }
            else
            {
                if(routeSpecification.isSatisfiedBy(itinerary))
                {
                    return RoutingStatus.ROUTED;
                }
                else
                {
                    return RoutingStatus.MISROUTED;
                }
            }
        }
        /// <summary>
        /// When this delivery was calculated.
        /// </summary>
        /// <returns>When this delivery was calculated.</returns>
        DateTime lastUpdatedOn()
        {
            return _lastUpdatedOn;
        }

        /// <summary>
        /// True if the cargo is routed and not misdirected
        /// </summary>
        /// <param name="itinerary">itinerary</param>
        /// <param name="routeSpecification">route specification</param>
        /// <returns>True if the cargo is routed and not misdirected</returns>
        internal bool isOnRoute(Itinerary itinerary, RouteSpecification routeSpecification)
        {
            return routingStatus(itinerary, routeSpecification) == RoutingStatus.ROUTED && !isMisdirected(itinerary);
        }

        internal bool isUnloadedIn(Location location)
        {
            return hasBeenHandled() &&
              _mostRecentHandlingActivity.location().sameAs(location) &&
              mostRecentHandlingActivity().type() == HandlingActivityType.UNLOAD;
        }

        Delivery()
        {
            // Needed by Hibernate
            _mostRecentHandlingActivity = _mostRecentPhysicalHandlingActivity = null;
        }
    }
}