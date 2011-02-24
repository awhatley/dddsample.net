using System;

using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Handling;
using DomainDrivenDelivery.Domain.Model.Shared;
using DomainDrivenDelivery.Infrastructure.Persistence.InMemory;
using DomainDrivenDelivery.Infrastructure.Reporting;
using DomainDrivenDelivery.Reporting.Api;
using L = DomainDrivenDelivery.Domain.Model.Locations.SampleLocations;

using NUnit.Framework;

using Rhino.Mocks;

namespace DomainDrivenDelivery.Infrastructure.Tests.Reporting
{
    [TestFixture]
    public class ReportsUpdaterTest
    {
        private ReportPusher reportPusher;
        private ReportSubmission reportSubmission;
        private EventSequenceNumber eventSequenceNumber;

        [SetUp]
        public void setUp()
        {
            reportSubmission = MockRepository.GenerateMock<ReportSubmission>();
            CargoRepository cargoRepository = new CargoRepositoryInMem();
            HandlingEventRepository handlingEventRepository = new HandlingEventRepositoryInMem();
            HandlingEventFactory handlingEventFactory = new HandlingEventFactory(cargoRepository,
                new VoyageRepositoryInMem(),
                new LocationRepositoryInMem());

            TrackingId trackingId = new TrackingId("ABC");
            RouteSpecification routeSpecification = new RouteSpecification(L.HONGKONG, L.ROTTERDAM, DateTime.Parse("2009-10-10"));
            Cargo cargo = new Cargo(trackingId, routeSpecification);
            cargoRepository.store(cargo);

            HandlingEvent handlingEvent = handlingEventFactory.createHandlingEvent(
                DateTime.Parse("2009-10-02"),
                trackingId,
                null,
                L.HONGKONG.UnLocode,
                HandlingActivityType.RECEIVE,
                new OperatorCode("ABCDE")
                );
            handlingEventRepository.store(handlingEvent);

            cargo.Handled(handlingEvent.Activity);

            reportPusher = new ReportPusher(reportSubmission, cargoRepository, handlingEventRepository);
            eventSequenceNumber = handlingEvent.SequenceNumber;
        }

        [Test]
        public void reportCargoUpdate()
        {
            reportPusher.reportCargoUpdate(new TrackingId("ABC"));

            CargoDetails expected = new CargoDetails();
            expected.setTrackingId("ABC");
            expected.setCurrentLocation("Hongkong");
            expected.setFinalDestination("Rotterdam");
            expected.setCurrentStatus("IN_PORT");

            reportSubmission.AssertWasCalled(s => s.submitCargoDetails(Arg.Is(expected)));
        }

        [Test]
        public void reportHandling()
        {
            reportPusher.reportHandlingEvent(eventSequenceNumber);

            Handling expected = new Handling();
            expected.setLocation("Hongkong");
            expected.setType("RECEIVE");
            expected.setVoyage("");

            reportSubmission.AssertWasCalled(s => s.submitHandling(Arg.Is("ABC"), Arg.Is(expected)));
        }
    }
}