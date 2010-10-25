using System;
using System.Linq;

using DomainDrivenDelivery.Domain.Model.Shared;
using DomainDrivenDelivery.Domain.Patterns.ValueObject;

namespace DomainDrivenDelivery.Domain.Model.Frieght
{
    public class LegActivityMatch : ValueObjectSupport<LegActivityMatch>, IComparable<LegActivityMatch>
    {
        private readonly Leg _leg;
        private readonly LegEnd _legEnd;
        private readonly HandlingActivity _handlingActivity;
        private readonly Itinerary _itinerary;

        private LegActivityMatch(Leg leg, LegEnd legEnd, HandlingActivity handlingActivity, Itinerary itinerary)
        {
            this._leg = leg;
            this._legEnd = legEnd;
            this._handlingActivity = handlingActivity;
            this._itinerary = itinerary;
        }

        internal static LegActivityMatch match(Leg leg, HandlingActivity handlingActivity, Itinerary itinerary)
        {
            switch(handlingActivity.type())
            {
                case HandlingActivityType.RECEIVE:
                case HandlingActivityType.LOAD:
                    return new LegActivityMatch(leg, LegEnd.LOAD_END, handlingActivity, itinerary);

                case HandlingActivityType.UNLOAD:
                case HandlingActivityType.CLAIM:
                case HandlingActivityType.CUSTOMS:
                    return new LegActivityMatch(leg, LegEnd.UNLOAD_END, handlingActivity, itinerary);

                default:
                    return noMatch(handlingActivity, itinerary);
            }
        }

        internal static LegActivityMatch ifLoadLocationSame(Leg leg, HandlingActivity handlingActivity, Itinerary itinerary)
        {
            if(leg.loadLocation().sameAs(handlingActivity.location()))
            {
                return new LegActivityMatch(leg, LegEnd.LOAD_END, handlingActivity, itinerary);
            }
            else
            {
                return noMatch(handlingActivity, itinerary);
            }
        }

        internal static LegActivityMatch ifUnloadLocationSame(Leg leg, HandlingActivity handlingActivity, Itinerary itinerary)
        {
            if(leg.unloadLocation().sameAs(handlingActivity.location()))
            {
                return new LegActivityMatch(leg, LegEnd.UNLOAD_END, handlingActivity, itinerary);
            }
            else
            {
                return noMatch(handlingActivity, itinerary);
            }
        }

        internal static LegActivityMatch noMatch(HandlingActivity handlingActivity, Itinerary itinerary)
        {
            return new LegActivityMatch(null, LegEnd.NO_END, handlingActivity, itinerary);
        }

        internal Leg leg()
        {
            return _leg;
        }

        internal HandlingActivity handlingActivity()
        {
            return _handlingActivity;
        }

        public int CompareTo(LegActivityMatch other)
        {
            var thisLegIndex = _itinerary.legs().Where(l => l == this._leg).Select((l, i) => i).First();
            var otherLegIndex = _itinerary.legs().Where(l => l == other._leg).Select((l, i) => i).First();

            if(thisLegIndex.Equals(otherLegIndex))
            {
                return this._legEnd.CompareTo(other._legEnd);
            }
            else
            {
                return toPositive(thisLegIndex).CompareTo(toPositive(otherLegIndex));
            }
        }

        private int toPositive(int thisLegIndex)
        {
            return thisLegIndex >= 0 ? thisLegIndex : Int32.MaxValue;
        }

        public override string ToString()
        {
            if(_legEnd == LegEnd.NO_END)
            {
                return "No match";
            }
            else
            {
                return "Activity " + _handlingActivity + " matches leg " + _leg + " at " + _legEnd;
            }
        }

        enum LegEnd
        {
            LOAD_END,
            UNLOAD_END,
            NO_END
        }
    }
}