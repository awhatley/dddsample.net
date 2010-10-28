using System.Collections.Generic;

using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Travel;

namespace DomainDrivenDelivery.Infrastructure.Persistence.InMemory
{
    /// <summary>
    /// CargoRepositoryInMem implement the CargoRepository interface but is a test
    /// class not intended for usage in real application.
    /// </summary>
    /// <remarks>
    /// It setup a simple local hash with a number of Cargo's with TrackingId as key
    /// defined at compile time.
    /// </remarks>
    public class CargoRepositoryInMem : CargoRepository
    {
        private readonly IDictionary<TrackingId, Cargo> cargoDb = new Dictionary<TrackingId, Cargo>();

        public Cargo find(TrackingId trackingId)
        {
            return cargoDb[trackingId];
        }

        public IEnumerable<Cargo> findCargosOnVoyage(Voyage voyage)
        {
            var onVoyage = new List<Cargo>();
            foreach(Cargo cargo in cargoDb.Values)
            {
                if(voyage.sameAs(cargo.currentVoyage()))
                {
                    onVoyage.Add(cargo);
                }
            }

            return onVoyage;
        }

        public void store(Cargo cargo)
        {
            cargoDb[cargo.trackingId()] = cargo;
        }

        public IEnumerable<Cargo> findAll()
        {
            return new List<Cargo>(cargoDb.Values);
        }
    }
}