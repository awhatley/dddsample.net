using DomainDrivenDelivery.Application.Utilities;
using DomainDrivenDelivery.Domain.Model.Handling;

using NHibernate;

using Spring.Data.Generic;
using Spring.Testing.NUnit;
using Spring.Transaction.Support;

namespace Infrastructure.Tests.Persistence.NHibernate
{
    public abstract class AbstractRepositoryTest : AbstractTransactionalDbProviderSpringContextTests
    {
        private AdoTemplate _genericTemplate;

        public HandlingEventFactory HandlingEventFactory { get; set; }
        public HandlingEventRepository HandlingEventRepository { get; set; }
        public ISessionFactory SessionFactory { get; set; }
        public AdoTemplate GenericTemplate { get { return _genericTemplate; } }

        protected void flush()
        {
            SessionFactory.GetCurrentSession().Flush();
        }

        protected override string[] ConfigLocations
        {
            get
            {
                return new[] { 
                    "contexts/context-infrastructure-persistence.xml",
                    "contexts/context-domain.xml"
                };
            }
        }

        protected override void OnSetUpInTransaction()
        {
            // TODO store Sample* and object instances here instead of handwritten SQL
            SampleDataGenerator.loadSampleData(adoTemplate, new TransactionTemplate(transactionManager));            
            _genericTemplate = new AdoTemplate(adoTemplate);
        }

        protected ISession getSession()
        {
            return SessionFactory.GetCurrentSession();
        }
    }
}