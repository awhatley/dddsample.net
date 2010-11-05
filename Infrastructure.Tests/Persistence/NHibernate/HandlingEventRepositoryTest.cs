using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Handling;
using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Model.Shared;

using NUnit.Framework;

namespace DomainDrivenDelivery.Infrastructure.Tests.Persistence.NHibernate
{
    [TestFixture]
    public class HandlingEventRepositoryTest : AbstractRepositoryTest
    {
        public CargoRepository CargoRepository { get; set; }

        [Test]
        public void testFindEventsForCargo()
        {
            Cargo cargo = CargoRepository.find(new TrackingId("XYZ"));
            IEnumerable<HandlingEvent> handlingEvents =
                HandlingEventRepository.lookupHandlingHistoryOfCargo(cargo).distinctEventsByCompletionTime();
            Assert.AreEqual(12, handlingEvents.Count());
        }

        [Test]
        public void testMostRecentHandling()
        {
            Cargo cargo = CargoRepository.find(new TrackingId("XYZ"));
            HandlingEvent handlingEvent = HandlingEventRepository.mostRecentHandling(cargo);
            Assert.AreEqual(cargo, handlingEvent.Cargo);
            Assert.AreEqual(DateTime.Parse("2007-09-27 04:00"), handlingEvent.CompletionTime);

            Assert.AreEqual(HandlingActivity.ClaimIn(SampleLocations.MELBOURNE), handlingEvent.Activity);
            Assert.AreEqual(handlingEvent.Activity, HandlingActivity.ClaimIn(SampleLocations.MELBOURNE));
        }

        [Test]
        public void testSave()
        {
            var unLocode = new UnLocode("SESTO");

            var trackingId = new TrackingId("XYZ");
            var completionTime = DateTime.Parse("2008-01-01");
            HandlingEvent @event = HandlingEventFactory.createHandlingEvent(completionTime,
                trackingId,
                null,
                unLocode,
                HandlingActivityType.CLAIM,
                null);

            HandlingEventRepository.store(@event);

            flush();

            var result = GenericTemplate.QueryForObjectDelegate(CommandType.Text,
                String.Format("select * from HandlingEvent where sequence_number = {0}", @event.SequenceNumber),
                (r, i) => new {CARGO_ID = r["CARGO_ID"], COMPLETIONTIME = r["COMPLETIONTIME"]});

            Assert.AreEqual(1L, result.CARGO_ID);
            Assert.AreEqual(completionTime, result.COMPLETIONTIME);
            // TODO: the rest of the columns
        }
    }
}