using Apache.NMS;

using DomainDrivenDelivery.Application.Event;
using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Handling;

using Spring.Messaging.Nms.Core;

namespace DomainDrivenDelivery.Infrastructure.Messaging.Nms
{
    /// <summary>
    /// NMS based implementation
    /// </summary>
    public sealed class NmsSystemEventsImpl : SystemEvents
    {
        private readonly INmsOperations nmsOperations;
        private readonly IDestination cargoHandledDestination;
        private readonly IDestination cargoUpdateDestination;

        public NmsSystemEventsImpl(INmsOperations nmsOperations,
                                   IDestination cargoHandledDestination,
                                   IDestination cargoUpdateDestination)
        {
            this.nmsOperations = nmsOperations;
            this.cargoHandledDestination = cargoHandledDestination;
            this.cargoUpdateDestination = cargoUpdateDestination;
        }

        public void notifyOfHandlingEvent(HandlingEvent @event)
        {
            var cargo = @event.Cargo;
            nmsOperations.SendWithDelegate(cargoHandledDestination, s => s.CreateObjectMessage(cargo.TrackingId));
        }

        public void notifyOfCargoUpdate(Cargo cargo)
        {
            nmsOperations.SendWithDelegate(cargoUpdateDestination, s => s.CreateObjectMessage(cargo.TrackingId));
        }
    }
}