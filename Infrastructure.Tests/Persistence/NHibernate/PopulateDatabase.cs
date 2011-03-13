using System;

using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Handling;
using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Model.Shared;
using DomainDrivenDelivery.Domain.Model.Travel;

using NUnit.Framework;

using Spring.Transaction.Support;

namespace DomainDrivenDelivery.Infrastructure.Tests.Persistence.NHibernate
{
    [TestFixture]
    [Ignore]
    public class PopulateDatabase : AbstractRepositoryTest
    {
        protected override void OnSetUp()
        {
            DefaultRollback = false;
        }
        
        protected override void OnSetUpInTransaction()
        {
        }

        [Test]
        public void LoadHibernateData()
        {
            var tt = new TransactionTemplate(transactionManager);
            tt.Execute(ts => {
                var session = SessionFactory.GetCurrentSession();

                foreach(var location in SampleLocations.getAll())
                {
                    session.Save(location);
                }

                session.Save(SampleVoyages.HONGKONG_TO_NEW_YORK);
                session.Save(SampleVoyages.NEW_YORK_TO_DALLAS);
                session.Save(SampleVoyages.DALLAS_TO_HELSINKI);
                session.Save(SampleVoyages.HELSINKI_TO_HONGKONG);
                session.Save(SampleVoyages.DALLAS_TO_HELSINKI_ALT);

                var routeSpecification = new RouteSpecification(
                    SampleLocations.HONGKONG,
                    SampleLocations.HELSINKI,
                    DateTime.Parse("2009-03-15"));
                var trackingId = new TrackingId("ABC123");
                var abc123 = new Cargo(trackingId, routeSpecification);

                var itinerary = new Itinerary(new[] {
                    Leg.DeriveLeg(SampleVoyages.HONGKONG_TO_NEW_YORK, SampleLocations.HONGKONG, SampleLocations.NEWYORK),
                    Leg.DeriveLeg(SampleVoyages.NEW_YORK_TO_DALLAS, SampleLocations.NEWYORK, SampleLocations.DALLAS),
                    Leg.DeriveLeg(SampleVoyages.DALLAS_TO_HELSINKI, SampleLocations.DALLAS, SampleLocations.HELSINKI),
                });

                abc123.AssignToRoute(itinerary);

                session.Save(abc123);

                var event01 = HandlingEventFactory.createHandlingEvent(
                    DateTime.Parse("2009-03-01"),
                    trackingId,
                    null,
                    SampleLocations.HONGKONG.UnLocode,
                    HandlingActivityType.RECEIVE,
                    new OperatorCode("ABCDE")
                    );
                session.Save(event01);

                var event02 = HandlingEventFactory.createHandlingEvent(
                    DateTime.Parse("2009-03-02"),
                    trackingId,
                    SampleVoyages.HONGKONG_TO_NEW_YORK.VoyageNumber,
                    SampleLocations.HONGKONG.UnLocode,
                    HandlingActivityType.LOAD,
                    new OperatorCode("ABCDE")
                    );
                session.Save(event02);

                var event03 = HandlingEventFactory.createHandlingEvent(
                    DateTime.Parse("2009-03-05"),
                    trackingId,
                    SampleVoyages.HONGKONG_TO_NEW_YORK.VoyageNumber,
                    SampleLocations.NEWYORK.UnLocode,
                    HandlingActivityType.UNLOAD,
                    new OperatorCode("ABCDE")
                    );
                session.Save(event03);

                var handlingEvent = HandlingEventRepository.mostRecentHandling(abc123);
                abc123.Handled(handlingEvent.Activity);
                session.Update(abc123);

                // Cargo JKL567

                var routeSpecification1 = new RouteSpecification(
                    SampleLocations.HANGZOU,
                    SampleLocations.STOCKHOLM,
                    DateTime.Parse("2009-03-18"));
                var trackingId1 = new TrackingId("JKL567");
                var jkl567 = new Cargo(trackingId1, routeSpecification1);

                var itinerary1 = new Itinerary(new[] {
                    Leg.DeriveLeg(SampleVoyages.HONGKONG_TO_NEW_YORK, SampleLocations.HANGZOU, SampleLocations.NEWYORK),
                    Leg.DeriveLeg(SampleVoyages.NEW_YORK_TO_DALLAS, SampleLocations.NEWYORK, SampleLocations.DALLAS),
                    Leg.DeriveLeg(SampleVoyages.DALLAS_TO_HELSINKI, SampleLocations.DALLAS, SampleLocations.STOCKHOLM),
                });
                jkl567.AssignToRoute(itinerary1);

                session.Save(jkl567);

                var event1 = HandlingEventFactory.createHandlingEvent(
                    DateTime.Parse("2009-03-01"),
                    trackingId1,
                    null,
                    SampleLocations.HANGZOU.UnLocode,
                    HandlingActivityType.RECEIVE,
                    new OperatorCode("ABCDE")
                    );
                session.Save(event1);

                var event2 = HandlingEventFactory.createHandlingEvent(
                    DateTime.Parse("2009-03-03"),
                    trackingId1,
                    SampleVoyages.HONGKONG_TO_NEW_YORK.VoyageNumber,
                    SampleLocations.HANGZOU.UnLocode,
                    HandlingActivityType.LOAD,
                    new OperatorCode("ABCDE")
                    );
                session.Save(event2);

                var event3 = HandlingEventFactory.createHandlingEvent(
                    DateTime.Parse("2009-03-05"),
                    trackingId1,
                    SampleVoyages.HONGKONG_TO_NEW_YORK.VoyageNumber,
                    SampleLocations.NEWYORK.UnLocode,
                    HandlingActivityType.UNLOAD,
                    new OperatorCode("ABCDE")
                    );
                session.Save(event3);

                var event4 = HandlingEventFactory.createHandlingEvent(
                    DateTime.Parse("2009-03-06"),
                    trackingId1,
                    SampleVoyages.HONGKONG_TO_NEW_YORK.VoyageNumber,
                    SampleLocations.NEWYORK.UnLocode,
                    HandlingActivityType.LOAD,
                    new OperatorCode("ABCDE")
                    );
                session.Save(event4);

                var handlingEvent1 = HandlingEventRepository.mostRecentHandling(jkl567);
                jkl567.Handled(handlingEvent1.Activity);
                session.Update(jkl567);

                return null;
            });
        }
    }
}