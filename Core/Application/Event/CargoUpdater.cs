using DomainDrivenDelivery.Domain.Model.Frieght;
using DomainDrivenDelivery.Domain.Model.Handling;

using Dotnet.Commons.Logging;

using Spring.Stereotype;
using Spring.Transaction.Interceptor;

namespace DomainDrivenDelivery.Application.Event
{
    [Service]
    public class CargoUpdater
    {
        private SystemEvents systemEvents;
        private CargoRepository cargoRepository;
        private HandlingEventRepository handlingEventRepository;
        private readonly ILog logger = LogFactory.GetLogger(typeof(CargoUpdater));

        public CargoUpdater(SystemEvents systemEvents,
                            CargoRepository cargoRepository,
                            HandlingEventRepository handlingEventRepository)
        {
            this.systemEvents = systemEvents;
            this.cargoRepository = cargoRepository;
            this.handlingEventRepository = handlingEventRepository;
        }

        [Transaction]
        public void updateCargo(EventSequenceNumber sequenceNumber)
        {
            var handlingEvent = handlingEventRepository.find(sequenceNumber);
            if(handlingEvent == null)
            {
                logger.Error("Could not find any handling event with sequence number " + sequenceNumber);
                return;
            }

            var activity = handlingEvent.activity().copy();
            var cargo = handlingEvent.cargo();

            cargo.handled(activity);
            cargoRepository.store(cargo);

            systemEvents.notifyOfCargoUpdate(cargo);
            logger.Info("Updated cargo " + cargo);
        }

        CargoUpdater()
        {
            // Needed by CGLIB
        }
    }
}