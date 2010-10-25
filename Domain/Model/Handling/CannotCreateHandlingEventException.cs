using System;
using System.Runtime.Serialization;

namespace DomainDrivenDelivery.Domain.Model.Handling
{
    /// <summary>
    /// If a <see cref="HandlingEvent"/> can't be created from a given set of parameters.
    /// </summary>
    [Serializable]
    public class CannotCreateHandlingEventException : Exception
    {
        public CannotCreateHandlingEventException() { }
        public CannotCreateHandlingEventException(string message) : base(message) { }
        public CannotCreateHandlingEventException(string message, Exception inner) : base(message, inner) { }
        protected CannotCreateHandlingEventException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}