using System.Linq;
using System.Reflection;

using DomainDrivenDelivery.Domain.Model.Freight;

using NHibernate.Event;
using NHibernate.Event.Default;

namespace DomainDrivenDelivery.Infrastructure.Persistence.NHibernate
{
    public class CargoPostLoadEventListener : DefaultPostLoadEventListener
    {
        private static readonly FieldInfo ITINERARY_FIELD;
        static CargoPostLoadEventListener()
        {
            ITINERARY_FIELD = typeof(Cargo).GetField("itinerary", BindingFlags.NonPublic | BindingFlags.Instance);
            if(ITINERARY_FIELD == null)
                throw new TargetException("itinerary field not found");
        }

        public override void OnPostLoad(PostLoadEvent @event)
        {
            if(@event.Entity is Cargo)
            {
                /*
                 * Itinerary is a column-less component with a collection field,
                 * and there's no way (that I know of) to map this behaviour in metadata.
                 *
                 * Hibernate is all about reflection, so helping the mapping along with
                 * another field manipulation is OK. This avoids the need for a public method
                 * on Cargo.
                 */
                Cargo cargo = (Cargo)@event.Entity;
                if(cargo.itinerary() != null && !cargo.itinerary().legs().Any())
                {
                    ITINERARY_FIELD.SetValue(cargo, null);
                }
            }

            base.OnPostLoad(@event);
        }
    }
}