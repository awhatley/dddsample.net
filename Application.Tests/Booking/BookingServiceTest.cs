using System;
using System.Linq;

using DomainDrivenDelivery.Application.Booking;
using DomainDrivenDelivery.Domain.Model.Frieght;
using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Services;
using L = DomainDrivenDelivery.Domain.Model.Locations.SampleLocations;

using NUnit.Framework;

using Rhino.Mocks;

namespace Application.Tests.Booking
{
    [TestFixture]
    public class BookingServiceTest
    {
        BookingServiceImpl bookingService;
        CargoRepository cargoRepository;
        LocationRepository locationRepository;
        RoutingService routingService;
        TrackingIdFactory trackingIdFactory;

        [SetUp]
        public void setUp()
        {
            cargoRepository = MockRepository.GenerateMock<CargoRepository>();
            locationRepository = MockRepository.GenerateMock<LocationRepository>();
            routingService = MockRepository.GenerateMock<RoutingService>();
            trackingIdFactory = MockRepository.GenerateMock<TrackingIdFactory>();
            bookingService = new BookingServiceImpl(routingService, trackingIdFactory, cargoRepository, locationRepository);
        }

        [Test]
        public void testRegisterNew()
        {
            TrackingId expectedTrackingId = new TrackingId("TRK1");
            UnLocode fromUnlocode = new UnLocode("USCHI");
            UnLocode toUnlocode = new UnLocode("SESTO");

            trackingIdFactory.Expect(t => t.nextTrackingId()).Return(expectedTrackingId);
            locationRepository.Expect(l => l.find(fromUnlocode)).Return(L.CHICAGO);
            locationRepository.Expect(l => l.find(toUnlocode)).Return(L.STOCKHOLM);
            cargoRepository.Expect(c => c.store(Arg<Cargo>.Is.TypeOf));

            TrackingId trackingId = bookingService.bookNewCargo(fromUnlocode, toUnlocode, DateTime.Now);
            Assert.AreEqual(expectedTrackingId, trackingId);
        }

        [TearDown]
        public void tearDown()
        {
            cargoRepository.VerifyAllExpectations();
            locationRepository.VerifyAllExpectations();
            trackingIdFactory.VerifyAllExpectations();
        }
    }
}