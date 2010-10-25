using System.Collections.Generic;

using DomainDrivenDelivery.Domain.Model.Locations;

using NHibernate;

using Spring.Stereotype;

namespace DomainDrivenDelivery.Infrastructure.Persistence.NHibernate
{
    [Repository]
    public class LocationRepositoryNHibernate : LocationRepository
    {
        private readonly ISessionFactory sessionFactory;

        public LocationRepositoryNHibernate(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public Location find(UnLocode unLocode)
        {
            return sessionFactory.GetCurrentSession().
              CreateQuery("from Location where unLocode = ?").
              SetParameter(0, unLocode).
              UniqueResult<Location>();
        }

        public IEnumerable<Location> findAll()
        {
            return sessionFactory.GetCurrentSession().CreateQuery("from Location").List<Location>();
        }
    }
}