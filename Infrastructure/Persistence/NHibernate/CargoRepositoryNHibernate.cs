using System.Collections.Generic;

using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Travel;

using NHibernate;

using Spring.Stereotype;
using Spring.Transaction.Interceptor;

namespace DomainDrivenDelivery.Infrastructure.Persistence.NHibernate
{
    /// <summary>
    /// NHibernate implementation of CargoRepository.
    /// </summary>
    [Repository]
    public class CargoRepositoryNHibernate : CargoRepository
    {
        private readonly ISessionFactory sessionFactory;

        public CargoRepositoryNHibernate(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        [Transaction(ReadOnly = true)]
        public Cargo find(TrackingId tid)
        {
            return sessionFactory.GetCurrentSession().
              CreateQuery("from Cargo where TrackingId = :tid").
              SetParameter("tid", tid).
              UniqueResult<Cargo>();
        }

        [Transaction(ReadOnly = true)]
        public IEnumerable<Cargo> findCargosOnVoyage(Voyage voyage)
        {
            return sessionFactory.GetCurrentSession().CreateQuery(
              "select cargo from Cargo as cargo " +
                "left join cargo.Itinerary.Legs as leg " +
                "where leg.Voyage = :voyage").
              SetParameter("voyage", voyage).
              List<Cargo>();
        }

        [Transaction]
        public void store(Cargo cargo)
        {
            sessionFactory.GetCurrentSession().SaveOrUpdate(cargo);
            // Delete-orphan does not seem to work correctly when the parent is a component
            sessionFactory.GetCurrentSession().CreateSQLQuery("delete from Leg where cargo_id = null").ExecuteUpdate();
        }

        [Transaction(ReadOnly = true)]
        public IEnumerable<Cargo> findAll()
        {
            return sessionFactory.GetCurrentSession().CreateQuery("from Cargo").List<Cargo>();
        }
    }
}