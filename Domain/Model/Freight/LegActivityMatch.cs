using System;
using System.Linq;

using DomainDrivenDelivery.Domain.Model.Shared;
using DomainDrivenDelivery.Domain.Patterns.ValueObject;

namespace DomainDrivenDelivery.Domain.Model.Freight
{
    internal class LegActivityMatch : ValueObjectSupport<LegActivityMatch>, IComparable<LegActivityMatch>
    {
        public virtual Leg Leg { get; private set; }
        public virtual HandlingActivity HandlingActivity { get; private set; }
        public virtual LegEnd LegEnd { get; private set; }
        public virtual Itinerary Itinerary { get; private set; }

        private LegActivityMatch(Leg leg, LegEnd legEnd, HandlingActivity handlingActivity, Itinerary itinerary)
        {
            Leg = leg;
            LegEnd = legEnd;
            HandlingActivity = handlingActivity;
            Itinerary = itinerary;
        }

        public static LegActivityMatch Match(Leg leg, HandlingActivity handlingActivity, Itinerary itinerary)
        {
            switch(handlingActivity.Type)
            {
                case HandlingActivityType.RECEIVE:
                case HandlingActivityType.LOAD:
                    return new LegActivityMatch(leg, LegEnd.LoadEnd, handlingActivity, itinerary);

                case HandlingActivityType.UNLOAD:
                case HandlingActivityType.CLAIM:
                case HandlingActivityType.CUSTOMS:
                    return new LegActivityMatch(leg, LegEnd.UnloadEnd, handlingActivity, itinerary);

                default:
                    return NoMatch(handlingActivity, itinerary);
            }
        }

        public static LegActivityMatch IfLoadLocationSame(Leg leg,
            HandlingActivity handlingActivity,
            Itinerary itinerary)
        {
            return leg.LoadLocation.sameAs(handlingActivity.Location)
                ? new LegActivityMatch(leg, LegEnd.LoadEnd, handlingActivity, itinerary)
                : NoMatch(handlingActivity, itinerary);
        }

        public static LegActivityMatch IfUnloadLocationSame(Leg leg,
            HandlingActivity handlingActivity,
            Itinerary itinerary)
        {
            return leg.UnloadLocation.sameAs(handlingActivity.Location)
                ? new LegActivityMatch(leg, LegEnd.UnloadEnd, handlingActivity, itinerary)
                : NoMatch(handlingActivity, itinerary);
        }

        public static LegActivityMatch NoMatch(HandlingActivity handlingActivity, Itinerary itinerary)
        {
            return new LegActivityMatch(null, LegEnd.NoEnd, handlingActivity, itinerary);
        }

        public virtual int CompareTo(LegActivityMatch other)
        {
            var thisLegIndex = Itinerary.Legs.ToList().IndexOf(Leg);
            var otherLegIndex = Itinerary.Legs.ToList().IndexOf(other.Leg);

            return thisLegIndex.Equals(otherLegIndex)
                ? LegEnd.CompareTo(other.LegEnd)
                : ToPositive(thisLegIndex).CompareTo(ToPositive(otherLegIndex));
        }

        private static int ToPositive(int thisLegIndex)
        {
            return thisLegIndex >= 0 ? thisLegIndex : Int32.MaxValue;
        }

        public override string ToString()
        {
            return LegEnd == LegEnd.NoEnd
                ? "No match"
                : "Activity " + HandlingActivity + " matches leg " + Leg + " at " + LegEnd;
        }
    }

    internal enum LegEnd
    {
        LoadEnd,
        UnloadEnd,
        NoEnd
    }
}