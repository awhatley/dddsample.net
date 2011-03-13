using System.Collections.Generic;

using DomainDrivenDelivery.Domain.Model.Locations;

using NHibernate;

using Spring.Stereotype;
using Spring.Transaction.Interceptor;

namespace DomainDrivenDelivery.Infrastructure.Persistence.NHibernate
{
    [Repository]
    public sealed class LocationRepositoryNHibernate : LocationRepository
    {
        private readonly ISessionFactory sessionFactory;

        public LocationRepositoryNHibernate(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        [Transaction(ReadOnly = true)]
        public Location find(UnLocode unLocode)
        {
            return sessionFactory.GetCurrentSession().
              CreateQuery("from Location where UnLocode = ?").
              SetParameter(0, unLocode).
              UniqueResult<Location>();
        }

        [Transaction(ReadOnly = true)]
        public IEnumerable<Location> findAll()
        {
            return sessionFactory.GetCurrentSession().CreateQuery("from Location").List<Location>();
        }
    }
}