using System.Linq;
using System.Reflection;

using DomainDrivenDelivery.Domain.Model.Freight;

using NHibernate.Event;
using NHibernate.Event.Default;

namespace DomainDrivenDelivery.Infrastructure.Persistence.NHibernate
{
    public class CargoPostLoadEventListener : DefaultPostLoadEventListener
    {
        private static readonly PropertyInfo ItineraryProperty;
        static CargoPostLoadEventListener()
        {
            ItineraryProperty = typeof(Cargo).GetProperty("Itinerary", BindingFlags.Public | BindingFlags.Instance);
            if(ItineraryProperty == null)
                throw new TargetException("Itinerary property not found");
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
                if(cargo.Itinerary != null && !cargo.Itinerary.Legs.Any())
                {
                    ItineraryProperty.SetValue(cargo, null, null);
                }
            }

            base.OnPostLoad(@event);
        }
    }
}