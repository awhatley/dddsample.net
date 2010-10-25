using DomainDrivenDelivery.Domain.Model.Travel;

namespace DomainDrivenDelivery.Infrastructure.Persistence.InMemory
{
    public sealed class VoyageRepositoryInMem : VoyageRepository
    {
        public Voyage find(VoyageNumber voyageNumber)
        {
            return SampleVoyages.lookup(voyageNumber);
        }
    }
}