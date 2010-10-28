using System;
using System.Collections.Generic;
using System.Linq;

using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Model.Shared;
using DomainDrivenDelivery.Domain.Model.Travel;
using DomainDrivenDelivery.Domain.Patterns.ValueObject;
using DomainDrivenDelivery.Utilities;

namespace DomainDrivenDelivery.Domain.Model.Freight
{
    /// <summary>
    /// An itinerary.
    /// </summary>
    public class Itinerary : ValueObjectSupport<Itinerary>
    {
        private readonly IEnumerable<Leg> _legs;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="legs">List of legs for this itinerary.</param>
        public Itinerary(IEnumerable<Leg> legs)
        {
            Validate.notEmpty(legs);
            Validate.noNullElements(legs);

            // TODO
            // Validate that legs are in proper order, connected
            //var it = legs.GetEnumerator();
            //var leg = it.Current;
            //while(it.MoveNext())
            //{
            //    var nextLeg = it.Current;
            //    Validate.isTrue(leg.unloadTime().before(nextLeg.loadTime()));
            //    leg = nextLeg;
            //}

            _legs = legs;
        }

        public Itinerary(params Leg[] legs)
            : this((IEnumerable<Leg>)legs)
        {
        }

        /// <summary>
        /// the legs of this itinerary, as an <b>immutable</b> list.
        /// </summary>
        /// <returns>the legs of this itinerary, as an <b>immutable</b> list.</returns>
        public IEnumerable<Leg> legs()
        {
            return new List<Leg>(_legs).AsReadOnly();
        }

        /// <summary>
        /// A new itinerary which is a copy of the old one, adjusted for the delay of the given voyage.
        /// </summary>
        /// <param name="rescheduledVoyage">the voyage that has been rescheduled</param>
        /// <returns>A new itinerary which is a copy of the old one, adjusted for the delay of the given voyage.</returns>
        public Itinerary withRescheduledVoyage(Voyage rescheduledVoyage)
        {
            var newLegsList = new List<Leg>(this._legs.Count());

            Leg lastAdded = null;
            foreach(var leg in _legs)
            {
                if(leg.voyage().sameAs(rescheduledVoyage))
                {
                    Leg modifiedLeg = leg.withRescheduledVoyage(rescheduledVoyage);
                    // This truncates the itinerary if the voyage rescheduling makes
                    // it impossible to maintain the old unload-load chain.
                    if(lastAdded != null && modifiedLeg.loadTime() < lastAdded.unloadTime())
                    {
                        break;
                    }
                    newLegsList.Add(modifiedLeg);
                }
                else
                {
                    newLegsList.Add(leg);
                }
                lastAdded = leg;
            }

            return new Itinerary(newLegsList);
        }

        /// <summary>
        /// Test if the given handling event was expected when executing this itinerary.
        /// </summary>
        /// <param name="handlingActivity">Event to test.</param>
        /// <returns><code>true</code> if the event is expected</returns>
        internal bool isExpectedActivity(HandlingActivity handlingActivity)
        {
            return matchLeg(handlingActivity).leg() != null;
        }

        /// <summary>
        /// The initial departure location.
        /// </summary>
        /// <returns>The initial departure location.</returns>
        internal Location initialLoadLocation()
        {
            return firstLeg().loadLocation();
        }

        /// <summary>
        /// The final arrival location.
        /// </summary>
        /// <returns>The final arrival location.</returns>
        internal Location finalUnloadLocation()
        {
            return lastLeg().unloadLocation();
        }

        /// <summary>
        /// Date when cargo arrives at final destination.
        /// </summary>
        /// <returns>Date when cargo arrives at final destination.</returns>
        internal DateTime finalUnloadTime()
        {
            return lastLeg().unloadTime();
        }

        /// <summary>
        /// A list of all locations on this itinerary.
        /// </summary>
        /// <returns>A list of all locations on this itinerary.</returns>
        internal List<Location> locations()
        {
            var result = new List<Location>(_legs.Count() + 1);
            result.Add(firstLeg().loadLocation());
            foreach(var leg in _legs)
            {
                result.Add(leg.unloadLocation());
            }
            return result;
        }

        /// <summary>
        /// Finds the load time at the specified location.
        /// </summary>
        /// <param name="location">a location</param>
        /// <returns>Load time at this location, or null if the location isn't on this itinerary.</returns>
        public DateTime loadTimeAt(Location location)
        {
            foreach(var leg in _legs)
            {
                if(leg.loadLocation().sameAs(location))
                {
                    return leg.loadTime();
                }
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// Estimated time of arrival
        /// </summary>
        /// <returns>Estimated time of arrival</returns>
        internal DateTime estimatedTimeOfArrival()
        {
            return finalUnloadTime();
        }

        /// <summary>
        /// Finds the unload time at the specified location.
        /// </summary>
        /// <param name="location">a location</param>
        /// <returns>Unload time at this location, or null if the location isn't on this itinerary.</returns>
        internal DateTime unloadTimeAt(Location location)
        {
            foreach(var leg in _legs)
            {
                if(leg.unloadLocation().sameAs(location))
                {
                    return leg.unloadTime();
                }
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// Determines the handling activity suceeding the specified activity.
        /// </summary>
        /// <param name="previousActivity">previous handling activity</param>
        /// <returns>Handling activity that succeeds, or null if it can't be determined.</returns>
        internal HandlingActivity activitySucceeding(HandlingActivity previousActivity)
        {
            if(previousActivity == null)
            {
                return HandlingActivity.receiveIn(firstLeg().loadLocation());
            }
            else
            {
                return deriveFromMatchingLeg(previousActivity, matchLeg(previousActivity).leg());
            }
        }

        /// <summary>
        /// Gets the activity which is strictly prior to the other, according to the itinerary, or null if neither is strictly prior.
        /// </summary>
        /// <param name="handlingActivity1">handling activity</param>
        /// <param name="handlingActivity2">handling activity</param>
        /// <returns>The activity which is strictly prior to the other, according to the itinerary, or null if neither is strictly prior.</returns>
        internal HandlingActivity strictlyPriorOf(HandlingActivity handlingActivity1, HandlingActivity handlingActivity2)
        {
            var match1 = matchLeg(handlingActivity1);
            var match2 = matchLeg(handlingActivity2);
            var compared = match1.CompareTo(match2);

            if(compared < 0)
            {
                return match1.handlingActivity();
            }
            else if(compared > 0)
            {
                return match2.handlingActivity();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the leg after the specified leg.
        /// </summary>
        /// <param name="leg">leg</param>
        /// <returns>The next leg, or null if this is the last leg.</returns>
        internal Leg nextLeg(Leg leg)
        {
            for(var it = _legs.GetEnumerator(); it.MoveNext(); )
            {
                if(it.Current.sameValueAs(leg))
                {
                    return it.MoveNext() ? it.Current : null;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the leg match of this handling activity. Never null.
        /// </summary>
        /// <param name="handlingActivity">handling activity</param>
        /// <returns>The leg match of this handling activity. Never null.</returns>
        internal LegActivityMatch matchLeg(HandlingActivity handlingActivity)
        {
            if(handlingActivity == null)
            {
                return LegActivityMatch.noMatch(handlingActivity, this);
            }
            else if(handlingActivity.type() == HandlingActivityType.RECEIVE)
            {
                return LegActivityMatch.ifLoadLocationSame(firstLeg(), handlingActivity, this);
            }
            else if(handlingActivity.type() == HandlingActivityType.CLAIM)
            {
                return LegActivityMatch.ifUnloadLocationSame(lastLeg(), handlingActivity, this);
            }
            else
            {
                return findLegMatchingActivity(handlingActivity);
            }
        }

        /// <summary>
        /// The first leg on the itinerary.
        /// </summary>
        /// <returns>The first leg on the itinerary.</returns>
        internal Leg firstLeg()
        {
            return _legs.First();
        }

        /// <summary>
        /// The last leg on the itinerary.
        /// </summary>
        /// <returns>The last leg on the itinerary.</returns>
        public Leg lastLeg()
        {
            return _legs.Last();
        }

        internal Itinerary truncatedAfter(Location location)
        {
            var newLegs = new List<Leg>();

            foreach(var leg in _legs)
            {
                if(leg.voyage().locations().Contains(location))
                {
                    newLegs.Add(Leg.deriveLeg(leg.voyage(), leg.loadLocation(), location));
                    break;
                }
                else
                {
                    newLegs.Add(leg);
                    if(leg.unloadLocation().sameAs(location))
                    {
                        break;
                    }
                }
            }

            return new Itinerary(newLegs);
        }

        internal Itinerary withLeg(Leg leg)
        {
            var newLegs = new List<Leg>(_legs.Count() + 1);
            newLegs.AddRange(_legs);
            newLegs.Add(leg);

            return new Itinerary(newLegs);
        }

        internal Itinerary appendBy(Itinerary other)
        {
            var newLegs = new List<Leg>(this._legs.Count() + other._legs.Count());
            newLegs.AddRange(this._legs);
            newLegs.AddRange(other._legs);

            return new Itinerary(newLegs);
        }

        private LegActivityMatch findLegMatchingActivity(HandlingActivity handlingActivity)
        {
            foreach(var leg in _legs)
            {
                if(leg.matchesActivity(handlingActivity))
                {
                    return LegActivityMatch.match(leg, handlingActivity, this);
                }
            }

            return LegActivityMatch.noMatch(handlingActivity, this);
        }

        private HandlingActivity deriveFromMatchingLeg(HandlingActivity handlingActivity, Leg matchingLeg)
        {
            if(matchingLeg == null)
            {
                return null;
            }
            else
            {
                if(handlingActivity.type() == HandlingActivityType.LOAD)
                {
                    return matchingLeg.deriveUnloadActivity();
                }
                else if(handlingActivity.type() == HandlingActivityType.UNLOAD)
                {
                    return deriveFromNextLeg(nextLeg(matchingLeg));
                }
                else
                {
                    // Will only derive from load and unload within the itinerary context
                    return null;
                }
            }
        }

        private HandlingActivity deriveFromNextLeg(Leg nextLeg)
        {
            if(nextLeg == null)
            {
                return HandlingActivity.claimIn(lastLeg().unloadLocation());
            }
            else
            {
                return nextLeg.deriveLoadActivity();
            }
        }


        public override string ToString()
        {
            return String.Join("\n", _legs);
        }

        Itinerary()
        {
            // Needed by Hibernate
            _legs = new List<Leg>();
        }
    }
}