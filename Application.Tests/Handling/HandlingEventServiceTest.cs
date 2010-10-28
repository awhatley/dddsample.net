using System;

using DomainDrivenDelivery.Application.Event;
using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Handling;
using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Model.Shared;
using DomainDrivenDelivery.Domain.Model.Travel;

using NUnit.Framework;

using Rhino.Mocks;

using L = DomainDrivenDelivery.Domain.Model.Locations.SampleLocations;

namespace DomainDrivenDelivery.Application.Handling
{
    [TestFixture]
    public class HandlingEventServiceTest
    {
        private HandlingEventServiceImpl service;
        private SystemEvents systemEvents;
        private CargoRepository cargoRepository;
        private VoyageRepository voyageRepository;
        private HandlingEventRepository handlingEventRepository;
        private LocationRepository locationRepository;

        private readonly Cargo cargo = new Cargo(new TrackingId("ABC"),
            new RouteSpecification(L.HAMBURG, L.TOKYO, DateTime.Now));

        [SetUp]
        protected void setUp()
        {
            cargoRepository = MockRepository.GenerateMock<CargoRepository>();
            voyageRepository = MockRepository.GenerateMock<VoyageRepository>();
            handlingEventRepository = MockRepository.GenerateMock<HandlingEventRepository>();
            locationRepository = MockRepository.GenerateMock<LocationRepository>();
            systemEvents = MockRepository.GenerateMock<SystemEvents>();

            HandlingEventFactory handlingEventFactory = new HandlingEventFactory(cargoRepository,
                voyageRepository,
                locationRepository);
            service = new HandlingEventServiceImpl(handlingEventRepository, systemEvents, handlingEventFactory);
        }

        [TearDown]
        protected void tearDown()
        {
            cargoRepository.VerifyAllExpectations();
            voyageRepository.VerifyAllExpectations();
            handlingEventRepository.VerifyAllExpectations();
            systemEvents.VerifyAllExpectations();
        }

        [Test]
        public void testRegisterEvent()
        {
            cargoRepository.Expect(cr => cr.find(cargo.trackingId())).Return(cargo);
            voyageRepository.Expect(vr => vr.find(SampleVoyages.pacific1.voyageNumber())).Return(SampleVoyages.pacific1);
            locationRepository.Expect(lr => lr.find(L.STOCKHOLM.unLocode())).Return(L.STOCKHOLM);

            handlingEventRepository.Expect(her => her.store(Arg<HandlingEvent>.Is.TypeOf));
            systemEvents.Expect(se => se.notifyOfHandlingEvent(Arg<HandlingEvent>.Is.TypeOf));

            service.registerHandlingEvent(DateTime.Now,
                cargo.trackingId(),
                SampleVoyages.pacific1.voyageNumber(),
                L.STOCKHOLM.unLocode(),
                HandlingActivityType.LOAD,
                new OperatorCode("ABCDE"));
        }
    }
}