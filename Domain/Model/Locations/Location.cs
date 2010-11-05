using System;

using DomainDrivenDelivery.Domain.Patterns.Entity;
using DomainDrivenDelivery.Utilities;

namespace DomainDrivenDelivery.Domain.Model.Locations
{
    /// <summary>
    /// A location is our model is stops on a journey, such as cargo
    /// origin or destination, or carrier movement endpoints.
    /// </summary>
    /// <remarks>
    /// It is uniquely identified by a UN Locode.
    /// </remarks>
    public class Location : EntitySupport<Location, UnLocode>
    {
        /// <summary>
        /// UN Locode for this location.
        /// </summary>
        public virtual UnLocode UnLocode { get; private set; }

        /// <summary>
        /// Actual name of this location, e.g. "Stockholm".
        /// </summary>
        public virtual string Name { get; private set; }

        /// <summary>
        /// Customs zone of this location.
        /// </summary>
        public virtual CustomsZone CustomsZone { get; private set; }

        /// <summary>
        /// Time zone of this location.
        /// </summary>
        public virtual TimeZoneInfo TimeZone { get; private set; }

        /// <summary>
        /// Special Location object that marks an unknown location.
        /// </summary>
        public static readonly Location None = new Location(
            new UnLocode("XXXXX"), "-", 
            TimeZoneInfo.FindSystemTimeZoneById("UTC"), 
            CustomsZone.None);

        internal Location(UnLocode unLocode, String name, TimeZoneInfo timeZone, CustomsZone customsZone)
        {
            Validate.notNull(unLocode);
            Validate.notNull(name);
            Validate.notNull(timeZone);
            Validate.notNull(customsZone);

            UnLocode = unLocode;
            Name = name;
            TimeZone = timeZone;
            CustomsZone = customsZone;
        }

        public override UnLocode Identity
        {
            get { return UnLocode; }
        }

        public override string ToString()
        {
            return Name + " [" + UnLocode + "]";
        }

        protected internal Location()
        {
        }
    }
}