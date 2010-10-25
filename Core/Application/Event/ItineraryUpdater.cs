using DomainDrivenDelivery.Domain.Model.Frieght;
using DomainDrivenDelivery.Domain.Model.Travel;

using Dotnet.Commons.Logging;

using Spring.Transaction.Interceptor;

namespace DomainDrivenDelivery.Application.Event
{
    public class ItineraryUpdater
    {
        private VoyageRepository voyageRepository;
        private CargoRepository cargoRepository;
        private static readonly ILog LOG = LogFactory.GetLogger(typeof(ItineraryUpdater));

        public ItineraryUpdater(VoyageRepository voyageRepository, CargoRepository cargoRepository)
        {
            this.voyageRepository = voyageRepository;
            this.cargoRepository = cargoRepository;
        }

        [Transaction]
        public void updateItineraries(VoyageNumber voyageNumber)
        {
            var voyage = voyageRepository.find(voyageNumber);
            var affectedCargos = cargoRepository.findCargosOnVoyage(voyage);

            foreach(Cargo cargo in affectedCargos)
            {
                var newItinerary = cargo.itinerary().withRescheduledVoyage(voyage);
                cargo.assignToRoute(newItinerary);
                LOG.Info("Updated itinerary of cargo " + cargo);
            }
        }
    }
}