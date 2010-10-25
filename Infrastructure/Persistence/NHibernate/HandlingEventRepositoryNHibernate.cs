using System.Linq;

using DomainDrivenDelivery.Domain.Model.Frieght;
using DomainDrivenDelivery.Domain.Model.Handling;

using NHibernate;

using Spring.Stereotype;

namespace DomainDrivenDelivery.Infrastructure.Persistence.NHibernate
{
    /// <summary>
    /// NHibernate implementation of HandlingEventRepository.
    /// </summary>
    [Repository]
    public class HandlingEventRepositoryNHibernate : HandlingEventRepository
    {
        private readonly ISessionFactory sessionFactory;

        public HandlingEventRepositoryNHibernate(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public HandlingEvent find(EventSequenceNumber eventSequenceNumber)
        {
            return sessionFactory.GetCurrentSession().
              CreateQuery("from HandlingEvent where sequenceNumber = :sn").
              SetParameter("sn", eventSequenceNumber).
              UniqueResult<HandlingEvent>();
        }

        public void store(HandlingEvent @event)
        {
            sessionFactory.GetCurrentSession().Save(@event);
        }

        public HandlingHistory lookupHandlingHistoryOfCargo(Cargo cargo)
        {
            var handlingEvents = sessionFactory.GetCurrentSession().
              CreateQuery("from HandlingEvent where cargo.trackingId = :tid").
              SetParameter("tid", cargo.trackingId()).
              List<HandlingEvent>();

            if(!handlingEvents.Any())
            {
                return HandlingHistory.emptyForCargo(cargo);
            }
            else
            {
                //noinspection unchecked
                return HandlingHistory.fromEvents(handlingEvents);
            }
        }

        public HandlingEvent mostRecentHandling(Cargo cargo)
        {
            return sessionFactory.GetCurrentSession().
                CreateQuery("from HandlingEvent where cargo = :cargo order by completionTime desc").
                SetParameter("cargo", cargo).
                SetMaxResults(1).
                UniqueResult<HandlingEvent>();
        }
    }
}