using System;

using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Model.Shared;
using DomainDrivenDelivery.Domain.Model.Travel;
using DomainDrivenDelivery.Domain.Patterns.Entity;
using DomainDrivenDelivery.Utilities;

namespace DomainDrivenDelivery.Domain.Model.Frieght
{
    /// <summary>
    /// A Cargo. This is the central class in the domain model,
    /// and it is the root of the Cargo-Itinerary-Leg-Delivery-RouteSpecification aggregate.
    /// </summary>
    /// <remarks>
    /// A cargo is identified by a unique tracking id, and it always has an origin
    /// and a route specification. The life cycle of a cargo begins with the booking procedure,
    /// when the tracking id is assigned. During a (short) period of time, between booking
    /// and initial routing, the cargo has no itinerary.
    /// <para />
    /// The booking clerk requests a list of possible routes, matching the route specification,
    /// and assigns the cargo to one route. The route to which a cargo is assigned is described
    /// by an itinerary.
    /// <para />
    /// A cargo can be re-routed during transport, on demand of the customer, in which case
    /// a new route is specified for the cargo and a new route is requested. The old itinerary,
    /// being a value object, is discarded and a new one is attached.
    /// <para />
    /// It may also happen that a cargo is accidentally misrouted, which should notify the proper
    /// personnel and also trigger a re-routing procedure.
    /// <para />
    /// When a cargo is handled, the status of the delivery changes. Everything about the delivery
    /// of the cargo is contained in the Delivery value object, which is replaced whenever a cargo
    /// is handled by an asynchronous event triggered by the registration of the handling event.
    /// <para />
    /// The delivery can also be affected by routing changes, i.e. when a the route specification
    /// changes, or the cargo is assigned to a new route. In that case, the delivery update is performed
    /// synchronously within the cargo aggregate.
    /// <para />
    /// The life cycle of a cargo ends when the cargo is claimed by the customer.
    /// <para />
    /// The cargo aggregate, and the entre domain model, is built to solve the problem
    /// of booking and tracking cargo. All important business rules for determining whether
    /// or not a cargo is misdirected, what the current status of the cargo is (on board carrier,
    /// in port etc), are captured in this aggregate.
    /// </remarks>
    public class Cargo : EntitySupport<Cargo, TrackingId>
    {
        private readonly TrackingId _trackingId;
        private RouteSpecification _routeSpecification;
        private Itinerary _itinerary;
        private Delivery _delivery;

        /// <summary>
        /// Initializes a new instance of the <see cref="Cargo"/> class.
        /// </summary>
        /// <param name="trackingId">A unique tracking ID.</param>
        /// <param name="routeSpecification">The route specification.</param>
        public Cargo(TrackingId trackingId, RouteSpecification routeSpecification)
        {            
            Validate.notNull(trackingId, "Tracking ID is required");
            Validate.notNull(routeSpecification, "Route specification is required");

            _trackingId = trackingId;
            _routeSpecification = routeSpecification;
            _delivery = Delivery.beforeHandling();
        }

        /// <summary>
        /// The tracking id is the identity of this entity, and is unique.
        /// </summary>
        /// <returns>Tracking id.</returns>
        public override TrackingId identity()
        {
            return _trackingId;
        }

        /// <summary>
        /// The tracking id is the identity of this entity, and is unique.
        /// </summary>
        /// <returns>Tracking id.</returns>
        public TrackingId trackingId()
        {
            return _trackingId;
        }

        /// <summary>
        /// The itinerary.
        /// </summary>
        /// <returns>The itinerary.</returns>
        public Itinerary itinerary()
        {
            return _itinerary;
        }

        /// <summary>
        /// The route specification.
        /// </summary>
        /// <returns>The route specification.</returns>
        public RouteSpecification routeSpecification()
        {
            return _routeSpecification;
        }

        /// <summary>
        /// Estimated time of arrival.
        /// </summary>
        /// <returns>Estimated time of arrival.</returns>
        public DateTime estimatedTimeOfArrival()
        {
            if(_delivery.isOnRoute(_itinerary, _routeSpecification))
            {
                return _itinerary.estimatedTimeOfArrival();
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Next expected activity. If the cargo is not on route (misdirected and/or misrouted),
        /// it cannot be determined and null is returned.
        /// </summary>
        /// <returns>Next expected activity.</returns>
        public HandlingActivity nextExpectedActivity()
        {
            if(!_delivery.isOnRoute(_itinerary, _routeSpecification))
            {
                return null;
            }

            if(_delivery.isUnloadedIn(customsClearancePoint()))
            {
                return HandlingActivity.customsIn(customsClearancePoint());
            }
            else
            {
                return _itinerary.activitySucceeding(_delivery.mostRecentPhysicalHandlingActivity());
            }
        }

        /// <summary>
        /// True if cargo is misdirected.
        /// </summary>
        /// <returns>True if cargo is misdirected.</returns>
        public bool isMisdirected()
        {
            return _delivery.isMisdirected(_itinerary);
        }

        /// <summary>
        /// Transport status.
        /// </summary>
        /// <returns>Transport status.</returns>
        public TransportStatus transportStatus()
        {
            return _delivery.transportStatus();
        }

        /// <summary>
        /// Routing status.
        /// </summary>
        /// <returns>Routing status.</returns>
        public RoutingStatus routingStatus()
        {
            return _delivery.routingStatus(_itinerary, _routeSpecification);
        }

        /// <summary>
        /// Current voyage.
        /// </summary>
        /// <returns>Current voyage.</returns>
        public Voyage currentVoyage()
        {
            return _delivery.currentVoyage();
        }

        /// <summary>
        /// Last known location.
        /// </summary>
        /// <returns>Last known location.</returns>
        public Location lastKnownLocation()
        {
            return _delivery.lastKnownLocation();
        }

        /// <summary>
        /// Updates all aspects of the cargo aggregate status
        /// based on the current route specification, itinerary and handling of the cargo.
        /// </summary>
        /// <remarks>
        /// When either of those three changes, i.e. when a new route is specified for the cargo,
        /// the cargo is assigned to a route or when the cargo is handled, the status must be
        /// re-calculated.
        /// <p/>
        /// <see cref="RouteSpecification"/> and <see cref="Itinerary"/> are both inside the Cargo
        /// aggregate, so changes to them cause the status to be updated <b>synchronously</b>,
        /// but handling cause the status update to happen <b>asynchronously</b>
        /// since <see cref="HandlingEvent"/> is in a different aggregate.
        /// </remarks>
        /// <param name="handlingActivity">handling activity</param>
        public void handled(HandlingActivity handlingActivity)
        {
            Validate.notNull(handlingActivity, "Handling activity is required");

            if(succedsMostRecentActivity(handlingActivity))
            {
                this._delivery = _delivery.onHandling(handlingActivity);
            }
        }

        /// <summary>
        /// Specifies a new route for this cargo.
        /// </summary>
        /// <param name="routeSpecification">route specification.</param>
        public void specifyNewRoute(RouteSpecification routeSpecification)
        {
            Validate.notNull(routeSpecification, "Route specification is required");

            this._routeSpecification = routeSpecification;
        }

        /// <summary>
        /// Attach a new itinerary to this cargo.
        /// </summary>
        /// <param name="itinerary">an itinerary. May not be null.</param>
        public void assignToRoute(Itinerary itinerary)
        {
            Validate.notNull(itinerary, "Itinerary is required");

            if(routingStatus() != RoutingStatus.NOT_ROUTED)
            {
                this._delivery = _delivery.onRouting();
            }
            this._itinerary = itinerary;
        }

        /// <summary>
        /// Customs zone.
        /// </summary>
        /// <returns>Customs zone.</returns>
        public CustomsZone customsZone()
        {
            return _routeSpecification.destination().customsZone();
        }

        /// <summary>
        /// Customs clearance point.
        /// </summary>
        /// <returns>Customs clearance point.</returns>
        public Location customsClearancePoint()
        {
            if(routingStatus() == RoutingStatus.NOT_ROUTED)
            {
                return Location.NONE;
            }
            else
            {
                return customsZone().entryPoint(_itinerary.locations());
            }
        }

        /// <summary>
        /// True if the cargo is ready to be claimed.
        /// </summary>
        /// <returns>True if the cargo is ready to be claimed.</returns>
        public bool isReadyToClaim()
        {
            if(customsClearancePoint().sameAs(_routeSpecification.destination()))
            {
                return HandlingActivity.customsIn(customsClearancePoint()).sameValueAs(mostRecentHandlingActivity());
            }
            else
            {
                return _delivery.isUnloadedIn(_routeSpecification.destination());
            }
        }

        /// <summary>
        /// Most recent handling activity, or null if never handled.
        /// </summary>
        /// <returns>Most recent handling activity, or null if never handled.</returns>
        public HandlingActivity mostRecentHandlingActivity()
        {
            return _delivery.mostRecentHandlingActivity();
        }

        /// <summary>
        /// The earliest rerouting location.
        /// </summary>
        /// <remarks>
        /// If the cargo is in port, it's the current location.
        /// If it's onboard a carrier it's the next arrival location.
        /// </remarks>
        /// <returns>The earliest rerouting location.</returns>
        public Location earliestReroutingLocation()
        {
            if(isMisdirected())
            {
                if(transportStatus() == TransportStatus.ONBOARD_CARRIER)
                {
                    return currentVoyage().arrivalLocationWhenDepartedFrom(lastKnownLocation());
                }
                else
                {
                    return lastKnownLocation();
                }
            }
            else
            {
                return _itinerary.matchLeg(_delivery.mostRecentPhysicalHandlingActivity()).leg().unloadLocation();
            }
        }

        /// <summary>
        /// Merges the current <see cref="Itinerary"/> with the provided <see cref="Itinerary"/>
        /// </summary>
        /// <param name="other">itinerary</param>
        /// <returns>A merge between the current itinerary and the provided itinerary
        /// that describes a continuous route even if the cargo is currently misdirected.</returns>
        public Itinerary itineraryMergedWith(Itinerary other)
        {
            if(routingStatus() == RoutingStatus.NOT_ROUTED)
            {
                return other;
            }
            else if(isMisdirected() && transportStatus() == TransportStatus.ONBOARD_CARRIER)
            {
                Leg currentLeg = Leg.deriveLeg(
                  currentVoyage(), lastKnownLocation(), currentVoyage().arrivalLocationWhenDepartedFrom(lastKnownLocation())
                );

                return this.itinerary().
                  truncatedAfter(lastKnownLocation()).
                  withLeg(currentLeg).
                  appendBy(other);
            }
            else
            {
                return this.itinerary().
                  truncatedAfter(earliestReroutingLocation()).
                  appendBy(other);
            }
        }

        private bool succedsMostRecentActivity(HandlingActivity newHandlingActivity)
        {
            if(_delivery.hasBeenHandled())
            {
                HandlingActivity priorActivity = _itinerary.strictlyPriorOf(
                  _delivery.mostRecentPhysicalHandlingActivity(), newHandlingActivity
                );
                return !newHandlingActivity.sameValueAs(priorActivity);
            }
            else
            {
                return true;
            }
        }

        public override string ToString()
        {
            return _trackingId + " (" + _routeSpecification + ")";
        }

        Cargo()
        {
            // Needed by Hibernate
            _trackingId = null;
        }
    }
}