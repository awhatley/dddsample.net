using System;
using System.Collections.Generic;

namespace DomainDrivenDelivery.Booking.Api
{
    /// <summary>
    /// This facade shields the domain layer - model, services, repositories -
    /// from concerns about such things as the user interface and remoting.
    /// </summary>
    public interface BookingServiceFacade
    {
        string bookNewCargo(string origin, string destination, DateTime arrivalDeadline);
        CargoRoutingDTO loadCargoForRouting(string trackingId);
        void assignCargoToRoute(string trackingId, RouteCandidateDTO route);
        void changeDestination(string trackingId, string destinationUnLocode);
        IEnumerable<RouteCandidateDTO> requestPossibleRoutesForCargo(string trackingId);
        IEnumerable<LocationDTO> listShippingLocations();
        IEnumerable<CargoRoutingDTO> listAllCargos();
        IEnumerable<VoyageDTO> listAllVoyages();
        void departureDelayed(VoyageDelayDTO delay);
        void arrivalDelayed(VoyageDelayDTO delay);
    }
}