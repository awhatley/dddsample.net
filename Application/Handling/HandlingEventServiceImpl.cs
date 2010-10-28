using System;

using DomainDrivenDelivery.Application.Event;
using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Handling;
using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Model.Shared;
using DomainDrivenDelivery.Domain.Model.Travel;

using Dotnet.Commons.Logging;

using Spring.Stereotype;
using Spring.Transaction.Interceptor;

namespace DomainDrivenDelivery.Application.Handling
{
    [Service]
    public sealed class HandlingEventServiceImpl : HandlingEventService
    {
        private readonly SystemEvents systemEvents;
        private readonly HandlingEventRepository handlingEventRepository;
        private readonly HandlingEventFactory handlingEventFactory;

        private static readonly ILog LOG = LogFactory.GetLogger(typeof(HandlingEventServiceImpl));

        public HandlingEventServiceImpl(HandlingEventRepository handlingEventRepository,
                                        SystemEvents systemEvents,
                                        HandlingEventFactory handlingEventFactory)
        {
            this.handlingEventRepository = handlingEventRepository;
            this.systemEvents = systemEvents;
            this.handlingEventFactory = handlingEventFactory;
        }

        [Transaction]
        public void registerHandlingEvent(DateTime completionTime, TrackingId trackingId,
                                          VoyageNumber voyageNumber, UnLocode unLocode,
                                          HandlingActivityType type, OperatorCode operatorCode)
        {
            // Using a factory to create a HandlingEvent (aggregate). This is where
            // it is determined wether the incoming data, the attempt, actually is capable
            // of representing a real handling handlingEvent.
            var handlingEvent = handlingEventFactory.createHandlingEvent(
              completionTime, trackingId, voyageNumber, unLocode, type, operatorCode
            );

            // Store the new handling handlingEvent, which updates the persistent
            // state of the handling handlingEvent aggregate (but not the cargo aggregate -
            // that happens asynchronously!)
            handlingEventRepository.store(handlingEvent);

            // Publish a system event
            systemEvents.notifyOfHandlingEvent(handlingEvent);

            LOG.Info("Registered handling event: " + handlingEvent);
        }
    }
}