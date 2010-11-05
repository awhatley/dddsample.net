using System.Collections.Generic;
using System.Linq;

using DomainDrivenDelivery.Booking.Api;
using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Model.Travel;

namespace DomainDrivenDelivery.Interfaces.Booking.Facade
{
    internal static class DTOAssembler
    {
        internal static CargoRoutingDTO toDTO(Cargo cargo)
        {
            var itinerary = cargo.Itinerary;

            var legDTOList = new List<LegDTO>();
            if(itinerary != null)
            {
                var legs = itinerary.Legs;

                legDTOList = new List<LegDTO>(legs.Count());
                foreach(Leg leg in legs)
                {
                    var legDTO = new LegDTO(
                      leg.Voyage.VoyageNumber.Value,
                      leg.LoadLocation.UnLocode.Value,
                      leg.UnloadLocation.UnLocode.Value,
                      leg.LoadTime,
                      leg.UnloadTime);
                    legDTOList.Add(legDTO);
                }
            }

            return new CargoRoutingDTO(
              cargo.TrackingId.Value,
              cargo.RouteSpecification.Origin.UnLocode.Value,
              cargo.RouteSpecification.Destination.UnLocode.Value,
              cargo.RouteSpecification.ArrivalDeadline,
              cargo.RoutingStatus == RoutingStatus.MISROUTED,
              legDTOList
            );
        }

        internal static RouteCandidateDTO toDTO(Itinerary itinerary)
        {
            var legDTOs = new List<LegDTO>(itinerary.Legs.Count());
            foreach(Leg leg in itinerary.Legs)
            {
                legDTOs.Add(toLegDTO(leg));
            }
            return new RouteCandidateDTO(legDTOs);
        }

        internal static LegDTO toLegDTO(Leg leg)
        {
            var voyageNumber = leg.Voyage.VoyageNumber;
            var from = leg.LoadLocation.UnLocode;
            var to = leg.UnloadLocation.UnLocode;
            return new LegDTO(voyageNumber.Value, from.Value, to.Value, leg.LoadTime, leg.UnloadTime);
        }

        internal static Itinerary fromDTO(RouteCandidateDTO routeCandidateDTO,
                                 VoyageRepository voyageRepository,
                                 LocationRepository locationRepository)
        {
            var legs = new List<Leg>(routeCandidateDTO.getLegs().Count());
            foreach(LegDTO legDTO in routeCandidateDTO.getLegs())
            {
                var voyageNumber = new VoyageNumber(legDTO.getVoyageNumber());
                var voyage = voyageRepository.find(voyageNumber);
                var from = locationRepository.find(new UnLocode(legDTO.getFrom()));
                var to = locationRepository.find(new UnLocode(legDTO.getTo()));
                legs.Add(Leg.DeriveLeg(voyage, from, to));
            }
            return new Itinerary(legs);
        }

        internal static LocationDTO toDTO(Location location)
        {
            return new LocationDTO(location.UnLocode.Value, location.Name);
        }

        internal static IEnumerable<LocationDTO> toDTOList(IEnumerable<Location> allLocations)
        {
            var dtoList = new List<LocationDTO>(allLocations.Count());
            foreach(Location location in allLocations)
            {
                dtoList.Add(toDTO(location));
            }

            return dtoList;
        }
    }
}