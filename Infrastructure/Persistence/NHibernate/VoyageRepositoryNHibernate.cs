using DomainDrivenDelivery.Domain.Model.Travel;

using NHibernate;

using Spring.Stereotype;
using Spring.Transaction.Interceptor;

namespace DomainDrivenDelivery.Infrastructure.Persistence.NHibernate
{
    /// <summary>
    /// Hibernate implementation of CarrierMovementRepository.
    /// </summary>
    [Repository]
    public sealed class VoyageRepositoryNHibernate : VoyageRepository
    {
        private readonly ISessionFactory sessionFactory;

        public VoyageRepositoryNHibernate(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        [Transaction(ReadOnly = true)]
        public Voyage find(VoyageNumber voyageNumber)
        {
            return sessionFactory.GetCurrentSession().
              CreateQuery("from Voyage where VoyageNumber = :vn").
              SetParameter("vn", voyageNumber).
              UniqueResult<Voyage>();
        }
    }
}