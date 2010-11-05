using System;

using DomainDrivenDelivery.Domain.Model.Handling;
using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Model.Shared;
using DomainDrivenDelivery.Domain.Model.Travel;
using DomainDrivenDelivery.Domain.Patterns.Entity;
using DomainDrivenDelivery.Utilities;

namespace DomainDrivenDelivery.Domain.Model.Freight
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
        /// <summary>
        /// Initializes a new instance of the <see cref="Cargo"/> class.
        /// </summary>
        /// <param name="trackingId">A unique tracking ID.</param>
        /// <param name="routeSpecification">The route specification.</param>
        public Cargo(TrackingId trackingId, RouteSpecification routeSpecification)
        {            
            Validate.notNull(trackingId, "Tracking ID is required");
            Validate.notNull(routeSpecification, "Route specification is required");

            TrackingId = trackingId;
            RouteSpecification = routeSpecification;
            Delivery = Delivery.BeforeHandling();
        }

        /// <summary>
        /// The tracking id is the identity of this entity, and is unique.
        /// </summary>
        /// <value>Tracking id.</value>
        public override TrackingId Identity
        {
            get { return TrackingId; }
        }

        /// <summary>
        /// The tracking id is the identity of this entity, and is unique.
        /// </summary>
        public virtual TrackingId TrackingId { get; private set; }

        /// <summary>
        /// The itinerary.
        /// </summary>
        public virtual Itinerary Itinerary { get; private set; }

        /// <summary>
        /// The route specification.
        /// </summary>
        public virtual RouteSpecification RouteSpecification { get; private set; }

        /// <summary>
        /// The delivery.
        /// </summary>
        private Delivery Delivery { get; set; }

        /// <summary>
        /// Estimated time of arrival.
        /// </summary>
        public virtual DateTime EstimatedTimeOfArrival
        {
            get
            {
                return Delivery.IsOnRoute(Itinerary, RouteSpecification)
                    ? Itinerary.EstimatedTimeOfArrival
                    : DateTime.MinValue;
            }
        }

        /// <summary>
        /// Next expected activity. If the cargo is not on route (misdirected and/or misrouted),
        /// it cannot be determined and null is returned.
        /// </summary>
        public virtual HandlingActivity NextExpectedActivity
        {
            get
            {
                if(!Delivery.IsOnRoute(Itinerary, RouteSpecification))
                    return null;

                return Delivery.IsUnloadedIn(CustomsClearancePoint)
                    ? HandlingActivity.CustomsIn(CustomsClearancePoint)
                    : Itinerary.ActivitySucceeding(Delivery.MostRecentPhysicalHandlingActivity);
            }
        }

        /// <summary>
        /// True if cargo is misdirected.
        /// </summary>
        public virtual bool IsMisdirected
        {
            get { return Delivery.IsMisdirected(Itinerary); }
        }

        /// <summary>
        /// Transport status.
        /// </summary>
        public virtual TransportStatus TransportStatus
        {
            get { return Delivery.TransportStatus; }
        }

        /// <summary>
        /// Routing status.
        /// </summary>
        public virtual RoutingStatus RoutingStatus
        {
            get { return RouteSpecification.StatusOf(Itinerary); }
        }

        /// <summary>
        /// Current voyage.
        /// </summary>
        public virtual Voyage CurrentVoyage
        {
            get { return Delivery.CurrentVoyage; }
        }

        /// <summary>
        /// Last known location.
        /// </summary>
        public virtual Location LastKnownLocation
        {
            get { return Delivery.LastKnownLocation; }
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
        /// <see cref="Freight.RouteSpecification"/> and <see cref="Freight.Itinerary"/> are both inside the Cargo
        /// aggregate, so changes to them cause the status to be updated <b>synchronously</b>,
        /// but handling cause the status update to happen <b>asynchronously</b>
        /// since <see cref="HandlingEvent"/> is in a different aggregate.
        /// </remarks>
        /// <param name="handlingActivity">handling activity</param>
        public virtual void Handled(HandlingActivity handlingActivity)
        {
            Validate.notNull(handlingActivity, "Handling activity is required");

            if(SuccedsMostRecentActivity(handlingActivity))
                Delivery = Delivery.OnHandling(handlingActivity);
        }

        /// <summary>
        /// Specifies a new route for this cargo.
        /// </summary>
        /// <param name="routeSpecification">route specification.</param>
        public virtual void SpecifyNewRoute(RouteSpecification routeSpecification)
        {
            Validate.notNull(routeSpecification, "Route specification is required");
            RouteSpecification = routeSpecification;
        }

        /// <summary>
        /// Attach a new itinerary to this cargo.
        /// </summary>
        /// <param name="itinerary">an itinerary. May not be null.</param>
        public virtual void AssignToRoute(Itinerary itinerary)
        {
            Validate.notNull(itinerary, "Itinerary is required");

            if(RoutingStatus != RoutingStatus.NOT_ROUTED)
                Delivery = Delivery.OnRouting();

            Itinerary = itinerary;
        }

        /// <summary>
        /// Customs zone.
        /// </summary>
        /// <value>Customs zone.</value>
        public virtual CustomsZone CustomsZone
        {
            get { return RouteSpecification.Destination.CustomsZone; }
        }

        /// <summary>
        /// Customs clearance point.
        /// </summary>
        /// <value>Customs clearance point.</value>
        public virtual Location CustomsClearancePoint
        {
            get
            {
                return RoutingStatus == RoutingStatus.NOT_ROUTED
                    ? Location.None
                    : CustomsZone.EntryPoint(Itinerary.Locations);
            }
        }

        /// <summary>
        /// True if the cargo is ready to be claimed.
        /// </summary>
        /// <value>True if the cargo is ready to be claimed.</value>
        public virtual bool IsReadyToClaim
        {
            get
            {
                return CustomsClearancePoint.sameAs(RouteSpecification.Destination)
                    ? HandlingActivity.CustomsIn(CustomsClearancePoint).sameValueAs(MostRecentHandlingActivity)
                    : Delivery.IsUnloadedIn(RouteSpecification.Destination);
            }
        }

        /// <summary>
        /// Most recent handling activity, or null if never handled.
        /// </summary>
        /// <value>Most recent handling activity, or null if never handled.</value>
        public virtual HandlingActivity MostRecentHandlingActivity
        {
            get { return Delivery.MostRecentHandlingActivity; }
        }

        /// <summary>
        /// The earliest rerouting location.
        /// </summary>
        /// <remarks>
        /// If the cargo is in port, it's the current location.
        /// If it's onboard a carrier it's the next arrival location.
        /// </remarks>
        /// <value>The earliest rerouting location.</value>
        public virtual Location EarliestReroutingLocation
        {
            get
            {
                if(IsMisdirected)
                    return TransportStatus == TransportStatus.ONBOARD_CARRIER
                        ? CurrentVoyage.ArrivalLocationWhenDepartedFrom(LastKnownLocation)
                        : LastKnownLocation;

                return Itinerary.MatchLeg(Delivery.MostRecentPhysicalHandlingActivity).Leg.UnloadLocation;
            }
        }

        /// <summary>
        /// Merges the current <see cref="Freight.Itinerary"/> with the provided <see cref="Freight.Itinerary"/>
        /// </summary>
        /// <param name="other">itinerary</param>
        /// <returns>A merge between the current itinerary and the provided itinerary
        /// that describes a continuous route even if the cargo is currently misdirected.</returns>
        public virtual Itinerary ItineraryMergedWith(Itinerary other)
        {
            if(RoutingStatus == RoutingStatus.NOT_ROUTED)
                return other;

            if(IsMisdirected && TransportStatus == TransportStatus.ONBOARD_CARRIER)
            {
                var currentLeg = Leg.DeriveLeg(CurrentVoyage, LastKnownLocation,
                    CurrentVoyage.ArrivalLocationWhenDepartedFrom(LastKnownLocation));

                return Itinerary.TruncatedAfter(LastKnownLocation).WithLeg(currentLeg).AppendBy(other);
            }
            
            return Itinerary.TruncatedAfter(EarliestReroutingLocation).AppendBy(other);
        }

        private bool SuccedsMostRecentActivity(HandlingActivity newHandlingActivity)
        {
            if(!Delivery.HasBeenHandled)
                return true;

            var mostRecentPhysicalHandlingActivity = Delivery.MostRecentPhysicalHandlingActivity;
            var priorActivity = Itinerary.StrictlyPriorOf(mostRecentPhysicalHandlingActivity, newHandlingActivity);
            return !newHandlingActivity.sameValueAs(priorActivity);
        }

        public override string ToString()
        {
            return TrackingId + " (" + RouteSpecification + ")";
        }

        protected internal Cargo()
        {
        }
    }
}