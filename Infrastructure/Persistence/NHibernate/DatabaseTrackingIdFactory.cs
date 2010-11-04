using DomainDrivenDelivery.Domain.Model.Freight;

using NHibernate;

using Spring.Data.NHibernate;
using Spring.Objects.Factory;
using Spring.Stereotype;

namespace DomainDrivenDelivery.Infrastructure.Persistence.NHibernate
{
    [Repository]
    public class DatabaseTrackingIdFactory : TrackingIdFactory
    {
        private readonly ISessionFactory sessionFactory;

        public DatabaseTrackingIdFactory(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public TrackingId nextTrackingId()
        {
            var seq = sessionFactory.GetCurrentSession().
              CreateSQLQuery("select ROW_NUMBER() OVER (ORDER BY tracking_id) FROM Cargo").
              UniqueResult<long>();

            return new TrackingId(seq);
        }
    }
}