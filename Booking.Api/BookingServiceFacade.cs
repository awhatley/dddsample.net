using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace DomainDrivenDelivery.Booking.Api
{
    /// <summary>
    /// This facade shields the domain layer - model, services, repositories -
    /// from concerns about such things as the user interface and remoting.
    /// </summary>    
    [ServiceContract]
    public interface BookingServiceFacade
    {
        [OperationContract] string bookNewCargo(string origin, string destination, DateTime arrivalDeadline);
        [OperationContract] CargoRoutingDTO loadCargoForRouting(string trackingId);
        [OperationContract] void assignCargoToRoute(string trackingId, RouteCandidateDTO route);
        [OperationContract] void changeDestination(string trackingId, string destinationUnLocode);
        [OperationContract] IEnumerable<RouteCandidateDTO> requestPossibleRoutesForCargo(string trackingId);
        [OperationContract] IEnumerable<LocationDTO> listShippingLocations();
        [OperationContract] IEnumerable<CargoRoutingDTO> listAllCargos();
        [OperationContract] IEnumerable<VoyageDTO> listAllVoyages();
        [OperationContract] void departureDelayed(VoyageDelayDTO delay);
        [OperationContract] void arrivalDelayed(VoyageDelayDTO delay);
    }
}