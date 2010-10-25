using DomainDrivenDelivery.Domain.Model.Travel;

using NHibernate;

using Spring.Stereotype;

namespace DomainDrivenDelivery.Infrastructure.Persistence.NHibernate
{
    /// <summary>
    /// Hibernate implementation of CarrierMovementRepository.
    /// </summary>
    [Repository]
    public class VoyageRepositoryNHibernate : VoyageRepository
    {
        private readonly ISessionFactory sessionFactory;

        public VoyageRepositoryNHibernate(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public Voyage find(VoyageNumber voyageNumber)
        {
            return sessionFactory.GetCurrentSession().
              CreateQuery("from Voyage where voyageNumber = :vn").
              SetParameter("vn", voyageNumber).
              UniqueResult<Voyage>();
        }
    }
}