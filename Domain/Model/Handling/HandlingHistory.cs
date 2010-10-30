using System;
using System.Collections.Generic;
using System.Linq;

using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Shared;
using DomainDrivenDelivery.Domain.Patterns;
using DomainDrivenDelivery.Domain.Patterns.ValueObject;
using DomainDrivenDelivery.Utilities;

namespace DomainDrivenDelivery.Domain.Model.Handling
{
    /// <summary>
    /// The handling history of a cargo.
    /// </summary>
    // TODO: eliminate from 1.2
    public class HandlingHistory : ValueObject<HandlingHistory>
    {
        private readonly IEnumerable<HandlingEvent> _handlingEvents;
        private readonly Cargo _cargo;

        public static HandlingHistory emptyForCargo(Cargo cargo)
        {
            return new HandlingHistory(cargo);
        }

        public static HandlingHistory fromEvents(IEnumerable<HandlingEvent> handlingEvents)
        {
            return new HandlingHistory(handlingEvents);
        }

        private HandlingHistory(Cargo cargo)
        {
            Validate.notNull(cargo, "Cargo is required");
            this._cargo = cargo;
            _handlingEvents = new List<HandlingEvent>();
        }

        private HandlingHistory(IEnumerable<HandlingEvent> handlingEvents)
        {
            Validate.notEmpty(handlingEvents, "Handling events are required");

            this._cargo = uniqueCargo(handlingEvents);
            this._handlingEvents = new List<HandlingEvent>(handlingEvents);
        }

        private Cargo uniqueCargo(IEnumerable<HandlingEvent> handlingEvents)
        {
            var it = handlingEvents.GetEnumerator();
            it.MoveNext();

            var firstCargo = it.Current.cargo();
            Validate.notNull(firstCargo, "Cargo is required");

            while(it.MoveNext())
            {
                var nextCargo = it.Current.cargo();
                Validate.isTrue(firstCargo.sameAs(nextCargo),
                  "A handling history can only contain handling events for a unique cargo. " +
                    "First event is for cargo " + firstCargo + ", also discovered cargo " + nextCargo
                );
            }

            return firstCargo;
        }

        /// <summary>
        /// A distinct list (no duplicate registrations) of handling events, ordered by completion time.
        /// </summary>
        /// <returns>A distinct list (no duplicate registrations) of handling events, ordered by completion time.</returns>
        public IEnumerable<HandlingEvent> distinctEventsByCompletionTime()
        {
            var set = new HashSet<HandlingEvent>(_handlingEvents);
            var ordered = new List<HandlingEvent>(set);
            ordered.Sort(BY_COMPLETION_TIME_COMPARATOR);
            return ordered.AsReadOnly();
        }

        /// <summary>
        /// Filter a list of handling events, returning only the LOAD and UNLOAD events.
        /// </summary>
        /// <param name="unfilteredEvents">The events to filter.</param>
        /// <returns>Filter a list of handling events, returning only the LOAD and UNLOAD events.</returns>
        public IEnumerable<HandlingEvent> physicalHandlingEvents(IEnumerable<HandlingEvent> unfilteredEvents)
        {
            var filtered = new List<HandlingEvent>();
            foreach(HandlingEvent @event in unfilteredEvents)
            {
                if(@event.type().Equals(HandlingActivityType.RECEIVE)) filtered.Add(@event);
                if(@event.type().Equals(HandlingActivityType.LOAD)) filtered.Add(@event);
                if(@event.type().Equals(HandlingActivityType.UNLOAD)) filtered.Add(@event);
                if(@event.type().Equals(HandlingActivityType.CLAIM)) filtered.Add(@event);
            }
            return filtered.AsReadOnly();
        }

        /// <summary>
        /// Most recently completed event, or null if the handling history is empty.
        /// </summary>
        /// <returns>Most recently completed event, or null if the handling history is empty.</returns>
        public HandlingEvent mostRecentlyCompletedEvent()
        {
            var distinctEvents = distinctEventsByCompletionTime();
            if(!distinctEvents.Any())
            {
                return null;
            }
            else
            {
                return distinctEvents.Last();
            }
        }

        /// <summary>
        /// Most recently completed load or unload, or null if there have been none.
        /// </summary>
        /// <returns>Most recently completed load or unload, or null if there have been none.</returns>
        public HandlingEvent mostRecentPhysicalHandling()
        {
            var loadsAndUnloads = physicalHandlingEvents(distinctEventsByCompletionTime());
            if(!loadsAndUnloads.Any())
            {
                return null;
            }
            else
            {
                return loadsAndUnloads.Last();
            }
        }

        /// <summary>
        /// The cargo to which this handling history refers.
        /// </summary>
        /// <returns>The cargo to which this handling history refers.</returns>
        public Cargo cargo()
        {
            return _cargo;
        }

        public bool sameValueAs(HandlingHistory other)
        {
            return other != null && this._handlingEvents.Equals(other._handlingEvents);
        }

        public override bool Equals(object o)
        {
            if(this == o) return true;
            if(o == null || GetType() != o.GetType()) return false;

            var other = (HandlingHistory)o;
            return sameValueAs(other);
        }

        public override int GetHashCode()
        {
            return _handlingEvents.GetHashCode();
        }

        private static readonly Comparison<HandlingEvent> BY_COMPLETION_TIME_COMPARATOR =
            (he1, he2) => he1.completionTime().CompareTo(he2.completionTime());
    }
}