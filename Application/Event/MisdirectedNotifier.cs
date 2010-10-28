using DomainDrivenDelivery.Domain.Model.Freight;

using Dotnet.Commons.Logging;

using Spring.Stereotype;
using Spring.Transaction.Interceptor;

namespace DomainDrivenDelivery.Application.Event
{
    [Service]
    public class MisdirectedNotifier
    {
        private CargoRepository cargoRepository;

        private static readonly ILog LOG = LogFactory.GetLogger(typeof(MisdirectedNotifier));

        public MisdirectedNotifier(CargoRepository cargoRepository)
        {
            this.cargoRepository = cargoRepository;
        }

        [Transaction]
        public void alertIfMisdirected(TrackingId trackingId)
        {
            var cargo = cargoRepository.find(trackingId);

            if(cargo.isMisdirected())
            {
                /**
                 * In a real system, some significant action would be taken
                 * when this happens.
                 */
                LOG.Info("Cargo " + cargo + " is misdirected!");
            }
        }

        MisdirectedNotifier()
        {
        }
    }
}