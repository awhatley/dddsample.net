using System;

using DomainDrivenDelivery.Domain.Model.Locations;

namespace DomainDrivenDelivery.Domain.Model.Handling
{
    /// <summary>
    /// Thrown when trying to register an event with an unknown UN locode.
    /// </summary>
    [Serializable]
    public class UnknownLocationException : CannotCreateHandlingEventException
    {
        public UnknownLocationException(UnLocode unlocode)
            : base("No location with UN locode " + unlocode.stringValue() + " exists in the system") { }
    }
}