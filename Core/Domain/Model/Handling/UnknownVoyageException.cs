using System;

using DomainDrivenDelivery.Domain.Model.Travel;

namespace DomainDrivenDelivery.Domain.Model.Handling
{
    /// <summary>
    /// Thrown when trying to register an event with an unknown carrier movement id.
    /// </summary>
    [Serializable]
    public class UnknownVoyageException : CannotCreateHandlingEventException
    {
        public UnknownVoyageException(VoyageNumber voyageNumber)
            : base("No voyage with number " + voyageNumber.stringValue() + " exists in the system") { }
    }
}