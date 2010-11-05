using DomainDrivenDelivery.Domain.Model.Freight;
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
            string trackingIdString = handlingEvent.Cargo.TrackingId.Value;

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
            handling.setLocation(handlingEvent.Location.Name);
            handling.setType(handlingEvent.Activity.Type.ToString());
            handling.setVoyage(handlingEvent.Voyage.VoyageNumber.Value);
            return handling;
        }

        private CargoDetails assembleFrom(Cargo cargo)
        {
            CargoDetails cargoDetails = new CargoDetails();
            cargoDetails.setTrackingId(cargo.TrackingId.Value);
            cargoDetails.setFinalDestination(cargo.RouteSpecification.Destination.Name);
            cargoDetails.setCurrentLocation(cargo.LastKnownLocation.Name);
            cargoDetails.setCurrentStatus(cargo.TransportStatus.ToString());
            return cargoDetails;
        }

        ReportPusher()
        {
            // Needed by CGLIB
        }
    }
}