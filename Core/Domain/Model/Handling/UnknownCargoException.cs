using System;

using DomainDrivenDelivery.Domain.Model.Frieght;

namespace DomainDrivenDelivery.Domain.Model.Handling
{
    /// <summary>
    /// Thrown when trying to register an event with an unknown tracking id.
    /// </summary>
    [Serializable]
    public class UnknownCargoException : CannotCreateHandlingEventException
    {
        public UnknownCargoException(TrackingId trackingId)
            : base("No cargo with tracking id " + trackingId.stringValue() + " exists in the system") { }
    }
}