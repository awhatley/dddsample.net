using System;

using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Handling;
using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Model.Shared;
using DomainDrivenDelivery.Domain.Model.Travel;
using DomainDrivenDelivery.Infrastructure.Persistence.InMemory;

using NUnit.Framework;

using Rhino.Mocks;

using L = DomainDrivenDelivery.Domain.Model.Locations.SampleLocations;
using V = DomainDrivenDelivery.Domain.Model.Travel.SampleVoyages;

namespace DomainDrivenDelivery.Application.Event
{
    [TestFixture]
    public class CargoUpdaterTest
    {
        private SystemEvents systemEvents;
        private CargoUpdater cargoUpdater;
        private HandlingEventFactory handlingEventFactory;
        private CargoRepository cargoRepository;
        private HandlingEventRepository handlingEventRepository;
        private LocationRepository locationRepository;
        private VoyageRepository voyageRepository;
        private TrackingIdFactoryInMem trackingIdFactory;

        [SetUp]
        public void setUp()
        {
            systemEvents = MockRepository.GenerateMock<SystemEvents>();
            cargoRepository = new CargoRepositoryInMem();
            handlingEventRepository = new HandlingEventRepositoryInMem();
            locationRepository = new LocationRepositoryInMem();
            voyageRepository = new VoyageRepositoryInMem();
            trackingIdFactory = new TrackingIdFactoryInMem();
            handlingEventFactory = new HandlingEventFactory(cargoRepository, voyageRepository, locationRepository);
            cargoUpdater = new CargoUpdater(systemEvents, cargoRepository, handlingEventRepository);
        }

        [Test]
        public void updateCargo()
        {
            TrackingId trackingId = trackingIdFactory.nextTrackingId();
            RouteSpecification routeSpecification = new RouteSpecification(L.HONGKONG,
                L.GOTHENBURG,
                DateTime.Parse("2009-10-15"));

            Cargo cargo = new Cargo(trackingId, routeSpecification);
            cargoRepository.store(cargo);

            HandlingEvent handlingEvent = handlingEventFactory.createHandlingEvent(DateTime.Parse("2009-10-01 14:30"),
                cargo.TrackingId,
                V.HONGKONG_TO_NEW_YORK.VoyageNumber,
                L.HONGKONG.UnLocode,
                HandlingActivityType.LOAD,
                new OperatorCode("ABCDE"));

            handlingEventRepository.store(handlingEvent);

            Assert.That(handlingEvent.Activity, Is.Not.EqualTo(cargo.MostRecentHandlingActivity));

            cargoUpdater.updateCargo(handlingEvent.SequenceNumber);

            Assert.That(handlingEvent.Activity, Is.EqualTo(cargo.MostRecentHandlingActivity));
            Assert.True(handlingEvent.Activity != cargo.MostRecentHandlingActivity);

            systemEvents.AssertWasCalled(se => se.notifyOfCargoUpdate(cargo));
        }

        [Test]
        public void handlingEventNotFound()
        {
            cargoUpdater.updateCargo(EventSequenceNumber.ValueOf(999L));
            systemEvents.AssertWasNotCalled(se => se.notifyOfCargoUpdate(Arg<Cargo>.Is.TypeOf));
            systemEvents.AssertWasNotCalled(se => se.notifyOfHandlingEvent(Arg<HandlingEvent>.Is.TypeOf));
        }
    }
}