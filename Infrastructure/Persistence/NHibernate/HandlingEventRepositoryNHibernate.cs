using System.Linq;

using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Handling;

using NHibernate;

using Spring.Stereotype;
using Spring.Transaction.Interceptor;

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

        [Transaction(ReadOnly = true)]
        public HandlingEvent find(EventSequenceNumber eventSequenceNumber)
        {
            return sessionFactory.GetCurrentSession().
              CreateQuery("from HandlingEvent where SequenceNumber = :sn").
              SetParameter("sn", eventSequenceNumber).
              UniqueResult<HandlingEvent>();
        }

        [Transaction]
        public void store(HandlingEvent @event)
        {
            sessionFactory.GetCurrentSession().Save(@event);
        }

        [Transaction(ReadOnly = true)]
        public HandlingHistory lookupHandlingHistoryOfCargo(Cargo cargo)
        {
            var handlingEvents = sessionFactory.GetCurrentSession().
              CreateQuery("from HandlingEvent where Cargo.TrackingId = :tid").
              SetParameter("tid", cargo.TrackingId).
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

        [Transaction(ReadOnly = true)]
        public HandlingEvent mostRecentHandling(Cargo cargo)
        {
            return sessionFactory.GetCurrentSession().
                CreateQuery("from HandlingEvent where Cargo = :cargo order by completionTime desc").
                SetParameter("cargo", cargo).
                SetMaxResults(1).
                UniqueResult<HandlingEvent>();
        }
    }
}