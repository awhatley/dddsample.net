using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Model.Travel;
using DomainDrivenDelivery.Infrastructure.Persistence.InMemory;
using DomainDrivenDelivery.Infrastructure.Routing;
using DomainDrivenDelivery.Pathfinder.Api;

using L = DomainDrivenDelivery.Domain.Model.Locations.SampleLocations;

using NUnit.Framework;

using Rhino.Mocks;

namespace DomainDrivenDelivery.Infrastructure.Tests.Routing
{
    [TestFixture]
    public class ExternalRoutingServiceTest
    {
        private ExternalRoutingService externalRoutingService;
        private VoyageRepository voyageRepository;

        [SetUp]
        protected void setUp()
        {
            LocationRepository locationRepository = new LocationRepositoryInMem();
            voyageRepository = MockRepository.GenerateMock<VoyageRepository>();

            GraphTraversalService graphTraversalService = MockRepository.GenerateMock<GraphTraversalService>();
            graphTraversalService.Expect(s => s.findShortestPath(
                Arg<string>.Is.TypeOf, Arg<string>.Is.TypeOf, Arg<Hashtable>.Is.TypeOf)).Return(new List<TransitPath>());

            graphTraversalService.Replay();

            // TODO expectations on GTS
            externalRoutingService = new ExternalRoutingService(graphTraversalService,
                locationRepository,
                voyageRepository);

            /*new GraphTraversalServiceImpl(new GraphDAO() {
      public List<String> listLocations() {
        return Arrays.asList(TOKYO.unLocode().stringValue(), STOCKHOLM.unLocode().stringValue(), GOTHENBURG.unLocode().stringValue());
      }

      public void storeCarrierMovementId(String cmId, String from, String to) {
      }
    });*/
        }

        // TODO this test belongs in com.pathfinder

        [Test]
        public void testCalculatePossibleRoutes()
        {
            TrackingId trackingId = new TrackingId("ABC");
            RouteSpecification routeSpecification = new RouteSpecification(L.HONGKONG,
                L.HELSINKI,
                DateTime.Parse("2009-04-01"));
            Cargo cargo = new Cargo(trackingId, routeSpecification);

            var candidates = externalRoutingService.fetchRoutesForSpecification(routeSpecification);
            Assert.NotNull(candidates);

            foreach(Itinerary itinerary in candidates)
            {
                var legs = itinerary.Legs;
                Assert.NotNull(legs);
                Assert.True(legs.Any());

                // Cargo origin and start of first leg should match
                Assert.AreEqual(cargo.RouteSpecification.Origin, legs.ElementAt(0).LoadLocation);

                // Cargo final destination and last leg stop should match
                Location lastLegStop = legs.Last().UnloadLocation;
                Assert.AreEqual(cargo.RouteSpecification.Destination, lastLegStop);

                for(int i = 0; i < legs.Count() - 1; i++)
                {
                    // Assert that all legs are connected
                    Assert.AreEqual(legs.ElementAt(i).UnloadLocation, legs.ElementAt(i + 1).LoadLocation);
                }
            }
        }
    }
}