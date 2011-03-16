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
        /// <summary>
        /// the legs of this itinerary, as an <b>immutable</b> list.
        /// </summary>
        public IEnumerable<Leg> Legs { get; private set; }

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

            Legs = legs;
        }

        public Itinerary(params Leg[] legs)
            : this((IEnumerable<Leg>)legs)
        {
        }

        /// <summary>
        /// A new itinerary which is a copy of the old one, adjusted for the delay of the given voyage.
        /// </summary>
        /// <param name="rescheduledVoyage">the voyage that has been rescheduled</param>
        /// <returns>A new itinerary which is a copy of the old one, adjusted for the delay of the given voyage.</returns>
        public Itinerary WithRescheduledVoyage(Voyage rescheduledVoyage)
        {
            var newLegsList = new List<Leg>(Legs.Count());

            Leg lastAdded = null;
            foreach(var leg in Legs)
            {
                if(leg.Voyage.sameAs(rescheduledVoyage))
                {
                    var modifiedLeg = leg.WithRescheduledVoyage(rescheduledVoyage);

                    // This truncates the itinerary if the voyage rescheduling makes
                    // it impossible to maintain the old unload-load chain.
                    if(lastAdded != null && modifiedLeg.LoadTime < lastAdded.UnloadTime)
                        break;

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
        public bool IsExpectedActivity(HandlingActivity handlingActivity)
        {
            return MatchLeg(handlingActivity).Leg != null;
        }

        /// <summary>
        /// The initial departure location.
        /// </summary>
        /// <value>The initial departure location.</value>
        internal Location InitialLoadLocation
        {
            get { return FirstLeg.LoadLocation; }
        }

        /// <summary>
        /// The final arrival location.
        /// </summary>
        /// <value>The final arrival location.</value>
        internal Location FinalUnloadLocation
        {
            get { return LastLeg.UnloadLocation; }
        }

        /// <summary>
        /// Date when cargo arrives at final destination.
        /// </summary>
        /// <value>Date when cargo arrives at final destination.</value>
        internal DateTime FinalUnloadTime
        {
            get { return LastLeg.UnloadTime; }
        }

        /// <summary>
        /// A list of all locations on this itinerary.
        /// </summary>
        /// <value>A list of all locations on this itinerary.</value>
        internal List<Location> Locations
        {
            get
            {
                var result = new List<Location>(Legs.Count() + 1) { FirstLeg.LoadLocation };
                result.AddRange(Legs.Select(leg => leg.UnloadLocation));
                return result;
            }
        }

        /// <summary>
        /// Finds the load time at the specified location.
        /// </summary>
        /// <param name="location">a location</param>
        /// <returns>Load time at this location, or null if the location isn't on this itinerary.</returns>
        public DateTime LoadTimeAt(Location location)
        {
            foreach(var leg in Legs)
            {
                if(leg.LoadLocation.sameAs(location))
                {
                    return leg.LoadTime;
                }
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// Estimated time of arrival
        /// </summary>
        /// <value>Estimated time of arrival</value>
        internal DateTime EstimatedTimeOfArrival
        {
            get { return FinalUnloadTime; }
        }

        /// <summary>
        /// Finds the unload time at the specified location.
        /// </summary>
        /// <param name="location">a location</param>
        /// <returns>Unload time at this location, or null if the location isn't on this itinerary.</returns>
        internal DateTime UnloadTimeAt(Location location)
        {
            foreach(var leg in Legs)
            {
                if(leg.UnloadLocation.sameAs(location))
                {
                    return leg.UnloadTime;
                }
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// Determines the handling activity suceeding the specified activity.
        /// </summary>
        /// <param name="previousActivity">previous handling activity</param>
        /// <returns>Handling activity that succeeds, or null if it can't be determined.</returns>
        internal HandlingActivity ActivitySucceeding(HandlingActivity previousActivity)
        {
            return previousActivity == null
                ? HandlingActivity.ReceiveIn(FirstLeg.LoadLocation)
                : DeriveFromMatchingLeg(previousActivity, MatchLeg(previousActivity).Leg);
        }

        /// <summary>
        /// Gets the activity which is strictly prior to the other, according to the itinerary, or null if neither is strictly prior.
        /// </summary>
        /// <param name="handlingActivity1">handling activity</param>
        /// <param name="handlingActivity2">handling activity</param>
        /// <returns>The activity which is strictly prior to the other, according to the itinerary, or null if neither is strictly prior.</returns>
        internal HandlingActivity StrictlyPriorOf(HandlingActivity handlingActivity1, HandlingActivity handlingActivity2)
        {
            var match1 = MatchLeg(handlingActivity1);
            var match2 = MatchLeg(handlingActivity2);
            var compared = match1.CompareTo(match2);

            if(compared < 0)
                return match1.HandlingActivity;
            else if(compared > 0)
                return match2.HandlingActivity;
            else
                return null;
        }

        /// <summary>
        /// Gets the leg after the specified leg.
        /// </summary>
        /// <param name="leg">leg</param>
        /// <returns>The next leg, or null if this is the last leg.</returns>
        internal Leg NextLeg(Leg leg)
        {
            for(var it = Legs.GetEnumerator(); it.MoveNext(); )
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
        internal LegActivityMatch MatchLeg(HandlingActivity handlingActivity)
        {
            if(handlingActivity == null)
            {
                return LegActivityMatch.NoMatch(handlingActivity, this);
            }
            else if(handlingActivity.Type == HandlingActivityType.RECEIVE)
            {
                return LegActivityMatch.IfLoadLocationSame(FirstLeg, handlingActivity, this);
            }
            else if(handlingActivity.Type == HandlingActivityType.CLAIM)
            {
                return LegActivityMatch.IfUnloadLocationSame(LastLeg, handlingActivity, this);
            }
            else
            {
                return FindLegMatchingActivity(handlingActivity);
            }
        }

        /// <summary>
        /// The first leg on the itinerary.
        /// </summary>
        /// <value>The first leg on the itinerary.</value>
        internal Leg FirstLeg
        {
            get { return Legs.First(); }
        }

        /// <summary>
        /// The last leg on the itinerary.
        /// </summary>
        /// <value>The last leg on the itinerary.</value>
        public Leg LastLeg
        {
            get { return Legs.Last(); }
        }

        internal Itinerary TruncatedAfter(Location location)
        {
            var newLegs = new List<Leg>();

            foreach(var leg in Legs)
            {
                if(leg.Voyage.Locations.Contains(location))
                {
                    newLegs.Add(Leg.DeriveLeg(leg.Voyage, leg.LoadLocation, location));
                    break;
                }
                else
                {
                    newLegs.Add(leg);
                    if(leg.UnloadLocation.sameAs(location))
                    {
                        break;
                    }
                }
            }

            return new Itinerary(newLegs);
        }

        internal Itinerary WithLeg(Leg leg)
        {
            var newLegs = new List<Leg>(Legs.Count() + 1);
            newLegs.AddRange(Legs);
            newLegs.Add(leg);

            return new Itinerary(newLegs);
        }

        internal Itinerary AppendBy(Itinerary other)
        {
            var newLegs = new List<Leg>(Legs.Count() + other.Legs.Count());
            newLegs.AddRange(Legs);
            newLegs.AddRange(other.Legs);

            return new Itinerary(newLegs);
        }

        private LegActivityMatch FindLegMatchingActivity(HandlingActivity handlingActivity)
        {
            foreach(var leg in Legs)
            {
                if(leg.MatchesActivity(handlingActivity))
                {
                    return LegActivityMatch.Match(leg, handlingActivity, this);
                }
            }

            return LegActivityMatch.NoMatch(handlingActivity, this);
        }

        private HandlingActivity DeriveFromMatchingLeg(HandlingActivity handlingActivity, Leg matchingLeg)
        {
            if(matchingLeg == null)
            {
                return null;
            }
            if(handlingActivity.Type == HandlingActivityType.LOAD)
            {
                return matchingLeg.DeriveUnloadActivity();
            }
            else if(handlingActivity.Type == HandlingActivityType.UNLOAD)
            {
                return DeriveFromNextLeg(NextLeg(matchingLeg));
            }
            else
            {
                // Will only derive from load and unload within the itinerary context
                return null;
            }
        }

        private HandlingActivity DeriveFromNextLeg(Leg nextLeg)
        {
            return nextLeg == null ? HandlingActivity.ClaimIn(LastLeg.UnloadLocation) : nextLeg.DeriveLoadActivity();
        }

        public override string ToString()
        {
            return String.Join("\n", Legs);
        }

        protected internal Itinerary()
        {
            Legs = new List<Leg>();
        }
    }
}