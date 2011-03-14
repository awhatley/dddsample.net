using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

using DomainDrivenDelivery.Application.Booking;
using DomainDrivenDelivery.Booking.Api;
using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Model.Travel;

using Spring.Stereotype;
using Spring.Transaction.Interceptor;

namespace DomainDrivenDelivery.Interfaces.Booking.Facade
{
    /// <summary>
    /// This implementation has additional support from the infrastructure, for exposing as an RMI
    /// service and for keeping the OR-mapper unit-of-work open during DTO assembly,
    /// analogous to the view rendering for web interfaces.
    /// </summary>
    [Service]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class BookingServiceFacadeImpl : BookingServiceFacade
    {
        private readonly BookingService bookingService;
        private readonly LocationRepository locationRepository;
        private readonly CargoRepository cargoRepository;
        private readonly VoyageRepository voyageRepository;

        public BookingServiceFacadeImpl(BookingService bookingService, LocationRepository locationRepository,
                                        CargoRepository cargoRepository, VoyageRepository voyageRepository)
        {
            this.bookingService = bookingService;
            this.locationRepository = locationRepository;
            this.cargoRepository = cargoRepository;
            this.voyageRepository = voyageRepository;
        }

        public IEnumerable<LocationDTO> listShippingLocations()
        {
            var allLocations = locationRepository.findAll();
            return DTOAssembler.toDTOList(allLocations);
        }

        public string bookNewCargo(string origin, string destination, DateTime arrivalDeadline)
        {
            TrackingId trackingId = bookingService.bookNewCargo(
              new UnLocode(origin),
              new UnLocode(destination),
              arrivalDeadline
            );
            return trackingId.Value;
        }

        [Transaction]
        public CargoRoutingDTO loadCargoForRouting(string trackingId)
        {
            var cargo = bookingService.loadCargoForRouting(new TrackingId(trackingId));
            return DTOAssembler.toDTO(cargo);
        }


        public void assignCargoToRoute(string trackingIdStr, RouteCandidateDTO routeCandidateDTO)
        {
            var itinerary = DTOAssembler.fromDTO(routeCandidateDTO, voyageRepository, locationRepository);
            var trackingId = new TrackingId(trackingIdStr);

            bookingService.assignCargoToRoute(itinerary, trackingId);
        }


        public void changeDestination(string trackingId, string destinationUnLocode)
        {
            bookingService.changeDestination(new TrackingId(trackingId), new UnLocode(destinationUnLocode));
        }

        [Transaction]
        public IEnumerable<CargoRoutingDTO> listAllCargos()
        {
            var cargoList = cargoRepository.findAll();
            var dtoList = new List<CargoRoutingDTO>(cargoList.Count());
            foreach(Cargo cargo in cargoList)
            {
                dtoList.Add(DTOAssembler.toDTO(cargo));
            }
            return dtoList;
        }


        public IEnumerable<RouteCandidateDTO> requestPossibleRoutesForCargo(string trackingId)
        {
            var itineraries = bookingService.requestPossibleRoutesForCargo(new TrackingId(trackingId));

            var routeCandidates = new List<RouteCandidateDTO>(itineraries.Count());
            foreach(Itinerary itinerary in itineraries)
            {
                routeCandidates.Add(DTOAssembler.toDTO(itinerary));
            }

            return routeCandidates;
        }

        public IEnumerable<VoyageDTO> listAllVoyages()
        {
            // TODO
            var voyages = new List<VoyageDTO>();

            voyages.Add(new VoyageDTO("V0100", new[] {
              new CarrierMovementDTO(new LocationDTO("CNHKG", "Hongkong"), new LocationDTO("USLBG", "Long Beach")),
              new CarrierMovementDTO(new LocationDTO("USLBG", "Long Beach"), new LocationDTO("USDAL", "Dallas")),
              new CarrierMovementDTO(new LocationDTO("USDAL", "Dallas"), new LocationDTO("CAOTT", "Ottawa")) }
            ));
            voyages.Add(new VoyageDTO("V0200", new[] {
              new CarrierMovementDTO(new LocationDTO("USLBG", "Long Beach"), new LocationDTO("USDAL", "Dallas")),
              new CarrierMovementDTO(new LocationDTO("CNHKG", "Hongkong"), new LocationDTO("USLBG", "Long Beach")),
              new CarrierMovementDTO(new LocationDTO("USDAL", "Dallas"), new LocationDTO("CAOTT", "Ottawa")) }
            ));

            return voyages;
        }

        public void departureDelayed(VoyageDelayDTO delay)
        {
            // TODO
        }

        public void arrivalDelayed(VoyageDelayDTO delay)
        {
            // TODO
        }
    }
}