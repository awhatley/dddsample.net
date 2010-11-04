using System;
using System.Collections.Generic;

using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Services;
using DomainDrivenDelivery.Utilities;

using Dotnet.Commons.Logging;

using Spring.Stereotype;
using Spring.Transaction.Interceptor;

namespace DomainDrivenDelivery.Application.Booking
{
    [Service]
    public sealed class BookingServiceImpl : BookingService
    {
        private readonly CargoRepository _cargoRepository;
        private readonly LocationRepository _locationRepository;
        private readonly ILog _logger = LogFactory.GetLogger(typeof(BookingServiceImpl));
        private readonly RoutingService _routingService;
        private readonly TrackingIdFactory _trackingIdFactory;

        public BookingServiceImpl(RoutingService routingService,
                              TrackingIdFactory trackingIdFactory,
                              CargoRepository cargoRepository,
                              LocationRepository locationRepository)
        {
            _routingService = routingService;
            _trackingIdFactory = trackingIdFactory;
            _cargoRepository = cargoRepository;
            _locationRepository = locationRepository;
        }

        [Transaction]
        public TrackingId bookNewCargo(UnLocode originUnLocode, UnLocode destinationUnLocode, DateTime arrivalDeadline)
        {
            var trackingId = _trackingIdFactory.nextTrackingId();
            var origin = _locationRepository.find(originUnLocode);
            var destination = _locationRepository.find(destinationUnLocode);
            var routeSpecification = new RouteSpecification(origin, destination, arrivalDeadline);

            var cargo = new Cargo(trackingId, routeSpecification);
            _cargoRepository.store(cargo);

            _logger.Info("Booked new cargo with tracking id " + cargo.TrackingId.stringValue());

            return cargo.TrackingId;
        }

        [Transaction(ReadOnly = true)]
        public IEnumerable<Itinerary> requestPossibleRoutesForCargo(TrackingId trackingId)
        {
            var cargo = _cargoRepository.find(trackingId);
            if(cargo == null)
                return new List<Itinerary>();

            return _routingService.fetchRoutesForSpecification(cargo.RouteSpecification);
        }

        [Transaction]
        public void assignCargoToRoute(Itinerary itinerary, TrackingId trackingId)
        {
            var cargo = _cargoRepository.find(trackingId);
            Validate.notNull(cargo, "Can't assign itinerary to non-existing cargo " + trackingId);
            cargo.assignToRoute(itinerary);
            _cargoRepository.store(cargo);

            _logger.Info("Assigned cargo " + trackingId + " to new route");
        }

        [Transaction]
        public void changeDestination(TrackingId trackingId, UnLocode unLocode)
        {
            var cargo = _cargoRepository.find(trackingId);
            Validate.notNull(cargo, "Can't change destination of non-existing cargo " + trackingId);
            var newDestination = _locationRepository.find(unLocode);

            var routeSpecification = cargo.RouteSpecification.withDestination(newDestination);
            cargo.specifyNewRoute(routeSpecification);

            _cargoRepository.store(cargo);
            _logger.Info("Changed destination for cargo " + trackingId + " to " + routeSpecification.destination());
        }

        [Transaction(ReadOnly = true)]
        public Cargo loadCargoForRouting(TrackingId trackingId)
        {
            return _cargoRepository.find(trackingId);
        }
    }
}