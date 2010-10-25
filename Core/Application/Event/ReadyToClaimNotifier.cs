using DomainDrivenDelivery.Domain.Model.Frieght;

using Dotnet.Commons.Logging;

using Spring.Stereotype;
using Spring.Transaction.Interceptor;

namespace DomainDrivenDelivery.Application.Event
{
    [Service]
    public class ReadyToClaimNotifier
    {
        private CargoRepository cargoRepository;
        private static readonly ILog LOG = LogFactory.GetLogger(typeof(ReadyToClaimNotifier));

        public ReadyToClaimNotifier(CargoRepository cargoRepository)
        {
            this.cargoRepository = cargoRepository;
        }

        [Transaction]
        public void alertIfReadyToClaim(TrackingId trackingId)
        {
            var cargo = cargoRepository.find(trackingId);

            if(cargo.isReadyToClaim())
            {
                // At this point, a real system would probably send an email or SMS
                // or something, but we simply log a message.
                LOG.Info("Cargo " + cargo + " is ready to be claimed");
            }
        }

        ReadyToClaimNotifier()
        {
        }
    }
}