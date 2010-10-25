using DomainDrivenDelivery.Domain.Model.Frieght;
using DomainDrivenDelivery.Domain.Model.Handling;
using DomainDrivenDelivery.Reporting.Api;

using Spring.Transaction.Interceptor;

namespace DomainDrivenDelivery.Infrastructure.Reporting
{
    public class ReportPusher
    {
        private CargoRepository cargoRepository;
        private HandlingEventRepository handlingEventRepository;
        private ReportSubmission reportSubmission;

        public ReportPusher(ReportSubmission reportSubmission,
                            CargoRepository cargoRepository,
                            HandlingEventRepository handlingEventRepository)
        {
            this.reportSubmission = reportSubmission;
            this.cargoRepository = cargoRepository;
            this.handlingEventRepository = handlingEventRepository;
        }

        [Transaction]
        public void reportHandlingEvent(EventSequenceNumber sequenceNumber)
        {
            HandlingEvent handlingEvent = handlingEventRepository.find(sequenceNumber);
            Handling handling = assembleFrom(handlingEvent);
            string trackingIdString = handlingEvent.cargo().trackingId().stringValue();

            reportSubmission.submitHandling(trackingIdString, handling);
        }

        [Transaction]
        public void reportCargoUpdate(TrackingId trackingId)
        {
            Cargo cargo = cargoRepository.find(trackingId);
            CargoDetails cargoDetails = assembleFrom(cargo);

            reportSubmission.submitCargoDetails(cargoDetails);
        }

        private Handling assembleFrom(HandlingEvent handlingEvent)
        {
            Handling handling = new Handling();
            handling.setLocation(handlingEvent.location().name());
            handling.setType(handlingEvent.activity().type().ToString());
            handling.setVoyage(handlingEvent.voyage().voyageNumber().stringValue());
            return handling;
        }

        private CargoDetails assembleFrom(Cargo cargo)
        {
            CargoDetails cargoDetails = new CargoDetails();
            cargoDetails.setTrackingId(cargo.trackingId().stringValue());
            cargoDetails.setFinalDestination(cargo.routeSpecification().destination().name());
            cargoDetails.setCurrentLocation(cargo.lastKnownLocation().name());
            cargoDetails.setCurrentStatus(cargo.transportStatus().ToString());
            return cargoDetails;
        }

        ReportPusher()
        {
            // Needed by CGLIB
        }
    }
}