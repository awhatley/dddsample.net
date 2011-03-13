using System;

using DomainDrivenDelivery.Domain.Model.Freight;

using NHibernate;

using Spring.Stereotype;
using Spring.Transaction.Interceptor;

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

        [Transaction]
        public TrackingId nextTrackingId()
        {
            string cargoSql = @"insert Cargo " + 
                @"(tracking_id, spec_origin_id, spec_destination_id, spec_arrival_deadline, last_update)" + 
                @" values (NULL, NULL, NULL, '{0}', '{1}');" +
                @" SELECT CAST(SCOPE_IDENTITY() AS bigint);" +
                @" DELETE FROM Cargo WHERE id = SCOPE_IDENTITY();";

            var seq = sessionFactory.GetCurrentSession().
              CreateSQLQuery(String.Format(cargoSql, DateTime.Now, DateTime.Now)).
              UniqueResult<long>();

            return new TrackingId(seq);
        }
    }
}