using System;
using System.Collections.Generic;

using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Handling;

namespace DomainDrivenDelivery.Infrastructure.Persistence.InMemory
{
    public class HandlingEventRepositoryInMem : HandlingEventRepository
    {
        private Dictionary<TrackingId, List<HandlingEvent>> eventMap = new Dictionary<TrackingId, List<HandlingEvent>>();
        private static readonly Comparison<HandlingEvent> BY_COMPLETION_TIME_DESC = (o1, o2) =>
            o2.CompletionTime.CompareTo(o1.CompletionTime);

        public HandlingEvent find(EventSequenceNumber eventSequenceNumber)
        {
            foreach(IEnumerable<HandlingEvent> handlingEvents in eventMap.Values)
            {
                foreach(HandlingEvent handlingEvent in handlingEvents)
                {
                    if(handlingEvent.SequenceNumber.sameValueAs(eventSequenceNumber))
                    {
                        return handlingEvent;
                    }
                }
            }

            return null;
        }

        public void store(HandlingEvent @event)
        {
            var trackingId = @event.Cargo.TrackingId;
            List<HandlingEvent> list;
            if(eventMap.TryGetValue(trackingId, out list) == false)
                eventMap[trackingId] = list = new List<HandlingEvent>();

            list.Add(@event);
        }

        public HandlingHistory lookupHandlingHistoryOfCargo(Cargo cargo)
        {
            var events = eventMap[cargo.TrackingId];

            if(events == null)
            {
                return HandlingHistory.emptyForCargo(cargo);
            }
            else
            {
                return HandlingHistory.fromEvents(events);
            }
        }

        public HandlingEvent mostRecentHandling(Cargo cargo)
        {
            var handlingEvents = eventMap[cargo.TrackingId];
            if(handlingEvents == null)
            {
                return null;
            }

            handlingEvents.Sort(BY_COMPLETION_TIME_DESC);
            return handlingEvents[0];
        }
    }
}