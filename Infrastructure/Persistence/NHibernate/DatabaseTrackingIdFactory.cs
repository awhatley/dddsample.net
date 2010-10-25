using DomainDrivenDelivery.Domain.Model.Frieght;

using NHibernate;

using Spring.Data.NHibernate;
using Spring.Objects.Factory;
using Spring.Stereotype;

namespace DomainDrivenDelivery.Infrastructure.Persistence.NHibernate
{
    [Repository]
    public class DatabaseTrackingIdFactory : TrackingIdFactory, IInitializingObject
    {
        private readonly ISessionFactory sessionFactory;
        private static readonly string SEQUENCE_NAME = "TRACKING_ID_SEQ";

        public DatabaseTrackingIdFactory(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public void AfterPropertiesSet()
        {
            var template = new HibernateTemplate(sessionFactory);
            template.Execute(s => s.CreateSQLQuery("create sequence " + SEQUENCE_NAME + " as bigint start with 1").ExecuteUpdate());
        }

        public TrackingId nextTrackingId()
        {
            var seq = sessionFactory.GetCurrentSession().
              CreateSQLQuery("call next value for " + SEQUENCE_NAME).
              UniqueResult<long>();

            return new TrackingId(seq);
        }
    }
}