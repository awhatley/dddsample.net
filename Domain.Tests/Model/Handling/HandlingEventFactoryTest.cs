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

namespace DomainDrivenDelivery.Domain.Tests.Model.Handling
{
    [TestFixture]
    public class HandlingEventFactoryTest
    {
        private HandlingEventFactory factory;
        private CargoRepository cargoRepository;
        private VoyageRepository voyageRepository;
        private LocationRepository locationRepository;
        private TrackingId trackingId;
        private Cargo cargo;

        [SetUp]
        protected void setUp()
        {
            cargoRepository = MockRepository.GenerateMock<CargoRepository>();
            voyageRepository = new VoyageRepositoryInMem();
            locationRepository = new LocationRepositoryInMem();
            factory = new HandlingEventFactory(cargoRepository, voyageRepository, locationRepository);

            trackingId = new TrackingId("ABC");
            RouteSpecification routeSpecification = new RouteSpecification(L.TOKYO, L.HELSINKI, DateTime.Now);
            cargo = new Cargo(trackingId, routeSpecification);
        }

        [Test]
        public void testCreateHandlingEventWithCarrierMovement()
        {
            cargoRepository.Expect(c => c.find(trackingId)).Return(cargo);

            VoyageNumber voyageNumber = SampleVoyages.pacific1.VoyageNumber;
            UnLocode unLocode = L.STOCKHOLM.UnLocode;
            HandlingEvent handlingEvent = factory.createHandlingEvent(new DateTime(100),
                trackingId,
                voyageNumber,
                unLocode,
                HandlingActivityType.LOAD,
                new OperatorCode("ABCDE"));

            Assert.IsNotNull(handlingEvent);
            Assert.AreEqual(L.STOCKHOLM, handlingEvent.Location);
            Assert.AreEqual(SampleVoyages.pacific1, handlingEvent.Voyage);
            Assert.AreEqual(cargo, handlingEvent.Cargo);
            Assert.AreEqual(new DateTime(100), handlingEvent.CompletionTime);
            Assert.True(handlingEvent.RegistrationTime < DateTime.Now.AddMilliseconds(1));
        }

        [Test]
        public void testCreateHandlingEventWithoutCarrierMovement()
        {
            cargoRepository.Expect(c => c.find(trackingId)).Return(cargo);

            UnLocode unLocode = L.STOCKHOLM.UnLocode;
            HandlingEvent handlingEvent = factory.createHandlingEvent(new DateTime(100),
                trackingId,
                null,
                unLocode,
                HandlingActivityType.CLAIM,
                new OperatorCode("ABCDE"));

            Assert.IsNotNull(handlingEvent);
            Assert.AreEqual(L.STOCKHOLM, handlingEvent.Location);
            Assert.AreEqual(Voyage.NONE, handlingEvent.Voyage);
            Assert.AreEqual(cargo, handlingEvent.Cargo);
            Assert.AreEqual(new DateTime(100), handlingEvent.CompletionTime);
            Assert.True(handlingEvent.RegistrationTime < DateTime.Now.AddMilliseconds(1));
        }

        [Test]
        public void testCreateHandlingEventUnknownLocation()
        {
            cargoRepository.Expect(c => c.find(trackingId)).Return(cargo);

            UnLocode invalid = new UnLocode("NOEXT");
            try
            {
                factory.createHandlingEvent(new DateTime(100),
                    trackingId,
                    SampleVoyages.pacific1.VoyageNumber,
                    invalid,
                    HandlingActivityType.LOAD,
                    new OperatorCode("ABCDE"));
                Assert.Fail("Expected UnknownLocationException");
            }
            catch(UnknownLocationException)
            {
            }
        }

        [Test]
        public void testCreateHandlingEventUnknownCarrierMovement()
        {
            cargoRepository.Expect(c => c.find(trackingId)).Return(cargo);

            try
            {
                VoyageNumber invalid = new VoyageNumber("XXX");
                factory.createHandlingEvent(new DateTime(100),
                    trackingId,
                    invalid,
                    L.STOCKHOLM.UnLocode,
                    HandlingActivityType.LOAD,
                    new OperatorCode("ABCDE"));
                Assert.Fail("Expected UnknownVoyageException");
            }
            catch(UnknownVoyageException)
            {
            }
        }

        [Test]
        public void testCreateHandlingEventUnknownTrackingId()
        {
            cargoRepository.Expect(c => c.find(trackingId)).Return(null);

            try
            {
                factory.createHandlingEvent(new DateTime(100),
                    trackingId,
                    SampleVoyages.pacific1.VoyageNumber,
                    L.STOCKHOLM.UnLocode,
                    HandlingActivityType.LOAD,
                    new OperatorCode("ABCDE"));
                Assert.Fail("Expected UnknownCargoException");
            }
            catch(UnknownCargoException)
            {
            }
        }

        [TearDown]
        protected void tearDown()
        {
            cargoRepository.VerifyAllExpectations();
        }
    }
}