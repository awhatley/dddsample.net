using DomainDrivenDelivery.Domain.Model.Travel;

using NHibernate;

using Spring.Stereotype;

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

        public Voyage find(VoyageNumber voyageNumber)
        {
            return sessionFactory.GetCurrentSession().
              CreateQuery("from Voyage where _voyageNumber = :vn").
              SetParameter("vn", voyageNumber).
              UniqueResult<Voyage>();
        }
    }
}