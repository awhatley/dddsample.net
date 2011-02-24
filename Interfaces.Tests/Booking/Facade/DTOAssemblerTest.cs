using System;
using System.Collections.Generic;
using System.Linq;

using DomainDrivenDelivery.Booking.Api;
using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Travel;
using DomainDrivenDelivery.Infrastructure.Persistence.InMemory;
using DomainDrivenDelivery.Interfaces.Booking.Facade;

using L = DomainDrivenDelivery.Domain.Model.Locations.SampleLocations;

using NUnit.Framework;

namespace DomainDrivenDelivery.Interfaces.Tests.Booking.Facade
{
    [TestFixture]
    public class DTOAssemblerTest
    {
        [Test]
        public void toCargoRoutingDTO()
        {
            var origin = L.HONGKONG;
            var destination = L.LONGBEACH;
            var cargo = new Cargo(new TrackingId("XYZ"), new RouteSpecification(origin, destination, DateTime.Now));

            var itinerary = new Itinerary(
                Leg.DeriveLeg(SampleVoyages.pacific1, L.HONGKONG, L.TOKYO),
                Leg.DeriveLeg(SampleVoyages.pacific1, L.TOKYO, L.LONGBEACH)
                );

            cargo.AssignToRoute(itinerary);

            var dto = DTOAssembler.toDTO(cargo);

            Assert.AreEqual(2, dto.getLegs().Count());

            LegDTO legDTO = dto.getLegs().ElementAt(0);
            Assert.AreEqual("PAC1", legDTO.getVoyageNumber());
            Assert.AreEqual("CNHKG", legDTO.getFrom());
            Assert.AreEqual("JNTKO", legDTO.getTo());

            legDTO = dto.getLegs().ElementAt(1);
            Assert.AreEqual("PAC1", legDTO.getVoyageNumber());
            Assert.AreEqual("JNTKO", legDTO.getFrom());
            Assert.AreEqual("USLBG", legDTO.getTo());
        }

        [Test]
        public void toCargoDTONoItinerary()
        {
            var cargo = new Cargo(new TrackingId("XYZ"), new RouteSpecification(L.HONGKONG, L.LONGBEACH, DateTime.Now));
            var dto = DTOAssembler.toDTO(cargo);

            Assert.AreEqual("XYZ", dto.getTrackingId());
            Assert.AreEqual("CNHKG", dto.getOrigin());
            Assert.AreEqual("USLBG", dto.getFinalDestination());
            Assert.False(dto.getLegs().Any());
        }

        [Test]
        public void toRouteCandidateDTO()
        {
            var itinerary = new Itinerary(
                Leg.DeriveLeg(SampleVoyages.pacific1, L.HONGKONG, L.TOKYO),
                Leg.DeriveLeg(SampleVoyages.pacific1, L.TOKYO, L.LONGBEACH)
                );

            var dto = DTOAssembler.toDTO(itinerary);

            Assert.AreEqual(2, dto.getLegs().Count());
            LegDTO legDTO = dto.getLegs().ElementAt(0);
            Assert.AreEqual("PAC1", legDTO.getVoyageNumber());
            Assert.AreEqual("CNHKG", legDTO.getFrom());
            Assert.AreEqual("JNTKO", legDTO.getTo());

            legDTO = dto.getLegs().ElementAt(1);
            Assert.AreEqual("PAC1", legDTO.getVoyageNumber());
            Assert.AreEqual("JNTKO", legDTO.getFrom());
            Assert.AreEqual("USLBG", legDTO.getTo());
        }

        [Test]
        public void fromRouteCandidateDTO()
        {
            var legs = new List<LegDTO>();
            legs.Add(new LegDTO("PAC1", "CNHKG", "USLBG", DateTime.Now, DateTime.Now));
            legs.Add(new LegDTO("CNT1", "USLBG", "USNYC", DateTime.Now, DateTime.Now));

            var locationRepository = new LocationRepositoryInMem();
            var voyageRepository = new VoyageRepositoryInMem();

            // Tested call
            var itinerary = DTOAssembler.fromDTO(new RouteCandidateDTO(legs), voyageRepository, locationRepository);

            Assert.NotNull(itinerary);
            Assert.NotNull(itinerary.Legs);
            Assert.AreEqual(2, itinerary.Legs.Count());

            var leg1 = itinerary.Legs.ElementAt(0);
            Assert.NotNull(leg1);
            Assert.AreEqual(L.HONGKONG, leg1.LoadLocation);
            Assert.AreEqual(L.LONGBEACH, leg1.UnloadLocation);

            var leg2 = itinerary.Legs.ElementAt(1);
            Assert.NotNull(leg2);
            Assert.AreEqual(L.LONGBEACH, leg2.LoadLocation);
            Assert.AreEqual(L.NEWYORK, leg2.UnloadLocation);
        }

        [Test]
        public void toDTOList()
        {
            var locationList = new[] { L.STOCKHOLM, L.HAMBURG };

            var dtos = DTOAssembler.toDTOList(locationList);

            Assert.AreEqual(2, dtos.Count());

            LocationDTO dto = dtos.ElementAt(0);
            Assert.AreEqual("SESTO", dto.getUnLocode());
            Assert.AreEqual("Stockholm", dto.getName());

            dto = dtos.ElementAt(1);
            Assert.AreEqual("DEHAM", dto.getUnLocode());
            Assert.AreEqual("Hamburg", dto.getName());
        }
    }
}